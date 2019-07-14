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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOnSelect : IndexBackingTableInfo
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowOnSelectSimple());
            execs.Add(new InfraNamedWindowOnSelectSceneTwo());
            execs.Add(new InfraNamedWindowOnSelectWPattern());
            return execs;
        }

        private static void SendSupportBean_A(
            RegressionEnvironment env,
            string id)
        {
            var bean = new SupportBean_A(id);
            env.SendEventBean(bean);
        }

        private static void SendSupportBean_B(
            RegressionEnvironment env,
            string id)
        {
            var bean = new SupportBean_B(id);
            env.SendEventBean(bean);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        public class InfraNamedWindowOnSelectSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                var path = new RegressionPath();

                var eplCreate = "@Name('create') create window MyWindow.win:keepall() as SupportBean";
                env.CompileDeploy(eplCreate, path).AddListener("create");

                var eplInsert = "@Name('insert') insert into MyWindow select * from SupportBean";
                env.CompileDeploy(eplInsert, path);

                var eplOnExpr = "@Name('delete') on SupportBean_S0 delete from MyWindow where IntPrimitive = id";
                env.CompileDeploy(eplOnExpr, path);

                env.Milestone(0);

                SendSupportBean(env, "E1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1"});

                env.UndeployAll();

                env.Milestone(2);
            }
        }

        public class InfraNamedWindowOnSelectSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportQueryPlanIndexHook.Reset();
                string[] fields = {"TheString", "IntPrimitive"};
                var path = new RegressionPath();

                var epl = "@Name('create') create window MyWindow#keepall as select * from SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean(theString like 'E%');\n" +
                          "@Name('select') on SupportBean_A insert into MyStream select mywin.* from MyWindow as mywin order by theString asc;\n" +
                          "@Name('consumer') select * from MyStream;\n" +
                          "insert into MyStream select * from SupportBean(theString like 'I%');\n";
                env.CompileDeploy(epl, path).AddListener("select").AddListener("consumer");
                Assert.AreEqual(
                    StatementType.ON_INSERT,
                    env.Statement("select").GetProperty(StatementProperty.STATEMENTTYPE));

                // send event
                SendSupportBean(env, "E1", 1);
                Assert.IsFalse(env.Listener("select").IsInvoked);
                Assert.IsFalse(env.Listener("consumer").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}});

                // fire trigger
                SendSupportBean_A(env, "A1");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});
                EPAssertionUtil.AssertProps(
                    env.Listener("consumer").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                // insert via 2nd insert into
                SendSupportBean(env, "I2", 2);
                Assert.IsFalse(env.Listener("select").IsInvoked);
                EPAssertionUtil.AssertProps(
                    env.Listener("consumer").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"I2", 2});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}});

                // send event
                SendSupportBean(env, "E3", 3);
                Assert.IsFalse(env.Listener("select").IsInvoked);
                Assert.IsFalse(env.Listener("consumer").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 3}});

                // fire trigger
                SendSupportBean_A(env, "A2");
                Assert.AreEqual(1, env.Listener("select").NewDataList.Count);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("select").LastNewData,
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 3}});
                env.Listener("select").Reset();
                Assert.AreEqual(2, env.Listener("consumer").NewDataList.Count);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("consumer").NewDataListFlattened,
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 3}});
                env.Listener("consumer").Reset();

                // check type
                var consumerType = env.Statement("consumer").EventType;
                Assert.AreEqual(typeof(string), consumerType.GetPropertyType("TheString"));
                Assert.IsTrue(consumerType.PropertyNames.Length > 10);
                Assert.AreEqual(typeof(SupportBean), consumerType.UnderlyingType);

                // check type
                var onSelectType = env.Statement("select").EventType;
                Assert.AreEqual(typeof(string), onSelectType.GetPropertyType("TheString"));
                Assert.IsTrue(onSelectType.PropertyNames.Length > 10);
                Assert.AreEqual(typeof(SupportBean), onSelectType.UnderlyingType);

                // delete all from named window
                var stmtTextDelete = "@Name('delete') on SupportBean_B delete from MyWindow";
                env.CompileDeploy(stmtTextDelete, path);
                SendSupportBean_B(env, "B1");

                // fire trigger - nothing to insert
                SendSupportBean_A(env, "A3");

                env.UndeployModuleContaining("delete");
                env.UndeployModuleContaining("create");
            }
        }

        internal class InfraNamedWindowOnSelectWPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow.win:keepall() as SupportBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean(TheString = 'Z')", path);
                env.SendEventBean(new SupportBean("Z", 0));

                var epl =
                    "@Name('s0') on pattern[every e = SupportBean(TheString = 'A') => SupportBean(IntPrimitive = e.IntPrimitive)] select * from MyWindow";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 1));
                env.SendEventBean(new SupportBean("B", 1));
                env.Listener("s0").AssertOneGetNewAndReset();

                env.UndeployAll();
            }
        }
    }
} // end of namespace