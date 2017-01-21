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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableOutputRateLimiting
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1), typeof(SupportBean_S2)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestOutputRateLimiting() {
            AtomicLong currentTime = new AtomicLong(0);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
    
            _epService.EPAdministrator.CreateEPL("@Name('create') create table MyTable as (\n" +
                    "key string primary key, thesum sum(int))");
            _epService.EPAdministrator.CreateEPL("@Name('select') into table MyTable " +
                    "select sum(IntPrimitive) as thesum from SupportBean group by TheString");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 30));
            _epService.EPAdministrator.GetStatement("create").Dispose();
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select key, thesum from MyTable output snapshot every 1 seconds");
            stmt.AddListener(_listener);
    
            currentTime.Set(currentTime.Get() + 1000L);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), "key,thesum".Split(','),
                    new object[][] { new object[] { "E1", 40 }, new object[] { "E2", 20 } });
    
            currentTime.Set(currentTime.Get() + 1000L);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
            Assert.IsTrue(_listener.IsInvoked);
    
            stmt.Dispose();
        }
    
    }
}
