///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestEventPropertyGetter
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
        }
    
        [Test]
        public void TestGetter() {
            String stmtText = "select * from " + typeof(SupportMarketDataBean).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            MyGetterUpdateListener listener = new MyGetterUpdateListener(stmt.EventType);
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("sym", 100, 1000L, "feed"));
            Assert.AreEqual("sym", listener.LastSymbol);
            Assert.AreEqual(1000L, (long) listener.LastVolume);
            Assert.AreEqual(stmt, listener.Statement);
            Assert.AreEqual(_epService, listener.ServiceProvider);
        }
    }
}
