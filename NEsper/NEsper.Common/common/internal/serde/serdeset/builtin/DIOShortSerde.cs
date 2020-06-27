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
	/// Binding for non-null short values.
	/// </summary>
	public class DIOShortSerde : DataInputOutputSerdeBase<short> {
	    public static readonly DIOShortSerde INSTANCE = new DIOShortSerde();

	    private DIOShortSerde() {
	    }

	    public override void Write(short @object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        output.WriteShort(@object);
	    }

	    public void Write(short @object, DataOutput stream) {
	        stream.WriteShort(@object);
	    }

	    public override short Read(DataInput input, byte[] resourceKey) {
	        return input.ReadShort();
	    }

	    public short Read(DataInput input) {
	        return input.ReadShort();
	    }
	}
} // end of namespace
