///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using Antlr4.Runtime;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// For use with ANTLR to create a case-insensitive token stream.
    /// </summary>
    public class NoCaseSensitiveStream : AntlrInputStream
    {
        /// <summary>Ctor.</summary>
        /// <param name="s">string to be parsed</param>
        /// <throws>IOException to indicate IO errors</throws>
        public NoCaseSensitiveStream(String s)
            : base(new StringReader(s))
        {
        }

        public override int La(int i)
        {
            if (i == 0)
            {
                return 0; // undefined
            }
            if (i < 0)
            {
                i++; // e.g., translate LA(-1) to use offset i=0; then data[p+0-1]
                if ((p + i - 1) < 0)
                {
                    return IntStreamConstants.Eof;
                }
            }
            // invalid; no char before first char
            if ((p + i - 1) >= n)
            {
                return (int)IntStreamConstants.Eof;
            }
            return Char.ToLower(data[p + i - 1]);
        }
    }
} // End of namespace
