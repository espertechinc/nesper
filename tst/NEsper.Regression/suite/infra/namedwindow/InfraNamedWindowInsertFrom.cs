///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowInsertFrom
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithCreateNamedAfterNamed(execs);
            WithInsertWhereTypeAndFilter(execs);
            WithInsertWhereOMStaggered(execs);
            WithInvalid(execs);
            WithVariantStream(execs);
            WithNamedWindowInsertLenientPropCount(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowInsertLenientPropCount(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowInsertLenientPropCount(EventRepresentationChoice.MAP));
            execs.Add(new InfraNamedWindowInsertLenientPropCount(EventRepresentationChoice.OBJECTARRAY));
            return execs;
        }

        public static IList<RegressionExecution> WithVariantStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraVariantStream());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertWhereOMStaggered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertWhereOMStaggered());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertWhereTypeAndFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertWhereTypeAndFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithCreateNamedAfterNamed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCreateNamedAfterNamed());
            return execs;
        }

        private class InfraNamedWindowInsertLenientPropCount : RegressionExecution
        {
            private readonly EventRepresentationChoice rep;

            public InfraNamedWindowInsertLenientPropCount(EventRepresentationChoice rep)
            {
                this.rep = rep;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create " +
                    rep.GetName() +
                    " schema MyTwoColEvent(c0 string, c1 int);\n" +
                    "@public @name('window') create window MyWindow#keepall as MyTwoColEvent;\n" +
                    "insert into MyWindow select TheString as c0 from SupportBean;\n" +
                    "insert into MyWindow select Id as c1 from SupportBean_S0;\n";
                env.CompileDeploy(epl, path);
                var fields = "c0,c1".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsPerRowIterator("window", fields, new object[][] { new object[] { "E1", null } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(10));
                env.AssertPropsPerRowIterator(
                    "window",
                    fields,
                    new object[][] { new object[] { "E1", null }, new object[] { null, 10 } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "rep=" +
                       rep +
                       '}';
            }
        }

        private class InfraCreateNamedAfterNamed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('windowOne') create window MyWindow#keepall as SupportBean;\n" +
                          "@name('windowTwo')create window MyWindowTwo#keepall as MyWindow;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "@name('selectOne') select TheString from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("selectOne").AddListener("windowOne");

                env.SendEventBean(new SupportBean("E1", 1));
                var fields = new string[] { "TheString" };
                env.AssertPropsNew("windowOne", fields, new object[] { "E1" });
                env.AssertPropsNew("selectOne", fields, new object[] { "E1" });

                env.UndeployAll();
            }
        }

        private class InfraInsertWhereTypeAndFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };
                var path = new RegressionPath();

                var epl = "@name('window') @public create window MyWindowIWT#keepall as SupportBean;\n" +
                          "insert into MyWindowIWT select * from SupportBean(IntPrimitive > 0);\n";
                env.CompileDeploy(epl, path).AddListener("window");

                env.Milestone(0);

                // populate some data
                env.AssertRuntime(runtime => Assert.AreEqual(0, GetCount(env, path, "window", "MyWindowIWT")));
                env.SendEventBean(new SupportBean("A1", 1));
                env.AssertRuntime(runtime => Assert.AreEqual(1, GetCount(env, path, "window", "MyWindowIWT")));
                env.SendEventBean(new SupportBean("B2", 1));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("C3", 1));
                env.SendEventBean(new SupportBean("A4", 4));
                env.SendEventBean(new SupportBean("C5", 4));
                env.AssertRuntime(runtime => Assert.AreEqual(5, GetCount(env, path, "window", "MyWindowIWT")));
                env.ListenerReset("window");

                env.Milestone(2);

                // create window with keep-all
                var stmtTextCreateTwo =
                    "@name('windowTwo') @public create window MyWindowTwo#keepall as MyWindowIWT insert";
                env.CompileDeploy(stmtTextCreateTwo, path).AddListener("windowTwo");
                env.AssertIterator(
                    "windowTwo",
                    iterator => EPAssertionUtil.AssertPropsPerRow(
                        iterator,
                        fields,
                        new object[][] {
                            new object[] { "A1" }, new object[] { "B2" }, new object[] { "C3" }, new object[] { "A4" },
                            new object[] { "C5" }
                        }));
                env.AssertListenerNotInvoked("windowTwo");
                env.AssertRuntime(runtime => Assert.AreEqual(5, GetCount(env, path, "windowTwo", "MyWindowTwo")));
                env.AssertStatement(
                    "windowTwo",
                    statement => {
                        Assert.AreEqual(
                            StatementType.CREATE_WINDOW,
                            statement.GetProperty(StatementProperty.STATEMENTTYPE));
                        Assert.AreEqual("MyWindowTwo", statement.GetProperty(StatementProperty.CREATEOBJECTNAME));
                    });

                // create window with keep-all and filter
                var stmtTextCreateThree =
                    "@name('windowThree') @public create window MyWindowThree#keepall as MyWindowIWT insert where TheString like 'A%'";
                env.CompileDeploy(stmtTextCreateThree, path).AddListener("windowThree");
                env.AssertPropsPerRowIterator(
                    "windowThree",
                    fields,
                    new object[][] { new object[] { "A1" }, new object[] { "A4" } });
                env.AssertListenerNotInvoked("windowThree");

                env.Milestone(3);

                env.AssertRuntime(runtime => Assert.AreEqual(2, GetCount(env, path, "windowThree", "MyWindowThree")));

                // create window with last-per-id
                var stmtTextCreateFour =
                    "@name('windowFour') @public create window MyWindowFour#unique(IntPrimitive) as MyWindowIWT insert";
                env.CompileDeploy(stmtTextCreateFour, path).AddListener("windowFour");
                env.AssertPropsPerRowIterator(
                    "windowFour",
                    fields,
                    new object[][] { new object[] { "C3" }, new object[] { "C5" } });
                env.AssertListenerNotInvoked("windowFour");

                env.Milestone(4);

                env.AssertRuntime(runtime => Assert.AreEqual(2, GetCount(env, path, "windowFour", "MyWindowFour")));

                env.CompileDeploy("insert into MyWindowIWT select * from SupportBean(TheString like 'A%')", path);
                env.CompileDeploy("insert into MyWindowTwo select * from SupportBean(TheString like 'B%')", path);
                env.CompileDeploy("insert into MyWindowThree select * from SupportBean(TheString like 'C%')", path);
                env.CompileDeploy("insert into MyWindowFour select * from SupportBean(TheString like 'D%')", path);
                env.AssertListenerNotInvoked("window");
                env.AssertListenerNotInvoked("windowTwo");
                env.AssertListenerNotInvoked("windowThree");
                env.AssertListenerNotInvoked("windowFour");

                env.SendEventBean(new SupportBean("B9", -9));
                env.AssertListener(
                    "windowTwo",
                    listener => {
                        var received = listener.AssertOneGetNewAndReset();
                        EPAssertionUtil.AssertProps(received, fields, new object[] { "B9" });
                        if (!env.IsHA) {
                            Assert.AreSame(env.Statement("windowTwo").EventType, received.EventType);
                        }
                    });

                env.AssertRuntime(runtime => Assert.AreEqual(6, GetCount(env, path, "windowTwo", "MyWindowTwo")));
                env.AssertListenerNotInvoked("window");
                env.AssertListenerNotInvoked("windowThree");
                env.AssertListenerNotInvoked("windowFour");

                env.Milestone(5);

                env.SendEventBean(new SupportBean("A8", -8));
                env.AssertListener(
                    "window",
                    listener => {
                        var received = listener.AssertOneGetNewAndReset();
                        EPAssertionUtil.AssertProps(received, fields, new object[] { "A8" });
                        Assert.AreSame(env.Statement("window").EventType, received.EventType);
                    });
                env.AssertListenerNotInvoked("windowTwo");
                env.AssertListenerNotInvoked("windowThree");
                env.AssertListenerNotInvoked("windowFour");

                env.Milestone(6);

                env.SendEventBean(new SupportBean("C7", -7));
                env.AssertListener(
                    "windowThree",
                    listener => {
                        var received = listener.AssertOneGetNewAndReset();
                        EPAssertionUtil.AssertProps(received, fields, new object[] { "C7" });
                        if (!env.IsHA) {
                            Assert.AreSame(env.GetEnumerator("windowThree").Advance().EventType, received.EventType);
                        }
                    });
                env.AssertListenerNotInvoked("window");
                env.AssertListenerNotInvoked("window");
                env.AssertListenerNotInvoked("windowTwo");
                env.AssertListenerNotInvoked("windowFour");

                env.SendEventBean(new SupportBean("D6", -6));
                env.AssertListener(
                    "windowFour",
                    listener => {
                        var received = listener.AssertOneGetNewAndReset();
                        EPAssertionUtil.AssertProps(received, fields, new object[] { "D6" });
                        if (!env.IsHA) {
                            Assert.AreSame(env.Statement("windowFour").EventType, received.EventType);
                        }
                    });
                env.AssertListenerNotInvoked("window");
                env.AssertListenerNotInvoked("windowTwo");
                env.AssertListenerNotInvoked("windowThree");

                env.UndeployAll();
            }
        }

        private class InfraInsertWhereOMStaggered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionInsertWhereOMStaggered(env, rep);
                }
            }

            private void TryAssertionInsertWhereOMStaggered(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum)
            {
                var path = new RegressionPath();
                var stmtTextCreateOne =
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyWindowIWOM)) +
                    " @name('window') @public create window MyWindowIWOM#keepall as select a, b from MyMapAB";
                env.CompileDeploy(stmtTextCreateOne, path).AddListener("window");
                env.AssertStatement(
                    "window",
                    statement =>
                        Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType)));

                // create insert into
                var stmtTextInsertOne = "@public insert into MyWindowIWOM select a, b from MyMapAB";
                env.CompileDeploy(stmtTextInsertOne, path);

                // populate some data
                env.SendEventMap(
                    BuildMap(new object[][] { new object[] { "a", "E1" }, new object[] { "b", 2 } }),
                    "MyMapAB");
                env.SendEventMap(
                    BuildMap(new object[][] { new object[] { "a", "E2" }, new object[] { "b", 10 } }),
                    "MyMapAB");
                env.SendEventMap(
                    BuildMap(new object[][] { new object[] { "a", "E3" }, new object[] { "b", 10 } }),
                    "MyMapAB");

                // create window with keep-all using OM
                var model = new EPStatementObjectModel();
                model.Annotations = new List<AnnotationPart>(2);
                eventRepresentationEnum.AddAnnotationForNonMap(model);
                model.Annotations.Add(new AnnotationPart("public"));
                Expression where = Expressions.Eq("b", 10);
                model.CreateWindow =
                    CreateWindowClause.Create("MyWindowIWOMTwo", View.Create("keepall"))
                        .WithInsert(true)
                        .WithInsertWhereClause(where)
                        .WithAsEventTypeName("MyWindowIWOM");
                model.SelectClause = SelectClause.CreateWildcard();
                var text = eventRepresentationEnum.GetAnnotationTextForNonMap() +
                           " @public create window MyWindowIWOMTwo#keepall as select * from MyWindowIWOM insert where b=10";
                Assert.AreEqual(text.Trim(), model.ToEPL());

                var modelTwo = env.EplToModel(text);
                Assert.AreEqual(text.Trim(), modelTwo.ToEPL());
                modelTwo.Annotations = Arrays.AsList(
                    AnnotationPart.NameAnnotation("windowTwo"),
                    new AnnotationPart("public"));
                env.CompileDeploy(modelTwo, path).AddListener("windowTwo");
                env.AssertPropsPerRowIterator(
                    "windowTwo",
                    "a,b".SplitCsv(),
                    new object[][] { new object[] { "E2", 10 }, new object[] { "E3", 10 } });

                // test select individual fields and from an insert-from named window
                env.CompileDeploy(
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(
                        typeof(MyLocalJsonProvidedMyWindowIWOMThree)) +
                    " @name('windowThree') create window MyWindowIWOMThree#keepall as select a from MyWindowIWOMTwo insert where a = 'E2'",
                    path);
                env.AssertPropsPerRowIterator("windowThree", "a".SplitCsv(), new object[][] { new object[] { "E2" } });

                env.UndeployAll();
            }
        }

        private class InfraVariantStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindowVS#keepall as select * from VarStream", path);
                env.CompileDeploy("@name('window') @public create window MyWindowVSTwo#keepall as MyWindowVS", path);

                env.CompileDeploy("insert into VarStream select * from SupportBean_A", path);
                env.CompileDeploy("insert into VarStream select * from SupportBean_B", path);
                env.CompileDeploy("insert into MyWindowVSTwo select * from VarStream", path);
                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                env.AssertIterator(
                    "window",
                    iterator => {
                        var events = EPAssertionUtil.EnumeratorToArray(iterator);
                        Assert.AreEqual("A1", events[0].Get("Id?"));
                    });
                env.AssertPropsPerRowIterator(
                    "window",
                    "Id?".SplitCsv(),
                    new object[][] { new object[] { "A1" }, new object[] { "B1" } });

                env.UndeployAll();
            }
        }

        private class InfraInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreateOne = "@public create window MyWindowINV#keepall as SupportBean";
                env.CompileDeploy(stmtTextCreateOne, path);

                env.TryInvalidCompile(
                    "create window testWindow3#keepall as SupportBean insert",
                    "A named window by name 'SupportBean' could not be located, the insert-keyword requires an existing named window");
                env.TryInvalidCompile(
                    "create window testWindow3#keepall as select * from SupportBean insert where (IntPrimitive = 10)",
                    "A named window by name 'SupportBean' could not be located, the insert-keyword requires an existing named window");
                env.TryInvalidCompile(
                    path,
                    "create window MyWindowTwo#keepall as MyWindowINV insert where (select IntPrimitive from SupportBean#lastevent)",
                    "Create window where-clause may not have a subselect");
                env.TryInvalidCompile(
                    path,
                    "create window MyWindowTwo#keepall as MyWindowINV insert where sum(IntPrimitive) > 2",
                    "Create window where-clause may not have an aggregation function");
                env.TryInvalidCompile(
                    path,
                    "create window MyWindowTwo#keepall as MyWindowINV insert where prev(1, IntPrimitive) = 1",
                    "Create window where-clause may not have a function that requires view resources (prior, prev)");

                env.UndeployAll();
            }
        }

        private static long GetCount(
            RegressionEnvironment env,
            RegressionPath path,
            string statementName,
            string windowName)
        {
            if (env.IsHA) {
                return env.CompileExecuteFAF("select count(*) as cnt from " + windowName, path)
                    .Array[0]
                    .Get("cnt")
                    .AsInt64();
            }

            return SupportInfraUtil.GetDataWindowCountNoContext(env, statementName, windowName);
        }

        public class MyLocalJsonProvidedMyWindowIWOM
        {
            public string a;
            public int b;
        }

        public class MyLocalJsonProvidedMyWindowIWOMThree
        {
            public string a;
        }
    }
} // end of namespace