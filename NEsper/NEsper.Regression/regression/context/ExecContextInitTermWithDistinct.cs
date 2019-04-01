///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextInitTermWithDistinct : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionNullSingleKey(epService);
            RunAssertionNullKeyMultiKey(epService);
            RunAssertionDistinctOverlappingSingleKey(epService);
            RunAssertionDistinctOverlappingMultiKey(epService);
            RunAssertionNullSingleKey(epService);
            RunAssertionNullKeyMultiKey(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            // require stream name assignment using 'as'
            TryInvalid(epService, "create context MyContext initiated by distinct(TheString) SupportBean terminated after 15 seconds",
                    "Error starting statement: Distinct-expressions require that a stream name is assigned to the stream using 'as' [create context MyContext initiated by distinct(TheString) SupportBean terminated after 15 seconds]");
    
            // require stream
            TryInvalid(epService, "create context MyContext initiated by distinct(a.TheString) pattern [a=SupportBean] terminated after 15 seconds",
                    "Error starting statement: Distinct-expressions require a stream as the initiated-by condition [create context MyContext initiated by distinct(a.TheString) pattern [a=SupportBean] terminated after 15 seconds]");
    
            // invalid distinct-clause expression
            TryInvalid(epService, "create context MyContext initiated by distinct((select * from MyWindow)) SupportBean as sb terminated after 15 seconds",
                    "Error starting statement: Invalid context distinct-clause expression 'subselect_0': Aggregation, sub-select, previous or prior functions are not supported in this context [create context MyContext initiated by distinct((select * from MyWindow)) SupportBean as sb terminated after 15 seconds]");
    
            // empty list of expressions
            TryInvalid(epService, "create context MyContext initiated by distinct() SupportBean terminated after 15 seconds",
                    "Error starting statement: Distinct-expressions have not been provided [create context MyContext initiated by distinct() SupportBean terminated after 15 seconds]");
    
            // non-overlapping context not allowed with distinct
            TryInvalid(epService, "create context MyContext start distinct(TheString) SupportBean end after 15 seconds",
                    "Incorrect syntax near 'distinct' (a reserved keyword) at line 1 column 31 [create context MyContext start distinct(TheString) SupportBean end after 15 seconds]");
        }
    
        private void RunAssertionDistinctOverlappingSingleKey(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL(
                    "create context MyContext " +
                            "  initiated by distinct(s0.TheString) SupportBean(IntPrimitive = 0) s0" +
                            "  terminated by SupportBean(TheString = s0.TheString and IntPrimitive = 1)");
    
            string[] fields = "TheString,LongPrimitive,cnt".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "context MyContext " +
                            "select TheString, LongPrimitive, count(*) as cnt from SupportBean(TheString = context.s0.TheString)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "A", -1, 10);
            SendEvent(epService, "A", 1, 11);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "A", 0, 12);   // allocate context
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 12L, 1L});
    
            SendEvent(epService, "A", 0, 13);   // counts towards the existing context, not having a new one
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 13L, 2L});
    
            SendEvent(epService, "A", -1, 14);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 14L, 3L});
    
            SendEvent(epService, "A", 1, 15);   // context termination
            SendEvent(epService, "A", -1, 16);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "A", 0, 17);   // allocate context
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 17L, 1L});
    
            SendEvent(epService, "A", -1, 18);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 18L, 2L});
    
            SendEvent(epService, "B", 0, 19);   // allocate context
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 19L, 1L});
    
            SendEvent(epService, "B", -1, 20);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 20L, 2L});
    
            SendEvent(epService, "A", 1, 21);   // context termination
            SendEvent(epService, "B", 1, 22);   // context termination
            SendEvent(epService, "A", -1, 23);
            SendEvent(epService, "B", -1, 24);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "A", 0, 25);   // allocate context
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 25L, 1L});
    
            SendEvent(epService, "B", 0, 26);   // allocate context
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 26L, 1L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDistinctOverlappingMultiKey(EPServiceProvider epService) {
            string epl = "create context MyContext as " +
                    "initiated by distinct(TheString, IntPrimitive) SupportBean as sb " +
                    "terminated SupportBean_S1";         // any S1 ends the contexts
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            EPStatement stmtContext = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtContext.Text, model.ToEPL());
    
            string[] fields = "id,p00,p01,cnt".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "context MyContext " +
                            "select id, p00, p01, count(*) as cnt " +
                            "from SupportBean_S0(id = context.sb.IntPrimitive and p00 = context.sb.TheString)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "A", "E1", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "A", "E2", 2L});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1)); // terminate all
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "E3"));
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "A", "E4", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "B", "E5", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B", "E6"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "B", "E6", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E7"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "B", "E7", 2L});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1)); // terminate all
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E8"));
            epService.EPRuntime.SendEvent(new SupportBean("B", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E9"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "B", "E9", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E10"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "B", "E10", 2L});
    
            // destroy context partition, should forget about the distinct key
            if (GetSpi(epService).IsSupportsExtract) {
                GetSpi(epService).DestroyContextPartitions("MyContext", new ContextPartitionSelectorAll());
                epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E11"));
                epService.EPRuntime.SendEvent(new SupportBean("B", 2));
                Assert.IsFalse(listener.IsInvoked);
    
                epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E12"));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "B", "E12", 1L});
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNullSingleKey(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyContext initiated by distinct(TheString) SupportBean as sb terminated after 24 hours");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context MyContext select count(*) as cnt from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 10));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 20));
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 30));
            Assert.AreEqual(2, listener.GetAndResetLastNewData().Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNullKeyMultiKey(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyContext initiated by distinct(TheString, IntBoxed, IntPrimitive) SupportBean as sb terminated after 100 hours");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context MyContext select count(*) as cnt from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSBEvent(epService, "A", null, 1);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            SendSBEvent(epService, "A", null, 1);
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            SendSBEvent(epService, "A", 10, 1);
            Assert.AreEqual(2, listener.GetAndResetLastNewData().Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private static void SendEvent(EPServiceProvider engine, string theString, int intPrimitive, long longPrimitive) {
            SupportBean @event = new SupportBean(theString, intPrimitive);
            @event.LongPrimitive = longPrimitive;
            engine.EPRuntime.SendEvent(@event);
        }
    
        private static void SendSBEvent(EPServiceProvider engine, string @string, int? intBoxed, int intPrimitive) {
            var bean = new SupportBean(@string, intPrimitive);
            bean.IntBoxed = intBoxed;
            engine.EPRuntime.SendEvent(bean);
        }
    
        private static EPContextPartitionAdminSPI GetSpi(EPServiceProvider epService) {
            return (EPContextPartitionAdminSPI) epService.EPAdministrator.ContextPartitionAdmin;
        }
    }
} // end of namespace
