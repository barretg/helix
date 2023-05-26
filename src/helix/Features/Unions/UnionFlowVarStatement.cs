﻿using Helix.Analysis;
using Helix.Analysis.Flow;
using Helix.Analysis.TypeChecking;
using Helix.Analysis.Types;
using Helix.Generation.Syntax;
using Helix.Generation;
using Helix.Parsing;
using Helix.Syntax;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helix.Collections;

namespace Helix.Features.Unions {
    public class UnionFlowVarStatement : ISyntaxTree {
        public StructMember UnionMember { get; }

        public IdentifierPath ShadowedPath { get; }

        public PointerType ShadowedType { get; }

        public TokenLocation Location { get; }

        public IEnumerable<ISyntaxTree> Children => Array.Empty<ISyntaxTree>();

        public bool IsPure => true;

        public UnionFlowVarStatement(TokenLocation loc, StructMember member, 
                                    IdentifierPath shadowed, PointerType shadowedType) {
            this.Location = loc;
            this.UnionMember = member;
            this.ShadowedPath = shadowed;
            this.ShadowedType = shadowedType;
        }

        public ISyntaxTree CheckTypes(TypeFrame types) {
            var varSig = new PointerType(this.UnionMember.Type, this.UnionMember.IsWritable && this.ShadowedType.IsWritable);
            var path = this.Location.Scope.Append(this.ShadowedPath.Segments.Last());

            types.SyntaxValues = types.SyntaxValues.SetItem(path, new TypeSyntax(this.Location, varSig));

            this.SetReturnType(PrimitiveType.Void, types);
            this.SetCapturedVariables(this.ShadowedPath, VariableCaptureKind.LocationCapture, this.ShadowedType, types);
            this.SetPredicate(types);

            return this;
        }

        public ISyntaxTree ToRValue(TypeFrame types) => this;

        public void AnalyzeFlow(FlowFrame flow) {
            this.DeclareValueLifetimes(flow);
            this.SetLifetimes(new LifetimeBundle(), flow);
        }

        private void DeclareValueLifetimes(FlowFrame flow) {
            var path = this.Location.Scope
                .Append(this.ShadowedPath.Segments.Last())
                .ToVariablePath();

            //var varLocation = new Lifetime(
            //    this.Location,
            //    this.path.ToVariablePath(),
            //    allowedRoots,
            //    LifetimeOrigin.LocalLocation);

            if (this.UnionMember.Type.IsValueType(flow)) {
                flow.LocalLifetimes = flow.LocalLifetimes.SetItem(path, new LifetimeBounds());
                return;
            }

            var valueLifetime = new ValueLifetime(
                path,
                LifetimeRole.Root,
                LifetimeOrigin.TempValue);

            // Add this variable lifetimes to the current frame
            var bounds = new LifetimeBounds(valueLifetime);
            flow.LocalLifetimes = flow.LocalLifetimes.SetItem(path, bounds);

            flow.LifetimeRoots = flow.LifetimeRoots.Add(valueLifetime);
        }

        public ICSyntax GenerateCode(FlowFrame flow, ICStatementWriter writer) {
            ICSyntax assign = new CAddressOf() {
                Target = new CMemberAccess() {
                    Target = new CMemberAccess() {
                        Target = new CVariableLiteral(writer.GetVariableName(this.ShadowedPath)),
                        MemberName = "data"
                    },
                    MemberName = this.UnionMember.Name
                }
            };

            var path = this.Location.Scope.Append(this.ShadowedPath.Segments.Last());
            var name = writer.GetVariableName(path);
            var cReturnType = new CPointerType(writer.ConvertType(this.UnionMember.Type));

            var stat = new CVariableDeclaration() {
                Type = cReturnType,
                Name = name,
                Assignment = Option.Some(assign)
            };

            writer.WriteComment($"Line {this.Location.Line}: Union downcast flowtyping");
            writer.WriteStatement(stat);
            writer.WriteEmptyLine();
            writer.VariableKinds[path] = CVariableKind.Allocated;

            return new CIntLiteral(0);
        }
    }
}