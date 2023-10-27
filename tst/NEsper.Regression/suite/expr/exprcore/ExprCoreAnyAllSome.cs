///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Numerics;

using Antlr4.Runtime.Sharpen;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;


namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreAnyAllSome
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithAnyAllSomeEqualsAll(execs);
            WithEqualsAllArray(execs);
            WithEqualsAny(execs);
            WithEqualsAnyBigInt(execs);
            WithEqualsAnyArray(execs);
            WithRelationalOpAllArray(execs);
            WithRelationalOpNullOrNoRows(execs);
            WithRelationalOpAnyArray(execs);
            WithRelationalOpAll(execs);
            WithRelationalOpAny(execs);
            WithEqualsInNullOrNoRows(execs);
            WithAnyAllSomeInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAnyAllSomeInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAnyAllSomeInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsInNullOrNoRows(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEqualsInNullOrNoRows());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpAny(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreRelationalOpAny());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreRelationalOpAll());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpAnyArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreRelationalOpAnyArray());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpNullOrNoRows(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreRelationalOpNullOrNoRows());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpAllArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreRelationalOpAllArray());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsAnyArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEqualsAnyArray());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsAnyBigInt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEqualsAnyBigInt());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsAny(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEqualsAny());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsAllArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEqualsAllArray());
            return execs;
        }

        public static IList<RegressionExecution> WithAnyAllSomeEqualsAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAnyAllSomeEqualsAll());
            return execs;
        }

        private class ExprCoreAnyAllSomeEqualsAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "eq,neq,sqlneq,nneq".SplitCsv();
                var eplReference = new AtomicReference<string>();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], "IntPrimitive=all(1,IntBoxed)")
                    .WithExpression(fields[1], "IntPrimitive!=all(1,IntBoxed)")
                    .WithExpression(fields[2], "IntPrimitive<>all(1,IntBoxed)")
                    .WithExpression(fields[3], "not IntPrimitive=all(1,IntBoxed)")
                    .WithStatementConsumer(
                        stmt => eplReference.Set(stmt.GetProperty(StatementProperty.EPL).ToString()));

                // in the format intPrimitive, intBoxed
                int[][] testdata = {
                    new int[] { 1, 1 },
                    new int[] { 1, 2 },
                    new int[] { 2, 2 },
                    new int[] { 2, 1 },
                };

                object[][] result = {
                    new object[] { true, false, false, false }, // 1, 1
                    new object[] { false, false, false, true }, // 1, 2
                    new object[] { false, false, false, true }, // 2, 2
                    new object[] { false, true, true, true } // 2, 1
                };

                for (var i = 0; i < testdata.Length; i++) {
                    var bean = new SupportBean("E", testdata[i][0]);
                    bean.IntBoxed = testdata[i][1];
                    builder.WithAssertion(bean).Expect(fields, result[i]);
                }

                builder.Run(env);
                env.UndeployAll();

                // test OM
                env.AssertThat(
                    () => {
                        var epl = eplReference.Get();
                        var model = env.EplToModel(epl);
                        Assert.AreEqual(epl.Replace("<>", "!="), model.ToEPL());
                        env.CompileDeploy(model).AddListener("s0");

                        for (var i = 0; i < testdata.Length; i++) {
                            var bean = new SupportBean("E", testdata[i][0]);
                            bean.IntBoxed = testdata[i][1];
                            env.SendEventBean(bean);
                            env.AssertPropsNew("s0", fields, result[i]);
                        }
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreEqualsAllArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "e,ne".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBeanArrayCollMap")
                    .WithExpressions(
                        fields,
                        "LongBoxed = all ({1, 1}, IntArr, LongCol)",
                        "LongBoxed != all ({1, 1}, IntArr, LongCol)");

                var arrayBean = MakeArrayBean();
                builder.WithAssertion(arrayBean).Expect(fields, true, false);

                arrayBean = MakeArrayBean();
                arrayBean.IntArr = new int[] { 1, 1, 0 };
                builder.WithAssertion(arrayBean).Expect(fields, false, false);

                arrayBean = MakeArrayBean();
                arrayBean.LongBoxed = 2L;
                builder.WithAssertion(arrayBean).Expect(fields, false, true);

                builder.Run(env);
                env.UndeployAll();
            }

            private SupportBeanArrayCollMap MakeArrayBean()
            {
                var arrayBean = new SupportBeanArrayCollMap(new int[] { 1, 1 });
                arrayBean.LongCol = Arrays.AsList(1L, 1L);
                arrayBean.LongBoxed = 1L;
                return arrayBean;
            }
        }

        private class ExprCoreEqualsAny : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "eq,neq,sqlneq,nneq".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], "IntPrimitive=any(1,IntBoxed)")
                    .WithExpression(fields[1], "IntPrimitive!=any(1,IntBoxed)")
                    .WithExpression(fields[2], "IntPrimitive<>any(1,IntBoxed)")
                    .WithExpression(fields[3], "not IntPrimitive=any(1,IntBoxed)");

                // in the format intPrimitive, intBoxed
                int[][] testdata = {
                    new int[] { 1, 1 },
                    new int[] { 1, 2 },
                    new int[] { 2, 2 },
                    new int[] { 2, 1 },
                };

                object[][] result = {
                    new object[] { true, false, false, false }, // 1, 1
                    new object[] { true, true, true, false }, // 1, 2
                    new object[] { true, true, true, false }, // 2, 2
                    new object[] { false, true, true, true } // 2, 1
                };

                for (var i = 0; i < testdata.Length; i++) {
                    var bean = new SupportBean("E", testdata[i][0]);
                    bean.IntBoxed = testdata[i][1];
                    builder.WithAssertion(bean).Expect(fields, result[i]);
                }

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreEqualsAnyBigInt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], "BigInteger = any (null, 1)")
                    .WithExpression(fields[1], "BigInteger = any (2, 3)")
                    .WithExpression(fields[2], "DecimalPrimitive = any (null, 1)")
                    .WithExpression(fields[3], "DecimalPrimitive = any (2, 3)");

                var bean = new SupportBean();
                bean.BigInteger = new BigInteger(1);
                bean.DecimalPrimitive = 1m;
                builder.WithAssertion(bean).Expect(fields, true, false, true, false);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreEqualsAnyArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "e,ne".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBeanArrayCollMap")
                    .WithExpressions(
                        fields,
                        "LongBoxed = any ({1, 1}, IntArr, LongCol)",
                        "LongBoxed != any ({1, 1}, IntArr, LongCol)");

                var arrayBean = MakeArrayBean();
                builder.WithAssertion(arrayBean).Expect(fields, true, false);

                arrayBean = MakeArrayBean();
                arrayBean.IntArr = new int[] { 1, 1, 0 };
                builder.WithAssertion(arrayBean).Expect(fields, true, true);

                arrayBean = MakeArrayBean();
                arrayBean.LongBoxed = 2L;
                builder.WithAssertion(arrayBean).Expect(fields, false, true);

                builder.Run(env);
                env.UndeployAll();
            }

            private SupportBeanArrayCollMap MakeArrayBean()
            {
                var arrayBean = new SupportBeanArrayCollMap(new int[] { 1, 1 });
                arrayBean.LongCol = Arrays.AsList(1L, 1L);
                arrayBean.LongBoxed = 1L;
                return arrayBean;
            }
        }

        private class ExprCoreRelationalOpAllArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBeanArrayCollMap")
                    .WithExpressions(
                        fields,
                        "LongBoxed>all({1,2},IntArr,IntCol)",
                        "LongBoxed>=all({1,2},IntArr,IntCol)");

                var arrayBean = MakeBean();
                arrayBean.IntCol = Arrays.AsList(1, 2);
                arrayBean.LongBoxed = 3L;
                builder.WithAssertion(arrayBean).Expect(fields, true, true);

                arrayBean = MakeBean();
                arrayBean.LongBoxed = 2L;
                env.SendEventBean(arrayBean);
                builder.WithAssertion(arrayBean).Expect(fields, false, true);

                arrayBean = new SupportBeanArrayCollMap(new int[] { 1, 3 });
                arrayBean.IntCol = Arrays.AsList(1, 2);
                arrayBean.LongBoxed = 3L;
                env.SendEventBean(arrayBean);
                builder.WithAssertion(arrayBean).Expect(fields, false, true);

                arrayBean = new SupportBeanArrayCollMap(new int[] { 1, 2 });
                arrayBean.IntCol = Arrays.AsList(1, 3);
                arrayBean.LongBoxed = 3L;
                env.SendEventBean(arrayBean);
                builder.WithAssertion(arrayBean).Expect(fields, false, true);

                var runResult = builder.Run(env);
                env.UndeployAll();

                // test OM
                var model = env.EplToModel(runResult.EPL);
                Assert.AreEqual(runResult.EPL.Replace("<>", "!="), model.ToEPL());
                env.CompileDeploy(model).AddListener("s0");

                arrayBean = new SupportBeanArrayCollMap(new int[] { 1, 2 });
                arrayBean.IntCol = Arrays.AsList(1, 2);
                arrayBean.LongBoxed = 3L;
                env.SendEventBean(arrayBean);
                env.AssertPropsNew("s0", fields, new object[] { true, true });

                env.UndeployAll();
            }

            private SupportBeanArrayCollMap MakeBean()
            {
                var arrayBean = new SupportBeanArrayCollMap(new int[] { 1, 2 });
                arrayBean.IntCol = Arrays.AsList(1, 2);
                arrayBean.LongBoxed = 3L;
                return arrayBean;
            }
        }

        private class ExprCoreRelationalOpNullOrNoRows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test array
                var fields = "vall,vany".SplitCsv();
                string epl;

                epl = "@name('s0') select " +
                      "IntBoxed >= all ({DoubleBoxed, LongBoxed}) as vall, " +
                      "IntBoxed >= any ({DoubleBoxed, LongBoxed}) as vany " +
                      " from SupportBean(TheString like 'E%')";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E3", null, null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null });
                SendEvent(env, "E4", 1, null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null });

                SendEvent(env, "E5", null, 1d, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null });
                SendEvent(env, "E6", 1, 1d, null);
                env.AssertPropsNew("s0", fields, new object[] { null, true });
                SendEvent(env, "E7", 0, 1d, null);
                env.AssertPropsNew("s0", fields, new object[] { false, false });

                env.UndeployAll();

                // test fields
                fields = "vall,vany".SplitCsv();
                epl = "@name('s0') select " +
                      "IntBoxed >= all (DoubleBoxed, LongBoxed) as vall, " +
                      "IntBoxed >= any (DoubleBoxed, LongBoxed) as vany " +
                      " from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                SendEvent(env, "E3", null, null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null });
                SendEvent(env, "E4", 1, null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null });

                SendEvent(env, "E5", null, 1d, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null });
                SendEvent(env, "E6", 1, 1d, null);
                env.AssertPropsNew("s0", fields, new object[] { null, true });
                SendEvent(env, "E7", 0, 1d, null);
                env.AssertPropsNew("s0", fields, new object[] { false, false });

                env.UndeployAll();
            }
        }

        private class ExprCoreRelationalOpAnyArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge".SplitCsv();
                var epl = "@name('s0') select " +
                          "LongBoxed > any ({1, 2}, IntArr, IntCol) as g, " +
                          "LongBoxed >= any ({1, 2}, IntArr, IntCol) as ge " +
                          "from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");

                var arrayBean = new SupportBeanArrayCollMap(new int[] { 1, 2 });
                arrayBean.IntCol = Arrays.AsList(1, 2);
                arrayBean.LongBoxed = 1L;
                env.SendEventBean(arrayBean);
                env.AssertPropsNew("s0", fields, new object[] { false, true });

                arrayBean.LongBoxed = 2L;
                env.SendEventBean(arrayBean);
                env.AssertPropsNew("s0", fields, new object[] { true, true });

                arrayBean = new SupportBeanArrayCollMap(new int[] { 2, 2 });
                arrayBean.IntCol = Arrays.AsList(2, 1);
                arrayBean.LongBoxed = 1L;
                env.SendEventBean(arrayBean);
                env.AssertPropsNew("s0", fields, new object[] { false, true });

                arrayBean = new SupportBeanArrayCollMap(new int[] { 1, 1 });
                arrayBean.IntCol = Arrays.AsList(1, 1);
                arrayBean.LongBoxed = 0L;
                env.SendEventBean(arrayBean);
                env.AssertPropsNew("s0", fields, new object[] { false, false });

                env.UndeployAll();
            }
        }

        private class ExprCoreRelationalOpAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge,l,le".SplitCsv();
                var epl = "@name('s0') select " +
                          "IntPrimitive > all (1, 3, 4) as g, " +
                          "IntPrimitive >= all (1, 3, 4) as ge, " +
                          "IntPrimitive < all (1, 3, 4) as l, " +
                          "IntPrimitive <= all (1, 3, 4) as le " +
                          " from SupportBean(TheString like 'E%')";
                env.CompileDeploy(epl).AddListener("s0");

                object[][] result = {
                    new object[] { false, false, true, true },
                    new object[] { false, false, false, true },
                    new object[] { false, false, false, false },
                    new object[] { false, false, false, false },
                    new object[] { false, true, false, false },
                    new object[] { true, true, false, false }
                };

                for (var i = 0; i < 6; i++) {
                    env.SendEventBean(new SupportBean("E1", i));
                    env.AssertPropsNew("s0", fields, result[i]);
                }

                env.UndeployAll();
            }
        }

        private class ExprCoreRelationalOpAny : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge,l,le".SplitCsv();
                var epl = "@name('s0') select " +
                          "IntPrimitive > any (1, 3, 4) as g, " +
                          "IntPrimitive >= some (1, 3, 4) as ge, " +
                          "IntPrimitive < any (1, 3, 4) as l, " +
                          "IntPrimitive <= some (1, 3, 4) as le " +
                          " from SupportBean(TheString like 'E%')";
                env.CompileDeploy(epl).AddListener("s0");

                object[][] result = {
                    new object[] { false, false, true, true },
                    new object[] { false, true, true, true },
                    new object[] { true, true, true, true },
                    new object[] { true, true, true, true },
                    new object[] { true, true, false, true },
                    new object[] { true, true, false, false }
                };

                for (var i = 0; i < 6; i++) {
                    env.SendEventBean(new SupportBean("E1", i));
                    env.AssertPropsNew("s0", fields, result[i]);
                }

                env.UndeployAll();
            }
        }

        private class ExprCoreEqualsInNullOrNoRows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test fixed array case
                string[] fields;
                string epl;

                fields = "eall,eany,neall,neany,isin".SplitCsv();
                epl = "@name('s0') select " +
                      "IntBoxed = all ({DoubleBoxed, LongBoxed}) as eall, " +
                      "IntBoxed = any ({DoubleBoxed, LongBoxed}) as eany, " +
                      "IntBoxed != all ({DoubleBoxed, LongBoxed}) as neall, " +
                      "IntBoxed != any ({DoubleBoxed, LongBoxed}) as neany, " +
                      "IntBoxed in ({DoubleBoxed, LongBoxed}) as isin " +
                      " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E3", null, null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });
                SendEvent(env, "E4", 1, null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });

                SendEvent(env, "E5", null, null, 1L);
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });
                SendEvent(env, "E6", 1, null, 1L);
                env.AssertPropsNew("s0", fields, new object[] { null, true, false, null, true });
                SendEvent(env, "E7", 0, null, 1L);
                env.AssertPropsNew("s0", fields, new object[] { false, null, null, true, null });

                env.UndeployAll();

                // test non-array case
                fields = "eall,eany,neall,neany,isin".SplitCsv();
                epl = "@name('s0') select " +
                      "IntBoxed = all (DoubleBoxed, LongBoxed) as eall, " +
                      "IntBoxed = any (DoubleBoxed, LongBoxed) as eany, " +
                      "IntBoxed != all (DoubleBoxed, LongBoxed) as neall, " +
                      "IntBoxed != any (DoubleBoxed, LongBoxed) as neany, " +
                      "IntBoxed in (DoubleBoxed, LongBoxed) as isin " +
                      " from SupportBean";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                SendEvent(env, "E3", null, null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });
                SendEvent(env, "E4", 1, null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });

                SendEvent(env, "E5", null, null, 1L);
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });
                SendEvent(env, "E6", 1, null, 1L);
                env.AssertPropsNew("s0", fields, new object[] { null, true, false, null, true });
                SendEvent(env, "E7", 0, null, 1L);
                env.AssertPropsNew("s0", fields, new object[] { false, null, null, true, null });

                env.UndeployAll();
            }
        }

        private class ExprCoreAnyAllSomeInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select IntArr = all (1, 2, 3) as r1 from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression 'IntArr=all(1,2,3)': Collection or array comparison and null-type values are not allowed for the IN, ANY, SOME or ALL keywords");
                env.TryInvalidCompile(
                    "select IntArr > all (1, 2, 3) as r1 from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression 'IntArr>all(1,2,3)': Collection or array comparison and null-type values are not allowed for the IN, ANY, SOME or ALL keywords");
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int? intBoxed,
            double? doubleBoxed,
            long? longBoxed)
        {
            var bean = new SupportBean(theString, -1);
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            env.SendEventBean(bean);
        }
    }
} // end of namespace