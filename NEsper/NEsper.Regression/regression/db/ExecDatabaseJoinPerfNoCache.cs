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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseJoinPerfNoCache : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);
    
            var epServiceRetained = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, "TestDatabaseJoinRetained", configuration);
            epServiceRetained.Initialize();
    
            configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.POOLED;
            configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);
            var epServicePooled = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, "TestDatabaseJoinPooled", configuration);
            epServicePooled.Initialize();
    
            RunAssertion100EventsRetained(epServiceRetained);
            RunAssertion100EventsPooled(epServicePooled);
            RunAssertionSelectRStream(epServiceRetained);
            RunAssertionSelectIStream(epServiceRetained);
            RunAssertionWhereClauseNoIndexNoCache(epServiceRetained);
    
            epServicePooled.Dispose();
            epServiceRetained.Dispose();
        }
    
        private void RunAssertion100EventsRetained(EPServiceProvider epServiceRetained)
        {
            var startTime = DateTimeHelper.CurrentTimeMillis;
            Try100Events(epServiceRetained);
            var endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(".test100EventsRetained delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 5000);
        }
    
        private void RunAssertion100EventsPooled(EPServiceProvider epServicePooled)
        {
            var startTime = DateTimeHelper.CurrentTimeMillis;
            Try100Events(epServicePooled);
            var endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(".test100EventsPooled delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 10000);
        }
    
        private void RunAssertionSelectRStream(EPServiceProvider epServiceRetained)
        {
            var stmtText = "select rstream myvarchar from " +
                    typeof(SupportBean_S0).FullName + "#length(1000) as s0," +
                    " sql:MyDB ['select myvarchar from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            var statement = epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // 1000 events should enter the window fast, no joins
            var startTime = DateTimeHelper.CurrentTimeMillis;
            for (var i = 0; i < 1000; i++) {
                var beanX = new SupportBean_S0(10);
                epServiceRetained.EPRuntime.SendEvent(beanX);
                Assert.IsFalse(listener.IsInvoked);
            }
            var endTime = DateTimeHelper.CurrentTimeMillis;
            var delta = endTime - startTime;
            Assert.IsTrue(endTime - startTime < 1000, "delta=" + delta);
    
            // 1001st event should finally join and produce a result
            var bean = new SupportBean_S0(10);
            epServiceRetained.EPRuntime.SendEvent(bean);
            Assert.AreEqual("J", listener.AssertOneGetNewAndReset().Get("myvarchar"));
    
            statement.Dispose();
        }
    
        private void RunAssertionSelectIStream(EPServiceProvider epServiceRetained)
        {
            // set time to zero
            epServiceRetained.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            var stmtText = "select istream myvarchar from " +
                    typeof(SupportBean_S0).FullName + "#time(1 sec) as s0," +
                    " sql:MyDB ['select myvarchar from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            var statement = epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // Send 100 events which all fireStatementStopped a join
            for (var i = 0; i < 100; i++) {
                var bean = new SupportBean_S0(5);
                epServiceRetained.EPRuntime.SendEvent(bean);
                Assert.AreEqual("E", listener.AssertOneGetNewAndReset().Get("myvarchar"));
            }
    
            // now advance the time, this should not produce events or join
            var startTime = DateTimeHelper.CurrentTimeMillis;
            epServiceRetained.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            var endTime = DateTimeHelper.CurrentTimeMillis;
    
            Log.Info(".testSelectIStream delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 200);
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionWhereClauseNoIndexNoCache(EPServiceProvider epServiceRetained)
        {
            var stmtText = "select id, mycol3, mycol2 from " +
                    typeof(SupportBean_S0).FullName + "#keepall as s0," +
                    " sql:MyDB ['select mycol3, mycol2 from mytesttable_large'] as s1 where s0.id = s1.mycol3";
    
            var statement = epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            for (var i = 0; i < 20; i++) {
                var num = i + 1;
                var col2 = Convert.ToString(Math.Round((float) num / 10, MidpointRounding.AwayFromZero));
                var bean = new SupportBean_S0(num);
                epServiceRetained.EPRuntime.SendEvent(bean);
                var testEventBean = listener.AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    testEventBean,
                    new string[]{"id", "mycol3", "mycol2"}, 
                    new object[]{num, num, col2});
            }
    
            statement.Dispose();
        }
    
        private void Try100Events(EPServiceProvider engine)
        {
            var stmtText = "select myint from " +
                    typeof(SupportBean_S0).FullName + " as s0," +
                    " sql:MyDB ['select myint from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            var statement = engine.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            for (var i = 0; i < 100; i++) {
                var id = i % 10 + 1;
    
                var bean = new SupportBean_S0(id);
                engine.EPRuntime.SendEvent(bean);
    
                var received = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(id * 10, received.Get("myint"));
            }
    
            statement.Dispose();
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
