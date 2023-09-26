///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOnSelect : IndexBackingTableInfo
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithSceneTwo(execs);
            WithWPattern(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowOnSelectWPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowOnSelectSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowOnSelectSimple());
            return execs;
        }

        public class InfraNamedWindowOnSelectSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString".SplitCsv();
                var path = new RegressionPath();

                var eplCreate = "@name('create') @public create window MyWindow.win:keepall() as SupportBean";
                env.CompileDeploy(eplCreate, path).AddListener("create");

                var eplInsert = "@name('insert') insert into MyWindow select * from SupportBean";
                env.CompileDeploy(eplInsert, path);

                var eplOnExpr = "@name('delete') on SupportBean_S0 delete from MyWindow where intPrimitive = id";
                env.CompileDeploy(eplOnExpr, path);

                env.Milestone(0);

                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("create", fields, new object[] { "E1" });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsOld("create", fields, new object[] { "E1" });

                env.UndeployAll();

                env.Milestone(2);
            }
        }

        public class InfraNamedWindowOnSelectSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportQueryPlanIndexHook.Reset();
                var fields = new string[] { "theString", "intPrimitive" };
                var path = new RegressionPath();

                var epl = "@name('create') @public create window MyWindow#keepall as select * from SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean(theString like 'E%');\n" +
                          "@name('select') on SupportBean_A insert into MyStream select mywin.* from MyWindow as mywin order by theString asc;\n" +
                          "@name('consumer') select * from MyStream;\n" +
                          "insert into MyStream select * from SupportBean(theString like 'I%');\n";
                env.CompileDeploy(epl, path).AddListener("select").AddListener("consumer");
                env.AssertStatement(
                    "select",
                    statement => Assert.AreEqual(
                        StatementType.ON_INSERT,
                        statement.GetProperty(StatementProperty.STATEMENTTYPE)));

                // send event
                SendSupportBean(env, "E1", 1);
                env.AssertListenerNotInvoked("select");
                env.AssertListenerNotInvoked("consumer");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1 } });

                // fire trigger
                SendSupportBean_A(env, "A1");
                env.AssertPropsNew("select", fields, new object[] { "E1", 1 });
                env.AssertPropsNew("consumer", fields, new object[] { "E1", 1 });

                // insert via 2nd insert into
                SendSupportBean(env, "I2", 2);
                env.AssertListenerNotInvoked("select");
                env.AssertPropsNew("consumer", fields, new object[] { "I2", 2 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1 } });

                // send event
                SendSupportBean(env, "E3", 3);
                env.AssertListenerNotInvoked("select");
                env.AssertListenerNotInvoked("consumer");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });

                // fire trigger
                SendSupportBean_A(env, "A2");
                env.AssertPropsPerRowNewOnly(
                    "select",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
                env.AssertPropsPerRowNewFlattened(
                    "consumer",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });

                // check type
                env.AssertStatement(
                    "consumer",
                    statement => {
                        var consumerType = statement.EventType;
                        Assert.AreEqual(typeof(string), consumerType.GetPropertyType("theString"));
                        Assert.IsTrue(consumerType.PropertyNames.Length > 10);
                        Assert.AreEqual(typeof(SupportBean), consumerType.UnderlyingType);
                    });

                // check type
                env.AssertStatement(
                    "select",
                    statement => {
                        var onSelectType = statement.EventType;
                        Assert.AreEqual(typeof(string), onSelectType.GetPropertyType("theString"));
                        Assert.IsTrue(onSelectType.PropertyNames.Length > 10);
                        Assert.AreEqual(typeof(SupportBean), onSelectType.UnderlyingType);
                    });

                // delete all from named window
                var stmtTextDelete = "@name('delete') on SupportBean_B delete from MyWindow";
                env.CompileDeploy(stmtTextDelete, path);
                SendSupportBean_B(env, "B1");

                // fire trigger - nothing to insert
                SendSupportBean_A(env, "A3");

                env.UndeployModuleContaining("delete");
                env.UndeployModuleContaining("create");
            }
        }

        private class InfraNamedWindowOnSelectWPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindow.win:keepall() as SupportBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean(theString = 'Z')", path);
                env.SendEventBean(new SupportBean("Z", 0));

                var epl =
                    "@name('s0') on pattern[every e = SupportBean(theString = 'A') -> SupportBean(intPrimitive = e.intPrimitive)] select * from MyWindow";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 1));
                env.SendEventBean(new SupportBean("B", 1));
                env.AssertListener("s0", _ => _.AssertOneGetNewAndReset());

                env.UndeployAll();
            }
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
    }
} // end of namespace