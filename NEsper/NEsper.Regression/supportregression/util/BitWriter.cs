///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

namespace com.espertech.esper.supportregression.util
{
    public class BitWriter
    {
        public static string Write(int value)
        {
            var current = 1 << 32;
            var builder = new StringBuilder();
            for (var ii = 0; ii < 32; ii++)
            {
                builder.Append((value & current) == 0 ? '0' : '1');
                current >>= 1;
            }

            return builder.ToString();
        }
    }
}
