///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Antlr4.Runtime;

namespace com.espertech.esper.grammar.@internal.util
{
    public class CaseInsensitiveInputStream : AntlrInputStream
    {
        private readonly char[] la;

        public CaseInsensitiveInputStream(string input)
            : base(input)
        {
            la = input.ToLowerInvariant().ToCharArray();
        }

        public override int LA(int i)
        {
            if (i == 0)
            {
                return 0;
            }

            if (i < 0)
            {
                i++;
                if ((p + i - 1) < 0)
                {
                    return IntStreamConstants.EOF;
                }
            }

            if ((p + i - 1) >= n)
            {
                return IntStreamConstants.EOF;
            }

            return la[p + i - 1];
        }
    }
} // end of namespace