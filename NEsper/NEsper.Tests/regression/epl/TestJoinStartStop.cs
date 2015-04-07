///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestJoinStartStop  {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        private Object[] setOne = new Object[5];
        private Object[] setTwo = new Object[5];
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
    
            long[] volumesOne = new long[]{10, 20, 20, 40, 50};
            long[] volumesTwo = new long[]{10, 20, 30, 40, 50};
    
            for (int i = 0; i < setOne.Length; i++) {
                setOne[i] = new SupportMarketDataBean("IBM", volumesOne[i], (long) i, "");
                setTwo[i] = new SupportMarketDataBean("CSCO", volumesTwo[i], (long) i, "");
            }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
        }
    
        [Test]
        public void TestJoinUniquePerId() {
            String joinStatement = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "(Symbol='IBM').win:length(3) s0, " +
                    typeof(SupportMarketDataBean).FullName + "(Symbol='CSCO').win:length(3) s1" +
                    " where s0.Volume=s1.Volume";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement, "MyJoin");
            joinView.Events += _updateListener.Update;
    
            SendEvent(setOne[0]);
            SendEvent(setTwo[0]);
            Assert.NotNull(_updateListener.LastNewData);
            _updateListener.Reset();
    
            joinView.Stop();
            SendEvent(setOne[1]);
            SendEvent(setTwo[1]);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            joinView.Start();
            SendEvent(setOne[2]);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            joinView.Stop();
            SendEvent(setOne[3]);
            SendEvent(setOne[4]);
            SendEvent(setTwo[3]);
    
            joinView.Start();
            SendEvent(setTwo[4]);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            // assert type-statement reference
            EPServiceProviderSPI spi = (EPServiceProviderSPI) _epService;
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportMarketDataBean).FullName));
            ICollection<String> stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportMarketDataBean).FullName);
            Assert.IsTrue(stmtNames.Contains("MyJoin"));
    
            joinView.Dispose();
    
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse(typeof(SupportMarketDataBean).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportMarketDataBean).FullName);
            EPAssertionUtil.AssertEqualsAnyOrder(null, stmtNames.ToArray());
            Assert.IsFalse(stmtNames.Contains("MyJoin"));
        }
    
        [Test]
        public void TestInvalidJoin() {
            _epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            _epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
    
            String invalidJoin = "select * from A, B";
            TryInvalid(invalidJoin,
                    "Error starting statement: Joins require that at least one view is specified for each stream, no view was specified for A [select * from A, B]");
    
            invalidJoin = "select * from A.win:time(5 min), B";
            TryInvalid(invalidJoin,
                    "Error starting statement: Joins require that at least one view is specified for each stream, no view was specified for B [select * from A.win:time(5 min), B]");
    
            invalidJoin = "select * from A.win:time(5 min), pattern[A->B]";
            TryInvalid(invalidJoin,
                    "Error starting statement: Joins require that at least one view is specified for each stream, no view was specified for pattern event stream [select * from A.win:time(5 min), pattern[A->B]]");
        }
    
        private void TryInvalid(String invalidJoin, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(invalidJoin);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void SendEvent(Object theEvent) {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
