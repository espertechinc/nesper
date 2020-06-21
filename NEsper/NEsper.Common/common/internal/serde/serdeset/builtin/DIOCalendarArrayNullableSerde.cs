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
	public class DIOCalendarArrayNullableSerde : DataInputOutputSerde<Calendar[]> {
	    public readonly static DIOCalendarArrayNullableSerde INSTANCE = new DIOCalendarArrayNullableSerde();

	    private DIOCalendarArrayNullableSerde() {
	    }

	    public void Write(Calendar[] @object, DataOutput output) {
	        WriteInternal(@object, output);
	    }

	    public Calendar[] Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public void Write(Calendar[] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteInternal(@object, output);
	    }

	    public Calendar[] Read(DataInput input, byte[] unitKey) {
	        return ReadInternal(input);
	    }

	    private void WriteInternal(Calendar[] @object, DataOutput output) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (Calendar i in @object) {
	            DIOCalendarSerde.INSTANCE.Write(i, output);
	        }
	    }

	    private Calendar[] ReadInternal(DataInput input) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        Calendar[] array = new Calendar[len];
	        for (int i = 0; i < len; i++) {
	            array[i] = DIOCalendarSerde.INSTANCE.Read(input);
	        }
	        return array;
	    }
	}
} // end of namespace
