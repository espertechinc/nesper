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
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;

using NUnit.Framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil; // assertPropsPerRow
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // tryInvalidFAFCompile

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherFromClauseOptional
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithContext(execs);
            WithNoContext(execs);
            WithFAFNoContext(execs);
            WithFAFContext(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherFromOptionalInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithFAFContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherFromOptionalFAFContext());
            return execs;
        }

        public static IList<RegressionExecution> WithFAFNoContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherFromOptionalFAFNoContext());
            return execs;
        }

        public static IList<RegressionExecution> WithNoContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherFromOptionalNoContext());
            return execs;
        }

        public static IList<RegressionExecution> WithContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherFromOptionalContext(false));
            execs.Add(new EPLOtherFromOptionalContext(true));
            return execs;
        }

        private class EPLOtherFromOptionalContext : RegressionExecution
        {
            private readonly bool soda;

            public EPLOtherFromOptionalContext(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context MyContext initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(Id=s0.Id)",
                    path);

                var eplOnInit = "@name('s0') context MyContext select context.s0 as ctxs0";
                env.CompileDeploy(soda, eplOnInit, path).AddListener("s0");

                var eplOnTerm = "@name('s1') context MyContext select context.s0 as ctxs0 output when terminated";
                env.CompileDeploy(soda, eplOnTerm, path).AddListener("s1");

                var s0A = new SupportBean_S0(10, "A");
                env.SendEventBean(s0A);
                env.AssertEqualsNew("s0", "ctxs0", s0A);
                env.AssertIterator("s0", iterator => Assert.AreEqual(s0A, iterator.Advance().Get("ctxs0")));

                env.Milestone(0);

                var s0B = new SupportBean_S0(20, "B");
                env.SendEventBean(s0B);
                env.AssertEqualsNew("s0", "ctxs0", s0B);
                AssertIterator(env, "s0", s0A, s0B);
                AssertIterator(env, "s1", s0A, s0B);

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(10, "A"));
                env.AssertEqualsNew("s1", "ctxs0", s0A);
                AssertIterator(env, "s0", s0B);
                AssertIterator(env, "s1", s0B);

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S1(20, "A"));
                env.AssertEqualsNew("s1", "ctxs0", s0B);
                AssertIterator(env, "s0");
                AssertIterator(env, "s1");

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        private class EPLOtherFromOptionalFAFNoContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.AdvanceTime(1000);

                var eplObjects = "@public create variable string MYVAR = 'abc';\n" +
                                 "@public create window MyWindow#keepall as SupportBean;\n" +
                                 "on SupportBean merge MyWindow insert select *;\n" +
                                 "@public create table MyTable(field int);\n" +
                                 "on SupportBean merge MyTable insert select IntPrimitive as field;\n";
                env.CompileDeploy(eplObjects, path);
                env.SendEventBean(new SupportBean("E1", 1));

                RunSelectFAFSimpleCol(env, path, 1, "1");
                RunSelectFAFSimpleCol(env, path, 1000L, "current_timestamp()");
                RunSelectFAFSimpleCol(env, path, "abc", "MYVAR");
                RunSelectFAFSimpleCol(env, path, 1, "sum(1)");
                RunSelectFAFSimpleCol(env, path, 1L, "(select count(*) from MyWindow)");
                RunSelectFAFSimpleCol(env, path, 1L, "(select count(*) from MyTable)");
                RunSelectFAFSimpleCol(env, path, 1, "MyTable.field");

                RunSelectFAF(env, path, null, "select 1 as value where 'a'='b'");
                RunSelectFAF(env, path, 1, "select 1 as value where 1-0=1");
                RunSelectFAF(env, path, null, "select 1 as value having 'a'='b'");

                var eplScript = "expression string one() ['x']\n select one() as value";
                RunSelectFAF(env, path, "x", eplScript);

                var eplInlinedClass = "inlined_class \"\"\"\n" +
                                      "  public class Helper {\n" +
                                      "    public static String doit() { return \"y\";}\n" +
                                      "  }\n" +
                                      "\"\"\"\n select Helper.doit() as value";
                RunSelectFAF(env, path, "y", eplInlinedClass);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class EPLOtherFromOptionalNoContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select 1 as value");
                env.AssertIterator("s0", enumerator => Assert.AreEqual(1, enumerator.Advance().Get("value")));

                env.UndeployAll();
            }
        }

        private class EPLOtherFromOptionalInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var context =
                    "@public create context MyContext initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(Id=s0.Id);";
                env.CompileDeploy(context, path);

                // subselect needs from clause
                env.TryInvalidCompile("select (select 1)", "Incorrect syntax near ')'");

                // wildcard not allowed
                env.TryInvalidCompile("select *", "Wildcard cannot be used when the from-clause is not provided");
                TryInvalidFAFCompile(
                    env,
                    path,
                    "select *",
                    "Wildcard cannot be used when the from-clause is not provided");

                // context requires a single selector
                var compiled = env.CompileFAF("context MyContext select context.s0.P00 as Id", path);
                try {
                    env.Runtime.FireAndForgetService.ExecuteQuery(compiled, new ContextPartitionSelector[2]);
                    Assert.Fail();
                }
                catch (ArgumentException ex) {
                    Assert.AreEqual(
                        "Fire-and-forget queries without a from-clause allow only a single context partition selector",
                        ex.Message);
                }

                // context + order-by not allowed
                TryInvalidFAFCompile(
                    env,
                    path,
                    "context MyContext select context.s0.P00 as P00 order by P00 desc",
                    "Fire-and-forget queries without a from-clause and with context do not allow Order-by");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLOtherFromOptionalFAFContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create context MyContext initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(Id=s0.Id);\n" +
                    "context MyContext select count(*) from SupportBean;\n";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean_S0(10, "A", "x"));
                env.SendEventBean(new SupportBean_S0(20, "B", "x"));
                var eplFAF = "context MyContext select context.s0.P00 as Id";
                var compiled = env.CompileFAF(eplFAF, path);
                AssertPropsPerRow(
                    env.Runtime.FireAndForgetService.ExecuteQuery(compiled).Array,
                    "Id".Split(","),
                    new object[][] { new object[] { "A" }, new object[] { "B" } });

                // context partition selector
                ContextPartitionSelector selector = new SupportSelectorById(1);
                AssertPropsPerRow(
                    env.Runtime.FireAndForgetService.ExecuteQuery(compiled, new ContextPartitionSelector[] { selector })
                        .Array,
                    "Id".Split(","),
                    new object[][] { new object[] { "B" } });

                // SODA
                var model = env.EplToModel(eplFAF);
                Assert.AreEqual(eplFAF, model.ToEPL());
                compiled = env.CompileFAF(model, path);
                AssertPropsPerRow(
                    env.Runtime.FireAndForgetService.ExecuteQuery(compiled).Array,
                    "Id".Split(","),
                    new object[][] { new object[] { "A" }, new object[] { "B" } });

                // distinct
                var eplFAFDistint = "context MyContext select distinct context.s0.P01 as P01";
                var result = env.CompileExecuteFAF(eplFAFDistint, path);
                AssertPropsPerRow(result.Array, "P01".Split(","), new object[][] { new object[] { "x" } });

                // where-clause and having-clause
                RunSelectFAF(env, path, null, "context MyContext select 1 as value where 'a'='b'");
                RunSelectFAF(env, path, "A", "context MyContext select context.s0.P00 as value where context.s0.Id=10");
                RunSelectFAF(
                    env,
                    path,
                    "A",
                    "context MyContext select context.s0.P00 as value having context.s0.Id=10");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private static void RunSelectFAFSimpleCol(
            RegressionEnvironment env,
            RegressionPath path,
            object expected,
            string col)
        {
            RunSelectFAF(env, path, expected, "select " + col + " as value");
        }

        private static void RunSelectFAF(
            RegressionEnvironment env,
            RegressionPath path,
            object expected,
            string epl)
        {
            var result = env.CompileExecuteFAF(epl, path).Array;
            if (expected == null) {
                Assert.AreEqual(0, result == null ? 0 : result.Length);
            }
            else {
                Assert.AreEqual(expected, result[0].Get("value"));
            }
        }

        private static void AssertIterator(
            RegressionEnvironment env,
            string name,
            params SupportBean_S0[] s0)
        {
            env.AssertIterator(
                name,
                it => {
                    for (var i = 0; i < s0.Length; i++) {
                        Assert.IsTrue(it.MoveNext());
                        Assert.AreEqual(s0[i], it.Current.Get("ctxs0"));
                    }

                    Assert.IsFalse(it.MoveNext());
                });
        }
    }
} // end of namespace