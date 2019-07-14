///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableOnSelectWDelete : IndexBackingTableInfo
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraNWTableOnSelectWDeleteAssertion(true));
            execs.Add(new InfraNWTableOnSelectWDeleteAssertion(false));
            return execs;
        }

        internal class InfraNWTableOnSelectWDeleteAssertion : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraNWTableOnSelectWDeleteAssertion(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fieldsWin = "theString,intPrimitive".SplitCsv();
                var fieldsSelect = "c0".SplitCsv();
                var path = new RegressionPath();

                var eplCreate = namedWindow
                    ? "@Name('create') create window MyInfra#keepall as SupportBean"
                    : "@Name('create') create table MyInfra (theString string primary key, IntPrimitive int primary key)";
                env.CompileDeploy(eplCreate, path);

                env.CompileDeploy("insert into MyInfra select TheString, IntPrimitive from SupportBean", path);

                var eplSelectDelete = "@Name('s0') on SupportBean_S0 as s0 " +
                                      "select and delete window(win.*).aggregate(0,(result,value) => result+value.IntPrimitive) as c0 " +
                                      "from MyInfra as win where s0.p00=win.TheString";
                env.CompileDeploy(eplSelectDelete, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                if (namedWindow) {
                    EPAssertionUtil.AssertPropsPerRow(
                        env.GetEnumerator("create"),
                        fieldsWin,
                        new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});
                }
                else {
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        env.GetEnumerator("create"),
                        fieldsWin,
                        new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});
                }

                // select and delete bean E1
                env.SendEventBean(new SupportBean_S0(100, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {1});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fieldsWin,
                    new[] {new object[] {"E2", 2}});

                env.Milestone(0);

                // add some E2 events
                env.SendEventBean(new SupportBean("E2", 3));
                env.SendEventBean(new SupportBean("E2", 4));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fieldsWin,
                    new[] {new object[] {"E2", 2}, new object[] {"E2", 3}, new object[] {"E2", 4}});

                // select and delete beans E2
                env.SendEventBean(new SupportBean_S0(101, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {2 + 3 + 4});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("create"), fieldsWin, new object[0][]);

                // test SODA
                env.EplToModelCompileDeploy(eplSelectDelete, path);

                env.UndeployAll();
            }
        }
    }
} // end of namespace