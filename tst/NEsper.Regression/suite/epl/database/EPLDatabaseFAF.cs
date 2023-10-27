///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseFAF
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithHook(execs);
            WithPrepareExecutePerformance(execs);
            WithSubstitutionParam(execs);
            WithDistinct(execs);
            WithWhereClause(execs);
            WithVariable(execs);
            WithSODA(execs);
            WithSQLTextParamSubquery(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithSQLTextParamSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFSQLTextParamSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithDistinct(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFDistinct());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstitutionParam(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFSubstitutionParam());
            return execs;
        }

        public static IList<RegressionExecution> WithPrepareExecutePerformance(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFPrepareExecutePerformance());
            return execs;
        }

        public static IList<RegressionExecution> WithHook(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFHook());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseFAFSimple());
            return execs;
        }

        private class EPLDatabaseFAFSimple : RegressionExecutionFAFOnly
        {
            public override void Run(RegressionEnvironment env)
            {
                var result = env.CompileExecuteFAF(
                    "select * from sql:MyDBPlain[\"select myint from mytesttable where myint between 5 and 15\"]");
                AssertSingleRowResult(result, "myint", 10);
            }
        }

        private class EPLDatabaseFAFHook : RegressionExecutionFAFOnly
        {
            public override void Run(RegressionEnvironment env)
            {
                var queryColummTypeConversion = "@name('s0') @Hook(HookType=HookType.SQLCOL, hook='" +
                                                typeof(SupportSQLColumnTypeConversion).Name +
                                                "')" +
                                                "select * from sql:MyDBPooled ['select myint as myintTurnedBoolean from mytesttable where myint = 50']";
                var resultColType = env.CompileExecuteFAF(queryColummTypeConversion);
                AssertSingleRowResult(resultColType, "myintTurnedBoolean", true);

                var queryRowConversion = "@name('s0') @Hook(HookType=HookType.SQLROW, hook='" +
                                         typeof(SupportSQLOutputRowConversion).Name +
                                         "')" +
                                         "select * from sql:MyDBPooled ['select * from mytesttable where myint = 10']";
                env.CompileDeploy(queryRowConversion);
                var resultRowConv = env.CompileExecuteFAF(queryRowConversion);
                EPAssertionUtil.AssertPropsPerRow(
                    resultRowConv.Array,
                    new string[] { "TheString", "IntPrimitive" },
                    new object[][] { new object[] { ">10<", 99010 } });
            }
        }

        private class EPLDatabaseFAFPrepareExecutePerformance : RegressionExecutionFAFOnly
        {
            public override void Run(RegressionEnvironment env)
            {
                var compiled = env.CompileFAF(
                    "select * from sql:MyDBPooled ['select * from mytesttable where myint = 10']",
                    new RegressionPath());

                var start = PerformanceObserver.MilliTime;
                var prepared = env.Runtime.FireAndForgetService.PrepareQuery(compiled);
                try {
                    for (var i = 0; i < 1000; i++) {
                        var result = prepared.Execute();
                        Assert.AreEqual(1, result.Array.Length);
                    }
                }
                finally {
                    prepared.Close();
                }

                var delta = PerformanceObserver.MilliTime - start;
                Assert.That(delta, Is.LessThan(2000), "delta=" + delta);
            }
        }

        private class EPLDatabaseFAFSubstitutionParam : RegressionExecutionFAFOnly
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "select myvarchar as c0, ?:selectValue:int as c1 from sql:MyDBPooled ['select myvarchar from mytesttable where myint = ${?:filterValue:int}']";
                var compiled = env.CompileFAF(epl, new RegressionPath());
                var parameterized = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);

                AssertQuery(env, parameterized, 1, 10, "A");
                AssertQuery(env, parameterized, 2, 60, "F");

                parameterized.Close();
            }

            private void AssertQuery(
                RegressionEnvironment env,
                EPFireAndForgetPreparedQueryParameterized parameterized,
                int selectValue,
                int filterValue,
                string expected)
            {
                parameterized.SetObject("selectValue", selectValue);
                parameterized.SetObject("filterValue", filterValue);
                var row = env.Runtime.FireAndForgetService.ExecuteQuery(parameterized).Array[0];
                EPAssertionUtil.AssertProps(row, "c0,c1".Split(","), new object[] { expected, selectValue });
            }
        }

        private class EPLDatabaseFAFDistinct : RegressionExecutionFAFOnly
        {
            public override void Run(RegressionEnvironment env)
            {
                var sql =
                    "select myint from mytesttable where myint = 10 union all select myint from mytesttable where myint = 10";
                var epl = "select distinct myint from sql:MyDBPooled ['" + sql + "']";

                var @out = env.CompileExecuteFAF(epl).Array;
                EPAssertionUtil.AssertPropsPerRow(
                    @out,
                    new string[] { "myint" },
                    new object[][] { new object[] { 10 } });
            }
        }

        private class EPLDatabaseFAFWhereClause : RegressionExecutionFAFOnly
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "select * from sql:MyDBPooled ['select myint, myvarchar from mytesttable'] where myvarchar in ('A', 'E')";
                var @out = env.CompileExecuteFAF(epl).Array;
                EPAssertionUtil.AssertPropsPerRow(
                    @out,
                    "myint,myvarchar".Split(","),
                    new object[][] { new object[] { 10, "A" }, new object[] { 50, "E" } });
            }
        }

        private class EPLDatabaseFAFVariable : RegressionExecutionFAFOnly
        {
            public override void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create variable int myvar = 20;\n" +
                    "on SupportBean set myvar = IntPrimitive;\n",
                    path);

                var epl =
                    "@name('s0') select * from sql:MyDBPooled ['select * from mytesttable where myint = ${myvar}']";
                var compiled = env.CompileFAF(epl, path);

                var prepared = env.Runtime.FireAndForgetService.PrepareQuery(compiled);
                AssertSingleRow(prepared.Execute(), "B");

                env.SendEventBean(new SupportBean(null, 50));
                AssertSingleRow(prepared.Execute(), "E");

                env.SendEventBean(new SupportBean(null, 30));
                AssertSingleRow(prepared.Execute(), "C");

                prepared.Close();
                env.UndeployAll();
            }
        }

        private class EPLDatabaseFAFSODA : RegressionExecutionFAFOnly
        {
            public override void Run(RegressionEnvironment env)
            {
                var epl =
                    "select col as c0 from sql:MyDBPooled[\"select myvarchar as col from mytesttable where myint between 20 and 30\"]";
                var model = env.EplToModel(epl);
                Assert.AreEqual(epl, model.ToEPL());
                var rows = env.CompileExecuteFAF(model, new RegressionPath()).Array;
                EPAssertionUtil.AssertPropsPerRow(
                    rows,
                    new string[] { "c0" },
                    new object[][] { new object[] { "B" }, new object[] { "C" } });
            }
        }

        private class EPLDatabaseFAFSQLTextParamSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create window MyWindow#lastevent as SupportBean;\n" +
                    "on SupportBean merge MyWindow insert select *",
                    path);

                var epl =
                    "select * from sql:MyDBPlain['select * from mytesttable where myint = ${(select IntPrimitive from MyWindow)}']";
                var compiled = env.CompileFAF(epl, path);

                var prepared = env.Runtime.FireAndForgetService.PrepareQuery(compiled);

                SendAssert(env, prepared, 30, "C");
                SendAssert(env, prepared, 10, "A");

                env.UndeployAll();
                prepared.Close();
            }

            private void SendAssert(
                RegressionEnvironment env,
                EPFireAndForgetPreparedQuery prepared,
                int intPrimitive,
                string expected)
            {
                env.SendEventBean(new SupportBean("", intPrimitive));
                var result = prepared.Execute().Array;
                EPAssertionUtil.AssertPropsPerRow(
                    result,
                    new string[] { "myvarchar" },
                    new object[][] { new object[] { expected } });
            }
        }

        private class EPLDatabaseFAFInvalid : RegressionExecutionFAFOnly
        {
            public override void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create context MyContext partition by TheString from SupportBean", path);

                // invalid join
                var eplJoin =
                    "select * from sql:MyDBPooled['select * from mytesttable'],sql:MyDBPooled['select * from mytesttable']";
                env.TryInvalidCompileFAF(
                    path,
                    eplJoin,
                    "Join between SQL query results in fire-and-forget is not supported");

                // invalid join
                var eplContext = "context MyContext select * from sql:MyDBPooled['select * from mytesttable']";
                env.TryInvalidCompileFAF(
                    path,
                    eplContext,
                    "Context specification for SQL queries in fire-and-forget is not supported");

                // invalid SQL
                var eplInvalidSQL = "select * from sql:MyDBPooled['select *']";
                env.TryInvalidCompileFAF(
                    path,
                    eplInvalidSQL,
                    "Error in statement 'select *', failed to obtain result metadata, consider turning off metadata interrogation via configuration, please check the statement, reason: No tables used");

                // closed called before execute
                var eplSimple = "select * from sql:MyDBPooled['select * from mytesttable']";
                var compiled = env.CompileFAF(eplSimple, path);
                var prepared = env.Runtime.FireAndForgetService.PrepareQuery(compiled);
                prepared.Close();
                try {
                    prepared.Execute();
                    Assert.Fail();
                }
                catch (EPException ex) {
                    Assert.AreEqual("Prepared fire-and-forget query is already closed", ex.Message);
                }

                env.UndeployAll();
            }
        }

        private static void AssertSingleRow(
            EPFireAndForgetQueryResult result,
            string expected)
        {
            AssertSingleRowResult(result, "myvarchar", expected);
        }

        private static void AssertSingleRowResult(
            EPFireAndForgetQueryResult result,
            string columnName,
            object expected)
        {
            EPAssertionUtil.AssertPropsPerRow(
                result.Array,
                new string[] { columnName },
                new object[][] { new object[] { expected } });
            Assert.AreEqual(expected.GetType(), result.Array[0].EventType.GetPropertyType(columnName));
        }
    }
} // end of namespace