///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableOnUpdate
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();

            WithOnUpdateSceneOne(execs);
            WithUpdateOrderOfFields(execs);
            WithSubquerySelf(execs);
            WithSubqueryMultikeyWArray(execs);

            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraSubqueryMultikeyWArray(true));
            execs.Add(new InfraSubqueryMultikeyWArray(false));
            return execs;
        }

        public static IList<RegressionExecution> WithSubquerySelf(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraSubquerySelf(true));
            execs.Add(new InfraSubquerySelf(false));
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateOrderOfFields(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraUpdateOrderOfFields(true));
            execs.Add(new InfraUpdateOrderOfFields(false));
            return execs;
        }

        public static IList<RegressionExecution> WithOnUpdateSceneOne(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraNWTableOnUpdateSceneOne(true));
            execs.Add(new InfraNWTableOnUpdateSceneOne(false));
            return execs;
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

        internal class InfraSubqueryMultikeyWArray : RegressionExecution
        {

            private bool namedWindow;

            public InfraSubqueryMultikeyWArray(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                RegressionPath path = new RegressionPath();
                string stmtTextCreate = namedWindow
                    ? "@Name('create') create window MyInfra#keepall() as (Value int)"
                    : "@Name('create') create table MyInfra(Value int)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");
                env.CompileExecuteFAF("insert into MyInfra select 0 as Value", path);

                string epl = "on SupportBean update MyInfra set Value = (select sum(Value) as c0 from SupportEventWithIntArray#keepall group by Array)";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportEventWithIntArray("E1", new int[] {1, 2}, 10));
                env.SendEventBean(new SupportEventWithIntArray("E2", new int[] {1, 2}, 11));

                env.Milestone(0);
                AssertUpdate(env, 21);

                env.SendEventBean(new SupportEventWithIntArray("E3", new int[] {1, 2}, 12));
                AssertUpdate(env, 33);

                env.Milestone(1);

                env.SendEventBean(new SupportEventWithIntArray("E4", new int[] {1}, 13));
                AssertUpdate(env, null);

                env.UndeployAll();
            }

            private void AssertUpdate(
                RegressionEnvironment env,
                int? expected)
            {
                env.SendEventBean(new SupportBean());
                var enumerator = env.GetEnumerator("create");
                Assert.That(enumerator.MoveNext(), Is.True);
                Assert.That(enumerator.Current, Is.Not.Null);
                Assert.That(enumerator.Current.Get("Value"), Is.EqualTo(expected));
            }
        }

        public class InfraNWTableOnUpdateSceneOne : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraNWTableOnUpdateSceneOne(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate = namedWindow
                    ? "@Name('create') create window MyInfra.win:keepall() as SupportBean"
                    : "@Name('create') create table MyInfra(TheString string, IntPrimitive int primary key)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "@Name('insert') insert into MyInfra select TheString, IntPrimitive from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(0);

                // populate some data
                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("B2", 2));

                // create onUpdate
                var stmtTextOnUpdate =
                    "@Name('update') on SupportBean_S0 update MyInfra set TheString = P00 where IntPrimitive = Id";
                env.CompileDeploy(stmtTextOnUpdate, path).AddListener("update");
                Assert.AreEqual(
                    StatementType.ON_UPDATE,
                    env.Statement("update").GetProperty(StatementProperty.STATEMENTTYPE));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(1, "X1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("update").AssertOneGetOld(),
                    fields,
                    new object[] {"A1", 1});
                EPAssertionUtil.AssertProps(
                    env.Listener("update").LastNewData[0],
                    fields,
                    new object[] {"X1", 1});
                env.Listener("update").Reset();
                if (namedWindow) {
                    EPAssertionUtil.AssertPropsPerRow(
                        env.GetEnumerator("create"),
                        fields,
                        new[] {new object[] {"B2", 2}, new object[] {"X1", 1}});
                }
                else {
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        env.GetEnumerator("create"),
                        fields,
                        new[] {new object[] {"B2", 2}, new object[] {"X1", 1}});
                }

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(2, "X2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("update").AssertOneGetOld(),
                    fields,
                    new object[] {"B2", 2});
                EPAssertionUtil.AssertProps(
                    env.Listener("update").LastNewData[0],
                    fields,
                    new object[] {"X2", 2});
                env.Listener("update").Reset();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"X1", 1}, new object[] {"X2", 2}});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"X1", 1}, new object[] {"X2", 2}});

                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("update");
                env.UndeployModuleContaining("create");

                env.Milestone(4);

                env.UndeployAll();
            }
        }

        internal class InfraUpdateOrderOfFields : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraUpdateOrderOfFields(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = namedWindow
                    ? "create window MyInfra#keepall as SupportBean;\n"
                    : "create table MyInfra(TheString string primary key, IntPrimitive int, IntBoxed int, DoublePrimitive double);\n";
                epl +=
                    "insert into MyInfra select TheString, IntPrimitive, IntBoxed, DoublePrimitive from SupportBean;\n";
                epl += "@Name('update') on SupportBean_S0 as sb " +
                       "update MyInfra as mywin" +
                       " set IntPrimitive=Id, IntBoxed=mywin.IntPrimitive, DoublePrimitive=initial.IntPrimitive" +
                       " where mywin.TheString = sb.P00;\n";
                env.CompileDeploy(epl).AddListener("update");
                var fields = new [] { "IntPrimitive","IntBoxed","DoublePrimitive" };

                env.SendEventBean(MakeSupportBean("E1", 1, 2));
                env.SendEventBean(new SupportBean_S0(5, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("update").GetAndResetLastNewData()[0],
                    fields,
                    new object[] {5, 5, 1.0});

                env.Milestone(0);

                env.SendEventBean(MakeSupportBean("E2", 10, 20));
                env.SendEventBean(new SupportBean_S0(6, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("update").GetAndResetLastNewData()[0],
                    fields,
                    new object[] {6, 6, 10.0});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(7, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("update").GetAndResetLastNewData()[0],
                    fields,
                    new object[] {7, 7, 5.0});

                env.UndeployAll();
            }
        }

        internal class InfraSubquerySelf : RegressionExecution
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
                    ? "@Name('create') create window MyInfraSS#keepall as SupportBean"
                    : "@Name('create') create table MyInfraSS(TheString string primary key, IntPrimitive int)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("insert into MyInfraSS select TheString, IntPrimitive from SupportBean", path);

                // This is better done with "set IntPrimitive = IntPrimitive + 1"
                var epl = "@Name(\"Self Update\")\n" +
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

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    new [] { "TheString","IntPrimitive" },
                    new[] {new object[] {"E1", 3}, new object[] {"E2", 7}});
                env.UndeployAll();
            }
        }
    }
} // end of namespace