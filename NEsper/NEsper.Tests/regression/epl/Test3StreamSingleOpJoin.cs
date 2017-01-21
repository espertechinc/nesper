///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class Test3StreamSingleOpJoin
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();

            _eventsA = new SupportBean_A[10];
            _eventsB = new SupportBean_B[10];
            _eventsC = new SupportBean_C[10];
            for (int i = 0; i < _eventsA.Length; i++)
            {
                _eventsA[i] = new SupportBean_A(Convert.ToString(i));
                _eventsB[i] = new SupportBean_B(Convert.ToString(i));
                _eventsC[i] = new SupportBean_C(Convert.ToString(i));
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;

        private SupportBean_A[] _eventsA;
        private SupportBean_B[] _eventsB;
        private SupportBean_C[] _eventsC;

        private readonly string _eventA = typeof(SupportBean_A).FullName;
        private readonly string _eventB = typeof(SupportBean_B).FullName;
        private readonly string _eventC = typeof(SupportBean_C).FullName;

        private void RunJoinUniquePerId()
        {
            // Test sending a C event
            SendEvent(_eventsA[0]);
            SendEvent(_eventsB[0]);
            Assert.IsNull(_updateListener.LastNewData);
            SendEvent(_eventsC[0]);
            AssertEventsReceived(_eventsA[0], _eventsB[0], _eventsC[0]);

            // Test sending a B event
            SendEvent(new Object[] {_eventsA[1], _eventsB[2], _eventsC[3]});
            SendEvent(_eventsC[1]);
            Assert.IsNull(_updateListener.LastNewData);
            SendEvent(_eventsB[1]);
            AssertEventsReceived(_eventsA[1], _eventsB[1], _eventsC[1]);

            // Test sending a C event
            SendEvent(new Object[] {_eventsA[4], _eventsA[5], _eventsB[4], _eventsB[3]});
            Assert.IsNull(_updateListener.LastNewData);
            SendEvent(_eventsC[4]);
            AssertEventsReceived(_eventsA[4], _eventsB[4], _eventsC[4]);
            Assert.IsNull(_updateListener.LastNewData);
        }

        private void AssertEventsReceived(SupportBean_A event_A, SupportBean_B event_B, SupportBean_C event_C)
        {
            Assert.AreEqual(1, _updateListener.LastNewData.Length);
            Assert.AreSame(event_A, _updateListener.LastNewData[0].Get("streamA"));
            Assert.AreSame(event_B, _updateListener.LastNewData[0].Get("streamB"));
            Assert.AreSame(event_C, _updateListener.LastNewData[0].Get("streamC"));
            _updateListener.Reset();
        }

        private void SendEvent(Object theEvent)
        {
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendEvent(Object[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
                _epService.EPRuntime.SendEvent(events[i]);
            }
        }

        [Test]
        public void TestJoinUniquePerId()
        {
            String joinStatement = "select * from " +
                                   _eventA + ".win:length(3) as streamA," +
                                   _eventB + ".win:length(3) as streamB," +
                                   _eventC + ".win:length(3) as streamC" +
                                   " where (streamA.id = streamB.id) " +
                                   "   and (streamB.id = streamC.id)" +
                                   "   and (streamA.id = streamC.id)";

            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;

            RunJoinUniquePerId();
        }

        [Test]
        public void TestJoinUniquePerIdCompile()
        {
            String joinStatement = "select * from " +
                                   _eventA + ".win:length(3) as streamA, " +
                                   _eventB + ".win:length(3) as streamB, " +
                                   _eventC + ".win:length(3) as streamC " +
                                   "where streamA.id=streamB.id " +
                                   "and streamB.id=streamC.id " +
                                   "and streamA.id=streamC.id";

            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(joinStatement);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            EPStatement joinView = _epService.EPAdministrator.Create(model);
            joinView.Events += _updateListener.Update;
            Assert.AreEqual(joinStatement, model.ToEPL());

            RunJoinUniquePerId();
        }

        [Test]
        public void TestJoinUniquePerIdOM()
        {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model.FromClause = FromClause.Create(
                FilterStream.Create(_eventA, "streamA").AddView(View.Create("win", "length", Expressions.Constant(3))),
                FilterStream.Create(_eventB, "streamB").AddView(View.Create("win", "length", Expressions.Constant(3))),
                FilterStream.Create(_eventC, "streamC").AddView(View.Create("win", "length", Expressions.Constant(3))));
            model.WhereClause = Expressions.And(
                Expressions.EqProperty("streamA.id", "streamB.id"),
                Expressions.EqProperty("streamB.id", "streamC.id"),
                Expressions.EqProperty("streamA.id", "streamC.id"));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

            String joinStatement = "select * from " +
                                   _eventA + ".win:length(3) as streamA, " +
                                   _eventB + ".win:length(3) as streamB, " +
                                   _eventC + ".win:length(3) as streamC " +
                                   "where streamA.id=streamB.id " +
                                   "and streamB.id=streamC.id " +
                                   "and streamA.id=streamC.id";

            EPStatement joinView = _epService.EPAdministrator.Create(model);
            joinView.Events += _updateListener.Update;
            Assert.AreEqual(joinStatement, model.ToEPL());

            RunJoinUniquePerId();
        }
    }
}
