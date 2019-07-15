///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of setting and reading variables.
    ///     <para />
    ///     Assume we have 2 statements that set 3 variables, and one statement that selects variables:
    ///     <para />
    ///     Result: If 4 threads send A and B events and assign a random value, then var1 and var2 should always be the same
    ///     value
    ///     both when selected in the select statement.
    ///     In addition, the counter var3 should not miss a single value when posted to listeners of the set-statements.
    ///     Each thread sends for each loop one A, B and C event, and returns the result for all "var3" values for checking
    ///     when done.
    /// </summary>
    public class MultithreadVariables : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var listenerSetOne = new SupportMTUpdateListener();
            var listenerSetTwo = new SupportMTUpdateListener();

            var stmtSetOneText =
                "@Name('setOne') on SupportBean set var1=LongPrimitive, var2=LongPrimitive, var3=var3+1";
            var stmtSetTwoText = "@Name('setTwo')on SupportMarketDataBean set var1=Volume, var2=Volume, var3=var3+1";
            env.CompileDeploy(stmtSetOneText).Statement("setOne").AddListener(listenerSetOne);
            env.CompileDeploy(stmtSetTwoText).Statement("setTwo").AddListener(listenerSetTwo);

            TrySetAndReadAtomic(env, listenerSetOne, listenerSetTwo, 2, 10000);

            env.UndeployAll();
        }

        private static void TrySetAndReadAtomic(
            RegressionEnvironment env,
            SupportMTUpdateListener listenerSetOne,
            SupportMTUpdateListener listenerSetTwo,
            int numThreads,
            int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadVariables)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new VariableReadWriteCallable(i, env, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ThreadpoolAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            // Determine if we have all numbers for var3 and didn't skip one.
            // Since "var3 = var3 + 1" is executed by multiple statements and threads we need to have
            // this counter have all the values from 0 to N-1.
            ISet<long> var3Values = new SortedSet<long>();
            foreach (var theEvent in listenerSetOne.GetNewDataListFlattened()) {
                var3Values.Add(theEvent.Get("var3").AsLong());
            }

            foreach (var theEvent in listenerSetTwo.GetNewDataListFlattened()) {
                var3Values.Add(theEvent.Get("var3").AsLong());
            }

            Assert.AreEqual(numThreads * numRepeats, var3Values.Count);
            for (var i = 1; i < numThreads * numRepeats + 1; i++) {
                Assert.IsTrue(var3Values.Contains(i));
            }
        }
    }
} // end of namespace