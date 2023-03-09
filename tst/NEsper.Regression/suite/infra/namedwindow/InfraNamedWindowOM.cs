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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOM
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraCompile());
            execs.Add(new InfraOM());
            execs.Add(new InfraOMCreateTableSyntax());
            return execs;
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

        internal class InfraCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                string[] fields = {"key", "value"};
                var stmtTextCreate =
                    "@Name('create') create window MyWindow#keepall as select TheString as key, LongBoxed as value from " +
                    nameof(SupportBean);
                var modelCreate = env.EplToModel(stmtTextCreate);
                env.CompileDeploy(modelCreate, path).AddListener("create");
                Assert.AreEqual(
                    "@Name('create') create window MyWindow#keepall as select TheString as key, LongBoxed as value from SupportBean",
                    modelCreate.ToEPL());

                var stmtTextOnSelect = "@Name('onselect') on SupportBean_B select mywin.* from MyWindow as mywin";
                var modelOnSelect = env.EplToModel(stmtTextOnSelect);
                env.CompileDeploy(modelOnSelect, path).AddListener("onselect");

                var stmtTextInsert =
                    "@Name('insert') insert into MyWindow select TheString as key, LongBoxed as value from SupportBean";
                var modelInsert = env.EplToModel(stmtTextInsert);
                env.CompileDeploy(modelInsert, path).AddListener("insert");

                var stmtTextSelectOne =
                    "@Name('select') select irstream key, value*2 as value from MyWindow(key is not null)";
                var modelSelect = env.EplToModel(stmtTextSelectOne);
                env.CompileDeploy(modelSelect, path).AddListener("select");
                Assert.AreEqual(stmtTextSelectOne, modelSelect.ToEPL());

                // send events
                SendSupportBean(env, "E1", 10L);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 20L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10L});

                SendSupportBean(env, "E2", 20L);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 40L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20L});

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol=S1.key";
                var modelDelete = env.EplToModel(stmtTextDelete);
                env.CompileDeploy(modelDelete, path).AddListener("delete");
                Assert.AreEqual(
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol=S1.key",
                    modelDelete.ToEPL());

                // send delete event
                SendMarketBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 20L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 10L});

                // send delete event again, none deleted now
                SendMarketBean(env, "E1");
                Assert.IsFalse(env.Listener("select").IsInvoked);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                // send delete event
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 40L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 20L});

                // trigger on-select on empty window
                Assert.IsFalse(env.Listener("onselect").IsInvoked);
                env.SendEventBean(new SupportBean_B("B1"));
                Assert.IsFalse(env.Listener("onselect").IsInvoked);

                SendSupportBean(env, "E3", 30L);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 60L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 30L});

                // trigger on-select on the filled window
                env.SendEventBean(new SupportBean_B("B2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("onselect").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 30L});

                env.UndeployModuleContaining("delete");
                env.UndeployModuleContaining("onselect");
                env.UndeployModuleContaining("select");
                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("create");
            }
        }

        internal class InfraOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"key", "value"};
                var path = new RegressionPath();

                // create window object model
                var model = new EPStatementObjectModel();
                model.CreateWindow = CreateWindowClause
                    .Create("MyWindow")
                    .AddView("keepall")
                    .WithAsEventTypeName("SupportBean");
                model.SelectClause = SelectClause.Create()
                    .AddWithAsProvidedName("TheString", "key")
                    .AddWithAsProvidedName("LongBoxed", "value");

                var stmtTextCreate =
                    "create window MyWindow#keepall as select TheString as key, LongBoxed as value from SupportBean";
                Assert.AreEqual(stmtTextCreate, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("create"));
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
                Assert.AreEqual(eplSelect, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("select"));
                env.CompileDeploy(model, path).AddListener("select");

                // send events
                SendSupportBean(env, "E1", 10L);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 20L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10L});

                SendSupportBean(env, "E2", 20L);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 40L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20L});

                // create delete stmt
                model = new EPStatementObjectModel();
                model.OnExpr = OnClause.CreateOnDelete("MyWindow", "S1");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportMarketDataBean", "S0"));
                model.WhereClause = Expressions.EqProperty("S0.Symbol", "S1.key");

                var stmtTextDelete = "on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol=S1.key";
                Assert.AreEqual(stmtTextDelete, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("ondelete"));
                env.CompileDeploy(model, path).AddListener("ondelete");

                // send delete event
                SendMarketBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 20L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 10L});

                // send delete event again, none deleted now
                SendMarketBean(env, "E1");
                Assert.IsFalse(env.Listener("select").IsInvoked);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                // send delete event
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 40L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 20L});

                // On-select object model
                model = new EPStatementObjectModel();
                model.OnExpr = OnClause.CreateOnSelect("MyWindow", "S1");
                model.WhereClause = Expressions.EqProperty("S0.Id", "S1.key");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_B", "S0"));
                model.SelectClause = SelectClause.CreateStreamWildcard("S1");

                var stmtTextOnSelect = "on SupportBean_B as S0 select S1.* from MyWindow as S1 where S0.Id=S1.key";
                Assert.AreEqual(stmtTextOnSelect, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("onselect"));
                env.CompileDeploy(model, path).AddListener("onselect");

                // send some more events
                SendSupportBean(env, "E3", 30L);
                SendSupportBean(env, "E4", 40L);

                env.SendEventBean(new SupportBean_B("B1"));
                Assert.IsFalse(env.Listener("onselect").IsInvoked);

                // trigger on-select
                env.SendEventBean(new SupportBean_B("E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("onselect").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 30L});

                env.UndeployAll();
            }
        }

        internal class InfraOMCreateTableSyntax : RegressionExecution
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
                Assert.AreEqual(expected, model.ToEPL());
            }
        }
    }
} // end of namespace