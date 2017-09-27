///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.compat.logging;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerfPropertyAccess 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _updateListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            _updateListener = null;
        }
    
        [Test]
        public void TestWithPerfPropertyAccess()
        {
            var methodName = ".testPerfPropertyAccess";
            var joinStatement = 
                "select * from " + typeof(SupportBeanCombinedProps).FullName + "#length(1)" +
                " where Indexed[0].Mapped('a').value = 'dummy'";
    
            _epService.EPAdministrator.CreateEPL(joinStatement).Events += 
                _updateListener.Update;
    
            // Send events for each stream
            var theEvent = SupportBeanCombinedProps.MakeDefaultBean();
            log.Info("{0} : Sending events", methodName);
    
            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        SendEvent(theEvent);
                    }
                    log.Info(methodName + " Done sending events");
                });

            log.Info("{0} : delta={1}", methodName, delta);
    
            Assert.That(delta, Is.LessThan(1000));
        }
    
        private void SendEvent(Object theEvent)
        {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
