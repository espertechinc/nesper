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
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreInBetweenLike
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprCoreInNumeric());
            executions.Add(new ExprCoreInObject());
            executions.Add(new ExprCoreInArraySubstitution());
            executions.Add(new ExprCoreInCollectionArrayProp());
            executions.Add(new ExprCoreInCollectionArrays());
            executions.Add(new ExprCoreInCollectionColl());
            executions.Add(new ExprCoreInCollectionMaps());
            executions.Add(new ExprCoreInCollectionMixed());
            executions.Add(new ExprCoreInCollectionObjectArrayProp());
            executions.Add(new ExprCoreInCollectionArrayConst());
            executions.Add(new ExprCoreInStringExprOM());
            executions.Add(new ExprCoreInStringExpr());
            executions.Add(new ExprCoreBetweenBigIntBigDecExpr());
            executions.Add(new ExprCoreBetweenStringExpr());
            executions.Add(new ExprCoreBetweenNumericExpr());
            executions.Add(new ExprCoreInBoolExpr());
            executions.Add(new ExprCoreInNumericCoercionLong());
            executions.Add(new ExprCoreInNumericCoercionDouble());
            executions.Add(new ExprCoreBetweenNumericCoercionLong());
            executions.Add(new ExprCoreInRange());
            executions.Add(new ExprCoreBetweenNumericCoercionDouble());
            executions.Add(new ExprCoreInBetweenInvalid());
            return executions;
        }

        private static void SendAndAssert1(
            RegressionEnvironment env,
            int? intBoxed,
            float? floatBoxed,
            double doublePrimitive,
            bool? result)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.FloatBoxed = floatBoxed;
            bean.DoublePrimitive = doublePrimitive;

            env.SendEventBean(bean);

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(result, theEvent.Get("result"));
        }

        private static void SendAndAssert2(
            RegressionEnvironment env,
            int intPrimitive,
            int shortBoxed,
            int? intBoxed,
            long? longBoxed,
            bool? result)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = 1;
            bean.ShortBoxed = (short) shortBoxed;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;

            env.SendEventBean(bean);

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(result, theEvent.Get("result"));
        }

        private static void SendAndAssert3(
            RegressionEnvironment env,
            int intPrimitive,
            int shortBoxed,
            long? longBoxed,
            bool? result)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.ShortBoxed = (short) shortBoxed;
            bean.LongBoxed = longBoxed;

            env.SendEventBean(bean);

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(result, theEvent.Get("result"));
        }

        private static void SendAndAssert4(
            RegressionEnvironment env,
            int? intBoxed,
            float? floatBoxed,
            double doublePrimitive,
            long? longBoxed,
            bool? result)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.FloatBoxed = floatBoxed;
            bean.DoublePrimitive = doublePrimitive;
            bean.LongBoxed = longBoxed;

            env.SendEventBean(bean);

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(result, theEvent.Get("result"));
        }

        private static void TryInBoolean(
            RegressionEnvironment env,
            string expr,
            bool[] input,
            bool[] result)
        {
            var epl = "@Name('s0') select " + expr + " as result from " + typeof(SupportBean).FullName;
            env.CompileDeploy(epl).AddListener("s0");
            Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

            for (var i = 0; i < input.Length; i++) {
                SendSupportBeanEvent(env, input[i]);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }

            env.UndeployAll();
        }

        private static void TryString(
            RegressionEnvironment env,
            string expression,
            string[] input,
            bool?[] result)
        {
            var epl = "@Name('s0') select " + expression + " as result from " + typeof(SupportBean).FullName;
            env.CompileDeploy(epl).AddListener("s0");

            Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

            for (var i = 0; i < input.Length; i++) {
                SendSupportBeanEvent(env, input[i]);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }

            env.UndeployAll();
        }

        private static void TryString(
            RegressionEnvironment env,
            EPStatementObjectModel model,
            string epl,
            string[] input,
            bool?[] result)
        {
            var compiled = env.Compile(model, new CompilerArguments(env.Configuration));
            Assert.AreEqual(epl, model.ToEPL());

            var objectmodel = env.EplToModel(epl);
            objectmodel = env.CopyMayFail(objectmodel);
            Assert.AreEqual(epl, objectmodel.ToEPL());

            env.Deploy(compiled).AddListener("s0");

            Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

            for (var i = 0; i < input.Length; i++) {
                SendSupportBeanEvent(env, input[i]);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }

            env.UndeployAll();
        }

        private static void TryNumeric(
            RegressionEnvironment env,
            string expr,
            double?[] input,
            bool?[] result)
        {
            var epl = "@Name('s0') select " + expr + " as result from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

            for (var i = 0; i < input.Length; i++) {
                SendSupportBeanEvent(env, input[i]);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }

            env.UndeployAll();
        }

        private static void SendArrayCollMap(
            RegressionEnvironment env,
            SupportBeanArrayCollMap @event)
        {
            env.SendEventBean(@event);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            double? doubleBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.DoubleBoxed = doubleBoxed;
            env.SendEventBean(theEvent);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            string theString)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            env.SendEventBean(theEvent);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            bool boolBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.BoolBoxed = boolBoxed;
            env.SendEventBean(theEvent);
        }

        internal class ExprCoreInNumeric : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                double?[] input = {1d, null, 1.1d, 1.0d, 1.0999999999, 2d, 4d};
                bool?[] result = {false, null, true, false, false, true, true};
                TryNumeric(env, "DoubleBoxed in (1.1d, 7/3.5, 2*6/3, 0)", input, result);

                TryNumeric(
                    env,
                    "DoubleBoxed in (7/3d, null)",
                    new double?[] {2d, 7 / 3d, null},
                    new bool?[] {null, true, null});

                TryNumeric(
                    env,
                    "DoubleBoxed in (5,5,5,5,5, -1)",
                    new double?[] {5.0, 5d, 0d, null, -1d},
                    new bool?[] {true, true, false, null, true});

                TryNumeric(
                    env,
                    "DoubleBoxed not in (1.1d, 7/3.5, 2*6/3, 0)",
                    new double?[] {1d, null, 1.1d, 1.0d, 1.0999999999, 2d, 4d},
                    new bool?[] {true, null, false, true, true, false, false});
            }
        }

        internal class ExprCoreInObject : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('stmt1') select S0.AnyObject in (ObjectArr) as value from SupportBeanArrayCollMap S0";

                env.CompileDeploy(epl).AddListener("stmt1");

                var s1 = new SupportBean_S1(100);
                var arrayBean = new SupportBeanArrayCollMap(s1);
                arrayBean.ObjectArr = new object[] {null, "a", false, s1};
                env.SendEventBean(arrayBean);
                Assert.AreEqual(true, env.Listener("stmt1").AssertOneGetNewAndReset().Get("value"));

                arrayBean.AnyObject = null;
                env.SendEventBean(arrayBean);
                Assert.IsNull(env.Listener("stmt1").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreInArraySubstitution : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select IntPrimitive in (?::int[primitive]) as result from SupportBean";
                var compiled = env.Compile(stmtText);
                env.Deploy(
                    compiled,
                    new DeploymentOptions()
                        .WithStatementSubstitutionParameter(
                            prepared => prepared.SetObject(
                                1,
                                new[] {10, 20, 30}
                            )));
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10), typeof(SupportBean).Name);
                Assert.IsTrue((bool) env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                env.SendEventBean(new SupportBean("E2", 9), typeof(SupportBean).Name);
                Assert.IsFalse((bool) env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreInCollectionArrayProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select 10 in (ArrayProperty) as result from SupportBeanComplexProps";
                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

                epl = "@Name('s1') select 5 in (ArrayProperty) as result from SupportBeanComplexProps";
                env.CompileDeploy(epl).AddListener("s1");
                env.Milestone(0);

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));
                Assert.AreEqual(false, env.Listener("s1").AssertOneGetNewAndReset().Get("result"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreInCollectionArrays : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select 1 in (IntArr, LongArr) as resOne, 1 not in (IntArr, LongArr) as resTwo from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "resOne"," resTwo" };
                SendArrayCollMap(env, new SupportBeanArrayCollMap(new[] {10, 20, 30}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(new[] {10, 1, 30}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(new[] {30}, new long?[] {20L, 1L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(new int[] { }, new long?[] {null, 1L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(null, new long?[] {1L, 100L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(null, new long?[] {0L, 100L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreInCollectionColl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "resOne"," resTwo" };
                var epl =
                    "@Name('s0') select " + 
                    " 1 in (IntCol, LongCol) as resOne," +
                    " 1 not in (LongCol, IntCol) as resTwo" +
                    " from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");

                SendArrayCollMap(env, new SupportBeanArrayCollMap(true, new[] {10, 20, 30}, null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(true, new[] {10, 20, 1}, null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(true, new[] {30}, new long?[] {20L, 1L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(true, new int[] { }, new long?[] {null, 1L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(true, null, new long?[] {1L, 100L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreInCollectionMaps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select 1 in (LongMap, IntMap) as resOne, 1 not in (LongMap, IntMap) as resTwo from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "resOne"," resTwo" };
                SendArrayCollMap(env, new SupportBeanArrayCollMap(false, new[] {10, 20, 30}, null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(false, new[] {10, 20, 1}, null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(false, new[] {30}, new long?[] {20L, 1L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(false, new int[] { }, new long?[] {null, 1L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(false, null, new long?[] {1L, 100L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreInCollectionMixed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select 1 in (LongBoxed, IntArr, LongMap, IntCol) as resOne, 1 not in (LongBoxed, IntArr, LongMap, IntCol) as resTwo from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "resOne"," resTwo" };
                SendArrayCollMap(env, new SupportBeanArrayCollMap(1L, new int[0], new long?[0], new int[0]));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(2L, null, new long?[0], new int[0]));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                SendArrayCollMap(
                    env,
                    new SupportBeanArrayCollMap(null, null, null, new[] {3, 4, 5, 6, 7, 7, 7, 8, 8, 8, 1}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                SendArrayCollMap(
                    env,
                    new SupportBeanArrayCollMap(-1L, null, new long?[] {1L}, new[] {3, 4, 5, 6, 7, 7, 7, 8, 8}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(-1L, new[] {1}, null, new int[] { }));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                env.UndeployAll();
            }
        }

        internal class ExprCoreInCollectionObjectArrayProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select 1 in (ObjectArr) as resOne, 2 in (ObjectArr) as resTwo from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = new [] { "resOne"," resTwo" };

                SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] { }));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] {1, 2}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] {1d, 2L}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});
                SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] {null, 2}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreInCollectionArrayConst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select 1 in ({1,2,3}) as resOne, 2 in ({0, 1}) as resTwo from SupportBeanArrayCollMap";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = new [] { "resOne"," resTwo" };

                SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] { }));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                env.UndeployAll();
            }
        }

        internal class ExprCoreInStringExprOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var caseExpr = "@Name('s0') select TheString in (\"a\",\"b\",\"c\") as result from " +
                               typeof(SupportBean).Name;
                var model = new EPStatementObjectModel();
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                model.SelectClause = SelectClause.Create().Add(Expressions.In("TheString", "a", "b", "c"), "result");
                model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).Name));

                TryString(
                    env,
                    model,
                    caseExpr,
                    new[] {"0", "a", "b", "c", "d", null},
                    new bool?[] {false, true, true, true, false, null});

                model = new EPStatementObjectModel();
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                model.SelectClause = SelectClause.Create().Add(Expressions.NotIn("TheString", "a", "b", "c"), "result");
                model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).Name));
                env.CopyMayFail(model);

                TryString(
                    env,
                    "TheString not in ('a', 'b', 'c')",
                    new[] {"0", "a", "b", "c", "d", null},
                    new bool?[] {true, false, false, false, true, null});
            }
        }

        internal class ExprCoreInStringExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryString(
                    env,
                    "TheString in ('a', 'b', 'c')",
                    new[] {"0", "a", "b", "c", "d", null},
                    new bool?[] {false, true, true, true, false, null});

                TryString(
                    env,
                    "TheString in ('a')",
                    new[] {"0", "a", "b", "c", "d", null},
                    new bool?[] {false, true, false, false, false, null});

                TryString(
                    env,
                    "TheString in ('a', 'b')",
                    new[] {"0", "b", "a", "c", "d", null},
                    new bool?[] {false, true, true, false, false, null});

                TryString(
                    env,
                    "TheString in ('a', null)",
                    new[] {"0", "b", "a", "c", "d", null},
                    new bool?[] {null, null, true, null, null, null});

                TryString(
                    env,
                    "TheString in (null)",
                    new[] {"0", null, "b"},
                    new bool?[] {null, null, null});

                TryString(
                    env,
                    "TheString not in ('a', 'b', 'c')",
                    new[] {"0", "a", "b", "c", "d", null},
                    new bool?[] {true, false, false, false, true, null});

                TryString(
                    env,
                    "TheString not in (null)",
                    new[] {"0", null, "b"},
                    new bool?[] {null, null, null});
            }
        }

        internal class ExprCoreBetweenBigIntBigDecExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1", "c2", "c3" };
                var epl = "@Name('s0') select " +
                          "IntPrimitive between BigIntegerHelper.ValueOf(1) and BigIntegerHelper.ValueOf(3) as c0," +
                          "IntPrimitive between 1.0m and 3.0m as c1," +
                          "IntPrimitive in (BigIntegerHelper.ValueOf(1):BigIntegerHelper.ValueOf(3)) as c2," +
                          "IntPrimitive in (1.0m:3.0m) as c3" +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E0", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, false});

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, false});

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true, true});

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, false});

                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, false});

                env.UndeployAll();
            }
        }

        internal class ExprCoreBetweenStringExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] input;
                bool?[] result;

                input = new[] {"0", "a1", "a10", "c", "d", null, "a0", "b9", "b90"};
                result = new bool?[] {false, true, true, false, false, false, true, true, false};
                TryString(env, "TheString between 'a0' and 'b9'", input, result);
                TryString(env, "TheString between 'b9' and 'a0'", input, result);

                TryString(
                    env,
                    "TheString between null and 'b9'",
                    new[] {"0", null, "a0", "b9"},
                    new bool?[] {false, false, false, false});

                TryString(
                    env,
                    "TheString between null and null",
                    new[] {"0", null, "a0", "b9"},
                    new bool?[] {false, false, false, false});

                TryString(
                    env,
                    "TheString between 'a0' and null",
                    new[] {"0", null, "a0", "b9"},
                    new bool?[] {false, false, false, false});

                input = new[] {"0", "a1", "a10", "c", "d", null, "a0", "b9", "b90"};
                result = new bool?[] {true, false, false, true, true, false, false, false, true};
                TryString(env, "TheString not between 'a0' and 'b9'", input, result);
                TryString(env, "TheString not between 'b9' and 'a0'", input, result);
            }
        }

        internal class ExprCoreBetweenNumericExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                double?[] input = {1d, null, 1.1d, 2d, 1.0999999999, 2d, 4d, 15d, 15.00001d};
                bool?[] result = {false, false, true, true, false, true, true, true, false};
                TryNumeric(env, "DoubleBoxed between 1.1 and 15", input, result);
                TryNumeric(env, "DoubleBoxed between 15 and 1.1", input, result);

                TryNumeric(
                    env,
                    "DoubleBoxed between null and 15",
                    new double?[] {1d, null, 1.1d},
                    new bool?[] {false, false, false});

                TryNumeric(
                    env,
                    "DoubleBoxed between 15 and null",
                    new double?[] {1d, null, 1.1d},
                    new bool?[] {false, false, false});

                TryNumeric(
                    env,
                    "DoubleBoxed between null and null",
                    new double?[] {1d, null, 1.1d},
                    new bool?[] {false, false, false});

                input = new double?[] {1d, null, 1.1d, 2d, 1.0999999999, 2d, 4d, 15d, 15.00001d};
                result = new bool?[] {true, false, false, false, true, false, false, false, true};
                TryNumeric(env, "DoubleBoxed not between 1.1 and 15", input, result);
                TryNumeric(env, "DoubleBoxed not between 15 and 1.1", input, result);

                TryNumeric(
                    env,
                    "DoubleBoxed not between 15 and null",
                    new double?[] {1d, null, 1.1d},
                    new bool?[] {false, false, false});
            }
        }

        internal class ExprCoreInBoolExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInBoolean(
                    env,
                    "BoolBoxed in (true, true)",
                    new[] {true, false},
                    new[] {true, false});

                TryInBoolean(
                    env,
                    "BoolBoxed in (1>2, 2=3, 4<=2)",
                    new[] {true, false},
                    new[] {false, true});

                TryInBoolean(
                    env,
                    "BoolBoxed not in (1>2, 2=3, 4<=2)",
                    new[] {true, false},
                    new[] {true, false});
            }
        }

        internal class ExprCoreInNumericCoercionLong : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select IntPrimitive in (ShortBoxed, IntBoxed, LongBoxed) as result from " +
                          typeof(SupportBean).Name;

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

                SendAndAssert4(env, 1, 2, 3, 4L, false);
                SendAndAssert4(env, 1, 1, 3, 4L, true);
                SendAndAssert4(env, 1, 3, 1, 4L, true);
                SendAndAssert4(env, 1, 3, 7, 1L, true);
                SendAndAssert2(env, 1, 3, 7, null, null);
                SendAndAssert2(env, 1, 1, null, null, true);
                SendAndAssert2(env, 1, 0, null, 1L, true);

                env.UndeployAll();
            }
        }

        internal class ExprCoreInNumericCoercionDouble : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select IntBoxed in (FloatBoxed, DoublePrimitive, LongBoxed) as result from " +
                          typeof(SupportBean).Name;
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

                SendAndAssert4(env, 1, 2f, 3d, 4L, false);
                SendAndAssert4(env, 1, 1f, 3d, 4L, true);
                SendAndAssert4(env, 1, 1.1f, 1.0d, 4L, true);
                SendAndAssert4(env, 1, 1.1f, 1.2d, 1L, true);
                SendAndAssert4(env, 1, null, 1.2d, 1L, true);
                SendAndAssert4(env, null, null, 1.2d, 1L, null);
                SendAndAssert4(env, null, 11f, 1.2d, 1L, null);

                env.UndeployAll();
            }
        }

        internal class ExprCoreBetweenNumericCoercionLong : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select IntPrimitive between ShortBoxed and LongBoxed as result from " +
                          typeof(SupportBean).Name;

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

                SendAndAssert3(env, 1, 2, 3L, false);
                SendAndAssert3(env, 2, 2, 3L, true);
                SendAndAssert3(env, 3, 2, 3L, true);
                SendAndAssert3(env, 4, 2, 3L, false);
                SendAndAssert3(env, 5, 10, 1L, true);
                SendAndAssert3(env, 1, 10, 1L, true);
                SendAndAssert3(env, 10, 10, 1L, true);
                SendAndAssert3(env, 11, 10, 1L, false);

                env.UndeployAll();
            }
        }

        internal class ExprCoreInRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "ro","rc","rho","rhc","nro","nrc","nrho","nrhc" };
                var eplOne =
                    "@Name('s0') select IntPrimitive in (2:4) as ro, IntPrimitive in [2:4] as rc, IntPrimitive in [2:4) as rho, IntPrimitive in (2:4] as rhc, " +
                    "IntPrimitive not in (2:4) as nro, IntPrimitive not in [2:4] as nrc, IntPrimitive not in [2:4) as nrho, IntPrimitive not in (2:4] as nrhc " +
                    "from SupportBean";
                env.CompileDeploy(eplOne).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, false, true, true, true, true});

                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, true, false, true, false, false, true});

                env.SendEventBean(new SupportBean("E1", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true, true, false, false, false, false});

                env.SendEventBean(new SupportBean("E1", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false, true, true, false, true, false});

                env.SendEventBean(new SupportBean("E1", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, false, true, true, true, true});

                env.UndeployAll();

                // test range reversed
                var eplTwo =
                    "@Name('s1') select IntPrimitive between 4 and 2 as r1, IntPrimitive in [4:2] as r2 from SupportBean";
                env.CompileDeployAddListenerMile(eplTwo, "s1", 1);

                fields = new [] { "r1","r2" };
                env.SendEventBean(new SupportBean("E1", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});

                env.UndeployAll();

                // test string type;
                fields = new [] { "ro" };
                var eplThree = "@Name('s2') select TheString in ('a':'d') as ro from SupportBean";
                env.CompileDeployAddListenerMile(eplThree, "s2", 2);

                env.SendEventBean(new SupportBean("a", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(new SupportBean("b", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.SendEventBean(new SupportBean("c", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.SendEventBean(new SupportBean("d", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.UndeployAll();
            }
        }

        internal class ExprCoreBetweenNumericCoercionDouble : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select IntBoxed between FloatBoxed and DoublePrimitive as result from " +
                          typeof(SupportBean).Name;
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

                SendAndAssert1(env, 1, 2f, 3d, false);
                SendAndAssert1(env, 2, 2f, 3d, true);
                SendAndAssert1(env, 3, 2f, 3d, true);
                SendAndAssert1(env, 4, 2f, 3d, false);
                SendAndAssert1(env, null, 2f, 3d, false);
                SendAndAssert1(env, null, null, 3d, false);
                SendAndAssert1(env, 1, 3f, 2d, false);
                SendAndAssert1(env, 2, 3f, 2d, true);
                SendAndAssert1(env, 3, 3f, 2d, true);
                SendAndAssert1(env, 4, 3f, 2d, false);
                SendAndAssert1(env, null, 3f, 2d, false);
                SendAndAssert1(env, null, null, 2d, false);

                env.UndeployAll();
            }
        }

        internal class ExprCoreInBetweenInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select IntArr in (1, 2, 3) as r1 from SupportBeanArrayCollMap";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'IntArr in (1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }
        }
    }
} // end of namespace