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
	public class DIOPrimitiveLongArrayNullableSerde : DataInputOutputSerdeBase<long[]> {
	    public static readonly DIOPrimitiveLongArrayNullableSerde INSTANCE = new DIOPrimitiveLongArrayNullableSerde();

	    private DIOPrimitiveLongArrayNullableSerde() {
	    }

	    public void Write(long[] @object, DataOutput output) {
	        WriteInternal(@object, output);
	    }

	    public long[] Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public override void Write(long[] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteInternal(@object, output);
	    }

	    public override long[] ReadValue(DataInput input, byte[] unitKey) {
	        return ReadInternal(input);
	    }

	    private void WriteInternal(long[] @object, DataOutput output) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (long i in @object) {
	            output.WriteLong(i);
	        }
	    }

	    private long[] ReadInternal(DataInput input) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        long[] array = new long[len];
	        for (int i = 0; i < len; i++) {
	            array[i] = input.ReadLong();
	        }
	        return array;
	    }
	}
} // end of namespace
