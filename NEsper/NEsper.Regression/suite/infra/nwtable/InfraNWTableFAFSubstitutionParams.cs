///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableFAFSubstitutionParams : IndexBackingTableInfo
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraParameterizedQuery(true));
            execs.Add(new InfraParameterizedQuery(false));
            execs.Add(new InfraParameterizedQueryNamedParameter());
            execs.Add(new InfraParameterizedQueryInvalidUse());
            execs.Add(new InfraParameterizedQueryInvalidInsufficientValues());
            execs.Add(new InfraParameterizedQueryInvalidParametersUntyped());
            execs.Add(new InfraParameterizedQueryInvalidParametersTyped());
            return execs;
        }

        private static RegressionPath SetupInfra(
            RegressionEnvironment env,
            bool namedWindow)
        {
            var path = new RegressionPath();
            var eplCreate = namedWindow
                ? "@Name('TheInfra') create window MyInfra#keepall as select * from SupportBean"
                : "@Name('TheInfra') create table MyInfra as (TheString string primary key, IntPrimitive int primary key, LongPrimitive long)";
            env.CompileDeploy(eplCreate, path);
            var eplInsert = namedWindow
                ? "@Name('Insert') insert into MyInfra select * from SupportBean"
                : "@Name('Insert') on SupportBean sb merge MyInfra mi where mi.TheString = sb.TheString and mi.IntPrimitive=sb.IntPrimitive" +
                  " when not matched then insert select TheString, IntPrimitive, LongPrimitive";
            env.CompileDeploy(eplInsert, path);

            for (var i = 0; i < 10; i++) {
                env.SendEventBean(MakeBean("E" + i, i, i * 1000));
            }

            return path;
        }

        private static SupportBean MakeBean(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.IntBoxed = 10 + intPrimitive;
            return bean;
        }

        private static void RunParameterizedQueryWCompile(
            RegressionEnvironment env,
            RegressionPath path,
            string eplOneParam,
            IDictionary<string, object> @params,
            string[] expected)
        {
            var query = CompilePrepare(eplOneParam, path, env);
            RunParameterizedQuery(env, query, @params, expected);
        }

        private static void RunParameterizedQuery(
            RegressionEnvironment env,
            EPFireAndForgetPreparedQueryParameterized parameterizedQuery,
            object[] parameters,
            string[] expected)
        {
            for (var i = 0; i < parameters.Length; i++) {
                parameterizedQuery.SetObject(i + 1, parameters[i]);
            }

            RunAndAssertResults(env, parameterizedQuery, expected);
        }

        private static void RunParameterizedQuery(
            RegressionEnvironment env,
            EPFireAndForgetPreparedQueryParameterized parameterizedQuery,
            IDictionary<string, object> parameters,
            string[] expected)
        {
            foreach (var entry in parameters) {
                parameterizedQuery.SetObject(entry.Key, entry.Value);
            }

            RunAndAssertResults(env, parameterizedQuery, expected);
        }

        private static void RunAndAssertResults(
            RegressionEnvironment env,
            EPFireAndForgetPreparedQueryParameterized parameterizedQuery,
            string[] expected)
        {
            var result = env.Runtime.FireAndForgetService.ExecuteQuery(parameterizedQuery);
            if (expected == null) {
                Assert.AreEqual(0, result.Array.Length);
            }
            else {
                Assert.AreEqual(expected.Length, result.Array.Length);
                var resultStrings = new string[result.Array.Length];
                for (var i = 0; i < resultStrings.Length; i++) {
                    resultStrings[i] = (string) result.Array[i].Get("TheString");
                }

                EPAssertionUtil.AssertEqualsAnyOrder(expected, resultStrings);
            }
        }

        private static EPFireAndForgetPreparedQueryParameterized CompilePrepare(
            string faf,
            RegressionPath path,
            RegressionEnvironment env)
        {
            var compiled = env.CompileFAF(faf, path);
            return env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
        }

        private static void TryInvalidCompileFAF(
            RegressionEnvironment env,
            RegressionPath path,
            string faf,
            string expected)
        {
            try {
                var args = new CompilerArguments(env.Configuration);
                args.Path.AddAll(path.Compileds);
                env.Compiler.CompileQuery(faf, args);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                AssertMessage(ex, expected);
            }
        }

        private static void TryInvalidlyParameterized(
            RegressionEnvironment env,
            EPCompiled compiled,
            Consumer<EPFireAndForgetPreparedQueryParameterized> query,
            string message)
        {
            var parameterized = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
            query.Invoke(parameterized);
            try {
                env.Runtime.FireAndForgetService.ExecuteQuery(parameterized);
                Assert.Fail();
            }
            catch (EPException ex) {
                AssertMessage(ex.Message, message);
            }
        }

        private static void TryInvalidSetObject(
            RegressionEnvironment env,
            EPCompiled compiled,
            Consumer<EPFireAndForgetPreparedQueryParameterized> query,
            string message)
        {
            var parameterized = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
            try {
                query.Invoke(parameterized);
                Assert.Fail();
            }
            catch (EPException ex) {
                AssertMessage(ex.Message, message);
            }
        }

        internal class InfraParameterizedQueryNamedParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = SetupInfra(env, true);
                EPFireAndForgetPreparedQueryParameterized query;

                var eplOneParam = "select * from MyInfra where IntPrimitive = ?:p0:int";
                RunParameterizedQueryWCompile(
                    env,
                    path,
                    eplOneParam,
                    Collections.SingletonDataMap("p0", 5),
                    new[] {"E5"});

                var eplTwiceUsed = "select * from MyInfra where IntPrimitive = ?:p0:int or IntBoxed = ?:p0:int";
                RunParameterizedQueryWCompile(
                    env,
                    path,
                    eplTwiceUsed,
                    Collections.SingletonDataMap("p0", 12),
                    new[] {"E2"});

                var eplTwoParam = "select * from MyInfra where IntPrimitive = ?:p1:int and IntBoxed = ?:p0:int";
                query = CompilePrepare(eplTwoParam, path, env);
                RunParameterizedQuery(env, query, CollectionUtil.PopulateNameValueMap("p0", 13, "p1", 3), new[] {"E3"});
                RunParameterizedQuery(env, query, CollectionUtil.PopulateNameValueMap("p0", 3, "p1", 3), new string[0]);
                RunParameterizedQuery(
                    env,
                    query,
                    CollectionUtil.PopulateNameValueMap("p0", 13, "p1", 13),
                    new string[0]);

                env.UndeployAll();
            }
        }

        internal class InfraParameterizedQueryInvalidUse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow#keepall as SupportBean", path);

                // invalid mix or named and unnamed
                TryInvalidCompileFAF(
                    env,
                    path,
                    "select ? as c0,?:a as c1 from MyWindow",
                    "Inconsistent use of substitution parameters, expecting all substitutions to either all provide a name or provide no name");

                // keyword used for name
                TryInvalidCompileFAF(
                    env,
                    path,
                    "select ?:select from MyWindow",
                    "Incorrect syntax near 'select' (a reserved keyword) at line 1 column 9");

                // invalid type incompatible
                TryInvalidCompileFAF(
                    env,
                    path,
                    "select ?:p0:int as c0, ?:p0:long from MyWindow",
                    $"Substitution parameter 'p0' incompatible type assignment between types '{typeof(int?).CleanName()}' and '{typeof(long?).CleanName()}'");

                env.UndeployAll();
            }
        }

        internal class InfraParameterizedQueryInvalidInsufficientValues : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow#keepall as SupportBean", path);

                // invalid execute without prepare-params
                var compiled = env.CompileFAF("select * from MyWindow where TheString=?::string", path);
                try {
                    env.Runtime.FireAndForgetService.ExecuteQuery(compiled);
                    Assert.Fail();
                }
                catch (EPException ex) {
                    AssertMessage(
                        ex.Message,
                        "Missing values for substitution parameters, use prepare-parameterized instead");
                }

                // invalid prepare without prepare-params
                try {
                    env.Runtime.FireAndForgetService.PrepareQuery(compiled);
                    Assert.Fail();
                }
                catch (EPException ex) {
                    AssertMessage(
                        ex.Message,
                        "Missing values for substitution parameters, use prepare-parameterized instead");
                }

                // missing params
                TryInvalidlyParameterized(env, compiled, query => { }, "Missing value for substitution parameter 1");

                compiled = env.CompileFAF(
                    "select * from MyWindow where TheString=?::string and IntPrimitive=?::int",
                    path);
                TryInvalidlyParameterized(env, compiled, query => { }, "Missing value for substitution parameter 1");
                TryInvalidlyParameterized(
                    env,
                    compiled,
                    query => { query.SetObject(1, "a"); },
                    "Missing value for substitution parameter 2");

                compiled = env.CompileFAF(
                    "select * from MyWindow where TheString=?:p0:string and IntPrimitive=?:p1:int",
                    path);
                TryInvalidlyParameterized(env, compiled, query => { }, "Missing value for substitution parameter 'p0'");
                TryInvalidlyParameterized(
                    env,
                    compiled,
                    query => { query.SetObject("p0", "a"); },
                    "Missing value for substitution parameter 'p1");

                env.UndeployAll();
            }
        }

        internal class InfraParameterizedQueryInvalidParametersUntyped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow#keepall as SupportBean", path);

                var compiled = env.CompileFAF("select * from MyWindow where TheString='ABC'", path);
                TryInvalidSetObject(
                    env,
                    compiled,
                    query => query.SetObject("x", 10),
                    "The query has no substitution parameters");
                TryInvalidSetObject(
                    env,
                    compiled,
                    query => query.SetObject("1", 10),
                    "The query has no substitution parameters");

                // numbered, untyped, casted at eventService
                compiled = env.CompileFAF("select * from MyWindow where TheString=cast(?, String)", path);
                TryInvalidSetObject(
                    env,
                    compiled,
                    query => query.SetObject("x", 10),
                    "Substitution parameter names have not been provided for this query");
                TryInvalidSetObject(
                    env,
                    compiled,
                    query => query.SetObject(0, "a"),
                    "Invalid substitution parameter index, expected an index between 1 and 1");
                TryInvalidSetObject(
                    env,
                    compiled,
                    query => query.SetObject(2, "a"),
                    "Invalid substitution parameter index, expected an index between 1 and 1");

                // named, untyped, casted at eventService
                compiled = env.CompileFAF("select * from MyWindow where TheString=cast(?:p0, String)", path);
                TryInvalidSetObject(
                    env,
                    compiled,
                    query => query.SetObject("x", 10),
                    "Failed to find substitution parameter named 'x', available parameters are [p0]");
                TryInvalidSetObject(
                    env,
                    compiled,
                    query => query.SetObject(0, "a"),
                    "Substitution parameter names have been provided for this query, please set the value by name");

                env.UndeployAll();
            }
        }

        internal class InfraParameterizedQueryInvalidParametersTyped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow#keepall as SupportBean", path);
                EPCompiled compiled;

                // numbered, typed
                compiled = env.CompileFAF("select * from MyWindow where TheString=?::string", path);
                TryInvalidSetObject(
                    env,
                    compiled,
                    query => query.SetObject(1, 10),
                    "Failed to set substitution parameter 1, expected a value of type 'System.String': " +
                    typeof(string));

                // name, typed
                compiled = env.CompileFAF("select * from MyWindow where TheString=?:p0:string", path);
                TryInvalidSetObject(
                    env,
                    compiled,
                    query => query.SetObject("P0", 10),
                    "Failed to set substitution parameter 'p0', expected a value of type 'System.String': " +
                    typeof(string));

                // consistent with variables/schema/table-column the "int" becomes "Integer" as a type and there is no fail checking for null

                env.UndeployAll();
            }
        }

        internal class InfraParameterizedQuery : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraParameterizedQuery(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = SetupInfra(env, namedWindow);

                // test one parameter
                var eplOneParam = "select * from MyInfra where IntPrimitive = ?::int";
                var pqOneParam = CompilePrepare(eplOneParam, path, env);
                for (var i = 0; i < 10; i++) {
                    RunParameterizedQuery(
                        env,
                        pqOneParam,
                        new object[] {i},
                        new[] {"E" + i});
                }

                RunParameterizedQuery(
                    env,
                    pqOneParam,
                    new object[] {-1},
                    null); // not found

                // test two parameter
                var eplTwoParam = "select * from MyInfra where IntPrimitive = ?::int and LongPrimitive = ?::long";
                var pqTwoParam = CompilePrepare(eplTwoParam, path, env);
                for (var i = 0; i < 10; i++) {
                    RunParameterizedQuery(
                        env,
                        pqTwoParam,
                        new object[] {i, (long) i * 1000},
                        new[] {"E" + i});
                }

                RunParameterizedQuery(
                    env,
                    pqTwoParam,
                    new object[] {-1, 1000L},
                    null); // not found

                // test in-clause with string objects
                var eplInSimple = "select * from MyInfra where TheString in (?::string, ?::string, ?::string)";
                var pqInSimple = CompilePrepare(eplInSimple, path, env);
                RunParameterizedQuery(
                    env,
                    pqInSimple,
                    new object[] {"A", "A", "A"},
                    null); // not found
                RunParameterizedQuery(
                    env,
                    pqInSimple,
                    new object[] {"A", "E3", "A"},
                    new[] {"E3"});

                // test in-clause with string array
                var eplInArray = "select * from MyInfra where TheString in (?::string[])";
                var pqInArray = CompilePrepare(eplInArray, path, env);
                RunParameterizedQuery(
                    env,
                    pqInArray,
                    new object[] {new[] {"E3", "E6", "E8"}},
                    new[] {"E3", "E6", "E8"});

                // various combinations
                RunParameterizedQuery(
                    env,
                    CompilePrepare(
                        "select * from MyInfra where TheString in (?::string[]) and LongPrimitive = 4000",
                        path,
                        env),
                    new object[] {new[] {"E3", "E4", "E8"}},
                    new[] {"E4"});
                RunParameterizedQuery(
                    env,
                    CompilePrepare("select * from MyInfra where LongPrimitive > 8000", path, env),
                    new object[] { },
                    new[] {"E9"});
                RunParameterizedQuery(
                    env,
                    CompilePrepare("select * from MyInfra where LongPrimitive < ?::long", path, env),
                    new object[] {2000L},
                    new[] {"E0", "E1"});
                RunParameterizedQuery(
                    env,
                    CompilePrepare("select * from MyInfra where LongPrimitive between ?::int and ?::int", path, env),
                    new object[] {2000, 4000},
                    new[] {"E2", "E3", "E4"});

                env.UndeployAll();
            }
        }
    }
} // end of namespace