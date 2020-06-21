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
	/// <summary>
	/// Binding for non-null float values.
	/// </summary>
	public class DIOFloatSerde : DataInputOutputSerde<float> {
	    public readonly static DIOFloatSerde INSTANCE = new DIOFloatSerde();

	    private DIOFloatSerde() {
	    }

	    public void Write(float @object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        output.WriteFloat(@object);
	    }

	    public void Write(float @object, DataOutput stream) {
	        stream.WriteFloat(@object);
	    }

	    public float Read(DataInput input) {
	        return input.ReadFloat();
	    }

	    public float Read(DataInput input, byte[] resourceKey) {
	        return input.ReadFloat();
	    }
	}
} // end of namespace
