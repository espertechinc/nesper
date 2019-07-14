///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreAnyAllSome
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprCoreAnyAllSomeEqualsAll());
            executions.Add(new ExprCoreEqualsAllArray());
            executions.Add(new ExprCoreEqualsAny());
            executions.Add(new ExprCoreEqualsAnyBigInt());
            executions.Add(new ExprCoreEqualsAnyArray());
            executions.Add(new ExprCoreRelationalOpAllArray());
            executions.Add(new ExprCoreRelationalOpNullOrNoRows());
            executions.Add(new ExprCoreRelationalOpAnyArray());
            executions.Add(new ExprCoreRelationalOpAll());
            executions.Add(new ExprCoreRelationalOpAny());
            executions.Add(new ExprCoreEqualsInNullOrNoRows());
            executions.Add(new ExprCoreAnyAllSomeInvalid());
            return executions;
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

        internal class ExprCoreAnyAllSomeEqualsAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "eq,neq,sqlneq,nneq".SplitCsv();
                var epl = "@Name('s0') select " +
                          "IntPrimitive=all(1,IntBoxed) as eq, " +
                          "IntPrimitive!=all(1,IntBoxed) as neq, " +
                          "IntPrimitive<>all(1,IntBoxed) as sqlneq, " +
                          "not IntPrimitive=all(1,IntBoxed) as nneq " +
                          "from SupportBean(theString like \"E%\")";
                env.CompileDeploy(epl).AddListener("s0");

                // in the format IntPrimitive, intBoxed
                int[][] testdata = {
                    new[] {1, 1},
                    new[] {1, 2},
                    new[] {2, 2},
                    new[] {2, 1}
                };

                object[][] result = {
                    new object[] {true, false, false, false}, // 1, 1
                    new object[] {false, false, false, true}, // 1, 2
                    new object[] {false, false, false, true}, // 2, 2
                    new object[] {false, true, true, true} // 2, 1
                };

                for (var i = 0; i < testdata.Length; i++) {
                    var bean = new SupportBean("E", testdata[i][0]);
                    bean.IntBoxed = testdata[i][1];
                    env.SendEventBean(bean);
                    //System.out.println("line " + i);
                    EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, result[i]);
                }

                env.UndeployAll();

                // test OM
                var model = env.EplToModel(epl);
                Assert.AreEqual(epl.Replace("<>", "!="), model.ToEPL());
                env.CompileDeploy(model).AddListener("s0");

                for (var i = 0; i < testdata.Length; i++) {
                    var bean = new SupportBean("E", testdata[i][0]);
                    bean.IntBoxed = testdata[i][1];
                    env.SendEventBean(bean);
                    EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, result[i]);
                }

                env.UndeployAll();
            }
        }

        internal class ExprCoreEqualsAllArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "e,ne".SplitCsv();
                var epl = "@Name('s0') select " +
                          "longBoxed = all ({1, 1}, intArr, longCol) as e, " +
                          "longBoxed != all ({1, 1}, intArr, longCol) as ne " +
                          "from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");

                var arrayBean = new SupportBeanArrayCollMap(new[] {1, 1});
                arrayBean.LongCol = Arrays.AsList(1L, 1L);
                arrayBean.LongBoxed = 1L;
                env.SendEventBean(arrayBean);

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                arrayBean.IntArr = new[] {1, 1, 0};
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});

                arrayBean.LongBoxed = 2L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreEqualsAny : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "eq,neq,sqlneq,nneq".SplitCsv();
                var epl = "@Name('s0') select " +
                          "IntPrimitive = any (1, intBoxed) as eq, " +
                          "IntPrimitive != any (1, intBoxed) as neq, " +
                          "IntPrimitive <> any (1, intBoxed) as sqlneq, " +
                          "not IntPrimitive = any (1, intBoxed) as nneq " +
                          " from SupportBean(theString like 'E%')";
                env.CompileDeploy(epl).AddListener("s0");

                // in the format IntPrimitive, intBoxed
                int[][] testdata = {
                    new[] {1, 1},
                    new[] {1, 2},
                    new[] {2, 2},
                    new[] {2, 1}
                };

                object[][] result = {
                    new object[] {true, false, false, false}, // 1, 1
                    new object[] {true, true, true, false}, // 1, 2
                    new object[] {true, true, true, false}, // 2, 2
                    new object[] {false, true, true, true} // 2, 1
                };

                for (var i = 0; i < testdata.Length; i++) {
                    var bean = new SupportBean("E", testdata[i][0]);
                    bean.IntBoxed = testdata[i][1];
                    env.SendEventBean(bean);
                    //System.out.println("line " + i);
                    EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, result[i]);
                }

                env.UndeployAll();
            }
        }

        internal class ExprCoreEqualsAnyBigInt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var epl = "@Name('s0') select " +
                          "bigInteger = any (null, 1) as c0," +
                          "bigInteger = any (2, 3) as c1," +
                          "bigDecimal = any (null, 1) as c2," +
                          "bigDecimal = any (2, 3) as c3" +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var bean = new SupportBean();
                bean.BigInteger = new BigInteger(1);
                bean.DecimalPrimitive = 1m;
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false, true, false});

                env.UndeployAll();
            }
        }

        internal class ExprCoreEqualsAnyArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "e,ne".SplitCsv();
                var epl = "@Name('s0') select " +
                          "longBoxed = any ({1, 1}, intArr, longCol) as e, " +
                          "longBoxed != any ({1, 1}, intArr, longCol) as ne " +
                          "from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");

                var arrayBean = new SupportBeanArrayCollMap(new[] {1, 1});
                arrayBean.LongCol = Arrays.AsList(1L, 1L);
                arrayBean.LongBoxed = 1L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                arrayBean.IntArr = new[] {1, 1, 0};
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});

                arrayBean.LongBoxed = 2L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreRelationalOpAllArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge".SplitCsv();
                var epl = "@Name('s0') select " +
                          "longBoxed>all({1,2},intArr,intCol) as g, " +
                          "longBoxed>=all({1,2},intArr,intCol) as ge " +
                          "from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");

                var arrayBean = new SupportBeanArrayCollMap(new[] {1, 2});
                arrayBean.IntCol = Arrays.AsList(1, 2);
                arrayBean.LongBoxed = 3L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});

                arrayBean.LongBoxed = 2L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                arrayBean = new SupportBeanArrayCollMap(new[] {1, 3});
                arrayBean.IntCol = Arrays.AsList(1, 2);
                arrayBean.LongBoxed = 3L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                arrayBean = new SupportBeanArrayCollMap(new[] {1, 2});
                arrayBean.IntCol = Arrays.AsList(1, 3);
                arrayBean.LongBoxed = 3L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                env.UndeployAll();

                // test OM
                var model = env.EplToModel(epl);
                Assert.AreEqual(epl.Replace("<>", "!="), model.ToEPL());
                env.CompileDeploy(model).AddListener("s0");

                arrayBean = new SupportBeanArrayCollMap(new[] {1, 2});
                arrayBean.IntCol = Arrays.AsList(1, 2);
                arrayBean.LongBoxed = 3L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreRelationalOpNullOrNoRows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test array
                var fields = "vall,vany".SplitCsv();
                string epl;

                epl = "@Name('s0') select " +
                      "intBoxed >= all ({doubleBoxed, longBoxed}) as vall, " +
                      "intBoxed >= any ({doubleBoxed, longBoxed}) as vany " +
                      " from SupportBean(theString like 'E%')";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E3", null, null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});
                SendEvent(env, "E4", 1, null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                SendEvent(env, "E5", null, 1d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});
                SendEvent(env, "E6", 1, 1d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, true});
                SendEvent(env, "E7", 0, 1d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});

                env.UndeployAll();

                // test fields
                fields = "vall,vany".SplitCsv();
                epl = "@Name('s0') select " +
                      "intBoxed >= all (doubleBoxed, longBoxed) as vall, " +
                      "intBoxed >= any (doubleBoxed, longBoxed) as vany " +
                      " from SupportBean(theString like 'E%')";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                SendEvent(env, "E3", null, null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});
                SendEvent(env, "E4", 1, null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                SendEvent(env, "E5", null, 1d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});
                SendEvent(env, "E6", 1, 1d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, true});
                SendEvent(env, "E7", 0, 1d, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});

                env.UndeployAll();
            }
        }

        internal class ExprCoreRelationalOpAnyArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge".SplitCsv();
                var epl = "@Name('s0') select " +
                          "longBoxed > any ({1, 2}, intArr, intCol) as g, " +
                          "longBoxed >= any ({1, 2}, intArr, intCol) as ge " +
                          "from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");

                var arrayBean = new SupportBeanArrayCollMap(new[] {1, 2});
                arrayBean.IntCol = Arrays.AsList(1, 2);
                arrayBean.LongBoxed = 1L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                arrayBean.LongBoxed = 2L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});

                arrayBean = new SupportBeanArrayCollMap(new[] {2, 2});
                arrayBean.IntCol = Arrays.AsList(2, 1);
                arrayBean.LongBoxed = 1L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                arrayBean = new SupportBeanArrayCollMap(new[] {1, 1});
                arrayBean.IntCol = Arrays.AsList(1, 1);
                arrayBean.LongBoxed = 0L;
                env.SendEventBean(arrayBean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});

                env.UndeployAll();
            }
        }

        internal class ExprCoreRelationalOpAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge,l,le".SplitCsv();
                var epl = "@Name('s0') select " +
                          "IntPrimitive > all (1, 3, 4) as g, " +
                          "IntPrimitive >= all (1, 3, 4) as ge, " +
                          "IntPrimitive < all (1, 3, 4) as l, " +
                          "IntPrimitive <= all (1, 3, 4) as le " +
                          " from SupportBean(theString like 'E%')";
                env.CompileDeploy(epl).AddListener("s0");

                object[][] result = {
                    new object[] {false, false, true, true},
                    new object[] {false, false, false, true},
                    new object[] {false, false, false, false},
                    new object[] {false, false, false, false},
                    new object[] {false, true, false, false},
                    new object[] {true, true, false, false}
                };

                for (var i = 0; i < 6; i++) {
                    env.SendEventBean(new SupportBean("E1", i));
                    EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, result[i]);
                }

                env.UndeployAll();
            }
        }

        internal class ExprCoreRelationalOpAny : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge,l,le".SplitCsv();
                var epl = "@Name('s0') select " +
                          "IntPrimitive > any (1, 3, 4) as g, " +
                          "IntPrimitive >= some (1, 3, 4) as ge, " +
                          "IntPrimitive < any (1, 3, 4) as l, " +
                          "IntPrimitive <= some (1, 3, 4) as le " +
                          " from SupportBean(theString like 'E%')";
                env.CompileDeploy(epl).AddListener("s0");

                object[][] result = {
                    new object[] {false, false, true, true},
                    new object[] {false, true, true, true},
                    new object[] {true, true, true, true},
                    new object[] {true, true, true, true},
                    new object[] {true, true, false, true},
                    new object[] {true, true, false, false}
                };

                for (var i = 0; i < 6; i++) {
                    env.SendEventBean(new SupportBean("E1", i));
                    EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, result[i]);
                }

                env.UndeployAll();
            }
        }

        internal class ExprCoreEqualsInNullOrNoRows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test fixed array case
                string[] fields;
                string epl;

                fields = "eall,eany,neall,neany,isin".SplitCsv();
                epl = "@Name('s0') select " +
                      "intBoxed = all ({doubleBoxed, longBoxed}) as eall, " +
                      "intBoxed = any ({doubleBoxed, longBoxed}) as eany, " +
                      "intBoxed != all ({doubleBoxed, longBoxed}) as neall, " +
                      "intBoxed != any ({doubleBoxed, longBoxed}) as neany, " +
                      "intBoxed in ({doubleBoxed, longBoxed}) as isin " +
                      " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E3", null, null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});
                SendEvent(env, "E4", 1, null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});

                SendEvent(env, "E5", null, null, 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});
                SendEvent(env, "E6", 1, null, 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, true, false, null, true});
                SendEvent(env, "E7", 0, null, 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, null, null, true, null});

                env.UndeployAll();

                // test non-array case
                fields = "eall,eany,neall,neany,isin".SplitCsv();
                epl = "@Name('s0') select " +
                      "intBoxed = all (doubleBoxed, longBoxed) as eall, " +
                      "intBoxed = any (doubleBoxed, longBoxed) as eany, " +
                      "intBoxed != all (doubleBoxed, longBoxed) as neall, " +
                      "intBoxed != any (doubleBoxed, longBoxed) as neany, " +
                      "intBoxed in (doubleBoxed, longBoxed) as isin " +
                      " from SupportBean";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                SendEvent(env, "E3", null, null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});
                SendEvent(env, "E4", 1, null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});

                SendEvent(env, "E5", null, null, 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});
                SendEvent(env, "E6", 1, null, 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, true, false, null, true});
                SendEvent(env, "E7", 0, null, 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, null, null, true, null});

                env.UndeployAll();
            }
        }

        internal class ExprCoreAnyAllSomeInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select intArr = all (1, 2, 3) as r1 from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression 'intArr=all(1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select intArr > all (1, 2, 3) as r1 from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression 'intArr>all(1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }
        }
    }
} // end of namespace