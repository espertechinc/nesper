///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertTrue

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectNamedWindowPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithConstantValue(execs);
            WithKeyAndRange(execs);
            WithRange(execs);
            WithKeyedRange(execs);
            WithNoShare(execs);
            WithShareCreate(execs);
            WithDisableShare(execs);
            WithDisableShareCreate(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDisableShareCreate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectDisableShareCreate());
            return execs;
        }

        public static IList<RegressionExecution> WithDisableShare(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectDisableShare());
            return execs;
        }

        public static IList<RegressionExecution> WithShareCreate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectShareCreate());
            return execs;
        }

        public static IList<RegressionExecution> WithNoShare(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectNoShare());
            return execs;
        }

        public static IList<RegressionExecution> WithKeyedRange(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectKeyedRange());
            return execs;
        }

        public static IList<RegressionExecution> WithRange(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectRange(false, false));
            execs.Add(new EPLSubselectRange(true, true));
            return execs;
        }

        public static IList<RegressionExecution> WithKeyAndRange(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectKeyAndRange(false, false));
            execs.Add(new EPLSubselectKeyAndRange(true, true));
            return execs;
        }

        public static IList<RegressionExecution> WithConstantValue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectConstantValue(false, false));
            execs.Add(new EPLSubselectConstantValue(true, true));
            return execs;
        }

        private void RunAssertionConstantValue(RegressionEnvironment env)
        {
        }

        private class EPLSubselectConstantValue : RegressionExecution
        {
            private readonly bool indexShare;
            private readonly bool buildIndex;

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public EPLSubselectConstantValue(
                bool indexShare,
                bool buildIndex)
            {
                this.indexShare = indexShare;
                this.buildIndex = buildIndex;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var createEpl = "@public create window MyWindow#keepall as select * from SupportBean";
                if (indexShare) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);

                if (buildIndex) {
                    env.CompileDeploy("create index idx1 on MyWindow(TheString hash)", path);
                }

                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                // preload
                for (var i = 0; i < 10000; i++) {
                    var bean = new SupportBean("E" + i, i);
                    bean.DoublePrimitive = i;
                    env.SendEventBean(bean);
                }

                // single-field compare
                var fields = "val".SplitCsv();
                var eplSingle =
                    "@name('s0') select (select IntPrimitive from MyWindow where TheString = 'E9734') as val from SupportBeanRange sbr";
                env.CompileDeploy(eplSingle, path).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "", -1, -1));
                    env.AssertPropsNew("s0", fields, new object[] { 9734 });
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);
                env.UndeployModuleContaining("s0");

                // two-field compare
                var eplTwoHash =
                    "@name('s1') select (select IntPrimitive from MyWindow where TheString = 'E9736' and IntPrimitive = 9736) as val from SupportBeanRange sbr";
                env.CompileDeploy(eplTwoHash, path).AddListener("s1");

                startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "", -1, -1));
                    env.AssertPropsNew("s1", fields, new object[] { 9736 });
                }

                delta = PerformanceObserver.MilliTime - startTime;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);
                env.UndeployModuleContaining("s1");

                // range compare single
                if (buildIndex) {
                    env.CompileDeploy("create index idx2 on MyWindow(IntPrimitive btree)", path);
                }

                var eplSingleBTree =
                    "@name('s2') select (select IntPrimitive from MyWindow where IntPrimitive between 9735 and 9735) as val from SupportBeanRange sbr";
                env.CompileDeploy(eplSingleBTree, path).AddListener("s2");

                startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "", -1, -1));
                    env.AssertPropsNew("s2", fields, new object[] { 9735 });
                }

                delta = PerformanceObserver.MilliTime - startTime;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);
                env.UndeployModuleContaining("s2");

                // range compare composite
                var eplComposite =
                    "@name('s3') select (select IntPrimitive from MyWindow where TheString = 'E9738' and IntPrimitive between 9738 and 9738) as val from SupportBeanRange sbr";
                env.CompileDeploy(eplComposite, path).AddListener("s3");

                startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "", -1, -1));
                    env.AssertPropsNew("s3", fields, new object[] { 9738 });
                }

                delta = PerformanceObserver.MilliTime - startTime;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);
                env.UndeployModuleContaining("s3");

                // destroy all
                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "indexShare=" +
                       indexShare +
                       ", buildIndex=" +
                       buildIndex +
                       '}';
            }
        }

        private class EPLSubselectKeyAndRange : RegressionExecution
        {
            private readonly bool indexShare;
            private readonly bool buildIndex;

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public EPLSubselectKeyAndRange(
                bool indexShare,
                bool buildIndex)
            {
                this.indexShare = indexShare;
                this.buildIndex = buildIndex;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var createEpl = "@public create window MyWindow#keepall as select * from SupportBean";
                if (indexShare) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);

                if (buildIndex) {
                    env.CompileDeploy("create index idx1 on MyWindow(TheString hash, IntPrimitive btree)", path);
                }

                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                // preload
                for (var i = 0; i < 10000; i++) {
                    var theString = i < 5000 ? "A" : "B";
                    env.SendEventBean(new SupportBean(theString, i));
                }

                var fields = "cols.mini,cols.maxi".SplitCsv();
                var queryEpl =
                    "@name('s0') select (select min(IntPrimitive) as mini, max(IntPrimitive) as maxi from MyWindow where TheString = sbr.key and IntPrimitive between sbr.rangeStart and sbr.rangeEnd) as cols from SupportBeanRange sbr";
                env.CompileDeploy(queryEpl, path).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R1", "A", 300, 312));
                    env.AssertPropsNew("s0", fields, new object[] { 300, 312 });
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "indexShare=" +
                       indexShare +
                       ", buildIndex=" +
                       buildIndex +
                       '}';
            }
        }

        private class EPLSubselectRange : RegressionExecution
        {
            private readonly bool indexShare;
            private readonly bool buildIndex;

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public EPLSubselectRange(
                bool indexShare,
                bool buildIndex)
            {
                this.indexShare = indexShare;
                this.buildIndex = buildIndex;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var createEpl = "@public create window MyWindow#keepall as select * from SupportBean";
                if (indexShare) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);

                if (buildIndex) {
                    env.CompileDeploy("create index idx1 on MyWindow(IntPrimitive btree)", path);
                }

                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                // preload
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean("E1", i));
                }

                var fields = "cols.mini,cols.maxi".SplitCsv();
                var queryEpl =
                    "@name('s0') select (select min(IntPrimitive) as mini, max(IntPrimitive) as maxi from MyWindow where IntPrimitive between sbr.rangeStart and sbr.rangeEnd) as cols from SupportBeanRange sbr";
                env.CompileDeploy(queryEpl, path).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R1", "K", 300, 312));
                    env.AssertPropsNew("s0", fields, new object[] { 300, 312 });
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "indexShare=" +
                       indexShare +
                       ", buildIndex=" +
                       buildIndex +
                       '}';
            }
        }

        private class EPLSubselectKeyedRange : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var createEpl = "@public create window MyWindow#keepall as select * from SupportBean";
                env.CompileDeploy(createEpl, path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                // preload
                for (var i = 0; i < 10000; i++) {
                    var key = i < 5000 ? "A" : "B";
                    env.SendEventBean(new SupportBean(key, i));
                }

                var fields = "cols.mini,cols.maxi".SplitCsv();
                var queryEpl =
                    "@name('s0') select (select min(IntPrimitive) as mini, max(IntPrimitive) as maxi from MyWindow " +
                    "where TheString = sbr.key and IntPrimitive between sbr.rangeStart and sbr.rangeEnd) as cols from SupportBeanRange sbr";
                env.CompileDeploy(queryEpl, path).AddListener("s0").Milestone(0);

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 500; i++) {
                    env.SendEventBean(new SupportBeanRange("R1", "A", 299, 313));
                    env.AssertPropsNew("s0", fields, new object[] { 299, 313 });

                    env.SendEventBean(new SupportBeanRange("R2", "B", 7500, 7510));
                    env.AssertPropsNew("s0", fields, new object[] { 7500, 7510 });
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);

                env.UndeployAll();
            }
        }

        private class EPLSubselectNoShare : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                TryAssertion(env, false, false, false);
            }
        }

        private class EPLSubselectShareCreate : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                TryAssertion(env, true, false, true);
            }
        }

        private class EPLSubselectDisableShare : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                TryAssertion(env, true, true, false);
            }
        }

        private class EPLSubselectDisableShareCreate : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                TryAssertion(env, true, true, true);
            }
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            bool enableIndexShareCreate,
            bool disableIndexShareConsumer,
            bool createExplicitIndex)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public @buseventtype create schema EventSchema(e0 string, e1 int, e2 string)", path);

            var createEpl = "@public create window MyWindow#keepall as select * from SupportBean";
            if (enableIndexShareCreate) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }

            env.CompileDeploy(createEpl, path);
            env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

            if (createExplicitIndex) {
                env.CompileDeploy("create index MyIndex on MyWindow (TheString)", path);
            }

            var consumeEpl =
                "@name('s0') select e0, (select TheString from MyWindow where IntPrimitive = es.e1 and TheString = es.e2) as val from EventSchema as es";
            if (disableIndexShareConsumer) {
                consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
            }

            env.CompileDeploy(consumeEpl, path).AddListener("s0");

            var fields = "e0,val".SplitCsv();

            // test once
            env.SendEventBean(new SupportBean("WX", 10));
            SendEvent(env, "E1", 10, "WX");
            env.AssertPropsNew("s0", fields, new object[] { "E1", "WX" });

            // preload
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(new SupportBean("W" + i, i));
            }

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 5000; i++) {
                SendEvent(env, "E" + i, i, "W" + i);
                env.AssertPropsNew("s0", fields, new object[] { "E" + i, "W" + i });
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Assert.That(delta, Is.LessThan(500), "delta=" + delta);

            env.UndeployAll();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string e0,
            int e1,
            string e2)
        {
            IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("e0", e0);
            theEvent.Put("e1", e1);
            theEvent.Put("e2", e2);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent()) {
                env.SendEventObjectArray(theEvent.Values.ToArray(), "EventSchema");
            }
            else {
                env.SendEventMap(theEvent, "EventSchema");
            }
        }
    }
} // end of namespace