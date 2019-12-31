///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NEsper.Benchmark.Common
{
    public class ByteUtil
    {
        public static byte[] ComputeMD5Hash( byte[] data, int offset, int length )
        {
            MD5 md5Hasher = MD5.Create();
            byte[] dataHash = md5Hasher.ComputeHash(data, offset, length);
            return dataHash;
        }

        /// <summary>
        /// Converts the provided bytes to a hex string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string ToHexString( byte[] data, int offset, int length )
        {
            var sb = new StringBuilder();

            int tail = offset + length;
            for( int ii = offset ; ii < tail ; ii++ ) {
                sb.AppendFormat("{0:X2}", data[ii]);
            }

            return sb.ToString();
        }

        public static string ToHexString( byte[] data )
        {
            return ToHexString(data, 0, data.Length);
        }

        /// <summary>
        /// Extracts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static byte[] Extract(byte[] data, int offset, int length)
        {
            byte[] edata = new byte[length];
            Array.Copy(data, offset, edata, 0, length);
            return edata;
        }
    }
}
