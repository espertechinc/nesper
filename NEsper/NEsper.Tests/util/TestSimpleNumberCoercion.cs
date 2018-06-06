///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestSimpleNumberCoercion
    {
        [Test]
        public void TestGetCoercer()
        {
            Assert.AreEqual(1d, CoercerFactory.GetCoercer(null, typeof(double?)).Invoke(1d));
            Assert.AreEqual(1d, CoercerFactory.GetCoercer(typeof(double?), typeof(double?)).Invoke(1d));
            Assert.AreEqual(5d, CoercerFactory.GetCoercer(typeof(int?), typeof(double?)).Invoke(5));
            Assert.AreEqual(6d, CoercerFactory.GetCoercer(typeof(byte?), typeof(double?)).Invoke((byte) 6));
            Assert.AreEqual(3f, CoercerFactory.GetCoercer(typeof(long?), typeof(float?)).Invoke((long) 3));
            Assert.AreEqual((short) 2, CoercerFactory.GetCoercer(typeof(long?), typeof(short?)).Invoke((long) 2));
            Assert.AreEqual(4, CoercerFactory.GetCoercer(typeof(long?), typeof(int?)).Invoke((long) 4));
            Assert.AreEqual((byte) 5, CoercerFactory.GetCoercer(typeof(long?), typeof(sbyte?)).Invoke((long) 5));
            Assert.AreEqual(8L, CoercerFactory.GetCoercer(typeof(long?), typeof(long?)).Invoke((long) 8));
            Assert.AreEqual(9.0m, CoercerFactory.GetCoercer(typeof(int), typeof(decimal)).Invoke(9));
            Assert.AreEqual(9.0m, CoercerFactory.GetCoercer(typeof(double), typeof(decimal)).Invoke(9.0));
            Assert.AreEqual(9.0m, CoercerFactory.GetCoercer(typeof(int), typeof(decimal?)).Invoke(9));
            Assert.AreEqual(9.0m, CoercerFactory.GetCoercer(typeof(double), typeof(decimal?)).Invoke(9.0));

            try {
                CoercerFactory.CoerceBoxed(10, typeof(char));
                Assert.Fail();
            }
            catch (ArgumentException) {
                // Expected
            }
        }
    }
}
