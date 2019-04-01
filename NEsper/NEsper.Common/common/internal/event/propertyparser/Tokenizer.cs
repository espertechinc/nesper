///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
	public class Tokenizer {

	    private readonly LinkedList<TokenInfo> tokenInfos = new LinkedList<>();

	    public void Add(string pattern, TokenType token) {
	        tokenInfos.Add(new TokenInfo(Pattern.Compile("^(" + pattern + ")"), token));
	    }

	    public ArrayDeque<Token> Tokenize(string str) {
	        ArrayDeque<Token> tokens = new ArrayDeque<>(4);
	        while (!str.Equals("")) {
	            bool match = false;
	            foreach (TokenInfo info in tokenInfos) {
	                Matcher m = info.regex.Matcher(str);
	                if (m.Find()) {
	                    match = true;

	                    string tok = m.Group().Trim();
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