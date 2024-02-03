///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanInheritAndInterface
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOverridingSubclass(execs);
            WithImplementationClass(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithImplementationClass(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventBeanInheritAndInterfaceImplementationClass());
            return execs;
        }

        public static IList<RegressionExecution> WithOverridingSubclass(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventBeanInheritAndInterfaceOverridingSubclass());
            return execs;
        }

        internal class EventBeanInheritAndInterfaceOverridingSubclass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Val as value from SupportOverrideOne#length(10)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportOverrideOneA("valA", "valOne", "valBase"));
                env.AssertEqualsNew("s0", "value", "valA");

                env.SendEventBean(new SupportOverrideBase("x"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportOverrideOneB("valB", "valTwo", "valBase2"));
                env.AssertEqualsNew("s0", "value", "valB");

                env.SendEventBean(new SupportOverrideOne("valThree", "valBase3"));
                env.AssertEqualsNew("s0", "value", "valThree");

                env.UndeployAll();
            }
        }

        internal class EventBeanInheritAndInterfaceImplementationClass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] epls = {
                    "select BaseAB from ISupportBaseAB#length(10)",
                    "select BaseAB, A from ISupportA#length(10)",
                    "select BaseAB, B from ISupportB#length(10)",
                    "select C from ISupportC#length(10)",
                    "select BaseAB, A, G from ISupportAImplSuperG#length(10)",
                    "select BaseAB, A, B, G, C from ISupportAImplSuperGImplPlus#length(10)"
                };

                string[][] expected = {
                    new[] { "BaseAB" },
                    new[] { "BaseAB", "A" },
                    new[] { "BaseAB", "B" },
                    new[] { "C" },
                    new[] { "BaseAB", "A", "G" },
                    new[] { "BaseAB", "A", "B", "G", "C" }
                };

                var stmts = new EPStatement[epls.Length];
                var listeners = new SupportUpdateListener[epls.Length];
                for (var i = 0; i < epls.Length; i++) {
                    var name = $"@name('stmt_{i}')";
                    env.CompileDeploy(name + epls[i]);
                    stmts[i] = env.Statement("stmt_" + i);
                    listeners[i] = new SupportUpdateListener();
                    stmts[i].AddListener(listeners[i]);
                }

                env.SendEventBean(new ISupportAImplSuperGImplPlus("G", "A", "BaseAB", "B", "C"));
                for (var i = 0; i < listeners.Length; i++) {
                    ClassicAssert.IsTrue(listeners[i].IsInvoked);
                    var theEvent = listeners[i].GetAndResetLastNewData()[0];

                    for (var j = 0; j < expected[i].Length; j++) {
                        ClassicAssert.IsTrue(
                            theEvent.EventType.IsProperty(expected[i][j]),
                            "failed property valid check for stmt=" + epls[i]);
                        ClassicAssert.AreEqual(
                            expected[i][j],
                            theEvent.Get(expected[i][j]),
                            "failed property check for stmt=" + epls[i]);
                    }
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }
    }
} // end of namespace