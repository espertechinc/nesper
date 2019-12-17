///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestDatabaseTypeEnum : AbstractCommonTest
    {
        [Test]
        public void TestLookup()
        {
            object[][] types = {
                new object[] {"string", DatabaseTypeEnum.STRING},
                new object[] {"System.String", DatabaseTypeEnum.STRING},
                new object[] {"System.String", DatabaseTypeEnum.STRING},
                new object[] {"decimal", DatabaseTypeEnum.DECIMAL},
                new object[] {typeof(bool?).FullName, DatabaseTypeEnum.BOOLEAN},
                new object[] {typeof(byte).FullName, DatabaseTypeEnum.BYTE},
                new object[] {"short", DatabaseTypeEnum.INT16},
                new object[] {"int", DatabaseTypeEnum.INT32},
                new object[] {"System.Int32", DatabaseTypeEnum.INT32},
                new object[] {typeof(int).FullName, DatabaseTypeEnum.INT32},
                new object[] {typeof(int?).FullName, DatabaseTypeEnum.INT32}
            };

            for (var i = 0; i < types.Length; i++) {
                var val = (DatabaseTypeEnum) types[i][1];
                Assert.AreEqual(val, DatabaseTypeEnumExtensions.GetEnum((string) types[i][0]));
            }
        }

        [Test]
        public void TestTypes()
        {
            object[][] types = {
                new object[] {DatabaseTypeEnum.STRING, typeof(string)},
                new object[] {DatabaseTypeEnum.DECIMAL, typeof(decimal?)},
                new object[] {DatabaseTypeEnum.BOOLEAN, typeof(bool?)},
                new object[] {DatabaseTypeEnum.BYTE, typeof(byte?)},
                new object[] {DatabaseTypeEnum.INT16, typeof(short?)},
                new object[] {DatabaseTypeEnum.INT32, typeof(int?)},
                new object[] {DatabaseTypeEnum.INT64, typeof(long?)},
                new object[] {DatabaseTypeEnum.FLOAT, typeof(float?)},
                new object[] {DatabaseTypeEnum.DOUBLE, typeof(double?)},
                new object[] {DatabaseTypeEnum.BYTE_ARRAY, typeof(byte[])}
            };

            for (var i = 0; i < types.Length; i++) {
                var val = (DatabaseTypeEnum) types[i][0];
                Assert.AreEqual(types[i][1], val.GetBinding().DataType.GetBoxedType());
                Assert.AreEqual(types[i][1], val.GetBoxedType());
            }
        }
    }
} // end of namespace
