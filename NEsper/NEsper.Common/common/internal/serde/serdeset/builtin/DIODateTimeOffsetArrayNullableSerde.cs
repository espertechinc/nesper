///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIODateTimeOffsetArrayNullableSerde : DataInputOutputSerdeBase<DateTimeOffset?[]>
	{
		public static readonly DIODateTimeOffsetArrayNullableSerde INSTANCE = new DIODateTimeOffsetArrayNullableSerde();

		private DIODateTimeOffsetArrayNullableSerde()
		{
		}

		public void Write(
			DateTimeOffset?[] @object,
			DataOutput output)
		{
			WriteInternal(@object, output);
		}

		public DateTimeOffset?[] Read(DataInput input)
		{
			return ReadInternal(input);
		}

		public override void Write(
			DateTimeOffset?[] @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(@object, output);
		}

		public override DateTimeOffset?[] Read(
			DataInput input,
			byte[] unitKey)
		{
			return ReadInternal(input);
		}

		private void WriteInternal(
			DateTimeOffset?[] @object,
			DataOutput output)
		{
			if (@object == null) {
				output.WriteInt(-1);
				return;
			}

			output.WriteInt(@object.Length);
			foreach (var value in @object) {
				DIODateTimeOffsetSerde.INSTANCE.Write(value.Value, output);
			}
		}

		private DateTimeOffset?[] ReadInternal(DataInput input)
		{
			int len = input.ReadInt();
			if (len == -1) {
				return null;
			}

			var array = new DateTimeOffset?[len];
			for (int i = 0; i < len; i++) {
				array[i] = DIODateTimeOffsetSerde.INSTANCE.Read(input);
			}

			return array;
		}
	}
} // end of namespace
