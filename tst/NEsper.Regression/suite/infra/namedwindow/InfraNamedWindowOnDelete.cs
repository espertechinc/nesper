///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOnDelete
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithFirstUnique(execs);
            WithStaggeredNamedWindow(execs);
            WithCoercionKeyMultiPropIndexes(execs);
            WithCoercionRangeMultiPropIndexes(execs);
            WithCoercionKeyAndRangeMultiPropIndexes(execs);
            WithNamedWindowSilentDeleteOnDelete(execs);
            WithNamedWindowSilentDeleteOnDeleteMany(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowSilentDeleteOnDeleteMany(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowSilentDeleteOnDeleteMany());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowSilentDeleteOnDelete(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowSilentDeleteOnDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithCoercionKeyAndRangeMultiPropIndexes(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCoercionKeyAndRangeMultiPropIndexes());
            return execs;
        }

        public static IList<RegressionExecution> WithCoercionRangeMultiPropIndexes(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCoercionRangeMultiPropIndexes());
            return execs;
        }

        public static IList<RegressionExecution> WithCoercionKeyMultiPropIndexes(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCoercionKeyMultiPropIndexes());
            return execs;
        }

        public static IList<RegressionExecution> WithStaggeredNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraStaggeredNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFirstUnique());
            return execs;
        }

        private class InfraNamedWindowSilentDeleteOnDeleteMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('create') create window MyWindow#groupwin(TheString)#length(2) as SupportBean;\n" +
                    "insert into MyWindow select * from SupportBean;\n" +
                    "@name('delete') @hint('silent_delete') on SupportBean_S0 delete from MyWindow;\n" +
                    "@name('count') select count(*) as cnt from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("delete").AddListener("count");

                env.SendEventBean(new SupportBean("A", 1));
                env.SendEventBean(new SupportBean("A", 2));
                env.SendEventBean(new SupportBean("B", 3));
                env.SendEventBean(new SupportBean("B", 4));

                env.AssertListener(
                    "count",
                    listener => { ClassicAssert.AreEqual(4L, listener.GetAndResetDataListsFlattened().First[3].Get("cnt")); });
                env.ListenerReset("create");

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertListener(
                    "count",
                    listener => ClassicAssert.AreEqual(0L, listener.AssertOneGetNewAndReset().Get("cnt")));
                env.AssertPropsPerRowLastNew(
                    "delete",
                    "TheString,IntPrimitive".SplitCsv(),
                    new object[][] {
                        new object[] { "A", 1 }, new object[] { "A", 2 }, new object[] { "B", 3 },
                        new object[] { "B", 4 }
                    });
                env.AssertListenerNotInvoked("create");

                env.UndeployAll();
            }
        }

        private class InfraNamedWindowSilentDeleteOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('create') create window MyWindow#length(2) as SupportBean;\n" +
                    "insert into MyWindow select * from SupportBean;\n" +
                    "@name('delete') @hint('silent_delete') on SupportBean_S0 delete from MyWindow where P00 = TheString;\n" +
                    "@name('count') select count(*) as cnt from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("delete").AddListener("count");

                env.SendEventBean(new SupportBean("E1", 1));
                AssertCount(env, 1L);
                AssertString(env, "E1");

                env.SendEventBean(new SupportBean_S0(0, "E1"));
                AssertCount(env, 0L);
                env.AssertEqualsNew("delete", "TheString", "E1");
                env.AssertListenerNotInvoked("create");

                env.SendEventBean(new SupportBean("E2", 2));
                AssertCount(env, 1L);
                AssertString(env, "E2");

                env.SendEventBean(new SupportBean("E3", 3));
                AssertCount(env, 2L);
                AssertString(env, "E3");

                env.SendEventBean(new SupportBean("E4", 4));
                AssertCount(env, 2L);
                env.AssertPropsIRPair("create", "TheString".SplitCsv(), new object[] { "E4" }, new object[] { "E2" });

                env.SendEventBean(new SupportBean_S0(0, "E4"));
                AssertCount(env, 1L);
                env.AssertEqualsNew("delete", "TheString", "E4");
                env.AssertListenerNotInvoked("create");

                env.SendEventBean(new SupportBean_S0(0, "E3"));
                AssertCount(env, 0L);
                env.AssertEqualsNew("delete", "TheString", "E3");
                env.AssertListenerNotInvoked("create");

                env.SendEventBean(new SupportBean_S0(0, "EX"));
                env.AssertListenerNotInvoked("count");
                env.AssertListenerNotInvoked("delete");
                env.AssertListenerNotInvoked("create");

                env.UndeployAll();
            }
        }

        private class InfraFirstUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString", "IntPrimitive" };
                var epl =
                    "@name('create') create window MyWindowFU#firstunique(TheString) as select * from SupportBean;\n" +
                    "insert into MyWindowFU select * from SupportBean;\n" +
                    "@name('delete') on SupportBean_A a delete from MyWindowFU where TheString=a.Id;\n";
                env.CompileDeploy(epl).AddListener("delete");

                env.SendEventBean(new SupportBean("A", 1));
                env.SendEventBean(new SupportBean("A", 2));

                env.SendEventBean(new SupportBean_A("A"));
                env.AssertPropsNew("delete", fields, new object[] { "A", 1 });

                env.SendEventBean(new SupportBean("A", 3));
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "A", 3 } });

                env.SendEventBean(new SupportBean_A("A"));
                env.AssertPropsPerRowIterator("create", fields, null);

                env.UndeployAll();
            }
        }

        private class InfraStaggeredNamedWindow : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }

            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionStaggered(env, rep);
                }
            }
        }

        private class InfraCoercionKeyMultiPropIndexes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // create window
                var stmtTextCreate = "@name('createOne') @public create window MyWindowCK#keepall as select " +
                                     "TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("createOne");

                IList<string> deleteStatements = new List<string>();
                var stmtTextDelete =
                    "@name('d1') on SupportBean(TheString='DB') as s0 delete from MyWindowCK as win where win.IntPrimitive = s0.DoubleBoxed";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d1");
                AssertIndexCount(env, 1, "createOne", "MyWindowCK");

                stmtTextDelete =
                    "@name('d2') on SupportBean(TheString='DP') as s0 delete from MyWindowCK as win where win.IntPrimitive = s0.DoublePrimitive";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d2");
                AssertIndexCount(env, 1, "createOne", "MyWindowCK");

                stmtTextDelete =
                    "@name('d3') on SupportBean(TheString='IB') as s0 delete from MyWindowCK where MyWindowCK.IntPrimitive = s0.IntBoxed";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d3");
                AssertIndexCount(env, 2, "createOne", "MyWindowCK");

                stmtTextDelete =
                    "@name('d4') on SupportBean(TheString='IPDP') as s0 delete from MyWindowCK as win where win.IntPrimitive = s0.IntPrimitive and win.DoublePrimitive = s0.DoublePrimitive";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d4");
                AssertIndexCount(env, 3, "createOne", "MyWindowCK");

                stmtTextDelete =
                    "@name('d5') on SupportBean(TheString='IPDP2') as s0 delete from MyWindowCK as win where win.DoublePrimitive = s0.DoublePrimitive and win.IntPrimitive = s0.IntPrimitive";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d5");
                AssertIndexCount(env, 4, "createOne", "MyWindowCK");

                stmtTextDelete =
                    "@name('d6') on SupportBean(TheString='IPDPIB') as s0 delete from MyWindowCK as win where win.DoublePrimitive = s0.DoublePrimitive and win.IntPrimitive = s0.IntPrimitive and win.IntBoxed = s0.IntBoxed";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d6");
                AssertIndexCount(env, 5, "createOne", "MyWindowCK");

                stmtTextDelete =
                    "@name('d7') on SupportBean(TheString='CAST') as s0 delete from MyWindowCK as win where win.IntBoxed = s0.IntPrimitive and win.DoublePrimitive = s0.DoubleBoxed and win.IntPrimitive = s0.IntBoxed";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d7");
                AssertIndexCount(env, 6, "createOne", "MyWindowCK");

                // create insert into
                var stmtTextInsertOne =
                    "insert into MyWindowCK select TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed " +
                    "from SupportBean(TheString like 'E%')";
                env.CompileDeploy(stmtTextInsertOne, path);

                SendSupportBean(env, "E1", 1, 10, 100d, 1000d);
                SendSupportBean(env, "E2", 2, 20, 200d, 2000d);
                SendSupportBean(env, "E3", 3, 30, 300d, 3000d);
                SendSupportBean(env, "E4", 4, 40, 400d, 4000d);
                env.ListenerReset("createOne");

                var fields = new string[] { "TheString" };
                env.AssertPropsPerRowIterator(
                    "createOne",
                    fields,
                    new object[][]
                        { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" } });

                SendSupportBean(env, "DB", 0, 0, 0d, null);
                env.AssertListenerNotInvoked("createOne");
                SendSupportBean(env, "DB", 0, 0, 0d, 3d);
                env.AssertPropsOld("createOne", fields, new object[] { "E3" });
                env.AssertPropsPerRowIterator(
                    "createOne",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E4" } });

                SendSupportBean(env, "DP", 0, 0, 5d, null);
                env.AssertListenerNotInvoked("createOne");
                SendSupportBean(env, "DP", 0, 0, 4d, null);
                env.AssertPropsOld("createOne", fields, new object[] { "E4" });
                env.AssertPropsPerRowIterator(
                    "createOne",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                SendSupportBean(env, "IB", 0, -1, 0d, null);
                env.AssertListenerNotInvoked("createOne");
                SendSupportBean(env, "IB", 0, 1, 0d, null);
                env.AssertPropsOld("createOne", fields, new object[] { "E1" });
                env.AssertPropsPerRowIterator("createOne", fields, new object[][] { new object[] { "E2" } });

                SendSupportBean(env, "E5", 5, 50, 500d, 5000d);
                SendSupportBean(env, "E6", 6, 60, 600d, 6000d);
                SendSupportBean(env, "E7", 7, 70, 700d, 7000d);
                env.ListenerReset("createOne");

                SendSupportBean(env, "IPDP", 5, 0, 500d, null);
                env.AssertPropsOld("createOne", fields, new object[] { "E5" });
                env.AssertPropsPerRowIterator(
                    "createOne",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E6" }, new object[] { "E7" } });

                SendSupportBean(env, "IPDP2", 6, 0, 600d, null);
                env.AssertPropsOld("createOne", fields, new object[] { "E6" });
                env.AssertPropsPerRowIterator(
                    "createOne",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E7" } });

                SendSupportBean(env, "IPDPIB", 7, 70, 0d, null);
                env.AssertListenerNotInvoked("createOne");
                SendSupportBean(env, "IPDPIB", 7, 70, 700d, null);
                env.AssertPropsOld("createOne", fields, new object[] { "E7" });
                env.AssertPropsPerRowIterator("createOne", fields, new object[][] { new object[] { "E2" } });

                SendSupportBean(env, "E8", 8, 80, 800d, 8000d);
                env.ListenerReset("createOne");
                env.AssertPropsPerRowIterator(
                    "createOne",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E8" } });

                SendSupportBean(env, "CAST", 80, 8, 0, 800d);
                env.AssertPropsOld("createOne", fields, new object[] { "E8" });
                env.AssertPropsPerRowIterator("createOne", fields, new object[][] { new object[] { "E2" } });

                foreach (var stmtName in deleteStatements) {
                    env.UndeployModuleContaining(stmtName);
                }

                deleteStatements.Clear();

                // late delete on a filled window
                stmtTextDelete =
                    "@name('d0') on SupportBean(TheString='LAST') as s0 delete from MyWindowCK as win where win.IntPrimitive = s0.IntPrimitive and win.DoublePrimitive = s0.DoublePrimitive";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d0");
                SendSupportBean(env, "LAST", 2, 20, 200, 2000d);
                env.AssertPropsOld("createOne", fields, new object[] { "E2" });
                env.AssertPropsPerRowIterator("createOne", fields, null);

                foreach (var stmtName in deleteStatements) {
                    env.UndeployModuleContaining(stmtName);
                }

                AssertIndexCount(env, 0, "createOne", "MyWindowCK");
                env.UndeployAll();

                // test single-two-field index reuse
                path = new RegressionPath();
                env.CompileDeploy("@name('createTwo') @public create window WinOne#keepall as SupportBean", path);
                env.CompileDeploy("on SupportBean_ST0 select * from WinOne where TheString = Key0", path);
                AssertIndexCount(env, 1, "createTwo", "WinOne");

                env.CompileDeploy(
                    "on SupportBean_ST0 select * from WinOne where TheString = Key0 and IntPrimitive = P00",
                    path);
                AssertIndexCount(env, 2, "createTwo", "WinOne");

                env.UndeployAll();
            }
        }

        private class InfraCoercionRangeMultiPropIndexes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var stmtTextCreate = "@name('createOne') @public create window MyWindowCR#keepall as select " +
                                     "TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("createOne");

                var stmtText =
                    "insert into MyWindowCR select TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtText, path);
                var fields = new string[] { "TheString" };

                SendSupportBean(env, "E1", 1, 10, 100d, 1000d);
                SendSupportBean(env, "E2", 2, 20, 200d, 2000d);
                SendSupportBean(env, "E3", 3, 30, 3d, 30d);
                SendSupportBean(env, "E4", 4, 40, 4d, 40d);
                SendSupportBean(env, "E5", 5, 50, 500d, 5000d);
                SendSupportBean(env, "E6", 6, 60, 600d, 6000d);
                env.ListenerReset("createOne");

                IList<string> deleteStatements = new List<string>();
                var stmtTextDelete =
                    "@name('d0') on SupportBeanTwo as s2 delete from MyWindowCR as win where win.IntPrimitive between s2.DoublePrimitiveTwo and s2.DoubleBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d0");
                AssertIndexCount(env, 1, "createOne", "MyWindowCR");

                SendSupportBeanTwo(env, "T", 0, 0, 0d, null);
                env.AssertListenerNotInvoked("createOne");
                SendSupportBeanTwo(env, "T", 0, 0, -1d, 1d);
                env.AssertPropsOld("createOne", fields, new object[] { "E1" });

                stmtTextDelete =
                    "@name('d1') on SupportBeanTwo as s2 delete from MyWindowCR as win where win.IntPrimitive between s2.IntPrimitiveTwo and s2.IntBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d1");
                AssertIndexCount(env, 2, "createOne", "MyWindowCR");

                SendSupportBeanTwo(env, "T", -2, 2, 0d, 0d);
                env.AssertPropsOld("createOne", fields, new object[] { "E2" });

                stmtTextDelete = "@name('d2') on SupportBeanTwo as s2 delete from MyWindowCR as win " +
                                 "where win.IntPrimitive between s2.IntPrimitiveTwo and s2.IntBoxedTwo and win.DoublePrimitive between s2.IntPrimitiveTwo and s2.IntBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d2");
                AssertIndexCount(env, 3, "createOne", "MyWindowCR");

                SendSupportBeanTwo(env, "T", -3, 3, -3d, 3d);
                env.AssertPropsOld("createOne", fields, new object[] { "E3" });

                stmtTextDelete = "@name('d3') on SupportBeanTwo as s2 delete from MyWindowCR as win " +
                                 "where win.DoublePrimitive between s2.IntPrimitiveTwo and s2.IntPrimitiveTwo and win.IntPrimitive between s2.IntPrimitiveTwo and s2.IntPrimitiveTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d3");
                AssertIndexCount(env, 4, "createOne", "MyWindowCR");

                SendSupportBeanTwo(env, "T", -4, 4, -4, 4d);
                env.AssertPropsOld("createOne", fields, new object[] { "E4" });

                stmtTextDelete =
                    "@name('d4') on SupportBeanTwo as s2 delete from MyWindowCR as win where win.IntPrimitive <= DoublePrimitiveTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d4");
                AssertIndexCount(env, 4, "createOne", "MyWindowCR");

                SendSupportBeanTwo(env, "T", 0, 0, 5, 1d);
                env.AssertPropsOld("createOne", fields, new object[] { "E5" });

                stmtTextDelete =
                    "@name('d5') on SupportBeanTwo as s2 delete from MyWindowCR as win where win.IntPrimitive not between s2.IntPrimitiveTwo and s2.IntBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d5");
                AssertIndexCount(env, 4, "createOne", "MyWindowCR");

                SendSupportBeanTwo(env, "T", 100, 200, 0, 0d);
                env.AssertPropsOld("createOne", fields, new object[] { "E6" });

                // delete
                foreach (var stmtName in deleteStatements) {
                    env.UndeployModuleContaining(stmtName);
                }

                deleteStatements.Clear();
                AssertIndexCount(env, 0, "createOne", "MyWindowCR");

                env.UndeployAll();
            }
        }

        private class InfraCoercionKeyAndRangeMultiPropIndexes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreate = "@name('createOne') @public create window MyWindowCKR#keepall as select " +
                                     "TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("createOne");

                var stmtText =
                    "insert into MyWindowCKR select TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtText, path);
                var fields = new string[] { "TheString" };

                SendSupportBean(env, "E1", 1, 10, 100d, 1000d);
                SendSupportBean(env, "E2", 2, 20, 200d, 2000d);
                SendSupportBean(env, "E3", 3, 30, 300d, 3000d);
                SendSupportBean(env, "E4", 4, 40, 400d, 4000d);
                env.ListenerReset("createOne");

                IList<string> deleteStatements = new List<string>();
                var stmtTextDelete =
                    "@name('d0') on SupportBeanTwo delete from MyWindowCKR where TheString = StringTwo and IntPrimitive between DoublePrimitiveTwo and DoubleBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d0");
                AssertIndexCount(env, 1, "createOne", "MyWindowCKR");

                SendSupportBeanTwo(env, "T", 0, 0, 1d, 200d);
                env.AssertListenerNotInvoked("createOne");
                SendSupportBeanTwo(env, "E1", 0, 0, 1d, 200d);
                env.AssertPropsOld("createOne", fields, new object[] { "E1" });

                stmtTextDelete =
                    "@name('d1') on SupportBeanTwo delete from MyWindowCKR where TheString = StringTwo and IntPrimitive = IntPrimitiveTwo and IntBoxed between DoublePrimitiveTwo and DoubleBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d1");
                AssertIndexCount(env, 2, "createOne", "MyWindowCKR");

                SendSupportBeanTwo(env, "E2", 2, 0, 19d, 21d);
                env.AssertPropsOld("createOne", fields, new object[] { "E2" });

                stmtTextDelete =
                    "@name('d2') on SupportBeanTwo delete from MyWindowCKR where IntBoxed between DoubleBoxedTwo and DoublePrimitiveTwo and IntPrimitive = IntPrimitiveTwo and TheString = StringTwo ";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d2");
                AssertIndexCount(env, 3, "createOne", "MyWindowCKR");

                SendSupportBeanTwo(env, "E3", 3, 0, 29d, 34d);
                env.AssertPropsOld("createOne", fields, new object[] { "E3" });

                stmtTextDelete =
                    "@name('d3') on SupportBeanTwo delete from MyWindowCKR where IntBoxed between IntBoxedTwo and IntBoxedTwo and IntPrimitive = IntPrimitiveTwo and TheString = StringTwo ";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d3");
                AssertIndexCount(env, 4, "createOne", "MyWindowCKR");

                SendSupportBeanTwo(env, "E4", 4, 40, 0d, null);
                env.AssertPropsOld("createOne", fields, new object[] { "E4" });

                // delete
                foreach (var stmtName in deleteStatements) {
                    env.UndeployModuleContaining(stmtName);
                }

                deleteStatements.Clear();
                AssertIndexCount(env, 0, "createOne", "MyWindowCKR");

                env.UndeployAll();
            }
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int? intBoxed,
            double doublePrimitive,
            double? doubleBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.DoublePrimitive = doublePrimitive;
            bean.DoubleBoxed = doubleBoxed;
            env.SendEventBean(bean);
        }

        private static void SendSupportBeanTwo(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int? intBoxed,
            double doublePrimitive,
            double? doubleBoxed)
        {
            var bean = new SupportBeanTwo();
            bean.StringTwo = theString;
            bean.IntPrimitiveTwo = intPrimitive;
            bean.IntBoxedTwo = intBoxed;
            bean.DoublePrimitiveTwo = doublePrimitive;
            bean.DoubleBoxedTwo = doubleBoxed;
            env.SendEventBean(bean);
        }

        private static void TryAssertionStaggered(
            RegressionEnvironment env,
            EventRepresentationChoice outputType)
        {
            var fieldsOne = new string[] { "a1", "b1" };
            var fieldsTwo = new string[] { "a2", "b2" };
            var path = new RegressionPath();

            // create window one
            var stmtTextCreateOne = outputType.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedSTAG)) +
                                    "@name('createOne') @public create window MyWindowSTAG#keepall as select TheString as a1, IntPrimitive as b1 from SupportBean";
            env.CompileDeploy(stmtTextCreateOne, path).AddListener("createOne");
            ClassicAssert.AreEqual(0, GetCount(env, "createOne", "MyWindowSTAG"));
            env.AssertStatement(
                "createOne",
                statement => ClassicAssert.IsTrue(outputType.MatchesClass(statement.EventType.UnderlyingType)));

            // create window two
            var stmtTextCreateTwo = outputType.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedSTAGTwo)) +
                                    " @name('createTwo') @public create window MyWindowSTAGTwo#keepall as select TheString as a2, IntPrimitive as b2 from SupportBean";
            env.CompileDeploy(stmtTextCreateTwo, path).AddListener("createTwo");
            ClassicAssert.AreEqual(0, GetCount(env, "createTwo", "MyWindowSTAGTwo"));
            env.AssertStatement(
                "createTwo",
                statement => ClassicAssert.IsTrue(outputType.MatchesClass(statement.EventType.UnderlyingType)));

            // create delete stmt
            var stmtTextDelete = "@name('delete') on MyWindowSTAG delete from MyWindowSTAGTwo where a1 = a2";
            env.CompileDeploy(stmtTextDelete, path).AddListener("delete");
            env.AssertStatement(
                "delete",
                statement => ClassicAssert.AreEqual(
                    StatementType.ON_DELETE,
                    statement.GetProperty(StatementProperty.STATEMENTTYPE)));

            // create insert into
            var stmtTextInsert =
                "@name('insert') insert into MyWindowSTAG select TheString as a1, IntPrimitive as b1 from SupportBean(IntPrimitive > 0)";
            env.CompileDeploy(stmtTextInsert, path);
            stmtTextInsert =
                "@name('insertTwo') insert into MyWindowSTAGTwo select TheString as a2, IntPrimitive as b2 from SupportBean(IntPrimitive < 0)";
            env.CompileDeploy(stmtTextInsert, path);

            SendSupportBean(env, "E1", -10);
            env.AssertPropsNew("createTwo", fieldsTwo, new object[] { "E1", -10 });
            env.AssertPropsPerRowIterator("createTwo", fieldsTwo, new object[][] { new object[] { "E1", -10 } });
            env.AssertListenerNotInvoked("createOne");
            ClassicAssert.AreEqual(1, GetCount(env, "createTwo", "MyWindowSTAGTwo"));

            SendSupportBean(env, "E2", 5);
            env.AssertPropsNew("createOne", fieldsOne, new object[] { "E2", 5 });
            env.AssertPropsPerRowIterator("createOne", fieldsOne, new object[][] { new object[] { "E2", 5 } });
            env.AssertListenerNotInvoked("createTwo");
            ClassicAssert.AreEqual(1, GetCount(env, "createOne", "MyWindowSTAG"));

            SendSupportBean(env, "E3", -1);
            env.AssertPropsNew("createTwo", fieldsTwo, new object[] { "E3", -1 });
            env.AssertPropsPerRowIterator(
                "createTwo",
                fieldsTwo,
                new object[][] { new object[] { "E1", -10 }, new object[] { "E3", -1 } });
            env.AssertListenerNotInvoked("createOne");
            ClassicAssert.AreEqual(2, GetCount(env, "createTwo", "MyWindowSTAGTwo"));

            SendSupportBean(env, "E3", 1);
            env.AssertPropsNew("createOne", fieldsOne, new object[] { "E3", 1 });
            env.AssertPropsPerRowIterator(
                "createOne",
                fieldsOne,
                new object[][] { new object[] { "E2", 5 }, new object[] { "E3", 1 } });
            env.AssertPropsOld("createTwo", fieldsTwo, new object[] { "E3", -1 });
            env.AssertPropsPerRowIterator("createTwo", fieldsTwo, new object[][] { new object[] { "E1", -10 } });
            ClassicAssert.AreEqual(2, GetCount(env, "createOne", "MyWindowSTAG"));
            ClassicAssert.AreEqual(1, GetCount(env, "createTwo", "MyWindowSTAGTwo"));

            env.UndeployModuleContaining("delete");
            env.UndeployModuleContaining("insert");
            env.UndeployModuleContaining("insertTwo");
            env.UndeployModuleContaining("createOne");
            env.UndeployModuleContaining("createTwo");
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

        private static long GetCount(
            RegressionEnvironment env,
            string statementName,
            string windowName)
        {
            return SupportInfraUtil.GetDataWindowCountNoContext(env, statementName, windowName);
        }

        private static int GetIndexCount(
            RegressionEnvironment env,
            string statementName,
            string windowName)
        {
            return SupportInfraUtil.GetIndexCountNoContext(env, true, statementName, windowName);
        }

        private static void AssertString(
            RegressionEnvironment env,
            string expected)
        {
            env.AssertEqualsNew("create", "TheString", expected);
        }

        private static void AssertCount(
            RegressionEnvironment env,
            long expected)
        {
            env.AssertEqualsNew("count", "cnt", expected);
        }

        private static void AssertIndexCount(
            RegressionEnvironment env,
            int expected,
            string statementName,
            string windowName)
        {
            env.AssertThat(() => ClassicAssert.AreEqual(expected, GetIndexCount(env, statementName, windowName)));
        }

        public class MyLocalJsonProvidedSTAG
        {
            public string a1;
            public int b1;
        }

        public class MyLocalJsonProvidedSTAGTwo
        {
            public string a2;
            public int b2;
        }
    }
} // end of namespace