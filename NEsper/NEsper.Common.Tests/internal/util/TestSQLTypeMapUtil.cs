///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestSQLTypeMapUtil
    {
        [Test]
        public void TestMapping()
        {
            IDictionary<int, Type> testData = new Dictionary<int, Type>();
            testData.Put(Types.CHAR, typeof(string));
            testData.Put(Types.VARCHAR, typeof(string));
            testData.Put(Types.LONGVARCHAR, typeof(string));
            testData.Put(Types.NUMERIC, typeof(decimal));
            testData.Put(Types.DECIMAL, typeof(decimal));
            testData.Put(Types.BIT, typeof(bool?));
            testData.Put(Types.BOOLEAN, typeof(bool?));
            testData.Put(Types.TINYINT, typeof(Byte));
            testData.Put(Types.SMALLINT, typeof(short?));
            testData.Put(Types.INTEGER, typeof(int));
            testData.Put(Types.BIGINT, typeof(long?));
            testData.Put(Types.REAL, typeof(float?));
            testData.Put(Types.FLOAT, typeof(double?));
            testData.Put(Types.DOUBLE, typeof(double?));
            testData.Put(Types.BINARY, typeof(byte[]));
            testData.Put(Types.VARBINARY, typeof(byte[]));
            testData.Put(Types.LONGVARBINARY, typeof(byte[]));
            testData.Put(Types.DATE, typeof(java.sql.Date));
            testData.Put(Types.TIMESTAMP, typeof(java.sql.Timestamp));
            testData.Put(Types.TIME, typeof(java.sql.Time));
            testData.Put(Types.CLOB, typeof(java.sql.Clob));
            testData.Put(Types.BLOB, typeof(java.sql.Blob));
            testData.Put(Types.ARRAY, typeof(java.sql.Array));
            testData.Put(Types.STRUCT, typeof(java.sql.Struct));
            testData.Put(Types.REF, typeof(java.sql.Ref));
            testData.Put(Types.DATALINK, typeof(java.net.URL));

            foreach (int type in testData.Keys)
            {
                Type result = SQLTypeMapUtil.SqlTypeToClass(type, null, ClassForNameProviderDefault.INSTANCE);
                log.Debug(".testMapping Mapping " + type + " to " + result.SimpleName);
                Assert.AreEqual(testData.Get(type), result);
            }

            Assert.AreEqual(typeof(string), SQLTypeMapUtil.SqlTypeToClass(Types.JAVA_OBJECT, "java.lang.String", ClassForNameProviderDefault.INSTANCE));
            Assert.AreEqual(typeof(string), SQLTypeMapUtil.SqlTypeToClass(Types.DISTINCT, "java.lang.String", ClassForNameProviderDefault.INSTANCE));
        }

        [Test]
        public void TestMappingInvalid()
        {
            TryInvalid(Types.JAVA_OBJECT, null);
            TryInvalid(Types.JAVA_OBJECT, "xx");
            TryInvalid(Types.DISTINCT, null);
            TryInvalid(Int32.MaxValue, "yy");
        }

        private void TryInvalid(int type, string classname)
        {
            try
            {
                SQLTypeMapUtil.SqlTypeToClass(type, classname, ClassForNameProviderDefault.INSTANCE);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // expected
            }
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace