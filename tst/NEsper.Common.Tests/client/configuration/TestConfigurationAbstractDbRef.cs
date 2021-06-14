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
    public class TestConfigurationAbstractDbRef : AbstractCommonTest
    {
        private void TryInvalid(
            Type type,
            string text)
        {
            try
            {
                var config = new ConfigurationCommonDBRef();
                config.AddTypeBinding(typeof(int), type);
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
                "Unsupported type 'System.Console' when expecting any of: [BOOLEAN, BYTE, BYTE_ARRAY, DECIMAL, DOUBLE, FLOAT, INT32, INT64, INT16, STRING, TIMESTAMP]");

            var config = new ConfigurationCommonDBRef();
            config.AddTypeBinding(typeof(long), typeof(int));
        }
    }
} // end of namespace
