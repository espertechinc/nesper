///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterOptimizablePerf
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOr(execs);
            WithEqualsWithFunc(execs);
            WithTrueWithFunc(execs);
            WithEqualsDeclaredExpr(execs);
            WithTrueDeclaredExpr(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTrueDeclaredExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptimizablePerfTrueDeclaredExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsDeclaredExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptimizablePerfEqualsDeclaredExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithTrueWithFunc(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptimizablePerfTrueWithFunc());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsWithFunc(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptimizablePerfEqualsWithFunc());
            return execs;
        }

        public static IList<RegressionExecution> WithOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptimizablePerfOr());
            return execs;
        }

        private class ExprFilterOptimizablePerfEqualsWithFunc : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                // func(...) = value
                TryOptimizableEquals(
                    env,
                    new RegressionPath(),
                    "select * from SupportBean(libSplit(theString) = !NUM!)",
                    10);
            }
        }

        private class ExprFilterOptimizablePerfTrueWithFunc : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                // func(...) implied true
                TryOptimizableBoolean(env, new RegressionPath(), "select * from SupportBean(libE1True(theString))");
            }
        }

        private class ExprFilterOptimizablePerfEqualsDeclaredExpr : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                // declared expression (...) = value
                var path = new RegressionPath();
                env.CompileDeploy(
                        "@name('create-expr') @public create expression thesplit {theString => libSplit(theString)}",
                        path)
                    .AddListener("create-expr");
                TryOptimizableEquals(env, path, "select * from SupportBean(thesplit(*) = !NUM!)", 10);
            }
        }

        private class ExprFilterOptimizablePerfTrueDeclaredExpr : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(
                    RegressionFlag.EXCLUDEWHENINSTRUMENTED,
                    RegressionFlag.OBSERVEROPS,
                    RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                // declared expression (...) implied true
                var path = new RegressionPath();
                env.CompileDeploy(
                        "@name('create-expr') @public create expression theE1Test {theString => libE1True(theString)}",
                        path)
                    .AddListener("create-expr");
                TryOptimizableBoolean(env, path, "select * from SupportBean(theE1Test(*))");
            }
        }

        private class ExprFilterOptimizablePerfOr : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(
                    RegressionFlag.EXCLUDEWHENINSTRUMENTED,
                    RegressionFlag.OBSERVEROPS,
                    RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var listener = new SupportUpdateListener();
                for (var i = 0; i < 100; i++) {
                    var epl = "@name('s" +
                              i +
                              "') select * from SupportBean(theString = '" +
                              i +
                              "' or intPrimitive=" +
                              i +
                              ")";
                    var compiled = env.Compile(epl);
                    env.Deploy(compiled).Statement("s" + i).AddListener(listener);
                }

                var start = PerformanceObserver.NanoTime;
                // Console.WriteLine("Starting " + DateTime.print(new Date()));
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean("100", 1));
                    Assert.IsTrue(listener.IsInvoked);
                    listener.Reset();
                }

                // Console.WriteLine("Ending " + DateTime.print(new Date()));
                var delta = (PerformanceObserver.NanoTime - start) / 1000d / 1000d;
                // Console.WriteLine("Delta=" + (delta + " msec"));
                Assert.IsTrue(delta < 500);

                env.UndeployAll();
            }
        }

        private static void TryOptimizableEquals(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            int numStatements)
        {
            // test function returns lookup value and "equals"
            for (var i = 0; i < numStatements; i++) {
                var text = "@name('s" + i + "') " + epl.Replace("!NUM!", Convert.ToString(i));
                env.CompileDeploy(text, path).AddListener("s" + i);
            }

            env.Milestone(0);

            var startTime = PerformanceObserver.MilliTime;
            SupportStaticMethodLib.ResetCountInvoked();
            var loops = 1000;
            for (var i = 0; i < loops; i++) {
                env.SendEventBean(new SupportBean("E_" + i % numStatements, 0));
                var stmtName = "s" + i % numStatements;
                env.AssertListenerInvoked(stmtName);
            }

            var delta = PerformanceObserver.MilliTime - startTime;
            Assert.AreEqual(loops, SupportStaticMethodLib.CountInvoked);

            Assert.That(delta, Is.LessThan(1000), "Delta is " + delta);
            env.UndeployAll();
        }

        private static void TryOptimizableBoolean(
            RegressionEnvironment env,
            RegressionPath path,
            string epl)
        {
            // test function returns lookup value and "equals"
            var count = 10;
            for (var i = 0; i < count; i++) {
                var compiled = env.Compile("@name('s" + i + "')" + epl, path);
                var admin = env.Runtime.DeploymentService;
                try {
                    admin.Deploy(compiled);
                }
                catch (EPDeployException ex) {
                    Console.WriteLine(ex.StackTrace);
                    Assert.Fail();
                }
            }

            env.Milestone(0);

            var listener = new SupportUpdateListener();
            for (var i = 0; i < 10; i++) {
                env.Statement("s" + i).AddListener(listener);
            }

            var startTime = PerformanceObserver.MilliTime;
            SupportStaticMethodLib.ResetCountInvoked();
            var loops = 10000;
            for (var i = 0; i < loops; i++) {
                var key = "E_" + i % 100;
                env.SendEventBean(new SupportBean(key, 0));
                if (key.Equals("E_1")) {
                    Assert.AreEqual(count, listener.NewDataList.Count);
                    listener.Reset();
                }
                else {
                    Assert.IsFalse(listener.IsInvoked);
                }
            }

            var delta = PerformanceObserver.MilliTime - startTime;
            Assert.AreEqual(loops, SupportStaticMethodLib.CountInvoked);

            Assert.That(delta, Is.LessThan(1000), "Delta is " + delta);
            env.UndeployAll();
        }
    }
} // end of namespace