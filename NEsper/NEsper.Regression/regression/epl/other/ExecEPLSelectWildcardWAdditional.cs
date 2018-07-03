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
using com.espertech.esper.client.soda;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    using Map = IDictionary<string, object>;

    public class ExecEPLSelectWildcardWAdditional : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var typeMap = new Dictionary<string, object>();
            typeMap.Put("int", typeof(int?));
            typeMap.Put("TheString", typeof(string));
            configuration.AddEventType("mapEvent", typeMap);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionSingleOM(epService);
            RunAssertionSingle(epService);
            RunAssertionSingleInsertInto(epService);
            RunAssertionJoinInsertInto(epService);
            RunAssertionJoinNoCommonProperties(epService);
            RunAssertionJoinCommonProperties(epService);
            RunAssertionCombinedProperties(epService);
            RunAssertionMapEvents(epService);
            RunAssertionInvalidRepeatedProperties(epService);
        }
    
        private void RunAssertionSingleOM(EPServiceProvider epService) {
            string eventName = typeof(SupportBeanSimple).FullName;
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard()
                .Add(Expressions.Concat("myString", "myString"), "concat");
            model.FromClause = FromClause.Create(FilterStream.Create(eventName)
                .AddView(View.Create("win", "length", Expressions.Constant(5))));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string text = "select *, myString||myString as concat from " + eventName + ".win:length(5)";
            Assert.AreEqual(text, model.ToEPL());
    
            EPStatement statement = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            AssertSimple(epService, listener);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("MyString", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("MyInt", typeof(int), null, false, false, false, false, false),
                    new EventPropertyDescriptor("concat", typeof(string), typeof(char), false, false, true, false, false),
            }, statement.EventType.PropertyDescriptors);
    
            statement.Dispose();
        }
    
        private void RunAssertionSingle(EPServiceProvider epService) {
            string eventName = typeof(SupportBeanSimple).FullName;
            string text = "select *, myString||myString as concat from " + eventName + "#length(5)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            AssertSimple(epService, listener);
    
            statement.Dispose();
        }
    
        private void RunAssertionSingleInsertInto(EPServiceProvider epService) {
            string eventName = typeof(SupportBeanSimple).FullName;
            string text = "insert into someEvent select *, myString||myString as concat from " + eventName + "#length(5)";
            string textTwo = "select * from someEvent#length(5)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            statement = epService.EPAdministrator.CreateEPL(textTwo);
            var insertListener = new SupportUpdateListener();
            statement.Events += insertListener.Update;
            AssertSimple(epService, listener);
            AssertProperties(Collections.EmptyDataMap, insertListener);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoinInsertInto(EPServiceProvider epService) {
            string eventNameOne = typeof(SupportBeanSimple).FullName;
            string eventNameTwo = typeof(SupportMarketDataBean).FullName;
            string text = "insert into someJoinEvent select *, myString||myString as concat " +
                    "from " + eventNameOne + "#length(5) as eventOne, "
                    + eventNameTwo + "#length(5) as eventTwo";
            string textTwo = "select * from someJoinEvent#length(5)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            statement = epService.EPAdministrator.CreateEPL(textTwo);
            var insertListener = new SupportUpdateListener();
            statement.Events += insertListener.Update;
    
            AssertNoCommonProperties(epService, listener);
            AssertProperties(Collections.EmptyDataMap, insertListener);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoinNoCommonProperties(EPServiceProvider epService) {
            string eventNameOne = typeof(SupportBeanSimple).FullName;
            string eventNameTwo = typeof(SupportMarketDataBean).FullName;
            string text = "select *, myString||myString as concat " +
                    "from " + eventNameOne + "#length(5) as eventOne, "
                    + eventNameTwo + "#length(5) as eventTwo";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            AssertNoCommonProperties(epService, listener);
    
            statement.Dispose();
    
            text = "select *, myString||myString as concat " +
                    "from " + eventNameOne + "#length(5) as eventOne, " +
                    eventNameTwo + "#length(5) as eventTwo " +
                    "where eventOne.myString = eventTwo.symbol";
    
            listener.Reset();
            statement = epService.EPAdministrator.CreateEPL(text);
            statement.Events += listener.Update;
    
            AssertNoCommonProperties(epService, listener);
    
            statement.Dispose();
        }
    
        private void RunAssertionJoinCommonProperties(EPServiceProvider epService) {
            string eventNameOne = typeof(SupportBean_A).FullName;
            string eventNameTwo = typeof(SupportBean_B).FullName;
            string text = "select *, eventOne.id||eventTwo.id as concat " +
                    "from " + eventNameOne + "#length(5) as eventOne, " +
                    eventNameTwo + "#length(5) as eventTwo ";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            AssertCommonProperties(epService, listener);
    
            statement.Dispose();
    
            text = "select *, eventOne.id||eventTwo.id as concat " +
                    "from " + eventNameOne + "#length(5) as eventOne, " +
                    eventNameTwo + "#length(5) as eventTwo " +
                    "where eventOne.id = eventTwo.id";
    
            listener.Reset();
            statement = epService.EPAdministrator.CreateEPL(text);
            statement.Events += listener.Update;
    
            AssertCommonProperties(epService, listener);
    
            statement.Dispose();
        }
    
        private void RunAssertionCombinedProperties(EPServiceProvider epService) {
            string eventName = typeof(SupportBeanCombinedProps).FullName;
            string text = "select *, indexed[0].Mapped('0ma').value||indexed[0].Mapped('0mb').value as concat from " + eventName + "#length(5)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            AssertCombinedProps(epService, listener);
            statement.Dispose();
        }
    
        private void RunAssertionMapEvents(EPServiceProvider epService) {
            string text = "select *, TheString||TheString as concat from mapEvent#length(5)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // The map to send into the runtime
            var props = new Dictionary<string, object>();
            props.Put("int", 1);
            props.Put("TheString", "xx");
            epService.EPRuntime.SendEvent(props, "mapEvent");
    
            // The map of expected results
            var properties = new Dictionary<string, object>();
            properties.Put("int", 1);
            properties.Put("TheString", "xx");
            properties.Put("concat", "xxxx");
    
            AssertProperties(properties, listener);
    
            statement.Dispose();
        }
    
        private void RunAssertionInvalidRepeatedProperties(EPServiceProvider epService) {
            string eventName = typeof(SupportBeanSimple).FullName;
            string text = "select *, myString||myString as MyString from " + eventName + "#length(5)";
    
            try {
                epService.EPAdministrator.CreateEPL(text);
                Assert.Fail();
            } catch (EPException) {
                //Expected
            }
        }
    
        private void AssertNoCommonProperties(EPServiceProvider epService, SupportUpdateListener listener) {
            SupportBeanSimple eventSimple = SendSimpleEvent(epService, "string");
            SupportMarketDataBean eventMarket = SendMarketEvent(epService, "string");
    
            EventBean theEvent = listener.LastNewData[0];
            var properties = new Dictionary<string, object>();
            properties.Put("concat", "stringstring");
            AssertProperties(properties, listener);
            Assert.AreSame(eventSimple, theEvent.Get("eventOne"));
            Assert.AreSame(eventMarket, theEvent.Get("eventTwo"));
        }
    
        private void AssertSimple(EPServiceProvider epService, SupportUpdateListener listener) {
            SupportBeanSimple theEvent = SendSimpleEvent(epService, "string");
    
            Assert.AreEqual("stringstring", listener.LastNewData[0].Get("concat"));
            var properties = new Dictionary<string, object>();
            properties.Put("concat", "stringstring");
            properties.Put("myString", "string");
            properties.Put("myInt", 0);
            AssertProperties(properties, listener);
    
            Assert.AreEqual(typeof(Pair<object, Map>), listener.LastNewData[0].EventType.UnderlyingType);
            Assert.IsTrue(listener.LastNewData[0].Underlying is Pair<object, Map>);
            var pair = (Pair<object, Map>) listener.LastNewData[0].Underlying;
            Assert.AreEqual(theEvent, pair.First);
            Assert.AreEqual("stringstring", ((Map) pair.Second).Get("concat"));
        }
    
        private void AssertCommonProperties(EPServiceProvider epService, SupportUpdateListener listener) {
            SendABEvents(epService, "string");
            EventBean theEvent = listener.LastNewData[0];
            var properties = new Dictionary<string, object>();
            properties.Put("concat", "stringstring");
            AssertProperties(properties, listener);
            Assert.IsNotNull(theEvent.Get("eventOne"));
            Assert.IsNotNull(theEvent.Get("eventTwo"));
        }
    
        private void AssertCombinedProps(EPServiceProvider epService, SupportUpdateListener listener) {
            SendCombinedProps(epService);
            EventBean eventBean = listener.LastNewData[0];
    
            Assert.AreEqual("0ma0", eventBean.Get("indexed[0].Mapped('0ma').value"));
            Assert.AreEqual("0ma1", eventBean.Get("indexed[0].Mapped('0mb').value"));
            Assert.AreEqual("1ma0", eventBean.Get("indexed[1].Mapped('1ma').value"));
            Assert.AreEqual("1ma1", eventBean.Get("indexed[1].Mapped('1mb').value"));
    
            Assert.AreEqual("0ma0", eventBean.Get("array[0].Mapped('0ma').value"));
            Assert.AreEqual("1ma1", eventBean.Get("array[1].Mapped('1mb').value"));
    
            Assert.AreEqual("0ma00ma1", eventBean.Get("concat"));
        }
    
        private void AssertProperties(IDictionary<string, Object> properties, SupportUpdateListener listener) {
            EventBean theEvent = listener.LastNewData[0];
            foreach (string property in properties.Keys) {
                Assert.AreEqual(properties.Get(property), theEvent.Get(property));
            }
        }
    
        private SupportBeanSimple SendSimpleEvent(EPServiceProvider epService, string s) {
            var bean = new SupportBeanSimple(s, 0);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportMarketDataBean SendMarketEvent(EPServiceProvider epService, string symbol) {
            var bean = new SupportMarketDataBean(symbol, 0.0, 0L, null);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendABEvents(EPServiceProvider epService, string id) {
            var beanOne = new SupportBean_A(id);
            var beanTwo = new SupportBean_B(id);
            epService.EPRuntime.SendEvent(beanOne);
            epService.EPRuntime.SendEvent(beanTwo);
        }
    
        private void SendCombinedProps(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(SupportBeanCombinedProps.MakeDefaultBean());
        }
    }
} // end of namespace
