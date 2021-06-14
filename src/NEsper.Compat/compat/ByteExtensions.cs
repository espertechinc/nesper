///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using Force.Crc32;

namespace com.espertech.esper.compat
{
    public static class ByteExtensions
    {
        public static string ToHexString(this byte[] input)
        {
            var stringBuilder = new StringBuilder();

            for (var ii = 0; ii < input.Length; ii++) {
                stringBuilder.AppendFormat("{0:x2}", input[ii]);
            }

            return stringBuilder.ToString();
        }

        public static long GetCrc32(this byte[] input)
        {
            return Crc32Algorithm.Compute(input);
        }
    }
}
