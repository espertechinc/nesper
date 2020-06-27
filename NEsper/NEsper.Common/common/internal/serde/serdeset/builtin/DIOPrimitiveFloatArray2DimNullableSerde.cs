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
	public class DIOPrimitiveFloatArray2DimNullableSerde : DataInputOutputSerdeBase<float[][]> {
	    public static readonly DIOPrimitiveFloatArray2DimNullableSerde INSTANCE = new DIOPrimitiveFloatArray2DimNullableSerde();

	    private DIOPrimitiveFloatArray2DimNullableSerde() {
	    }

	    public override void Write(float[][] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (float[] i in @object) {
	            WriteArray(i, output);
	        }
	    }

	    public override float[][] Read(DataInput input, byte[] unitKey) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        float[][] array = new float[len][];
	        for (int i = 0; i < len; i++) {
	            array[i] = ReadArray(input);
	        }
	        return array;
	    }

	    private void WriteArray(float[] array, DataOutput output) {
	        if (array == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(array.Length);
	        foreach (float i in array) {
	            output.WriteFloat(i);
	        }
	    }

	    private float[] ReadArray(DataInput input) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        float[] array = new float[len];
	        for (int i = 0; i < len; i++) {
	            array[i] = input.ReadFloat();
	        }
	        return array;
	    }
	}
} // end of namespace
