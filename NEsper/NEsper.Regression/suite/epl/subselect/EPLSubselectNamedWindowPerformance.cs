///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectNamedWindowPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSubselectConstantValue(false, false));
            execs.Add(new EPLSubselectConstantValue(true, true));

            execs.Add(new EPLSubselectKeyAndRange(false, false));
            execs.Add(new EPLSubselectKeyAndRange(true, true));

            execs.Add(new EPLSubselectRange(false, false));
            execs.Add(new EPLSubselectRange(true, true));

            execs.Add(new EPLSubselectKeyedRange());
            execs.Add(new EPLSubselectNoShare());

            execs.Add(new EPLSubselectShareCreate());
            execs.Add(new EPLSubselectDisableShare());
            execs.Add(new EPLSubselectDisableShareCreate());
            return execs;
        }

        private void RunAssertionConstantValue(RegressionEnvironment env)
        {
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            bool enableIndexShareCreate,
            bool disableIndexShareConsumer,
            bool createExplicitIndex)
        {
            var path = new RegressionPath();
            env.CompileDeployWBusPublicType("create schema EventSchema(e0 string, e1 int, e2 string)", path);

            var createEpl = "create window MyWindow#keepall as select * from SupportBean";
            if (enableIndexShareCreate) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }

            env.CompileDeploy(createEpl, path);
            env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

            if (createExplicitIndex) {
                env.CompileDeploy("create index MyIndex on MyWindow (TheString)", path);
            }

            var consumeEpl =
                "@Name('s0') select e0, (select TheString from MyWindow where IntPrimitive = es.e1 and TheString = es.e2) as val from EventSchema as es";
            if (disableIndexShareConsumer) {
                consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
            }

            env.CompileDeploy(consumeEpl, path).AddListener("s0");

            var fields = "e0,val".SplitCsv();

            // test once
            env.SendEventBean(new SupportBean("WX", 10));
            SendEvent(env, "E1", 10, "WX");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", "WX"});

            // preload
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(new SupportBean("W" + i, i));
            }

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 5000; i++) {
                SendEvent(env, "E" + i, i, "W" + i);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E" + i, "W" + i});
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);

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

        internal class EPLSubselectConstantValue : RegressionExecution
        {
            private readonly bool buildIndex;
            private readonly bool indexShare;

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
                var createEpl = "create window MyWindow#keepall as select * from SupportBean";
                if (indexShare) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);

                if (buildIndex) {
                    env.CompileDeploy("create index Idx1 on MyWindow(TheString hash)", path);
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
                    "@Name('s0') select (select IntPrimitive from MyWindow where TheString = 'E9734') as val from SupportBeanRange sbr";
                env.CompileDeploy(eplSingle, path).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "", -1, -1));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {9734});
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.IsTrue(delta < 500, "delta=" + delta);
                env.UndeployModuleContaining("s0");

                // two-field compare
                var eplTwoHash =
                    "@Name('s1') select (select IntPrimitive from MyWindow where TheString = 'E9736' and IntPrimitive = 9736) as val from SupportBeanRange sbr";
                env.CompileDeploy(eplTwoHash, path).AddListener("s1");

                startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "", -1, -1));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s1").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {9736});
                }

                delta = PerformanceObserver.MilliTime - startTime;
                Assert.IsTrue(delta < 500, "delta=" + delta);
                env.UndeployModuleContaining("s1");

                // range compare single
                if (buildIndex) {
                    env.CompileDeploy("create index Idx2 on MyWindow(IntPrimitive btree)", path);
                }

                var eplSingleBTree =
                    "@Name('s2') select (select IntPrimitive from MyWindow where IntPrimitive between 9735 and 9735) as val from SupportBeanRange sbr";
                env.CompileDeploy(eplSingleBTree, path).AddListener("s2");

                startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "", -1, -1));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s2").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {9735});
                }

                delta = PerformanceObserver.MilliTime - startTime;
                Assert.IsTrue(delta < 500, "delta=" + delta);
                env.UndeployModuleContaining("s2");

                // range compare composite
                var eplComposite =
                    "@Name('s3') select (select IntPrimitive from MyWindow where TheString = 'E9738' and IntPrimitive between 9738 and 9738) as val from SupportBeanRange sbr";
                env.CompileDeploy(eplComposite, path).AddListener("s3");

                startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "", -1, -1));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s3").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {9738});
                }

                delta = PerformanceObserver.MilliTime - startTime;
                Assert.IsTrue(delta < 500, "delta=" + delta);
                env.UndeployModuleContaining("s3");

                // destroy all
                env.UndeployAll();
            }
        }

        internal class EPLSubselectKeyAndRange : RegressionExecution
        {
            private readonly bool buildIndex;
            private readonly bool indexShare;

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
                var createEpl = "create window MyWindow#keepall as select * from SupportBean";
                if (indexShare) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);

                if (buildIndex) {
                    env.CompileDeploy("create index Idx1 on MyWindow(TheString hash, IntPrimitive btree)", path);
                }

                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                // preload
                for (var i = 0; i < 10000; i++) {
                    var theString = i < 5000 ? "A" : "B";
                    env.SendEventBean(new SupportBean(theString, i));
                }

                var fields = "cols.mini,cols.maxi".SplitCsv();
                var queryEpl =
                    "@Name('s0') select (select min(IntPrimitive) as mini, max(IntPrimitive) as maxi from MyWindow where TheString = sbr.key and IntPrimitive between sbr.rangeStart and sbr.rangeEnd) as cols from SupportBeanRange sbr";
                env.CompileDeploy(queryEpl, path).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R1", "A", 300, 312));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {300, 312});
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.IsTrue(delta < 500, "delta=" + delta);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectRange : RegressionExecution
        {
            private readonly bool buildIndex;

            private readonly bool indexShare;

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
                var createEpl = "create window MyWindow#keepall as select * from SupportBean";
                if (indexShare) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);

                if (buildIndex) {
                    env.CompileDeploy("create index Idx1 on MyWindow(IntPrimitive btree)", path);
                }

                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                // preload
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean("E1", i));
                }

                var fields = "cols.mini,cols.maxi".SplitCsv();
                var queryEpl =
                    "@Name('s0') select (select min(IntPrimitive) as mini, max(IntPrimitive) as maxi from MyWindow where IntPrimitive between sbr.rangeStart and sbr.rangeEnd) as cols from SupportBeanRange sbr";
                env.CompileDeploy(queryEpl, path).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R1", "K", 300, 312));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {300, 312});
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.IsTrue(delta < 500, "delta=" + delta);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectKeyedRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var createEpl = "create window MyWindow#keepall as select * from SupportBean";
                env.CompileDeploy(createEpl, path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                // preload
                for (var i = 0; i < 10000; i++) {
                    var key = i < 5000 ? "A" : "B";
                    env.SendEventBean(new SupportBean(key, i));
                }

                var fields = "cols.mini,cols.maxi".SplitCsv();
                var queryEpl =
                    "@Name('s0') select (select min(IntPrimitive) as mini, max(IntPrimitive) as maxi from MyWindow " +
                    "where TheString = sbr.key and IntPrimitive between sbr.rangeStart and sbr.rangeEnd) as cols from SupportBeanRange sbr";
                env.CompileDeploy(queryEpl, path).AddListener("s0").Milestone(0);

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 500; i++) {
                    env.SendEventBean(new SupportBeanRange("R1", "A", 299, 313));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {299, 313});

                    env.SendEventBean(new SupportBeanRange("R2", "B", 7500, 7510));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {7500, 7510});
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.IsTrue(delta < 500, "delta=" + delta);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectNoShare : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertion(env, false, false, false);
            }
        }

        internal class EPLSubselectShareCreate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertion(env, true, false, true);
            }
        }

        internal class EPLSubselectDisableShare : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertion(env, true, true, false);
            }
        }

        internal class EPLSubselectDisableShareCreate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertion(env, true, true, true);
            }
        }
    }
} // end of namespace