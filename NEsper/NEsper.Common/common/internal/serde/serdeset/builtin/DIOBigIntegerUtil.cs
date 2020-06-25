///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Numerics;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;


namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIOBigIntegerUtil {
	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="bigInteger">value</param>
	    /// <param name="stream">output</param>
	    /// <throws>IOException io error</throws>
	    public static void WriteBigInt(BigInteger bigInteger, DataOutput stream) {
	        byte[] a = bigInteger.ToByteArray();
	        if (a.Length > Short.MAX_VALUE) {
	            throw new ArgumentException("BigInteger byte array is larger than 0x7fff bytes");
	        }
	        int firstByte = a[0];
	        stream.WriteShort((firstByte < 0) ? (-a.Length) : a.Length);
	        stream.WriteByte(firstByte);
	        stream.Write(a, 1, a.Length - 1);
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="input">input</param>
	    /// <returns>big int</returns>
	    /// <throws>IOException io error</throws>
	    public static BigInteger ReadBigInt(DataInput input) {
	        int len = input.ReadShort();
	        if (len < 0) {
	            len = -len;
	        }
	        byte[] a = new byte[len];
	        a[0] = input.ReadByte();
	        input.ReadFully(a, 1, a.Length - 1);
	        return new BigInteger(a);
	    }
	}
} // end of namespace
