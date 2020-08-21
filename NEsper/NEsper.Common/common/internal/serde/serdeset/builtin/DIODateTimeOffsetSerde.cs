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
	public class DIODateTimeOffsetSerde : DataInputOutputSerdeBase<DateTimeOffset>
	{
		public static readonly DIODateTimeOffsetSerde INSTANCE = new DIODateTimeOffsetSerde();

		private DIODateTimeOffsetSerde()
		{
		}

		public void Write(
			DateTimeOffset @object,
			DataOutput output)
		{
			WriteInternal(@object, output);
		}

		public DateTimeOffset Read(DataInput input)
		{
			return ReadInternal(input);
		}

		public override void Write(
			DateTimeOffset @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(@object, output);
		}

		public override DateTimeOffset ReadValue(
			DataInput input,
			byte[] unitKey)
		{
			return ReadInternal(input);
		}

		internal static void WriteInternal(
			DateTimeOffset @object,
			DataOutput output)
		{
			var nanos = DateTimeOffsetHelper.UtcNanos(@object);
			var offset = @object.Offset.Ticks;
			output.WriteLong(nanos);
			output.WriteLong(offset);
		}

		internal static DateTimeOffset ReadInternal(DataInput input)
		{
			var nanos = input.ReadLong();
			var offset = TimeSpan.FromTicks(input.ReadLong());
			return DateTimeOffsetHelper.TimeFromNanos(nanos, offset);
		}
	}
} // end of namespace
