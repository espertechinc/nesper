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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using DataMap = IDictionary<string, object>;
    using DataMapImpl = Dictionary<string, object>;

    [TestFixture]
    public class TestSplitStream 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener[] _listeners;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType("S0", typeof(SupportBean_S0));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            _listeners = new SupportUpdateListener[10];
            for (var i = 0; i < _listeners.Length; i++)
            {
                _listeners[i] = new SupportUpdateListener();
            }
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _listeners = null;
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalid("on SupportBean select * where IntPrimitive=1 insert into BStream select * where 1=2",
                       "Error starting statement: Required insert-into clause is not provided, the clause is required for split-stream syntax [on SupportBean select * where IntPrimitive=1 insert into BStream select * where 1=2]");
    
            TryInvalid("on SupportBean insert into AStream select * where IntPrimitive=1 group by string insert into BStream select * where 1=2",
                       "Error starting statement: A group-by clause, having-clause or order-by clause is not allowed for the split stream syntax [on SupportBean insert into AStream select * where IntPrimitive=1 group by string insert into BStream select * where 1=2]");
    
            TryInvalid("on SupportBean insert into AStream select * where IntPrimitive=1 insert into BStream select Avg(IntPrimitive) where 1=2",
                       "Error starting statement: Aggregation functions are not allowed in this context [on SupportBean insert into AStream select * where IntPrimitive=1 insert into BStream select Avg(IntPrimitive) where 1=2]");
        }
    
        private void TryInvalid(String stmtText, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        [Test]
        public void TestSplitPremptiveNamedWindow()
        {
            RunAssertionSplitPremptiveNamedWindow(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionSplitPremptiveNamedWindow(EventRepresentationEnum.MAP);
            RunAssertionSplitPremptiveNamedWindow(EventRepresentationEnum.DEFAULT);
        }
    
        public void RunAssertionSplitPremptiveNamedWindow(EventRepresentationEnum eventRepresentationEnum)
        {
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeTwo(col2 int)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeTrigger(trigger int)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window WinTwo.win:keepall() as TypeTwo");
    
            var stmtOrigText = "on TypeTrigger " +
                        "insert into OtherStream select 1 " +
                        "insert into WinTwo(col2) select 2 " +
                        "output all";
            _epService.EPAdministrator.CreateEPL(stmtOrigText);
    
            var stmt = _epService.EPAdministrator.CreateEPL("on OtherStream select col2 from WinTwo");
            stmt.Events += _listener.Update;
            
            // populate WinOne
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
    
            // fire trigger
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new Object[] { null });
            }
            else {
                _epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new DataMapImpl());
            }
    
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("col2"));
    
            _epService.Initialize();
        }
    
        [Test]
        public void Test1SplitDefault()
        {
            // test wildcard
            var stmtOrigText = "on SupportBean insert into AStream select *";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmt.Events += _listener.Update;
    
            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;
    
            SendSupportBean("E1", 1);
            AssertReceivedSingle(0, "E1");
            Assert.IsFalse(_listener.IsInvoked);
    
            // test select
            stmtOrigText = "on SupportBean insert into BStream select 3*IntPrimitive as value";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
    
            stmtOne = _epService.EPAdministrator.CreateEPL("select value from BStream");
            stmtOne.Events += _listeners[1].Update;
    
            SendSupportBean("E1", 6);
            Assert.AreEqual(18, _listeners[1].AssertOneGetNewAndReset().Get("value"));
    
            // assert type is original type
            Assert.AreEqual(typeof(SupportBean), stmtOrig.EventType.UnderlyingType);
            Assert.IsFalse(stmtOrig.HasFirst());
        }
    
        [Test]
        public void Test2SplitNoDefaultOutputFirst()
        {
            var stmtOrigText = "@Audit on SupportBean " +
                        "insert into AStream select * where IntPrimitive=1 " +
                        "insert into BStream select * where IntPrimitive=1 or IntPrimitive=2";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            RunAssertion(stmtOrig);
    
            // statement object model
            var model = new EPStatementObjectModel();
            model.Annotations = Collections.SingletonList(new AnnotationPart("Audit"));
            model.FromClause = FromClause.Create(FilterStream.Create("SupportBean"));
            model.InsertInto = InsertIntoClause.Create("AStream");
            model.SelectClause = SelectClause.CreateWildcard();
            model.WhereClause = Expressions.Eq("IntPrimitive", 1);
            var clause = OnClause.CreateOnInsertSplitStream();
            model.OnExpr = clause;
            var item = OnInsertSplitStreamItem.Create(
                    InsertIntoClause.Create("BStream"),
                    SelectClause.CreateWildcard(),
                    Expressions.Or(Expressions.Eq("IntPrimitive", 1), Expressions.Eq("IntPrimitive", 2)));
            clause.AddItem(item);
            Assert.AreEqual(stmtOrigText, model.ToEPL());
            stmtOrig = _epService.EPAdministrator.Create(model);
            RunAssertion(stmtOrig);
    
            var newModel = _epService.EPAdministrator.CompileEPL(stmtOrigText);
            stmtOrig = _epService.EPAdministrator.Create(newModel);
            Assert.AreEqual(stmtOrigText, newModel.ToEPL());
            RunAssertion(stmtOrig);
    
            SupportModelHelper.CompileCreate(_epService, stmtOrigText + " output all");
        }
    
        [Test]
        public void TestSubquery()
        {
            var stmtOrigText = "on SupportBean " +
                                  "insert into AStream select (select p00 from S0.std:lastevent()) as string where IntPrimitive=(select id from S0.std:lastevent()) " +
                                  "insert into BStream select (select p01 from S0.std:lastevent()) as string where IntPrimitive<>(select id from S0.std:lastevent()) or (select id from S0.std:lastevent()) is null";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;
    
            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL("select * from BStream");
            stmtTwo.Events += _listeners[1].Update;
            
            SendSupportBean("E1", 1);
            Assert.IsFalse(_listeners[0].GetAndClearIsInvoked());
            Assert.IsNull(_listeners[1].AssertOneGetNewAndReset().Get("string"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "x", "y"));
    
            SendSupportBean("E2", 10);
            Assert.AreEqual("x", _listeners[0].AssertOneGetNewAndReset().Get("string"));
            Assert.IsFalse(_listeners[1].GetAndClearIsInvoked());
    
            SendSupportBean("E3", 9);
            Assert.IsFalse(_listeners[0].GetAndClearIsInvoked());
            Assert.AreEqual("y", _listeners[1].AssertOneGetNewAndReset().Get("string"));
        }
    
        [Test]
        public void Test2SplitNoDefaultOutputAll()
        {
            var stmtOrigText = "on SupportBean " +
                                  "insert into AStream select TheString where IntPrimitive=1 " +
                                  "insert into BStream select TheString where IntPrimitive=1 or IntPrimitive=2 " +
                                  "output all";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;
    
            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL("select * from BStream");
            stmtTwo.Events += _listeners[1].Update;
    
            Assert.AreNotSame(stmtOne.EventType, stmtTwo.EventType);
            Assert.AreSame(stmtOne.EventType.UnderlyingType, stmtTwo.EventType.UnderlyingType);
    
            SendSupportBean("E1", 1);
            AssertReceivedEach(new String[] {"E1", "E1"});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E2", 2);
            AssertReceivedEach(new String[] {null, "E2"});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E3", 1);
            AssertReceivedEach(new String[] {"E3", "E3"});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E4", -999);
            AssertReceivedEach(new String[] {null, null});
            Assert.AreEqual("E4", _listener.AssertOneGetNewAndReset().Get("TheString"));
    
            stmtOrig.Dispose();
            stmtOrigText = "on SupportBean " +
                                  "insert into AStream select TheString || '_1' as TheString where IntPrimitive in (1, 2) " +
                                  "insert into BStream select TheString || '_2' as TheString where IntPrimitive in (2, 3) " +
                                  "insert into CStream select TheString || '_3' as TheString " +
                                  "output all";
            stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;
    
            var stmtThree = _epService.EPAdministrator.CreateEPL("select * from CStream");
            stmtThree.Events += _listeners[2].Update;
    
            SendSupportBean("E1", 2);
            AssertReceivedEach(new String[] {"E1_1", "E1_2", "E1_3"});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E2", 1);
            AssertReceivedEach(new String[] {"E2_1", null, "E2_3"});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E3", 3);
            AssertReceivedEach(new String[] {null, "E3_2", "E3_3"});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E4", -999);
            AssertReceivedEach(new String[] {null, null, "E4_3"});
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void Test3And4SplitDefaultOutputFirst()
        {
            var stmtOrigText = "on SupportBean as mystream " +
                                  "insert into AStream select mystream.TheString||'_1' as TheString where IntPrimitive=1 " +
                                  "insert into BStream select mystream.TheString||'_2' as TheString where IntPrimitive=2 " +
                                  "insert into CStream select TheString||'_3' as TheString";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;
    
            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL("select * from BStream");
            stmtTwo.Events += _listeners[1].Update;
            var stmtThree = _epService.EPAdministrator.CreateEPL("select * from CStream");
            stmtThree.Events += _listeners[2].Update;
    
            Assert.AreNotSame(stmtOne.EventType, stmtTwo.EventType);
            Assert.AreSame(stmtOne.EventType.UnderlyingType, stmtTwo.EventType.UnderlyingType);
    
            SendSupportBean("E1", 1);
            AssertReceivedSingle(0, "E1_1");
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E2", 2);
            AssertReceivedSingle(1, "E2_2");
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E3", 1);
            AssertReceivedSingle(0, "E3_1");
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E4", -999);
            AssertReceivedSingle(2, "E4_3");
            Assert.IsFalse(_listener.IsInvoked);
    
            stmtOrigText = "on SupportBean " +
                                  "insert into AStream select TheString||'_1' as TheString where IntPrimitive=10 " +
                                  "insert into BStream select TheString||'_2' as TheString where IntPrimitive=20 " +
                                  "insert into CStream select TheString||'_3' as TheString where IntPrimitive<0 " +
                                  "insert into DStream select TheString||'_4' as TheString";
            stmtOrig.Dispose();
            stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;
    
            var stmtFour = _epService.EPAdministrator.CreateEPL("select * from DStream");
            stmtFour.Events += _listeners[3].Update;
    
            SendSupportBean("E5", -999);
            AssertReceivedSingle(2, "E5_3");
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E6", 9999);
            AssertReceivedSingle(3, "E6_4");
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E7", 20);
            AssertReceivedSingle(1, "E7_2");
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E8", 10);
            AssertReceivedSingle(0, "E8_1");
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        private void AssertReceivedEach(String[] stringValue)
        {
            for (var i = 0; i < stringValue.Length; i++)
            {
                if (stringValue[i] != null)
                {
                    Assert.AreEqual(stringValue[i], _listeners[i].AssertOneGetNewAndReset().Get("TheString"));
                }
                else
                {
                    Assert.IsFalse(_listeners[i].IsInvoked);
                }
            }
        }
    
        private void AssertReceivedSingle(int index, String stringValue)
        {
            for (var i = 0; i < _listeners.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }
                Assert.IsFalse(_listeners[i].IsInvoked);
            }
            Assert.AreEqual(stringValue, _listeners[index].AssertOneGetNewAndReset().Get("TheString"));
        }
    
        private void AssertReceivedNone()
        {
            for (var i = 0; i < _listeners.Length; i++)
            {
                Assert.IsFalse(_listeners[i].IsInvoked);
            }
        }
    
        private SupportBean SendSupportBean(String theString, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void RunAssertion(EPStatement stmtOrig)
        {
            stmtOrig.Events += _listener.Update;
    
            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL("select * from BStream");
            stmtTwo.Events += _listeners[1].Update;
    
            Assert.AreNotSame(stmtOne.EventType, stmtTwo.EventType);
            Assert.AreSame(stmtOne.EventType.UnderlyingType, stmtTwo.EventType.UnderlyingType);
    
            SendSupportBean("E1", 1);
            AssertReceivedSingle(0, "E1");
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E2", 2);
            AssertReceivedSingle(1, "E2");
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E3", 1);
            AssertReceivedSingle(0, "E3");
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBean("E4", -999);
            AssertReceivedNone();
            Assert.AreEqual("E4", _listener.AssertOneGetNewAndReset().Get("TheString"));
    
            stmtOrig.Dispose();
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    }
}
