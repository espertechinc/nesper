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

using java.sql;
namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIODateTimeOffsetSerde : DataInputOutputSerde<DateTimeOffset?> {
	    public readonly static DIODateTimeOffsetSerde INSTANCE = new DIODateTimeOffsetSerde();

	    private DIODateTimeOffsetSerde() {
	    }

	    public void Write(DateTimeOffset @object, DataOutput output) {
	        WriteInternal(@object, output);
	    }

	    public DateTimeOffset? Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public void Write(DateTimeOffset? @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteInternal(@object, output);
	    }

	    public DateTimeOffset? Read(DataInput input, byte[] unitKey) {
	        return ReadInternal(input);
	    }

	    internal static void WriteInternal(DateTimeOffset? @object, DataOutput output) {
	        if (@object == null) {
	            output.WriteLong(-1);
	            return;
	        }
	        output.WriteLong(object.Time);
	    }

	    internal static DateTimeOffset? ReadInternal(DataInput input) {
	        long value = input.ReadLong();
	        if (value == -1) {
	            return null;
	        }
	        return new DateTimeOffset(value);
	    }
	}
} // end of namespace
