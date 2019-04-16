///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text.RegularExpressions;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
    public class Tokenizer
    {
        private readonly LinkedList<TokenInfo> tokenInfos = new LinkedList<TokenInfo>();

        public void Add(
            string pattern,
            TokenType token)
        {
            tokenInfos.AddLast(new TokenInfo(new Regex("^(" + pattern + ")"), token));
        }

        public ArrayDeque<Token> Tokenize(string str)
        {
            var tokens = new ArrayDeque<Token>(4);
            while (!str.Equals("")) {
                var match = false;
                foreach (var info in tokenInfos) {
                    var m = info.regex.Match(str);
                    if (m != Match.Empty) {
                        match = true;

                        var tok = m.Value.Trim();
                        tokens.Add(new Token(info.token, tok));

                        str = m.ReplaceFirst("").Trim();
                        break;
                    }
                }

                if (!match) {
                    throw new PropertyParseNodepException("Unexpected token '" + str + "'");
                }
            }

            return tokens;
        }
    }
} // end of namespace