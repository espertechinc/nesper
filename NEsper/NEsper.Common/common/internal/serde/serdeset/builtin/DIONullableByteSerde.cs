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
	/// Binding for nullable byte values.
	/// </summary>
	public class DIONullableByteSerde : DataInputOutputSerdeBase<byte?> {
	    public static readonly DIONullableByteSerde INSTANCE = new DIONullableByteSerde();

	    private DIONullableByteSerde() {
	    }

	    public override void Write(byte? @object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        Write(@object, output);
	    }

	    public void Write(byte? @object, DataOutput stream) {
	        bool isNull = @object == null;
	        stream.WriteBoolean(isNull);
	        if (!isNull) {
	            stream.WriteByte(@object.Value);
	        }
	    }

	    public byte? Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public override byte? Read(DataInput s, byte[] resourceKey) {
	        return ReadInternal(s);
	    }

	    private byte? ReadInternal(DataInput input) {
	        bool isNull = input.ReadBoolean();
	        if (isNull) {
	            return null;
	        }
	        return input.ReadByte();
	    }
	}
} // end of namespace
