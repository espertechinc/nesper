///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Numerics;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestSimpleNumberCoercion : AbstractCommonTest
    {
        [Test]
        public void TestGetCoercer()
        {
            ClassicAssert.AreEqual(1d, SimpleNumberCoercerFactory.GetCoercer(null, typeof(double?)).CoerceBoxed(1d));
            ClassicAssert.AreEqual(1d, SimpleNumberCoercerFactory.GetCoercer(typeof(double?), typeof(double?)).CoerceBoxed(1d));
            ClassicAssert.AreEqual(5d, SimpleNumberCoercerFactory.GetCoercer(typeof(int?), typeof(double?)).CoerceBoxed(5));
            ClassicAssert.AreEqual(6d, SimpleNumberCoercerFactory.GetCoercer(typeof(byte?), typeof(double?)).CoerceBoxed((byte) 6));
            ClassicAssert.AreEqual(3f, SimpleNumberCoercerFactory.GetCoercer(typeof(long?), typeof(float?)).CoerceBoxed((long) 3));
            ClassicAssert.AreEqual((short) 2, SimpleNumberCoercerFactory.GetCoercer(typeof(long?), typeof(short?)).CoerceBoxed((long) 2));
            ClassicAssert.AreEqual(4, SimpleNumberCoercerFactory.GetCoercer(typeof(long?), typeof(int?)).CoerceBoxed((long) 4));
            ClassicAssert.AreEqual((byte) 5, SimpleNumberCoercerFactory.GetCoercer(typeof(long?), typeof(byte?)).CoerceBoxed((long) 5));
            ClassicAssert.AreEqual(8L, SimpleNumberCoercerFactory.GetCoercer(typeof(long?), typeof(long?)).CoerceBoxed((long) 8));
            ClassicAssert.AreEqual(new BigInteger(8), SimpleNumberCoercerFactory.GetCoercer(typeof(int), typeof(BigInteger)).CoerceBoxed(8));
            ClassicAssert.AreEqual(9m, SimpleNumberCoercerFactory.GetCoercer(typeof(int), typeof(decimal?)).CoerceBoxed(9));
            ClassicAssert.AreEqual(9m, SimpleNumberCoercerFactory.GetCoercer(typeof(double), typeof(decimal?)).CoerceBoxed(9.0));

            ClassicAssert.AreEqual(9.0m, SimpleNumberCoercerFactory.GetCoercer(typeof(double), typeof(decimal?)).CoerceBoxed(9.0));
            ClassicAssert.AreEqual(9m, SimpleNumberCoercerFactory.GetCoercer(typeof(long), typeof(decimal?)).CoerceBoxed(9));
            ClassicAssert.AreEqual(10m, SimpleNumberCoercerFactory.GetCoercer(typeof(decimal), typeof(decimal?)).CoerceBoxed(10m));

            ClassicAssert.AreEqual(new BigInteger(9), SimpleNumberCoercerFactory.GetCoercerBigInteger(typeof(long)).CoerceBoxedBigInt(9));
            ClassicAssert.AreEqual(new BigInteger(10), SimpleNumberCoercerFactory.GetCoercerBigInteger(typeof(BigInteger)).CoerceBoxedBigInt(new BigInteger(10)));

            //Assert.Throws<ArgumentException>(() => TypeHelper.CoerceBoxed(10, typeof(int)));
        }
    }
} // end of namespace
