///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestSimpleTypeCasterFactory : AbstractTestBase
    {
        [Test]
        public void TestGetCaster()
        {
            object[][] tests = {
                new object[] {typeof(long), 10, 10L},
                new object[] {typeof(double), 1, 1d},
                new object[] {typeof(int), 0x1, 1},
                new object[] {typeof(float), 100, 100f},
                new object[] {typeof(int?), (short) 2, 2},
                new object[] {typeof(byte), (short) 2, (byte) 2},
                new object[] {typeof(short), (long) 2, (short) 2},
                new object[] {typeof(char), 'a', 'a'}
            };

            for (var i = 0; i < tests.Length; i++)
            {
                var caster = SimpleTypeCasterFactory.GetCaster(null, (Type) tests[i][0]);
                Assert.AreEqual(tests[i][2], caster.Cast(tests[i][1]), "error in row:" + i);
            }

            Assert.AreEqual('A', SimpleTypeCasterFactory.GetCaster(typeof(string), typeof(char)).Cast("ABC"));
            Assert.AreEqual(new BigInteger(100), SimpleTypeCasterFactory.GetCaster(typeof(long?), typeof(BigInteger)).Cast(100L));
            Assert.AreEqual(100.0m, SimpleTypeCasterFactory.GetCaster(typeof(long?), typeof(decimal)).Cast(100L));
            Assert.AreEqual(100.0m, SimpleTypeCasterFactory.GetCaster(typeof(double?), typeof(decimal)).Cast(100d));
        }
    }
} // end of namespace