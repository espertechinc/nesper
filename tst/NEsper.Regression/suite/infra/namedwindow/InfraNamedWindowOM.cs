///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOM
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithCompile(execs);
            WithOM(execs);
            WithOMCreateTableSyntax(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOMCreateTableSyntax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOMCreateTableSyntax());
            return execs;
        }

        public static IList<RegressionExecution> WithOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOM());
            return execs;
        }

        public static IList<RegressionExecution> WithCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCompile());
            return execs;
        }

        private class InfraCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = new string[] { "key", "value" };
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow#keepall as select TheString as key, LongBoxed as value from " +
                    nameof(SupportBean);
                var modelCreate = env.EplToModel(stmtTextCreate);
                env.CompileDeploy(modelCreate, path).AddListener("create");
                ClassicAssert.AreEqual(
                    "@name('create') @public create window MyWindow#keepall as select TheString as key, LongBoxed as value from SupportBean",
                    modelCreate.ToEPL());

                var stmtTextOnSelect = "@name('onselect') on SupportBean_B select mywin.* from MyWindow as mywin";
                var modelOnSelect = env.EplToModel(stmtTextOnSelect);
                env.CompileDeploy(modelOnSelect, path).AddListener("onselect");

                var stmtTextInsert =
                    "@name('insert') insert into MyWindow select TheString as key, LongBoxed as value from SupportBean";
                var modelInsert = env.EplToModel(stmtTextInsert);
                env.CompileDeploy(modelInsert, path).AddListener("insert");

                var stmtTextSelectOne =
                    "@name('select') select irstream key, value*2 as value from MyWindow(key is not null)";
                var modelSelect = env.EplToModel(stmtTextSelectOne);
                env.CompileDeploy(modelSelect, path).AddListener("select");
                ClassicAssert.AreEqual(stmtTextSelectOne, modelSelect.ToEPL());

                // send events
                SendSupportBean(env, "E1", 10L);
                env.AssertPropsNew("select", fields, new object[] { "E1", 20L });
                env.AssertPropsNew("create", fields, new object[] { "E1", 10L });

                SendSupportBean(env, "E2", 20L);
                env.AssertPropsNew("select", fields, new object[] { "E2", 40L });
                env.AssertPropsNew("create", fields, new object[] { "E2", 20L });

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol=s1.key";
                var modelDelete = env.EplToModel(stmtTextDelete);
                env.CompileDeploy(modelDelete, path).AddListener("delete");
                ClassicAssert.AreEqual(
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol=s1.key",
                    modelDelete.ToEPL());

                // send delete event
                SendMarketBean(env, "E1");
                env.AssertPropsOld("select", fields, new object[] { "E1", 20L });
                env.AssertPropsOld("create", fields, new object[] { "E1", 10L });

                // send delete event again, none deleted now
                SendMarketBean(env, "E1");
                env.AssertListenerNotInvoked("select");
                env.AssertListenerNotInvoked("create");

                // send delete event
                SendMarketBean(env, "E2");
                env.AssertPropsOld("select", fields, new object[] { "E2", 40L });
                env.AssertPropsOld("create", fields, new object[] { "E2", 20L });

                // trigger on-select on empty window
                env.AssertListenerNotInvoked("onselect");
                env.SendEventBean(new SupportBean_B("B1"));
                env.AssertListenerNotInvoked("onselect");

                SendSupportBean(env, "E3", 30L);
                env.AssertPropsNew("select", fields, new object[] { "E3", 60L });
                env.AssertPropsNew("create", fields, new object[] { "E3", 30L });

                // trigger on-select on the filled window
                env.SendEventBean(new SupportBean_B("B2"));
                env.AssertPropsNew("onselect", fields, new object[] { "E3", 30L });

                env.UndeployModuleContaining("delete");
                env.UndeployModuleContaining("onselect");
                env.UndeployModuleContaining("select");
                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("create");
            }
        }

        private class InfraOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window object model
                var model = new EPStatementObjectModel();
                model.Annotations = Arrays.AsList(
                    AnnotationPart.NameAnnotation("create"),
                    new AnnotationPart("public"));
                model.CreateWindow = CreateWindowClause.Create("MyWindow")
                    .AddView("keepall")
                    .WithAsEventTypeName("SupportBean");
                model.SelectClause = SelectClause.Create()
                    .AddWithAsProvidedName("TheString", "key")
                    .AddWithAsProvidedName("LongBoxed", "value");

                var stmtTextCreate =
                    "@Name('create') @public create window MyWindow#keepall as select TheString as key, LongBoxed as value from SupportBean";
                ClassicAssert.AreEqual(stmtTextCreate, model.ToEPL());
                env.CompileDeploy(model, path).AddListener("create");

                var stmtTextInsert =
                    "insert into MyWindow select TheString as key, LongBoxed as value from SupportBean";
                env.EplToModelCompileDeploy(stmtTextInsert, path);

                // Consumer statement object model
                model = new EPStatementObjectModel();
                Expression multi = Expressions.Multiply(Expressions.Property("value"), Expressions.Constant(2));
                model.SelectClause = SelectClause.Create()
                    .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
                    .Add("key")
                    .Add(multi, "value");
                model.FromClause = FromClause.Create(FilterStream.Create("MyWindow", Expressions.IsNotNull("value")));
                var eplSelect = "select irstream key, value*2 as value from MyWindow(value is not null)";
                ClassicAssert.AreEqual(eplSelect, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("select"));
                env.CompileDeploy(model, path).AddListener("select");

                // send events
                SendSupportBean(env, "E1", 10L);
                env.AssertPropsNew("select", fields, new object[] { "E1", 20L });
                env.AssertPropsNew("create", fields, new object[] { "E1", 10L });

                SendSupportBean(env, "E2", 20L);
                env.AssertPropsNew("select", fields, new object[] { "E2", 40L });
                env.AssertPropsNew("create", fields, new object[] { "E2", 20L });

                // create delete stmt
                model = new EPStatementObjectModel();
                model.OnExpr = OnClause.CreateOnDelete("MyWindow", "s1");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportMarketDataBean", "s0"));
                model.WhereClause = Expressions.EqProperty("s0.Symbol", "s1.key");

                var stmtTextDelete = "on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol=s1.key";
                ClassicAssert.AreEqual(stmtTextDelete, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("ondelete"));
                env.CompileDeploy(model, path).AddListener("ondelete");

                // send delete event
                SendMarketBean(env, "E1");
                env.AssertPropsOld("select", fields, new object[] { "E1", 20L });
                env.AssertPropsOld("create", fields, new object[] { "E1", 10L });

                // send delete event again, none deleted now
                SendMarketBean(env, "E1");
                env.AssertListenerNotInvoked("select");
                env.AssertListenerNotInvoked("create");

                // send delete event
                SendMarketBean(env, "E2");
                env.AssertPropsOld("select", fields, new object[] { "E2", 40L });
                env.AssertPropsOld("create", fields, new object[] { "E2", 20L });

                // On-select object model
                model = new EPStatementObjectModel();
                model.OnExpr = OnClause.CreateOnSelect("MyWindow", "s1");
                model.WhereClause = Expressions.EqProperty("s0.Id", "s1.key");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_B", "s0"));
                model.SelectClause = SelectClause.CreateStreamWildcard("s1");

                var stmtTextOnSelect = "on SupportBean_B as s0 select s1.* from MyWindow as s1 where s0.Id=s1.key";
                ClassicAssert.AreEqual(stmtTextOnSelect, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("onselect"));
                env.CompileDeploy(model, path).AddListener("onselect");

                // send some more events
                SendSupportBean(env, "E3", 30L);
                SendSupportBean(env, "E4", 40L);

                env.SendEventBean(new SupportBean_B("B1"));
                env.AssertListenerNotInvoked("onselect");

                // trigger on-select
                env.SendEventBean(new SupportBean_B("E3"));
                env.AssertPropsNew("onselect", fields, new object[] { "E3", 30L });

                env.UndeployAll();
            }
        }

        private class InfraOMCreateTableSyntax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expected = "create window MyWindowOM#keepall as (a1 string, a2 double, a3 int)";

                // create window object model
                var model = new EPStatementObjectModel();
                var clause = CreateWindowClause.Create("MyWindowOM").AddView("keepall");
                clause.WithColumn(new SchemaColumnDesc("a1", "string"));
                clause.WithColumn(new SchemaColumnDesc("a2", "double"));
                clause.WithColumn(new SchemaColumnDesc("a3", "int"));
                model.CreateWindow = clause;
                ClassicAssert.AreEqual(expected, model.ToEPL());
            }
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            long? longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongBoxed = longBoxed;
            env.SendEventBean(bean);
        }

        private static void SendMarketBean(
            RegressionEnvironment env,
            string symbol)
        {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            env.SendEventBean(bean);
        }
    }
} // end of namespace