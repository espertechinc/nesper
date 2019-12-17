///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety for creating and stopping various statements.
    /// </summary>
    public class MultithreadStmtMgmt : RegressionExecution
    {
        private static readonly string EVENT_NAME = typeof(SupportMarketDataBean).Name;

        private static readonly string[] EPL = {
            "select * from " + EVENT_NAME + " where Symbol = 'IBM'",
            "select * from " + EVENT_NAME + " (Symbol = 'IBM')",
            "select * from " + EVENT_NAME + " (Price>1)",
            "select * from " + EVENT_NAME + " (Feed='RT')",
            "select * from " + EVENT_NAME + " (Symbol='IBM', Price>1, Feed='RT')",
            "select * from " + EVENT_NAME + " (Price>1, Feed='RT')",
            "select * from " + EVENT_NAME + " (Symbol='IBM', Feed='RT')",
            "select * from " + EVENT_NAME + " (Symbol='IBM', Feed='RT') where Price between 0 and 1000",
            "select * from " + EVENT_NAME + " (Symbol='IBM') where Price between 0 and 1000 and Feed='RT'",
            "select * from " + EVENT_NAME + " (Symbol='IBM') where 'a'='a'",
            "select a.* from pattern[every a=" + EVENT_NAME + "(Symbol='IBM')]",
            "select a.* from pattern[every a=" + EVENT_NAME + "(Symbol='IBM', Price < 1000)]",
            "select a.* from pattern[every a=" + EVENT_NAME + "(Feed='RT', Price < 1000)]",
            "select a.* from pattern[every a=" + EVENT_NAME + "(Symbol='IBM', Feed='RT')]"
        };

        public void Run(RegressionEnvironment env)
        {
            var eplAndStmt = new StmtMgmtCallablePair[EPL.Length];
            for (var i = 0; i < EPL.Length; i++) {
                var compiled = env.Compile(EPL[i]);
                eplAndStmt[i] = new StmtMgmtCallablePair(EPL[i], compiled);
            }

            RunAssertionPatterns(env, eplAndStmt);
            RunAssertionEachStatementAlone(env, eplAndStmt);
            RunAssertionStatementsMixed(env, eplAndStmt);
            RunAssertionStatementsAll(env, eplAndStmt);
        }

        private static void RunAssertionPatterns(
            RegressionEnvironment env,
            StmtMgmtCallablePair[] eplAndStmt)
        {
            var numThreads = 3;
            StmtMgmtCallablePair[] statements;

            statements = new[] {eplAndStmt[10]};
            TryStatementCreateSendAndStop(env, numThreads, statements, 10);
            env.UndeployAll();

            statements = new[] {eplAndStmt[10], eplAndStmt[11]};
            TryStatementCreateSendAndStop(env, numThreads, statements, 10);
            env.UndeployAll();

            statements = new[] {eplAndStmt[10], eplAndStmt[11], eplAndStmt[12]};
            TryStatementCreateSendAndStop(env, numThreads, statements, 10);
            env.UndeployAll();

            statements = new[] {eplAndStmt[10], eplAndStmt[11], eplAndStmt[12], eplAndStmt[13]};
            TryStatementCreateSendAndStop(env, numThreads, statements, 10);
            env.UndeployAll();
        }

        private static void RunAssertionEachStatementAlone(
            RegressionEnvironment env,
            StmtMgmtCallablePair[] eplAndStmt)
        {
            var numThreads = 4;
            for (var i = 0; i < eplAndStmt.Length; i++) {
                StmtMgmtCallablePair[] statements = {eplAndStmt[i]};
                TryStatementCreateSendAndStop(env, numThreads, statements, 10);
                env.UndeployAll();
            }
        }

        private static void RunAssertionStatementsMixed(
            RegressionEnvironment env,
            StmtMgmtCallablePair[] eplAndStmt)
        {
            var numThreads = 2;
            StmtMgmtCallablePair[] statements =
                {eplAndStmt[1], eplAndStmt[4], eplAndStmt[6], eplAndStmt[7], eplAndStmt[8]};
            TryStatementCreateSendAndStop(env, numThreads, statements, 10);
            env.UndeployAll();

            statements = new[] {eplAndStmt[1], eplAndStmt[7], eplAndStmt[8], eplAndStmt[11], eplAndStmt[12]};
            TryStatementCreateSendAndStop(env, numThreads, statements, 10);
            env.UndeployAll();
        }

        private static void RunAssertionStatementsAll(
            RegressionEnvironment env,
            StmtMgmtCallablePair[] eplAndStmt)
        {
            var numThreads = 3;
            TryStatementCreateSendAndStop(env, numThreads, eplAndStmt, 10);
            env.UndeployAll();
        }

        private static void TryStatementCreateSendAndStop(
            RegressionEnvironment env,
            int numThreads,
            StmtMgmtCallablePair[] statements,
            int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtMgmt)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtMgmtCallable(env.Runtime, statements, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);

            var statementDigest = new StringBuilder();
            for (var i = 0; i < statements.Length; i++) {
                statementDigest.Append(statements[i].Epl);
            }

            SupportCompileDeployUtil.AssertFutures(future);
        }
    }
} // end of namespace