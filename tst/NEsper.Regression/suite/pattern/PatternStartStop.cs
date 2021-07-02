///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternStartStop
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithStartStopOne(execs);
            WithAddRemoveListener(execs);
            WithStartStopTwo(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithStartStopTwo(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternStartStopTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithAddRemoveListener(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternAddRemoveListener());
            return execs;
        }

        public static IList<RegressionExecution> WithStartStopOne(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternStartStopOne());
            return execs;
        }

        private static void SendAndAssert(RegressionEnvironment env)
        {
            for (var i = 0; i < 1000; i++) {
                object theEvent = null;
                if (i % 3 == 0) {
                    theEvent = new SupportBean();
                }
                else {
                    theEvent = SupportBeanComplexProps.MakeDefaultBean();
                }

                env.SendEventBean(theEvent);

                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                if (theEvent is SupportBean) {
                    Assert.AreSame(theEvent, eventBean.Get("a"));
                    Assert.IsNull(eventBean.Get("b"));
                }
                else {
                    Assert.AreSame(theEvent, eventBean.Get("b"));
                    Assert.IsNull(eventBean.Get("a"));
                }
            }
        }

        private static SupportBean SendEvent(RegressionEnvironment env)
        {
            var theEvent = new SupportBean();
            env.SendEventBean(theEvent);
            return theEvent;
        }

        internal class PatternStartStopTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from pattern [every(a=SupportBean or b=SupportBeanComplexProps)]";
                var compiled = env.Compile(stmtText);
                env.Deploy(compiled).AddListener("s0");

                for (var i = 0; i < 100; i++) {
                    SendAndAssert(env);

                    var listener = env.Listener("s0");
                    listener.Reset();
                    env.UndeployModuleContaining("s0");

                    env.SendEventBean(new SupportBean());
                    env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                    Assert.IsFalse(listener.IsInvoked);

                    env.Deploy(compiled).AddListener("s0");
                }

                env.UndeployAll();
            }
        }

        internal class PatternStartStopOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') @IterableUnbound select * from pattern[every tag=SupportBean]";
                var compiled = env.Compile(epl);
                env.Deploy(compiled).AddListener("s0");
                Assert.AreEqual(StatementType.SELECT, env.Statement("s0").GetProperty(StatementProperty.STATEMENTTYPE));

                // Pattern started when created
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());
                using (var safe = env.Statement("s0").GetSafeEnumerator()) {
                    Assert.IsFalse(safe.MoveNext());
                }

                // Stop pattern
                var listener = env.Listener("s0");
                listener.Reset();

                env.UndeployModuleContaining("s0");
                SendEvent(env);
                Assert.IsFalse(listener.IsInvoked);

                // Start pattern
                env.Deploy(compiled).AddListener("s0");
                Assert.IsFalse(env.GetEnumerator("s0").MoveNext());

                // Send event
                var theEvent = SendEvent(env);
                Assert.AreSame(theEvent, env.GetEnumerator("s0").Advance().Get("tag"));
                using (var safe = env.Statement("s0").GetSafeEnumerator()) {
                    Assert.AreSame(theEvent, safe.Advance().Get("tag"));
                }

                // Stop pattern
                listener = env.Listener("s0");
                listener.Reset();
                var stmt = env.Statement("s0");
                env.UndeployModuleContaining("s0");
                SendEvent(env);
                try {
                    stmt.GetEnumerator();
                }
                catch (IllegalStateException ex) {
                    Assert.AreEqual("Statement has already been undeployed", ex.Message);
                }

                Assert.IsFalse(listener.IsInvoked);

                // Start again, iterator is zero
                env.Deploy(compiled);
                Assert.IsFalse(env.GetEnumerator("s0").MoveNext());
                env.UndeployAll();
            }
        }

        internal class PatternAddRemoveListener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') @IterableUnbound select * from pattern[every tag=SupportBean]";
                env.CompileDeploy(epl);

                // Pattern started when created

                // Add listener
                var listener = new SupportUpdateListener();
                env.Statement("s0").AddListener(listener);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsFalse(env.GetEnumerator("s0").MoveNext());

                // Send event
                var theEvent = SendEvent(env);
                Assert.AreEqual(theEvent, listener.GetAndResetLastNewData()[0].Get("tag"));
                Assert.AreSame(theEvent, env.Statement("s0").First().Get("tag"));

                // Remove listener
                env.Statement("s0").RemoveListener(listener);
                theEvent = SendEvent(env);
                Assert.AreSame(theEvent, env.GetEnumerator("s0").Advance().Get("tag"));
                Assert.IsNull(listener.LastNewData);

                // Add listener back
                env.Statement("s0").AddListener(listener);
                theEvent = SendEvent(env);
                Assert.AreSame(theEvent, env.GetEnumerator("s0").Advance().Get("tag"));
                Assert.AreEqual(theEvent, listener.GetAndResetLastNewData()[0].Get("tag"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace