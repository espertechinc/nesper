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
	public class DIOPrimitiveCharArray2DimNullableSerde : DataInputOutputSerdeBase<char[][]> {
	    public static readonly DIOPrimitiveCharArray2DimNullableSerde INSTANCE = new DIOPrimitiveCharArray2DimNullableSerde();

	    private DIOPrimitiveCharArray2DimNullableSerde() {
	    }

	    public override void Write(char[][] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (char[] i in @object) {
	            WriteArray(i, output);
	        }
	    }

	    public override char[][] ReadValue(DataInput input, byte[] unitKey) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        char[][] array = new char[len][];
	        for (int i = 0; i < len; i++) {
	            array[i] = ReadArray(input);
	        }
	        return array;
	    }

	    private void WriteArray(char[] array, DataOutput output) {
	        if (array == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(array.Length);
	        foreach (char i in array) {
	            output.WriteChar(i);
	        }
	    }

	    private char[] ReadArray(DataInput input) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        char[] array = new char[len];
	        for (int i = 0; i < len; i++) {
	            array[i] = input.ReadChar();
	        }
	        return array;
	    }
	}
} // end of namespace
