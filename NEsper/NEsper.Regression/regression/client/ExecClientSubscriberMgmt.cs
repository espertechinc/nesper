///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.subscriber;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    using Map = IDictionary<string, object>;

    public class ExecClientSubscriberMgmt : RegressionExecution
    {
        private static readonly string[] FIELDS = "TheString,IntPrimitive".Split(',');
    
        public override void Configure(Configuration configuration) {
            var pkg = typeof(SupportBean).Namespace;
            configuration.AddEventTypeAutoName(pkg);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionStartStopStatement(epService);
            RunAssertionVariables(epService);
            RunAssertionNamedWindow(epService);
            RunAssertionSimpleSelectUpdateOnly(epService);
        }
    
        private void RunAssertionStartStopStatement(EPServiceProvider epService) {
            var subscriber = new SubscriberInterface();
            var stmt = epService.EPAdministrator.CreateEPL("select * from SupportMarkerInterface");
            stmt.Subscriber = subscriber;
    
            var a1 = new SupportBean_A("A1");
            epService.EPRuntime.SendEvent(a1);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{a1}, subscriber.GetAndResetIndicate().ToArray());
    
            var b1 = new SupportBean_B("B1");
            epService.EPRuntime.SendEvent(b1);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{b1}, subscriber.GetAndResetIndicate().ToArray());
    
            stmt.Stop();
    
            var c1 = new SupportBean_C("C1");
            epService.EPRuntime.SendEvent(c1);
            Assert.AreEqual(0, subscriber.GetAndResetIndicate().Count);
    
            stmt.Start();
    
            var d1 = new SupportBean_D("D1");
            epService.EPRuntime.SendEvent(d1);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{d1}, subscriber.GetAndResetIndicate().ToArray());
    
            stmt.Dispose();
        }
    
        private void RunAssertionVariables(EPServiceProvider epService) {
            var fields = "myvar".Split(',');
            var subscriberCreateVariable = new SubscriberMap();
            var stmtTextCreate = "create variable string myvar = 'abc'";
            var stmt = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmt.Subscriber = subscriberCreateVariable;
    
            var subscriberSetVariable = new SubscriberMap();
            var stmtTextSet = "on SupportBean set myvar = TheString";
            stmt = epService.EPAdministrator.CreateEPL(stmtTextSet);
            stmt.Subscriber = subscriberSetVariable;
    
            epService.EPRuntime.SendEvent(new SupportBean("def", 1));
            EPAssertionUtil.AssertPropsMap(subscriberCreateVariable.GetAndResetIndicate()[0], fields, new object[]{"def"});
            EPAssertionUtil.AssertPropsMap(subscriberSetVariable.GetAndResetIndicate()[0], fields, new object[]{"def"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionNamedWindow(EPServiceProvider epService) {
            TryAssertionNamedWindow(epService, EventRepresentationChoice.MAP);
        }
    
        private void TryAssertionNamedWindow(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            var fields = "key,value".Split(',');
            var subscriberNamedWindow = new SubscriberMap();
            var stmtTextCreate = eventRepresentationEnum.GetAnnotationText() + " create window MyWindow#keepall as select TheString as key, IntPrimitive as value from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmt.Subscriber = subscriberNamedWindow;
    
            var subscriberInsertInto = new SubscriberFields();
            var stmtTextInsertInto = "insert into MyWindow select TheString as key, IntPrimitive as value from SupportBean";
            stmt = epService.EPAdministrator.CreateEPL(stmtTextInsertInto);
            stmt.Subscriber = subscriberInsertInto;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap(subscriberNamedWindow.GetAndResetIndicate()[0], fields, new object[]{"E1", 1});
            EPAssertionUtil.AssertEqualsExactOrder(new object[][]{new object[] {"E1", 1}}, subscriberInsertInto.GetAndResetIndicate());
    
            // test on-delete
            var subscriberDelete = new SubscriberMap();
            var stmtTextDelete = "on SupportMarketDataBean s0 delete from MyWindow s1 where s0.symbol = s1.key";
            stmt = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmt.Subscriber = subscriberDelete;
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("E1", 0, 1L, ""));
            EPAssertionUtil.AssertPropsMap(subscriberDelete.GetAndResetIndicate()[0], fields, new object[]{"E1", 1});
    
            // test on-select
            var subscriberSelect = new SubscriberMap();
            var stmtTextSelect = "on SupportMarketDataBean s0 select key, value from MyWindow s1";
            stmt = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmt.Subscriber = subscriberSelect;
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("M1", 0, 1L, ""));
            EPAssertionUtil.AssertPropsMap(subscriberSelect.GetAndResetIndicate()[0], fields, new object[]{"E2", 2});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSimpleSelectUpdateOnly(EPServiceProvider epService) {
            var subscriber = new SupportSubscriberRowByRowSpecificNStmt();
            var stmt = epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from " + typeof(SupportBean).FullName + "#lastevent");
            stmt.Subscriber = subscriber;
    
            // get statement, attach listener
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // send event
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{"E1", 100});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), FIELDS, new object[][]{new object[] {"E1", 100}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 100});
    
            // remove listener
            stmt.RemoveAllEventHandlers();
    
            // send event
            epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{"E2", 200});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), FIELDS, new object[][]{new object[] {"E2", 200}});
            Assert.IsFalse(listener.IsInvoked);
    
            // add listener
            var stmtAwareListener = new SupportStmtAwareUpdateListener();
            stmt.Events += stmtAwareListener.Update;
    
            // send event
            epService.EPRuntime.SendEvent(new SupportBean("E3", 300));
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{"E3", 300});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), FIELDS, new object[][]{new object[] {"E3", 300}});
            EPAssertionUtil.AssertProps(stmtAwareListener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E3", 300});
    
            // subscriber with EPStatement in the footprint
            stmt.RemoveAllEventHandlers();
            var subsWithStatement = new SupportSubscriberRowByRowSpecificWStmt();
            stmt.Subscriber = subsWithStatement;
            epService.EPRuntime.SendEvent(new SupportBean("E10", 999));
            subsWithStatement.AssertOneReceivedAndReset(stmt, new object[]{"E10", 999});
    
            stmt.Dispose();
        }
    
        public class SubscriberFields {
            private List<object[]> _indicate = new List<object[]>();
    
            public void Update(string key, int value) {
                _indicate.Add(new object[]{key, value});
            }
    
            public List<object[]> GetAndResetIndicate() {
                var result = _indicate;
                _indicate = new List<object[]>();
                return result;
            }
        }
    
        public class SubscriberInterface {
            private List<SupportMarkerInterface> _indicate = new List<SupportMarkerInterface>();
    
            public void Update(SupportMarkerInterface impl) {
                _indicate.Add(impl);
            }

            public List<SupportMarkerInterface> GetAndResetIndicate()
            {
                var result = _indicate;
                _indicate = new List<SupportMarkerInterface>();
                return result;
            }
        }
    
        public class SubscriberMap {
            private List<Map> _indicate = new List<Map>();
    
            public void Update(Map row) {
                _indicate.Add(row);
            }
    
            public IList<Map> GetAndResetIndicate() {
                var result = _indicate;
                _indicate = new List<Map>();
                return result;
            }
        }
    }
} // end of namespace
