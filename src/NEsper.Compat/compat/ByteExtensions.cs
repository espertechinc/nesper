///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

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
            const uint polynomial = 0xEDB88320u;
            uint crc = 0xFFFFFFFFu;

            for (var i = 0; i < input.Length; i++) {
                crc ^= input[i];

                for (var bit = 0; bit < 8; bit++) {
                    if ((crc & 1u) != 0u) {
                        crc = (crc >> 1) ^ polynomial;
                    }
                    else {
                        crc >>= 1;
                    }
                }
            }

            crc ^= 0xFFFFFFFFu;
            return crc;
        }
    }
}
