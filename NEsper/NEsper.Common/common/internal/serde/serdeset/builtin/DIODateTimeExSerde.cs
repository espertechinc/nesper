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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIODateTimeExSerde : DataInputOutputSerdeBase<DateTimeEx> {
	    public static readonly DIODateTimeExSerde INSTANCE = new DIODateTimeExSerde();

	    private DIODateTimeExSerde() {
	    }

	    public void Write(DateTimeEx @object, DataOutput output) {
	        WriteValue(@object, output);
	    }

	    public DateTimeEx Read(DataInput input) {
	        return ReadValue(input);
	    }

	    public override void Write(DateTimeEx @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteValue(@object, output);
	    }

	    public override DateTimeEx ReadValue(DataInput input, byte[] unitKey) {
	        return ReadValue(input);
	    }

	    public static void WriteValue(DateTimeEx value, DataOutput output) {
	        if (value == null) {
	            output.WriteBoolean(true);
	            return;
	        }
	        output.WriteBoolean(false);
	        output.WriteUTF(value.TimeZone.Id);
	        output.WriteLong(value.UtcMillis);
	    }

	    public static DateTimeEx ReadValue(DataInput input) {
	        bool isNull = input.ReadBoolean();
	        if (isNull) {
	            return null;
	        }
	        var timeZoneId = input.ReadUTF();
	        var timeZone = TimeZoneHelper.GetTimeZoneInfo(timeZoneId);
	        var millis = input.ReadLong();
	        var value = DateTimeEx.GetInstance(timeZone, millis);
	        
	        return value;
	    }
	}
} // end of namespace
