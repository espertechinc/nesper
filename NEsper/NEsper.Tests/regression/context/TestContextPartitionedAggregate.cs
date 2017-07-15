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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.client;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextPartitionedAggregate
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
            configuration.AddPlugInSingleRowFunction("toArray", GetType().FullName, "ToArray");
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestAccessOnly()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            var eplContext = "@Name('CTX') create context SegmentedByString partition by TheString from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplContext);
    
            var fieldsGrouped = "TheString,IntPrimitive,col1".Split(',');
            var eplGroupedAccess = "@Name('S2') context SegmentedByString select TheString,IntPrimitive,window(LongPrimitive) as col1 from SupportBean.win:keepall() sb group by IntPrimitive";
            _epService.EPAdministrator.CreateEPL(eplGroupedAccess);
            _epService.EPAdministrator.GetStatement("S2").Events += _listener.Update;

            _epService.EPRuntime.SendEvent(MakeEvent("G1", 1, 10L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsGrouped, new Object[]{"G1", 1, new Object[]{10L}});

            _epService.EPRuntime.SendEvent(MakeEvent("G1", 2, 100L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsGrouped, new Object[]{"G1", 2, new Object[]{100L}});

            _epService.EPRuntime.SendEvent(MakeEvent("G2", 1, 200L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsGrouped, new Object[]{"G2", 1, new Object[]{200L}});

            _epService.EPRuntime.SendEvent(MakeEvent("G1", 1, 11L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsGrouped, new Object[]{"G1", 1, new Object[]{10L, 11L}});
        }
    
        [Test]
        public void TestSegmentedSubqueryWithAggregation()
        {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fields = new String[] {"TheString", "IntPrimitive", "val0"};
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select TheString, IntPrimitive, (select count(*) from SupportBean_S0.win:keepall() as s0 where sb.IntPrimitive = s0.Id) as val0 " +
                    "from SupportBean as sb");
            stmtOne.AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "s1"));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "G1", 10, 0L });
        }
    
        [Test]
        public void TestGroupByEventPerGroupStream()
        {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fieldsOne = "IntPrimitive,count(*)".Split(',');
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString select IntPrimitive, Count(*) from SupportBean group by IntPrimitive");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{200, 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{11, 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{200, 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 1L});
    
            stmtOne.Dispose();
    
            // add "string" : a context property
            var fieldsTwo = "TheString,IntPrimitive,count(*)".Split(',');
            var stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('B') context SegmentedByString select TheString, IntPrimitive, Count(*) from SupportBean group by IntPrimitive");
            stmtTwo.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{"G1", 10, 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{"G2", 200, 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{"G1", 10, 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{"G1", 11, 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{"G2", 200, 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{"G2", 10, 1L});
        }
    
        [Test]
        public void TestGroupByEventPerGroupBatchContextProp()
        {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fieldsOne = "IntPrimitive,count(*)".Split(',');
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString select IntPrimitive, Count(*) from SupportBean.win:length_batch(2) group by IntPrimitive order by IntPrimitive asc");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            Assert.IsFalse(_listener.IsInvoked);
            
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fieldsOne, new Object[]{10, 1L});
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[1], fieldsOne, new Object[]{11, 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{200, 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fieldsOne, new Object[]{10, 2L});
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[1], fieldsOne, new Object[]{11, 0L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fieldsOne, new Object[]{10, 2L});
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[1], fieldsOne, new Object[]{200, 0L});
    
            stmtOne.Dispose();
    
            // add "string" : add context property
            var fieldsTwo = "TheString,IntPrimitive,count(*)".Split(',');
            var stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('B') context SegmentedByString select TheString, IntPrimitive, Count(*) from SupportBean.win:length_batch(2) group by IntPrimitive order by TheString, IntPrimitive asc");
            stmtTwo.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fieldsTwo, new Object[]{"G1", 10, 1L});
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[1], fieldsTwo, new Object[]{"G1", 11, 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{"G2", 200, 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fieldsTwo, new Object[]{"G1", 10, 2L});
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[1], fieldsTwo, new Object[]{"G1", 11, 0L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fieldsTwo, new Object[]{"G2", 10, 2L});
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[1], fieldsTwo, new Object[]{"G2", 200, 0L});
        }
    
        [Test]
        public void TestGroupByEventPerGroupWithAccess()
        {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fieldsOne = "IntPrimitive,col1,col2,col3".Split(',');
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select IntPrimitive, Count(*) as col1, ToArray(window(*).SelectFrom(v=>v.LongPrimitive)) as col2, First().LongPrimitive as col3 " +
                    "from SupportBean.win:keepall() as sb " +
                    "group by IntPrimitive order by IntPrimitive asc");
            stmtOne.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(MakeEvent("G1", 10, 200L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 1L, new Object[]{200L}, 200L});

            _epService.EPRuntime.SendEvent(MakeEvent("G1", 10, 300L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 2L, new Object[]{200L, 300L}, 200L});

            _epService.EPRuntime.SendEvent(MakeEvent("G2", 10, 1000L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 1L, new Object[]{1000L}, 1000L});
    
            _epService.EPRuntime.SendEvent(MakeEvent("G2", 10, 1010L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 2L, new Object[]{1000L, 1010L}, 1000L});
    
            stmtOne.Dispose();
        }
    
        [Test]
        public void TestGroupByEventForAll()
        {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            // test aggregation-only (no access)
            var fieldsOne = "col1".Split(',');
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select Sum(IntPrimitive) as col1 " +
                    "from SupportBean");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{3});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{2});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 4));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{7});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{3});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G3", -1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{-1});
    
            stmtOne.Dispose();
    
            // test mixed with access
            var fieldsTwo = "col1,col2".Split(',');
            var stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select Sum(IntPrimitive) as col1, ToArray(window(*).SelectFrom(v=>v.IntPrimitive)) as col2 " +
                    "from SupportBean.win:keepall()");
            stmtTwo.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 8));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{8, new Object[]{8}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 5));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{5, new Object[]{5}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{9, new Object[]{8, 1}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[]{7, new Object[]{5, 2}});
    
            stmtTwo.Dispose();
    
            // test only access
            var fieldsThree = "col1".Split(',');
            var stmtThree = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select ToArray(window(*).SelectFrom(v=>v.IntPrimitive)) as col1 " +
                    "from SupportBean.win:keepall()");
            stmtThree.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 8));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsThree, new Object[]{new Object[]{8}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 5));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsThree, new Object[]{new Object[]{5}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsThree, new Object[]{new Object[]{8, 1}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsThree, new Object[]{new Object[]{5, 2}});
    
            stmtThree.Dispose();
    
            // test subscriber
            var stmtFour = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select Count(*) as col1 " +
                    "from SupportBean");
            var subs = new SupportSubscriber();
            stmtFour.Subscriber = subs;
            
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            Assert.AreEqual(1L, subs.AssertOneGetNewAndReset());
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            Assert.AreEqual(2L, subs.AssertOneGetNewAndReset());
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            Assert.AreEqual(1L, subs.AssertOneGetNewAndReset());
        }
    
        [Test]
        public void TestGroupByEventPerGroupUnidirectionalJoin()
        {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var fieldsOne = "IntPrimitive,col1".Split(',');
            var stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context SegmentedByString " +
                    "select IntPrimitive, Count(*) as col1 " +
                    "from SupportBean unidirectional, SupportBean_S0.win:keepall() " +
                    "group by IntPrimitive order by IntPrimitive asc");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 3L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{20, 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5));
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{20, 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[]{10, 5L});
    
            stmtOne.Dispose();
        }
    
        private SupportBean MakeEvent(String theString, int intPrimitive, long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        public static Object ToArray(ICollection<object> input)
        {
            return input.ToArray();
        }
    }
}
