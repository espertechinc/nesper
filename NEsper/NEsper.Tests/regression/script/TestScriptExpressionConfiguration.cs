///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using NUnit.Framework;

namespace com.espertech.esper.regression.script
{
    [TestFixture]
    public class TestScriptExpressionConfiguration
    {
        [Test]
        public void TestConfig()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ScriptsConfig.DefaultDialect = "dummy";
            config.AddEventType(typeof(SupportBean));
            EPServiceProvider engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();

            try
            {
                engine.EPAdministrator.CreateEPL("expression abc [10] select * from SupportBean");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Failed to obtain script engine for dialect 'dummy' for script 'abc' [expression abc [10] select * from SupportBean]",
                    ex.Message);
            }
        }
    }
}