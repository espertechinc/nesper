///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.configuration.common;

using NUnit.Framework;

namespace com.espertech.esper.common.client.configuration
{
    [TestFixture]
    public class TestConfigurationAbstractDbRef : AbstractTestBase
    {
        private void TryInvalid(
            Type type,
            string text)
        {
            try
            {
                var config = new ConfigurationCommonDBRef();
                config.AddSqlTypeBinding(typeof(int), type);
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual(text, ex.Message);
            }
        }

        [Test]
        public void TestTypeMapping()
        {
            TryInvalid(
                typeof(Console),
                "Unsupported type 'Console' when expecting any of: [String, Decimal, Boolean, Byte, Short, Int, Long, Float, Double, ByteArray]");

            var config = new ConfigurationCommonDBRef();
            config.AddSqlTypeBinding(typeof(long), typeof(int));
        }
    }
} // end of namespace
