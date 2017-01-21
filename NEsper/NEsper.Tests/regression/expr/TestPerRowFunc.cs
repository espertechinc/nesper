///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestPerRowFunc
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;

        private void TryCoalesceBeans(String viewExpr)
        {
            _epService.Initialize();
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;

            SupportBean theEvent = SendEvent("s0");
            EventBean eventReceived = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual("s0", eventReceived.Get("MyString"));
            Assert.AreSame(theEvent, eventReceived.Get("myBean"));

            theEvent = SendEvent("s1");
            eventReceived = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual("s1", eventReceived.Get("MyString"));
            Assert.AreSame(theEvent, eventReceived.Get("myBean"));
        }

        private void RunCoalesceLong()
        {
            SendEvent(1L, 2, 3);
            Assert.AreEqual(1L, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendBoxedEvent(null, 2, null);
            Assert.AreEqual(2L, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendBoxedEvent(null, null, short.Parse("3"));
            Assert.AreEqual(3L, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendBoxedEvent(null, null, null);
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("result"));
        }

        private EPStatement SetupCoalesce(String coalesceExpr)
        {
            _epService.Initialize();
            String viewExpr = "select " + coalesceExpr + " as result" +
                              " from " + typeof (SupportBean).FullName + ".win:length(1000) ";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
            return selectTestView;
        }

        private void TryCoalesceInvalid(String coalesceExpr)
        {
            String viewExpr = "select " + coalesceExpr + " as result" +
                              " from " + typeof (SupportBean).FullName + ".win:length(3) ";

            try
            {
                _epService.EPAdministrator.CreateEPL(viewExpr);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                // expected
            }
        }

        private void RunMinMaxWindowStats()
        {
            SendEvent(10, 20, 4);
            EventBean received = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(20L, received.Get("myMax"));
            Assert.AreEqual(10L, received.Get("myMin"));
            Assert.AreEqual(4L, received.Get("myMinEx"));
            Assert.AreEqual(20L, received.Get("myMaxEx"));

            SendEvent(-10, -20, -30);
            received = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(-10L, received.Get("myMax"));
            Assert.AreEqual(-20L, received.Get("myMin"));
            Assert.AreEqual(-30L, received.Get("myMinEx"));
            Assert.AreEqual(-10L, received.Get("myMaxEx"));
        }

        private EPStatement SetUpMinMax()
        {
            String viewExpr = "select max(LongBoxed, IntBoxed) as myMax, " +
                              "max(LongBoxed, IntBoxed, ShortBoxed) as myMaxEx," +
                              "min(LongBoxed, IntBoxed) as myMin," +
                              "min(LongBoxed, IntBoxed, ShortBoxed) as myMinEx" +
                              " from " + typeof (SupportBean).FullName + ".win:length(3) ";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
            return selectTestView;
        }

        private SupportBean SendEvent(String stringValue)
        {
            var bean = new SupportBean();
            bean.TheString = stringValue;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private void SendEvent(long longBoxed, int intBoxed, short shortBoxed)
        {
            SendBoxedEvent(longBoxed, intBoxed, shortBoxed);
        }

        private void SendBoxedEvent(long? longBoxed, int? intBoxed, short? shortBoxed)
        {
            var bean = new SupportBean();
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEventWithDouble(byte? byteBoxed,
                                         short? shortBoxed,
                                         int? intBoxed,
                                         long? longBoxed,
                                         float? floatBoxed,
                                         double? doubleBoxed)
        {
            var bean = new SupportBean();
            bean.ByteBoxed = byteBoxed;
            bean.ShortBoxed = shortBoxed;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.FloatBoxed = floatBoxed;
            bean.DoubleBoxed = doubleBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void AssertConcat(String c1, String c2, String c3)
        {
            EventBean theEvent = _testListener.LastNewData[0];
            Assert.AreEqual(c1, theEvent.Get("c1"));
            Assert.AreEqual(c2, theEvent.Get("c2"));
            Assert.AreEqual(c3, theEvent.Get("c3"));
            _testListener.Reset();
        }

        [Test]
        public void TestCoalesceBeans()
        {
            TryCoalesceBeans("select coalesce(a.TheString, b.TheString) as MyString, coalesce(a, b) as myBean" +
                             " from pattern [every (a=" + typeof (SupportBean).FullName + "(TheString='s0') or b=" +
                             typeof (SupportBean).FullName + "(TheString='s1'))]");

            TryCoalesceBeans("SELECT COALESCE(a.TheString, b.TheString) AS MyString, COALESCE(a, b) AS myBean" +
                             " FROM PATTERN [EVERY (a=" + typeof (SupportBean).FullName + "(TheString='s0') OR b=" +
                             typeof (SupportBean).FullName + "(TheString='s1'))]");
        }

        [Test]
        public void TestCoalesceDouble()
        {
            EPStatement selectTestView =
                SetupCoalesce("coalesce(null, ByteBoxed, ShortBoxed, IntBoxed, LongBoxed, FloatBoxed, DoubleBoxed)");
            Assert.AreEqual(typeof (double?), selectTestView.EventType.GetPropertyType("result"));

            SendEventWithDouble(null, null, null, null, null, null);
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendEventWithDouble(null, short.Parse("2"), null, null, null, 1d);
            Assert.AreEqual(2d, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendEventWithDouble(null, null, null, null, null, 100d);
            Assert.AreEqual(100d, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendEventWithDouble(null, null, null, null, 10f, 100d);
            Assert.AreEqual(10d, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendEventWithDouble(null, null, 1, 5l, 10f, 100d);
            Assert.AreEqual(1d, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendEventWithDouble(Byte.Parse("3"), null, null, null, null, null);
            Assert.AreEqual(3d, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendEventWithDouble(null, null, null, 5l, 10f, 100d);
            Assert.AreEqual(5d, _testListener.AssertOneGetNewAndReset().Get("result"));
        }

        [Test]
        public void TestCoalesceInvalid()
        {
            String viewExpr = "select coalesce(null, null) as result" +
                              " from " + typeof (SupportBean).FullName + ".win:length(3) ";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            Assert.AreEqual(null, selectTestView.EventType.GetPropertyType("result"));

            TryCoalesceInvalid("coalesce(IntPrimitive)");
            TryCoalesceInvalid("coalesce(IntPrimitive, String)");
            TryCoalesceInvalid("coalesce(IntPrimitive, xxx)");
            TryCoalesceInvalid("coalesce(IntPrimitive, BooleanBoxed)");
            TryCoalesceInvalid("coalesce(CharPrimitive, LongBoxed)");
            TryCoalesceInvalid("coalesce(CharPrimitive, TheString, String)");
            TryCoalesceInvalid("coalesce(TheString, LongBoxed)");
            TryCoalesceInvalid("coalesce(null, LongBoxed, String)");
            TryCoalesceInvalid("coalesce(null, null, BoolBoxed, 1l)");
        }

        [Test]
        public void TestCoalesceLong()
        {
            EPStatement selectTestView = SetupCoalesce("coalesce(LongBoxed, IntBoxed, ShortBoxed)");
            Assert.AreEqual(typeof (long?), selectTestView.EventType.GetPropertyType("result"));

            SendEvent(1L, 2, 3);
            Assert.AreEqual(1L, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendBoxedEvent(null, 2, null);
            Assert.AreEqual(2L, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendBoxedEvent(null, null, short.Parse("3"));
            Assert.AreEqual(3L, _testListener.AssertOneGetNewAndReset().Get("result"));

            SendBoxedEvent(null, null, null);
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("result"));
        }

        [Test]
        public void TestCoalesceLong_Compile()
        {
            String viewExpr = "select coalesce(LongBoxed,IntBoxed,ShortBoxed) as result" +
                              " from " + typeof (SupportBean).FullName + ".win:length(1000)";

            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(viewExpr);
            Assert.AreEqual(viewExpr, model.ToEPL());

            _epService.Initialize();
            EPStatement selectTestView = _epService.EPAdministrator.Create(model);
            selectTestView.Events += _testListener.Update;
            Assert.AreEqual(typeof (long?), selectTestView.EventType.GetPropertyType("result"));

            RunCoalesceLong();
        }

        [Test]
        public void TestCoalesceLong_OM()
        {
            String viewExpr = "select coalesce(LongBoxed,IntBoxed,ShortBoxed) as result" +
                              " from " + typeof (SupportBean).FullName + ".win:length(1000)";

            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.Coalesce(
                "LongBoxed", "IntBoxed", "ShortBoxed"), "result");
            model.FromClause = FromClause
                .Create(FilterStream.Create(typeof (SupportBean).FullName)
                    .AddView("win", "length", Expressions.Constant(1000)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(viewExpr, model.ToEPL());

            _epService.Initialize();
            EPStatement selectTestView = _epService.EPAdministrator.Create(model);
            selectTestView.Events += _testListener.Update;
            Assert.AreEqual(typeof (long?), selectTestView.EventType.GetPropertyType("result"));

            RunCoalesceLong();
        }

        [Test]
        public void TestConcat()
        {
            String viewExpr = "select p00 || p01 as c1, p00 || p01 || p02 as c2, p00 || '|' || p01 as c3" +
                              " from " + typeof (SupportBean_S0).FullName + ".win:length(10)";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a", "b", "c"));
            AssertConcat("ab", "abc", "a|b");

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, null, "b", "c"));
            AssertConcat(null, null, null);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "", "b", "c"));
            AssertConcat("b", "bc", "|b");

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "123", null, "c"));
            AssertConcat(null, null, null);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "123", "456", "c"));
            AssertConcat("123456", "123456c", "123|456");

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "123", "456", null));
            AssertConcat("123456", null, "123|456");
        }

        [Test]
        public void TestMinMaxEventType()
        {
            EPStatement selectTestView = SetUpMinMax();
            EventType type = selectTestView.EventType;
            Log.Debug(".testGetEventType properties=" + type.PropertyNames.Render());
            Assert.AreEqual(typeof (long?), type.GetPropertyType("myMax"));
            Assert.AreEqual(typeof (long?), type.GetPropertyType("myMin"));
            Assert.AreEqual(typeof (long?), type.GetPropertyType("myMinEx"));
            Assert.AreEqual(typeof (long?), type.GetPropertyType("myMaxEx"));
        }

        [Test]
        public void TestMinMaxWindowStats()
        {
            SetUpMinMax();
            _testListener.Reset();
            RunMinMaxWindowStats();
        }

        [Test]
        public void TestMinMaxWindowStats_Compile()
        {
            String viewExpr = "select max(LongBoxed,IntBoxed) as myMax, " +
                              "max(LongBoxed,IntBoxed,ShortBoxed) as myMaxEx, " +
                              "min(LongBoxed,IntBoxed) as myMin, " +
                              "min(LongBoxed,IntBoxed,ShortBoxed) as myMinEx" +
                              " from " + typeof (SupportBean).FullName + ".win:length(3)";

            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(viewExpr);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(viewExpr, model.ToEPL());

            EPStatement selectTestView = _epService.EPAdministrator.Create(model);
            selectTestView.Events += _testListener.Update;
            _testListener.Reset();

            RunMinMaxWindowStats();
        }

        [Test]
        public void TestMinMaxWindowStats_OM()
        {
            String viewExpr = "select max(LongBoxed,IntBoxed) as myMax, " +
                              "max(LongBoxed,IntBoxed,ShortBoxed) as myMaxEx, " +
                              "min(LongBoxed,IntBoxed) as myMin, " +
                              "min(LongBoxed,IntBoxed,ShortBoxed) as myMinEx" +
                              " from " + typeof (SupportBean).FullName + ".win:length(3)";

            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.Max("LongBoxed", "IntBoxed"), "myMax")
                .Add(Expressions.Max(Expressions.Property("LongBoxed"),
                    Expressions.Property("IntBoxed"),
                    Expressions.Property("ShortBoxed")), "myMaxEx")
                .Add(Expressions.Min("LongBoxed", "IntBoxed"), "myMin")
                .Add(Expressions.Min(Expressions.Property("LongBoxed"),
                    Expressions.Property("IntBoxed"),
                    Expressions.Property("ShortBoxed")), "myMinEx");
            model.FromClause = FromClause
                .Create(FilterStream.Create(typeof (SupportBean).FullName)
                    .AddView("win", "length",Expressions.Constant(3)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(viewExpr, model.ToEPL());

            EPStatement selectTestView = _epService.EPAdministrator.Create(model);
            selectTestView.Events += _testListener.Update;
            _testListener.Reset();

            RunMinMaxWindowStats();
        }

        [Test]
        public void TestOperators()
        {
            String viewExpr = "select LongBoxed % IntBoxed as myMod " +
                              " from " + typeof (SupportBean).FullName +
                              ".win:length(3) where Not(LongBoxed > IntBoxed)";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;

            SendEvent(1, 1, 0);
            Assert.AreEqual(0l, _testListener.LastNewData[0].Get("myMod"));
            _testListener.Reset();

            SendEvent(2, 1, 0);
            Assert.IsFalse(_testListener.GetAndClearIsInvoked());

            SendEvent(2, 3, 0);
            Assert.AreEqual(2l, _testListener.LastNewData[0].Get("myMod"));
            _testListener.Reset();
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
