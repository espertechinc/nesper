///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    public class ExecInsertIntoTransposePattern : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionThisAsColumn(epService);
            RunAssertionTransposePOJOEventPattern(epService);
            RunAssertionTransposeMapEventPattern(epService);
        }
    
        private void RunAssertionThisAsColumn(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window OneWindow#time(1 day) as select TheString as alertId, this from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into OneWindow select '1' as alertId, stream0.quote.this as this " +
                    " from pattern [every quote=SupportBean(TheString='A')] as stream0");
            epService.EPAdministrator.CreateEPL("insert into OneWindow select '2' as alertId, stream0.quote as this " +
                    " from pattern [every quote=SupportBean(TheString='B')] as stream0");
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 10));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"alertId", "this.IntPrimitive"}, new object[][]{new object[] {"1", 10}});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"alertId", "this.IntPrimitive"}, new object[][]{new object[] {"1", 10}, new object[] {"2", 20}});
    
            stmt = epService.EPAdministrator.CreateEPL("create window TwoWindow#time(1 day) as select TheString as alertId, * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into TwoWindow select '3' as alertId, quote.* " +
                    " from pattern [every quote=SupportBean(TheString='C')] as stream0");
    
            epService.EPRuntime.SendEvent(new SupportBean("C", 30));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"alertId", "IntPrimitive"}, new object[][]{new object[] {"3", 30}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionTransposePOJOEventPattern(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("AEventBean", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("BEventBean", typeof(SupportBean_B));
    
            string stmtTextOne = "insert into MyStreamABBean select a, b from pattern [a=AEventBean -> b=BEventBean]";
            epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            string stmtTextTwo = "select a.id, b.id from MyStreamABBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.id,b.id".Split(','), new object[]{"A1", "B1"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionTransposeMapEventPattern(EPServiceProvider epService) {
            IDictionary<string, Object> type = MakeMap(new object[][]{new object[] {"id", typeof(string)}});
    
            epService.EPAdministrator.Configuration.AddEventType("AEventMap", type);
            epService.EPAdministrator.Configuration.AddEventType("BEventMap", type);
    
            string stmtTextOne = "insert into MyStreamABMap select a, b from pattern [a=AEventMap -> b=BEventMap]";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listenerInsertInto = new SupportUpdateListener();
            stmtOne.Events += listenerInsertInto.Update;
            Assert.AreEqual(typeof(IDictionary<string, object>), stmtOne.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(IDictionary<string, object>), stmtOne.EventType.GetPropertyType("b"));
    
            string stmtTextTwo = "select a.id, b.id from MyStreamABMap";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmtTwo.Events += listener.Update;
            Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("a.id"));
            Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("b.id"));
    
            IDictionary<string, Object> eventOne = MakeMap(new object[][]{new object[] {"id", "A1"}});
            IDictionary<string, Object> eventTwo = MakeMap(new object[][]{new object[] {"id", "B1"}});
    
            epService.EPRuntime.SendEvent(eventOne, "AEventMap");
            epService.EPRuntime.SendEvent(eventTwo, "BEventMap");
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, "a.id,b.id".Split(','), new object[]{"A1", "B1"});
    
            theEvent = listenerInsertInto.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, "a,b".Split(','), new object[]{eventOne, eventTwo});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private IDictionary<string, Object> MakeMap(object[][] entries) {
            var result = new Dictionary<string, Object>();
            foreach (object[] entry in entries) {
                result.Put((string) entry[0], entry[1]);
            }
            return result;
        }
    }
} // end of namespace
