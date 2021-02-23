﻿using Attempt20;
using Attempt20.Analysis;
using Attempt20.Analysis.Types;
using Attempt20.Features.FlowControl;
using Attempt20.Features.Primitives;
using Attempt20.Features.Variables;
using Attempt20.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Features.FlowControl {
    public class ForSyntaxA : ISyntaxA {
        private readonly string id;
        private readonly ISyntaxA startIndex, endIndex, body;

        public TokenLocation Location { get; }

        public ForSyntaxA(TokenLocation loc, string id, ISyntaxA start, ISyntaxA end, ISyntaxA body) {
            this.Location = loc;
            this.id = id;
            this.startIndex = start;
            this.endIndex = end;
            this.body = body;
        }

        public ISyntaxB CheckNames(INameRecorder names) {
            // Rewrite for syntax to use while loops
            var counterName = "$for_counter_" + names.GetNewVariableId();
            var start = new AsSyntaxA(this.startIndex.Location, this.startIndex, TrophyType.Integer);
            var end = new AsSyntaxA(this.startIndex.Location, this.endIndex, TrophyType.Integer);

            var counterDecl = new VarRefSyntaxA(this.Location, counterName, start, false);

            var idDecl = new VarRefSyntaxA(
                this.Location, 
                this.id, 
                new IdentifierAccessSyntaxA(this.Location, counterName, VariableAccessKind.ValueAccess), 
                true);

            var comp = new BinarySyntaxA(
                this.Location,
                new IdentifierAccessSyntaxA(this.Location, counterName, VariableAccessKind.ValueAccess),
                end,
                BinaryOperation.LessThanOrEqualTo);

            var store = new StoreSyntaxA(
                this.Location,
                new IdentifierAccessSyntaxA(this.Location, counterName, VariableAccessKind.LiteralAccess),
                new BinarySyntaxA(
                    this.Location,
                    new IdentifierAccessSyntaxA(this.Location, counterName, VariableAccessKind.ValueAccess),
                    new IntLiteralSyntax(this.Location, 1),
                    BinaryOperation.Add));

            var block = new BlockSyntaxA(this.Location, new ISyntaxA[] {
                counterDecl,
                new WhileSyntaxA(
                    this.Location,
                    comp,
                    new BlockSyntaxA(this.Location, new ISyntaxA[] { 
                        idDecl,
                        this.body,
                        store
                    }))
            });

            return block.CheckNames(names);
        }
    }
}