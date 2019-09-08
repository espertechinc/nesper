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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOnUpdate
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraUpdateNonPropertySet());
            execs.Add(new InfraMultipleDataWindowIntersect());
            execs.Add(new InfraMultipleDataWindowUnion());
            execs.Add(new InfraSubclass());
            execs.Add(new InfraUpdateCopyMethodBean());
            execs.Add(new InfraUpdateWrapper());
            return execs;
        }

        // Don't delete me, dynamically-invoked
        public static void SetBeanLongPrimitive999(SupportBean @event)
        {
            @event.LongPrimitive = 999;
        }

        internal class InfraUpdateWrapper : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('window') create window MyWindow#keepall as select *, 1 as p0 from SupportBean;\n" +
                          "insert into MyWindow select *, 2 as p0 from SupportBean;\n" +
                          "on SupportBean_S0 update MyWindow set TheString = 'x', p0 = 2;\n";
                env.CompileDeploy(epl);
                env.SendEventBean(new SupportBean("E1", 100));
                env.SendEventBean(new SupportBean_S0(-1));
                EPAssertionUtil.AssertProps(
                    env.GetEnumerator("window").Advance(),
                    new[] {"TheString", "P0"},
                    new object[] {"x", 2});

                env.UndeployAll();
            }
        }

        internal class InfraUpdateCopyMethodBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('window') create window MyWindowBeanCopyMethod#keepall as SupportBeanCopyMethod;\n" +
                          "insert into MyWindowBeanCopyMethod select * from SupportBeanCopyMethod;\n" +
                          "on SupportBean update MyWindowBeanCopyMethod set valOne = 'x';\n";
                env.CompileDeploy(epl);
                env.SendEventBean(new SupportBeanCopyMethod("a", "b"));
                env.SendEventBean(new SupportBean());
                Assert.AreEqual("x", env.GetEnumerator("window").Advance().Get("valOne"));

                env.UndeployAll();
            }
        }

        internal class InfraUpdateNonPropertySet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindowUNP#keepall as SupportBean;\n" +
                          "insert into MyWindowUNP select * from SupportBean;\n" +
                          "@Name('Update') on SupportBean_S0 as sb " +
                          "update MyWindowUNP as mywin" +
                          " set mywin.setIntPrimitive(10)," +
                          "     setBeanLongPrimitive999(mywin);\n";
                env.CompileDeploy(epl).AddListener("update");

                var fields = new [] { "IntPrimitive","LongPrimitive" };
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertProps(
                    env.Listener("update").GetAndResetLastNewData()[0],
                    fields,
                    new object[] {10, 999L});

                env.UndeployAll();
            }
        }

        internal class InfraMultipleDataWindowIntersect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('create') create window MyWindowMDW#unique(TheString)#length(2) as select * from SupportBean;\n" +
                    "insert into MyWindowMDW select * from SupportBean;\n" +
                    "on SupportBean_A update MyWindowMDW set IntPrimitive=IntPrimitive*100 where TheString=Id;\n";
                env.CompileDeploy(epl).AddListener("create");

                env.SendEventBean(new SupportBean("E1", 2));
                env.SendEventBean(new SupportBean("E2", 3));
                env.SendEventBean(new SupportBean_A("E2"));
                var newevents = env.Listener("create").LastNewData;
                var oldevents = env.Listener("create").LastOldData;

                Assert.AreEqual(1, newevents.Length);
                EPAssertionUtil.AssertProps(
                    newevents[0],
                    new [] { "IntPrimitive" },
                    new object[] {300});
                Assert.AreEqual(1, oldevents.Length);
                oldevents = EPAssertionUtil.Sort(oldevents, "TheString");
                EPAssertionUtil.AssertPropsPerRow(
                    oldevents,
                    new [] { "TheString","IntPrimitive" },
                    new[] {new object[] {"E2", 3}});

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    new [] { "TheString","IntPrimitive" },
                    new[] {new object[] {"E1", 2}, new object[] {"E2", 300}});

                env.UndeployAll();
            }
        }

        internal class InfraMultipleDataWindowUnion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('create') create window MyWindowMU#unique(TheString)#length(2) retain-union as select * from SupportBean;\n" +
                    "insert into MyWindowMU select * from SupportBean;\n" +
                    "on SupportBean_A update MyWindowMU mw set mw.IntPrimitive=IntPrimitive*100 where TheString=Id;\n";
                env.CompileDeploy(epl).AddListener("create");

                env.SendEventBean(new SupportBean("E1", 2));
                env.SendEventBean(new SupportBean("E2", 3));
                env.SendEventBean(new SupportBean_A("E2"));
                var newevents = env.Listener("create").LastNewData;
                var oldevents = env.Listener("create").LastOldData;

                Assert.AreEqual(1, newevents.Length);
                EPAssertionUtil.AssertProps(
                    newevents[0],
                    new [] { "IntPrimitive" },
                    new object[] {300});
                Assert.AreEqual(1, oldevents.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    oldevents,
                    new [] { "TheString","IntPrimitive" },
                    new[] {new object[] {"E2", 3}});

                var events = EPAssertionUtil.Sort(env.GetEnumerator("create"), "TheString");
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    new [] { "TheString","IntPrimitive" },
                    new[] {new object[] {"E1", 2}, new object[] {"E2", 300}});

                env.UndeployAll();
            }
        }

        internal class InfraSubclass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('create') create window MyWindowSC#keepall as select * from SupportBeanAbstractSub;\n" +
                    "insert into MyWindowSC select * from SupportBeanAbstractSub;\n" +
                    "on SupportBean update MyWindowSC set v1=TheString, v2=TheString;\n";
                env.CompileDeploy(epl).AddListener("create");

                env.SendEventBean(new SupportBeanAbstractSub("value2"));
                env.Listener("create").Reset();

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    new[] {"v1", "v2"},
                    new object[] {"E1", "E1"});

                env.UndeployAll();
            }
        }
    }
} // end of namespace