///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.context;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextPartitioned : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionPatternFilter(epService);
            RunAssertionMatchRecognize(epService);
            RunAssertionJoinRemoveStream(epService);
            RunAssertionIterateTargetedCP(epService);
            RunAssertionLargeNumberContexts(epService);
            RunAssertionAdditionalFilters(epService);
            RunAssertionMultiStatementFilterCount(epService);
            RunAssertionSegmentedSubtype(epService);
            RunAssertionSegmentedJoinMultitypeMultifield(epService);
            RunAssertionSegmentedSubselectPrevPrior(epService);
            RunAssertionSegmentedPrior(epService);
            RunAssertionSegmentedSubqueryFiltered(epService);
            RunAssertionSegmentedSubqueryNamedWindowIndexShared(epService);
            RunAssertionSegmentedSubqueryNamedWindowIndexUnShared(epService);
            RunAssertionSegmentedJoin(epService);
            RunAssertionSegmentedPattern(epService);
            RunAssertionSegmentedViews(epService);
            RunAssertionJoinWhereClauseOnPartitionKey(epService);
            RunAssertionNullSingleKey(epService);
            RunAssertionNullKeyMultiKey(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionPatternFilter(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("stringContainsX", GetType().FullName, "stringContainsX");
            string eplContext = "create context IndividualBean partition by theString from SupportBean";
            epService.EPAdministrator.CreateEPL(eplContext);
    
            string eplAnalysis = "context IndividualBean " +
                    "select * from pattern [every (event1=SupportBean(StringContainsX(theString) = false) -> event2=SupportBean(StringContainsX(theString) = true))]";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(eplAnalysis).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("F1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("F1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("X1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("X1", 0));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMatchRecognize(EPServiceProvider epService) {
    
            string eplContextOne = "create context SegmentedByString partition by theString from SupportBean";
            epService.EPAdministrator.CreateEPL(eplContextOne);
    
            string eplMatchRecog = "context SegmentedByString " +
                    "select * from SupportBean\n" +
                    "match_recognize ( \n" +
                    "  measures A.longPrimitive as a, B.longPrimitive as b\n" +
                    "  pattern (A B) \n" +
                    "  define " +
                    "    A as A.intPrimitive = 1," +
                    "    B as B.intPrimitive = 2\n" +
                    ")";
            EPStatement stmtMatchRecog = epService.EPAdministrator.CreateEPL(eplMatchRecog);
            var listener = new SupportUpdateListener();
            stmtMatchRecog.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("A", 1, 10));
            epService.EPRuntime.SendEvent(MakeEvent("B", 1, 30));
    
            epService.EPRuntime.SendEvent(MakeEvent("A", 2, 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b".Split(','), new object[]{10L, 20L});
    
            epService.EPRuntime.SendEvent(MakeEvent("B", 2, 40));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b".Split(','), new object[]{30L, 40L});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // try with "prev"
            string eplContextTwo = "create context SegmentedByString partition by theString from SupportBean";
            epService.EPAdministrator.CreateEPL(eplContextTwo);
    
            string eplMatchRecogWithPrev = "context SegmentedByString select * from SupportBean " +
                    "match_recognize ( " +
                    "  measures A.longPrimitive as e1, B.longPrimitive as e2" +
                    "  pattern (A B) " +
                    "  define A as A.intPrimitive >= Prev(A.intPrimitive),B as B.intPrimitive >= Prev(B.intPrimitive) " +
                    ")";
            EPStatement stmtMatchRecogWithPrev = epService.EPAdministrator.CreateEPL(eplMatchRecogWithPrev);
            stmtMatchRecogWithPrev.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("A", 1, 101));
            epService.EPRuntime.SendEvent(MakeEvent("B", 1, 201));
            epService.EPRuntime.SendEvent(MakeEvent("A", 2, 102));
            epService.EPRuntime.SendEvent(MakeEvent("B", 2, 202));
            epService.EPRuntime.SendEvent(MakeEvent("A", 3, 103));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "e1,e2".Split(','), new object[]{102L, 103L});
    
            epService.EPRuntime.SendEvent(MakeEvent("B", 3, 203));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "e1,e2".Split(','), new object[]{202L, 203L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoinRemoveStream(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.Configuration.AddEventType(typeof(WebEvent));
    
            string stmtContext = "create context SegmentedBySession partition by sessionId from WebEvent";
            epService.EPAdministrator.CreateEPL(stmtContext);
    
            string epl = " context SegmentedBySession " +
                    " select rstream A.pageName as pageNameA , A.sessionId as sessionIdA, B.pageName as pageNameB, C.pageName as pageNameC from " +
                    "WebEvent(pageName='Start')#Time(30) A " +
                    "full outer join " +
                    "WebEvent(pageName='Middle')#Time(30) B on A.sessionId = B.sessionId " +
                    "full outer join " +
                    "WebEvent(pageName='End')#Time(30) C on A.sessionId  = C.sessionId " +
                    "where A.pageName is not null and (B.pageName is null or C.pageName is null) ";
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // Set up statement for finding missing events
            SendWebEventsComplete(epService, 0);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(20000));
            SendWebEventsComplete(epService, 1);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(40000));
            Assert.IsFalse(listener.IsInvoked);
            SendWebEventsComplete(epService, 2);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(60000));
            SendWebEventsIncomplete(epService, 3);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(80000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(100000));
            Assert.IsTrue(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionIterateTargetedCP(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context PartitionedByString partition by theString from SupportBean");
            string[] fields = "c0,c1".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('StmtOne') context PartitionedByString select context.key1 as c0, sum(intPrimitive) as c1 from SupportBean#length(5)");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
    
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new[] {new object[] {"E1", 10}, new object[] {"E2", 41}});
    
            // test iterator targeted
            var selector = new SupportSelectorPartitioned(Collections.SingletonList(new object[]{"E2"}));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new[] {new object[] {"E2", 41}});
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorPartitioned((List<object[]>) null)).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorPartitioned(Collections.SingletonList(new object[]{"EX"}))).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorPartitioned(Collections.GetEmptyList<object[]>())).MoveNext());
    
            // test iterator filtered
            var filtered = new MySelectorFilteredPartitioned(new object[]{"E2"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new[] {new object[] {"E2", 41}});
    
            // test always-false filter - compare context partition info
            var filteredFalse = new MySelectorFilteredPartitioned(null);
            Assert.IsFalse(stmt.GetEnumerator(filteredFalse).MoveNext());
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{new object[]{"E1"}, new object[]{"E2"}}, filteredFalse.Contexts.ToArray());
    
            try {
                stmt.GetEnumerator(new ProxyContextPartitionSelectorCategory() {
                    ProcLabels = () => null
                });
                Assert.Fail();
            } catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorSegmented] interfaces but received com."),
                    "message: " + ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            // invalid filter spec
            epl = "create context SegmentedByAString partition by string from SupportBean(dummy = 1)";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");
    
            // property not found
            epl = "create context SegmentedByAString partition by dummy from SupportBean";
            TryInvalid(epService, epl, "Error starting statement: For context 'SegmentedByAString' property name 'dummy' not found on type SupportBean [");
    
            // mismatch number pf properties
            epl = "create context SegmentedByAString partition by theString from SupportBean, id, p00 from SupportBean_S0";
            TryInvalid(epService, epl, "Error starting statement: For context 'SegmentedByAString' expected the same number of property names for each event type, found 1 properties for event type 'SupportBean' and 2 properties for event type 'SupportBean_S0' [create context SegmentedByAString partition by theString from SupportBean, id, p00 from SupportBean_S0]");
    
            // incompatible property types
            epl = "create context SegmentedByAString partition by theString from SupportBean, id from SupportBean_S0";
            TryInvalid(epService, epl, "Error starting statement: For context 'SegmentedByAString' for context 'SegmentedByAString' found mismatch of property types, property 'theString' of type 'java.lang.string' compared to property 'id' of type 'java.lang.int?' [");
    
            // duplicate type specification
            epl = "create context SegmentedByAString partition by theString from SupportBean, theString from SupportBean";
            TryInvalid(epService, epl, "Error starting statement: For context 'SegmentedByAString' the event type 'SupportBean' is listed twice [");
    
            // duplicate type: subtype
            epService.EPAdministrator.Configuration.AddEventType(typeof(ISupportBaseAB));
            epService.EPAdministrator.Configuration.AddEventType(typeof(ISupportA));
            epl = "create context SegmentedByAString partition by baseAB from ISupportBaseAB, a from ISupportA";
            TryInvalid(epService, epl, "Error starting statement: For context 'SegmentedByAString' the event type 'ISupportA' is listed twice: Event type 'ISupportA' is a subtype or supertype of event type 'ISupportBaseAB' [");
    
            // validate statement not applicable filters
            epService.EPAdministrator.CreateEPL("create context SegmentedByAString partition by theString from SupportBean");
            epl = "context SegmentedByAString select * from SupportBean_S0";
            TryInvalid(epService, epl, "Error starting statement: Segmented context 'SegmentedByAString' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");
    
            // invalid attempt to partition a named window's streams
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epl = "create context SegmentedByWhat partition by theString from MyWindow";
            TryInvalid(epService, epl, "Error starting statement: Partition criteria may not include named windows [create context SegmentedByWhat partition by theString from MyWindow]");
    
            // partitioned with named window
            epService.EPAdministrator.CreateEPL("create schema SomeSchema(ipAddress string)");
            epService.EPAdministrator.CreateEPL("create context TheSomeSchemaCtx Partition By ipAddress From SomeSchema");
            epl = "context TheSomeSchemaCtx create window MyEvent#Time(30 sec) (ipAddress string)";
            TryInvalid(epService, epl, "Error starting statement: Segmented context 'TheSomeSchemaCtx' requires that named windows are associated to an existing event type and that the event type is listed among the partitions defined by the create-context statement");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLargeNumberContexts(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByAString  partition by theString from SupportBean");
    
            string[] fields = "col1".Split(',');
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("context SegmentedByAString " +
                    "select sum(intPrimitive) as col1," +
                    "Prev(1, intPrimitive)," +
                    "Prior(1, intPrimitive)," +
                    "(select id from SupportBean_S0#lastevent)" +
                    "  from SupportBean#keepall");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{i});
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAdditionalFilters(EPServiceProvider epService) {
            FilterServiceSPI filterSPI = (FilterServiceSPI) ((EPServiceProviderSPI) epService).FilterService;
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByAString " +
                    "partition by theString from SupportBean(intPrimitive>0), p00 from SupportBean_S0(id > 0)");
    
            // first send a view events
            epService.EPRuntime.SendEvent(new SupportBean("B1", -1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(-2, "S0"));
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            string[] fields = "col1,col2".Split(',');
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("context SegmentedByAString " +
                    "select sum(sb.intPrimitive) as col1, sum(s0.id) as col2 " +
                    "from pattern [every (s0=SupportBean_S0 or sb=SupportBean)]");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-3, "S0"));
            epService.EPRuntime.SendEvent(new SupportBean("S0", -1));
            epService.EPRuntime.SendEvent(new SupportBean("S1", -2));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, 2});
    
            epService.EPRuntime.SendEvent(new SupportBean("S1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-2, "S0"));
            epService.EPRuntime.SendEvent(new SupportBean("S1", -10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 3});
    
            epService.EPRuntime.SendEvent(new SupportBean("S0", 9));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{9, 2});
    
            epService.EPAdministrator.DestroyAllStatements();
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            // Test unnecessary filter
            string epl = "create context CtxSegmented partition by theString from SupportBean;" +
                    "context CtxSegmented select * from pattern [every a=SupportBean -> c=SupportBean(c.theString=a.theString)];";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMultiStatementFilterCount(EPServiceProvider epService) {
            FilterServiceSPI filterSPI = (FilterServiceSPI) ((EPServiceProviderSPI) epService).FilterService;
            EPStatement stmtContext = epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByAString " +
                    "partition by theString from SupportBean, p00 from SupportBean_S0");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            // first send a view events
            epService.EPRuntime.SendEvent(new SupportBean("B1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0"));
    
            var fields = new[]{"col1"};
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("context SegmentedByAString select sum(id) as col1 from SupportBean_S0");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10});
    
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(8, "S1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{8});
    
            Assert.AreEqual(4, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "S0"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{14});
    
            Assert.AreEqual(4, filterSPI.FilterCountApprox);
    
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("context SegmentedByAString select sum(intPrimitive) as col1 from SupportBean");
            stmtTwo.Events += listener.Update;
    
            Assert.AreEqual(6, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("S0", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5});
    
            Assert.AreEqual(6, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("S2", 6));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{6});
    
            Assert.AreEqual(8, filterSPI.FilterCountApprox);
    
            stmtOne.Dispose();
            Assert.AreEqual(5, filterSPI.FilterCountApprox);  // 5 = 3 from context instances and 2 from context itself
    
            stmtTwo.Dispose();
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            stmtContext.Dispose();
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedSubtype(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("ISupportBaseAB", typeof(ISupportBaseAB));
            epService.EPAdministrator.Configuration.AddEventType("ISupportA", typeof(ISupportA));
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by baseAB from ISupportBaseAB");
    
            string[] fields = "col1".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context SegmentedByString select count(*) as col1 from ISupportA");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new ISupportAImpl("A1", "AB1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
    
            epService.EPRuntime.SendEvent(new ISupportAImpl("A2", "AB1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L});
    
            epService.EPRuntime.SendEvent(new ISupportAImpl("A3", "AB2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
    
            epService.EPRuntime.SendEvent(new ISupportAImpl("A4", "AB1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedJoinMultitypeMultifield(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedBy2Fields " +
                    "partition by theString and intPrimitive from SupportBean, p00 and id from SupportBean_S0");
    
            string[] fields = "c1,c2,c3,c4,c5,c6".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context SegmentedBy2Fields " +
                    "select theString as c1, intPrimitive as c2, id as c3, p00 as c4, context.key1 as c5, context.key2 as c6 " +
                    "from SupportBean#lastevent, SupportBean_S0#lastevent");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "G1"));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "G2"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 1, 1, "G2", "G2", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "G2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 2, 2, "G2", "G2", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "G1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 1, 1, "G1", "G1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 2, 2, "G1", "G1", 2});
    
            // ESPER-663
            epService.EPAdministrator.Configuration.AddEventType("Event", typeof(Event));
            string epl =
                    "@Audit @Name('CTX') create context Ctx partition by grp, subGrp from Event;\n" +
                            "@Audit @Name('Window') context Ctx create window EventData#unique(type) as Event;" +
                            "@Audit @Name('Insert') context Ctx insert into EventData select * from Event;" +
                            "@Audit @Name('Test') context Ctx select irstream * from EventData;";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            epService.EPAdministrator.GetStatement("Test").Events += listener.Update;
            epService.EPRuntime.SendEvent(new Event("G1", "SG1", 1, 10.45));
            Assert.IsTrue(listener.IsInvoked);
            epService.EPAdministrator.DestroyAllStatements();
    
            // Esper-695
            string eplTwo =
                    "create context Ctx partition by theString from SupportBean;\n" +
                            "context Ctx create window MyWindow#unique(intPrimitive) as SupportBean;" +
                            "context Ctx select irstream * from pattern [MyWindow];";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplTwo);
            TryInvalidCreateWindow(epService);
            TryInvalidCreateWindow(epService); // making sure all is cleaned up
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryInvalidCreateWindow(EPServiceProvider epService) {
            try {
                epService.EPAdministrator.CreateEPL("context Ctx create window MyInvalidWindow#unique(p00) as SupportBean_S0");
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Error starting statement: Segmented context 'Ctx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [context Ctx create window MyInvalidWindow#unique(p00) as SupportBean_S0]", ex.Message);
            }
        }
    
        private void RunAssertionSegmentedSubselectPrevPrior(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by theString from SupportBean");
    
            var fieldsPrev = new[]{"theString", "col1"};
            EPStatement stmtPrev = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select theString, (select Prev(0, id) from SupportBean_S0#keepall) as col1 from SupportBean");
            var listener = new SupportUpdateListener();
            stmtPrev.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrev, new object[]{"G1", null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrev, new object[]{"G1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrev, new object[]{"G2", null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrev, new object[]{"G2", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrev, new object[]{"G1", null});  // since returning multiple rows
    
            stmtPrev.Stop();
    
            var fieldsPrior = new[]{"theString", "col1"};
            EPStatement stmtPrior = epService.EPAdministrator.CreateEPL("@Name('B') context SegmentedByString " +
                    "select theString, (select Prior(0, id) from SupportBean_S0#keepall) as col1 from SupportBean");
            stmtPrior.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrior, new object[]{"G1", null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrior, new object[]{"G1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrior, new object[]{"G2", null});    // since category started as soon as statement added
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrior, new object[]{"G2", 2}); // since returning multiple rows
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsPrior, new object[]{"G1", null});  // since returning multiple rows
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedPrior(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by theString from SupportBean");
    
            var fields = new[]{"val0", "val1"};
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select intPrimitive as val0, Prior(1, intPrimitive) as val1 from SupportBean");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{11, 10});
    
            stmtOne.Stop();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedSubqueryFiltered(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by theString from SupportBean");
    
            var fields = new[]{"theString", "intPrimitive", "val0"};
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select theString, intPrimitive, (select p00 from SupportBean_S0#lastevent as s0 where sb.intPrimitive = s0.id) as val0 " +
                    "from SupportBean as sb");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s1"));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s2"));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10, "s2"});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 10, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s3"));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 10, "s3"});
    
            epService.EPRuntime.SendEvent(new SupportBean("G3", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G3", 10, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10, "s3"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedSubqueryNamedWindowIndexShared(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by theString from SupportBean");
            epService.EPAdministrator.CreateEPL("@Hint('enable_window_subquery_indexshare') create window MyWindowTwo#keepall as SupportBean_S0");
            epService.EPAdministrator.CreateEPL("insert into MyWindowTwo select * from SupportBean_S0");
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select theString, intPrimitive, (select p00 from MyWindowTwo as s0 where sb.intPrimitive = s0.id) as val0 " +
                    "from SupportBean as sb");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            TryAssertionSubqueryNW(epService, listener);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedSubqueryNamedWindowIndexUnShared(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by theString from SupportBean");
            epService.EPAdministrator.CreateEPL("create window MyWindowThree#keepall as SupportBean_S0");
            epService.EPAdministrator.CreateEPL("insert into MyWindowThree select * from SupportBean_S0");
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select theString, intPrimitive, (select p00 from MyWindowThree as s0 where sb.intPrimitive = s0.id) as val0 " +
                    "from SupportBean as sb");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            TryAssertionSubqueryNW(epService, listener);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionSubqueryNW(EPServiceProvider epService, SupportUpdateListener listener) {
            var fields = new[]{"theString", "intPrimitive", "val0"};
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s1"));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10, "s1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 10, "s1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("G3", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G3", 20, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "s2"));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G3", 20, "s2"});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 20, "s2"});
        }
    
        private void RunAssertionSegmentedJoin(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by theString from SupportBean");
    
            var fields = new[]{"sb.theString", "sb.intPrimitive", "s0.id"};
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select * from SupportBean#keepall as sb, SupportBean_S0#keepall as s0 " +
                    "where intPrimitive = id");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 20, 20});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(30));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 30));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 30));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 30, 30});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 30));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 30, 30});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedPattern(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by theString from SupportBean");
    
            var fields = new[]{"a.theString", "a.intPrimitive", "b.theString", "b.intPrimitive"};
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean -> b=SupportBean(intPrimitive=a.intPrimitive+1)]");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 20));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 20, "G2", 21});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10, "G1", 11});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 22));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 21, "G2", 22});
    
            stmtOne.Dispose();
    
            // add another statement: contexts already exist, this one uses @Consume
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean -> b=SupportBean(intPrimitive=a.intPrimitive+1)@Consume]");
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 20));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 20, "G2", 21});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10, "G1", 11});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 22));
            Assert.IsFalse(listener.IsInvoked);
    
            stmtTwo.Dispose();
    
            // test truly segmented consume
            var fieldsThree = new[]{"a.theString", "a.intPrimitive", "b.id", "b.p00"};
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean -> b=SupportBean_S0(id=a.intPrimitive)@Consume]");
            stmtThree.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));   // should be 2 output rows
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.LastNewData, fieldsThree, new[] {new object[] {"G1", 10, 10, "E1"}, new object[] {"G2", 10, 10, "E1"}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedViews(EPServiceProvider epService) {
            string contextEPL = "@Name('context') create context SegmentedByString as partition by theString from SupportBean";
            epService.EPAdministrator.CreateEPL(contextEPL);
    
            string[] fieldsIterate = "intPrimitive".Split(',');
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select irstream intPrimitive, Prevwindow(items) as pw from SupportBean#length(2) as items");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            AssertViewData(listener, 10, new[] {new object[] {"G1", 10}}, null);
            EPAssertionUtil.AssertPropsPerRow(stmtOne.GetEnumerator(), stmtOne.GetSafeEnumerator(), fieldsIterate, new[] {new object[] {10}});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            AssertViewData(listener, 20, new[] {new object[] {"G2", 20}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            AssertViewData(listener, 11, new[] {new object[] {"G1", 11}, new object[] {"G1", 10}}, null);
            EPAssertionUtil.AssertPropsPerRow(stmtOne.GetEnumerator(), stmtOne.GetSafeEnumerator(), fieldsIterate, new[] {new object[] {10}, new object[] {11}, new object[] {20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            AssertViewData(listener, 21, new[] {new object[] {"G2", 21}, new object[] {"G2", 20}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 12));
            AssertViewData(listener, 12, new[] {new object[] {"G1", 12}, new object[] {"G1", 11}}, 10);
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 22));
            AssertViewData(listener, 22, new[] {new object[] {"G2", 22}, new object[] {"G2", 21}}, 20);
    
            stmtOne.Dispose();
    
            // test SODA
            epService.EPAdministrator.DestroyAllStatements();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(contextEPL);
            Assert.AreEqual(contextEPL, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(contextEPL, stmt.Text);
    
            // test built-in properties
            string[] fields = "c1,c2,c3,c4".Split(',');
            string ctx = "SegmentedByString";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select context.name as c1, context.id as c2, context.key1 as c3, theString as c4 " +
                    "from SupportBean#length(2) as items");
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, 0, "G1", "G1"});
            epService.EPAdministrator.DestroyAllStatements();
    
            // test grouped delivery
            epService.EPAdministrator.CreateEPL("create variable bool trigger = false");
            epService.EPAdministrator.CreateEPL("create context MyCtx partition by theString from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('Out') context MyCtx select * from SupportBean#Expr(not trigger) for Grouped_delivery(theString)");
            epService.EPAdministrator.GetStatement("Out").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SetVariableValue("trigger", true);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(100));
    
            Assert.AreEqual(2, listener.NewDataList.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoinWhereClauseOnPartitionKey(EPServiceProvider epService) {
            string epl = "create context MyCtx partition by theString from SupportBean;\n" +
                    "@Name('select') context MyCtx select * from SupportBean#lastevent as sb, SupportBean_S0#lastevent as s0 " +
                    "where theString is 'Test'";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("select").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("Test", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.IsTrue(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNullSingleKey(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyContext partition by theString from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context MyContext select count(*) as cnt from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 10));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 20));
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 30));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNullKeyMultiKey(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyContext partition by theString, intBoxed, intPrimitive from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context MyContext select count(*) as cnt from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSBEvent(epService, "A", null, 1);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            SendSBEvent(epService, "A", null, 1);
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            SendSBEvent(epService, "A", 10, 1);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertViewData(SupportUpdateListener listener, int newIntExpected, object[][] newArrayExpected, int? oldIntExpected) {
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(newIntExpected, listener.LastNewData[0].Get("intPrimitive"));
            SupportBean[] beans = (SupportBean[]) listener.LastNewData[0].Get("pw");
            Assert.AreEqual(newArrayExpected.Length, beans.Length);
            for (int i = 0; i < beans.Length; i++) {
                Assert.AreEqual(newArrayExpected[i][0], beans[i].TheString);
                Assert.AreEqual(newArrayExpected[i][1], beans[i].IntPrimitive);
            }
    
            if (oldIntExpected != null) {
                Assert.AreEqual(1, listener.LastOldData.Length);
                Assert.AreEqual(oldIntExpected, listener.LastOldData[0].Get("intPrimitive"));
            } else {
                Assert.IsNull(listener.LastOldData);
            }
            listener.Reset();
        }
    
        internal class MySelectorFilteredPartitioned : ContextPartitionSelectorFiltered
        {
            private object[] _match;
    
            private IList<object[]> _contexts = new List<object[]>();
            private ISet<int?> _cpids = new LinkedHashSet<int?>();

            internal MySelectorFilteredPartitioned(object[] match) {
                _match = match;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                ContextPartitionIdentifierPartitioned id = (ContextPartitionIdentifierPartitioned) contextPartitionIdentifier;
                if (_match == null && _cpids.Contains(id.ContextPartitionId)) {
                    throw new EPRuntimeException("Already Exists context id: " + id.ContextPartitionId);
                }
                _cpids.Add(id.ContextPartitionId);
                _contexts.Add(id.Keys);
                return Collections.AreEqual(id.Keys, _match);
            }

            public IList<object[]> Contexts => _contexts;
        }
    
        [Serializable]
        public class Event  {
            private string _grp;
            private string _subGrp;
            private int _type;
            private double _value;
    
            public Event() {
            }
    
            public Event(string group, string subGroup, int type, double value) {
                _grp = group;
                _subGrp = subGroup;
                _type = type;
                _value = value;
            }

            public string Grp
            {
                get => _grp;
                set => _grp = value;
            }

            public string SubGrp
            {
                get => _subGrp;
                set => _subGrp = value;
            }

            public int Type
            {
                get => _type;
                set => _type = value;
            }

            public double Value
            {
                get => _value;
                set => _value = value;
            }

            public override bool Equals(object obj) {
                if (this == obj) {
                    return true;
                }
                if (obj is Event evt) {
                    return Grp.Equals(evt._grp) && 
                           _subGrp.Equals(evt._subGrp) && 
                           _type == evt._type && 
                           Math.Abs(_value - evt._value) < 1e-6;
                }
    
                return false;
            }
    
            public override string ToString() {
                return "(" + _grp + ", " + _subGrp + ")@" + _type + "=" + _value;
            }
        }
    
        private void SendWebEventsIncomplete(EPServiceProvider epService, int id) {
            epService.EPRuntime.SendEvent(new WebEvent("Start", Convert.ToString(id)));
            epService.EPRuntime.SendEvent(new WebEvent("End", Convert.ToString(id)));
        }
    
        private void SendWebEventsComplete(EPServiceProvider epService, int id) {
            epService.EPRuntime.SendEvent(new WebEvent("Start", Convert.ToString(id)));
            epService.EPRuntime.SendEvent(new WebEvent("Middle", Convert.ToString(id)));
            epService.EPRuntime.SendEvent(new WebEvent("End", Convert.ToString(id)));
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        public static bool StringContainsX(string theString) {
            return theString.Contains("X");
        }
    
        private static void SendSBEvent(
            EPServiceProvider engine,
            string @string, 
            int? intBoxed,
            int intPrimitive)
        {
            var bean = new SupportBean(@string, intPrimitive);
            bean.IntBoxed = intBoxed;
            engine.EPRuntime.SendEvent(bean);
        }
    
        [Serializable]
        public class WebEvent
        {
            private readonly string _pageName;
            private readonly string _sessionId;
    
            public WebEvent(string pageName, string sessionId) {
                _pageName = pageName;
                _sessionId = sessionId;
            }

            public string PageName => _pageName;

            public string SessionId => _sessionId;
        }
    }
} // end of namespace
