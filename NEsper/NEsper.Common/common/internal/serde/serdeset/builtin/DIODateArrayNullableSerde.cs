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
	public class DIODateArrayNullableSerde : DataInputOutputSerde<Date[]> {
	    public readonly static DIODateArrayNullableSerde INSTANCE = new DIODateArrayNullableSerde();

	    private DIODateArrayNullableSerde() {
	    }

	    public void Write(Date[] @object, DataOutput output) {
	        WriteInternal(@object, output);
	    }

	    public Date[] Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public void Write(Date[] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteInternal(@object, output);
	    }

	    public Date[] Read(DataInput input, byte[] unitKey) {
	        return ReadInternal(input);
	    }

	    private void WriteInternal(Date[] @object, DataOutput output) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (Date i in @object) {
	            DIODateTimeSerde.INSTANCE.Write(i, output);
	        }
	    }

	    private Date[] ReadInternal(DataInput input) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        Date[] array = new Date[len];
	        for (int i = 0; i < len; i++) {
	            array[i] = DIODateTimeSerde.INSTANCE.Read(input);
	        }
	        return array;
	    }
	}
} // end of namespace
