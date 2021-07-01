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
	public class DIOPrimitiveShortArray2DimNullableSerde : DataInputOutputSerdeBase<short[][]> {
	    public static readonly DIOPrimitiveShortArray2DimNullableSerde INSTANCE = new DIOPrimitiveShortArray2DimNullableSerde();

	    private DIOPrimitiveShortArray2DimNullableSerde() {
	    }

	    public override void Write(short[][] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (short[] i in @object) {
	            WriteArray(i, output);
	        }
	    }

	    public override short[][] ReadValue(DataInput input, byte[] unitKey) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        short[][] array = new short[len][];
	        for (int i = 0; i < len; i++) {
	            array[i] = ReadArray(input);
	        }
	        return array;
	    }

	    private void WriteArray(short[] array, DataOutput output) {
	        if (array == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(array.Length);
	        foreach (short i in array) {
	            output.WriteShort(i);
	        }
	    }

	    private short[] ReadArray(DataInput input) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        short[] array = new short[len];
	        for (int i = 0; i < len; i++) {
	            array[i] = input.ReadShort();
	        }
	        return array;
	    }
	}
} // end of namespace
