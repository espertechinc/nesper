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
	public class DIOBoxedFloatArrayNullableSerde : DataInputOutputSerde<float?[]> {
	    public readonly static DIOBoxedFloatArrayNullableSerde INSTANCE = new DIOBoxedFloatArrayNullableSerde();

	    private DIOBoxedFloatArrayNullableSerde() {
	    }

	    public void Write(float?[] @object, DataOutput output) {
	        WriteInternal(@object, output);
	    }

	    public float?[] Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public void Write(float?[] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteInternal(@object, output);
	    }

	    public float?[] Read(DataInput input, byte[] unitKey) {
	        return ReadInternal(input);
	    }

	    private void WriteInternal(float?[] @object, DataOutput output) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (float? i in @object) {
	            DIONullableFloatSerde.INSTANCE.Write(i, output);
	        }
	    }

	    private float?[] ReadInternal(DataInput input) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        float?[] array = new float?[len];
	        for (int i = 0; i < len; i++) {
	            array[i] = DIONullableFloatSerde.INSTANCE.Read(input);
	        }
	        return array;
	    }
	}
} // end of namespace
