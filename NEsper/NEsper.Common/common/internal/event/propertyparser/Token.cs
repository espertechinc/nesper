///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
    public class Token
    {
        internal readonly string sequence;
        internal readonly TokenType token;

        public Token(
            TokenType token,
            string sequence)
        {
            this.token = token;
            this.sequence = sequence;
        }

        public override string ToString()
        {
            return "Token{" +
                   "token=" +
                   token +
                   ", sequence='" +
                   sequence +
                   '\'' +
                   '}';
        }
    }
} // end of namespace