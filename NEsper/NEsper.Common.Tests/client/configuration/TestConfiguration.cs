///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.epl.dataflow.ops;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.client.configuration
{
    [TestFixture]
    public class TestConfiguration : AbstractCommonTest
    {
        public const string ESPER_TEST_CONFIG = "regression/esper.test.readconfig.cfg.xml";

        private Configuration config;

        [SetUp]
        public void SetUp()
        {
            config = new Configuration(container);
            config.Runtime.Logging.IsEnableExecutionDebug = true;
        }

        [Test, RunInApplicationDomain]
        public void TestString()
        {
            config.Configure(ESPER_TEST_CONFIG);
            TestConfigurationParser.AssertFileConfig(config);
        }

        [Test, RunInApplicationDomain]
        public void TestURL()
        {
            config.Configure(container.ResourceManager().ResolveResourceURL(ESPER_TEST_CONFIG));
            TestConfigurationParser.AssertFileConfig(config);
        }

        [Test, RunInApplicationDomain]
        public void TestFile()
        {
            config.Configure(container.ResourceManager().ResolveResourceFile(ESPER_TEST_CONFIG));
            TestConfigurationParser.AssertFileConfig(config);
        }

        [Test, RunInApplicationDomain]
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
            Assert.That(
                common.Imports,
                Has.Count.EqualTo(4));
            Assert.That(
                common.Imports,
                Contains.Item(ImportBuiltinAnnotations.Instance));
            Assert.That(
                common.Imports,
                Contains.Item(new ImportNamespace("System")));
            Assert.That(
                common.Imports,
                Contains.Item(new ImportNamespace("System.Text")));
            Assert.That(
                common.Imports,
                Contains.Item(new ImportNamespace(typeof(BeaconSourceForge))));
        }
    }
} // end of namespace
