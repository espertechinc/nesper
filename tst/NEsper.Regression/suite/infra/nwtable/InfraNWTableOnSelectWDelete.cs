///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableOnSelectWDelete : IndexBackingTableInfo
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withn(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNWTableOnSelectWDeleteAssertion(true));
            execs.Add(new InfraNWTableOnSelectWDeleteAssertion(false));
            return execs;
        }

        private class InfraNWTableOnSelectWDeleteAssertion : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraNWTableOnSelectWDeleteAssertion(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fieldsWin = "TheString,IntPrimitive".SplitCsv();
                var fieldsSelect = "c0".SplitCsv();
                var path = new RegressionPath();

                var eplCreate = namedWindow
                    ? "@name('create') @public create window MyInfra#keepall as SupportBean"
                    : "@name('create') @public create table MyInfra (TheString string primary key, IntPrimitive int primary key)";
                env.CompileDeploy(eplCreate, path);

                env.CompileDeploy("insert into MyInfra select TheString, IntPrimitive from SupportBean", path);

                var eplSelectDelete = "@name('s0') on SupportBean_S0 as s0 " +
                                      "select and delete window(win.*).aggregate(0,(result,value) => result+value.IntPrimitive) as c0 " +
                                      "from MyInfra as win where s0.P00=win.TheString";
                env.CompileDeploy(eplSelectDelete, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                if (namedWindow) {
                    env.AssertPropsPerRowIterator(
                        "create",
                        fieldsWin,
                        new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
                }
                else {
                    env.AssertPropsPerRowIteratorAnyOrder(
                        "create",
                        fieldsWin,
                        new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
                }

                // select and delete bean E1
                env.SendEventBean(new SupportBean_S0(100, "E1"));
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 1 });
                env.AssertPropsPerRowIteratorAnyOrder("create", fieldsWin, new object[][] { new object[] { "E2", 2 } });

                env.Milestone(0);

                // add some E2 events
                env.SendEventBean(new SupportBean("E2", 3));
                env.SendEventBean(new SupportBean("E2", 4));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fieldsWin,
                    new object[][] { new object[] { "E2", 2 }, new object[] { "E2", 3 }, new object[] { "E2", 4 } });

                // select and delete beans E2
                env.SendEventBean(new SupportBean_S0(101, "E2"));
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 2 + 3 + 4 });
                env.AssertPropsPerRowIteratorAnyOrder("create", fieldsWin, Array.Empty<object[]>());

                // test SODA
                env.EplToModelCompileDeploy(eplSelectDelete, path);

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }
    }
} // end of namespace