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
using NUnit.Framework.Legacy;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternStartStop
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithStartStopOne(execs);
            WithAddRemoveListener(execs);
            WithStartStopTwo(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithStartStopTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternStartStopTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithAddRemoveListener(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternAddRemoveListener());
            return execs;
        }

        public static IList<RegressionExecution> WithStartStopOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternStartStopOne());
            return execs;
        }

        private class PatternStartStopTwo : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.OBSERVEROPS);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select * from pattern [every(a=SupportBean or b=SupportBeanComplexProps)]";
                var compiled = env.Compile(stmtText);
                env.Deploy(compiled).AddListener("s0");

                for (var i = 0; i < 100; i++) {
                    SendAndAssert(env);

                    var listener = env.Listener("s0");
                    listener.Reset();
                    env.UndeployModuleContaining("s0");

                    env.SendEventBean(new SupportBean());
                    env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                    ClassicAssert.IsFalse(listener.IsInvoked);

                    env.Deploy(compiled).AddListener("s0");
                }

                env.UndeployAll();
            }
        }

        private class PatternStartStopOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') @IterableUnbound select * from pattern[every tag=SupportBean]";
                var compiled = env.Compile(epl);
                env.Deploy(compiled).AddListener("s0");
                var stmt = env.Statement("s0");
                ClassicAssert.AreEqual(StatementType.SELECT, stmt.GetProperty(StatementProperty.STATEMENTTYPE));
                ClassicAssert.IsNull(stmt.GetProperty(StatementProperty.CONTEXTNAME));
                ClassicAssert.IsNull(stmt.GetProperty(StatementProperty.CONTEXTDEPLOYMENTID));

                // Pattern started when created
                ClassicAssert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());
                var safe = env.Statement("s0").GetSafeEnumerator();
                ClassicAssert.IsFalse(safe.MoveNext());
                safe.Dispose();

                // Stop pattern
                var listener = env.Listener("s0");
                listener.Reset();

                env.UndeployModuleContaining("s0");
                SendEvent(env);
                ClassicAssert.IsFalse(listener.IsInvoked);

                // Start pattern
                env.Deploy(compiled).AddListener("s0");
                ClassicAssert.IsFalse(env.GetEnumerator("s0").MoveNext());

                // Send event
                var theEvent = SendEvent(env);
                ClassicAssert.AreSame(theEvent, env.GetEnumerator("s0").Advance().Get("tag"));
                safe = env.Statement("s0").GetSafeEnumerator();
                ClassicAssert.AreSame(theEvent, safe.Advance().Get("tag"));
                safe.Dispose();

                // Stop pattern
                listener = env.Listener("s0");
                listener.Reset();
                stmt = env.Statement("s0");
                env.UndeployModuleContaining("s0");
                SendEvent(env);
                try {
                    stmt.GetEnumerator();
                }
                catch (IllegalStateException ex) {
                    ClassicAssert.AreEqual("Statement has already been undeployed", ex.Message);
                }

                ClassicAssert.IsFalse(listener.IsInvoked);

                // Start again, iterator is zero
                env.Deploy(compiled);
                ClassicAssert.IsFalse(env.GetEnumerator("s0").MoveNext());
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class PatternAddRemoveListener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') @IterableUnbound select * from pattern[every tag=SupportBean]";
                env.CompileDeploy(epl);

                // Pattern started when created

                // Add listener
                var listener = new SupportUpdateListener();
                env.Statement("s0").AddListener(listener);
                ClassicAssert.IsNull(env.Listener("s0").LastNewData);
                ClassicAssert.IsFalse(env.GetEnumerator("s0").MoveNext());

                // Send event
                var theEvent = SendEvent(env);
                ClassicAssert.AreEqual(theEvent, listener.GetAndResetLastNewData()[0].Get("tag"));
                ClassicAssert.AreSame(theEvent, env.Statement("s0").First().Get("tag"));

                // Remove listener
                env.Statement("s0").RemoveListener(listener);
                theEvent = SendEvent(env);
                ClassicAssert.AreSame(theEvent, env.GetEnumerator("s0").Advance().Get("tag"));
                ClassicAssert.IsNull(listener.LastNewData);

                // Add listener back
                env.Statement("s0").AddListener(listener);
                theEvent = SendEvent(env);
                ClassicAssert.AreSame(theEvent, env.GetEnumerator("s0").Advance().Get("tag"));
                ClassicAssert.AreEqual(theEvent, listener.GetAndResetLastNewData()[0].Get("tag"));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private static void SendAndAssert(RegressionEnvironment env)
        {
            for (var i = 0; i < 1000; i++) {
                object theEvent;
                if (i % 3 == 0) {
                    theEvent = new SupportBean();
                }
                else {
                    theEvent = SupportBeanComplexProps.MakeDefaultBean();
                }

                env.SendEventBean(theEvent);

                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        if (theEvent is SupportBean) {
                            ClassicAssert.AreSame(theEvent, eventBean.Get("a"));
                            ClassicAssert.IsNull(eventBean.Get("b"));
                        }
                        else {
                            ClassicAssert.AreSame(theEvent, eventBean.Get("b"));
                            ClassicAssert.IsNull(eventBean.Get("a"));
                        }
                    });
            }
        }

        private static SupportBean SendEvent(RegressionEnvironment env)
        {
            var theEvent = new SupportBean();
            env.SendEventBean(theEvent);
            return theEvent;
        }
    }
} // end of namespace