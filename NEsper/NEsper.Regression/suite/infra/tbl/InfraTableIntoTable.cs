///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableIntoTable
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraIntoTableUnkeyedSimpleSameModule());
            execs.Add(new InfraIntoTableUnkeyedSimpleTwoModule());
            execs.Add(new InfraBoundUnbound());
            execs.Add(new InfraIntoTableWindowSortedFromJoin());
            execs.Add(new InfraTableIntoTableNoKeys());
            execs.Add(new InfraTableIntoTableWithKeys());
            execs.Add(new InfraTableBigNumberAggregation());
            return execs;
        }

        private static void TryAssertionLastFirstWindow(
            RegressionEnvironment env,
            bool soda,
            AtomicLong milestone)
        {
            var fields = "lasteveru,firsteveru,windowb".SplitCsv();
            var path = new RegressionPath();
            var eplDeclare = "create table varagg (" +
                             "lasteveru lastever(*) @type('SupportBean'), " +
                             "firsteveru firstever(*) @type('SupportBean'), " +
                             "windowb window(*) @type('SupportBean'))";
            env.CompileDeploy(soda, eplDeclare, path);

            var eplIterate = "@Name('iterate') select varagg from SupportBean_S0#lastevent";
            env.CompileDeploy(soda, eplIterate, path);
            env.SendEventBean(new SupportBean_S0(0));

            var eplBoundInto = "into table varagg select window(*) as windowb from SupportBean#length(2)";
            env.CompileDeploy(soda, eplBoundInto, path);

            var eplUnboundInto =
                "into table varagg select lastever(*) as lasteveru, firstever(*) as firsteveru from SupportBean";
            env.CompileDeploy(soda, eplUnboundInto, path);

            var b1 = MakeSendBean(env, "E1", 20);
            var b2 = MakeSendBean(env, "E2", 15);

            env.MilestoneInc(milestone);

            var b3 = MakeSendBean(env, "E3", 10);
            AssertResults(
                env.Statement("iterate"),
                fields,
                new object[] {
                    b3, b1,
                    new object[] {b2, b3}
                });

            env.MilestoneInc(milestone);

            var b4 = MakeSendBean(env, "E4", 5);
            AssertResults(
                env.Statement("iterate"),
                fields,
                new object[] {
                    b4, b1,
                    new object[] {b3, b4}
                });

            // invalid: bound aggregation into unbound max
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                path,
                "into table varagg select last(*) as lasteveru from SupportBean#length(2)",
                "Failed to validate select-clause expression 'last(*)': For into-table use 'window(*)' or 'window(stream.*)' instead");
            // invalid: unbound aggregation into bound max
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                path,
                "into table varagg select lastever(*) as windowb from SupportBean#length(2)",
                "Incompatible aggregation function for table 'varagg' column 'windowb', expecting 'window(*)' and received 'lastever(*)': The table declares 'window(*)' and provided is 'lastever(*)'");

            // valid: bound with unbound variable
            var eplBoundIntoUnbound = "into table varagg select lastever(*) as lasteveru from SupportBean#length(2)";
            env.CompileDeploy(soda, eplBoundIntoUnbound, path);

            env.UndeployAll();
        }

        private static void TryAssertionSortedMinMaxBy(
            RegressionEnvironment env,
            bool soda,
            AtomicLong milestone)
        {
            var fields = "maxbyeveru,minbyeveru,sortedb".SplitCsv();
            var path = new RegressionPath();

            var eplDeclare = "create table varagg (" +
                             "maxbyeveru maxbyever(IntPrimitive) @type('SupportBean'), " +
                             "minbyeveru minbyever(IntPrimitive) @type('SupportBean'), " +
                             "sortedb sorted(IntPrimitive) @type('SupportBean'))";
            env.CompileDeploy(soda, eplDeclare, path);

            var eplIterate = "@Name('iterate') select varagg from SupportBean_S0#lastevent";
            env.CompileDeploy(soda, eplIterate, path);
            env.SendEventBean(new SupportBean_S0(0));

            var eplBoundInto = "into table varagg select sorted() as sortedb from SupportBean#length(2)";
            env.CompileDeploy(soda, eplBoundInto, path);

            var eplUnboundInto =
                "into table varagg select maxbyever() as maxbyeveru, minbyever() as minbyeveru from SupportBean";
            env.CompileDeploy(soda, eplUnboundInto, path);

            var b1 = MakeSendBean(env, "E1", 20);
            var b2 = MakeSendBean(env, "E2", 15);

            env.MilestoneInc(milestone);

            var b3 = MakeSendBean(env, "E3", 10);
            AssertResults(
                env.Statement("iterate"),
                fields,
                new object[] {
                    b1, b3,
                    new object[] {b3, b2}
                });

            // invalid: bound aggregation into unbound max
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                path,
                "into table varagg select maxby(IntPrimitive) as maxbyeveru from SupportBean#length(2)",
                "Failed to validate select-clause expression 'maxby(IntPrimitive)': When specifying into-table a sort expression cannot be provided [");
            // invalid: unbound aggregation into bound max
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                path,
                "into table varagg select maxbyever() as sortedb from SupportBean#length(2)",
                "Incompatible aggregation function for table 'varagg' column 'sortedb', expecting 'sorted(IntPrimitive)' and received 'maxbyever()': The required aggregation function name is 'sorted' and provided is 'maxbyever' [");

            // valid: bound with unbound variable
            var eplBoundIntoUnbound = "into table varagg select " +
                                      "maxbyever() as maxbyeveru, minbyever() as minbyeveru " +
                                      "from SupportBean#length(2)";
            env.CompileDeploy(soda, eplBoundIntoUnbound, path);

            env.UndeployAll();
        }

        private static void TryAssertionMinMax(
            RegressionEnvironment env,
            bool soda,
            AtomicLong milestone)
        {
            var fields = "maxb,maxu,minb,minu".SplitCsv();
            var path = new RegressionPath();
            var eplDeclare = "create table varagg (" +
                             "maxb max(int), maxu maxever(int), minb min(int), minu minever(int))";
            env.CompileDeploy(soda, eplDeclare, path);

            var eplIterate = "@Name('iterate') select varagg from SupportBean_S0#lastevent";
            env.CompileDeploy(soda, eplIterate, path);
            env.SendEventBean(new SupportBean_S0(0));

            var eplBoundInto = "into table varagg select " +
                               "max(IntPrimitive) as maxb, min(IntPrimitive) as minb " +
                               "from SupportBean#length(2)";
            env.CompileDeploy(soda, eplBoundInto, path);

            var eplUnboundInto = "into table varagg select " +
                                 "maxever(IntPrimitive) as maxu, minever(IntPrimitive) as minu " +
                                 "from SupportBean";
            env.CompileDeploy(soda, eplUnboundInto, path);

            env.SendEventBean(new SupportBean("E1", 20));
            env.SendEventBean(new SupportBean("E2", 15));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E3", 10));
            AssertResults(
                env.Statement("iterate"),
                fields,
                new object[] {15, 20, 10, 10});

            env.SendEventBean(new SupportBean("E4", 5));
            AssertResults(
                env.Statement("iterate"),
                fields,
                new object[] {10, 20, 5, 5});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E5", 25));
            AssertResults(
                env.Statement("iterate"),
                fields,
                new object[] {25, 25, 5, 5});

            // invalid: unbound aggregation into bound max
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                path,
                "into table varagg select max(IntPrimitive) as maxb from SupportBean",
                "Incompatible aggregation function for table 'varagg' column 'maxb', expecting 'max(int)' and received 'max(IntPrimitive)': The table declares use with data windows and provided is unbound [");

            // valid: bound with unbound variable
            var eplBoundIntoUnbound = "into table varagg select " +
                                      "maxever(IntPrimitive) as maxu, minever(IntPrimitive) as minu " +
                                      "from SupportBean#length(2)";
            env.CompileDeploy(soda, eplBoundIntoUnbound, path);

            env.UndeployAll();
        }

        private static void AssertResults(
            EPStatement stmt,
            string[] fields,
            object[] values)
        {
            var @event = stmt.First();
            var map = (IDictionary<string, object>) @event.Get("varagg");
            EPAssertionUtil.AssertPropsMap(map, fields, values);
        }

        private static SupportBean MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            env.SendEventBean(bean);
            return bean;
        }

        private static void RunAssertionIntoTableUnkeyedSimple(RegressionEnvironment env)
        {
            Assert.IsFalse(env.GetEnumerator("tbl").MoveNext());

            env.SendEventBean(new SupportBean());
            AssertIteratorUnkeyedSimple(env, 1);

            env.Milestone(0);

            AssertIteratorUnkeyedSimple(env, 1);

            env.SendEventBean(new SupportBean());
            AssertIteratorUnkeyedSimple(env, 2);
        }

        private static void AssertIteratorUnkeyedSimple(
            RegressionEnvironment env,
            long expected)
        {
            Assert.AreEqual(expected, env.GetEnumerator("tbl").Advance().Get("mycnt"));
        }

        internal class InfraIntoTableUnkeyedSimpleSameModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('tbl') create table MyTable(mycnt count(*));\n" +
                          "into table MyTable select count(*) as mycnt from SupportBean;\n";
                env.CompileDeploy(epl);
                RunAssertionIntoTableUnkeyedSimple(env);
                env.UndeployAll();
            }
        }

        internal class InfraIntoTableUnkeyedSimpleTwoModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('tbl') create table MyTable(mycnt count(*))", path);
                env.CompileDeploy("into table MyTable select count(*) as mycnt from SupportBean;\n", path);
                RunAssertionIntoTableUnkeyedSimple(env);
                env.UndeployAll();
            }
        }

        internal class InfraIntoTableWindowSortedFromJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create table MyTable(" +
                    "thewin window(*) @type('SupportBean')," +
                    "thesort sorted(intPrimitive desc) @type('SupportBean')" +
                    ")",
                    path);

                env.CompileDeploy(
                    "into table MyTable " +
                    "select window(sb.*) as thewin, sorted(sb.*) as thesort " +
                    "from SupportBean_S0#lastevent, SupportBean#keepall as sb",
                    path);
                env.SendEventBean(new SupportBean_S0(1));

                var sb1 = new SupportBean("E1", 1);
                env.SendEventBean(sb1);

                env.Milestone(0);

                var sb2 = new SupportBean("E2", 2);
                env.SendEventBean(sb2);

                var result = env.CompileExecuteFAF("select * from MyTable", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    "thewin,thesort".SplitCsv(),
                    new[] {
                        new object[] {new[] {sb1, sb2}, new[] {sb2, sb1}}
                    });

                env.UndeployAll();
            }
        }

        internal class InfraBoundUnbound : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // Bound: max/min; Unbound: maxever/minever
                TryAssertionMinMax(env, false, milestone);
                TryAssertionMinMax(env, true, milestone);

                // Bound: window; Unbound: lastever/firstever; Disallowed: last, first
                TryAssertionLastFirstWindow(env, false, milestone);
                TryAssertionLastFirstWindow(env, true, milestone);

                // Bound: sorted; Unbound: maxbyever/minbyever; Disallowed: minby, maxby declaration (must use sorted instead)
                // - requires declaring the same sort expression but can be against subtype of declared event type
                TryAssertionSortedMinMaxBy(env, false, milestone);
                TryAssertionSortedMinMaxBy(env, true, milestone);
            }
        }

        public class InfraTableIntoTableNoKeys : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "sumint".SplitCsv();
                var path = new RegressionPath();

                var eplCreateTable = "@Name('Create-Table') create table MyTable(sumint sum(int))";
                env.CompileDeploy(eplCreateTable, path);

                var eplIntoTable =
                    "@Name('Into-Table') into table MyTable select sum(IntPrimitive) as sumint from SupportBean";
                env.CompileDeploy(eplIntoTable, path);

                var eplQueryTable = "@Name('s0') select (select sumint from MyTable) as c0 from SupportBean_S0 as s0";
                env.CompileDeploy(eplQueryTable, path).AddListener("s0");

                env.Milestone(1);

                AssertValue(env, null);

                MakeSendBean(env, "E1", 10);
                AssertValue(env, 10);

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {10}
                    });
                MakeSendBean(env, "E2", 200);
                AssertValue(env, 210);

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {210}
                    });
                MakeSendBean(env, "E1", 11);
                AssertValue(env, 221);

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {221}
                    });
                MakeSendBean(env, "E3", 3000);
                AssertValue(env, 3221);

                env.Milestone(5);
                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {3221}
                    });
                MakeSendBean(env, "E2", 201);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {3422}
                    });
                MakeSendBean(env, "E3", 3001);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {6423}
                    });

                MakeSendBean(env, "E1", 12);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {6435}
                    });

                env.UndeployAll();
            }

            private static void AssertValue(
                RegressionEnvironment env,
                int? value)
            {
                env.SendEventBean(new SupportBean_S0(0));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(value, @event.Get("c0"));
            }
        }

        public class InfraTableIntoTableWithKeys : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "pkey,sumint".SplitCsv();
                var valueList = "E1,E2,E3";
                var path = new RegressionPath();

                var eplCreateTable =
                    "@Name('Create-Table') create table MyTable(pkey string primary key, sumint sum(int))";
                env.CompileDeploy(eplCreateTable, path);

                var eplIntoTable =
                    "@Name('Into-Table') into table MyTable select sum(IntPrimitive) as sumint from SupportBean group by TheString";
                env.CompileDeploy(eplIntoTable, path);

                var eplQueryTable =
                    "@Name('s0') select (select sumint from MyTable where pkey = s0.p00) as c0 from SupportBean_S0 as s0";
                env.CompileDeploy(eplQueryTable, path).AddListener("s0");

                env.Milestone(1);

                AssertValues(env, valueList, new int?[] {null, null, null});

                MakeSendBean(env, "E1", 10);
                AssertValues(env, valueList, new int?[] {10, null, null});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {"E1", 10}
                    });
                MakeSendBean(env, "E2", 200);
                AssertValues(env, valueList, new int?[] {10, 200, null});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {"E1", 10},
                        new object[] {"E2", 200}
                    });
                MakeSendBean(env, "E1", 11);
                AssertValues(env, valueList, new int?[] {21, 200, null});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {"E1", 21},
                        new object[] {"E2", 200}
                    });
                MakeSendBean(env, "E3", 3000);
                AssertValues(env, valueList, new int?[] {21, 200, 3000});

                env.Milestone(5);
                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {"E1", 21},
                        new object[] {"E2", 200},
                        new object[] {"E3", 3000}
                    });
                MakeSendBean(env, "E2", 201);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {"E1", 21},
                        new object[] {"E2", 401},
                        new object[] {"E3", 3000}
                    });
                MakeSendBean(env, "E3", 3001);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {"E1", 21},
                        new object[] {"E2", 401},
                        new object[] {"E3", 6001}
                    });

                MakeSendBean(env, "E1", 12);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("Create-Table"),
                    fields,
                    new[] {
                        new object[] {"E1", 33},
                        new object[] {"E2", 401},
                        new object[] {"E3", 6001}
                    });

                env.UndeployAll();
            }

            private static void AssertValues(
                RegressionEnvironment env,
                string keys,
                int?[] values)
            {
                var keyarr = keys.SplitCsv();
                for (var i = 0; i < keyarr.Length; i++) {
                    env.SendEventBean(new SupportBean_S0(0, keyarr[i]));
                    var @event = env.Listener("s0").AssertOneGetNewAndReset();
                    Assert.AreEqual(values[i], @event.Get("c0"), "Failed for key '" + keyarr[i] + "'");
                }
            }
        }

        internal class InfraTableBigNumberAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var epl =
                    "@Name('tbl') create table MyTable as (c0 avg(BigInteger), c1 avg(BigDecimal), c2 sum(BigInteger), c3 sum(BigDecimal));\n" +
                    "into table MyTable select avg(bigint) as c0, avg(bigdec) as c1, sum(bigint) as c2, sum(bigdec) as c3  from SupportBeanNumeric#lastevent;\n";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportBeanNumeric(new BigInteger(5), 100m));
                var result = env.GetEnumerator("tbl").Advance();
                EPAssertionUtil.AssertProps(
                    result,
                    fields,
                    new object[] {5.0m, 100m, new BigInteger(5), 100m});

                env.SendEventBean(new SupportBeanNumeric(new BigInteger(4), 200m));
                result = env.GetEnumerator("tbl").Advance();
                EPAssertionUtil.AssertProps(
                    result,
                    fields,
                    new object[] {4m, 200m, new BigInteger(4), 200m});

                env.UndeployAll();
            }
        }
    }
} // end of namespace