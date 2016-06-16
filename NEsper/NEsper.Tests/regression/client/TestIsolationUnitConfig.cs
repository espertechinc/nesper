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

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestIsolationUnitConfig 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_A>();
            configuration.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
        }
    
        [TearDown]
        public void TearDown() {
        }
    
        [Test]
        public void TestNotAllowed()
        {
            try {
                _epService.GetEPServiceIsolated("i1");
                Assert.Fail();
            }
            catch (EPServiceNotAllowedException ex) {
                Assert.AreEqual("Isolated runtime requires execution setting to allow isolated services, please change execution settings under engine defaults", ex.Message);
            }
        }
    }
}
