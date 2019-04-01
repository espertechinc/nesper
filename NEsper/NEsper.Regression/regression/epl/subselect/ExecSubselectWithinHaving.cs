///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectWithinHaving : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(MaxAmountEvent));
            RunAssertionHavingSubselectWithGroupBy(epService, true);
            RunAssertionHavingSubselectWithGroupBy(epService, false);
        }
    
        private void RunAssertionHavingSubselectWithGroupBy(EPServiceProvider epService, bool namedWindow) {
            string eplCreate = namedWindow ?
                    "create window MyInfra#unique(key) as MaxAmountEvent" :
                    "create table MyInfra(key string primary key, maxAmount double)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select * from MaxAmountEvent");
    
            string stmtText = "select TheString as c0, sum(IntPrimitive) as c1 " +
                    "from SupportBean#groupwin(TheString)#length(2) as sb " +
                    "group by TheString " +
                    "having sum(IntPrimitive) > (select maxAmount from MyInfra as mw where sb.TheString = mw.key)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "c0,c1".Split(',');
    
            // set some amounts
            epService.EPRuntime.SendEvent(new MaxAmountEvent("G1", 10));
            epService.EPRuntime.SendEvent(new MaxAmountEvent("G2", 20));
            epService.EPRuntime.SendEvent(new MaxAmountEvent("G3", 30));
    
            // send some events
            epService.EPRuntime.SendEvent(new SupportBean("G1", 5));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 19));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 28));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 21});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 18));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 4));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("G3", 29));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G3", 31});
    
            epService.EPRuntime.SendEvent(new SupportBean("G3", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G3", 33});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 6));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 26));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 99));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 105});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 100});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        public class MaxAmountEvent
        {
            public MaxAmountEvent(string key, double maxAmount)
            {
                Key = key;
                MaxAmount = maxAmount;
            }

            [PropertyName("key")]
            public string Key { get; }

            [PropertyName("maxAmount")]
            public double MaxAmount { get; }
        }
    }
} // end of namespace
