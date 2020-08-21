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
	public class DIOMultiKeyArrayFloatSerde : DataInputOutputSerdeBase<MultiKeyArrayFloat>
	{
		public static readonly DIOMultiKeyArrayFloatSerde INSTANCE = new DIOMultiKeyArrayFloatSerde();

		public override void Write(
			MultiKeyArrayFloat mk,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(mk.Keys, output);
		}

		public override MultiKeyArrayFloat ReadValue(
			DataInput input,
			byte[] unitKey)
		{
			return new MultiKeyArrayFloat(ReadInternal(input));
		}

		private void WriteInternal(
			float[] @object,
			DataOutput output)
		{
			if (@object == null) {
				output.WriteInt(-1);
				return;
			}

			output.WriteInt(@object.Length);
			foreach (float i in @object) {
				output.WriteFloat(i);
			}
		}

		private float[] ReadInternal(DataInput input)
		{
			int len = input.ReadInt();
			if (len == -1) {
				return null;
			}

			float[] array = new float[len];
			for (int i = 0; i < len; i++) {
				array[i] = input.ReadFloat();
			}

			return array;
		}
	}
} // end of namespace
