///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework; // assertEquals

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

        //@Hook(type=HookType.SQLCOL, hook="this is a sample and not used")
        private class EPLDatabaseOutputColumnConversion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportSQLColumnTypeConversion.Reset();

                var fields = new string[] { "myint" };
                var columnTypeName = typeof(SupportSQLColumnTypeConversion).FullName;
                var stmtText =
                    $"@name('s0') @Hook(type=HookType.SQLCOL, hook='{columnTypeName}')select * from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where myint = ${{myvariableOCC}}']";
                env.CompileDeploy(stmtText);

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(bool?), statement.EventType.GetPropertyType("myint")));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { false } });

                // assert contexts
                env.AssertThat(
                    () => {
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
                    });

                env.RuntimeSetVariable(null, "myvariableOCC", 60); // greater 50 turns true
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { true } });

                env.UndeployAll();
            }
        }

        private class EPLDatabaseInputParameterConversion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportSQLColumnTypeConversion.Reset();

                var fields = new string[] { "myint" };
                var stmtText = "@name('s0') @Hook(type=HookType.SQLCOL, hook='" +
                               typeof(SupportSQLColumnTypeConversion).FullName +
                               "')" +
                               "select * from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where myint = ${myvariableIPC}']";
                env.CompileDeploy(stmtText);

                env.RuntimeSetVariable(null, "myvariableIPC", "x60"); // greater 50 turns true
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { true } });

                env.AssertThat(
                    () => {
                        var param = SupportSQLColumnTypeConversion.ParamContexts[0];
                        Assert.AreEqual(1, param.ParameterNumber);
                        Assert.AreEqual("x60", param.ParameterValue);
                    });

                env.UndeployAll();
            }
        }

        private class EPLDatabaseOutputRowConversion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportSQLColumnTypeConversion.Reset();

                var fields = "theString,intPrimitive".SplitCsv();
                var stmtText = "@name('s0') @Hook(type=HookType.SQLROW, hook='" +
                               typeof(SupportSQLOutputRowConversion).FullName +
                               "')" +
                               "select * from sql:MyDBWithTxnIso1WithReadOnly ['select * from mytesttable where myint = ${myvariableORC}']";
                env.CompileDeploy(stmtText);

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(SupportBean), statement.EventType.UnderlyingType));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { ">10<", 99010 } });

                env.AssertThat(
                    () => {
                        var type = SupportSQLOutputRowConversion.TypeContexts[0];
                        Assert.AreEqual("MyDBWithTxnIso1WithReadOnly", type.Db);
                        Assert.AreEqual("select * from mytesttable where myint = ${myvariableORC}", type.Sql);
                        Assert.AreEqual(typeof(int?), type.Fields.Get("myint"));

                        var val = SupportSQLOutputRowConversion.ValueContexts[0];
                        Assert.AreEqual(10, val.Values.Get("myint"));
                    });

                env.RuntimeSetVariable(null, "myvariableORC", 60); // greater 50 turns true
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { ">60<", 99060 } });

                env.RuntimeSetVariable(null, "myvariableORC", 90); // greater 50 turns true
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                env.UndeployAll();
            }
        }
    }
} // end of namespace