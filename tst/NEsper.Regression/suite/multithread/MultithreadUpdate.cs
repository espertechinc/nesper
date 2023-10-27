///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety (or lack thereof) for iterators: iterators fail with concurrent mods as expected
    ///     behavior
    /// </summary>
    public class MultithreadUpdate : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy("@name('s0') select TheString from SupportBean");

            IList<string> strings = new List<string>().AsSyncList();
            env.Statement("s0").Events += (
                    sender,
                    updateEventArgs) =>
                strings.Add((string)updateEventArgs.NewEvents[0].Get("TheString"));

            TrySend(env, 2, 50000);

            var found = false;
            foreach (var value in strings) {
                if (value.Equals("a")) {
                    found = true;
                }
            }

            Assert.IsTrue(found);

            env.UndeployAll();
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats)
        {
            var compiled = env.Compile("@name('upd') update istream SupportBean set TheString='a'");

            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadUpdate)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtUpdateSendCallable(i, env.Runtime, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            for (var i = 0; i < 50; i++) {
                env.Deploy(compiled);
                SupportCompileDeployUtil.ThreadSleep(10);
                env.UndeployModuleContaining("upd");
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 5, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);
        }
    }
} // end of namespace