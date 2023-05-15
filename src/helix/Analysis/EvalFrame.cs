﻿using helix.Syntax;
using Helix.Analysis.Lifetimes;
using Helix.Analysis.Types;
using Helix.Features;
using Helix.Features.Aggregates;
using Helix.Features.Functions;
using Helix.Features.Variables;
using Helix.Generation;
using System.Runtime.CompilerServices;

namespace Helix.Analysis {
    public delegate void DeclarationCG(ICWriter writer);

    public class EvalFrame : ITypedFrame {
        private int tempCounter = 0;

        // Frame-specific things
        public IDictionary<IdentifierPath, ISyntaxTree> SyntaxValues { get; }

        public IDictionary<IdentifierPath, Lifetime> LifetimeRoots { get; }

        // Global things
        public IDictionary<IdentifierPath, VariableSignature> Variables { get; }

        public IDictionary<IdentifierPath, FunctionSignature> Functions { get; }

        public IDictionary<IdentifierPath, StructSignature> Structs { get; }

        public IDictionary<HelixType, DeclarationCG> TypeDeclarations { get; }

        public IDictionary<ISyntaxTree, HelixType> ReturnTypes { get; }

        public EvalFrame() {
            this.Variables = new Dictionary<IdentifierPath, VariableSignature>();
            this.LifetimeRoots = new Dictionary<IdentifierPath, Lifetime>();

            this.SyntaxValues = new Dictionary<IdentifierPath, ISyntaxTree>() {
                { new IdentifierPath("void"), new TypeSyntax(default, PrimitiveType.Void) },
                { new IdentifierPath("int"), new TypeSyntax(default, PrimitiveType.Int) },
                { new IdentifierPath("bool"), new TypeSyntax(default, PrimitiveType.Bool) }
            };

            this.Functions = new Dictionary<IdentifierPath, FunctionSignature>();
            this.Structs = new Dictionary<IdentifierPath, StructSignature>();

            this.TypeDeclarations = new Dictionary<HelixType, DeclarationCG>();
            this.ReturnTypes = new Dictionary<ISyntaxTree, HelixType>();
        }

        public EvalFrame(EvalFrame prev) {
            this.Variables = prev.Variables; //new StackedDictionary<IdentifierPath, VariableSignature>(prev.Variables);
            this.SyntaxValues = new StackedDictionary<IdentifierPath, ISyntaxTree>(prev.SyntaxValues);
            this.LifetimeRoots = new StackedDictionary<IdentifierPath, Lifetime>(prev.LifetimeRoots);

            this.Functions = prev.Functions;
            this.Structs = prev.Structs;

            this.TypeDeclarations = prev.TypeDeclarations;
            this.ReturnTypes = prev.ReturnTypes;
        }

        public string GetVariableName() {
            return "$t_" + this.tempCounter++;
        }

        public bool TryResolvePath(IdentifierPath scope, string name, out IdentifierPath path) {
            while (true) {
                path = scope.Append(name);
                if (this.SyntaxValues.ContainsKey(path)) {
                    return true;
                }

                if (scope.Segments.Any()) {
                    scope = scope.Pop();
                }
                else {
                    return false;
                }
            }
        }

        public IdentifierPath ResolvePath(IdentifierPath scope, string path) {
            if (this.TryResolvePath(scope, path, out var value)) {
                return value;
            }

            throw new InvalidOperationException(
                $"Compiler error: The path '{path}' does not contain a value.");
        }

        public bool TryResolveName(IdentifierPath scope, string name, out ISyntaxTree value) {
            if (!this.TryResolvePath(scope, name, out var path)) {
                value = null;
                return false;
            }

            return this.SyntaxValues.TryGetValue(path, out value);
        }

        public ISyntaxTree ResolveName(IdentifierPath scope, string name) {
            return this.SyntaxValues[this.ResolvePath(scope, name)];
        }
    }
}