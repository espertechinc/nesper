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
	/// Binding for non-null double values.
	/// </summary>
	public class DIODoubleSerde : DataInputOutputSerde<double> {
	    public readonly static DIODoubleSerde INSTANCE = new DIODoubleSerde();

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    private DIODoubleSerde() {
	    }

	    public void Write(double @object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        output.WriteDouble(@object);
	    }

	    public void Write(double @object, DataOutput stream) {
	        stream.WriteDouble(@object);
	    }

	    public double Read(DataInput s, byte[] resourceKey) {
	        return s.ReadDouble();
	    }

	    public double Read(DataInput input) {
	        return input.ReadDouble();
	    }
	}
} // end of namespace
