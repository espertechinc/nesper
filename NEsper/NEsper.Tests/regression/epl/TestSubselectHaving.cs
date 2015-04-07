///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectHaving 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();        
            config.AddEventType("SupportBean", typeof(SupportBean));
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestHavingSubselectWithGroupBy()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MaxAmountEvent));
            RunAssertionHavingSubselectWithGroupBy(true);
            RunAssertionHavingSubselectWithGroupBy(false);
        }

        private void RunAssertionHavingSubselectWithGroupBy(bool namedWindow)
        {
            String eplCreate = namedWindow ?
                    "create window MyInfra.std:unique(Key) as MaxAmountEvent" :
                    "create table MyInfra(Key string primary key, MaxAmount double)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select * from MaxAmountEvent");
    
            String stmtText = "select TheString as c0, Sum(IntPrimitive) as c1 " +
                    "from SupportBean.std:groupwin(TheString).win:length(2) as sb " +
                    "group by TheString " +
                    "having Sum(IntPrimitive) > (select MaxAmount from MyInfra as mw where sb.TheString = mw.Key)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            String[] fields = "c0,c1".Split(',');
    
            // set some amounts
            _epService.EPRuntime.SendEvent(new MaxAmountEvent("G1", 10));
            _epService.EPRuntime.SendEvent(new MaxAmountEvent("G2", 20));
            _epService.EPRuntime.SendEvent(new MaxAmountEvent("G3", 30));
    
            // send some events
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 5));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 19));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 28));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 21});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 18));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 4));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 2));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 29));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G3", 31});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 4));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G3", 33});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 6));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 26));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 99));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 105});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 100});

            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        public sealed class MaxAmountEvent
        {
            public MaxAmountEvent(String key, double maxAmount)
            {
                Key = key;
                MaxAmount = maxAmount;
            }

            public string Key { get; private set; }

            public double MaxAmount { get; private set; }
        }
    }
}
