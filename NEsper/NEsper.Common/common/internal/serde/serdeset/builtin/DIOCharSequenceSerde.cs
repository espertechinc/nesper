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
namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	/// <summary>
	/// Binding for (nullable) String-typed values.
	/// </summary>
	public class DIOCharSequenceSerde : DataInputOutputSerde<CharSequence> {
	    public readonly static DIOCharSequenceSerde INSTANCE = new DIOCharSequenceSerde();

	    private DIOCharSequenceSerde() {
	    }

	    public void Write(CharSequence object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        Write(object, output);
	    }

	    public void Write(CharSequence object, DataOutput stream) {
	        if (object != null) {
	            stream.WriteBoolean(true);
	            stream.WriteUTF(object.ToString());
	        } else {
	            stream.WriteBoolean(false);
	        }
	    }

	    public CharSequence Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public CharSequence Read(DataInput input, byte[] resourceKey) {
	        return ReadInternal(input);
	    }

	    private CharSequence ReadInternal(DataInput input) {
	        if (input.ReadBoolean()) {
	            return input.ReadUTF();
	        }
	        return null;
	    }
	}
} // end of namespace
