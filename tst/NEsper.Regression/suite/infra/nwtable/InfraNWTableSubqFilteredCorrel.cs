///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubqFilteredCorrel
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            // named window tests
            execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, false, false, false)); // no-share
            execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, false, false, true)); // no-share create
            execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, true, false, false)); // share no-create
            execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, true, true, false)); // disable share no-create
            execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, true, true, true)); // disable share create

            // table tests
            execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(false, false, false, false)); // table no-create
            execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(false, false, false, true)); // table create
            return execs;
        }

        internal class InfraNWTableSubqFilteredCorrelAssertion : RegressionExecution
        {
            private readonly bool createExplicitIndex;
            private readonly bool disableIndexShareConsumer;
            private readonly bool enableIndexShareCreate;
            private readonly bool namedWindow;

            public InfraNWTableSubqFilteredCorrelAssertion(
                bool namedWindow,
                bool enableIndexShareCreate,
                bool disableIndexShareConsumer,
                bool createExplicitIndex)
            {
                this.namedWindow = namedWindow;
                this.enableIndexShareCreate = enableIndexShareCreate;
                this.disableIndexShareConsumer = disableIndexShareConsumer;
                this.createExplicitIndex = createExplicitIndex;
            }

            public void Run(RegressionEnvironment env)
            {
                var createEpl = namedWindow
                    ? "create window MyInfra#keepall as select * from SupportBean"
                    : "create table MyInfra (TheString string primary key, IntPrimitive int primary key)";
                if (enableIndexShareCreate) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl);
                env.CompileDeploy("insert into MyInfra select TheString, IntPrimitive from SupportBean");

                if (createExplicitIndex) {
                    env.CompileDeploy("@Name('index') create index MyIndex on MyInfra(TheString)");
                }

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", -2));

                var consumeEpl =
                    "@Name('consume') select (select IntPrimitive from MyInfra(IntPrimitive<0) sw where S0.P00=sw.TheString) as val from S0 s0";
                if (disableIndexShareConsumer) {
                    consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
                }

                env.CompileDeploy(consumeEpl).AddListener("consume");

                env.SendEventBean(new SupportBean_S0(10, "E1"));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(new SupportBean_S0(20, "E2"));
                Assert.AreEqual(-2, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(new SupportBean("E3", -3));
                env.SendEventBean(new SupportBean("E4", 4));

                env.SendEventBean(new SupportBean_S0(-3, "E3"));
                Assert.AreEqual(-3, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.SendEventBean(new SupportBean_S0(20, "E4"));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.UndeployModuleContaining("consume");
                env.UndeployAll();
            }
        }
    }
} // end of namespace