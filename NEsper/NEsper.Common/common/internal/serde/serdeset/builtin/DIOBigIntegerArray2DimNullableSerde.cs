///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Numerics;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using java.math;
namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIOBigIntegerArray2DimNullableSerde : DataInputOutputSerde<BigInteger[][]> {
	    public readonly static DIOBigIntegerArray2DimNullableSerde INSTANCE = new DIOBigIntegerArray2DimNullableSerde();

	    private DIOBigIntegerArray2DimNullableSerde() {
	    }

	    public void Write(BigInteger[][] @object, DataOutput output) {
	        WriteInternal(@object, output);
	    }

	    public BigInteger[][] Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public void Write(BigInteger[][] @object, DataOutput output, byte[] unitKey, EventBeanCollatedWriter writer) {
	        WriteInternal(@object, output);
	    }

	    public BigInteger[][] Read(DataInput input, byte[] unitKey) {
	        return ReadInternal(input);
	    }

	    private void WriteInternal(BigInteger[][] @object, DataOutput output) {
	        if (@object == null) {
	            output.WriteInt(-1);
	            return;
	        }
	        output.WriteInt(@object.Length);
	        foreach (BigInteger[] i in @object) {
	            DIOBigIntegerArrayNullableSerde.INSTANCE.Write(i, output);
	        }
	    }

	    private BigInteger[][] ReadInternal(DataInput input) {
	        int len = input.ReadInt();
	        if (len == -1) {
	            return null;
	        }
	        BigInteger[][] array = new BigInteger[len][];
	        for (int i = 0; i < len; i++) {
	            array[i] = DIOBigIntegerArrayNullableSerde.INSTANCE.Read(input);
	        }
	        return array;
	    }
	}
} // end of namespace
