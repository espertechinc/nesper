///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using Common.Logging;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadFireAndForgetIndex : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int NUMREPEATS_QUERY = 10000;

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var epl = "@public create window MyWindow#keepall as (key string, value string);\n" +
                      "create index MyIndex on MyWindow(key);\n" +
                      "on SupportBean_S0 merge MyWindow insert select P00 as key, P01 as value;\n" +
                      "on SupportBean_S1 as s1 delete from MyWindow as mw where mw.key = s1.P10;\n";
            env.CompileDeploy(epl, path);
            SendS0(env, 1, "A");

            var faf = "select * from MyWindow where key = 'A' and value like '%hello%'";
            var query = Prepare(env, path, faf);
            var threadPool = Executors.NewFixedThreadPool(
                3,
                new SupportThreadFactory(typeof(MultithreadFireAndForgetIndex)).ThreadFactory);
            var runnable = new QueryRunnable(NUMREPEATS_QUERY, query);
            threadPool.Submit(runnable.Run);

            for (var i = 0; i < NUMREPEATS_QUERY; i++) {
                SendS0(env, 0, "A");
                SendS1(env, 0, "A");
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeUnitHelper.ToTimeSpan(10, TimeUnit.SECONDS));
            ClassicAssert.IsNull(runnable.Exception);

            env.UndeployAll();
        }

        private void SendS0(
            RegressionEnvironment env,
            int id,
            string p00)
        {
            env.SendEventBean(new SupportBean_S0(id, p00));
        }

        private void SendS1(
            RegressionEnvironment env,
            int id,
            string p10)
        {
            env.SendEventBean(new SupportBean_S1(id, p10));
        }

        private EPFireAndForgetPreparedQuery Prepare(
            RegressionEnvironment env,
            RegressionPath path,
            string faf)
        {
            var compiled = env.CompileFAF(faf, path);
            return env.Runtime.FireAndForgetService.PrepareQuery(compiled);
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public class QueryRunnable : IRunnable
        {
            private readonly int numRepeats;
            private readonly EPFireAndForgetPreparedQuery query;
            private Exception exception;

            public QueryRunnable(
                int numRepeats,
                EPFireAndForgetPreparedQuery query)
            {
                this.numRepeats = numRepeats;
                this.query = query;
            }

            public void Run()
            {
                try {
                    for (var i = 0; i < numRepeats; i++) {
                        query.Execute();
                    }
                }
                catch (Exception ex) {
                    Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                    this.exception = exception;
                }
            }

            public Exception Exception => exception;
        }
    }
} // end of namespace