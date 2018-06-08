///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateFirstLastWindow : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNoParamChainedAndProperty(epService);
            RunAssertionLastMaxMixedOnSelect(epService);
            RunAssertionPrevNthIndexedFirstLast(epService);
            RunAssertionFirstLastIndexed(epService);
            RunAssertionInvalid(epService);
            RunAssertionSubquery(epService);
            RunAssertionMethodAndAccessTogether(epService);
            RunAssertionOutputRateLimiting(epService);
            RunAssertionTypeAndColNameAndEquivalency(epService);
            RunAssertionJoin2Access(epService);
            RunAssertionOuterJoin1Access(epService);
            RunAssertionBatchWindow(epService);
            RunAssertionBatchWindowGrouped(epService);
            RunAssertionLateInitialize(epService);
            RunAssertionOnDelete(epService);
            RunAssertionOnDemandQuery(epService);
            RunAssertionStar(epService);
            RunAssertionUnboundedStream(epService);
            RunAssertionWindowedUnGrouped(epService);
            RunAssertionWindowedGrouped(epService);
        }
    
        private void RunAssertionNoParamChainedAndProperty(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("ChainEvent", typeof(ChainEvent));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select first().property as val0, first().MyMethod() as val1, window() as val2 from ChainEvent#lastevent");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new ChainEvent("p1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new object[]{"p1", "abc"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionLastMaxMixedOnSelect(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindowOne#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowOne select * from SupportBean(TheString like 'A%')");
    
            string epl = "on SupportBean(TheString like 'B%') select last(mw.IntPrimitive) as li, max(mw.IntPrimitive) as mi from MyWindowOne mw";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "li,mi".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("B1", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 10});
    
            for (int i = 11; i < 20; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("A1", i));
                epService.EPRuntime.SendEvent(new SupportBean("Bx", -1));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{i, i});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B1", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 19});
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B1", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, 19});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPrevNthIndexedFirstLast(EPServiceProvider epService) {
            string epl = "select " +
                    "prev(IntPrimitive, 0) as p0, " +
                    "prev(IntPrimitive, 1) as p1, " +
                    "prev(IntPrimitive, 2) as p2, " +
                    "nth(IntPrimitive, 0) as n0, " +
                    "nth(IntPrimitive, 1) as n1, " +
                    "nth(IntPrimitive, 2) as n2, " +
                    "last(IntPrimitive, 0) as l1, " +
                    "last(IntPrimitive, 1) as l2, " +
                    "last(IntPrimitive, 2) as l3 " +
                    "from SupportBean#length(3)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "p0,p1,p2,n0,n1,n2,l1,l2,l3".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, null, null, 10, null, null, 10, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{11, 10, null, 11, 10, null, 11, 10, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{12, 11, 10, 12, 11, 10, 12, 11, 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 13));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{13, 12, 11, 13, 12, 11, 13, 12, 11});
    
            stmt.Dispose();
        }
    
        private void RunAssertionFirstLastIndexed(EPServiceProvider epService) {
            string epl = "select " +
                    "first(IntPrimitive, 0) as f0, " +
                    "first(IntPrimitive, 1) as f1, " +
                    "first(IntPrimitive, 2) as f2, " +
                    "first(IntPrimitive, 3) as f3, " +
                    "last(IntPrimitive, 0) as l0, " +
                    "last(IntPrimitive, 1) as l1, " +
                    "last(IntPrimitive, 2) as l2, " +
                    "last(IntPrimitive, 3) as l3 " +
                    "from SupportBean#length(3)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionFirstLastIndexed(epService, listener);
    
            // test join
            stmt.Dispose();
            epl += ", SupportBean_A#lastevent";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
    
            TryAssertionFirstLastIndexed(epService, listener);
    
            // test variable
            stmt.Dispose();
            epService.EPAdministrator.CreateEPL("create variable int indexvar = 2");
            epl = "select " +
                    "first(IntPrimitive, indexvar) as f0 " +
                    "from SupportBean#keepall";
    
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            string[] fields = "f0".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{12});
    
            epService.EPRuntime.SetVariableValue("indexvar", 0);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 13));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10});
            stmt.Dispose();
    
            // test as part of function
            epService.EPAdministrator.CreateEPL("select Math.Abs(last(IntPrimitive)) from SupportBean").Dispose();
        }
    
        private void TryAssertionFirstLastIndexed(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "f0,f1,f2,f3,l0,l1,l2,l3".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, null, null, null, 10, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 11, null, null, 11, 10, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 11, 12, null, 12, 11, 10, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 13));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{11, 12, 13, null, 13, 12, 11, null});
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "select window(distinct IntPrimitive) from SupportBean",
                    "Incorrect syntax near '(' ('distinct' is a reserved keyword) at line 1 column 13 near reserved keyword 'distinct' [");
    
            TryInvalid(epService, "select window(sa.IntPrimitive + sb.IntPrimitive) from SupportBean#lastevent sa, SupportBean#lastevent sb",
                    "Error starting statement: Failed to validate select-clause expression 'window(sa.IntPrimitive+sb.IntPrimitive)': The 'window' aggregation function requires that any child expressions evaluate properties of the same stream; Use 'firstever' or 'lastever' or 'nth' instead [select window(sa.IntPrimitive + sb.IntPrimitive) from SupportBean#lastevent sa, SupportBean#lastevent sb]");
    
            TryInvalid(epService, "select last(*) from SupportBean#lastevent sa, SupportBean#lastevent sb",
                    "Error starting statement: Failed to validate select-clause expression 'last(*)': The 'last' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead [select last(*) from SupportBean#lastevent sa, SupportBean#lastevent sb]");
    
            TryInvalid(epService, "select TheString, (select first(*) from SupportBean#lastevent sa) from SupportBean#lastevent sb",
                    "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Failed to validate select-clause expression 'first(*)': The 'first' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead [select TheString, (select first(*) from SupportBean#lastevent sa) from SupportBean#lastevent sb]");
    
            TryInvalid(epService, "select window(x.*) from SupportBean#lastevent",
                    "Error starting statement: Failed to validate select-clause expression 'window(x.*)': Stream by name 'x' could not be found among all streams [select window(x.*) from SupportBean#lastevent]");
    
            TryInvalid(epService, "select window(*) from SupportBean x",
                    "Error starting statement: Failed to validate select-clause expression 'window(*)': The 'window' aggregation function requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(*) from SupportBean x]");
            TryInvalid(epService, "select window(x.*) from SupportBean x",
                    "Error starting statement: Failed to validate select-clause expression 'window(x.*)': The 'window' aggregation function requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(x.*) from SupportBean x]");
            TryInvalid(epService, "select window(x.IntPrimitive) from SupportBean x",
                    "Error starting statement: Failed to validate select-clause expression 'window(x.IntPrimitive)': The 'window' aggregation function requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(x.IntPrimitive) from SupportBean x]");
    
            TryInvalid(epService, "select window(x.IntPrimitive, 10) from SupportBean#keepall x",
                    "Error starting statement: Failed to validate select-clause expression 'window(x.IntPrimitive,10)': The 'window' aggregation function does not accept an index expression; Use 'first' or 'last' instead [");
    
            TryInvalid(epService, "select first(x.*, 10d) from SupportBean#lastevent as x",
                    "Error starting statement: Failed to validate select-clause expression 'first(x.*,10.0d)': The 'first' aggregation function requires an index expression that returns an integer value [select first(x.*, 10d) from SupportBean#lastevent as x]");
        }
    
        private void RunAssertionSubquery(EPServiceProvider epService) {
            string epl = "select id, (select window(sb.*) from SupportBean#length(2) as sb) as w from SupportBean_A";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "id,w".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", null});
    
            SupportBean beanOne = SendEvent(epService, "E1", 0, 1);
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A2", new object[]{beanOne}});
    
            SupportBean beanTwo = SendEvent(epService, "E2", 0, 1);
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A3", new object[]{beanOne, beanTwo}});
    
            SupportBean beanThree = SendEvent(epService, "E2", 0, 1);
            epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A4", new object[]{beanTwo, beanThree}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMethodAndAccessTogether(EPServiceProvider epService) {
            string epl = "select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean#length(2) as sa";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "si,wi".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, IntArray(1)});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, IntArray(1, 2)});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5, IntArray(2, 3)});
    
            stmt.Dispose();
            epl = "select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean#keepall as sa group by TheString";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, IntArray(1)});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, IntArray(2)});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5, IntArray(2, 3)});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5, IntArray(1, 4)});
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputRateLimiting(EPServiceProvider epService) {
            string epl = "select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean#keepall as sa output every 2 events";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "si,wi".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[]
            {
                    new object[] {1, IntArray(1)},
                    new object[] {3, IntArray(1, 2)},
            });
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[]
            {
                    new object[] {6, IntArray(1, 2, 3)},
                    new object[] {10, IntArray(1, 2, 3, 4)},
            });
    
            stmt.Dispose();
        }
    
        private void RunAssertionTypeAndColNameAndEquivalency(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
    
            string epl = "select " +
                    "first(sa.DoublePrimitive + sa.IntPrimitive), " +
                    "first(sa.IntPrimitive), " +
                    "window(sa.*), " +
                    "last(*) from SupportBean#length(2) as sa";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var rows = new[]
            {
                    new object[] {"first(sa.DoublePrimitive+sa.IntPrimitive)", typeof(double?)},
                    new object[] {"first(sa.IntPrimitive)", typeof(int)},
                    new object[] {"window(sa.*)", typeof(SupportBean[])},
                    new object[] {"last(*)", typeof(SupportBean)},
            };
            for (int i = 0; i < rows.Length; i++) {
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }
    
            stmt.Dispose();
            epl = "select " +
                    "first(sa.DoublePrimitive + sa.IntPrimitive) as f1, " +
                    "first(sa.IntPrimitive) as f2, " +
                    "window(sa.*) as w1, " +
                    "last(*) as l1 " +
                    "from SupportBean#length(2) as sa";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            TryAssertionType(epService, listener, false);
    
            stmt.Dispose();
    
            epl = "select " +
                    "first(sa.DoublePrimitive + sa.IntPrimitive) as f1, " +
                    "first(sa.IntPrimitive) as f2, " +
                    "window(sa.*) as w1, " +
                    "last(*) as l1 " +
                    "from SupportBean#length(2) as sa " +
                    "having SupportStaticMethodLib.AlwaysTrue({first(sa.DoublePrimitive + sa.IntPrimitive), " +
                    "first(sa.IntPrimitive), window(sa.*), last(*)})";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            TryAssertionType(epService, listener, true);
    
            stmt.Dispose();
        }
    
        private void TryAssertionType(EPServiceProvider epService, SupportUpdateListener listener, bool isCheckStatic) {
            string[] fields = "f1,f2,w1,l1".Split(',');
            SupportStaticMethodLib.Invocations.Clear();
    
            SupportBean beanOne = SendEvent(epService, "E1", 10d, 100);
            var expected = new object[]{110d, 100, new object[]{beanOne}, beanOne};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
            if (isCheckStatic) {
                object[] parameters = SupportStaticMethodLib.Invocations[0];
                SupportStaticMethodLib.Invocations.Clear();
                EPAssertionUtil.AssertEqualsExactOrder(expected, parameters);
            }
        }
    
        private void RunAssertionJoin2Access(EPServiceProvider epService) {
            string epl = "select " +
                    "sa.id as ast, " +
                    "sb.id as bst, " +
                    "first(sa.id) as fas, " +
                    "window(sa.id) as was, " +
                    "last(sa.id) as las, " +
                    "first(sb.id) as fbs, " +
                    "window(sb.id) as wbs, " +
                    "last(sb.id) as lbs " +
                    "from SupportBean_A#length(2) as sa, SupportBean_B#length(2) as sb " +
                    "order by ast, bst";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "ast,bst,fas,was,las,fbs,wbs,lbs".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"A1", "B1", "A1", Split("A1"), "A1", "B1", Split("B1"), "B1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new[]
                    {
                            new[] {"A2", "B1", "A1", Split("A1,A2"), "A2", "B1", Split("B1"), "B1"}
                    });
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new[]
                    {
                            new[] {"A3", "B1", "A2", Split("A2,A3"), "A3", "B1", Split("B1"), "B1"}
                    });
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new[]
                    {
                            new[] {"A2", "B2", "A2", Split("A2,A3"), "A3", "B1", Split("B1,B2"), "B2"},
                            new[] {"A3", "B2", "A2", Split("A2,A3"), "A3", "B1", Split("B1,B2"), "B2"}
                    });
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B3"));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new[]
                    {
                            new[] {"A2", "B3", "A2", Split("A2,A3"), "A3", "B2", Split("B2,B3"), "B3"},
                            new[] {"A3", "B3", "A2", Split("A2,A3"), "A3", "B2", Split("B2,B3"), "B3"}
                    });
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new[]
                    {
                            new[] {"A4", "B2", "A3", Split("A3,A4"), "A4", "B2", Split("B2,B3"), "B3"},
                            new[] {"A4", "B3", "A3", Split("A3,A4"), "A4", "B2", Split("B2,B3"), "B3"}
                    });
    
            stmt.Dispose();
        }
    
        private void RunAssertionOuterJoin1Access(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
            string epl = "select " +
                    "sa.id as aid, " +
                    "sb.id as bid, " +
                    "first(sb.p10) as fb, " +
                    "window(sb.p10) as wb, " +
                    "last(sb.p10) as lb " +
                    "from S0#keepall as sa " +
                    "left outer join " +
                    "S1#keepall as sb " +
                    "on sa.id = sb.id";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "aid,bid,fb,wb,lb".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{1, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new[]{1, 1, "A", Split("A"), "A"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new[]{2, 2, "A", Split("A,B"), "B"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(3, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "C"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new[]{3, 3, "A", Split("A,B,C"), "C"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionBatchWindow(EPServiceProvider epService) {
            string epl = "select irstream " +
                    "first(TheString) as fs, " +
                    "window(TheString) as ws, " +
                    "last(TheString) as ls " +
                    "from SupportBean#length_batch(2) as sb";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "fs,ws,ls".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{null, null, null});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new[]{"E1", Split("E1,E2"), "E2"});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new[]{"E1", Split("E1,E2"), "E2"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new[]{"E3", Split("E3,E4"), "E4"});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new[]{"E3", Split("E3,E4"), "E4"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new[]{"E5", Split("E5,E6"), "E6"});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionBatchWindowGrouped(EPServiceProvider epService) {
            string epl = "select " +
                    "TheString, " +
                    "first(IntPrimitive) as fi, " +
                    "window(IntPrimitive) as wi, " +
                    "last(IntPrimitive) as li " +
                    "from SupportBean#length_batch(6) as sb group by TheString order by TheString asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "TheString,fi,wi,li".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[]
            {
                    new object[] {"E1", 10, IntArray(10, 11, 12), 12},
                    new object[] {"E2", 20, IntArray(20), 20},
                    new object[] {"E3", 30, IntArray(30, 31), 31}
            });
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 13));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 14));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 15));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 16));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 17));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 18));
            EventBean[] result = listener.GetAndResetLastNewData();
            EPAssertionUtil.AssertPropsPerRow(result, fields, new[]
            {
                    new object[] {"E1", 13, IntArray(13, 14, 15, 16, 17, 18), 18},
                    new object[] {"E2", null, null, null},
                    new object[] {"E3", null, null, null}
            });
    
            stmt.Dispose();
        }
    
        private void RunAssertionLateInitialize(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindowTwo#keepall as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowTwo select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
    
            string[] fields = "firststring,windowstring,laststring".Split(',');
            string epl = "select " +
                    "first(TheString) as firststring, " +
                    "window(TheString) as windowstring, " +
                    "last(TheString) as laststring " +
                    "from MyWindowTwo";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E1", Split("E1,E2,E3"), "E3"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOnDelete(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindowThree#keepall as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowThree select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindowThree where TheString = id");
    
            string[] fields = "firststring,windowstring,laststring".Split(',');
            string epl = "select " +
                    "first(TheString) as firststring, " +
                    "window(TheString) as windowstring, " +
                    "last(TheString) as laststring " +
                    "from MyWindowThree";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E1", Split("E1"), "E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E1", Split("E1,E2"), "E2"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E1", Split("E1,E2,E3"), "E3"});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E1", Split("E1,E3"), "E3"});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E1", Split("E1"), "E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 40));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E4", Split("E4"), "E4"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 50));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E4", Split("E4,E5"), "E5"});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E5", Split("E5"), "E5"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 60));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new[]{"E5", Split("E5,E6"), "E6"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOnDemandQuery(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindowFour#keepall as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowFour select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
    
            EPOnDemandPreparedQuery q = epService.EPRuntime.PrepareQuery("select first(IntPrimitive) as f, window(IntPrimitive) as w, last(IntPrimitive) as l from MyWindowFour as s");
            EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "f,w,l".Split(','),
                    new[] {new object[] {10, IntArray(10, 20, 30, 31, 11, 12), 12}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 13));
            EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "f,w,l".Split(','),
                    new[] {new object[] {10, IntArray(10, 20, 30, 31, 11, 12, 13), 13}});
    
            q = epService.EPRuntime.PrepareQuery("select TheString as s, first(IntPrimitive) as f, window(IntPrimitive) as w, last(IntPrimitive) as l from MyWindowFour as s group by TheString order by TheString asc");
            var expected = new[]
            {
                    new object[] {"E1", 10, IntArray(10, 11, 12, 13), 13},
                    new object[] {"E2", 20, IntArray(20), 20},
                    new object[] {"E3", 30, IntArray(30, 31), 31}
            };
            EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "s,f,w,l".Split(','), expected);
            EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "s,f,w,l".Split(','), expected);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStar(EPServiceProvider epService) {
            string epl = "select " +
                    "first(*) as firststar, " +
                    "first(sb.*) as firststarsb, " +
                    "last(*) as laststar, " +
                    "last(sb.*) as laststarsb, " +
                    "window(*) as windowstar, " +
                    "window(sb.*) as windowstarsb " +
                    "from SupportBean#length(2) as sb";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionStar(epService, listener);
            stmt.Dispose();
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
            Assert.AreEqual(epl, model.ToEPL());
    
            TryAssertionStar(epService, listener);
    
            stmt.Dispose();
        }
    
        private void TryAssertionStar(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "firststar,firststarsb,laststar,laststarsb,windowstar,windowstarsb".Split(',');
    
            var beanE1 = new SupportBean("E1", 10);
            epService.EPRuntime.SendEvent(beanE1);
            var window = new object[]{beanE1};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{beanE1, beanE1, beanE1, beanE1, window, window});
    
            var beanE2 = new SupportBean("E2", 20);
            epService.EPRuntime.SendEvent(beanE2);
            window = new object[]{beanE1, beanE2};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{beanE1, beanE1, beanE2, beanE2, window, window});
    
            var beanE3 = new SupportBean("E3", 30);
            epService.EPRuntime.SendEvent(beanE3);
            window = new object[]{beanE2, beanE3};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{beanE2, beanE2, beanE3, beanE3, window, window});
        }
    
        private void RunAssertionUnboundedStream(EPServiceProvider epService) {
            string epl = "select " +
                    "first(TheString) as f1, " +
                    "first(sb.*) as f2, " +
                    "first(*) as f3, " +
                    "last(TheString) as l1, " +
                    "last(sb.*) as l2, " +
                    "last(*) as l3 " +
                    "from SupportBean as sb";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "f1,f2,f3,l1,l2,l3".Split(',');
    
            SupportBean beanOne = SendEvent(epService, "E1", 1d, 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", beanOne, beanOne, "E1", beanOne, beanOne});
    
            SupportBean beanTwo = SendEvent(epService, "E2", 2d, 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", beanOne, beanOne, "E2", beanTwo, beanTwo});
    
            SupportBean beanThree = SendEvent(epService, "E3", 3d, 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", beanOne, beanOne, "E3", beanThree, beanThree});
    
            stmt.Dispose();
        }
    
        private void RunAssertionWindowedUnGrouped(EPServiceProvider epService) {
            string epl = "select " +
                    "first(TheString) as firststring, " +
                    "last(TheString) as laststring, " +
                    "first(IntPrimitive) as firstint, " +
                    "last(IntPrimitive) as lastint, " +
                    "window(IntPrimitive) as allint " +
                    "from SupportBean#length(2)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionUngrouped(epService, listener);
    
            stmt.Dispose();
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
            Assert.AreEqual(epl, model.ToEPL());
    
            TryAssertionUngrouped(epService, listener);
    
            stmt.Dispose();
    
            // test null-value provided
            EPStatement stmtWNull = epService.EPAdministrator.CreateEPL("select window(IntBoxed).take(10) from SupportBean#length(2)");
            stmtWNull.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            stmtWNull.Dispose();
        }
    
        private void RunAssertionWindowedGrouped(EPServiceProvider epService) {
            string epl = "select " +
                    "TheString, " +
                    "first(TheString) as firststring, " +
                    "last(TheString) as laststring, " +
                    "first(IntPrimitive) as firstint, " +
                    "last(IntPrimitive) as lastint, " +
                    "window(IntPrimitive) as allint " +
                    "from SupportBean#length(5) " +
                    "group by TheString order by TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionGrouped(epService, listener);
    
            stmt.Dispose();
    
            // SODA
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
            Assert.AreEqual(epl, model.ToEPL());
    
            TryAssertionGrouped(epService, listener);
    
            // test hints
            stmt.Dispose();
            string newEPL = "@Hint('disable_reclaim_group') " + epl;
            stmt = epService.EPAdministrator.CreateEPL(newEPL);
            stmt.Events += listener.Update;
            TryAssertionGrouped(epService, listener);
    
            // test hints
            stmt.Dispose();
            newEPL = "@Hint('reclaim_group_aged=10,reclaim_group_freq=5') " + epl;
            stmt = epService.EPAdministrator.CreateEPL(newEPL);
            stmt.Events += listener.Update;
            TryAssertionGrouped(epService, listener);
            stmt.Dispose();
    
            // test SODA indexes
            string eplFirstLast = "select " +
                    "last(IntPrimitive), " +
                    "last(IntPrimitive,1), " +
                    "first(IntPrimitive), " +
                    "first(IntPrimitive,1) " +
                    "from SupportBean#length(3)";
            EPStatementObjectModel modelFirstLast = epService.EPAdministrator.CompileEPL(eplFirstLast);
            Assert.AreEqual(eplFirstLast, modelFirstLast.ToEPL());
        }
    
        private void TryAssertionGrouped(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "TheString,firststring,firstint,laststring,lastint,allint".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 10, "E1", 10, new[]{10}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 11, "E2", 11, new[]{11}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 10, "E1", 12, new[]{10, 12}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 13));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 11, "E2", 13, new[]{11, 13}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 14));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 11, "E2", 14, new[]{11, 13, 14}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 15));  // push out E1/10
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 12, "E1", 15, new[]{12, 15}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 16));  // push out E2/11 --> 2 events
            EventBean[] received = listener.GetAndResetLastNewData();
            EPAssertionUtil.AssertPropsPerRow(received, fields,
                    new[]
                    {
                            new object[]{"E1", "E1", 12, "E1", 16, new[]{12, 15, 16}},
                            new object[]{"E2", "E2", 13, "E2", 14, new[]{13, 14}}
                    });
        }
    
        private void TryAssertionUngrouped(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "firststring,firstint,laststring,lastint,allint".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E1", 10, new[]{10}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E2", 11, new[]{10, 11}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 11, "E3", 12, new[]{11, 12}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 13));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 12, "E4", 13, new[]{12, 13}});
        }
    
        private object Split(string s) {
            if (s == null) {
                return new object[0];
            }
            return s.Split(',');
        }
    
        private int[] IntArray(params int[] value) {
            if (value == null) {
                return new int[0];
            }
            return value;
        }
    
        private SupportBean SendEvent(EPServiceProvider epService, string theString, double doublePrimitive, int intPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        public class ChainEvent {
            public ChainEvent() {
            }
    
            public ChainEvent(string property) {
                this.Property = property;
            }

            public string Property { get; }

            public string MyMethod() {
                return "abc";
            }
        }
    }
} // end of namespace
