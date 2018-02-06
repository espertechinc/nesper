///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.client
{
    [TestFixture]
    public class TestConfiguration 
    {
        protected internal static readonly String ESPER_TEST_CONFIG = "regression/esper.test.readconfig.cfg.xml";
    
        private Configuration _config;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _config = new Configuration(_container);
            _config.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        [Test]
        public void TestString()
        {
            _config.Configure(ESPER_TEST_CONFIG);
            TestConfigurationParser.AssertFileConfig(_config);
        }
    
        [Test]
        public void TestURL()
        {
            Uri url = _container.Resolve<IResourceManager>().ResolveResourceURL(ESPER_TEST_CONFIG);
            _config.Configure(url);
            TestConfigurationParser.AssertFileConfig(_config);
        }
    
        [Test]
        public void TestFile()
        {
            FileInfo fileInfo = _container.Resolve<IResourceManager>().ResolveResourceFile(ESPER_TEST_CONFIG);
            _config.Configure(fileInfo);
            TestConfigurationParser.AssertFileConfig(_config);
        }
    
        [Test]
        public void TestAddEventTypeName()
        {
            _config.AddEventType("AEventType", "BClassName");
    
            Assert.IsTrue(_config.IsEventTypeExists("AEventType"));
            Assert.AreEqual(1, _config.EventTypeNames.Count);
            Assert.AreEqual("BClassName", _config.EventTypeNames.Get("AEventType"));
            AssertDefaultConfig();
        }

        private void AssertDefaultConfig()
        {
            Assert.AreEqual(5, _config.Imports.Count);
            Assert.AreEqual(new AutoImportDesc("System"), _config.Imports[0]);
            Assert.AreEqual(new AutoImportDesc("System.Collections"), _config.Imports[1]);
            Assert.AreEqual(new AutoImportDesc("System.Text"), _config.Imports[2]);
            Assert.AreEqual(new AutoImportDesc("com.espertech.esper.client.annotation"), _config.Imports[3]);
            Assert.AreEqual(new AutoImportDesc("com.espertech.esper.dataflow.ops"), _config.Imports[4]);
        }
    }
}
