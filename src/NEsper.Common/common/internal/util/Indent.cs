///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Utility class around indenting and formatting text.
    /// </summary>
    public class Indent
    {
        /// <summary> Utility method to indent a text for a number of characters.</summary>
        /// <param name="numChars">is the number of character to indent with spaces
        /// </param>
        /// <returns> the formatted string
        /// </returns>
        public static string CreateIndent(int numChars)
        {
            if (numChars < 0) {
                throw new ArgumentException("Number of characters less then zero");
            }

            var buf = new char[numChars];
            for (var ii = 0; ii < buf.Length; ii++) {
                buf[ii] = ' ';
            }

            return new string(buf);
        }
    }
}