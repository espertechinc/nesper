///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestFilterExpressions 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
    
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportEvent", typeof(SupportTradeEvent));
            config.AddEventType("SupportBean", typeof(SupportBean));
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestPromoteIndexToSetNotIn()
        {
            var listenerOne = new SupportUpdateListener();
            var listenerTwo = new SupportUpdateListener();
            var eplOne = "select * from SupportBean(theString != 'x' and theString != 'y' and doubleBoxed is not null)";
            var eplTwo = "select * from SupportBean(theString != 'x' and theString != 'y' and longBoxed is not null)";

            _epService.EPAdministrator.CreateEPL(eplOne).AddListener(listenerOne);
            _epService.EPAdministrator.CreateEPL(eplTwo).AddListener(listenerTwo);

            var bean = new SupportBean("E1", 0);
            bean.DoubleBoxed = 1d;
            bean.LongBoxed = 1L;
            _epService.EPRuntime.SendEvent(bean);

            listenerOne.AssertOneGetNewAndReset();
            listenerTwo.AssertOneGetNewAndReset();
        }

        [Test]
        public void TestFilterInstanceMethodWWildcard() {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
    
            RunAssertionFilterInstanceMethod("select * from TestEvent(s0.MyInstanceMethodAlwaysTrue()) as s0", new bool[]{true, true, true});
            RunAssertionFilterInstanceMethod("select * from TestEvent(s0.MyInstanceMethodEventBean(s0, 'x', 1)) as s0", new bool[]{false, true, false});
            RunAssertionFilterInstanceMethod("select * from TestEvent(s0.MyInstanceMethodEventBean(*, 'x', 1)) as s0", new bool[]{false, true, false});
        }
    
        private void RunAssertionFilterInstanceMethod(String epl, bool[] expected) {
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            for (var i = 0; i < 3; i++) {
                _epService.EPRuntime.SendEvent(new TestEvent(i));
                Assert.AreEqual(expected[i], _listener.GetAndClearIsInvoked());
            }
            stmt.Dispose();
        }
    
        [Test]
        public void TestShortCircuitEvalAndOverspecified()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); } // not instrumented

            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            
    		var stmt = _epService.EPAdministrator.CreateEPL("select * from MyEvent(MyEvent.property2 = '4' and MyEvent.property1 = '1')");
    		stmt.Events += _listener.Update;
    
    		_epService.EPRuntime.SendEvent(new MyEvent());
            Assert.IsFalse(_listener.IsInvoked, "EPSubscriber should not have received Result(s)");
            stmt.Dispose();
    
    		stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString='A' and TheString='B')");
    		stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestRelationalOpConstantFirst() {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
    
            _epService.EPAdministrator.CreateEPL("@Name('A') select * from TestEvent where 4 < x").Events += _listener.Update;
            AssertSendReceive(new int[]{3, 4, 5}, new bool[]{false, false, true});
    
            _epService.EPAdministrator.GetStatement("A").Dispose();
            _epService.EPAdministrator.CreateEPL("@Name('A') select * from TestEvent where 4 <= x").Events += _listener.Update;
            AssertSendReceive(new int[]{3, 4, 5}, new bool[]{false, true, true});
    
            _epService.EPAdministrator.GetStatement("A").Dispose();
            _epService.EPAdministrator.CreateEPL("@Name('A') select * from TestEvent where 4 > x").Events += _listener.Update;
            AssertSendReceive(new int[]{3, 4, 5}, new bool[]{true, false, false});
    
            _epService.EPAdministrator.GetStatement("A").Dispose();
            _epService.EPAdministrator.CreateEPL("@Name('A') select * from TestEvent where 4 >= x").Events += _listener.Update;
            AssertSendReceive(new int[]{3, 4, 5}, new bool[]{true, true, false});
        }
    
        private void AssertSendReceive(int[] ints, bool[] booleans) {
            for (var i = 0; i < ints.Length; i++) {
                _epService.EPRuntime.SendEvent(new TestEvent(ints[i]));
                Assert.AreEqual(booleans[i], _listener.GetAndClearIsInvoked());
            }
        }
    
        [Test]
        public void TestInSet()
        {
            // Esper-484
            var start_load_type = new Dictionary<String, Object>();
            start_load_type.Put("versions", typeof(ICollection<string>));
            _epService.EPAdministrator.Configuration.AddEventType("StartLoad", start_load_type);
    
            var single_load_type = new Dictionary<String, Object>();
            single_load_type.Put("ver", typeof(String));
            _epService.EPAdministrator.Configuration.AddEventType("SingleLoad", single_load_type);
    
            _epService.EPAdministrator.CreateEPL(
                    "select * from \n" +
                            "pattern [ \n" +
                            " every start_load=StartLoad \n" +
                            " -> \n" +
                            " single_load=SingleLoad(ver in (start_load.versions)) \n" +
                            "]"
            ).Events += _listener.Update;
    
            var versions = new HashSet<String>();
            versions.Add("Version1");
            versions.Add("Version2");
    
            _epService.EPRuntime.SendEvent(Collections.SingletonDataMap("versions", versions), "StartLoad");
            _epService.EPRuntime.SendEvent(Collections.SingletonDataMap("ver", "Version1"), "SingleLoad");
            Assert.IsTrue(_listener.IsInvoked);
        }
    
        [Test]
        public void TestRewriteWhere() {
            var epl = "select * from SupportBean as A0 where A0.IntPrimitive = 3";
            var statement = _epService.EPAdministrator.CreateEPL(epl);
            statement.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
        }
    
        [Test]
        public void TestNullBooleanExpr()
        {
            var stmtOneText = "every event1=SupportEvent(userId like '123%')";
            var statement = _epService.EPAdministrator.CreatePattern(stmtOneText);
            statement.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportTradeEvent(1, null, 1001));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportTradeEvent(2, "1234", 1001));
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("event1.id"));
        }
    
        [Test]
        public void TestFilterOverInClause()
        {
            // Test for Esper-159
            var stmtOneText = "every event1=SupportEvent(userId in ('100','101'),amount>=1000)";
            var statement = _epService.EPAdministrator.CreatePattern(stmtOneText);
            statement.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportTradeEvent(1, "100", 1001));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("event1.id"));
    
            var stmtTwoText = "every event1=SupportEvent(userId in ('100','101'))";
            _epService.EPAdministrator.CreatePattern(stmtTwoText);
    
            _epService.EPRuntime.SendEvent(new SupportTradeEvent(2, "100", 1001));
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("event1.id"));
        }
    
        [Test]
        public void TestConstant()
        {
            var text = "select * from pattern [" +
                typeof(SupportBean).FullName + "(IntPrimitive=" + 
                typeof(ISupportAConst).FullName + ".VALUE_1)]";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            var theEvent = new SupportBean("e1", 2);
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            theEvent = new SupportBean("e1", 1);
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.IsTrue(_listener.IsInvoked);
        }
    
        [Test]
        public void TestEnumSyntaxOne()
        {
            var text = "select * from pattern [" +
                typeof(SupportBeanWithEnum).FullName + "(supportEnum=" +
                typeof(SupportEnumHelper).FullName + ".GetEnumFor('ENUM_VALUE_1'))]";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            var theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_1);
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.IsTrue(_listener.IsInvoked);
        }
    
        [Test]
        public void TestEnumSyntaxTwo()
        {
            var text = "select * from pattern [" +
                typeof(SupportBeanWithEnum).FullName + "(supportEnum=" +
                typeof(SupportEnum).FullName + ".ENUM_VALUE_2)]";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            var theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            theEvent = new SupportBeanWithEnum("e2", SupportEnum.ENUM_VALUE_1);
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(_listener.IsInvoked);
    
            stmt.Dispose();
    
            // test where clause
            text = "select * from " + typeof(SupportBeanWithEnum).FullName + " where supportEnum=" + typeof(SupportEnum).FullName + ".ENUM_VALUE_2";
            stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            theEvent = new SupportBeanWithEnum("e2", SupportEnum.ENUM_VALUE_1);
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestNotEqualsConsolidate()
        {
            TryNotEqualsConsolidate("IntPrimitive not in (1, 2)");
            TryNotEqualsConsolidate("IntPrimitive != 1, IntPrimitive != 2");
            TryNotEqualsConsolidate("IntPrimitive != 1 and IntPrimitive != 2");
        }
    
        private void TryNotEqualsConsolidate(String filter)
        {
            var text = "select * from " + typeof(SupportBean).FullName + "(" + filter + ")";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            for (var i = 0; i < 5; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("", i));
    
                if ((i == 1) || (i == 2))
                {
                    Assert.IsFalse(_listener.IsInvoked, "incorrect:" + i);
                }
                else
                {
                    Assert.IsTrue(_listener.IsInvoked, "incorrect:" + i);
                }
                _listener.Reset();
            }
    
            stmt.Dispose();
        }
    
        [Test]
        public void TestEqualsSemanticFilter()
        {
            // Test for Esper-114
            var text = "select * from " + typeof(SupportBeanComplexProps).FullName + "(nested=nested)";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            var eventOne = SupportBeanComplexProps.MakeDefaultBean();
            eventOne.SimpleProperty = "1";
    
            _epService.EPRuntime.SendEvent(eventOne);
            Assert.IsTrue(_listener.IsInvoked);
        }
    
        [Test]
        public void TestEqualsSemanticExpr()
        {
            // Test for Esper-114
            var text = "select * from " + typeof(SupportBeanComplexProps).FullName + "(simpleProperty='1').win:keepall() as s0" +
                    ", " + typeof(SupportBeanComplexProps).FullName + "(simpleProperty='2').win:keepall() as s1" +
                    " where s0.nested = s1.nested";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            var eventOne = SupportBeanComplexProps.MakeDefaultBean();
            eventOne.SimpleProperty = "1";
    
            var eventTwo = SupportBeanComplexProps.MakeDefaultBean();
            eventTwo.SimpleProperty = "2";
    
            Assert.AreEqual(eventOne.Nested, eventTwo.Nested);
    
            _epService.EPRuntime.SendEvent(eventOne);
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(eventTwo);
            Assert.IsTrue(_listener.IsInvoked);
        }
    
        [Test]
        public void TestNotEqualsNull() {
            var listeners = new SupportUpdateListener[6];
            for (var i = 0; i < listeners.Length; i++) {
                listeners[i] = new SupportUpdateListener();
            }
    
            // test equals&where-clause (can be optimized into filter)
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString != 'A'").Events += listeners[0].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString != 'A' or IntPrimitive != 0").Events += listeners[1].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString = 'A'").Events += listeners[2].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString = 'A' or IntPrimitive != 0").Events += listeners[3].Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            AssertListeners(listeners, new bool[] {false, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 1));
            AssertListeners(listeners, new bool[] {false, true, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            AssertListeners(listeners, new bool[] {false, false, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            AssertListeners(listeners, new bool[] {false, true, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("B", 0));
            AssertListeners(listeners, new bool[] {true, true, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            AssertListeners(listeners, new bool[] {true, true, false, true});
    
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test equals&selection
            var fields = "val0,val1,val2,val3,val4,val5".Split(',');
            _epService.EPAdministrator.CreateEPL("select " +
                    "TheString != 'A' as val0, " +
                    "TheString != 'A' or IntPrimitive != 0 as val1, " +
                    "TheString != 'A' and IntPrimitive != 0 as val2, " +
                    "TheString = 'A' as val3," +
                    "TheString = 'A' or IntPrimitive != 0 as val4, " +
                    "TheString = 'A' and IntPrimitive != 0 as val5 from SupportBean").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, false, null, null, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, true, null, null, true, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, false, false, true, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, true, false, true, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("B", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true, false, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true, true, false, true, false});
    
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test is-and-isnot&where-clause
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString is null").Events += listeners[0].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString is null or IntPrimitive != 0").Events += listeners[1].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString is not null").Events += listeners[2].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString is not null or IntPrimitive != 0").Events += listeners[3].Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            AssertListeners(listeners, new bool[] {true, true, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 1));
            AssertListeners(listeners, new bool[] {true, true, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            AssertListeners(listeners, new bool[] {false, false, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            AssertListeners(listeners, new bool[] {false, true, true, true});
    
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test is-and-isnot&selection
            _epService.EPAdministrator.CreateEPL("select " +
                    "TheString is null as val0, " +
                    "TheString is null or IntPrimitive != 0 as val1, " +
                    "TheString is null and IntPrimitive != 0 as val2, " +
                    "TheString is not null as val3," +
                    "TheString is not null or IntPrimitive != 0 as val4, " +
                    "TheString is not null and IntPrimitive != 0 as val5 " +
                    "from SupportBean").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true, false, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true, true, false, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, false, false, true, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, true, false, true, true, true});
    
            _epService.EPAdministrator.DestroyAllStatements();
    
            // filter expression
            _epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString is null)").Events += listeners[0].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString = null").Events += listeners[1].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString = null)").Events += listeners[2].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString is not null)").Events += listeners[3].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString != null").Events += listeners[4].Update;
            _epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString != null)").Events += listeners[5].Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            AssertListeners(listeners, new bool[] {true, false, false, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            AssertListeners(listeners, new bool[] {false, false, false, true, false, false});
    
            _epService.EPAdministrator.DestroyAllStatements();
    
            // select constants
            fields = "val0,val1,val2,val3".Split(',');
            _epService.EPAdministrator.CreateEPL("select " +
                    "2 != null as val0," +
                    "null = null as val1," +
                    "2 != null or 1 = 2 as val2," +
                    "2 != null and 2 = 2 as val3 " +
                    "from SupportBean").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, null});
    
            // test SODA
            var epl = "select IntBoxed is null, IntBoxed is not null, IntBoxed=1, IntBoxed!=1 from SupportBean";
            var stmt = SupportModelHelper.CompileCreate(_epService, epl);
            EPAssertionUtil.AssertEqualsExactOrder(new String[]{"IntBoxed is null", "IntBoxed is not null",
                    "IntBoxed=1", "IntBoxed!=1"}, stmt.EventType.PropertyNames);
        }
    
        [Test]
        public void TestPatternFunc3Stream()
        {
            String text;
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed=a.IntBoxed, IntBoxed=b.IntBoxed and IntBoxed != null)]";
            TryPattern3Stream(text, new int?[] {null, 2, 1, null,   8,  1,  2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 4, -2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 5, null}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                        new bool[] {false, false, false, false, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed is a.IntBoxed, IntBoxed is b.IntBoxed and IntBoxed is not null)]";
            TryPattern3Stream(text, new int?[] {null, 2, 1, null,   8,  1,  2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 4, -2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 5, null}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                        new bool[] {false, false, true, false, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed=a.IntBoxed or IntBoxed=b.IntBoxed)]";
            TryPattern3Stream(text, new int?[] {null, 2, 1, null,   8, 1, 2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 4, -2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 5, null}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                        new bool[] {false, true, true, true, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed=a.IntBoxed, IntBoxed=b.IntBoxed)]";
            TryPattern3Stream(text, new int?[] {null, 2, 1, null,   8,  1,  2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 4, -2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 5, null}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                        new bool[] {false, false, true, false, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed!=a.IntBoxed, IntBoxed!=b.IntBoxed)]";
            TryPattern3Stream(text, new int?[] {null, 2, 1, null,   8,  1,  2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 4, -2}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, 3, 1,    8, null, 5, null}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                        new bool[] {false, false, false, false, false, true, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed!=a.IntBoxed)]";
            TryPattern3Stream(text, new int?[] {2,    8,    null, 2, 1, null, 1}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {-2,   null, null, 3, 1,    8, 4}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, null, null, 3, 1,    8, 5}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                   new bool[] {false, false, false, true, false, false, true});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed is not a.IntBoxed)]";
            TryPattern3Stream(text, new int?[] {2,    8,    null, 2, 1, null, 1}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {-2,   null, null, 3, 1,    8, 4}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {null, null, null, 3, 1,    8, 5}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                   new bool[] {true, true, false, true, false, true, true});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed=a.IntBoxed, DoubleBoxed=b.DoubleBoxed)]";
            TryPattern3Stream(text, new int?[] {2, 2, 1, 2, 1, 7, 1}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {0, 0, 0, 0, 0, 0, 0}, new double?[] {1d, 2d, 0d, 2d, 0d, 1d, 0d},
                                    new int?[] {2, 2, 3, 2, 1, 7, 5}, new double?[] {1d, 1d, 1d, 2d, 1d, 1d, 1d},
                                   new bool[] {true, false, false, true, false, true, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed in (a.IntBoxed, b.IntBoxed))]";
            TryPattern3Stream(text, new int?[] {2,    1, 1,     null,   1, null,    1}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {1,    2, 1,     null, null,   2,    0}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {2,    2, 3,     null,   1, null,  null}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                               new bool[]   {true, true, false, false, true, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed in [a.IntBoxed:b.IntBoxed])]";
            TryPattern3Stream(text, new int?[] {2,    1, 1,     null,   1, null,    1}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {1,    2, 1,     null, null,   2,    0}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {2,    1, 3,     null,   1, null,  null}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                               new bool[]   {true, true, false, false, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed not in [a.IntBoxed:b.IntBoxed])]";
            TryPattern3Stream(text, new int?[] {2,    1, 1,     null,   1, null,    1}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {1,    2, 1,     null, null,   2,    0}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                                    new int?[] {2,    1, 3,     null,   1, null,  null}, new double?[] {0d, 0d, 0d, 0d, 0d, 0d, 0d},
                               new bool[]   {false, false, true, false, false, false, false});
        }
    
        [Test]
        public void TestPatternFunc()
        {
            String text;
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(IntBoxed = a.IntBoxed and DoubleBoxed = a.DoubleBoxed)]";
            TryPattern(text, new int?[] {null, 2, 1, null, 8, 1, 2}, new double?[] {2d, 2d, 2d, 1d, 5d, 6d, 7d},
                             new int?[] {null, 3, 1, 8, null, 1, 2}, new double?[] {2d, 3d, 2d, 1d, 5d, 6d, 8d},
                        new bool[] {false, false, true, false, false, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(IntBoxed is a.IntBoxed and DoubleBoxed = a.DoubleBoxed)]";
            TryPattern(text, new int?[] {null, 2, 1, null, 8, 1, 2}, new double?[] {2d, 2d, 2d, 1d, 5d, 6d, 7d},
                             new int?[] {null, 3, 1, 8, null, 1, 2}, new double?[] {2d, 3d, 2d, 1d, 5d, 6d, 8d},
                        new bool[] {true, false, true, false, false, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(a.DoubleBoxed = DoubleBoxed)]";
            TryPattern(text, new int?[] {0, 0}, new double?[] {2d, 2d},
                             new int?[] {0, 0}, new double?[] {2d, 3d},
                        new bool[] {true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(a.DoubleBoxed = b.DoubleBoxed)]";
            TryPattern(text, new int?[] {0, 0}, new double?[] {2d, 2d},
                             new int?[] {0, 0}, new double?[] {2d, 3d},
                        new bool[] {true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(a.DoubleBoxed != DoubleBoxed)]";
            TryPattern(text, new int?[] {0, 0}, new double?[] {2d, 2d},
                             new int?[] {0, 0}, new double?[] {2d, 3d},
                        new bool[] {false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(a.DoubleBoxed != b.DoubleBoxed)]";
            TryPattern(text, new int?[] {0, 0}, new double?[] {2d, 2d},
                             new int?[] {0, 0}, new double?[] {2d, 3d},
                        new bool[] {false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed in [a.DoubleBoxed:a.IntBoxed])]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {false, true, true, true, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed in (a.DoubleBoxed:a.IntBoxed])]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {false, false, true, true, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(b.DoubleBoxed in (a.DoubleBoxed:a.IntBoxed))]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {false, false, true, true, false, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed in [a.DoubleBoxed:a.IntBoxed))]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {false, true, true, true, false, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in [a.DoubleBoxed:a.IntBoxed])]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {true, false, false, false, false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in (a.DoubleBoxed:a.IntBoxed])]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {true, true, false, false, false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(b.DoubleBoxed not in (a.DoubleBoxed:a.IntBoxed))]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {true, true, false, false, true, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in [a.DoubleBoxed:a.IntBoxed))]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {true, false, false, false, true, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in (a.DoubleBoxed, a.IntBoxed, 9))]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {true, false, true, false, false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed in (a.DoubleBoxed, a.IntBoxed, 9))]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {false, true, false, true, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(b.DoubleBoxed in (DoubleBoxed, a.IntBoxed, 9))]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {true, true, true, true, true, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in (DoubleBoxed, a.IntBoxed, 9))]";
            TryPattern(text, new int?[] {1, 1, 1, 1, 1, 1}, new double?[] {10d, 10d, 10d, 10d, 10d, 10d},
                             new int?[] {0, 0, 0, 0, 0, 0}, new double?[] {0d, 1d, 2d, 9d, 10d, 11d},
                        new bool[] {false, false, false, false, false, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed = " + typeof(SupportStaticMethodLib).FullName + ".MinusOne(a.DoubleBoxed))]";
            TryPattern(text, new int?[] {0, 0, 0}, new double?[] {10d, 10d, 10d},
                             new int?[] {0, 0, 0}, new double?[] {9d, 10d, 11d, },
                        new bool[] {true, false, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed = " + typeof(SupportStaticMethodLib).FullName + ".MinusOne(a.DoubleBoxed) or " +
                        "DoubleBoxed = " + typeof(SupportStaticMethodLib).FullName + ".MinusOne(a.IntBoxed))]";
            TryPattern(text, new int?[] {0, 0, 12}, new double?[] {10d, 10d, 10d},
                             new int?[] {0, 0, 0}, new double?[] {9d, 10d, 11d, },
                        new bool[] {true, false, true});
        }
    
        private void TryPattern(String text,
                                int?[] intBoxedA,
                                double?[] doubleBoxedA,
                                int?[] intBoxedB,
                                double?[] doubleBoxedB,
                                bool[] expected)
        {
            Assert.AreEqual(intBoxedA.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedB.Length, doubleBoxedB.Length);
            Assert.AreEqual(expected.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedA.Length, doubleBoxedB.Length);
    
            for (var i = 0; i < intBoxedA.Length; i++)
            {
                var stmt = _epService.EPAdministrator.CreateEPL(text);
                stmt.Events += _listener.Update;
    
                SendBeanIntDouble(intBoxedA[i], doubleBoxedA[i]);
                SendBeanIntDouble(intBoxedB[i], doubleBoxedB[i]);
                Assert.AreEqual(expected[i], _listener.GetAndClearIsInvoked(), "failed at index " + i);
                stmt.Stop();
            }
        }
    
        private void TryPattern3Stream(String text,
                                       int?[] intBoxedA,
                                       double?[] doubleBoxedA,
                                       int?[] intBoxedB,
                                       double?[] doubleBoxedB,
                                       int?[] intBoxedC,
                                       double?[] doubleBoxedC,
                                       bool[] expected)
        {
            Assert.AreEqual(intBoxedA.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedB.Length, doubleBoxedB.Length);
            Assert.AreEqual(expected.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedA.Length, doubleBoxedB.Length);
            Assert.AreEqual(intBoxedC.Length, doubleBoxedC.Length);
            Assert.AreEqual(intBoxedB.Length, doubleBoxedC.Length);
    
            for (var i = 0; i < intBoxedA.Length; i++)
            {
                var stmt = _epService.EPAdministrator.CreateEPL(text);
                stmt.Events += _listener.Update;
    
                SendBeanIntDouble(intBoxedA[i], doubleBoxedA[i]);
                SendBeanIntDouble(intBoxedB[i], doubleBoxedB[i]);
                SendBeanIntDouble(intBoxedC[i], doubleBoxedC[i]);
                Assert.AreEqual(expected[i], _listener.GetAndClearIsInvoked(), "failed at index " + i);
                stmt.Stop();
            }
        }
    
        [Test]
        public void TestIn3ValuesAndNull()
        {
            String text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntPrimitive in (IntBoxed, DoubleBoxed))";
            Try3Fields(text, new int[]{1, 1, 1}, new int?[]{0, 1, 0}, new double?[]{2d, 2d, 1d}, new bool[]{false, true, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntPrimitive in (IntBoxed, " +
                typeof(SupportStaticMethodLib).FullName + ".MinusOne(DoubleBoxed)))";
            Try3Fields(text, new int[]{1, 1, 1}, new int?[]{0, 1, 0}, new double?[]{2d, 2d, 1d}, new bool[]{true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntPrimitive not in (IntBoxed, DoubleBoxed))";
            Try3Fields(text, new int[]{1, 1, 1}, new int?[]{0, 1, 0}, new double?[]{2d, 2d, 1d}, new bool[]{true, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed = DoubleBoxed)";
            Try3Fields(text, new int[]{1, 1, 1}, new int?[]{null, 1, null}, new double?[]{null, null, 1d}, new bool[]{false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (DoubleBoxed))";
            Try3Fields(text, new int[]{1, 1, 1}, new int?[]{null, 1, null}, new double?[]{null, null, 1d}, new bool[]{false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (DoubleBoxed))";
            Try3Fields(text, new int[]{1, 1, 1}, new int?[]{null, 1, null}, new double?[]{null, null, 1d}, new bool[]{false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [DoubleBoxed:10))";
            Try3Fields(text, new int[]{1, 1, 1}, new int?[]{null, 1, 2}, new double?[]{null, null, 1d}, new bool[]{false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in [DoubleBoxed:10))";
            Try3Fields(text, new int[]{1, 1, 1}, new int?[]{null, 1, 2}, new double?[]{null, null, 1d}, new bool[]{false, true, false});
        }
    
        [Test]
        public void TestFilterStaticFunc()
        {
            String text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(" +
                    typeof(SupportStaticMethodLib).FullName + ".IsStringEquals('b', TheString))";
            TryFilter(text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(" +
                    typeof(SupportStaticMethodLib).FullName + ".IsStringEquals('bx', TheString || 'x'))";
            TryFilter(text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "('b'=TheString," +
                    typeof(SupportStaticMethodLib).FullName + ".IsStringEquals('bx', TheString || 'x'))";
            TryFilter(text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "('b'=TheString, TheString='b', TheString != 'a')";
            TryFilter(text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(TheString != 'a', TheString != 'c')";
            TryFilter(text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(TheString = 'b', TheString != 'c')";
            TryFilter(text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(TheString != 'a' and TheString != 'c')";
            TryFilter(text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(TheString = 'a' and TheString = 'c' and " +
                    typeof(SupportStaticMethodLib).FullName + ".IsStringEquals('bx', TheString || 'x'))";
            TryFilter(text, false);
        }
    
        [Test]
        public void TestFilterRelationalOpRange()
        {
            String text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [2:3])";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [2:3] and IntBoxed in [2:3])";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [2:3] and IntBoxed in [2:2])";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, true, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [1:10] and IntBoxed in [3:2])";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [3:3] and IntBoxed in [1:3])";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, false, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [3:3] and IntBoxed in [1:3] and IntBoxed in [4:5])";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in [3:3] and IntBoxed not in [1:3])";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (2:4) and IntBoxed not in (1:3))";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {true, false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in [2:4) and IntBoxed not in [1:3))";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (2:4] and IntBoxed not in (1:3])";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {true, false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed not in (2:4)";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {true, true, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed not in [2:4]";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {true, false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed not in [2:4)";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {true, false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed not in (2:4]";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {true, true, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed in (2:4)";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, false, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed in [2:4]";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, true, true, true});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed in [2:4)";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed in (2:4]";
            TryFilterRelationalOpRange(text, new int[] {1, 2, 3, 4}, new bool[] {false, false, true, true});
        }
    
        private void TryFilterRelationalOpRange(String text, int[] testData, bool[] isReceived)
        {
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            Assert.AreEqual(testData.Length,  isReceived.Length);
            for (var i = 0; i < testData.Length; i++)
            {
                SendBeanIntDouble(testData[i], 0D);
                Assert.AreEqual(isReceived[i], _listener.GetAndClearIsInvoked(), "failed testing index " + i);
            }
            stmt.Events -= _listener.Update;
        }
    
        private void TryFilter(String text, bool isReceived)
        {
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            SendBeanString("a");
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
            SendBeanString("b");
            Assert.AreEqual(isReceived, _listener.GetAndClearIsInvoked());
            SendBeanString("c");
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            stmt.Events -= _listener.Update;
        }
    
        private void Try3Fields(String text,
                                int[] intPrimitive,
                                int?[] intBoxed,
                                double?[] doubleBoxed,
                                bool[] expected)
        {
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            Assert.AreEqual(intPrimitive.Length, doubleBoxed.Length);
            Assert.AreEqual(intBoxed.Length, doubleBoxed.Length);
            Assert.AreEqual(expected.Length, doubleBoxed.Length);
            for (var i = 0; i < intBoxed.Length; i++)
            {
                SendBeanIntIntDouble(intPrimitive[i], intBoxed[i], doubleBoxed[i]);
                Assert.AreEqual(expected[i], _listener.GetAndClearIsInvoked(), "failed at index " + i);
            }
    
            stmt.Stop();
        }
    
        [Test]
        public void TestFilterBooleanExpr()
        {
            var text = "select * from " + typeof(SupportBean).FullName + "(2*IntBoxed=DoubleBoxed)";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            SendBeanIntDouble(20, 50d);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
            SendBeanIntDouble(25, 50d);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            text = "select * from " + typeof(SupportBean).FullName + "(2*IntBoxed=DoubleBoxed, TheString='s')";
            stmt = _epService.EPAdministrator.CreateEPL(text);
            var listenerTwo = new SupportUpdateListener();
            stmt.Events += listenerTwo.Update;
    
            SendBeanIntDoubleString(25, 50d, "s");
            Assert.IsTrue(listenerTwo.GetAndClearIsInvoked());
            SendBeanIntDoubleString(25, 50d, "x");
            Assert.IsFalse(listenerTwo.GetAndClearIsInvoked());
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test priority of equals and boolean
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib));
            _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 1 or IntPrimitive = 2)");
            _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 3, SupportStaticMethodLib.AlwaysTrue({}))");
    
            SupportStaticMethodLib.GetInvocations().Clear();
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(SupportStaticMethodLib.GetInvocations().IsEmpty());
        }
    
        [Test]
        public void TestFilterWithEqualsSameCompare()
        {
            String text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed=DoubleBoxed)";
            TryFilterWithEqualsSameCompare(text, new int[] {1, 1}, new double[] {1, 10}, new bool[] {true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed=IntBoxed and DoubleBoxed=DoubleBoxed)";
            TryFilterWithEqualsSameCompare(text, new int[] {1, 1}, new double[] {1, 10}, new bool[] {true, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(DoubleBoxed=IntBoxed)";
            TryFilterWithEqualsSameCompare(text, new int[] {1, 1}, new double[] {1, 10}, new bool[] {true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(DoubleBoxed in (IntBoxed))";
            TryFilterWithEqualsSameCompare(text, new int[] {1, 1}, new double[] {1, 10}, new bool[] {true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (DoubleBoxed))";
            TryFilterWithEqualsSameCompare(text, new int[] {1, 1}, new double[] {1, 10}, new bool[] {true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(DoubleBoxed not in (10, IntBoxed))";
            TryFilterWithEqualsSameCompare(text, new int[] {1, 1, 1}, new double[] {1, 5, 10}, new bool[] {false, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(DoubleBoxed in (IntBoxed:20))";
            TryFilterWithEqualsSameCompare(text, new int[] {0, 1, 2}, new double[] {1, 1, 1}, new bool[] {true, false, false});
        }
    
        private void TryFilterWithEqualsSameCompare(String text, int[] intBoxed, double[] doubleBoxed, bool[] expected)
        {
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            Assert.AreEqual(intBoxed.Length, doubleBoxed.Length);
            Assert.AreEqual(expected.Length, doubleBoxed.Length);
            for (var i = 0; i < intBoxed.Length; i++)
            {
                SendBeanIntDouble(intBoxed[i], doubleBoxed[i]);
                Assert.AreEqual(expected[i], _listener.GetAndClearIsInvoked(), "failed at index " + i);
            }
    
            stmt.Stop();
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalid("select * from pattern [every a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportMarketDataBean).FullName + "(sum(a.LongBoxed) = 2)]",
                    "Aggregation functions not allowed within filters [select * from pattern [every a=com.espertech.esper.support.bean.SupportBean -> b=com.espertech.esper.support.bean.SupportMarketDataBean(sum(a.LongBoxed) = 2)]]");
    
            TryInvalid("select * from pattern [every a=" + typeof(SupportBean).FullName + "(prior(1, a.LongBoxed))]",
                    "Failed to validate filter expression 'prior(1,a.LongBoxed)': Prior function cannot be used in this context [select * from pattern [every a=com.espertech.esper.support.bean.SupportBean(prior(1, a.LongBoxed))]]");
    
            TryInvalid("select * from pattern [every a=" + typeof(SupportBean).FullName + "(prev(1, a.LongBoxed))]",
                    "Failed to validate filter expression 'prev(1,a.LongBoxed)': Previous function cannot be used in this context [select * from pattern [every a=com.espertech.esper.support.bean.SupportBean(prev(1, a.LongBoxed))]]");
    
            TryInvalid("select * from " + typeof(SupportBean).FullName + "(5 - 10)",
                    "Filter expression not returning a boolean value: '5-10' [select * from com.espertech.esper.support.bean.SupportBean(5 - 10)]");
    
            TryInvalid("select * from " + typeof(SupportBeanWithEnum).FullName + "(TheString=" + typeof(SupportEnum).FullName + ".ENUM_VALUE_1)",
                    "Failed to validate filter expression 'TheString=ENUM_VALUE_1': Implicit conversion from datatype '" + Name.Of<SupportEnum>() + "' to 'System.String' is not allowed [select * from com.espertech.esper.support.bean.SupportBeanWithEnum(TheString=com.espertech.esper.support.bean.SupportEnum.ENUM_VALUE_1)]");
    
            TryInvalid("select * from " + typeof(SupportBeanWithEnum).FullName + "(supportEnum=A.b)",
                    "Failed to validate filter expression 'supportEnum=A.b': Failed to resolve property 'A.b' to a stream or nested property in a stream [select * from com.espertech.esper.support.bean.SupportBeanWithEnum(supportEnum=A.b)]");
    
            TryInvalid("select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in (DoubleBoxed, x.IntBoxed, 9))]",
                    "Failed to validate filter expression 'DoubleBoxed not in (DoubleBoxed,x.I...(45 chars)': Failed to find a stream named 'x' (did you mean 'b'?) [select * from pattern [a=com.espertech.esper.support.bean.SupportBean -> b=com.espertech.esper.support.bean.SupportBean(DoubleBoxed not in (DoubleBoxed, x.IntBoxed, 9))]]");
    
            TryInvalid("select * from pattern [a=" + typeof(SupportBean).FullName
                    + " -> b=" + typeof(SupportBean).FullName + "(cluedo.IntPrimitive=a.IntPrimitive)"
                    + " -> c=" + typeof(SupportBean).FullName
                    + "]",
                    "Failed to validate filter expression 'cluedo.IntPrimitive=a.IntPrimitive': Failed to resolve property 'cluedo.IntPrimitive' to a stream or nested property in a stream [select * from pattern [a=com.espertech.esper.support.bean.SupportBean -> b=com.espertech.esper.support.bean.SupportBean(cluedo.IntPrimitive=a.IntPrimitive) -> c=com.espertech.esper.support.bean.SupportBean]]");
        }
    
        private void TryInvalid(String text, String expectedMsg)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(text);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(expectedMsg, ex.Message);
            }
        }
    
        [Test]
        public void TestPatternWithExpr()
        {
            var text = "select * from pattern [every a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportMarketDataBean).FullName + "(a.LongBoxed=volume*2)]";
            TryPatternWithExpr(text);
    
            text = "select * from pattern [every a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportMarketDataBean).FullName + "(volume*2=a.LongBoxed)]";
            TryPatternWithExpr(text);
        }
    
        private void TryPatternWithExpr(String text)
        {
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            SendBeanLong(10L);
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 0L, ""));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 5L, ""));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            SendBeanLong(0L);
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 0L, ""));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 1L, ""));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendBeanLong(20L);
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 10L, ""));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            stmt.RemoveAllEventHandlers();
        }
    
        [Test]
        public void TestMathExpression()
        {
            String text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed*DoubleBoxed > 20)";
            TryArithmatic(text);
    
            text = "select * from " + typeof(SupportBean).FullName + "(20 < IntBoxed*DoubleBoxed)";
            TryArithmatic(text);
    
            text = "select * from " + typeof(SupportBean).FullName + "(20/IntBoxed < DoubleBoxed)";
            TryArithmatic(text);
    
            text = "select * from " + typeof(SupportBean).FullName + "(20/IntBoxed/DoubleBoxed < 1)";
            TryArithmatic(text);
        }
    
        [Test]
        public void TestShortAndByteArithmetic()
        {
            var epl = "select " +
                    "ShortPrimitive + ShortBoxed as c0," +
                    "BytePrimitive + ByteBoxed as c1, " +
                    "ShortPrimitive - ShortBoxed as c2," +
                    "BytePrimitive - ByteBoxed as c3, " +
                    "ShortPrimitive * ShortBoxed as c4," +
                    "BytePrimitive * ByteBoxed as c5, " +
                    "ShortPrimitive / ShortBoxed as c6," +
                    "BytePrimitive / ByteBoxed as c7," +
                    "ShortPrimitive + LongPrimitive as c8," +
                    "BytePrimitive + LongPrimitive as c9 " +
                    "from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9".Split(',');
    
            foreach (var field in fields) {
                var expected = typeof(int?);
                if (field.Equals("c6") || field.Equals("c7")) {
                    expected = typeof(double?);
                }
                if (field.Equals("c8") || field.Equals("c9")) {
                    expected = typeof(long?);
                }
                Assert.AreEqual(expected, stmt.EventType.GetPropertyType(field), "for field " + field);
            }
    
            var bean = new SupportBean();
            bean.ShortPrimitive = (short)5;
            bean.ShortBoxed = (short)6;
            bean.BytePrimitive = (byte)4;
            bean.ByteBoxed = (byte)2;
            bean.LongPrimitive = 10;
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] {11, 6, -1, 2, 30, 8, 5d/6d, 2d, 15L, 14L});
        }
    
        private void TryArithmatic(String text)
        {
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            SendBeanIntDouble(5, 5d);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            SendBeanIntDouble(5, 4d);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendBeanIntDouble(5, 4.001d);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        [Test]
        public void TestExpressionReversed()
        {
            var expr = "select * from " + typeof(SupportBean).FullName + "(5 = IntBoxed)";
            var stmt = _epService.EPAdministrator.CreateEPL(expr);
            stmt.Events += _listener.Update;
    
            SendBean("IntBoxed", 5);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
        }

        private void SendBeanIntDouble(int? intBoxed, double? doubleBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.IntBoxed = intBoxed;
            theEvent.DoubleBoxed = doubleBoxed;
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendBeanIntDoubleString(int? intBoxed, double? doubleBoxed, String theString)
        {
            var theEvent = new SupportBean();
            theEvent.IntBoxed = intBoxed;
            theEvent.DoubleBoxed = doubleBoxed;
            theEvent.TheString = theString;
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendBeanIntIntDouble(int intPrimitive, int? intBoxed, double? doubleBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.IntBoxed = intBoxed;
            theEvent.DoubleBoxed = doubleBoxed;
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendBeanLong(long? longBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.LongBoxed = longBoxed;
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendBeanString(String theString)
        {
            var num = new SupportBean(theString, -1);
            _epService.EPRuntime.SendEvent(num);
        }
    
        private void SendBean(String fieldName, Object value)
        {
            var theEvent = new SupportBean();
            if (fieldName.Equals("TheString"))
            {
                theEvent.TheString = ((String) value);
            }
            else if (fieldName.Equals("BoolPrimitive"))
            {
                theEvent.BoolPrimitive = ((bool) value);
            }
            else if (fieldName.Equals("IntBoxed"))
            {
                theEvent.IntBoxed = ((int?) value);
            }
            else if (fieldName.Equals("LongBoxed"))
            {
                theEvent.LongBoxed = ((long?) value);
            }
            else
            {
                throw new ArgumentException("field name not known");
            }
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void AssertListeners(SupportUpdateListener[] listeners, bool[] invoked)
        {
            for (var i = 0; i < invoked.Length; i++)
            {
                Assert.AreEqual(invoked[i], listeners[i].GetAndClearIsInvoked(), "Failed for listener " + i);
            }
        }

        public class MyEvent
        {
            public String GetProperty1()
            {
                throw new Exception("I should not have been called!");
            }
    
            public String GetProperty2()
            {
                return "2";
            }
        }
    
        public class TestEvent
        {
            public TestEvent(int x)
            {
                X = x;
            }

            public int X { get; private set; }

            public bool MyInstanceMethodAlwaysTrue() {
                return true;
            }
    
            public bool MyInstanceMethodEventBean(EventBean @event, String propertyName, int expected) {
                var value = @event.Get(propertyName);
                return value.Equals(expected);
            }
        }
    }
}
