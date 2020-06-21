///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIOPrimitiveIntArrayNullableSerde : DataInputOutputSerdeBase<int[]> {
	    public static readonly DIOPrimitiveIntArrayNullableSerde INSTANCE = new DIOPrimitiveIntArrayNullableSerde();

	    private DIOPrimitiveIntArrayNullableSerde() {
	    }

	    public void Write(int[] @object, DataOutput output) {
	        WriteInternal(@object, output);
	    }

	    public int[] Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public override void Write(int[] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteInternal(@object, output);
	    }

	    public override int[] ReadValue(DataInput input, byte[] unitKey) {
	        return ReadInternal(input);
	    }

	    private void WriteInternal(int[] @object, DataOutput output) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (int i in @object) {
	            output.WriteInt(i);
	        }
	    }

	    private int[] ReadInternal(DataInput input) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        int[] array = new int[len];
	        for (int i = 0; i < len; i++) {
	            array[i] = input.ReadInt();
	        }
	        return array;
	    }
	}
} // end of namespace
