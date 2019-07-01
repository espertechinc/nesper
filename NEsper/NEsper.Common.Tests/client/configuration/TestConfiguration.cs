///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.client.configuration
{
    [TestFixture]
    public class TestConfiguration : AbstractTestBase
    {
        public const string ESPER_TEST_CONFIG = "regression/esper.test.readconfig.cfg.xml";

        private Configuration config;

        [SetUp]
        public void SetUp()
        {
            config = new Configuration(container);
            config.Runtime.Logging.IsEnableExecutionDebug = true;
        }

        [Test]
        public void TestString()
        {
            config.Configure(ESPER_TEST_CONFIG);
            TestConfigurationParser.AssertFileConfig(config);
        }

        [Test]
        public void TestURL()
        {
            config.Configure(container.ResourceManager().ResolveResourceURL(ESPER_TEST_CONFIG));
            TestConfigurationParser.AssertFileConfig(config);
        }

        [Test]
        public void TestFile()
        {
            config.Configure(container.ResourceManager().ResolveResourceFile(ESPER_TEST_CONFIG));
            TestConfigurationParser.AssertFileConfig(config);
        }

        [Test]
        public void TestAddEventTypeName()
        {
            ConfigurationCommon common = config.Common;
            common.AddEventType("AEventType", "BClassName");

            Assert.IsTrue(common.IsEventTypeExists("AEventType"));
            Assert.AreEqual(1, common.EventTypeNames.Count);
            Assert.AreEqual("BClassName", common.EventTypeNames.Get("AEventType"));
            AssertDefaultConfig();
        }

        private void AssertDefaultConfig()
        {
            ConfigurationCommon common = config.Common;
            Assert.AreEqual(5, common.Imports.Count);
            Assert.AreEqual("System", common.Imports[0]);
            Assert.AreEqual("System.Collections", common.Imports[1]);
            Assert.AreEqual("System.Text", common.Imports[2]);
            Assert.AreEqual("com.espertech.esper.common.client.annotation", common.Imports[3]);
            Assert.AreEqual("com.espertech.esper.common.internal.epl.dataflow.ops", common.Imports[4]);
        }
    }
} // end of namespace
