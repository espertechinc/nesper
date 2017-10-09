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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestInsertIntoTransposePattern 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener _listenerInsertInto;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            _listenerInsertInto = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _listenerInsertInto= null;
        }
    
        [Test]
        public void TestThisAsColumn()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create window OneWindow#time(1 day) as select TheString as alertId, this from SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into OneWindow select '1' as alertId, stream0.quote.this as this " +
                    " from pattern [every quote=SupportBean(TheString='A')] as stream0");
            _epService.EPAdministrator.CreateEPL("insert into OneWindow select '2' as alertId, stream0.quote as this " +
                    " from pattern [every quote=SupportBean(TheString='B')] as stream0");
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 10));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new String[] { "alertId", "this.IntPrimitive" }, new Object[][] { new Object[] { "1", 10 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("B", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new String[] { "alertId", "this.IntPrimitive" }, new Object[][] { new Object[] { "1", 10 }, new Object[] { "2", 20 } });
    
            stmt = _epService.EPAdministrator.CreateEPL("create window TwoWindow#time(1 day) as select TheString as alertId, * from SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into TwoWindow select '3' as alertId, quote.* " +
                    " from pattern [every quote=SupportBean(TheString='C')] as stream0");
    
            _epService.EPRuntime.SendEvent(new SupportBean("C", 30));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new String[] { "alertId", "IntPrimitive" }, new Object[][] { new Object[] { "3", 30 } });
        }
    
        [Test]
        public void TestTransposePONOEventPattern()
        {
            _epService.EPAdministrator.Configuration.AddEventType("AEvent", typeof(SupportBean_A));
            _epService.EPAdministrator.Configuration.AddEventType("BEvent", typeof(SupportBean_B));
    
            String stmtTextOne = "insert into MyStream select a, b from pattern [a=AEvent -> b=BEvent]";
            _epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            String stmtTextTwo = "select a.id, b.id from MyStream";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a.id,b.id".Split(','), new Object[] {"A1", "B1"});
        }
    
        [Test]
        public void TestTransposeMapEventPattern()
        {
            IDictionary<String, Object> type = MakeMap(new Object[][] { new Object[] {"id", typeof(string)}});
    
            _epService.EPAdministrator.Configuration.AddEventType("AEvent", type);
            _epService.EPAdministrator.Configuration.AddEventType("BEvent", type);
    
            String stmtTextOne = "insert into MyStream select a, b from pattern [a=AEvent -> b=BEvent]";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listenerInsertInto.Update;
            Assert.AreEqual(typeof(Map), stmtOne.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(Map), stmtOne.EventType.GetPropertyType("b"));
    
            String stmtTextTwo = "select a.id, b.id from MyStream";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmtTwo.Events += _listener.Update;
            Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("a.id"));
            Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("b.id"));
    
            IDictionary<String, Object> eventOne = MakeMap(new Object[][] { new Object[] {"id", "A1"}});
            IDictionary<String, Object> eventTwo = MakeMap(new Object[][] { new Object[] {"id", "B1"}});
    
            _epService.EPRuntime.SendEvent(eventOne, "AEvent");
            _epService.EPRuntime.SendEvent(eventTwo, "BEvent");
    
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, "a.id,b.id".Split(','), new Object[] {"A1", "B1"});
    
            theEvent = _listenerInsertInto.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, "a,b".Split(','), new Object[] {eventOne, eventTwo});
        }
    
        private IDictionary<String, Object> MakeMap(Object[][] entries)
        {
            var result = new Dictionary<String, Object>();
            foreach (Object[] entry in entries)
            {
                result.Put((string) entry[0], entry[1]);
            }
            return result;
        }
    }
}
