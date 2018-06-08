///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.revision
{
    public class ExecEventRevisionWindowedTime : RegressionExecution {
        private readonly string[] fields = "K0,P1,P5".Split(',');
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("FullEvent", typeof(SupportRevisionFull));
            configuration.AddEventType("D1", typeof(SupportDeltaOne));
            configuration.AddEventType("D5", typeof(SupportDeltaFive));
    
            var configRev = new ConfigurationRevisionEventType();
            configRev.KeyPropertyNames = new string[]{"K0"};
            configRev.AddNameBaseEventType("FullEvent");
            configRev.AddNameDeltaEventType("D1");
            configRev.AddNameDeltaEventType("D5");
            configuration.AddRevisionEventType("RevisableQuote", configRev);
        }
    
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecEventRevisionWindowedTime))) {
                return;
            }
            RunAssertionTimeWindow(epService);
        }
    
        private void RunAssertionTimeWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmtCreateWin = epService.EPAdministrator.CreateEPL("create window RevQuote#time(10 sec) as select * from RevisableQuote");
            epService.EPAdministrator.CreateEPL("insert into RevQuote select * from FullEvent");
            epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D1");
            epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D5");
    
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL("select irstream * from RevQuote");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportRevisionFull("a", "a10", "a50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"a", "a10", "a50"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"a", "a10", "a50"});
    
            SendTimer(epService, 1000);
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive("a", "a11", "a51"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"a", "a11", "a51"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"a", "a10", "a50"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"a", "a11", "a51"});
    
            SendTimer(epService, 2000);
    
            epService.EPRuntime.SendEvent(new SupportRevisionFull("b", "b10", "b50"));
            epService.EPRuntime.SendEvent(new SupportRevisionFull("c", "c10", "c50"));
    
            SendTimer(epService, 3000);
            epService.EPRuntime.SendEvent(new SupportDeltaOne("c", "c11", "c51"));
    
            SendTimer(epService, 8000);
            epService.EPRuntime.SendEvent(new SupportDeltaOne("c", "c12", "c52"));
            listenerOne.Reset();
    
            SendTimer(epService, 10000);
            Assert.IsFalse(listenerOne.IsInvoked);
    
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetOldAndReset(), fields, new object[]{"a", "a11", "a51"});
    
            SendTimer(epService, 12000);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetOldAndReset(), fields, new object[]{"b", "b10", "b50"});
    
            SendTimer(epService, 13000);
            Assert.IsFalse(listenerOne.IsInvoked);
    
            SendTimer(epService, 18000);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetOldAndReset(), fields, new object[]{"c", "c12", "c52"});
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
