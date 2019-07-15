///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreNewInstance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> executions = new List<RegressionExecution>();
            executions.Add(new ExecCoreNewInstanceKeyword(true));
            executions.Add(new ExecCoreNewInstanceKeyword(false));
            executions.Add(new ExecCoreNewInstanceStreamAlias());
            executions.Add(new ExecCoreNewInstanceInvalid());
            return executions;
        }

        internal class ExecCoreNewInstanceInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try variable
                env.CompileDeploy(
                    "create constant variable java.util.concurrent.atomic.AtomicLong cnt = new java.util.concurrent.atomic.AtomicLong(1)");

                // try shallow invalid cases
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select new Dummy() from SupportBean",
                    "Failed to valIdate select-clause expression 'new Dummy()': Failed to resolve new-operator class name 'Dummy'");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select new SupportPrivateCtor() from SupportBean",
                    "Failed to valIdate select-clause expression 'new SupportPrivateCtor()': Failed to find a suitable constructor for class ");

                env.UndeployAll();
            }
        }

        internal class ExecCoreNewInstanceStreamAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select new SupportObjectCtor(sb) as c0 from SupportBean as sb";
                env.CompileDeploy(epl).AddListener("s0");

                var sb = new SupportBean();
                env.SendEventBean(sb);
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(sb, ((SupportObjectCtor) @event.Get("c0")).Object);

                env.UndeployAll();
            }
        }

        internal class ExecCoreNewInstanceKeyword : RegressionExecution
        {
            private readonly bool soda;

            public ExecCoreNewInstanceKeyword(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "new SupportBean(\"A\",IntPrimitive) as c0, " +
                          "new SupportBean(\"B\",IntPrimitive+10), " +
                          "new SupportBean() as c2, " +
                          "new SupportBean(\"ABC\",0).getTheString() as c3 " +
                          "from SupportBean";
                env.CompileDeploy(soda, epl).AddListener("s0");
                object[][] expectedAggType = {
                    new object[] {"c0", typeof(SupportBean)},
                    new object[] {"new SupportBean(\"B\",IntPrimitive+10)", typeof(SupportBean)}
                };
                SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedAggType,
                    env.Statement("s0").EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE);

                env.SendEventBean(new SupportBean("E1", 10));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertSupportBean(
                    @event.Get("c0"),
                    new object[] {"A", 10});
                AssertSupportBean(
                    ((IDictionary<string, object>) @event.Underlying).Get("new SupportBean(\"B\",IntPrimitive+10)"),
                    new object[] {"B", 20});
                AssertSupportBean(
                    @event.Get("c2"),
                    new object[] {null, 0});
                Assert.AreEqual("ABC", @event.Get("c3"));

                env.UndeployAll();
            }

            private void AssertSupportBean(
                object bean,
                object[] objects)
            {
                var b = (SupportBean) bean;
                Assert.AreEqual(objects[0], b.TheString);
                Assert.AreEqual(objects[1], b.IntPrimitive);
            }
        }
    }
} // end of namespace