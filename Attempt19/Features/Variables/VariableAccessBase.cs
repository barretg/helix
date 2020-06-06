﻿using System.Collections.Immutable;
using Attempt19.Parsing;
using Attempt19.TypeChecking;
using Attempt19.Types;

namespace Attempt19.Features.Variables {
    public abstract class VariableAccessBase : IParsedData, ITypeCheckedData {
        public string VariableName { get; set; }

        public IdentifierPath ContainingScope { get; set; }

        public IdentifierPath VariablePath { get; set; }

        public TokenLocation Location { get; set; }

        public LanguageType ReturnType { get; set; }

        public ImmutableHashSet<IdentifierPath> Lifetimes { get; set; }
    }    
}