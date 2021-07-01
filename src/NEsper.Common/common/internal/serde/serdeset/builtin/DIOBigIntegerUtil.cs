///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.compat;
using com.espertech.esper.compat.io;


namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIOBigIntegerUtil
	{
		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="bigInteger">value</param>
		/// <param name="stream">output</param>
		/// <throws>IOException io error</throws>
		public static void WriteBigInt(
			BigInteger bigInteger,
			DataOutput stream)
		{
			var byteArray = bigInteger.ToByteArray();
			if (byteArray.Length > Int16.MaxValue) {
				throw new ArgumentException("BigInteger byte array is larger than 0x7fff bytes");
			}

			var length = (short) byteArray.Length;
			stream.WriteShort(length);
			stream.Write(byteArray, 0, byteArray.Length);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="input">input</param>
		/// <returns>big int</returns>
		/// <throws>IOException io error</throws>
		public static BigInteger ReadBigInt(DataInput input)
		{
			var len = input.ReadShort();
			if (len < 0) {
				throw new IllegalStateException("Negative length on byte array");
			}

			var byteArray = new byte[len];
			input.ReadFully(byteArray);
			return new BigInteger(byteArray);
		}
	}
} // end of namespace
