///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	/// <summary>
	/// Binding for a value that must provide serialization for a specific type, and
	/// for the opaque "object" type.
	/// </summary>

	public abstract class DIOWrappingInputOutputSerde<T> : DataInputOutputSerde<object>
	{
		private DataInputOutputSerde<T> _serde;
		private bool _canBeNull;
		
	    private DIOWrappingInputOutputSerde(DataInputOutputSerde<T> serde)
	    {
		    _serde = serde;
		    _canBeNull = typeof(T).CanBeNull();
	    }

	    public void Write(
		    object @object,
		    DataOutput output,
		    byte[] unitKey,
		    EventBeanCollatedWriter writer)
	    {
		    if (@object == null) {
			    if (_canBeNull) {
				    _serde.Write(default(T), output, unitKey, writer);
			    }
			    else {
				    throw new ArgumentNullException(nameof(@object));
			    }
		    }
		    else {
			    _serde.Write((T) @object, output, unitKey, writer);
		    }
	    }

	    public object Read(
		    DataInput input,
		    byte[] unitKey)
	    {
		    return _serde.Read(input, unitKey);
	    }
	}
} // end of namespace
