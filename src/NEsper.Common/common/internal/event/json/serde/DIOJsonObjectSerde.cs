///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.@event.json.serde.DIOJsonSerdeHelper;

namespace com.espertech.esper.common.@internal.@event.json.serde
{
	public class DIOJsonObjectSerde : DataInputOutputSerde<IDictionary<string, object>>
	{
		private static readonly byte NULL_TYPE = 0;
		private static readonly byte INT_TYPE = 1;
		private static readonly byte DOUBLE_TYPE = 2;
		private static readonly byte STRING_TYPE = 3;
		private static readonly byte BOOLEAN_TYPE = 4;
		private static readonly byte OBJECT_TYPE = 5;
		private static readonly byte ARRAY_TYPE = 6;

		public static readonly DIOJsonObjectSerde INSTANCE = new DIOJsonObjectSerde();

		private DIOJsonObjectSerde()
		{
		}

		
		public void Write(
			object @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			Write((IDictionary<string, object>) @object, output, unitKey, writer);
		}

		public void Write(
			IDictionary<string, object> @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			if (@object == null) {
				output.WriteBoolean(false);
				return;
			}

			output.WriteBoolean(true);
			Write(@object, output);
		}

		public object Read(
			DataInput input,
			byte[] unitKey)
		{
			return ReadValue(input, unitKey);
		}

		public IDictionary<string, object> ReadValue(
			DataInput input,
			byte[] unitKey)
		{
			var nonNull = input.ReadBoolean();
			return nonNull ? Read(input) : null;
		}

		public void Write(
			IDictionary<string, object> @object,
			DataOutput output)
		{
			output.WriteInt(@object.Count);
			foreach (var entry in @object) {
				output.WriteUTF(entry.Key);
				WriteValue(entry.Value, output);
			}
		}

		public IDictionary<string, object> Read(DataInput input)
		{
			var size = input.ReadInt();
			var map = new LinkedHashMap<string, object>();
			for (var i = 0; i < size; i++) {
				var key = input.ReadUTF();
				var value = DIOJsonSerdeHelper.ReadValue(input);
				map.Put(key, value);
			}

			return map;
		}
	}
} // end of namespace
