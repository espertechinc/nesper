///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIONullableObjectArraySerde : DataInputOutputSerde<object[]> {
	    private readonly Type componentType;
	    private readonly DataInputOutputSerde componentBinding;

	    public DIONullableObjectArraySerde(Type componentType, DataInputOutputSerde componentBinding) {
	        this.componentType = componentType;
	        this.componentBinding = componentBinding;
	    }

	    public void Write(object[] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteInternal(@object, output, unitKey, writer);
	    }

	    public object[] Read(DataInput input, byte[] unitKey) {
	        return ReadInternal(input, unitKey);
	    }

	    private void WriteInternal(object[] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (object i in @object) {
	            componentBinding.Write(i, output, unitKey, writer);
	        }
	    }

	    private object[] ReadInternal(DataInput input, byte[] unitKey) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        object array = Array.NewInstance(componentType, len);
	        for (int i = 0; i < len; i++) {
	            Array.Set(array, i, componentBinding.Read(input, unitKey));
	        }
	        return (object[]) array;
	    }
	}
} // end of namespace
