///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.RegularExpressions;

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
    public class TokenInfo
    {
        internal readonly Regex regex;
        internal readonly TokenType token;

        public TokenInfo(
            Regex regex,
            TokenType token)
        {
            this.regex = regex;
            this.token = token;
        }

        public override string ToString()
        {
            return "TokenInfo{" +
                   "regex=" +
                   regex +
                   ", token=" +
                   token +
                   '}';
        }
    }
} // end of namespace