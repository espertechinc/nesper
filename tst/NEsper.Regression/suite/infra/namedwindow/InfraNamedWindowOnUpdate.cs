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
            WithUpdateNonPropertySet(execs);
            WithMultipleDataWindowIntersect(execs);
            WithMultipleDataWindowUnion(execs);
            WithSubclass(execs);
            WithUpdateCopyMethodBean(execs);
            WithUpdateWrapper(execs);
            WithUpdateMultikeyWArrayPrimitiveArray(execs);
            WithUpdateMultikeyWArrayTwoFields(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateMultikeyWArrayTwoFields(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraUpdateMultikeyWArrayTwoFields());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateMultikeyWArrayPrimitiveArray(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraUpdateMultikeyWArrayPrimitiveArray());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateWrapper(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraUpdateWrapper());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateCopyMethodBean(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraUpdateCopyMethodBean());
            return execs;
        }

        public static IList<RegressionExecution> WithSubclass(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraSubclass());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleDataWindowUnion(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraMultipleDataWindowUnion());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleDataWindowIntersect(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraMultipleDataWindowIntersect());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateNonPropertySet(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraUpdateNonPropertySet());
            return execs;
        }

        // Don't delete me, dynamically-invoked
        public static void SetBeanLongPrimitive999(SupportBean @event)
        {
            @event.LongPrimitive = 999;
        }

        internal class InfraUpdateMultikeyWArrayTwoFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('create') create window MyWindow#keepall as SupportEventWithManyArray;\n" +
                             "insert into MyWindow select * from SupportEventWithManyArray;\n" +
                             "on SupportEventWithIntArray as sewia " +
                             "update MyWindow as mw set Value = sewia.Value " +
                             "where mw.Id = sewia.Id and mw.IntOne = sewia.Array;\n";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportEventWithManyArray("ID1").WithIntOne(new int[] {1, 2}));
                env.SendEventBean(new SupportEventWithManyArray("ID2").WithIntOne(new int[] {3, 4}));
                env.SendEventBean(new SupportEventWithManyArray("ID3").WithIntOne(new int[] {1}));

                env.Milestone(0);

                env.SendEventBean(new SupportEventWithIntArray("ID2", new int[] {3, 4}, 10));
                env.SendEventBean(new SupportEventWithIntArray("ID3", new int[] {1}, 11));
                env.SendEventBean(new SupportEventWithIntArray("ID1", new int[] {1, 2}, 12));
                env.SendEventBean(new SupportEventWithIntArray("IDX", new int[] {1}, 14));
                env.SendEventBean(new SupportEventWithIntArray("ID1", new int[] {1, 2, 3}, 15));

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    "Id,Value".SplitCsv(),
                    new object[][] {
                        new object[] {"ID1", 12},
                        new object[] {"ID2", 10},
                        new object[] {"ID3", 11}
                    });

                env.UndeployAll();
            }
        }

        internal class InfraUpdateMultikeyWArrayPrimitiveArray : RegressionExecution
        {

            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('create') create window MyWindow#keepall as SupportEventWithManyArray;\n" +
                             "insert into MyWindow select * from SupportEventWithManyArray;\n" +
                             "on SupportEventWithIntArray as sewia " +
                             "update MyWindow as mw set Value = sewia.Value " +
                             "where mw.IntOne = sewia.Array;\n";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportEventWithManyArray("E1").WithIntOne(new int[] {1, 2}));
                env.SendEventBean(new SupportEventWithManyArray("E2").WithIntOne(new int[] {3, 4}));
                env.SendEventBean(new SupportEventWithManyArray("E3").WithIntOne(new int[] {1}));
                env.SendEventBean(new SupportEventWithManyArray("E4").WithIntOne(new int[] { }));

                env.Milestone(0);

                env.SendEventBean(new SupportEventWithIntArray("U1", new int[] {3, 4}, 10));
                env.SendEventBean(new SupportEventWithIntArray("U2", new int[] {1}, 11));
                env.SendEventBean(new SupportEventWithIntArray("U3", new int[] { }, 12));
                env.SendEventBean(new SupportEventWithIntArray("U4", new int[] {1, 2}, 13));

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    "Id,Value".SplitCsv(),
                    new object[][] {
                        new object[] {"E1", 13},
                        new object[] {"E2", 10},
                        new object[] {"E3", 11},
                        new object[] {"E4", 12}
                    });

                env.UndeployAll();
            }
        }

        internal class InfraUpdateWrapper : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('window') create window MyWindow#keepall as select *, 1 as P0 from SupportBean;\n" +
                          "insert into MyWindow select *, 2 as P0 from SupportBean;\n" +
                          "on SupportBean_S0 update MyWindow set TheString = 'x', P0 = 2;\n";
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
                          "on SupportBean update MyWindowBeanCopyMethod set ValOne = 'x';\n";
                env.CompileDeploy(epl);
                env.SendEventBean(new SupportBeanCopyMethod("a", "b"));
                env.SendEventBean(new SupportBean());
                Assert.AreEqual("x", env.GetEnumerator("window").Advance().Get("ValOne"));

                env.UndeployAll();
            }
        }

        internal class InfraUpdateNonPropertySet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindowUNP#keepall as SupportBean;\n" +
                          "insert into MyWindowUNP select * from SupportBean;\n" +
                          "@Name('update') on SupportBean_S0 as sb " +
                          "update MyWindowUNP as mywin" +
                          " set mywin.IntPrimitive = (10)," +
                          "     SetBeanLongPrimitive999(mywin);\n";
                env.CompileDeploy(epl).AddListener("update");

                var fields = new[] {"IntPrimitive", "LongPrimitive"};
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
                    new[] {"IntPrimitive"},
                    new object[] {300});
                Assert.AreEqual(1, oldevents.Length);
                oldevents = EPAssertionUtil.Sort(oldevents, "TheString");
                EPAssertionUtil.AssertPropsPerRow(
                    oldevents,
                    new[] {"TheString", "IntPrimitive"},
                    new[] {new object[] {"E2", 3}});

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    new[] {"TheString", "IntPrimitive"},
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
                    new[] {"IntPrimitive"},
                    new object[] {300});
                Assert.AreEqual(1, oldevents.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    oldevents,
                    new[] {"TheString", "IntPrimitive"},
                    new[] {new object[] {"E2", 3}});

                var events = EPAssertionUtil.Sort(env.GetEnumerator("create"), "TheString");
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    new[] {"TheString", "IntPrimitive"},
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
                    "on SupportBean update MyWindowSC set V1=TheString, V2=TheString;\n";
                env.CompileDeploy(epl).AddListener("create");

                env.SendEventBean(new SupportBeanAbstractSub("value2"));
                env.Listener("create").Reset();

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    new[] {"V1", "V2"},
                    new object[] {"E1", "E1"});

                env.UndeployAll();
            }
        }
    }
} // end of namespace