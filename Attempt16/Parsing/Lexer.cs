﻿using System;
using System.Collections.Generic;

namespace Attempt16.Parsing {
    public class Lexer {
        private readonly string text;

        private int pos = 0;

        private char current => this.text[this.pos];

        private TokenLocation location => new TokenLocation(pos, pos);

        public Lexer(string text) {
            this.text = text;
        }

        private IToken GetLessThanOrArrow() {

            if (pos + 1 < this.text.Length && text[pos + 1] == '-') {
                pos++;
                return new Token(TokenKind.LeftArrow, new TokenLocation(pos - 1, pos));
            }
            else {
                throw new Exception();
            }
        }

        private IToken GetEqualsOrYields() {
            if (pos + 1 < text.Length && text[pos+1] == '>') {
                pos++;
                return new Token(TokenKind.YieldSign, new TokenLocation(pos - 1, pos));
            }
            else {
                return new Token(TokenKind.EqualSign, location);
            }
        }
    
        private IToken GetNumber() {
            int start = pos;
            string strNum = "";

            while (pos < this.text.Length && char.IsDigit(current)) {
                strNum += this.text[pos];
                pos++;
            }

            pos--;

            if (long.TryParse(strNum, out long num)) {
                return new Token<long>(num, TokenKind.IntLiteral, new TokenLocation(start, pos));
            }
            else {
                throw new Exception();
            }
        }

        private IToken GetIdentifier() {
            int start = pos;
            string id = "";

            while (pos < this.text.Length && char.IsLetterOrDigit(current)) {
                id += this.text[pos];
                pos++;
            }

            pos--;

            var location = new TokenLocation(start, pos);

            if (id == "var") {
                return new Token(TokenKind.VarKeyword, location);
            }
            else if (id == "int") {
                return new Token(TokenKind.IntKeyword, location);
            }
            else if (id == "void") {
                return new Token(TokenKind.VoidKeyword, location);
            }
            else if (id == "alloc") {
                return new Token(TokenKind.AllocKeyword, location);
            }
            else if (id == "free") {
                return new Token(TokenKind.FreeKeyword, location);
            }
            else if (id == "valueof") {
                return new Token(TokenKind.CopyKeyword, location);
            }
            else if (id == "if") {
                return new Token(TokenKind.IfKeyword, location);
            }
            else if (id == "then") {
                return new Token(TokenKind.ThenKeyword, location);
            }
            else if (id == "else") {
                return new Token(TokenKind.ElseKeyword, location);
            }
            else if (id == "function") {
                return new Token(TokenKind.FunctionKeyword, location);
            }
            else if (id == "while") {
                return new Token(TokenKind.WhileKeyword, location);
            }
            else if (id == "do") {
                return new Token(TokenKind.DoKeyword, location);
            }
            else if (id == "new") {
                return new Token(TokenKind.NewKeyword, location);
            }
            else if (id == "struct") {
                return new Token(TokenKind.StructKeyword, location);
            }
            else if (id == "ref") {
                return new Token(TokenKind.RefKeyword, location);
            }
            else {
                return new Token<string>(id, TokenKind.Identifier, location);
            }
        }

        private IToken GetComment() {
            int start = pos;

            while (pos < text.Length && text[pos] != '\n') {
                pos++;
            }

            pos--;

            var location = new TokenLocation(start, pos);
            return new Token(TokenKind.Whitespace, location);
        }

        private IToken GetToken() {
            if (pos >= text.Length) {
                return null;
            }

            if (current == '(') {
                return new Token(TokenKind.OpenParenthesis, location);
            }
            else if (current == ')') {
                return new Token(TokenKind.CloseParenthesis, location);
            }
            else if (current == '{') {
                return new Token(TokenKind.OpenBrace, location);
            }
            else if (current == '}') {
                return new Token(TokenKind.CloseBrace, location);
            }
            else if (current == '@') {
                return new Token(TokenKind.LiteralSign, location);
            }
            else if (current == ',') {
                return new Token(TokenKind.Comma, location);
            }
            else if (current == '.') {
                return new Token(TokenKind.Dot, location);
            }
            else if (current == ':') {
                return new Token(TokenKind.Colon, location);
            }
            else if (current == '=') {
                return this.GetEqualsOrYields();
            }
            else if (current == '<') {
                return this.GetLessThanOrArrow();
            }
            else if (current == '*') {
                return new Token(TokenKind.MultiplySign, location);
            }
            else if (current == '+') {
                return new Token(TokenKind.AddSign, location);
            }
            else if (current == '-') {
                return new Token(TokenKind.SubtractSign, location);
            }
            else if (char.IsDigit(current)) {
                return this.GetNumber();
            }
            else if (char.IsLetter(current)) {
                return this.GetIdentifier();
            }
            else if (char.IsWhiteSpace(current)) {
                return new Token(TokenKind.Whitespace, location);
            }
            else if (current == '#') {
                return this.GetComment();
            }
            else {
                throw new Exception();
            }
        }

        public IReadOnlyList<IToken> GetTokens() {
            var list = new List<IToken>();

            while (pos < this.text.Length) {
                var tok = this.GetToken();

                if (tok.Kind != TokenKind.Whitespace) {
                    list.Add(tok);
                }
                pos++;
            }

            return list;
        }
    }
}