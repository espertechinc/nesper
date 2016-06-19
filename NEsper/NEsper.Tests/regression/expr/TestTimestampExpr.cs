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

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestTimestampExpr 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestGetTimestamp()
        {
            SendTimer(0);
            String stmtText = "select current_timestamp(), " +
                              " current_timestamp as t0, " +
                              " current_timestamp() as t1, " +
                              " current_timestamp + 1 as t2 " +
                              " from " + typeof(SupportBean).FullName;
    
            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += _listener.Update;

            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("current_timestamp()"));
            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("t0"));
            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("t1"));
            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("t2"));
    
            SendTimer(100);
            _epService.EPRuntime.SendEvent(new SupportBean());
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {100l, 100l, 101l});
    
            SendTimer(999);
            _epService.EPRuntime.SendEvent(new SupportBean());
            theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {999l, 999l, 1000l});
            Assert.AreEqual(theEvent.Get("current_timestamp()"), theEvent.Get("t0"));
        }
    
        [Test]
        public void TestGetTimestamp_OM()
        {
            SendTimer(0);
            String stmtText = "select current_timestamp() as t0 from " + typeof(SupportBean).FullName;
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.CurrentTimestamp(), "t0");
            model.FromClause = FromClause.Create().Add(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;

            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("t0"));
    
            SendTimer(777);
            _epService.EPRuntime.SendEvent(new SupportBean());
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {777l});
        }
    
        [Test]
        public void TestGetTimestamp_Compile()
        {
            SendTimer(0);
            String stmtText = "select current_timestamp() as t0 from " + typeof(SupportBean).FullName;
    
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
    
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("t0"));
    
            SendTimer(777);
            _epService.EPRuntime.SendEvent(new SupportBean());
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {777l});
        }
    
        private void SendTimer(long timeInMSec)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void AssertResults(EventBean theEvent, Object[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    }
}
