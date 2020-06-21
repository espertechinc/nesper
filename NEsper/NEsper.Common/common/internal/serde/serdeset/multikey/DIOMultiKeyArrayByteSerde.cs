///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
	public class DIOMultiKeyArrayByteSerde : DataInputOutputSerde<MultiKeyArrayByte>
	{
		public readonly static DIOMultiKeyArrayByteSerde INSTANCE = new DIOMultiKeyArrayByteSerde();

		public void Write(
			MultiKeyArrayByte mk,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(mk.Keys, output);
		}

		public MultiKeyArrayByte Read(
			DataInput input,
			byte[] unitKey)
		{
			return new MultiKeyArrayByte(ReadInternal(input));
		}

		private void WriteInternal(
			byte[] @object,
			DataOutput output)
		{
			if (@object == null) {
				output.WriteInt(-1);
				return;
			}

			output.WriteInt(@object.Length);
			foreach (byte i in @object) {
				output.WriteByte(i);
			}
		}

		private byte[] ReadInternal(DataInput input)
		{
			int len = input.ReadInt();
			if (len == -1) {
				return null;
			}

			byte[] array = new byte[len];
			for (int i = 0; i < len; i++) {
				array[i] = input.ReadByte();
			}

			return array;
		}
	}
} // end of namespace
