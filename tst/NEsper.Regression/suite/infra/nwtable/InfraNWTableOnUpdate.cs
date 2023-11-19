///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableOnUpdate
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNWTableOnUpdateSceneOne(execs);
            WithUpdateOrderOfFields(execs);
            WithSubquerySelf(execs);
            WithSubqueryMultikeyWArray(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSubqueryMultikeyWArray(true));
            execs.Add(new InfraSubqueryMultikeyWArray(false));
            return execs;
        }

        public static IList<RegressionExecution> WithSubquerySelf(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSubquerySelf(true));
            execs.Add(new InfraSubquerySelf(false));
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateOrderOfFields(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraUpdateOrderOfFields(true));
            execs.Add(new InfraUpdateOrderOfFields(false));
            return execs;
        }

        public static IList<RegressionExecution> WithNWTableOnUpdateSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNWTableOnUpdateSceneOne(true));
            execs.Add(new InfraNWTableOnUpdateSceneOne(false));
            return execs;
        }

        private class InfraSubqueryMultikeyWArray : RegressionExecution
        {
            private bool namedWindow;

            public InfraSubqueryMultikeyWArray(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreate = namedWindow
                    ? "@name('create') @public create window MyInfra#keepall() as (value int)"
                    : "@name('create') @public create table MyInfra(value int)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");
                env.CompileExecuteFAFNoResult("insert into MyInfra select 0 as value", path);

                var epl =
                    "on SupportBean update MyInfra set value = (select sum(value) as c0 from SupportEventWithIntArray#keepall group by array)";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportEventWithIntArray("E1", new int[] { 1, 2 }, 10));
                env.SendEventBean(new SupportEventWithIntArray("E2", new int[] { 1, 2 }, 11));

                env.Milestone(0);
                AssertUpdate(env, 21);

                env.SendEventBean(new SupportEventWithIntArray("E3", new int[] { 1, 2 }, 12));
                AssertUpdate(env, 33);

                env.Milestone(1);

                env.SendEventBean(new SupportEventWithIntArray("E4", new int[] { 1 }, 13));
                AssertUpdate(env, null);

                env.UndeployAll();
            }

            private void AssertUpdate(
                RegressionEnvironment env,
                int? expected)
            {
                env.SendEventBean(new SupportBean());
                env.AssertIterator("create", iterator => Assert.AreEqual(expected, iterator.Advance().Get("value")));
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

        public class InfraNWTableOnUpdateSceneOne : RegressionExecution
        {
            private bool namedWindow;

            public InfraNWTableOnUpdateSceneOne(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString", "IntPrimitive" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate = namedWindow
                    ? "@name('create') @public create window MyInfra.win:keepall() as SupportBean"
                    : "@name('create') @public create table MyInfra(TheString string, IntPrimitive int primary key)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "@name('insert') insert into MyInfra select TheString, IntPrimitive from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(0);

                // populate some data
                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("B2", 2));

                // create onUpdate
                var stmtTextOnUpdate =
                    "@name('update') on SupportBean_S0 update MyInfra set TheString = P00 where IntPrimitive = Id";
                env.CompileDeploy(stmtTextOnUpdate, path).AddListener("update");
                env.AssertStatement(
                    "update",
                    statement => Assert.AreEqual(
                        StatementType.ON_UPDATE,
                        statement.GetProperty(StatementProperty.STATEMENTTYPE)));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(1, "X1"));
                env.AssertPropsIRPair("update", fields, new object[] { "X1", 1 }, new object[] { "A1", 1 });
                if (namedWindow) {
                    env.AssertPropsPerRowIterator(
                        "create",
                        fields,
                        new object[][] { new object[] { "B2", 2 }, new object[] { "X1", 1 } });
                }
                else {
                    env.AssertPropsPerRowIteratorAnyOrder(
                        "create",
                        fields,
                        new object[][] { new object[] { "B2", 2 }, new object[] { "X1", 1 } });
                }

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(2, "X2"));
                env.AssertPropsIRPair("update", fields, new object[] { "X2", 2 }, new object[] { "B2", 2 });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "X1", 1 }, new object[] { "X2", 2 } });

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "X1", 1 }, new object[] { "X2", 2 } });

                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("update");
                env.UndeployModuleContaining("create");

                env.Milestone(4);

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

        private class InfraUpdateOrderOfFields : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraUpdateOrderOfFields(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = namedWindow
                    ? "@public @public create window MyInfra#keepall as SupportBean;\n"
                    : "@public @public create table MyInfra(TheString string primary key, IntPrimitive int, IntBoxed int, DoublePrimitive double);\n";
                epl +=
                    "insert into MyInfra select TheString, IntPrimitive, IntBoxed, DoublePrimitive from SupportBean;\n";
                epl += "@name('update') on SupportBean_S0 as sb " +
                       "update MyInfra as mywin" +
                       " set IntPrimitive=Id, IntBoxed=mywin.IntPrimitive, DoublePrimitive=initial.IntPrimitive" +
                       " where mywin.TheString = sb.P00;\n";
                env.CompileDeploy(epl).AddListener("update");
                var fields = "IntPrimitive,IntBoxed,DoublePrimitive".SplitCsv();

                env.SendEventBean(MakeSupportBean("E1", 1, 2));
                env.SendEventBean(new SupportBean_S0(5, "E1"));
                env.AssertPropsPerRowLastNew("update", fields, new object[][] { new object[] { 5, 5, 1.0 } });

                env.Milestone(0);

                env.SendEventBean(MakeSupportBean("E2", 10, 20));
                env.SendEventBean(new SupportBean_S0(6, "E2"));
                env.AssertPropsPerRowLastNew("update", fields, new object[][] { new object[] { 6, 6, 10.0 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(7, "E1"));
                env.AssertPropsPerRowLastNew("update", fields, new object[][] { new object[] { 7, 7, 5.0 } });

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

        private class InfraSubquerySelf : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSubquerySelf(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                // ESPER-507
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "@name('create') @public create window MyInfraSS#keepall as SupportBean"
                    : "@name('create') @public create table MyInfraSS(TheString string primary key, IntPrimitive int)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("insert into MyInfraSS select TheString, IntPrimitive from SupportBean", path);

                // This is better done with "set intPrimitive = intPrimitive + 1"
                var epl = "@name(\"Self Update\")\n" +
                          "on SupportBean_A c\n" +
                          "update MyInfraSS s\n" +
                          "set IntPrimitive = (select IntPrimitive from MyInfraSS t where t.TheString = c.Id) + 1\n" +
                          "where s.TheString = c.Id";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 6));
                env.SendEventBean(new SupportBean_A("E1"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("E1"));
                env.SendEventBean(new SupportBean_A("E2"));

                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    "TheString,IntPrimitive".SplitCsv(),
                    new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 7 } });
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

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            double doublePrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    }
} // end of namespace