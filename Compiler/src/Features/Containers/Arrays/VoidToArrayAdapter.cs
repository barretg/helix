﻿using System.Collections.Immutable;
using Trophy.Analysis;
using Trophy.Analysis.Types;
using Trophy.CodeGeneration.CSyntax;
using Trophy.Features.FlowControl;

namespace Trophy.Features.Containers.Arrays {
    public class VoidToArrayAdapterC : ISyntaxC {
        public readonly ISyntaxC target;

        public TrophyType ReturnType { get; }

        public ImmutableHashSet<IdentifierPath> Lifetimes => ImmutableHashSet.Create<IdentifierPath>();

        public VoidToArrayAdapterC(ISyntaxC target, TrophyType returnType) {
            this.target = target;
            this.ReturnType = returnType;
        }

        public CExpression GenerateCode(ICWriter declWriter, ICStatementWriter statWriter) {
            var syntax = new BlockSyntaxC(new[] {
                this.target,
                new ArrayLiteralSyntaxC(IdentifierPath.StackPath, new ISyntaxC[0], this.ReturnType)
            });

            return syntax.GenerateCode(declWriter, statWriter);
        }
    }
}
