///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using NUnit.Framework;

namespace com.espertech.esper.client
{
    [TestFixture]
    public class TestConfigurationDBRef 
    {
        [Test]
        public void TestTypeMapping()
        {
            TryInvalid("sometype", "Unsupported java type 'sometype' when expecting any of: [String, BigDecimal, Boolean, Byte, Short, Int, long?, Float, double?, ByteArray, SqlDate, SqlTime, SqlTimestamp]");
    
            ConfigurationDBRef config = new ConfigurationDBRef();
            //config.AddSqlTypesBinding(1, "int");
        }
    
        private void TryInvalid(String type, String text)
        {
            try
            {
                ConfigurationDBRef config = new ConfigurationDBRef();
                //config.AddSqlTypesBinding(1, type);
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual(text, ex.Message);
            }
        }
    }
}
