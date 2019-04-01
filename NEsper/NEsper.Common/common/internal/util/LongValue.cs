///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.util
{
    public class LongValue
    {
        /// <summary>
        ///     Parse the string containing a long value.
        /// </summary>
        /// <param name="value">is the textual long value</param>
        /// <returns>long value</returns>
        public static long ParseString(string value)
        {
            if (value.EndsWith("L") || value.EndsWith("l")) {
                value = value.Substring(0, value.Length - 1);
            }

            if (value.StartsWith("+")) {
                value = value.Substring(1);
            }

            return long.Parse(value);
        }
    }
} // end of namespace