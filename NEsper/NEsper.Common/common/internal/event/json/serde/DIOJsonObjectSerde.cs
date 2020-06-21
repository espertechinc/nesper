///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.@event.json.serde.DIOJsonSerdeHelper; // readValue
using static com.espertech.esper.common.@internal.@event.json.serde.DIOJsonSerdeHelper; // writeValue

namespace com.espertech.esper.common.@internal.@event.json.serde
{
	public class DIOJsonObjectSerde : DataInputOutputSerde<IDictionary<string, object>>
	{
		private readonly static byte NULL_TYPE = 0;
		private readonly static byte INT_TYPE = 1;
		private readonly static byte DOUBLE_TYPE = 2;
		private readonly static byte STRING_TYPE = 3;
		private readonly static byte BOOLEAN_TYPE = 4;
		private readonly static byte OBJECT_TYPE = 5;
		private readonly static byte ARRAY_TYPE = 6;

		public readonly static DIOJsonObjectSerde INSTANCE = new DIOJsonObjectSerde();

		private DIOJsonObjectSerde()
		{
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

		public IDictionary<string, object> Read(
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
				var value = ReadValue(input);
				map.Put(key, value);
			}

			return map;
		}
	}
} // end of namespace
