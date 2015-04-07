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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabaseJoinPerfNoCache 
    {
        private EPServiceProvider _epServiceRetained;
        private EPServiceProvider _epServicePooled;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);
            configuration.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;

            _epServiceRetained = EPServiceProviderManager.GetProvider("TestDatabaseJoinRetained", configuration);
            _epServiceRetained.Initialize();

            configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.POOLED;
            configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);
            configuration.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
             _epServicePooled = EPServiceProviderManager.GetProvider("TestDatabaseJoinPooled", configuration);
            _epServicePooled.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
            _epServicePooled.Dispose();
            _epServiceRetained.Dispose();
        }
    
        [Test]
        public void Test100EventsRetained()
        {
            long startTime = PerformanceObserver.MilliTime;
            Try100Events(_epServiceRetained);
            long endTime = PerformanceObserver.MilliTime;
            Log.Info(".test100EventsRetained delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 5000);
        }
    
        [Test]
        public void Test100EventsPooled()
        {
            long startTime = PerformanceObserver.MilliTime;
            Try100Events(_epServicePooled);
            long endTime = PerformanceObserver.MilliTime;
            Log.Info(".test100EventsPooled delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 10000);
        }
    
        [Test]
        public void TestSelectRStream()
        {
            String stmtText = "select rstream myvarchar from " +
                    typeof(SupportBean_S0).FullName + ".win:length(1000) as s0," +
                    " sql:MyDB ['select myvarchar from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            // 1000 events should enter the window fast, no joins
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                SupportBean_S0 supportBean = new SupportBean_S0(10);
                _epServiceRetained.EPRuntime.SendEvent(supportBean);
                Assert.IsFalse(_listener.IsInvoked);
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = (endTime - startTime);
            Assert.IsTrue(endTime - startTime < 1000, "delta=" + delta);
    
            // 1001st event should finally join and produce a result
            SupportBean_S0 bean = new SupportBean_S0(10);
            _epServiceRetained.EPRuntime.SendEvent(bean);
            Assert.AreEqual("J", _listener.AssertOneGetNewAndReset().Get("myvarchar"));
        }
    
        [Test]
        public void TestSelectIStream()
        {
            // set time to zero
            _epServiceRetained.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            String stmtText = "select istream myvarchar from " +
                    typeof(SupportBean_S0).FullName + ".win:time(1 sec) as s0," +
                    " sql:MyDB ['select myvarchar from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            // Send 100 events which all fireStatementStopped a join
            for (int i = 0; i < 100; i++)
            {
                SupportBean_S0 bean = new SupportBean_S0(5);
                _epServiceRetained.EPRuntime.SendEvent(bean);
                Assert.AreEqual("E", _listener.AssertOneGetNewAndReset().Get("myvarchar"));
            }
    
            // now advance the time, this should not produce events or join
            long startTime = PerformanceObserver.MilliTime;
            _epServiceRetained.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            long endTime = PerformanceObserver.MilliTime;
    
            Log.Info(".testSelectIStream delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 200);
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestWhereClauseNoIndexNoCache()
        {
            String stmtText = "select id, mycol3, mycol2 from " +
                    typeof(SupportBean_S0).FullName + ".win:keepall() as s0," +
                    " sql:MyDB ['select mycol3, mycol2 from mytesttable_large'] as s1 where s0.id = s1.mycol3";
    
            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            for (int i = 0; i < 20; i++)
            {
                var num = i + 1;
                var col2 = Convert.ToString(Math.Round((float)num / 10, MidpointRounding.AwayFromZero));
                var bean = new SupportBean_S0(num);
                _epServiceRetained.EPRuntime.SendEvent(bean);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), new String[] {"id", "mycol3", "mycol2"}, new Object[] {num, num, col2});
            }
        }
    
        private void Try100Events(EPServiceProvider engine)
        {
            String stmtText = "select myint from " +
                    typeof(SupportBean_S0).FullName + " as s0," +
                    " sql:MyDB ['select myint from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            EPStatement statement = engine.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            for (int i = 0; i < 100; i++)
            {
                int id = i % 10 + 1;
    
                SupportBean_S0 bean = new SupportBean_S0(id);
                engine.EPRuntime.SendEvent(bean);
    
                EventBean received = _listener.AssertOneGetNewAndReset();
                Assert.AreEqual(id * 10, received.Get("myint"));
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}

