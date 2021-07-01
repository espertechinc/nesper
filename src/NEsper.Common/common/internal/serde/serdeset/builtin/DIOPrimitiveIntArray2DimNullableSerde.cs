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
	public class DIOPrimitiveIntArray2DimNullableSerde : DataInputOutputSerdeBase<int[][]> {
	    public static readonly DIOPrimitiveIntArray2DimNullableSerde INSTANCE = new DIOPrimitiveIntArray2DimNullableSerde();

	    private DIOPrimitiveIntArray2DimNullableSerde() {
	    }

	    public override void Write(int[][] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (int[] i in @object) {
	            WriteArray(i, output);
	        }
	    }

	    public override int[][] ReadValue(DataInput input, byte[] unitKey) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        int[][] array = new int[len][];
	        for (int i = 0; i < len; i++) {
	            array[i] = ReadArray(input);
	        }
	        return array;
	    }

	    private void WriteArray(int[] array, DataOutput output) {
	        if (array == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(array.Length);
	        foreach (int i in array) {
	            output.WriteInt(i);
	        }
	    }

	    private int[] ReadArray(DataInput input) {
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
