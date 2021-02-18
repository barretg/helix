﻿namespace Attempt20.Parsing {
    public class ParseException : TrophyException {
        public ParseException(TokenLocation location, string title, string message) : base(location, title, message) { }
    }
}