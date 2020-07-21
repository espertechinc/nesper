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
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowInsertFrom
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraCreateNamedAfterNamed());
            execs.Add(new InfraInsertWhereTypeAndFilter());
            execs.Add(new InfraInsertWhereOMStaggered());
            execs.Add(new InfraInvalid());
            execs.Add(new InfraVariantStream());
            return execs;
        }

        private static long GetCount(
            RegressionEnvironment env,
            RegressionPath path,
            string statementName,
            string windowName)
        {
            if (env.IsHA) {
                return env
                    .CompileExecuteFAF("select count(*) as cnt from " + windowName, path)
                    .Array[0]
                    .Get("cnt")
                    .AsInt64();
            }

            return SupportInfraUtil.GetDataWindowCountNoContext(env, statementName, windowName);
        }

        internal class InfraCreateNamedAfterNamed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('windowOne') create window MyWindow#keepall as SupportBean;\n" +
                          "@name('windowTwo')create window MyWindowTwo#keepall as MyWindow;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "@name('selectOne') select TheString from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("selectOne").AddListener("windowOne");

                env.SendEventBean(new SupportBean("E1", 1));
                string[] fields = {"TheString"};
                EPAssertionUtil.AssertProps(
                    env.Listener("windowOne").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("selectOne").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.UndeployAll();
            }
        }

        internal class InfraInsertWhereTypeAndFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};
                var path = new RegressionPath();

                var epl = "@name('window') create window MyWindowIWT#keepall as SupportBean;\n" +
                          "insert into MyWindowIWT select * from SupportBean(IntPrimitive > 0);\n";
                env.CompileDeploy(epl, path).AddListener("window");

                env.Milestone(0);

                // populate some data
                Assert.AreEqual(0, GetCount(env, path, "window", "MyWindowIWT"));
                env.SendEventBean(new SupportBean("A1", 1));
                Assert.AreEqual(1, GetCount(env, path, "window", "MyWindowIWT"));
                env.SendEventBean(new SupportBean("B2", 1));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("C3", 1));
                env.SendEventBean(new SupportBean("A4", 4));
                env.SendEventBean(new SupportBean("C5", 4));
                Assert.AreEqual(5, GetCount(env, path, "window", "MyWindowIWT"));
                env.Listener("window").Reset();

                env.Milestone(2);

                // create window with keep-all
                var stmtTextCreateTwo = "@name('windowTwo') create window MyWindowTwo#keepall as MyWindowIWT insert";
                env.CompileDeploy(stmtTextCreateTwo, path).AddListener("windowTwo");
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("windowTwo"),
                    fields,
                    new[] {
                        new object[] {"A1"}, new object[] {"B2"}, new object[] {"C3"}, new object[] {"A4"},
                        new object[] {"C5"}
                    });
                var eventTypeTwo = env.GetEnumerator("windowTwo").Advance().EventType;
                Assert.IsFalse(env.Listener("windowTwo").IsInvoked);
                Assert.AreEqual(5, GetCount(env, path, "windowTwo", "MyWindowTwo"));
                Assert.AreEqual(
                    StatementType.CREATE_WINDOW,
                    env.Statement("windowTwo").GetProperty(StatementProperty.STATEMENTTYPE));

                // create window with keep-all and filter
                var stmtTextCreateThree =
                    "@name('windowThree') create window MyWindowThree#keepall as MyWindowIWT insert where TheString like 'A%'";
                env.CompileDeploy(stmtTextCreateThree, path).AddListener("windowThree");
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("windowThree"),
                    fields,
                    new[] {new object[] {"A1"}, new object[] {"A4"}});
                var eventTypeThree = env.GetEnumerator("windowThree").Advance().EventType;
                Assert.IsFalse(env.Listener("windowThree").IsInvoked);

                env.Milestone(3);

                Assert.AreEqual(2, GetCount(env, path, "windowThree", "MyWindowThree"));

                // create window with last-per-id
                var stmtTextCreateFour =
                    "@name('windowFour') create window MyWindowFour#unique(IntPrimitive) as MyWindowIWT insert";
                env.CompileDeploy(stmtTextCreateFour, path).AddListener("windowFour");
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("windowFour"),
                    fields,
                    new[] {new object[] {"C3"}, new object[] {"C5"}});
                var eventTypeFour = env.GetEnumerator("windowFour").Advance().EventType;
                Assert.IsFalse(env.Listener("windowFour").IsInvoked);

                env.Milestone(4);

                Assert.AreEqual(2, GetCount(env, path, "windowFour", "MyWindowFour"));

                env.CompileDeploy("insert into MyWindowIWT select * from SupportBean(TheString like 'A%')", path);
                env.CompileDeploy("insert into MyWindowTwo select * from SupportBean(TheString like 'B%')", path);
                env.CompileDeploy("insert into MyWindowThree select * from SupportBean(TheString like 'C%')", path);
                env.CompileDeploy("insert into MyWindowFour select * from SupportBean(TheString like 'D%')", path);
                Assert.IsFalse(
                    env.Listener("window").IsInvoked ||
                    env.Listener("windowTwo").IsInvoked ||
                    env.Listener("windowThree").IsInvoked ||
                    env.Listener("windowFour").IsInvoked);

                env.SendEventBean(new SupportBean("B9", -9));
                var received = env.Listener("windowTwo").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new object[] {"B9"});
                if (!env.IsHA) {
                    Assert.AreSame(eventTypeTwo, received.EventType);
                }

                Assert.IsFalse(
                    env.Listener("window").IsInvoked ||
                    env.Listener("windowThree").IsInvoked ||
                    env.Listener("windowFour").IsInvoked);
                Assert.AreEqual(6, GetCount(env, path, "windowTwo", "MyWindowTwo"));

                env.Milestone(5);

                env.SendEventBean(new SupportBean("A8", -8));
                received = env.Listener("window").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new object[] {"A8"});
                Assert.AreSame(env.Statement("window").EventType, received.EventType);
                Assert.IsFalse(
                    env.Listener("windowTwo").IsInvoked ||
                    env.Listener("windowThree").IsInvoked ||
                    env.Listener("windowFour").IsInvoked);

                env.Milestone(6);

                env.SendEventBean(new SupportBean("C7", -7));
                received = env.Listener("windowThree").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new object[] {"C7"});
                if (!env.IsHA) {
                    Assert.AreSame(eventTypeThree, received.EventType);
                }

                Assert.IsFalse(
                    env.Listener("windowTwo").IsInvoked ||
                    env.Listener("window").IsInvoked ||
                    env.Listener("windowFour").IsInvoked);

                env.SendEventBean(new SupportBean("D6", -6));
                received = env.Listener("windowFour").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new object[] {"D6"});
                if (!env.IsHA) {
                    Assert.AreSame(eventTypeFour, received.EventType);
                }

                Assert.IsFalse(
                    env.Listener("windowTwo").IsInvoked ||
                    env.Listener("window").IsInvoked ||
                    env.Listener("windowThree").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraInsertWhereOMStaggered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionInsertWhereOMStaggered(env, rep);
                }
            }

            private void TryAssertionInsertWhereOMStaggered(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum)
            {
                var path = new RegressionPath();
                var stmtTextCreateOne = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyWindowIWOM>() +
                                        " @Name('window') create window MyWindowIWOM#keepall as select a, b from MyMapAB";
                env.CompileDeploy(stmtTextCreateOne, path);
                Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("window").EventType.UnderlyingType));
                env.AddListener("window");

                // create insert into
                var stmtTextInsertOne = "insert into MyWindowIWOM select a, b from MyMapAB";
                env.CompileDeploy(stmtTextInsertOne, path);

                // populate some data
                env.SendEventMap(BuildMap(new[] {new object[] {"a", "E1"}, new object[] {"b", 2}}), "MyMapAB");
                env.SendEventMap(BuildMap(new[] {new object[] {"a", "E2"}, new object[] {"b", 10}}), "MyMapAB");
                env.SendEventMap(BuildMap(new[] {new object[] {"a", "E3"}, new object[] {"b", 10}}), "MyMapAB");

                // create window with keep-all using OM
                var model = new EPStatementObjectModel();
                eventRepresentationEnum.AddAnnotationForNonMap(model);
                Expression where = Expressions.Eq("b", 10);
                model.CreateWindow =
                    CreateWindowClause.Create("MyWindowIWOMTwo", View.Create("keepall"))
                        .WithInsert(true)
                        .WithInsertWhereClause(where)
                        .WithAsEventTypeName("MyWindowIWOM");
                model.SelectClause = SelectClause.CreateWildcard();
                var text = eventRepresentationEnum.GetAnnotationTextForNonMap() +
                           " create window MyWindowIWOMTwo#keepall as select * from MyWindowIWOM insert where b=10";
                Assert.AreEqual(text.Trim(), model.ToEPL());

                var modelTwo = env.EplToModel(text);
                Assert.AreEqual(text.Trim(), modelTwo.ToEPL());
                modelTwo.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("windowTwo"));
                env.CompileDeploy(modelTwo, path).AddListener("windowTwo");

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("windowTwo"),
                    new[] {"a", "b"},
                    new[] {new object[] {"E2", 10}, new object[] {"E3", 10}});

                // test select individual fields and from an insert-from named window
                env.CompileDeploy(
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyWindowIWOMThree>() +
                    " @Name('windowThree') create window MyWindowIWOMThree#keepall as select a from MyWindowIWOMTwo insert where a = 'E2'",
                    path);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("windowThree"),
                    new[] {"a"},
                    new[] {new object[] {"E2"}});

                env.UndeployAll();
            }
        }

        internal class InfraVariantStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindowVS#keepall as select * from VarStream", path);
                env.CompileDeploy("@name('window') create window MyWindowVSTwo#keepall as MyWindowVS", path);

                env.CompileDeploy("insert into VarStream select * from SupportBean_A", path);
                env.CompileDeploy("insert into VarStream select * from SupportBean_B", path);
                env.CompileDeploy("insert into MyWindowVSTwo select * from VarStream", path);
                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("window"));
                Assert.AreEqual("A1", events[0].Get("Id?"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("window"),
                    new[] {"Id?"},
                    new[] {new object[] {"A1"}, new object[] {"B1"}});

                env.UndeployAll();
            }
        }

        internal class InfraInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreateOne = "create window MyWindowINV#keepall as SupportBean";
                env.CompileDeploy(stmtTextCreateOne, path);

                TryInvalidCompile(
                    env,
                    "create window testWindow3#keepall as SupportBean insert",
                    "A named window by name 'SupportBean' could not be located, the insert-keyword requires an existing named window");
                TryInvalidCompile(
                    env,
                    "create window testWindow3#keepall as select * from SupportBean insert where (IntPrimitive = 10)",
                    "A named window by name 'SupportBean' could not be located, the insert-keyword requires an existing named window");
                TryInvalidCompile(
                    env,
                    path,
                    "create window MyWindowTwo#keepall as MyWindowINV insert where (select IntPrimitive from SupportBean#lastevent)",
                    "Create window where-clause may not have a subselect");
                TryInvalidCompile(
                    env,
                    path,
                    "create window MyWindowTwo#keepall as MyWindowINV insert where sum(IntPrimitive) > 2",
                    "Create window where-clause may not have an aggregation function");
                TryInvalidCompile(
                    env,
                    path,
                    "create window MyWindowTwo#keepall as MyWindowINV insert where prev(1, IntPrimitive) = 1",
                    "Create window where-clause may not have a function that requires view resources (prior, prev)");

                env.UndeployAll();
            }
        }


        [Serializable]
        public class MyLocalJsonProvidedMyWindowIWOM
        {
            public string a;
            public int b;
        }

        [Serializable]
        public class MyLocalJsonProvidedMyWindowIWOMThree
        {
            public string a;
        }
    }
} // end of namespace