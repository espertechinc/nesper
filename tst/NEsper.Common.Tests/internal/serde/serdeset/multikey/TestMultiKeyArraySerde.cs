///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.serde.serdeset.builtin.TestBuiltinSerde;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
	[TestFixture]
	public class TestMultiKeyArraySerde
	{
		[Test]
		public void TestSerde()
		{
			AssertSerde(DIOMultiKeyArrayBooleanSerde.INSTANCE, new MultiKeyArrayBoolean(new bool[] { true, false }));
			AssertSerde(DIOMultiKeyArrayByteSerde.INSTANCE, new MultiKeyArrayByte(new byte[] { 1, 2 }));
			AssertSerde(DIOMultiKeyArrayCharSerde.INSTANCE, new MultiKeyArrayChar(new char[] { 'a', 'b' }));
			AssertSerde(DIOMultiKeyArrayDoubleSerde.INSTANCE, new MultiKeyArrayDouble(new double[] { 1d, 2d }));
			AssertSerde(DIOMultiKeyArrayFloatSerde.INSTANCE, new MultiKeyArrayFloat(new float[] { 1f, 2f }));
			AssertSerde(DIOMultiKeyArrayIntSerde.INSTANCE, new MultiKeyArrayInt(new int[] { 1, 2 }));
			AssertSerde(DIOMultiKeyArrayLongSerde.INSTANCE, new MultiKeyArrayLong(new long[] { 1, 2 }));
			AssertSerde(DIOMultiKeyArrayObjectSerde.INSTANCE, new MultiKeyArrayObject(new object[] { "A", "B" }));
			AssertSerde(DIOMultiKeyArrayShortSerde.INSTANCE, new MultiKeyArrayShort(new short[] { 1, 2 }));
		}
	}
} // end of namespace
