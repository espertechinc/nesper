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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextPartitionedAggregate : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
            configuration.AddPlugInSingleRowFunction("toArray", GetType(), "ToArray");
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionAccessOnly(epService);
            RunAssertionSegmentedSubqueryWithAggregation(epService);
            RunAssertionGroupByEventPerGroupStream(epService);
            RunAssertionGroupByEventPerGroupBatchContextProp(epService);
            RunAssertionGroupByEventPerGroupWithAccess(epService);
            RunAssertionGroupByEventForAll(epService);
            RunAssertionGroupByEventPerGroupUnidirectionalJoin(epService);
        }

        private void RunAssertionAccessOnly(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            string eplContext = "@Name('CTX') create context SegmentedByString partition by TheString from SupportBean";
            epService.EPAdministrator.CreateEPL(eplContext);

            string[] fieldsGrouped = "TheString,IntPrimitive,col1".Split(',');
            string eplGroupedAccess =
                "@Name('S2') context SegmentedByString select TheString,IntPrimitive,window(LongPrimitive) as col1 from SupportBean#keepall sb group by IntPrimitive";
            epService.EPAdministrator.CreateEPL(eplGroupedAccess);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("S2").Events += listener.Update;

            epService.EPRuntime.SendEvent(MakeEvent("G1", 1, 10L));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsGrouped, new object[] {"G1", 1, new object[] {10L}});

            epService.EPRuntime.SendEvent(MakeEvent("G1", 2, 100L));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsGrouped, new object[] {"G1", 2, new object[] {100L}});

            epService.EPRuntime.SendEvent(MakeEvent("G2", 1, 200L));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsGrouped, new object[] {"G2", 1, new object[] {200L}});

            epService.EPRuntime.SendEvent(MakeEvent("G1", 1, 11L));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsGrouped, new object[] {"G1", 1, new object[] {10L, 11L}});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSegmentedSubqueryWithAggregation(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "@Name('context') create context SegmentedByString partition by TheString from SupportBean");

            var fields = new string[] {"TheString", "IntPrimitive", "val0"};
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(
                "@Name('A') context SegmentedByString " +
                "select TheString, IntPrimitive, (select count(*) from SupportBean_S0#keepall as s0 where sb.IntPrimitive = s0.id) as val0 " +
                "from SupportBean as sb");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s1"));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"G1", 10, 0L});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionGroupByEventPerGroupStream(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "@Name('context') create context SegmentedByString partition by TheString from SupportBean");

            string[] fieldsOne = "IntPrimitive,count(*)".Split(',');
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(
                "@Name('A') context SegmentedByString select IntPrimitive, count(*) from SupportBean group by IntPrimitive");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {10, 1L});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {200, 1L});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {10, 2L});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {11, 1L});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {200, 2L});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {10, 1L});

            stmtOne.Dispose();

            // add "string" : a context property
            string[] fieldsTwo = "TheString,IntPrimitive,count(*)".Split(',');
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(
                "@Name('B') context SegmentedByString select TheString, IntPrimitive, count(*) from SupportBean group by IntPrimitive");
            stmtTwo.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {"G1", 10, 1L});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {"G2", 200, 1L});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {"G1", 10, 2L});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {"G1", 11, 1L});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {"G2", 200, 2L});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {"G2", 10, 1L});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionGroupByEventPerGroupBatchContextProp(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "@Name('context') create context SegmentedByString partition by TheString from SupportBean");

            string[] fieldsOne = "IntPrimitive,count(*)".Split(',');
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(
                "@Name('A') context SegmentedByString select IntPrimitive, count(*) from SupportBean#length_batch(2) group by IntPrimitive order by IntPrimitive asc");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fieldsOne, new object[] {10, 1L});
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[1], fieldsOne, new object[] {11, 1L});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {200, 2L});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fieldsOne, new object[] {10, 2L});
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[1], fieldsOne, new object[] {11, 0L});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fieldsOne, new object[] {10, 2L});
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[1], fieldsOne, new object[] {200, 0L});

            stmtOne.Dispose();

            // add "string" : add context property
            string[] fieldsTwo = "TheString,IntPrimitive,count(*)".Split(',');
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(
                "@Name('B') context SegmentedByString select TheString, IntPrimitive, count(*) from SupportBean#length_batch(2) group by IntPrimitive order by TheString, IntPrimitive asc");
            stmtTwo.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fieldsTwo, new object[] {"G1", 10, 1L});
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[1], fieldsTwo, new object[] {"G1", 11, 1L});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {"G2", 200, 2L});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fieldsTwo, new object[] {"G1", 10, 2L});
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[1], fieldsTwo, new object[] {"G1", 11, 0L});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fieldsTwo, new object[] {"G2", 10, 2L});
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[1], fieldsTwo, new object[] {"G2", 200, 0L});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionGroupByEventPerGroupWithAccess(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "@Name('context') create context SegmentedByString partition by TheString from SupportBean");

            string[] fieldsOne = "IntPrimitive,col1,col2,col3".Split(',');
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(
                "@Name('A') context SegmentedByString " +
                "select IntPrimitive, count(*) as col1, toArray(window(*).selectFrom(v=>v.LongPrimitive)) as col2, first().LongPrimitive as col3 " +
                "from SupportBean#keepall as sb " +
                "group by IntPrimitive order by IntPrimitive asc");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;

            epService.EPRuntime.SendEvent(MakeEvent("G1", 10, 200L));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {10, 1L, new object[] {200L}, 200L});

            epService.EPRuntime.SendEvent(MakeEvent("G1", 10, 300L));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {10, 2L, new object[] {200L, 300L}, 200L});

            epService.EPRuntime.SendEvent(MakeEvent("G2", 10, 1000L));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {10, 1L, new object[] {1000L}, 1000L});

            epService.EPRuntime.SendEvent(MakeEvent("G2", 10, 1010L));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsOne,
                new object[] {10, 2L, new object[] {1000L, 1010L}, 1000L});

            stmtOne.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionGroupByEventForAll(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "@Name('context') create context SegmentedByString partition by TheString from SupportBean");

            // test aggregation-only (no access)
            string[] fieldsOne = "col1".Split(',');
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(
                "@Name('A') context SegmentedByString " +
                "select sum(IntPrimitive) as col1 " +
                "from SupportBean");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {3});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {2});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {7});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {3});

            epService.EPRuntime.SendEvent(new SupportBean("G3", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {-1});

            stmtOne.Dispose();

            // test mixed with access
            string[] fieldsTwo = "col1,col2".Split(',');
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(
                "@Name('A') context SegmentedByString " +
                "select sum(IntPrimitive) as col1, toArray(window(*).selectFrom(v=>v.IntPrimitive)) as col2 " +
                "from SupportBean#keepall");
            stmtTwo.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("G1", 8));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {8, new object[] {8}});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 5));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {5, new object[] {5}});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {9, new object[] {8, 1}});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {7, new object[] {5, 2}});

            stmtTwo.Dispose();

            // test only access
            string[] fieldsThree = "col1".Split(',');
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(
                "@Name('A') context SegmentedByString " +
                "select toArray(window(*).selectFrom(v=>v.IntPrimitive)) as col1 " +
                "from SupportBean#keepall");
            stmtThree.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("G1", 8));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsThree, new object[] {new object[] {8}});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 5));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsThree, new object[] {new object[] {5}});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsThree, new object[] {new object[] {8, 1}});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsThree, new object[] {new object[] {5, 2}});

            stmtThree.Dispose();

            // test subscriber
            EPStatement stmtFour = epService.EPAdministrator.CreateEPL(
                "@Name('A') context SegmentedByString " +
                "select count(*) as col1 " +
                "from SupportBean");
            var subs = new SupportSubscriber();

            stmtFour.Subscriber = subs;

            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            Assert.AreEqual(1L, subs.AssertOneGetNewAndReset());

            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            Assert.AreEqual(2L, subs.AssertOneGetNewAndReset());

            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            Assert.AreEqual(1L, subs.AssertOneGetNewAndReset());

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionGroupByEventPerGroupUnidirectionalJoin(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "@Name('context') create context SegmentedByString partition by TheString from SupportBean");

            string[] fieldsOne = "IntPrimitive,col1".Split(',');
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(
                "@Name('A') context SegmentedByString " +
                "select IntPrimitive, count(*) as col1 " +
                "from SupportBean unidirectional, SupportBean_S0#keepall " +
                "group by IntPrimitive order by IntPrimitive asc");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {10, 2L});

            epService.EPRuntime.SendEvent(new SupportBean_S0(3));

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {10, 3L});

            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {20, 1L});

            epService.EPRuntime.SendEvent(new SupportBean_S0(5));

            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {20, 2L});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {10, 5L});

            stmtOne.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }

        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        public static Object ToArray(ICollection<int> input)
        {
            return input.ToArray();
        }

        public static Object ToArray(ICollection<long> input)
        {
            return input.ToArray();
        }
    }
} // end of namespace
