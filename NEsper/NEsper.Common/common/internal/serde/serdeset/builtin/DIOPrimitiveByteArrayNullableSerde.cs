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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIOPrimitiveByteArrayNullableSerde : DataInputOutputSerdeBase<byte[]>
	{
		public static readonly DIOPrimitiveByteArrayNullableSerde INSTANCE = new DIOPrimitiveByteArrayNullableSerde();

		private DIOPrimitiveByteArrayNullableSerde()
		{
		}

		public void Write(
			byte[] @object,
			DataOutput output)
		{
			WriteInternal(@object, output);
		}

		public byte[] Read(DataInput input)
		{
			return ReadInternal(input);
		}

		public override void Write(
			byte[] @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(@object, output);
		}

		public override byte[] ReadValue(
			DataInput input,
			byte[] unitKey)
		{
			return ReadInternal(input);
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
			output.Write(@object);
		}

		private byte[] ReadInternal(DataInput input)
		{
			int len = input.ReadInt();
			if (len == -1) {
				return null;
			}

			byte[] array = new byte[len];
			input.ReadFully(array);
			return array;
		}
	}
} // end of namespace
