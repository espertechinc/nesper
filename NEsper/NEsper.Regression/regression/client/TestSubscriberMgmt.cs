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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.subscriber;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestSubscriberMgmt
    {
        private EPServiceProvider _epService;
        private readonly String[] _fields = "TheString,IntPrimitive".Split(',');
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            var pkg = typeof(SupportBean).Namespace;
            config.AddEventTypeAutoName(pkg);
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestStartStopStatement() {
            var subscriber = new SubscriberInterface();
            var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportMarkerInterface");
            stmt.Subscriber = subscriber;
    
            var a1 = new SupportBean_A("A1");
            _epService.EPRuntime.SendEvent(a1);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{a1}, subscriber.GetAndResetIndicate().ToArray());
    
            var b1 = new SupportBean_B("B1");
            _epService.EPRuntime.SendEvent(b1);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{b1}, subscriber.GetAndResetIndicate().ToArray());
    
            stmt.Stop();
    
            var c1 = new SupportBean_C("C1");
            _epService.EPRuntime.SendEvent(c1);
            Assert.AreEqual(0, subscriber.GetAndResetIndicate().Count);
    
            stmt.Start();
    
            var d1 = new SupportBean_D("D1");
            _epService.EPRuntime.SendEvent(d1);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{d1}, subscriber.GetAndResetIndicate().ToArray());
        }
    
        [Test]
        public void TestVariables() {
            var fields = "myvar".Split(',');
            var subscriberCreateVariable = new SubscriberMap();
            var stmtTextCreate = "create variable string myvar = 'abc'";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmt.Subscriber = subscriberCreateVariable;
    
            var subscriberSetVariable = new SubscriberMap();
            var stmtTextSet = "on SupportBean set myvar = TheString";
            stmt = _epService.EPAdministrator.CreateEPL(stmtTextSet);
            stmt.Subscriber = subscriberSetVariable;
    
            _epService.EPRuntime.SendEvent(new SupportBean("def", 1));
            EPAssertionUtil.AssertPropsMap(subscriberCreateVariable.GetAndResetIndicate()[0], fields, new Object[]{"def"});
            EPAssertionUtil.AssertPropsMap(subscriberSetVariable.GetAndResetIndicate()[0], fields, new Object[]{"def"});
        }
    
        [Test]
        public void TestNamedWindow() {
            RunAssertionNamedWindow(EventRepresentationChoice.MAP);
        }

        private void RunAssertionNamedWindow(EventRepresentationChoice eventRepresentationEnum)
        {
            var fields = "key,value".Split(',');
            var subscriberNamedWindow = new SubscriberMap();
            var stmtTextCreate = eventRepresentationEnum.GetAnnotationText() 
                + " create window MyWindow#keepall as select TheString as key, IntPrimitive as value from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmt.Subscriber = subscriberNamedWindow;
    
            var subscriberInsertInto = new SubscriberFields();
            var stmtTextInsertInto = "insert into MyWindow select TheString as key, IntPrimitive as value from SupportBean";
            stmt = _epService.EPAdministrator.CreateEPL(stmtTextInsertInto);
            stmt.Subscriber = subscriberInsertInto;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap(subscriberNamedWindow.GetAndResetIndicate()[0], fields, new Object[]{"E1", 1});
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]{new Object[] {"E1", 1}}, subscriberInsertInto.GetAndResetIndicate());
    
            // test on-delete
            var subscriberDelete = new SubscriberMap();
            var stmtTextDelete = "on SupportMarketDataBean s0 delete from MyWindow s1 where s0.Symbol = s1.key";
            stmt = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmt.Subscriber = subscriberDelete;
    
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("E1", 0, 1L, ""));
            EPAssertionUtil.AssertPropsMap(subscriberDelete.GetAndResetIndicate()[0], fields, new Object[]{"E1", 1});
    
            // test on-select
            var subscriberSelect = new SubscriberMap();
            var stmtTextSelect = "on SupportMarketDataBean s0 select key, value from MyWindow s1";
            stmt = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmt.Subscriber = subscriberSelect;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("M1", 0, 1L, ""));
            EPAssertionUtil.AssertPropsMap(subscriberSelect.GetAndResetIndicate()[0], fields, new Object[]{"E2", 2});
        }
    
        [Test]
        public void TestSimpleSelectUpdateOnly()
        {
            var subscriber = new SupportSubscriberRowByRowSpecificNStmt();
            var stmt = _epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from " + typeof(SupportBean).FullName + "#lastevent");
            stmt.Subscriber = subscriber;
    
            // get statement, attach listener
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // send event
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            subscriber.AssertOneReceivedAndReset(stmt, new Object[] { "E1", 100 });
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), _fields, new Object[][]{new Object[] {"E1", 100}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), _fields, new Object[]{"E1", 100});
    
            // remove listener
            stmt.RemoveAllEventHandlers();
    
            // send event
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
            subscriber.AssertOneReceivedAndReset(stmt, new Object[] { "E2", 200 }); 
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), _fields, new Object[][] { new Object[] { "E2", 200 } });
            Assert.IsFalse(listener.IsInvoked);
    
            // add listener
            var stmtAwareListener = new SupportStmtAwareUpdateListener();
            stmt.Events += stmtAwareListener.Update;
    
            // send event
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 300));
            subscriber.AssertOneReceivedAndReset(stmt, new Object[] { "E3", 300 });
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), _fields, new Object[][]{new Object[] {"E3", 300}});
            EPAssertionUtil.AssertProps(stmtAwareListener.AssertOneGetNewAndReset(), _fields, new Object[]{"E3", 300});

            // subscriber with EPStatement in the footprint
            stmt.RemoveAllEventHandlers();
            var subsWithStatement = new SupportSubscriberRowByRowSpecificWStmt();
            stmt.Subscriber = subsWithStatement;
            _epService.EPRuntime.SendEvent(new SupportBean("E10", 999));
            subsWithStatement.AssertOneReceivedAndReset(stmt, new Object[] { "E10", 999 });
        }
    
        public class SubscriberFields {
            private List<Object[]> indicate = new List<Object[]>();
    
            public void Update(String key, int value) {
                indicate.Add(new Object[]{key, value});
            }
    
            public List<Object[]> GetAndResetIndicate() {
                var result = indicate;
                indicate = new List<Object[]>();
                return result;
            }
        }
    
        public class SubscriberInterface {
            private List<SupportMarkerInterface> indicate = new List<SupportMarkerInterface>();
    
            public void Update(SupportMarkerInterface impl) {
                indicate.Add(impl);
            }
    
            public List<SupportMarkerInterface> GetAndResetIndicate() {
                var result = indicate;
                indicate = new List<SupportMarkerInterface>();
                return result;
            }
        }
    
        public class SubscriberMap {
            private List<Map> indicate = new List<Map>();
    
            public void Update(Map row) {
                indicate.Add(row);
            }
    
            public List<Map> GetAndResetIndicate() {
                var result = indicate;
                indicate = new List<Map>();
                return result;
            }
        }
    }
}
