///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadContextMultiStmtStartEnd : RegressionExecutionPreConfigured
    {
        private EPRuntimeProvider _runtimeProvider;
        private readonly Configuration _configuration;

        public MultithreadContextMultiStmtStartEnd(Configuration configuration)
        {
            _configuration = configuration;
        }

        public void Run()
        {
            _runtimeProvider = new EPRuntimeProvider();

            _configuration.Runtime.Threading.IsInternalTimerEnabled = true;
            RunAssertion(FilterServiceProfile.READMOSTLY, _configuration);
            RunAssertion(FilterServiceProfile.READWRITE, _configuration);
        }

        private void RunAssertion(
            FilterServiceProfile profile,
            Configuration configuration)
        {
            configuration.Runtime.Execution.FilterServiceProfile = profile;
            configuration.Common.AddEventType(typeof(MyEvent));

            var runtimeURI = GetType().Name + "_" + profile;
            var runtime = _runtimeProvider.GetRuntimeInstance(runtimeURI, configuration);

            var path = new RegressionPath();
            var eplContext = "@public create context MyContext start @now end after 100 milliseconds;\n";
            var compiledContext = SupportCompileDeployUtil.Compile(eplContext, configuration, path);
            SupportCompileDeployUtil.Deploy(compiledContext, runtime);
            path.Add(compiledContext);

            var epl = "context MyContext select FieldOne, count(*) as cnt from MyEvent " +
                      "group by FieldOne output last when terminated;\n";
            var compiledStmt = SupportCompileDeployUtil.Compile(epl, configuration, path);
            var listeners = new SupportUpdateListener[100];

            for (var i = 0; i < 100; i++) {
                listeners[i] = new SupportUpdateListener();
                var stmtName = "s" + i;
                SupportCompileDeployUtil.DeployAddListener(compiledStmt, stmtName, listeners[i], runtime);
            }

            var eventCount = 100000; // keep this divisible by 1000
            for (var i = 0; i < eventCount; i++) {
                var group = Convert.ToString(eventCount % 1000);
                runtime.EventService.SendEventBean(new MyEvent(Convert.ToString(i), group), "MyEvent");
            }

            SupportCompileDeployUtil.ThreadSleep(2000);

            AssertReceived(eventCount, listeners);

            try {
                runtime.DeploymentService.UndeployAll();
            }
            catch (EPUndeployException e) {
                throw new EPException(e);
            }

            runtime.Destroy();
        }

        private static void AssertReceived(
            int eventCount,
            SupportUpdateListener[] listeners)
        {
            foreach (var listener in listeners) {
                var outputEvents = listener.NewDataListFlattened;
                long total = 0;

                foreach (var @out in outputEvents) {
                    var cnt = @out.Get("cnt").AsInt64();
                    total += cnt;
                }

                if (total != eventCount) {
                    Assert.Fail("Listener received " + total + " expected " + eventCount);
                }
            }
        }

        public class MyEvent
        {
            public MyEvent(
                string id,
                string fieldOne)
            {
                Id = id;
                FieldOne = fieldOne;
            }

            public string Id { get; }

            public string FieldOne { get; }
        }
    }
} // end of namespace