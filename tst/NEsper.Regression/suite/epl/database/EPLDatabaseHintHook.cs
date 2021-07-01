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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseHintHook
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOutputColumnConversion(execs);
            WithInputParameterConversion(execs);
            WithOutputRowConversion(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOutputRowConversion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseOutputRowConversion());
            return execs;
        }

        public static IList<RegressionExecution> WithInputParameterConversion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInputParameterConversion());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputColumnConversion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseOutputColumnConversion());
            return execs;
        }

        //@Hook(HookType=HookType.SQLCOL, Hook="this is a sample and not used")
        internal class EPLDatabaseOutputColumnConversion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportSQLColumnTypeConversion.Reset();

                string[] fields = {"myint"};
                var hookType = typeof(SupportSQLColumnTypeConversion).FullName;
                var stmtText =
                    $"@Name('s0') @Hook(HookType=HookType.SQLCOL, Hook='{hookType}') select * from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where myint = ${{myvariableOCC}}']";
                env.CompileDeploy(stmtText);

                Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("myint"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {false}});

                // assert contexts
                var type = SupportSQLColumnTypeConversion.TypeContexts[0];
                Assert.AreEqual("System.Int32", type.ColumnSqlType);
                Assert.AreEqual("MyDBWithTxnIso1WithReadOnly", type.Db);
                Assert.AreEqual("select myint from mytesttable where myint = ${myvariableOCC}", type.Sql);
                Assert.AreEqual("myint", type.ColumnName);
                Assert.AreEqual(1, type.ColumnNumber);
                Assert.AreEqual(typeof(int?), type.ColumnClassType);

                var val = SupportSQLColumnTypeConversion.ValueContexts[0];
                Assert.AreEqual(10, val.ColumnValue);
                Assert.AreEqual("myint", val.ColumnName);
                Assert.AreEqual(1, val.ColumnNumber);

                env.Runtime.VariableService.SetVariableValue(null, "myvariableOCC", 60); // greater 50 turns true
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {true}});

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseInputParameterConversion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportSQLColumnTypeConversion.Reset();

                string[] fields = {"myint"};
                var hookType = typeof(SupportSQLColumnTypeConversion).FullName;
                var stmtText =
                    $"@Name('s0') @Hook(HookType=HookType.SQLCOL, Hook='{hookType}')select * from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where myint = ${{myvariableIPC}}']";
                env.CompileDeploy(stmtText);

                env.Runtime.VariableService.SetVariableValue(null, "myvariableIPC", "x60"); // greater 50 turns true
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {true}});

                var param = SupportSQLColumnTypeConversion.ParamContexts[0];
                Assert.AreEqual(1, param.ParameterNumber);
                Assert.AreEqual("x60", param.ParameterValue);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseOutputRowConversion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportSQLColumnTypeConversion.Reset();

                var fields = new [] { "TheString","IntPrimitive" };
                var hookType = typeof(SupportSQLOutputRowConversion).FullName;
                var stmtText =
                    $"@Name('s0') @Hook(HookType=HookType.SQLROW, Hook='{hookType}')select * from sql:MyDBWithTxnIso1WithReadOnly ['select * from mytesttable where myint = ${{myvariableORC}}']";
                env.CompileDeploy(stmtText);

                Assert.AreEqual(typeof(SupportBean), env.Statement("s0").EventType.UnderlyingType);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {">10<", 99010}});

                var type = SupportSQLOutputRowConversion.TypeContexts[0];
                Assert.AreEqual("MyDBWithTxnIso1WithReadOnly", type.Db);
                Assert.AreEqual("select * from mytesttable where myint = ${myvariableORC}", type.Sql);
                Assert.AreEqual(typeof(int), type.Fields.Get("myint"));

                var val = SupportSQLOutputRowConversion.ValueContexts[0];
                Assert.AreEqual(10, val.Values.Get("myint"));

                env.Runtime.VariableService.SetVariableValue(null, "myvariableORC", 60); // greater 50 turns true
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {">60<", 99060}});

                env.Runtime.VariableService.SetVariableValue(null, "myvariableORC", 90); // greater 50 turns true
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                env.UndeployAll();
            }
        }
    }
} // end of namespace