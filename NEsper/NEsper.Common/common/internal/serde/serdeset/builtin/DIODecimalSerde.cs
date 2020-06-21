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
	/// Binding for non-null decimal values.
	/// </summary>
	public class DIODecimalSerde : DataInputOutputSerdeBase<decimal> {
	    public static readonly DIODecimalSerde INSTANCE = new DIODecimalSerde();

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    private DIODecimalSerde() {
	    }

	    public override void Write(decimal @object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        output.WriteDecimal(@object);
	    }

	    public void Write(decimal @object, DataOutput stream) {
	        stream.WriteDecimal(@object);
	    }

	    public override decimal ReadValue(DataInput s, byte[] resourceKey) {
	        return s.ReadDecimal();
	    }

	    public decimal Read(DataInput input) {
	        return input.ReadDecimal();
	    }
	}
} // end of namespace
