﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Attempt20.CodeGeneration.CSyntax {
    public class CParameter {
        public string Name { get; }

        public CType Type { get; }

        public CParameter(CType type, string name) {
            this.Type = type;
            this.Name = name;
        }
    }

    public abstract class CDeclaration {
        public static CDeclaration Function(CType returnType, string name, IReadOnlyList<CParameter> pars, IReadOnlyList<CStatement> stats) {
            return new CFunctionDeclaration(returnType, name, pars, Option.Some(stats));
        }

        public static CDeclaration Function(string name, IReadOnlyList<CParameter> pars, IReadOnlyList<CStatement> stats) {
            return new CFunctionDeclaration(name, pars, Option.Some(stats));
        }

        public static CDeclaration FunctionPrototype(CType returnType, string name, IReadOnlyList<CParameter> pars) {
            return new CFunctionDeclaration(returnType, name, pars, Option.None<IReadOnlyList<CStatement>>());
        }

        public static CDeclaration FunctionPrototype(string name, IReadOnlyList<CParameter> pars) {
            return new CFunctionDeclaration(name, pars, Option.None<IReadOnlyList<CStatement>>());
        }

        public static CDeclaration Struct(string name, IReadOnlyList<CParameter> members) {
            return new CStructDeclaration(name, Option.Some(members));
        }

        public static CDeclaration StructPrototype(string name) {
            return new CStructDeclaration(name, Option.None<IReadOnlyList<CParameter>>());
        }

        public static CDeclaration EmptyLine() {
            return new CEmptyLine();
        }

        private CDeclaration() { }

        public abstract void WriteToC(int indentLevel, StringBuilder sb);

        private class CEmptyLine : CDeclaration {
            public override void WriteToC(int indentLevel, StringBuilder sb) {
                sb.AppendLine();
            }
        }

        private class CStructDeclaration : CDeclaration {
            private readonly string Name;
            private readonly IOption<IReadOnlyList<CParameter>> members;

            public CStructDeclaration(string name, IOption<IReadOnlyList<CParameter>> members) {
                this.Name = name;
                this.members = members;
            }

            public override void WriteToC(int indentLevel, StringBuilder sb) {
                CHelper.Indent(indentLevel, sb);

                if (this.members.TryGetValue(out var mems)) {
                    sb.Append("struct ").Append(this.Name).AppendLine(" {");

                    foreach (var mem in mems) {
                        CHelper.Indent(indentLevel + 1, sb);
                        sb.Append(mem.Type.ToString()).Append(" ").Append(mem.Name).AppendLine(";");
                    }

                    CHelper.Indent(indentLevel, sb);
                    sb.AppendLine("};");
                }
                else {
                    sb.Append("typedef struct ").Append(this.Name).Append(" ").Append(this.Name).AppendLine(";");
                }
            }
        }

        private class CFunctionDeclaration : CDeclaration {
            private readonly IOption<CType> ReturnType;
            private readonly string name;
            private readonly IReadOnlyList<CParameter> pars;
            private readonly IOption<IReadOnlyList<CStatement>> stats;

            public CFunctionDeclaration(CType returnType, string name, IReadOnlyList<CParameter> pars, IOption<IReadOnlyList<CStatement>> stats) {
                this.ReturnType = Option.Some(returnType);
                this.name = name;
                this.pars = pars;
                this.stats = stats;
            }

            public CFunctionDeclaration(string name, IReadOnlyList<CParameter> pars, IOption<IReadOnlyList<CStatement>> stats) {
                this.ReturnType = Option.None<CType>();
                this.name = name;
                this.pars = pars;
                this.stats = stats;
            }

            public override void WriteToC(int indentLevel, StringBuilder sb) {
                CHelper.Indent(indentLevel, sb);

                if (this.ReturnType.TryGetValue(out var type)) {
                    sb.Append(type);
                }
                else {
                    sb.Append("void");
                }

                sb.Append(" ").Append(this.name).Append("(");

                if (this.pars.Any()) {
                    sb.Append(this.pars[0].Type).Append(" ").Append(this.pars[0].Name);

                    foreach (var par in this.pars.Skip(1)) {
                        sb.Append(", ").Append(par.Type).Append(" ").Append(par.Name);
                    }
                }

                sb.Append(")");

                if (this.stats.TryGetValue(out var stats)) {
                    if (stats.Any()) {
                        sb.AppendLine(" {");
                    }

                    foreach (var stat in stats) {
                        stat.WriteToC(indentLevel + 1, sb);
                    }

                    if (stats.Any()) {
                        CHelper.Indent(indentLevel, sb);
                    }

                    sb.AppendLine("}");
                }
                else {
                    sb.AppendLine(";");
                }                
            }
        }
    }
}
