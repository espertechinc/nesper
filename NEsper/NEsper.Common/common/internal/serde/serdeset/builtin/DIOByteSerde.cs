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
	/// Binding for non-null byte values.
	/// </summary>
	public class DIOByteSerde : DataInputOutputSerdeBase<byte> {
	    public static readonly DIOByteSerde INSTANCE = new DIOByteSerde();

	    private DIOByteSerde() {
	    }

	    public override void Write(byte @object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        output.WriteByte(@object);
	    }

	    public void Write(byte @object, DataOutput stream) {
	        stream.WriteByte(@object);
	    }

	    public override byte ReadValue(DataInput s, byte[] resourceKey) {
	        return s.ReadByte();
	    }

	    public byte Read(DataInput input) {
	        return input.ReadByte();
	    }
	}
} // end of namespace
