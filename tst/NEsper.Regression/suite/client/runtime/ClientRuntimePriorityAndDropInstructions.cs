///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimePriorityAndDropInstructions
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSchedulingPriority(execs);
            WithSchedulingDrop(execs);
            WithNamedWindowPriority(execs);
            WithNamedWindowDrop(execs);
            WithPriority(execs);
            WithAddRemoveStmts(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAddRemoveStmts(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeAddRemoveStmts());
            return execs;
        }

        public static IList<RegressionExecution> WithPriority(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimePriority());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowDrop(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeNamedWindowDrop());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowPriority(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeNamedWindowPriority());
            return execs;
        }

        public static IList<RegressionExecution> WithSchedulingDrop(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSchedulingDrop());
            return execs;
        }

        public static IList<RegressionExecution> WithSchedulingPriority(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSchedulingPriority());
            return execs;
        }

        [Priority(10)]
        [Drop]
        private class ClientRuntimeSchedulingPriority : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var listener = env.ListenerNew();
                env.CompileDeploy("@name('s1') @Priority(1) select 1 as prio from pattern [every timer:interval(10)]");
                env.CompileDeploy("@name('s3') @Priority(3) select 3 as prio from pattern [every timer:interval(10)]");
                env.CompileDeploy("@name('s2') @Priority(2) select 2 as prio from pattern [every timer:interval(10)]");
                env.CompileDeploy("@name('s4') @Priority(4) select 4 as prio from pattern [every timer:interval(10)]");
                env.Statement("s1").AddListener(listener);
                env.Statement("s2").AddListener(listener);
                env.Statement("s3").AddListener(listener);
                env.Statement("s4").AddListener(listener);

                SendTimer(10000, env);
                AssertPrio(listener, null, new int[] { 4, 3, 2, 1 });

                env.UndeployModuleContaining("s2");
                env.CompileDeploy("@name('s0') select 0 as prio from pattern [every timer:interval(10)]");
                env.Statement("s0").AddListener(listener);

                SendTimer(20000, env);
                AssertPrio(listener, null, new int[] { 4, 3, 1, 0 });

                env.CompileDeploy("@name('s2') @Priority(2) select 2 as prio from pattern [every timer:interval(10)]");
                env.Statement("s2").AddListener(listener);

                SendTimer(30000, env);
                AssertPrio(listener, null, new int[] { 4, 3, 2, 1, 0 });

                env.CompileDeploy("@name('s5') @Priority(3) select 3 as prio from pattern [every timer:interval(10)]");
                env.Statement("s5").AddListener(listener);

                SendTimer(40000, env);
                AssertPrio(listener, null, new int[] { 4, 3, 3, 2, 1, 0 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSchedulingDrop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var listener = env.ListenerNew();
                env.CompileDeploy("@name('s1') @Drop select 1 as prio from pattern [every timer:interval(10)]");
                env.Statement("s1").AddListener(listener);

                env.CompileDeploy("@name('s3') @Priority(2) select 3 as prio from pattern [every timer:interval(10)]");
                env.Statement("s3").AddListener(listener);

                env.CompileDeploy("@name('s2') select 2 as prio from pattern [every timer:interval(10)]");
                env.Statement("s2").AddListener(listener);

                SendTimer(10000, env);
                AssertPrio(listener, null, new int[] { 3, 1 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeNamedWindowPriority : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string stmtText;
                var path = new RegressionPath();
                var listener = env.ListenerNew();

                stmtText = "@public create window MyWindow#lastevent as select * from SupportBean";
                env.CompileDeploy(stmtText, path);

                stmtText = "insert into MyWindow select * from SupportBean";
                env.CompileDeploy(stmtText, path);

                stmtText =
                    "@name('s1') @Priority(1) on MyWindow e select e.TheString as TheString, 1 as prio from MyWindow";
                env.CompileDeploy(stmtText, path);
                env.Statement("s1").AddListener(listener);

                stmtText =
                    "@name('s3') @Priority(3) on MyWindow e select e.TheString as TheString, 3 as prio from MyWindow";
                env.CompileDeploy(stmtText, path);
                env.Statement("s3").AddListener(listener);

                stmtText =
                    "@name('s2') @Priority(2) on MyWindow e select e.TheString as TheString, 2 as prio from MyWindow";
                env.CompileDeploy(stmtText, path);
                env.Statement("s2").AddListener(listener);

                stmtText =
                    "@name('s4') @Priority(4) on MyWindow e select e.TheString as TheString, 4 as prio from MyWindow";
                env.CompileDeploy(stmtText, path);
                env.Statement("s4").AddListener(listener);

                env.SendEventBean(new SupportBean("E1", 0));
                AssertPrio(listener, "E1", new int[] { 4, 3, 2, 1 });

                env.UndeployModuleContaining("s2");
                env.CompileDeploy(
                    "@name('s0') on MyWindow e select e.TheString as TheString, 0 as prio from MyWindow",
                    path);
                env.Statement("s0").AddListener(listener);

                env.SendEventBean(new SupportBean("E2", 0));
                AssertPrio(listener, "E2", new int[] { 4, 3, 1, 0 });

                stmtText =
                    "@name('s2') @Priority(2) on MyWindow e select e.TheString as TheString, 2 as prio from MyWindow";
                env.CompileDeploy(stmtText, path);
                env.Statement("s2").AddListener(listener);

                env.SendEventBean(new SupportBean("E3", 0));
                AssertPrio(listener, "E3", new int[] { 4, 3, 2, 1, 0 });

                stmtText =
                    "@name('sx') @Priority(3) on MyWindow e select e.TheString as TheString, 3 as prio from MyWindow";
                env.CompileDeploy(stmtText, path);
                env.Statement("sx").AddListener(listener);

                env.SendEventBean(new SupportBean("E4", 0));
                AssertPrio(listener, "E4", new int[] { 4, 3, 3, 2, 1, 0 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeNamedWindowDrop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string stmtText;
                var path = new RegressionPath();
                var listener = env.ListenerNew();

                stmtText = "@public create window MyWindow#lastevent as select * from SupportBean";
                env.CompileDeploy(stmtText, path);

                stmtText = "insert into MyWindow select * from SupportBean";
                env.CompileDeploy(stmtText, path);

                stmtText = "@name('s2') @Drop on MyWindow e select e.TheString as TheString, 2 as prio from MyWindow";
                env.CompileDeploy(stmtText, path);
                env.Statement("s2").AddListener(listener);

                stmtText =
                    "@name('s3') @Priority(3) on MyWindow e select e.TheString as TheString, 3 as prio from MyWindow";
                env.CompileDeploy(stmtText, path);
                env.Statement("s3").AddListener(listener);

                stmtText = "@name('s4') on MyWindow e select e.TheString as TheString, 0 as prio from MyWindow";
                env.CompileDeploy(stmtText, path);
                env.Statement("s4").AddListener(listener);

                env.SendEventBean(new SupportBean("E1", 0));
                AssertPrio(listener, "E1", new int[] { 3, 2 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimePriority : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = env.ListenerNew();

                env.CompileDeploy("@name('s1') @Priority(1) select *, 1 as prio from SupportBean");
                env.Statement("s1").AddListener(listener);

                env.CompileDeploy("@name('s3') @Priority(3) select *, 3 as prio from SupportBean");
                env.Statement("s3").AddListener(listener);

                env.CompileDeploy("@name('s2') @Priority(2) select *, 2 as prio from SupportBean");
                env.Statement("s2").AddListener(listener);

                env.CompileDeploy("@name('s4') @Priority(4) select *, 4 as prio from SupportBean");
                env.Statement("s4").AddListener(listener);

                env.SendEventBean(new SupportBean("E1", 0));
                AssertPrio(listener, "E1", new int[] { 4, 3, 2, 1 });

                env.UndeployModuleContaining("s2");
                env.CompileDeploy("@name('s0') select *, 0 as prio from SupportBean");
                env.Statement("s0").AddListener(listener);

                env.SendEventBean(new SupportBean("E2", 0));
                AssertPrio(listener, "E2", new int[] { 4, 3, 1, 0 });

                env.CompileDeploy("@name('s2') @Priority(2) select *, 2 as prio from SupportBean");
                env.Statement("s2").AddListener(listener);

                env.SendEventBean(new SupportBean("E3", 0));
                AssertPrio(listener, "E3", new int[] { 4, 3, 2, 1, 0 });

                env.CompileDeploy("@name('sx') @Priority(3) select *, 3 as prio from SupportBean");
                env.Statement("sx").AddListener(listener);

                env.SendEventBean(new SupportBean("E4", 0));
                AssertPrio(listener, "E4", new int[] { 4, 3, 3, 2, 1, 0 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeAddRemoveStmts : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtSelectText = "@name('s0') insert into ABCStream select * from SupportBean";
                env.CompileDeploy(stmtSelectText, path).AddListener("s0");

                var stmtOneText = "@name('l0') @Drop select * from SupportBean where IntPrimitive = 1";
                env.CompileDeploy(stmtOneText).AddListener("l0");

                var stmtTwoText = "@name('l1') @Drop select * from SupportBean where IntPrimitive = 2";
                env.CompileDeploy(stmtTwoText).AddListener("l1");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertListenerNotInvoked("s0");
                AssertReceivedSingle(env, "l0,l1", 0, "E1");

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertListenerNotInvoked("s0");
                AssertReceivedSingle(env, "l0,l1", 1, "E2");

                env.SendEventBean(new SupportBean("E3", 1));
                env.AssertListenerNotInvoked("s0");
                AssertReceivedSingle(env, "l0,l1", 0, "E3");

                env.SendEventBean(new SupportBean("E4", 3));
                env.AssertEqualsNew("s0", "TheString", "E4");
                AssertReceivedNone(env, "l0,l1");

                var stmtThreeText = "@name('l2') @Drop select * from SupportBean where IntPrimitive = 3";
                env.CompileDeploy(stmtThreeText).AddListener("l2");

                env.SendEventBean(new SupportBean("E5", 3));
                env.AssertListenerNotInvoked("s0");
                AssertReceivedSingle(env, "l0,l1,l2", 2, "E5");

                env.SendEventBean(new SupportBean("E6", 1));
                env.AssertListenerNotInvoked("s0");
                AssertReceivedSingle(env, "l0,l1,l2", 0, "E6");

                env.UndeployModuleContaining("l0");
                env.SendEventBean(new SupportBean("E7", 1));
                env.AssertEqualsNew("s0", "TheString", "E7");
                AssertReceivedNone(env, "l1,l2");

                var stmtSelectTextTwo = "@name('s1') @Priority(50) select * from SupportBean";
                env.CompileDeploy(stmtSelectTextTwo).AddListener("s1");

                env.SendEventBean(new SupportBean("E8", 1));
                env.AssertEqualsNew("s0", "TheString", "E8");
                env.AssertEqualsNew("s1", "TheString", "E8");
                AssertReceivedNone(env, "l1,l2");

                env.SendEventBean(new SupportBean("E9", 2));
                env.AssertListenerNotInvoked("s0");
                AssertReceivedSingle(env, "l1,l2", 0, "E9");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private static void AssertReceivedSingle(
            RegressionEnvironment env,
            string namesCSV,
            int index,
            string stringValue)
        {
            var names = namesCSV.SplitCsv();
            for (var i = 0; i < names.Length; i++) {
                if (i == index) {
                    continue;
                }

                ClassicAssert.IsFalse(env.Listener(names[i]).IsInvoked);
            }

            ClassicAssert.AreEqual(stringValue, env.Listener(names[index]).AssertOneGetNewAndReset().Get("TheString"));
        }

        private static void AssertPrio(
            SupportListener listener,
            string theString,
            int[] prioValues)
        {
            var events = listener.NewDataListFlattened;
            ClassicAssert.AreEqual(prioValues.Length, events.Length);
            for (var i = 0; i < prioValues.Length; i++) {
                ClassicAssert.AreEqual(prioValues[i], events[i].Get("prio"));
                if (theString != null) {
                    ClassicAssert.AreEqual(theString, events[i].Get("TheString"));
                }
            }

            listener.Reset();
        }

        private static void AssertReceivedNone(
            RegressionEnvironment env,
            string namesCSV)
        {
            var names = namesCSV.SplitCsv();
            for (var i = 0; i < names.Length; i++) {
                env.AssertListenerNotInvoked(names[i]);
            }
        }

        private static void SendTimer(
            long time,
            RegressionEnvironment env)
        {
            env.AdvanceTime(time);
        }
    }
} // end of namespace