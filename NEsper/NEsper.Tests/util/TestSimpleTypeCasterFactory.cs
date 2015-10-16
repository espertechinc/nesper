///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class TestSimpleTypeCasterFactory 
    {
        [Test]
        public void TestGetCaster()
        {
            Object[][] tests = new Object[][] {
                    new object[] {typeof(long), 10, 10L},
                    new object[] {typeof(double), 1, 1d},
                    new object[] {typeof(int), 0x1, 1},
                    new object[] {typeof(float), 100, 100f},
                    new object[] {typeof(int?), (short)2, 2},
                    new object[] {typeof(byte?), (short)2, (byte)2},
                    new object[] {typeof(short), (long)2, (short)2},
                    };
    
            for (int i = 0; i < tests.Length; i++)
            {
                SimpleTypeCaster caster = SimpleTypeCasterFactory.GetCaster(null, (Type)tests[i][0]);
                Assert.AreEqual(tests[i][2], caster.Invoke(tests[i][1]), "error in row:" + i);
            }
            
            Assert.AreEqual('A', SimpleTypeCasterFactory.GetCaster(typeof(string), typeof(char)).Invoke("ABC"));

            //Assert.AreEqual(BigInteger.ValueOf(100), SimpleTypeCasterFactory.GetCaster(typeof(long?), typeof(BigInteger)).Cast(100L));
            //Assert.AreEqual(100.0m, SimpleTypeCasterFactory.GetCaster(typeof(long?), typeof(BigDecimal)).Cast(100L));
            //Assert.AreEqual(new BigDecimal(100d), SimpleTypeCasterFactory.GetCaster(typeof(double?), typeof(BigDecimal)).Cast(100d));
        }
    }
}
