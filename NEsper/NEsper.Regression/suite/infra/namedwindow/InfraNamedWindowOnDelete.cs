///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
    public class InfraNamedWindowOnDelete
    {
        public static IList<RegressionExecution> Executions()
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

        public static IList<RegressionExecution> WithNamedWindowSilentDeleteOnDeleteMany(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowSilentDeleteOnDeleteMany());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowSilentDeleteOnDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowSilentDeleteOnDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithCoercionKeyAndRangeMultiPropIndexes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCoercionKeyAndRangeMultiPropIndexes());
            return execs;
        }

        public static IList<RegressionExecution> WithCoercionRangeMultiPropIndexes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCoercionRangeMultiPropIndexes());
            return execs;
        }

        public static IList<RegressionExecution> WithCoercionKeyMultiPropIndexes(IList<RegressionExecution> execs = null)
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
            string[] fieldsOne = {"a1", "b1"};
            string[] fieldsTwo = {"a2", "b2"};
            var path = new RegressionPath();

            // create window one
            var stmtTextCreateOne = outputType.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedSTAG>() +
                                    "@Name('createOne') create window MyWindowSTAG#keepall as select TheString as a1, IntPrimitive as b1 from SupportBean";
            env.CompileDeploy(stmtTextCreateOne, path).AddListener("createOne");
            Assert.AreEqual(0, GetCount(env, "createOne", "MyWindowSTAG"));
            Assert.IsTrue(outputType.MatchesClass(env.Statement("createOne").EventType.UnderlyingType));

            // create window two
            var stmtTextCreateTwo = outputType.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedSTAGTwo>() +
                                    " @Name('createTwo') create window MyWindowSTAGTwo#keepall as select TheString as a2, IntPrimitive as b2 from SupportBean";
            env.CompileDeploy(stmtTextCreateTwo, path).AddListener("createTwo");
            Assert.AreEqual(0, GetCount(env, "createTwo", "MyWindowSTAGTwo"));
            Assert.IsTrue(outputType.MatchesClass(env.Statement("createTwo").EventType.UnderlyingType));

            // create delete stmt
            var stmtTextDelete = "@Name('delete') on MyWindowSTAG delete from MyWindowSTAGTwo where a1 = a2";
            env.CompileDeploy(stmtTextDelete, path).AddListener("delete");
            Assert.AreEqual(
                StatementType.ON_DELETE,
                env.Statement("delete").GetProperty(StatementProperty.STATEMENTTYPE));

            // create insert into
            var stmtTextInsert =
                "@Name('insert') insert into MyWindowSTAG select TheString as a1, IntPrimitive as b1 from SupportBean(IntPrimitive > 0)";
            env.CompileDeploy(stmtTextInsert, path);
            stmtTextInsert =
                "@Name('insertTwo') insert into MyWindowSTAGTwo select TheString as a2, IntPrimitive as b2 from SupportBean(IntPrimitive < 0)";
            env.CompileDeploy(stmtTextInsert, path);

            SendSupportBean(env, "E1", -10);
            EPAssertionUtil.AssertProps(
                env.Listener("createTwo").AssertOneGetNewAndReset(),
                fieldsTwo,
                new object[] {"E1", -10});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("createTwo"),
                fieldsTwo,
                new[] {new object[] {"E1", -10}});
            Assert.IsFalse(env.Listener("createOne").IsInvoked);
            Assert.AreEqual(1, GetCount(env, "createTwo", "MyWindowSTAGTwo"));

            SendSupportBean(env, "E2", 5);
            EPAssertionUtil.AssertProps(
                env.Listener("createOne").AssertOneGetNewAndReset(),
                fieldsOne,
                new object[] {"E2", 5});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("createOne"),
                fieldsOne,
                new[] {new object[] {"E2", 5}});
            Assert.IsFalse(env.Listener("createTwo").IsInvoked);
            Assert.AreEqual(1, GetCount(env, "createOne", "MyWindowSTAG"));

            SendSupportBean(env, "E3", -1);
            EPAssertionUtil.AssertProps(
                env.Listener("createTwo").AssertOneGetNewAndReset(),
                fieldsTwo,
                new object[] {"E3", -1});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("createTwo"),
                fieldsTwo,
                new[] {new object[] {"E1", -10}, new object[] {"E3", -1}});
            Assert.IsFalse(env.Listener("createOne").IsInvoked);
            Assert.AreEqual(2, GetCount(env, "createTwo", "MyWindowSTAGTwo"));

            SendSupportBean(env, "E3", 1);
            EPAssertionUtil.AssertProps(
                env.Listener("createOne").AssertOneGetNewAndReset(),
                fieldsOne,
                new object[] {"E3", 1});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("createOne"),
                fieldsOne,
                new[] {new object[] {"E2", 5}, new object[] {"E3", 1}});
            EPAssertionUtil.AssertProps(
                env.Listener("createTwo").AssertOneGetOldAndReset(),
                fieldsTwo,
                new object[] {"E3", -1});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("createTwo"),
                fieldsTwo,
                new[] {new object[] {"E1", -10}});
            Assert.AreEqual(2, GetCount(env, "createOne", "MyWindowSTAG"));
            Assert.AreEqual(1, GetCount(env, "createTwo", "MyWindowSTAGTwo"));

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

        internal class InfraNamedWindowSilentDeleteOnDeleteMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl =
                    "@Name('create') create window MyWindow#groupwin(TheString)#length(2) as SupportBean;\n" +
                    "insert into MyWindow select * from SupportBean;\n" +
                    "@Name('delete') @hint('silent_delete') on SupportBean_S0 delete from MyWindow;\n" +
                    "@Name('count') select count(*) as cnt from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("delete").AddListener("count");

                env.SendEventBean(new SupportBean("A", 1));
                env.SendEventBean(new SupportBean("A", 2));
                env.SendEventBean(new SupportBean("B", 3));
                env.SendEventBean(new SupportBean("B", 4));

                Assert.AreEqual(4L, env.Listener("count").GetAndResetDataListsFlattened().First[3].Get("cnt"));
                env.Listener("create").Reset();

                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual(0L, env.Listener("count").AssertOneGetNewAndReset().Get("cnt"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("delete").GetAndResetLastNewData(),
                    "TheString,IntPrimitive".SplitCsv(),
                    new object[][] {
                        new object[] {"A", 1}, new object[] {"A", 2}, new object[] {"B", 3}, new object[] {"B", 4}
                    });
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraNamedWindowSilentDeleteOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl =
                    "@Name('create') create window MyWindow#length(2) as SupportBean;\n" +
                    "insert into MyWindow select * from SupportBean;\n" +
                    "@Name('delete') @hint('silent_delete') on SupportBean_S0 delete from MyWindow where P00 = TheString;\n" +
                    "@Name('count') select count(*) as cnt from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("delete").AddListener("count");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(1L, env.Listener("count").AssertOneGetNewAndReset().Get("cnt"));
                Assert.AreEqual("E1", env.Listener("create").AssertOneGetNewAndReset().Get("TheString"));

                env.SendEventBean(new SupportBean_S0(0, "E1"));
                Assert.AreEqual(0L, env.Listener("count").AssertOneGetNewAndReset().Get("cnt"));
                Assert.AreEqual("E1", env.Listener("delete").AssertOneGetNewAndReset().Get("TheString"));
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 2));
                Assert.AreEqual(1L, env.Listener("count").AssertOneGetNewAndReset().Get("cnt"));
                Assert.AreEqual("E2", env.Listener("create").AssertOneGetNewAndReset().Get("TheString"));

                env.SendEventBean(new SupportBean("E3", 3));
                Assert.AreEqual(2L, env.Listener("count").AssertOneGetNewAndReset().Get("cnt"));
                Assert.AreEqual("E3", env.Listener("create").AssertOneGetNewAndReset().Get("TheString"));

                env.SendEventBean(new SupportBean("E4", 4));
                Assert.AreEqual(2L, env.Listener("count").AssertOneGetNewAndReset().Get("cnt"));
                EPAssertionUtil.AssertProps(env.Listener("create").AssertPairGetIRAndReset(), "TheString".SplitCsv(), new object[] {"E4"}, new object[] {"E2"});

                env.SendEventBean(new SupportBean_S0(0, "E4"));
                Assert.AreEqual(1L, env.Listener("count").AssertOneGetNewAndReset().Get("cnt"));
                Assert.AreEqual("E4", env.Listener("delete").AssertOneGetNewAndReset().Get("TheString"));
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.SendEventBean(new SupportBean_S0(0, "E3"));
                Assert.AreEqual(0L, env.Listener("count").AssertOneGetNewAndReset().Get("cnt"));
                Assert.AreEqual("E3", env.Listener("delete").AssertOneGetNewAndReset().Get("TheString"));
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.SendEventBean(new SupportBean_S0(0, "EX"));
                Assert.IsFalse(env.Listener("count").IsInvoked);
                Assert.IsFalse(env.Listener("delete").IsInvoked);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraFirstUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var epl =
                    "@Name('create') create window MyWindowFU#firstunique(TheString) as select * from SupportBean;\n" +
                    "insert into MyWindowFU select * from SupportBean;\n" +
                    "@Name('delete') on SupportBean_A a delete from MyWindowFU where TheString=a.Id;\n";
                env.CompileDeploy(epl).AddListener("delete");

                env.SendEventBean(new SupportBean("A", 1));
                env.SendEventBean(new SupportBean("A", 2));

                env.SendEventBean(new SupportBean_A("A"));
                EPAssertionUtil.AssertProps(
                    env.Listener("delete").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 1});

                env.SendEventBean(new SupportBean("A", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"A", 3}});

                env.SendEventBean(new SupportBean_A("A"));
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                env.UndeployAll();
            }
        }

        internal class InfraStaggeredNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionStaggered(env, rep);
                }
            }
        }

        internal class InfraCoercionKeyMultiPropIndexes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // create window
                var stmtTextCreate = "@Name('createOne') create window MyWindowCK#keepall as select " +
                                     "TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("createOne");

                IList<string> deleteStatements = new List<string>();
                var stmtTextDelete =
                    "@Name('d1') on SupportBean(TheString='DB') as S0 delete from MyWindowCK as win where win.IntPrimitive = S0.DoubleBoxed";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d1");
                Assert.AreEqual(1, GetIndexCount(env, "createOne", "MyWindowCK"));

                stmtTextDelete =
                    "@Name('d2') on SupportBean(TheString='DP') as S0 delete from MyWindowCK as win where win.IntPrimitive = S0.DoublePrimitive";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d2");
                Assert.AreEqual(1, GetIndexCount(env, "createOne", "MyWindowCK"));

                stmtTextDelete =
                    "@Name('d3') on SupportBean(TheString='IB') as S0 delete from MyWindowCK where MyWindowCK.IntPrimitive = S0.IntBoxed";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d3");
                Assert.AreEqual(2, GetIndexCount(env, "createOne", "MyWindowCK"));

                stmtTextDelete =
                    "@Name('d4') on SupportBean(TheString='IPDP') as S0 delete from MyWindowCK as win where win.IntPrimitive = S0.IntPrimitive and win.DoublePrimitive = S0.DoublePrimitive";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d4");
                Assert.AreEqual(3, GetIndexCount(env, "createOne", "MyWindowCK"));

                stmtTextDelete =
                    "@Name('d5') on SupportBean(TheString='IPDP2') as S0 delete from MyWindowCK as win where win.DoublePrimitive = S0.DoublePrimitive and win.IntPrimitive = S0.IntPrimitive";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d5");
                Assert.AreEqual(4, GetIndexCount(env, "createOne", "MyWindowCK"));

                stmtTextDelete =
                    "@Name('d6') on SupportBean(TheString='IPDPIB') as S0 delete from MyWindowCK as win where win.DoublePrimitive = S0.DoublePrimitive and win.IntPrimitive = S0.IntPrimitive and win.IntBoxed = S0.IntBoxed";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d6");
                Assert.AreEqual(5, GetIndexCount(env, "createOne", "MyWindowCK"));

                stmtTextDelete =
                    "@Name('d7') on SupportBean(TheString='CAST') as S0 delete from MyWindowCK as win where win.IntBoxed = S0.IntPrimitive and win.DoublePrimitive = S0.DoubleBoxed and win.IntPrimitive = S0.IntBoxed";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d7");
                Assert.AreEqual(6, GetIndexCount(env, "createOne", "MyWindowCK"));

                // create insert into
                var stmtTextInsertOne =
                    "insert into MyWindowCK select TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed " +
                    "from SupportBean(TheString like 'E%')";
                env.CompileDeploy(stmtTextInsertOne, path);

                SendSupportBean(env, "E1", 1, 10, 100d, 1000d);
                SendSupportBean(env, "E2", 2, 20, 200d, 2000d);
                SendSupportBean(env, "E3", 3, 30, 300d, 3000d);
                SendSupportBean(env, "E4", 4, 40, 400d, 4000d);
                env.Listener("createOne").Reset();

                string[] fields = {"TheString"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("createOne"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});

                SendSupportBean(env, "DB", 0, 0, 0d, null);
                Assert.IsFalse(env.Listener("createOne").IsInvoked);
                SendSupportBean(env, "DB", 0, 0, 0d, 3d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("createOne"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E4"}});

                SendSupportBean(env, "DP", 0, 0, 5d, null);
                Assert.IsFalse(env.Listener("createOne").IsInvoked);
                SendSupportBean(env, "DP", 0, 0, 4d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E4"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("createOne"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                SendSupportBean(env, "IB", 0, -1, 0d, null);
                Assert.IsFalse(env.Listener("createOne").IsInvoked);
                SendSupportBean(env, "IB", 0, 1, 0d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("createOne"),
                    fields,
                    new[] {new object[] {"E2"}});

                SendSupportBean(env, "E5", 5, 50, 500d, 5000d);
                SendSupportBean(env, "E6", 6, 60, 600d, 6000d);
                SendSupportBean(env, "E7", 7, 70, 700d, 7000d);
                env.Listener("createOne").Reset();

                SendSupportBean(env, "IPDP", 5, 0, 500d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E5"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("createOne"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E6"}, new object[] {"E7"}});

                SendSupportBean(env, "IPDP2", 6, 0, 600d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E6"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("createOne"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E7"}});

                SendSupportBean(env, "IPDPIB", 7, 70, 0d, null);
                Assert.IsFalse(env.Listener("createOne").IsInvoked);
                SendSupportBean(env, "IPDPIB", 7, 70, 700d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E7"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("createOne"),
                    fields,
                    new[] {new object[] {"E2"}});

                SendSupportBean(env, "E8", 8, 80, 800d, 8000d);
                env.Listener("createOne").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("createOne"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E8"}});

                SendSupportBean(env, "CAST", 80, 8, 0, 800d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E8"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("createOne"),
                    fields,
                    new[] {new object[] {"E2"}});

                foreach (var stmtName in deleteStatements) {
                    env.UndeployModuleContaining(stmtName);
                }

                deleteStatements.Clear();

                // late delete on a filled window
                stmtTextDelete =
                    "@Name('d0') on SupportBean(TheString='LAST') as S0 delete from MyWindowCK as win where win.IntPrimitive = S0.IntPrimitive and win.DoublePrimitive = S0.DoublePrimitive";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d0");
                SendSupportBean(env, "LAST", 2, 20, 200, 2000d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("createOne"), fields, null);

                foreach (var stmtName in deleteStatements) {
                    env.UndeployModuleContaining(stmtName);
                }

                Assert.AreEqual(0, GetIndexCount(env, "createOne", "MyWindowCK"));
                env.UndeployAll();

                // test single-two-field index reuse
                path = new RegressionPath();
                env.CompileDeploy("@Name('createTwo') create window WinOne#keepall as SupportBean", path);
                env.CompileDeploy("on SupportBean_ST0 select * from WinOne where TheString = Key0", path);
                Assert.AreEqual(1, GetIndexCount(env, "createTwo", "WinOne"));

                env.CompileDeploy(
                    "on SupportBean_ST0 select * from WinOne where TheString = Key0 and IntPrimitive = P00",
                    path);
                Assert.AreEqual(2, GetIndexCount(env, "createTwo", "WinOne"));

                env.UndeployAll();
            }
        }

        internal class InfraCoercionRangeMultiPropIndexes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var stmtTextCreate = "@Name('createOne') create window MyWindowCR#keepall as select " +
                                     "TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("createOne");

                var stmtText =
                    "insert into MyWindowCR select TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtText, path);
                string[] fields = {"TheString"};

                SendSupportBean(env, "E1", 1, 10, 100d, 1000d);
                SendSupportBean(env, "E2", 2, 20, 200d, 2000d);
                SendSupportBean(env, "E3", 3, 30, 3d, 30d);
                SendSupportBean(env, "E4", 4, 40, 4d, 40d);
                SendSupportBean(env, "E5", 5, 50, 500d, 5000d);
                SendSupportBean(env, "E6", 6, 60, 600d, 6000d);
                env.Listener("createOne").Reset();

                IList<string> deleteStatements = new List<string>();
                var stmtTextDelete =
                    "@Name('d0') on SupportBeanTwo as S2 delete from MyWindowCR as win where win.IntPrimitive between S2.DoublePrimitiveTwo and S2.DoubleBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d0");
                Assert.AreEqual(1, GetIndexCount(env, "createOne", "MyWindowCR"));

                SendSupportBeanTwo(env, "T", 0, 0, 0d, null);
                Assert.IsFalse(env.Listener("createOne").IsInvoked);
                SendSupportBeanTwo(env, "T", 0, 0, -1d, 1d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1"});

                stmtTextDelete =
                    "@Name('d1') on SupportBeanTwo as S2 delete from MyWindowCR as win where win.IntPrimitive between S2.IntPrimitiveTwo and S2.IntBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d1");
                Assert.AreEqual(2, GetIndexCount(env, "createOne", "MyWindowCR"));

                SendSupportBeanTwo(env, "T", -2, 2, 0d, 0d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2"});

                stmtTextDelete = "@Name('d2') on SupportBeanTwo as S2 delete from MyWindowCR as win " +
                                 "where win.IntPrimitive between S2.IntPrimitiveTwo and S2.IntBoxedTwo and win.DoublePrimitive between S2.IntPrimitiveTwo and S2.IntBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d2");
                Assert.AreEqual(3, GetIndexCount(env, "createOne", "MyWindowCR"));

                SendSupportBeanTwo(env, "T", -3, 3, -3d, 3d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3"});

                stmtTextDelete = "@Name('d3') on SupportBeanTwo as S2 delete from MyWindowCR as win " +
                                 "where win.DoublePrimitive between S2.IntPrimitiveTwo and S2.IntPrimitiveTwo and win.IntPrimitive between S2.IntPrimitiveTwo and S2.IntPrimitiveTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d3");
                Assert.AreEqual(4, GetIndexCount(env, "createOne", "MyWindowCR"));

                SendSupportBeanTwo(env, "T", -4, 4, -4, 4d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E4"});

                stmtTextDelete =
                    "@Name('d4') on SupportBeanTwo as S2 delete from MyWindowCR as win where win.IntPrimitive <= DoublePrimitiveTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d4");
                Assert.AreEqual(4, GetIndexCount(env, "createOne", "MyWindowCR"));

                SendSupportBeanTwo(env, "T", 0, 0, 5, 1d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E5"});

                stmtTextDelete =
                    "@Name('d5') on SupportBeanTwo as S2 delete from MyWindowCR as win where win.IntPrimitive not between S2.IntPrimitiveTwo and S2.IntBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d5");
                Assert.AreEqual(4, GetIndexCount(env, "createOne", "MyWindowCR"));

                SendSupportBeanTwo(env, "T", 100, 200, 0, 0d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E6"});

                // delete
                foreach (var stmtName in deleteStatements) {
                    env.UndeployModuleContaining(stmtName);
                }

                deleteStatements.Clear();
                Assert.AreEqual(0, GetIndexCount(env, "createOne", "MyWindowCR"));

                env.UndeployAll();
            }
        }

        internal class InfraCoercionKeyAndRangeMultiPropIndexes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreate = "@Name('createOne') create window MyWindowCKR#keepall as select " +
                                     "TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("createOne");

                var stmtText =
                    "insert into MyWindowCKR select TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
                env.CompileDeploy(stmtText, path);
                string[] fields = {"TheString"};

                SendSupportBean(env, "E1", 1, 10, 100d, 1000d);
                SendSupportBean(env, "E2", 2, 20, 200d, 2000d);
                SendSupportBean(env, "E3", 3, 30, 300d, 3000d);
                SendSupportBean(env, "E4", 4, 40, 400d, 4000d);
                env.Listener("createOne").Reset();

                IList<string> deleteStatements = new List<string>();
                var stmtTextDelete =
                    "@Name('d0') on SupportBeanTwo delete from MyWindowCKR where TheString = StringTwo and IntPrimitive between DoublePrimitiveTwo and DoubleBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d0");
                Assert.AreEqual(1, GetIndexCount(env, "createOne", "MyWindowCKR"));

                SendSupportBeanTwo(env, "T", 0, 0, 1d, 200d);
                Assert.IsFalse(env.Listener("createOne").IsInvoked);
                SendSupportBeanTwo(env, "E1", 0, 0, 1d, 200d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1"});

                stmtTextDelete =
                    "@Name('d1') on SupportBeanTwo delete from MyWindowCKR where TheString = StringTwo and IntPrimitive = IntPrimitiveTwo and IntBoxed between DoublePrimitiveTwo and DoubleBoxedTwo";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d1");
                Assert.AreEqual(2, GetIndexCount(env, "createOne", "MyWindowCKR"));

                SendSupportBeanTwo(env, "E2", 2, 0, 19d, 21d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2"});

                stmtTextDelete =
                    "@Name('d2') on SupportBeanTwo delete from MyWindowCKR where IntBoxed between DoubleBoxedTwo and DoublePrimitiveTwo and IntPrimitive = IntPrimitiveTwo and TheString = StringTwo ";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d2");
                Assert.AreEqual(3, GetIndexCount(env, "createOne", "MyWindowCKR"));

                SendSupportBeanTwo(env, "E3", 3, 0, 29d, 34d);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3"});

                stmtTextDelete =
                    "@Name('d3') on SupportBeanTwo delete from MyWindowCKR where IntBoxed between IntBoxedTwo and IntBoxedTwo and IntPrimitive = IntPrimitiveTwo and TheString = StringTwo ";
                env.CompileDeploy(stmtTextDelete, path);
                deleteStatements.Add("d3");
                Assert.AreEqual(4, GetIndexCount(env, "createOne", "MyWindowCKR"));

                SendSupportBeanTwo(env, "E4", 4, 40, 0d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("createOne").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E4"});

                // delete
                foreach (var stmtName in deleteStatements) {
                    env.UndeployModuleContaining(stmtName);
                }

                deleteStatements.Clear();
                Assert.AreEqual(0, GetIndexCount(env, "createOne", "MyWindowCKR"));

                env.UndeployAll();
            }
        }

        [Serializable]
        public class MyLocalJsonProvidedSTAG
        {
            public string a1;
            public int b1;
        }

        [Serializable]
        public class MyLocalJsonProvidedSTAGTwo
        {
            public string a2;
            public int b2;
        }
    }
} // end of namespace