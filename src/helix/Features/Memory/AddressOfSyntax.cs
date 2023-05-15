﻿using helix.FlowAnalysis;
using Helix.Analysis;
using Helix.Analysis.Lifetimes;
using Helix.Analysis.Types;
using Helix.Generation.Syntax;
using Helix.Generation;
using Helix.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using helix.Syntax;

namespace helix.Features.Memory {
    public class AddressOfSyntax : ISyntaxTree {
        private readonly ISyntaxTree target;

        public TokenLocation Location { get; }

        public IEnumerable<ISyntaxTree> Children => new[] { target };

        public bool IsPure => target.IsPure;

        public AddressOfSyntax(TokenLocation loc, ISyntaxTree target) {
            Location = loc;
            this.target = target;
        }

        public ISyntaxTree CheckTypes(EvalFrame types) {
            if (this.IsTypeChecked(types)) {
                return this;
            }

            var target = this.target.CheckTypes(types).ToLValue(types);
            var ptrType = (PointerType)target.GetReturnType(types);
            var result = new AddressOfSyntax(Location, target);

            result.SetReturnType(ptrType, types);
            return result;
        }

        public ISyntaxTree ToRValue(EvalFrame types) {
            return this;
        }

        public void AnalyzeFlow(FlowFrame flow) {
            if (this.IsFlowAnalyzed(flow)) {
                return;
            }

            target.AnalyzeFlow(flow);

            var lifetime = target.GetLifetimes(flow).Components[new IdentifierPath()];
            var dict = new Dictionary<IdentifierPath, Lifetime>() { { new IdentifierPath(), lifetime } };

            this.SetLifetimes(new LifetimeBundle(dict), flow);
        }

        public ICSyntax GenerateCode(FlowFrame types, ICStatementWriter writer) {
            return new CCompoundExpression() {
                Arguments = new ICSyntax[] {
                    target.GenerateCode(types, writer),
                    writer.GetLifetime(this.GetLifetimes(types).Components[new IdentifierPath()])
                },
                Type = writer.ConvertType(this.GetReturnType(types))
            };
        }
    }
}