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
using com.espertech.esper.compat;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIODateTimeSerde : DataInputOutputSerde<DateTime?>
	{
		public readonly static DIODateTimeSerde INSTANCE = new DIODateTimeSerde();

		private DIODateTimeSerde()
		{
		}

		public void Write(
			DateTime? @object,
			DataOutput output)
		{
			WriteInternal(@object, output);
		}

		public DateTime? Read(DataInput input)
		{
			return ReadInternal(input);
		}

		public void Write(
			DateTime? @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(@object, output);
		}

		public DateTime? Read(
			DataInput input,
			byte[] unitKey)
		{
			return ReadInternal(input);
		}

		internal static void WriteInternal(
			DateTime? @object,
			DataOutput output)
		{
			if (@object == null) {
				output.WriteLong(-1);
				return;
			}

			output.WriteLong(DateTimeHelper.UtcNanos(@object.Value));
		}

		internal static DateTime? ReadInternal(DataInput input)
		{
			long value = input.ReadLong();
			if (value == -1) {
				return null;
			}

			return DateTimeHelper.TimeFromNanos(value);
		}
	}
} // end of namespace
