///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
	public class DIOMultiKeyArrayObjectSerde : DataInputOutputSerdeBase<MultiKeyArrayObject>
	{
		public static readonly DIOMultiKeyArrayObjectSerde INSTANCE = new DIOMultiKeyArrayObjectSerde();

		public override void Write(
			MultiKeyArrayObject mk,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(mk.Keys, output);
		}

		public override MultiKeyArrayObject ReadValue(
			DataInput input,
			byte[] unitKey)
		{
			return new MultiKeyArrayObject(ReadInternal(input));
		}

		private void WriteInternal(
			object[] @object,
			DataOutput output)
		{
			if (@object == null) {
				output.WriteInt(-1);
				return;
			}

			output.WriteInt(@object.Length);
			
			foreach (var obj in @object) {
				byte[] data = DIOSerializableObjectSerde.ObjectToByteArr(obj); 
				output.WriteInt(data.Length);
				output.Write(data);
			}
		}

		private object[] ReadInternal(DataInput input)
		{
			var len = input.ReadInt();
			if (len == -1) {
				return null;
			}

			var array = new object[len];

			for (var i = 0; i < array.Length; i++) {
				var itemLength = input.ReadInt();
				var itemData = new byte[itemLength];
				input.ReadFully(itemData);
				array[i] = DIOSerializableObjectSerde.ByteArrToObject(itemData);
			}

			return array;
		}
	}
} // end of namespace
