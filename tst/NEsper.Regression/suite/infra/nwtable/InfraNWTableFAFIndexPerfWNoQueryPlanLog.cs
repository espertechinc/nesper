///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableFAFIndexPerfWNoQueryPlanLog : IndexBackingTableInfo
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithKeyBTreePerformance(execs);
            WithKeyAndRangePerformance(execs);
            WithRangePerformance(execs);
            WithKeyPerformance(execs);
            WithInKeywordSingleIndex(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInKeywordSingleIndex(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraFAFInKeywordSingleIndex(true));
            execs.Add(new InfraFAFInKeywordSingleIndex(false));
            return execs;
        }

        public static IList<RegressionExecution> WithKeyPerformance(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraFAFKeyPerformance(true));
            execs.Add(new InfraFAFKeyPerformance(false));
            return execs;
        }

        public static IList<RegressionExecution> WithRangePerformance(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraFAFRangePerformance(true));
            execs.Add(new InfraFAFRangePerformance(false));
            return execs;
        }

        public static IList<RegressionExecution> WithKeyAndRangePerformance(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraFAFKeyAndRangePerformance(true));
            execs.Add(new InfraFAFKeyAndRangePerformance(false));
            return execs;
        }

        public static IList<RegressionExecution> WithKeyBTreePerformance(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraFAFKeyBTreePerformance(true));
            execs.Add(new InfraFAFKeyBTreePerformance(false));
            return execs;
        }

        private static void RunFAFAssertion(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            int? expected)
        {
            var start = PerformanceObserver.MilliTime;
            var loops = 500;

            var query = Prepare(env, path, epl);
            for (var i = 0; i < loops; i++) {
                RunFAFQuery(query, expected);
            }

            var end = PerformanceObserver.MilliTime;
            var delta = end - start;

#if DEBUG // DEBUG builds are a little slower
            Assert.That(delta, Is.LessThan(2750), "delta=" + delta);
#else
            Assert.That(delta, Is.LessThan(2000), "delta=" + delta);
#endif
        }

        private static void RunFAFQuery(
            EPFireAndForgetPreparedQuery query,
            int? expectedValue)
        {
            var result = query.Execute();
            Assert.AreEqual(1, result.Array.Length);
            Assert.AreEqual(expectedValue, result.Array[0].Get("sumi"));
        }

        private static EPFireAndForgetPreparedQuery Prepare(
            RegressionEnvironment env,
            RegressionPath path,
            string queryText)
        {
            var compiled = env.CompileFAF(queryText, path);
            return env.Runtime.FireAndForgetService.PrepareQuery(compiled);
        }

        internal class InfraFAFKeyBTreePerformance : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraFAFKeyBTreePerformance(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // create window one
                var eplCreate = namedWindow
                    ? "create window MyInfraFAFKB#keepall as SupportBean"
                    : "create table MyInfraFAFKB (TheString string primary key, IntPrimitive int primary key)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("insert into MyInfraFAFKB select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("@Name('Idx') create index Idx1 on MyInfraFAFKB(IntPrimitive btree)", path);

                // insert X rows
                var maxRows = 10000; //for performance testing change to int maxRows = 100000;
                for (var i = 0; i < maxRows; i++) {
                    env.SendEventBean(new SupportBean("A", i));
                }

                env.SendEventBean(new SupportBean("B", 100));

                // fire single-key queries
                var eplIdx1One = "select IntPrimitive as sumi from MyInfraFAFKB where IntPrimitive = 5501";
                RunFAFAssertion(env, path, eplIdx1One, 5501);

                var eplIdx1Two = "select sum(IntPrimitive) as sumi from MyInfraFAFKB where IntPrimitive > 9997";
                RunFAFAssertion(env, path, eplIdx1Two, 9998 + 9999);

                // drop index, create multikey btree
                env.UndeployModuleContaining("Idx");

                env.CompileDeploy("create index Idx2 on MyInfraFAFKB(IntPrimitive btree, TheString btree)", path);

                var eplIdx2One =
                    "select IntPrimitive as sumi from MyInfraFAFKB where IntPrimitive = 5501 and TheString = 'A'";
                RunFAFAssertion(env, path, eplIdx2One, 5501);

                var eplIdx2Two =
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKB where IntPrimitive in [5000:5004) and TheString = 'A'";
                RunFAFAssertion(env, path, eplIdx2Two, 5000 + 5001 + 5003 + 5002);

                var eplIdx2Three =
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKB where IntPrimitive=5001 and TheString between 'A' and 'B'";
                RunFAFAssertion(env, path, eplIdx2Three, 5001);

                env.UndeployAll();
            }
        }

        internal class InfraFAFKeyAndRangePerformance : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraFAFKeyAndRangePerformance(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // create window one
                var eplCreate = namedWindow
                    ? "create window MyInfraFAFKR#keepall as SupportBean"
                    : "create table MyInfraFAFKR (TheString string primary key, IntPrimitive int primary key)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("insert into MyInfraFAFKR select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("create index Idx1 on MyInfraFAFKR(TheString hash, IntPrimitive btree)", path);

                // insert X rows
                var maxRows = 10000; //for performance testing change to int maxRows = 100000;
                for (var i = 0; i < maxRows; i++) {
                    env.SendEventBean(new SupportBean("A", i));
                }

                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive not in [3:9997]",
                    1 + 2 + 9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive not in [3:9997)",
                    1 + 2 + 9997 + 9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive not in (3:9997]",
                    1 + 2 + 3 + 9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive not in (3:9997)",
                    1 + 2 + 3 + 9997 + 9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'B' and IntPrimitive not in (3:9997)",
                    null);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive between 200 and 202",
                    603);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive between 202 and 199",
                    199 + 200 + 201 + 202);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive >= 200 and IntPrimitive <= 202",
                    603);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive >= 202 and IntPrimitive <= 200",
                    null);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive > 9997",
                    9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive >= 9997",
                    9997 + 9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive < 5",
                    4 + 3 + 2 + 1);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive <= 5",
                    5 + 4 + 3 + 2 + 1);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive in [200:202]",
                    603);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive in [200:202)",
                    401);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive in (200:202]",
                    403);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive in (200:202)",
                    201);

                // test no value returned
                var query = Prepare(env, path, "select * from MyInfraFAFKR where TheString = 'A' and IntPrimitive < 0");
                var result = query.Execute();
                Assert.AreEqual(0, result.Array.Length);

                env.UndeployAll();
            }
        }

        internal class InfraFAFRangePerformance : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraFAFRangePerformance(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // create window one
                var eplCreate = namedWindow
                    ? "create window MyInfraRP#keepall as SupportBean"
                    : "create table MyInfraRP (TheString string primary key, IntPrimitive int primary key)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("insert into MyInfraRP select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("create index Idx1 on MyInfraRP(IntPrimitive btree)", path);

                // insert X rows
                var maxRows = 10000; //for performance testing change to int maxRows = 100000;
                for (var i = 0; i < maxRows; i++) {
                    env.SendEventBean(new SupportBean("K", i));
                }

                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive between 200 and 202",
                    603);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive between 202 and 199",
                    199 + 200 + 201 + 202);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive >= 200 and IntPrimitive <= 202",
                    603);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive >= 202 and IntPrimitive <= 200",
                    null);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive > 9997",
                    9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive >= 9997",
                    9997 + 9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive < 5",
                    4 + 3 + 2 + 1);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive <= 5",
                    5 + 4 + 3 + 2 + 1);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive in [200:202]",
                    603);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive in [200:202)",
                    401);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive in (200:202]",
                    403);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive in (200:202)",
                    201);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive not in [3:9997]",
                    1 + 2 + 9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive not in [3:9997)",
                    1 + 2 + 9997 + 9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive not in (3:9997]",
                    1 + 2 + 3 + 9998 + 9999);
                RunFAFAssertion(
                    env,
                    path,
                    "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive not in (3:9997)",
                    1 + 2 + 3 + 9997 + 9998 + 9999);

                // test no value returned
                var query = Prepare(env, path, "select * from MyInfraRP where IntPrimitive < 0");
                var result = query.Execute();
                Assert.AreEqual(0, result.Array.Length);

                env.UndeployAll();
            }
        }

        internal class InfraFAFKeyPerformance : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraFAFKeyPerformance(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                // create window one
                var path = new RegressionPath();
                var stmtTextCreateOne = namedWindow
                    ? "create window MyInfraOne#keepall as (f1 string, f2 int)"
                    : "create table MyInfraOne (f1 string primary key, f2 int primary key)";
                env.CompileDeploy(stmtTextCreateOne, path);
                env.CompileDeploy(
                    "insert into MyInfraOne(f1, f2) select TheString, IntPrimitive from SupportBean",
                    path);
                env.CompileDeploy("create index MyInfraOneIndex on MyInfraOne(f1)", path);

                // insert X rows
                var maxRows = 100; //for performance testing change to int maxRows = 100000;
                for (var i = 0; i < maxRows; i++) {
                    env.SendEventBean(new SupportBean("K" + i, i));
                }

                long start;
                string queryText;
                EPFireAndForgetPreparedQuery query;
                EPFireAndForgetQueryResult result;

                // fire N queries each returning 1 row
                start = PerformanceObserver.MilliTime;
                queryText = "select * from MyInfraOne where f1='K10'";
                query = Prepare(env, path, queryText);
                var loops = 10000;

                for (var i = 0; i < loops; i++) {
                    result = query.Execute();
                    Assert.AreEqual(1, result.Array.Length);
                    Assert.AreEqual("K10", result.Array[0].Get("f1"));
                }

                var end = PerformanceObserver.MilliTime;
                var delta = end - start;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);

                // test no value returned
                queryText = "select * from MyInfraOne where f1='KX'";
                query = Prepare(env, path, queryText);
                result = query.Execute();
                Assert.AreEqual(0, result.Array.Length);

                // test query null
                queryText = "select * from MyInfraOne where f1=null";
                query = Prepare(env, path, queryText);
                result = query.Execute();
                Assert.AreEqual(0, result.Array.Length);

                // insert null and test null
                env.SendEventBean(new SupportBean(null, -2));
                result = query.Execute();
                Assert.AreEqual(0, result.Array.Length);

                // test two values
                env.SendEventBean(new SupportBean(null, -1));
                query = Prepare(env, path, "select * from MyInfraOne where f1 is null order by f2 asc");
                result = query.Execute();
                Assert.AreEqual(2, result.Array.Length);
                Assert.AreEqual(-2, result.Array[0].Get("f2"));
                Assert.AreEqual(-1, result.Array[1].Get("f2"));

                env.UndeployAll();
            }
        }

        internal class InfraFAFInKeywordSingleIndex : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraFAFInKeywordSingleIndex(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "create window MyInfraIKW#keepall as SupportBean"
                    : "create table MyInfraIKW (TheString string primary key)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("create index Idx on MyInfraIKW(TheString)", path);
                env.CompileDeploy("insert into MyInfraIKW select TheString from SupportBean", path);

                var eventCount = 10;
                for (var i = 0; i < eventCount; i++) {
                    env.SendEventBean(new SupportBean("E" + i, 0));
                }

                InvocationCounter.Count = 0;
                var fafEPL = "select * from MyInfraIKW as mw where justCount(mw) and TheString in ('notfound')";
                env.CompileExecuteFAF(fafEPL, path);
                Assert.AreEqual(0, InvocationCounter.Count);

                env.UndeployAll();
            }
        }

        public class InvocationCounter
        {
            public static int Count { get; set; }

            public static bool JustCount(object o)
            {
                Count++;
                return true;
            }
        }
    }
} // end of namespace