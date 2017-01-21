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
    public class TestDatabaseTypeEnum
    {
        [Test]
        public void TestLookup()
        {
            var types = new[]
                        {
                            new Object[] {"String", DatabaseTypeEnum.String},
                            new Object[] {"System.String", DatabaseTypeEnum.String},
                            new Object[] {"decimal", DatabaseTypeEnum.Decimal},
                            new Object[] {typeof(bool).FullName, DatabaseTypeEnum.Boolean},
                            new Object[] {typeof(byte).FullName, DatabaseTypeEnum.Byte},
                            new Object[] {"short", DatabaseTypeEnum.Short},
                            new Object[] {"int", DatabaseTypeEnum.Int},
                            new Object[] {"System.Int32", DatabaseTypeEnum.Int},
                            new Object[] {typeof(int).FullName, DatabaseTypeEnum.Int},
                            new Object[] {typeof(int?).FullName, DatabaseTypeEnum.Int},
                            new Object[] {"long", DatabaseTypeEnum.Long},
                            new Object[] {typeof(long).FullName, DatabaseTypeEnum.Long},
                            new Object[] {"System.DateTime", DatabaseTypeEnum.Timestamp}

                            //new Object[] {"sqldate", DatabaseTypeEnum.Timestamp},
                            //new Object[] {"date", DatabaseTypeEnum.Timestamp},
                            //new Object[] {typeof(DateTime).FullName, DatabaseTypeEnum.Timestamp},
                            //new Object[] {"time", DatabaseTypeEnum.Timestamp},
                            //new Object[] {"sqltimestamp", DatabaseTypeEnum.Timestamp},
                            //new Object[] {"timestamp", DatabaseTypeEnum.Timestamp}
                        };

            for (int i = 0; i < types.Length; i++) {
                var val = (DatabaseTypeEnum) types[i][1];
                Assert.AreEqual(val, DatabaseTypeEnum.GetEnum((String) types[i][0]));
            }
        }

        [Test]
        public void TestTypes()
        {
            var types = new[]
                        {
                            new object[] {DatabaseTypeEnum.String, typeof(string)},
                            new object[] {DatabaseTypeEnum.Decimal, typeof(decimal)},
                            new object[] {DatabaseTypeEnum.Boolean, typeof(bool)},
                            new object[] {DatabaseTypeEnum.Byte, typeof(byte)},
                            new object[] {DatabaseTypeEnum.Short, typeof(short)},
                            new object[] {DatabaseTypeEnum.Int, typeof(int)},
                            new object[] {DatabaseTypeEnum.Long, typeof(long)},
                            new object[] {DatabaseTypeEnum.Float, typeof(float)},
                            new object[] {DatabaseTypeEnum.Double, typeof(double)},
                            new object[] {DatabaseTypeEnum.ByteArray, typeof(byte[])},
                        };

            for (int i = 0; i < types.Length; i++) {
                var val = (DatabaseTypeEnum) types[i][0];
                Assert.AreEqual(types[i][1], val.Binding.DataType);
                Assert.AreEqual(types[i][1], val.DataType);
            }
        }
    }
}
