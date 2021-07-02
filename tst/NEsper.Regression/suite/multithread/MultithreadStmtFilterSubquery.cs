///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    public class MultithreadStmtFilterSubquery : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            TryNamedWindowFilterSubquery(env);
            TryStreamFilterSubquery(env);
        }

        private static void TryNamedWindowFilterSubquery(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create window MyWindow#keepall as SupportBean_S0", path);
            env.CompileDeploy("insert into MyWindow select * from SupportBean_S0", path);

            var epl =
                "select * from pattern[SupportBean_S0 -> SupportBean(not exists (select * from MyWindow mw where mw.P00 = 'E'))]";
            env.CompileDeploy(epl, path);
            env.SendEventBean(new SupportBean_S0(1));

            var insertThread = new Thread(new InsertRunnable(env.Runtime, 1000).Run);
            insertThread.Name = nameof(MultithreadStmtFilterSubquery) + "-insert";
            var filterThread = new Thread(new FilterRunnable(env.Runtime, 1000).Run);
            filterThread.Name = nameof(MultithreadStmtFilterSubquery) + "-filter";

            log.Info("Starting threads");
            insertThread.Start();
            filterThread.Start();

            log.Info("Waiting for join");
            ThreadJoin(insertThread);
            ThreadJoin(filterThread);

            env.UndeployAll();
        }

        private static void TryStreamFilterSubquery(RegressionEnvironment env)
        {
            var epl =
                "select * from SupportBean(not exists (select * from SupportBean_S0#keepall mw where mw.P00 = 'E'))";
            env.CompileDeploy(epl);

            var insertThread = new Thread(
                new InsertRunnable(env.Runtime, 1000).Run);
            insertThread.Name = nameof(MultithreadStmtFilterSubquery) + "-insert";

            var filterThread = new Thread(
                new FilterRunnable(env.Runtime, 1000).Run);
            filterThread.Name = nameof(MultithreadStmtFilterSubquery) + "-filter";

            log.Info("Starting threads");
            insertThread.Start();
            filterThread.Start();

            log.Info("Waiting for join");
            ThreadJoin(insertThread);
            ThreadJoin(filterThread);

            env.UndeployAll();
        }

        public class InsertRunnable : IRunnable
        {
            private readonly int numInserts;
            private readonly EPRuntime runtime;

            public InsertRunnable(
                EPRuntime runtime,
                int numInserts)
            {
                this.runtime = runtime;
                this.numInserts = numInserts;
            }

            public void Run()
            {
                log.Info("Starting insert thread");
                for (var i = 0; i < numInserts; i++) {
                    runtime.EventService.SendEventBean(new SupportBean_S0(i, "E"), "SupportBean_S0");
                }

                log.Info("Completed insert thread, " + numInserts + " inserted");
            }
        }

        public class FilterRunnable : IRunnable
        {
            private readonly int numEvents;
            private readonly EPRuntime runtime;

            public FilterRunnable(
                EPRuntime runtime,
                int numEvents)
            {
                this.runtime = runtime;
                this.numEvents = numEvents;
            }

            public void Run()
            {
                log.Info("Starting filter thread");
                for (var i = 0; i < numEvents; i++) {
                    runtime.EventService.SendEventBean(new SupportBean("G" + i, i), "SupportBean");
                }

                log.Info("Completed filter thread, " + numEvents + " completed");
            }
        }
    }
} // end of namespace