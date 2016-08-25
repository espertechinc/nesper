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
using com.espertech.esper.client.soda;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestModifiedWildcardSelect
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener _insertListener;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            _insertListener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _insertListener = null;
        }

        [Test]
        public void TestSingleOM()
        {
            String eventName = typeof(SupportBeanSimple).FullName;

            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard().Add(Expressions.Concat("MyString", "MyString"), "concat");
            model.FromClause = FromClause.Create(FilterStream.Create(eventName).AddView(View.Create("win", "length", Expressions.Constant(5))));
            model = (EPStatementObjectModel)SerializableObjectCopier.Copy(model);

            String text = "select *, MyString||MyString as concat from " + eventName + ".win:length(5)";
            Assert.AreEqual(text, model.ToEPL());

            EPStatement statement = _epService.EPAdministrator.Create(model);
            statement.Events += _listener.Update;
            AssertSimple();

            EPAssertionUtil.AssertEqualsAnyOrder(new[]{
                    new EventPropertyDescriptor("MyString", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("MyInt", typeof(int), null, false, false, false, false, false),
                    new EventPropertyDescriptor("concat", typeof(string), typeof(char), false, false, true, false, false),
            }, statement.EventType.PropertyDescriptors);
        }

        [Test]
        public void TestSingle()
        {
            String eventName = typeof(SupportBeanSimple).FullName;
            String text = "select *, MyString||MyString as concat from " + eventName + ".win:length(5)";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(text);
            statement.Events += _listener.Update;
            AssertSimple();
        }

        [Test]
        public void TestSingleInsertInto()
        {
            String eventName = typeof(SupportBeanSimple).FullName;
            String text = "insert into someEvent select *, MyString||MyString as concat from " + eventName + ".win:length(5)";
            String textTwo = "select * from someEvent.win:length(5)";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(text);
            statement.Events += _listener.Update;

            statement = _epService.EPAdministrator.CreateEPL(textTwo);
            statement.Events += _insertListener.Update;
            AssertSimple();
            AssertProperties(Collections.EmptyDataMap, _insertListener);
        }

        [Test]
        public void TestJoinInsertInto()
        {
            String eventNameOne = typeof(SupportBeanSimple).FullName;
            String eventNameTwo = typeof(SupportMarketDataBean).FullName;
            String text = "insert into someJoinEvent select *, MyString||MyString as concat " +
                    "from " + eventNameOne + ".win:length(5) as eventOne, "
                    + eventNameTwo + ".win:length(5) as eventTwo";
            String textTwo = "select * from someJoinEvent.win:length(5)";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(text);
            statement.Events += _listener.Update;

            statement = _epService.EPAdministrator.CreateEPL(textTwo);
            statement.Events += _insertListener.Update;

            AssertNoCommonProperties();
            AssertProperties(Collections.EmptyDataMap, _insertListener);
        }

        [Test]
        public void TestJoinNoCommonProperties()
        {
            String eventNameOne = typeof(SupportBeanSimple).FullName;
            String eventNameTwo = typeof(SupportMarketDataBean).FullName;
            String text = "select *, MyString||MyString as concat " +
                    "from " + eventNameOne + ".win:length(5) as eventOne, "
                    + eventNameTwo + ".win:length(5) as eventTwo";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(text);
            statement.Events += _listener.Update;

            AssertNoCommonProperties();

            _listener.Reset();
            _epService.Initialize();

            text = "select *, MyString||MyString as concat " +
                    "from " + eventNameOne + ".win:length(5) as eventOne, " +
                    eventNameTwo + ".win:length(5) as eventTwo " +
                    "where eventOne.MyString = eventTwo.Symbol";

            statement = _epService.EPAdministrator.CreateEPL(text);
            statement.Events += _listener.Update;

            AssertNoCommonProperties();
        }

        [Test]
        public void TestJoinCommonProperties()
        {
            String eventNameOne = typeof(SupportBean_A).FullName;
            String eventNameTwo = typeof(SupportBean_B).FullName;
            String text = "select *, eventOne.id||eventTwo.id as concat " +
                    "from " + eventNameOne + ".win:length(5) as eventOne, " +
                    eventNameTwo + ".win:length(5) as eventTwo ";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(text);
            statement.Events += _listener.Update;

            AssertCommonProperties();

            _listener.Reset();
            _epService.Initialize();

            text = "select *, eventOne.id||eventTwo.id as concat " +
                    "from " + eventNameOne + ".win:length(5) as eventOne, " +
                    eventNameTwo + ".win:length(5) as eventTwo " +
                    "where eventOne.id = eventTwo.id";

            statement = _epService.EPAdministrator.CreateEPL(text);
            statement.Events += _listener.Update;

            AssertCommonProperties();
        }

        [Test]
        public void TestCombinedProperties()
        {
            String eventName = typeof(SupportBeanCombinedProps).FullName;
            String text = "select *, Indexed[0].Mapped('0ma').value||Indexed[0].Mapped('0mb').value as concat from " + eventName + ".win:length(5)";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(text);
            statement.Events += _listener.Update;
            AssertCombinedProps();
        }

        [Test]
        public void TestMapEvents()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            IDictionary<String, Object> typeMap = new Dictionary<String, Object>();
            typeMap["int"] = typeof(int);
            typeMap["string"] = typeof(string);
            configuration.AddEventType("mapEvent", typeMap);
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();

            String text = "select *, string||string as concat from mapEvent.win:length(5)";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(text);
            statement.Events += _listener.Update;

            // The map to send into the runtime
            IDictionary<String, Object> props = new Dictionary<String, Object>();
            props["int"] = 1;
            props["string"] = "xx";
            _epService.EPRuntime.SendEvent(props, "mapEvent");

            // The map of expected results
            IDictionary<String, Object> properties = new Dictionary<String, Object>();
            properties["int"] = 1;
            properties["string"] = "xx";
            properties["concat"] = "xxxx";

            AssertProperties(properties, _listener);

            _epService.Dispose();
        }

        [Test]
        public void TestInvalidRepeatedProperties()
        {
            String eventName = typeof(SupportBeanSimple).FullName;
            String text = "select *, MyString||MyString as MyString from " + eventName + ".win:length(5)";

            try
            {
                _epService.EPAdministrator.CreateEPL(text);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                //Expected
            }
        }

        private void AssertNoCommonProperties()
        {
            SupportBeanSimple eventSimple = SendSimpleEvent("string");
            SupportMarketDataBean eventMarket = SendMarketEvent("string");

            EventBean theEvent = _listener.LastNewData[0];
            IDictionary<String, Object> properties = new Dictionary<String, Object>();
            properties["concat"] = "stringstring";
            AssertProperties(properties, _listener);
            Assert.AreSame(eventSimple, theEvent.Get("eventOne"));
            Assert.AreSame(eventMarket, theEvent.Get("eventTwo"));
        }

        private void AssertSimple()
        {
            SupportBeanSimple theEvent = SendSimpleEvent("string");

            Assert.AreEqual("stringstring", _listener.LastNewData[0].Get("concat"));
            IDictionary<String, Object> properties = new Dictionary<String, Object>();
            properties["concat"] = "stringstring";
            properties["MyString"] = "string";
            properties["MyInt"] = 0;
            AssertProperties(properties, _listener);

            Assert.AreEqual(typeof(Pair<object, Map>), _listener.LastNewData[0].EventType.UnderlyingType);
            Assert.That(_listener.LastNewData[0].Underlying, Is.InstanceOf<Pair<object, Map>>());
            var pair = (Pair<object, IDictionary<string, object>>)_listener.LastNewData[0].Underlying;
            Assert.AreEqual(theEvent, pair.First);
            Assert.AreEqual("stringstring", pair.Second.Get("concat"));
        }

        private void AssertCommonProperties()
        {
            SendABEvents("string");
            EventBean theEvent = _listener.LastNewData[0];
            IDictionary<String, Object> properties = new Dictionary<String, Object>();
            properties["concat"] = "stringstring";
            AssertProperties(properties, _listener);
            Assert.NotNull(theEvent.Get("eventOne"));
            Assert.NotNull(theEvent.Get("eventTwo"));
        }

        private void AssertCombinedProps()
        {
            SendCombinedProps();
            EventBean eventBean = _listener.LastNewData[0];

            Assert.AreEqual("0ma0", eventBean.Get("Indexed[0].Mapped('0ma').value"));
            Assert.AreEqual("0ma1", eventBean.Get("Indexed[0].Mapped('0mb').value"));
            Assert.AreEqual("1ma0", eventBean.Get("Indexed[1].Mapped('1ma').value"));
            Assert.AreEqual("1ma1", eventBean.Get("Indexed[1].Mapped('1mb').value"));

            Assert.AreEqual("0ma0", eventBean.Get("array[0].Mapped('0ma').value"));
            Assert.AreEqual("1ma1", eventBean.Get("array[1].Mapped('1mb').value"));

            Assert.AreEqual("0ma00ma1", eventBean.Get("concat"));
        }

        private void AssertProperties(IDictionary<String, Object> properties, SupportUpdateListener listener)
        {
            EventBean theEvent = listener.LastNewData[0];
            foreach (String property in properties.Keys)
            {
                Assert.AreEqual(properties.Get(property), theEvent.Get(property));
            }
        }

        private SupportBeanSimple SendSimpleEvent(String s)
        {
            SupportBeanSimple bean = new SupportBeanSimple(s, 0);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private SupportMarketDataBean SendMarketEvent(String symbol)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0.0, 0L, null);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private void SendABEvents(String id)
        {
            SupportBean_A beanOne = new SupportBean_A(id);
            SupportBean_B beanTwo = new SupportBean_B(id);
            _epService.EPRuntime.SendEvent(beanOne);
            _epService.EPRuntime.SendEvent(beanTwo);
        }

        private void SendCombinedProps()
        {
            _epService.EPRuntime.SendEvent(SupportBeanCombinedProps.MakeDefaultBean());
        }
    }
}
