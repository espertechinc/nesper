///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextPartitioned
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private EPServiceProviderSPI _spi;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _spi = (EPServiceProviderSPI) _epService;
    
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestPatternFilter()
        {
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("stringContainsX", GetType().FullName, "StringContainsX");
            String eplContext = "create context IndividualBean partition by TheString from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplContext);

            String eplAnalysis = "context IndividualBean " +
                    "select * from pattern [every (event1=SupportBean(stringContainsX(TheString) = false) -> event2=SupportBean(stringContainsX(TheString) = true))]";
            _epService.EPAdministrator.CreateEPL(eplAnalysis).AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean("F1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("F1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("X1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("X1", 0));
        }

        [Test]
        public void TestMatchRecognize()
        {
            String eplContextOne = "create context SegmentedByString partition by TheString from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplContextOne);

            String eplMatchRecog = "context SegmentedByString " +
                    "select * from SupportBean\n" +
                    "match_recognize ( \n" +
                    "  measures A.LongPrimitive as a, B.LongPrimitive as b\n" +
                    "  pattern (A B) \n" +
                    "  define " +
                    "    A as A.IntPrimitive = 1," +
                    "    B as B.IntPrimitive = 2\n" +
                    ")";
            EPStatement stmtMatchRecog = _epService.EPAdministrator.CreateEPL(eplMatchRecog);
            stmtMatchRecog.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(MakeEvent("A", 1, 10));
            _epService.EPRuntime.SendEvent(MakeEvent("B", 1, 30));

            _epService.EPRuntime.SendEvent(MakeEvent("A", 2, 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a,b".Split(','), new Object[] { 10L, 20L });

            _epService.EPRuntime.SendEvent(MakeEvent("B", 2, 40));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a,b".Split(','), new Object[] { 30L, 40L });

            _epService.EPAdministrator.DestroyAllStatements();

            // try with "prev"
            String eplContextTwo = "create context SegmentedByString partition by TheString from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplContextTwo);

            String eplMatchRecogWithPrev = "context SegmentedByString select * from SupportBean " +
                    "match_recognize ( " +
                    "  measures A.LongPrimitive as e1, B.LongPrimitive as e2" +
                    "  pattern (A B) " +
                    "  define A as A.IntPrimitive >= prev(A.IntPrimitive),B as B.IntPrimitive >= prev(B.IntPrimitive) " +
                    ")";
            EPStatement stmtMatchRecogWithPrev = _epService.EPAdministrator.CreateEPL(eplMatchRecogWithPrev);
            stmtMatchRecogWithPrev.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(MakeEvent("A", 1, 101));
            _epService.EPRuntime.SendEvent(MakeEvent("B", 1, 201));
            _epService.EPRuntime.SendEvent(MakeEvent("A", 2, 102));
            _epService.EPRuntime.SendEvent(MakeEvent("B", 2, 202));
            _epService.EPRuntime.SendEvent(MakeEvent("A", 3, 103));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "e1,e2".Split(','), new Object[] { 102L, 103L });

            _epService.EPRuntime.SendEvent(MakeEvent("B", 3, 203));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "e1,e2".Split(','), new Object[] { 202L, 203L });
        }

        [Test]
        public void TestJoinRemoveStream()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(WebEvent));
    
            var stmtContext = "create context SegmentedBySession partition by sessionId from WebEvent";
            _epService.EPAdministrator.CreateEPL(stmtContext);
    
            var epl = " context SegmentedBySession " +
                    " select rstream A.pageName as pageNameA , A.sessionId as sessionIdA, B.pageName as pageNameB, C.pageName as pageNameC from " +
                    "WebEvent(pageName='Start').win:time(30) A " +
                    "full outer join " +
                    "WebEvent(pageName='Middle').win:time(30) B on A.sessionId = B.sessionId " +
                    "full outer join " +
                    "WebEvent(pageName='End').win:time(30) C on A.sessionId  = C.sessionId " +
                    "where A.pageName is not null and (B.pageName is null or C.pageName is null) "
                    ;
            var statement = _epService.EPAdministrator.CreateEPL(epl);
            statement.Events += _listener.Update;
    
            // Set up statement for finding missing events
            SendWebEventsComplete(0);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(20000));
            SendWebEventsComplete(1);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(40000));
            Assert.IsFalse(_listener.IsInvoked);
            SendWebEventsComplete(2);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(60000));
            SendWebEventsIncomplete(3);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(80000));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(100000));
            Assert.IsTrue(_listener.IsInvoked);
        }
    
        [Test]
        public void TestIterateTargetedCP()
        {
            _epService.EPAdministrator.CreateEPL("create context PartitionedByString partition by TheString from SupportBean");
            var fields = "c0,c1".Split(',');
            var stmt = _epService.EPAdministrator.CreateEPL("@Name('StmtOne') context PartitionedByString select context.key1 as c0, Sum(IntPrimitive) as c1 from SupportBean.win:length(5)");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
    
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new Object[][]{new Object[] {"E1", 10}, new Object[] {"E2", 41}});
    
            // test iterator targeted
            var selector = new SupportSelectorPartitioned(Collections.SingletonList(new Object[]{"E2"}));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new Object[][]{new Object[] {"E2", 41}});
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorPartitioned((IList<object>) null)).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorPartitioned(Collections.SingletonList(new Object[]{"EX"}))).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorPartitioned(Collections.GetEmptyList<Object[]>())).MoveNext());
    
            // test iterator filtered
            var filtered = new MySelectorFilteredPartitioned(new Object[] {"E2"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new Object[][]{new Object[] {"E2", 41}});
    
            // test always-false filter - compare context partition INFO
            var filteredFalse = new MySelectorFilteredPartitioned(null);
            Assert.IsFalse(stmt.GetEnumerator(filteredFalse).MoveNext());
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[]{new Object[]{"E1"}, new Object[]{"E2"}}, filteredFalse.Contexts.ToArray());
            
            try {
                stmt.GetEnumerator(new ProxyContextPartitionSelectorCategory
                {
                    ProcLabels = () => null
                });
                Assert.Fail();
            }
            catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorSegmented] interfaces but received com."), "message: " + ex.Message);
            }
        }
    
        [Test]
        public void TestInvalid()
        {
            String epl;
    
            // invalid filter spec
            epl = "create context SegmentedByAString partition by string from SupportBean(dummy = 1)";
            TryInvalid(epl, "Error starting statement: Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");
    
            // property not found
            epl = "create context SegmentedByAString partition by dummy from SupportBean";
            TryInvalid(epl, "Error starting statement: For context 'SegmentedByAString' property name 'dummy' not found on type SupportBean [");
    
            // mismatch number pf properties
            epl = "create context SegmentedByAString partition by TheString from SupportBean, id, p00 from SupportBean_S0";
            TryInvalid(epl, "Error starting statement: For context 'SegmentedByAString' expected the same number of property names for each event type, found 1 properties for event type 'SupportBean' and 2 properties for event type 'SupportBean_S0' [create context SegmentedByAString partition by TheString from SupportBean, id, p00 from SupportBean_S0]");
    
            // incompatible property types
            epl = "create context SegmentedByAString partition by TheString from SupportBean, id from SupportBean_S0";
            TryInvalid(epl, "Error starting statement: For context 'SegmentedByAString' for context 'SegmentedByAString' found mismatch of property types, property 'TheString' of type 'System.String' compared to property 'id' of type '" + typeof(int?).FullName + "' [");
    
            // duplicate type specification
            epl = "create context SegmentedByAString partition by TheString from SupportBean, TheString from SupportBean";
            TryInvalid(epl, "Error starting statement: For context 'SegmentedByAString' the event type 'SupportBean' is listed twice [");
    
            // duplicate type: subtype
            _epService.EPAdministrator.Configuration.AddEventType(typeof(ISupportBaseAB));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(ISupportA));
            epl = "create context SegmentedByAString partition by baseAB from ISupportBaseAB, a from ISupportA";
            TryInvalid(epl, "Error starting statement: For context 'SegmentedByAString' the event type 'ISupportA' is listed twice: Event type 'ISupportA' is a subtype or supertype of event type 'ISupportBaseAB' [");
    
            // validate statement not applicable filters
            _epService.EPAdministrator.CreateEPL("create context SegmentedByAString partition by TheString from SupportBean");
            epl = "context SegmentedByAString select * from SupportBean_S0";
            TryInvalid(epl, "Error starting statement: Segmented context 'SegmentedByAString' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");

            // invalid attempt to partition a named window's streams
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
            epl = "create context SegmentedByWhat partition by TheString from MyWindow";
            TryInvalid(epl, "Error starting statement: Partition criteria may not include named windows [create context SegmentedByWhat partition by TheString from MyWindow]");

            // partitioned with named window
            _epService.EPAdministrator.CreateEPL("create schema SomeSchema(ipAddress string)");
            _epService.EPAdministrator.CreateEPL("create context TheSomeSchemaCtx Partition By ipAddress From SomeSchema");
            epl = "context TheSomeSchemaCtx create window MyEvent.win:time(30 sec) (ipAddress string)";
            TryInvalid(epl, "Error starting statement: Segmented context 'TheSomeSchemaCtx' requires that named windows are associated to an existing event type and that the event type is listed among the partitions defined by the create-context statement");
        }
    
        private void TryInvalid(String epl, String expected)
        {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                if (!ex.Message.StartsWith(expected)) {
                    throw new Exception("Expected/Received:\n" + expected + "\n" + ex.Message + "\n");
                }
                Assert.IsTrue(expected.Trim().Length != 0);
            }
        }
    
        [Test]
        public void TestLargeNumberContexts()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByAString  partition by TheString from SupportBean");
    
            var fields = "col1".Split(',');
            var stmtOne = _epService.EPAdministrator.CreateEPL("context SegmentedByAString " +
                    "select Sum(IntPrimitive) as col1," +
                    "prev(1, IntPrimitive)," +
                    "prior(1, IntPrimitive)," +
                    "(select id from SupportBean_S0.std:lastevent())" +
                    "  from SupportBean.win:keepall()");
            stmtOne.Events += _listener.Update;
            
            for (var i = 0; i < 10000; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{i});
            }
        }
    
        [Test]
        public void TestAdditionalFilters() {
            var filterSPI = (FilterServiceSPI) _spi.FilterService;
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByAString " +
                    "partition by TheString from SupportBean(IntPrimitive>0), p00 from SupportBean_S0(id > 0)");
    
            // first send a view events
            _epService.EPRuntime.SendEvent(new SupportBean("B1", -1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-2, "S0"));
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            var fields = "col1,col2".Split(',');
            var stmtOne = _epService.EPAdministrator.CreateEPL("context SegmentedByAString " +
                    "select Sum(sb.IntPrimitive) as col1, Sum(s0.id) as col2 " +
                    "from pattern [every (s0=SupportBean_S0 or sb=SupportBean)]");
            stmtOne.Events += _listener.Update;
    
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-3, "S0"));
            _epService.EPRuntime.SendEvent(new SupportBean("S0", -1));
            _epService.EPRuntime.SendEvent(new SupportBean("S1", -2));
            Assert.IsFalse(_listener.IsInvoked);
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, 2});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{10, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-2, "S0"));
            _epService.EPRuntime.SendEvent(new SupportBean("S1", -10));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{10, 3});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S0", 9));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{9, 2});
    
            _epService.EPAdministrator.DestroyAllStatements();
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            // Test unnecessary filter
            var epl = "create context CtxSegmented partition by TheString from SupportBean;" +
                         "context CtxSegmented select * from pattern [every a=SupportBean -> c=SupportBean(c.TheString=a.TheString)];";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
        }
    
        [Test]
        public void TestMultiStatementFilterCount() {
            var filterSPI = (FilterServiceSPI) _spi.FilterService;
            var stmtContext = _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByAString " +
                    "partition by TheString from SupportBean, p00 from SupportBean_S0");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            // first send a view events
            _epService.EPRuntime.SendEvent(new SupportBean("B1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0"));
    
            var fields = new String[] {"col1"};
            var stmtOne = _epService.EPAdministrator.CreateEPL("context SegmentedByAString select Sum(id) as col1 from SupportBean_S0");
            stmtOne.Events += _listener.Update;
    
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{10});
    
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(8, "S1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{8});
    
            Assert.AreEqual(4, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "S0"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{14});
    
            Assert.AreEqual(4, filterSPI.FilterCountApprox);
    
            var stmtTwo = _epService.EPAdministrator.CreateEPL("context SegmentedByAString select Sum(IntPrimitive) as col1 from SupportBean");
            stmtTwo.Events += _listener.Update;
    
            Assert.AreEqual(6, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("S0", 5));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{5});
    
            Assert.AreEqual(6, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("S2", 6));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{6});
    
            Assert.AreEqual(8, filterSPI.FilterCountApprox);
    
            stmtOne.Dispose();
            Assert.AreEqual(5, filterSPI.FilterCountApprox);  // 5 = 3 from context instances and 2 from context itself
    
            stmtTwo.Dispose();
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            stmtContext.Dispose();
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
        }
    
        [Test]
        public void TestSegmentedSubtype() {
            _epService.EPAdministrator.Configuration.AddEventType("ISupportBaseAB", typeof(ISupportBaseAB));
            _epService.EPAdministrator.Configuration.AddEventType("ISupportA", typeof(ISupportA));
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by baseAB from ISupportBaseAB");
    
            var fields = "col1".Split(',');
            var stmt = _epService.EPAdministrator.CreateEPL("context SegmentedByString select Count(*) as col1 from ISupportA");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new ISupportAImpl("A1", "AB1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
    
            _epService.EPRuntime.SendEvent(new ISupportAImpl("A2", "AB1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{2L});
    
            _epService.EPRuntime.SendEvent(new ISupportAImpl("A3", "AB2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
    
            _epService.EPRuntime.SendEvent(new ISupportAImpl("A4", "AB1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{3L});
        }
    
        [Test]
        public void TestSegmentedJoinMultitypeMultifield()
        {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedBy2Fields " +
                    "partition by TheString and IntPrimitive from SupportBean, p00 and id from SupportBean_S0");
    
            var fields = "c1,c2,c3,c4,c5,c6".Split(',');
            var stmt = _epService.EPAdministrator.CreateEPL("context SegmentedBy2Fields " +
                    "select TheString as c1, IntPrimitive as c2, id as c3, p00 as c4, context.key1 as c5, context.key2 as c6 " +
                    "from SupportBean.std:lastevent(), SupportBean_S0.std:lastevent()");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "G1"));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "G2"));
            Assert.IsFalse(_listener.IsInvoked);
            
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 1, 1, "G2", "G2", 1});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "G2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 2, 2, "G2", "G2", 2});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "G1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 1, 1, "G1", "G1", 1});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 2, 2, "G1", "G1", 2});
    
            // ESPER-663
            _epService.EPAdministrator.Configuration.AddEventType<Event>("Event");
            var epl =
                "@Audit @Name('CTX') create context Ctx partition by grp, subGrp from Event;\n" +
                "@Audit @Name('Window') context Ctx create window EventData.std:unique(type) as Event;" +
                "@Audit @Name('Insert') context Ctx insert into EventData select * from Event;" +
                "@Audit @Name('Test') context Ctx select irstream * from EventData;";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            _epService.EPAdministrator.GetStatement("Test").Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new Event("G1", "SG1", 1, 10.45));
            Assert.IsTrue(_listener.IsInvoked);
            _epService.EPAdministrator.DestroyAllStatements();
    
            // Esper-695
            var eplTwo =
                    "create context Ctx partition by TheString from SupportBean;\n" +
                    "context Ctx create window MyWindow.std:unique(IntPrimitive) as SupportBean;" +
                    "context Ctx select irstream * from pattern [MyWindow];";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplTwo);
            TryInvalidCreateWindow();
            TryInvalidCreateWindow(); // making sure all is cleaned up
        }
    
        private void TryInvalidCreateWindow() {
            try {
                _epService.EPAdministrator.CreateEPL("context Ctx create window MyInvalidWindow.std:unique(p00) as SupportBean_S0");
                Assert.Fail();
            }
            catch (EPException ex) {
                Assert.AreEqual("Error starting statement: Segmented context 'Ctx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [context Ctx create window MyInvalidWindow.std:unique(p00) as SupportBean_S0]", ex.Message);
            }
        }
    
        [Test]
        public void TestSegmentedSubselectPrevPrior() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fieldsPrev = new String[] {"TheString", "col1"};
            var stmtPrev = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select TheString, (select Prev(0, id) from SupportBean_S0.win:keepall()) as col1 from SupportBean");
            stmtPrev.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrev, new Object[]{"G1", null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrev, new Object[]{"G1", 1});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrev, new Object[]{"G2", null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrev, new Object[]{"G2", 2});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrev, new Object[]{"G1", null});  // since returning multiple rows
    
            stmtPrev.Stop();
    
            var fieldsPrior = new String[] {"TheString", "col1"};
            var stmtPrior = _epService.EPAdministrator.CreateEPL("@Name('B') context SegmentedByString " +
                    "select TheString, (select Prior(0, id) from SupportBean_S0.win:keepall()) as col1 from SupportBean");
            stmtPrior.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrior, new Object[]{"G1", null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrior, new Object[]{"G1", 1});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrior, new Object[]{"G2", null});    // since category started as soon as statement added
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrior, new Object[]{"G2", 2}); // since returning multiple rows
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsPrior, new Object[]{"G1", null});  // since returning multiple rows
        }
    
        [Test]
        public void TestSegmentedPrior() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fields = new String[] {"val0", "val1"};
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select IntPrimitive as val0, Prior(1, IntPrimitive) as val1 from SupportBean");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{10, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{20, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{11, 10});
            
            stmtOne.Stop();
    
            stmtOne.Dispose();
        }
    
        [Test]
        public void TestSegmentedSubqueryFiltered() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fields = new String[] {"TheString", "IntPrimitive", "val0"};
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select TheString, IntPrimitive, (select p00 from SupportBean_S0.std:lastevent() as s0 where sb.IntPrimitive = s0.id) as val0 " +
                    "from SupportBean as sb");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s1"));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s2"));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10, "s2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 10, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s3"));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 10, "s3"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G3", 10, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10, "s3"});
        }
    
        [Test]
        public void TestSegmentedSubqueryNamedWindowIndexShared() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("@Hint('enable_window_subquery_indexshare') create window MyWindow.win:keepall() as SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_S0");
    
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select TheString, IntPrimitive, (select p00 from MyWindow as s0 where sb.IntPrimitive = s0.id) as val0 " +
                    "from SupportBean as sb");
            stmtOne.Events += _listener.Update;
    
            RunAssertionSubqueryNW();
        }
    
        [Test]
        public void TestSegmentedSubqueryNamedWindowIndexUnShared() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_S0");
    
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select TheString, IntPrimitive, (select p00 from MyWindow as s0 where sb.IntPrimitive = s0.id) as val0 " +
                    "from SupportBean as sb");
            stmtOne.Events += _listener.Update;
    
            RunAssertionSubqueryNW();
        }
    
        private void RunAssertionSubqueryNW() {
            var fields = new String[] {"TheString", "IntPrimitive", "val0"};
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s1"));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10, "s1"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 10, "s1"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G3", 20, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "s2"));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G3", 20, "s2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 20, "s2"});
        }
    
        [Test]
        public void TestSegmentedJoin() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fields = new String[] {"sb.TheString", "sb.IntPrimitive", "s0.id"};
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select * from SupportBean.win:keepall() as sb, SupportBean_S0.win:keepall() as s0 " +
                    "where IntPrimitive = id");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 20, 20});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(30));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 30));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 30));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 30, 30});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 30));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 30, 30});
        }
    
        [Test]
        public void TestSegmentedPattern() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fields = new String[] {"a.TheString", "a.IntPrimitive", "b.TheString", "b.IntPrimitive"};
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean -> b=SupportBean(IntPrimitive=a.IntPrimitive+1)]");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 20, "G2", 21});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10, "G1", 11});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 22));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 21, "G2", 22});
    
            stmtOne.Dispose();
    
            // add another statement: contexts already exist, this one uses @Consume
            var stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean -> b=SupportBean(IntPrimitive=a.IntPrimitive+1)@Consume]");
            stmtTwo.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 20, "G2", 21});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10, "G1", 11});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 22));
            Assert.IsFalse(_listener.IsInvoked);
    
            stmtTwo.Dispose();
    
            // test truly segmented consume
            var fieldsThree = new String[] {"a.TheString", "a.IntPrimitive", "b.id", "b.p00"};
            var stmtThree = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean -> b=SupportBean_S0(id=a.IntPrimitive)@Consume]");
            stmtThree.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));   // should be 2 output rows
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.LastNewData, fieldsThree, new Object[][] { new Object[] {"G1", 10, 10, "E1"}, new Object[] {"G2", 10, 10, "E1"}});
        }
    
        [Test]
        public void TestSegmentedViews()
        {
            var contextEPL = "@Name('context') create context SegmentedByString as partition by TheString from SupportBean";
            _epService.EPAdministrator.CreateEPL(contextEPL);
    
            var fieldsIterate = "IntPrimitive".Split(',');
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select irstream IntPrimitive, Prevwindow(items) as pw from SupportBean.win:length(2) as items");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            AssertViewData(10, new Object[][]{new Object[] {"G1", 10}}, null);
            EPAssertionUtil.AssertPropsPerRow(stmtOne.GetEnumerator(), stmtOne.GetSafeEnumerator(), fieldsIterate, new Object[][]{new Object[] {10}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            AssertViewData(20, new Object[][]{new Object[] {"G2", 20}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            AssertViewData(11, new Object[][]{new Object[] {"G1", 11}, new Object[] {"G1", 10}}, null);
            EPAssertionUtil.AssertPropsPerRow(stmtOne.GetEnumerator(), stmtOne.GetSafeEnumerator(), fieldsIterate, new Object[][]{new Object[] {10}, new Object[] {11}, new Object[] {20}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            AssertViewData(21, new Object[][]{new Object[] {"G2", 21}, new Object[] {"G2", 20}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 12));
            AssertViewData(12, new Object[][]{new Object[] {"G1", 12}, new Object[] {"G1", 11}}, 10);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 22));
            AssertViewData(22, new Object[][]{new Object[] {"G2", 22}, new Object[] {"G2", 21}}, 20);
    
            stmtOne.Dispose();
            
            // test SODA
            _epService.EPAdministrator.DestroyAllStatements();
            var model = _epService.EPAdministrator.CompileEPL(contextEPL);
            Assert.AreEqual(contextEPL, model.ToEPL());
            var stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(contextEPL, stmt.Text);
    
            // test built-in properties
            var fields = "c1,c2,c3,c4".Split(',');
            var ctx = "SegmentedByString";
            var stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select context.name as c1, context.id as c2, context.key1 as c3, TheString as c4 " +
                    "from SupportBean.win:length(2) as items");
            stmtTwo.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));

            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { ctx, 0, "G1", "G1" });
            _epService.EPAdministrator.DestroyAllStatements();

            // test grouped delivery
            _epService.EPAdministrator.CreateEPL("create variable boolean trigger = false");
            _epService.EPAdministrator.CreateEPL("create context MyCtx partition by TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('Out') context MyCtx select * from SupportBean.win:expr(not trigger) for grouped_delivery(TheString)");
            _epService.EPAdministrator.GetStatement("Out").Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SetVariableValue("trigger", true);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(100));

            Assert.AreEqual(2, _listener.NewDataList.Count);
        }

        [Test]
        public void TestJoinWhereClauseOnPartitionKey()
        {
            String epl = "create context MyCtx partition by TheString from SupportBean;\n" +
                    "@Name('select') context MyCtx select * from SupportBean.std:lastevent() as sb, SupportBean_S0.std:lastevent() as s0 " +
                    "where TheString is 'Test'";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            _epService.EPAdministrator.GetStatement("select").Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("Test", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.IsTrue(_listener.IsInvoked);
        }
    
        private void AssertViewData(int newIntExpected, Object[][] newArrayExpected, int? oldIntExpected) {
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(newIntExpected, _listener.LastNewData[0].Get("IntPrimitive"));
            var beans = (SupportBean[]) _listener.LastNewData[0].Get("pw");
            Assert.AreEqual(newArrayExpected.Length, beans.Length);
            for (var i = 0; i < beans.Length; i++) {
                Assert.AreEqual(newArrayExpected[i][0], beans[i].TheString);
                Assert.AreEqual(newArrayExpected[i][1], beans[i].IntPrimitive);
            }
    
            if (oldIntExpected != null) {
                Assert.AreEqual(1, _listener.LastOldData.Length);
                Assert.AreEqual(oldIntExpected, _listener.LastOldData[0].Get("IntPrimitive"));
            }
            else {
                Assert.IsNull(_listener.LastOldData);
            }
            _listener.Reset();
        }
    
        private class MySelectorFilteredPartitioned : ContextPartitionSelectorFiltered
        {
            private readonly Object[] _match;
    
            private readonly IList<Object[]> _contexts = new List<Object[]>();
            private readonly LinkedHashSet<int> _cpids = new LinkedHashSet<int>();
    
            internal MySelectorFilteredPartitioned(Object[] match) {
                _match = match;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                var id = (ContextPartitionIdentifierPartitioned) contextPartitionIdentifier;
                if (_match == null && _cpids.Contains(id.ContextPartitionId.Value)) {
                    throw new Exception("Already exists context id: " + id.ContextPartitionId);
                }
                _cpids.Add(id.ContextPartitionId.Value);
                _contexts.Add(id.Keys);
                return Collections.AreEqual(id.Keys, _match);
            }

            public IList<object[]> Contexts
            {
                get { return _contexts; }
            }
        }
    
        public class Event
        {
            public Event() {}
    
       		public Event(String group, String subGroup, int type, double value)
            {
       			Grp = group;
       			SubGrp = subGroup;
       			Type = type;
       			Value = value;
       		}

            public string Grp { get; set; }

            public string SubGrp { get; set; }

            public int Type { get; set; }

            public double Value { get; set; }

            public override bool Equals(Object obj)
            {
       			if (this == obj) {
       				return true;
       			}
       			if (obj is Event) {
       				Event evt = (Event) obj;
       				return Grp.Equals(evt.Grp) && SubGrp.Equals(evt.SubGrp) && Type == evt.Type && Math.Abs(Value - evt.Value) < 1e-6;
       			}
    
       			return false;
       		}

            public override String ToString() {
       			return "(" + Grp + ", " + SubGrp + ")@" + Type + "=" + Value;
       		}
    
       	}
    
        private void SendWebEventsIncomplete(int id) {
            _epService.EPRuntime.SendEvent(new WebEvent("Start", id.ToString(CultureInfo.InvariantCulture)));
            _epService.EPRuntime.SendEvent(new WebEvent("End", id.ToString(CultureInfo.InvariantCulture)));
        }
    
        private void SendWebEventsComplete(int id) {
            _epService.EPRuntime.SendEvent(new WebEvent("Start", id.ToString(CultureInfo.InvariantCulture)));
            _epService.EPRuntime.SendEvent(new WebEvent("Middle", id.ToString(CultureInfo.InvariantCulture)));
            _epService.EPRuntime.SendEvent(new WebEvent("End", id.ToString(CultureInfo.InvariantCulture)));
        }

        private SupportBean MakeEvent(String theString, int intPrimitive, long longPrimitive)
        {
            SupportBean bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        public static bool StringContainsX(String theString)
        {
            return theString.Contains("X");
        }

        public class WebEvent
        {
            public WebEvent(String pageName, String sessionId) {
                PageName = pageName;
                SessionId = sessionId;
            }

            public string SessionId { get; private set; }

            public string PageName { get; private set; }
        }
    }
}
