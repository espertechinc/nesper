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
	/// <summary>
	/// Binding for nullable integer values.
	/// </summary>
	public class DIONullableIntegerSerde : DataInputOutputSerdeBase<int?> {
	    public static readonly DIONullableIntegerSerde INSTANCE = new DIONullableIntegerSerde();

	    private DIONullableIntegerSerde() {
	    }

	    public override void Write(int? @object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        Write(@object, output);
	    }

	    public void Write(int? @object, DataOutput stream) {
	        bool isNull = @object == null;
	        stream.WriteBoolean(isNull);
	        if (!isNull) {
	            stream.WriteInt(@object.Value);
	        }
	    }

	    public int? Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public override int? ReadValue(DataInput input, byte[] resourceKey) {
	        return ReadInternal(input);
	    }

	    private int? ReadInternal(DataInput s) {
	        bool isNull = s.ReadBoolean();
	        if (isNull) {
	            return null;
	        }
	        return s.ReadInt();
	    }
	}
} // end of namespace
