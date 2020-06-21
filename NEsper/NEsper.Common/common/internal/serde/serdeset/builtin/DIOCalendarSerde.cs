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
namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIOCalendarSerde : DataInputOutputSerde<Calendar> {
	    public readonly static DIOCalendarSerde INSTANCE = new DIOCalendarSerde();

	    private DIOCalendarSerde() {
	    }

	    public void Write(Calendar object, DataOutput output) {
	        WriteCalendar(object, output);
	    }

	    public Calendar Read(DataInput input) {
	        return ReadCalendar(input);
	    }

	    public void Write(Calendar object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteCalendar(object, output);
	    }

	    public Calendar Read(DataInput input, byte[] unitKey) {
	        return ReadCalendar(input);
	    }

	    public static void WriteCalendar(Calendar cal, DataOutput output) {
	        if (cal == null) {
	            output.WriteBoolean(true);
	            return;
	        }
	        output.WriteBoolean(false);
	        output.WriteUTF(cal.TimeZone.ID);
	        output.WriteLong(cal.TimeInMillis);
	    }

	    public static Calendar ReadCalendar(DataInput input) {
	        bool isNull = input.ReadBoolean();
	        if (isNull) {
	            return null;
	        }
	        string timeZoneId = input.ReadUTF();
	        long millis = input.ReadLong();
	        Calendar cal = Calendar.GetInstance(TimeZone.GetTimeZone(timeZoneId));
	        cal.TimeInMillis = millis;
	        return cal;
	    }
	}
} // end of namespace
