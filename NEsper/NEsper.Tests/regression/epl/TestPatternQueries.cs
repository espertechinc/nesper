///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPatternQueries 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _updateListener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
        }
    
        [Test]
        public void TestWhere_OM()
        {
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().AddWithAsProvidedName("s0.id", "idS0").AddWithAsProvidedName("s1.id", "idS1");
            PatternExpr pattern = Patterns.Or()
                    .Add(Patterns.EveryFilter(typeof(SupportBean_S0).FullName, "s0"))
                    .Add(Patterns.EveryFilter(typeof(SupportBean_S1).FullName, "s1")
                    );
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model.WhereClause = Expressions.Or()
                .Add(Expressions.And()
                    .Add(Expressions.IsNotNull("s0.id"))
                    .Add(Expressions.Lt("s0.id", 100))
                )
                .Add(Expressions.And()
                    .Add(Expressions.IsNotNull("s1.id"))
                    .Add(Expressions.Ge("s1.id", 100))
                );
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            String reverse = model.ToEPL();
            String stmtText = "select s0.id as idS0, s1.id as idS1 " +
                    "from pattern [every s0=" + typeof(SupportBean_S0).FullName +
                    " or every s1=" + typeof(SupportBean_S1).FullName + "] " +
                    "where s0.id is not null and s0.id<100 or s1.id is not null and s1.id>=100";
            Assert.AreEqual(stmtText, reverse);
    
            EPStatement statement = _epService.EPAdministrator.Create(model);
            statement.Events += _updateListener.Update;
    
            SendEventS0(1);
            AssertEventIds(1, null);
    
            SendEventS0(101);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            SendEventS1(1);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            SendEventS1(100);
            AssertEventIds(null, 100);
        }
    
        [Test]
        public void TestWhere_Compile()
        {
            String stmtText = "select s0.id as idS0, s1.id as idS1 " +
                    "from pattern [every s0=" + typeof(SupportBean_S0).FullName +
                    " or every s1=" + typeof(SupportBean_S1).FullName + "] " +
                    "where s0.id is not null and s0.id<100 or s1.id is not null and s1.id>=100";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            String reverse = model.ToEPL();
            Assert.AreEqual(stmtText, reverse);
    
            EPStatement statement = _epService.EPAdministrator.Create(model);
            statement.Events += _updateListener.Update;
    
            SendEventS0(1);
            AssertEventIds(1, null);
    
            SendEventS0(101);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            SendEventS1(1);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            SendEventS1(100);
            AssertEventIds(null, 100);
        }
    
        [Test]
        public void TestWhere()
        {
            String stmtText = "select s0.id as idS0, s1.id as idS1 " +
                    "from pattern [every s0=" + typeof(SupportBean_S0).FullName +
                    " or every s1=" + typeof(SupportBean_S1).FullName + "] " +
                    "where (s0.id is not null and s0.id < 100) or (s1.id is not null and s1.id >= 100)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _updateListener.Update;
    
            SendEventS0(1);
            AssertEventIds(1, null);
    
            SendEventS0(101);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            SendEventS1(1);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            SendEventS1(100);
            AssertEventIds(null, 100);
        }
    
        [Test]
        public void TestAggregation()
        {
            String stmtText = "select sum(s0.id) as sumS0, sum(s1.id) as sumS1, sum(s0.id + s1.id) as sumS0S1 " +
                    "from pattern [every s0=" + typeof(SupportBean_S0).FullName +
                    " or every s1=" + typeof(SupportBean_S1).FullName + "]";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _updateListener.Update;
    
            SendEventS0(1);
            AssertEventSums(1, null, null);
    
            SendEventS1(2);
            AssertEventSums(1, 2, null);
    
            SendEventS1(10);
            AssertEventSums(1, 12, null);
    
            SendEventS0(20);
            AssertEventSums(21, 12, null);
        }
    
        [Test]
        public void TestFollowedByAndWindow()
        {
            String stmtText = "select irstream a.id as idA, b.id as idB, " +
                    "a.P00 as p00A, b.P00 as p00B from pattern [every a=" + typeof(SupportBean_S0).FullName +
                    " -> every b=" + typeof(SupportBean_S0).FullName + "(p00=a.P00)].win:time(1)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
    
            statement.Events += _updateListener.Update;
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            SendEvent(1, "e1a");
            Assert.IsFalse(_updateListener.IsInvoked);
            SendEvent(2, "e1a");
            AssertNewEvent(1, 2, "e1a");
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(500));
            SendEvent(10, "e2a");
            SendEvent(11, "e2b");
            SendEvent(12, "e2c");
            Assert.IsFalse(_updateListener.IsInvoked);
            SendEvent(13, "e2b");
            AssertNewEvent(11, 13, "e2b");
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            AssertOldEvent(1, 2, "e1a");
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
            AssertOldEvent(11, 13, "e2b");
        }
    
        private void AssertNewEvent(int idA, int idB, String p00)
        {
            EventBean eventBean = _updateListener.AssertOneGetNewAndReset();
            CompareEvent(eventBean, idA, idB, p00);
        }
    
        private void AssertOldEvent(int idA, int idB, String p00)
        {
            EventBean eventBean = _updateListener.AssertOneGetOldAndReset();
            CompareEvent(eventBean, idA, idB, p00);
        }
    
        private void CompareEvent(EventBean eventBean, int idA, int idB, String p00)
        {
            Assert.AreEqual(idA, eventBean.Get("idA"));
            Assert.AreEqual(idB, eventBean.Get("idB"));
            Assert.AreEqual(p00, eventBean.Get("p00A"));
            Assert.AreEqual(p00, eventBean.Get("p00B"));
        }
    
        private void SendEvent(int id, String p00)
        {
            SupportBean_S0 theEvent = new SupportBean_S0(id, p00);
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private SupportBean_S0 SendEventS0(int id)
        {
            SupportBean_S0 theEvent = new SupportBean_S0(id);
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean_S1 SendEventS1(int id)
        {
            SupportBean_S1 theEvent = new SupportBean_S1(id);
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private void AssertEventIds(int? idS0, int? idS1)
        {
            EventBean eventBean = _updateListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(idS0, eventBean.Get("idS0"));
            Assert.AreEqual(idS1, eventBean.Get("idS1"));
            _updateListener.Reset();
        }
    
        private void AssertEventSums(int? sumS0, int? sumS1, int? sumS0S1)
        {
            EventBean eventBean = _updateListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(sumS0, eventBean.Get("sumS0"));
            Assert.AreEqual(sumS1, eventBean.Get("sumS1"));
            Assert.AreEqual(sumS0S1, eventBean.Get("sumS0S1"));
            _updateListener.Reset();
        }
    }
}
