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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectOrderOfEval : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsShareViews = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionCorrelatedSubqueryOrder(epService);
            RunAssertionOrderOfEvaluationSubselectFirst(epService);
        }
    
        private void RunAssertionCorrelatedSubqueryOrder(EPServiceProvider epService) {
            // ESPER-564
            epService.EPAdministrator.Configuration.AddEventType("TradeEvent", typeof(TradeEvent));
            var listener = new SupportUpdateListener();
    
            epService.EPAdministrator.CreateEPL("select * from TradeEvent#lastevent");
    
            epService.EPAdministrator.CreateEPL(
                    "select window(tl.*) as longItems, " +
                            "       (SELECT window(ts.*) AS shortItems FROM TradeEvent#time(20 minutes) as ts WHERE ts.securityID=tl.securityID) " +
                            "from TradeEvent#time(20 minutes) as tl " +
                            "where tl.securityID = 1000" +
                            "group by tl.securityID "
            ).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new TradeEvent(DateTimeHelper.CurrentTimeMillis, 1000, 50, 1));
            Assert.AreEqual(1, ((object[]) listener.AssertOneGetNew().Get("longItems")).Length);
            Assert.AreEqual(1, ((object[]) listener.AssertOneGetNew().Get("shortItems")).Length);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new TradeEvent(DateTimeHelper.CurrentTimeMillis + 10, 1000, 50, 1));
            Assert.AreEqual(2, ((object[]) listener.AssertOneGetNew().Get("longItems")).Length);
            Assert.AreEqual(2, ((object[]) listener.AssertOneGetNew().Get("shortItems")).Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOrderOfEvaluationSubselectFirst(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string epl = "select * from SupportBean(IntPrimitive<10) where IntPrimitive not in (select IntPrimitive from SupportBean#unique(IntPrimitive))";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(epl);
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            stmtOne.Dispose();
    
            string eplTwo = "select * from SupportBean where IntPrimitive not in (select IntPrimitive from SupportBean(IntPrimitive<10)#unique(IntPrimitive))";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            stmtTwo.Dispose();
        }
    
        public class TradeEvent {
            private long time;
            private int securityID;
            private double price;
            private long volume;
    
            public TradeEvent(long time, int securityID, double price, long volume) {
                this.time = time;
                this.securityID = securityID;
                this.price = price;
                this.volume = volume;
            }
    
            public int GetSecurityID() {
                return securityID;
            }
    
            public long GetTime() {
                return time;
            }
    
            public double GetPrice() {
                return price;
            }
    
            public long GetVolume() {
                return volume;
            }
        }
    }
} // end of namespace
