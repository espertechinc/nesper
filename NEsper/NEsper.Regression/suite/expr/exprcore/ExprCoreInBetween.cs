///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreInBetween
	{
		public static ICollection<RegressionExecution> Executions()
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

		private class ExprCoreInNumeric : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var input = new double?[] {1d, null, 1.1d, 1.0d, 1.0999999999, 2d, 4d};
				var result = new bool?[] {false, null, true, false, false, true, true};
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

		private class ExprCoreInObject : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBeanArrayCollMap")
					.WithExpressions(fields, "anyObject in (objectArr)");

				builder.WithAssertion(Make()).Expect(fields, true);

				var arrayBean = Make();
				arrayBean.AnyObject = null;
				builder.WithAssertion(arrayBean).Expect(fields, new object[] {null});

				builder.Run(env);
				env.UndeployAll();
			}

			private static SupportBeanArrayCollMap Make()
			{
				var s1 = new SupportBean_S1(100);
				var arrayBean = new SupportBeanArrayCollMap(s1);
				arrayBean.ObjectArr = new object[] {null, "a", false, s1};
				return arrayBean;
			}
		}

		private class ExprCoreInArraySubstitution : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select IntPrimitive in (?::int[primitive]) as result from SupportBean";
				var compiled = env.Compile(stmtText);
				env.Deploy(
					compiled,
					new DeploymentOptions()
						.WithStatementSubstitutionParameter(
							prepared => prepared
								.SetObject(1, new[] {10, 20, 30})));
				env.AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 10), typeof(SupportBean).Name);
				Assert.IsTrue((Boolean) env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

				env.SendEventBean(new SupportBean("E2", 9), typeof(SupportBean).Name);
				Assert.IsFalse((Boolean) env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

				env.UndeployAll();
			}
		}

		private class ExprCoreInCollectionArrayProp : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var epl = "@name('s0') select 10 in (arrayProperty) as result from SupportBeanComplexProps";
				env.CompileDeploy(epl).AddListener("s0");
				Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

				epl = "@name('s1') select 5 in (arrayProperty) as result from SupportBeanComplexProps";
				env.CompileDeploy(epl).AddListener("s1");
				env.Milestone(0);

				env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
				Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));
				Assert.AreEqual(false, env.Listener("s1").AssertOneGetNewAndReset().Get("result"));

				env.UndeployAll();
			}
		}

		private class ExprCoreInCollectionArrays : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var epl = "@name('s0') select 1 in (intArr, longArr) as resOne, 1 not in (intArr, longArr) as resTwo from SupportBeanArrayCollMap";
				env.CompileDeploy(epl).AddListener("s0");

				var fields = "resOne, resTwo".SplitCsv();
				SendArrayCollMap(env, new SupportBeanArrayCollMap(new[] {10, 20, 30}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(new[] {10, 1, 30}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(new[] {30}, new long?[] {20L, 1L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(new int[] { }, new long?[] {null, 1L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(null, new long?[] {1L, 100L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(null, new long?[] {0L, 100L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);

				env.UndeployAll();
			}
		}

		private class ExprCoreInCollectionColl : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var fields = "resOne, resTwo".SplitCsv();
				var epl = "@name('s0') select 1 in (intCol, longCol) as resOne, 1 not in (longCol, intCol) as resTwo from SupportBeanArrayCollMap";
				env.CompileDeploy(epl).AddListener("s0");

				SendArrayCollMap(env, new SupportBeanArrayCollMap(true, new[] {10, 20, 30}, null));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(true, new[] {10, 20, 1}, null));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(true, new[] {30}, new long?[] {20L, 1L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(true, new int[] { }, new long?[] {null, 1L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(true, null, new long?[] {1L, 100L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);

				env.UndeployAll();
			}
		}

		private class ExprCoreInCollectionMaps : RegressionExecution
		{
			public bool IsExcludeWhenInstrumented => true;

			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select 1 in (longMap, intMap) as resOne, 1 not in (longMap, intMap) as resTwo from SupportBeanArrayCollMap";
				env.CompileDeploy(epl).AddListener("s0");

				var fields = "resOne, resTwo".SplitCsv();
				SendArrayCollMap(env, new SupportBeanArrayCollMap(false, new[] {10, 20, 30}, null));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(false, new[] {10, 20, 1}, null));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(false, new[] {30}, new long?[] {20L, 1L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(false, new int[] { }, new long?[] {null, 1L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(false, null, new long?[] {1L, 100L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);

				env.UndeployAll();
			}
		}

		private class ExprCoreInCollectionMixed : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var epl =
					"@name('s0') select 1 in (longBoxed, intArr, longMap, intCol) as resOne, 1 not in (LongBoxed, intArr, longMap, intCol) as resTwo from SupportBeanArrayCollMap";
				env.CompileDeploy(epl).AddListener("s0");

				var fields = "resOne, resTwo".SplitCsv();
				SendArrayCollMap(env, new SupportBeanArrayCollMap(1L, new int[0], new long?[0], new int[0]));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(2L, null, new long?[0], new int[0]));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);

				SendArrayCollMap(env, new SupportBeanArrayCollMap(null, null, null, new[] {3, 4, 5, 6, 7, 7, 7, 8, 8, 8, 1}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);

				SendArrayCollMap(env, new SupportBeanArrayCollMap(-1L, null, new long?[] {1L}, new[] {3, 4, 5, 6, 7, 7, 7, 8, 8}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, true);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(-1L, new[] {1}, null, new int[] { }));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);

				env.UndeployAll();
			}
		}

		private class ExprCoreInCollectionObjectArrayProp : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select 1 in (objectArr) as resOne, 2 in (objectArr) as resTwo from SupportBeanArrayCollMap";
				env.CompileDeploy(epl).AddListener("s0");
				var fields = "resOne, resTwo".SplitCsv();

				SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] { }));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, false);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] {1, 2}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, true);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] {1d, 2L}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, false, false);
				SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] {null, 2}));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, null, true);

				env.UndeployAll();
			}
		}

		private class ExprCoreInCollectionArrayConst : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select 1 in ({1,2,3}) as resOne, 2 in ({0, 1}) as resTwo from SupportBeanArrayCollMap";
				env.CompileDeploy(epl).AddListener("s0");
				var fields = "resOne, resTwo".SplitCsv();

				SendArrayCollMap(env, new SupportBeanArrayCollMap(new object[] { }));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, true, false);

				env.UndeployAll();
			}
		}

		private class ExprCoreInStringExprOM : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var caseExpr = "@name('s0') select TheString in (\"a\",\"b\",\"c\") as result from " + typeof(SupportBean).Name;
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
				SerializableObjectCopier.GetInstance(env.Container).Copy(model);

				TryString(
					env,
					"TheString not in ('a', 'b', 'c')",
					new[] {"0", "a", "b", "c", "d", null},
					new bool?[] {true, false, false, false, true, null});
			}
		}

		private class ExprCoreInStringExpr : RegressionExecution
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

		private class ExprCoreBetweenBigIntBigDecExpr : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpression(fields[0], "IntPrimitive between BigInteger.valueOf(1) and BigInteger.valueOf(3)")
					.WithExpression(fields[1], "IntPrimitive between BigDecimal.valueOf(1) and BigDecimal.valueOf(3)")
					.WithExpression(fields[2], "IntPrimitive in (BigInteger.valueOf(1):BigInteger.valueOf(3))")
					.WithExpression(fields[3], "IntPrimitive in (BigDecimal.valueOf(1):BigDecimal.valueOf(3))");

				builder.WithAssertion(new SupportBean("E0", 0)).Expect(fields, false, false, false, false);

				builder.WithAssertion(new SupportBean("E1", 1)).Expect(fields, true, true, false, false);

				builder.WithAssertion(new SupportBean("E2", 2)).Expect(fields, true, true, true, true);

				builder.WithAssertion(new SupportBean("E3", 3)).Expect(fields, true, true, false, false);

				builder.WithAssertion(new SupportBean("E4", 4)).Expect(fields, false, false, false, false);

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private class ExprCoreBetweenStringExpr : RegressionExecution
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

		private class ExprCoreBetweenNumericExpr : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var input = new double?[] {1d, null, 1.1d, 2d, 1.0999999999, 2d, 4d, 15d, 15.00001d};
				var result = new bool?[] {false, false, true, true, false, true, true, true, false};
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

		private class ExprCoreInBoolExpr : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryInBoolean(
					env,
					"BoolBoxed in (true, true)",
					new bool?[] {true, false},
					new[] {true, false});

				TryInBoolean(
					env,
					"BoolBoxed in (1>2, 2=3, 4<=2)",
					new bool?[] {true, false},
					new[] {false, true});

				TryInBoolean(
					env,
					"BoolBoxed not in (1>2, 2=3, 4<=2)",
					new bool?[] {true, false},
					new[] {true, false});
			}
		}

		private class ExprCoreInNumericCoercionLong : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select IntPrimitive in (ShortBoxed, IntBoxed, LongBoxed) as result from " + typeof(SupportBean).Name;

				env.CompileDeploy(epl).AddListener("s0");
				Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

				SendAndAssert(env, 1, 2, 3, 4L, false);
				SendAndAssert(env, 1, 1, 3, 4L, true);
				SendAndAssert(env, 1, 3, 1, 4L, true);
				SendAndAssert(env, 1, 3, 7, 1L, true);
				SendAndAssert(env, 1, 3, 7, null, null);
				SendAndAssert(env, 1, 1, null, null, true);
				SendAndAssert(env, 1, 0, null, 1L, true);

				env.UndeployAll();
			}
		}

		private class ExprCoreInNumericCoercionDouble : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select IntBoxed in (FloatBoxed, DoublePrimitive, LongBoxed) as result from " + typeof(SupportBean).Name;
				env.CompileDeploy(epl).AddListener("s0");

				Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

				SendAndAssert(env, 1, 2f, 3d, 4L, false);
				SendAndAssert(env, 1, 1f, 3d, 4L, true);
				SendAndAssert(env, 1, 1.1f, 1.0d, 4L, true);
				SendAndAssert(env, 1, 1.1f, 1.2d, 1L, true);
				SendAndAssert(env, 1, null, 1.2d, 1L, true);
				SendAndAssert(env, null, null, 1.2d, 1L, null);
				SendAndAssert(env, null, 11f, 1.2d, 1L, null);

				env.UndeployAll();
			}
		}

		private class ExprCoreBetweenNumericCoercionLong : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpressions(fields, "IntPrimitive between ShortBoxed and LongBoxed")
					.WithStatementConsumer(stmt => Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("c0")));

				builder.WithAssertion(MakeBean(1, 2, 3L)).Expect(fields, false);
				builder.WithAssertion(MakeBean(2, 2, 3L)).Expect(fields, true);
				builder.WithAssertion(MakeBean(3, 2, 3L)).Expect(fields, true);
				builder.WithAssertion(MakeBean(4, 2, 3L)).Expect(fields, false);
				builder.WithAssertion(MakeBean(5, 10, 1L)).Expect(fields, true);
				builder.WithAssertion(MakeBean(1, 10, 1L)).Expect(fields, true);
				builder.WithAssertion(MakeBean(10, 10, 1L)).Expect(fields, true);
				builder.WithAssertion(MakeBean(11, 10, 1L)).Expect(fields, false);

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private class ExprCoreInRange : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var fields = "ro,rc,rho,rhc,nro,nrc,nrho,nrhc".SplitCsv();
				var eplOne =
					"@name('s0') select IntPrimitive in (2:4) as ro, IntPrimitive in [2:4] as rc, IntPrimitive in [2:4) as rho, IntPrimitive in (2:4] as rhc, " +
					"IntPrimitive not in (2:4) as nro, IntPrimitive not in [2:4] as nrc, IntPrimitive not in [2:4) as nrho, IntPrimitive not in (2:4] as nrhc " +
					"from SupportBean";
				env.CompileDeploy(eplOne).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fields,
					false,
					false,
					false,
					false,
					true,
					true,
					true,
					true);

				env.SendEventBean(new SupportBean("E1", 2));
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fields,
					false,
					true,
					true,
					false,
					true,
					false,
					false,
					true);

				env.SendEventBean(new SupportBean("E1", 3));
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fields,
					true,
					true,
					true,
					true,
					false,
					false,
					false,
					false);

				env.SendEventBean(new SupportBean("E1", 4));
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fields,
					false,
					true,
					false,
					true,
					true,
					false,
					true,
					false);

				env.SendEventBean(new SupportBean("E1", 5));
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fields,
					false,
					false,
					false,
					false,
					true,
					true,
					true,
					true);

				env.UndeployAll();

				var model = env.EplToModel(eplOne);
				var epl = model.ToEPL();
				epl = epl.Replace("IntPrimitive between 2 and 4 as rc", "IntPrimitive in [2:4] as rc");
				epl = epl.Replace("IntPrimitive not between 2 and 4 as nrc", "IntPrimitive not in [2:4] as nrc");
				Assert.AreEqual(eplOne, epl);

				// test range reversed
				var eplTwo = "@name('s1') select IntPrimitive between 4 and 2 as r1, IntPrimitive in [4:2] as r2 from SupportBean";
				env.CompileDeployAddListenerMile(eplTwo, "s1", 1);

				fields = "r1,r2".SplitCsv();
				env.SendEventBean(new SupportBean("E1", 3));
				EPAssertionUtil.AssertProps(env.Listener("s1").AssertOneGetNewAndReset(), fields, true, true);

				env.UndeployAll();

				// test string type;
				fields = "ro".SplitCsv();
				var eplThree = "@name('s2') select TheString in ('a':'d') as ro from SupportBean";
				env.CompileDeployAddListenerMile(eplThree, "s2", 2);

				env.SendEventBean(new SupportBean("a", 5));
				EPAssertionUtil.AssertProps(env.Listener("s2").AssertOneGetNewAndReset(), fields, false);

				env.SendEventBean(new SupportBean("b", 5));
				EPAssertionUtil.AssertProps(env.Listener("s2").AssertOneGetNewAndReset(), fields, true);

				env.SendEventBean(new SupportBean("c", 5));
				EPAssertionUtil.AssertProps(env.Listener("s2").AssertOneGetNewAndReset(), fields, true);

				env.SendEventBean(new SupportBean("d", 5));
				EPAssertionUtil.AssertProps(env.Listener("s2").AssertOneGetNewAndReset(), fields, false);

				env.UndeployAll();
			}
		}

		private class ExprCoreBetweenNumericCoercionDouble : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select IntBoxed between FloatBoxed and DoublePrimitive as result from " + typeof(SupportBean).Name;
				env.CompileDeploy(epl).AddListener("s0");

				Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("result"));

				SendAndAssert(env, 1, 2f, 3d, false);
				SendAndAssert(env, 2, 2f, 3d, true);
				SendAndAssert(env, 3, 2f, 3d, true);
				SendAndAssert(env, 4, 2f, 3d, false);
				SendAndAssert(env, null, 2f, 3d, false);
				SendAndAssert(env, null, null, 3d, false);
				SendAndAssert(env, 1, 3f, 2d, false);
				SendAndAssert(env, 2, 3f, 2d, true);
				SendAndAssert(env, 3, 3f, 2d, true);
				SendAndAssert(env, 4, 3f, 2d, false);
				SendAndAssert(env, null, 3f, 2d, false);
				SendAndAssert(env, null, null, 2d, false);

				env.UndeployAll();
			}
		}

		private class ExprCoreInBetweenInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "select intArr in (1, 2, 3) as r1 from SupportBeanArrayCollMap";
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					epl,
					"Failed to validate select-clause expression 'intArr in (1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
			}
		}

		private static void SendAndAssert(
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

		private static void SendAndAssert(
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

		private static SupportBean MakeBean(
			int intPrimitive,
			int shortBoxed,
			long? longBoxed)
		{
			var bean = new SupportBean();
			bean.IntPrimitive = intPrimitive;
			bean.ShortBoxed = (short) shortBoxed;
			bean.LongBoxed = longBoxed;
			return bean;
		}

		private static void SendAndAssert(
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
			bool?[] input,
			bool[] result)
		{
			var fields = "c0".SplitCsv();
			var builder = new SupportEvalBuilder("SupportBean")
				.WithExpressions(fields, expr)
				.WithStatementConsumer(stmt => Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("c0")));

			for (var i = 0; i < input.Length; i++) {
				builder.WithAssertion(MakeSupportBeanEvent(input[i])).Expect(fields, result[i]);
			}

			builder.Run(env);
			env.UndeployAll();
		}

		private static void TryString(
			RegressionEnvironment env,
			string expression,
			string[] input,
			bool?[] result)
		{
			var fields = "c0".SplitCsv();
			var builder = new SupportEvalBuilder("SupportBean")
				.WithExpressions(fields, expression)
				.WithStatementConsumer(stmt => Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("c0")));

			for (var i = 0; i < input.Length; i++) {
				builder.WithAssertion(new SupportBean(input[i], i)).Expect(fields, result[i]);
			}

			builder.Run(env);
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
			objectmodel = SerializableObjectCopier.GetInstance(env.Container).Copy(objectmodel);
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
			var fields = "c0".SplitCsv();
			var builder = new SupportEvalBuilder("SupportBean")
				.WithExpressions(fields, expr)
				.WithStatementConsumer(stmt => Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("c0")));

			for (var i = 0; i < input.Length; i++) {
				builder.WithAssertion(MakeSupportBeanEvent(input[i])).Expect(fields, result[i]);
			}

			builder.Run(env);
			env.UndeployAll();
		}

		private static void SendArrayCollMap(
			RegressionEnvironment env,
			SupportBeanArrayCollMap @event)
		{
			env.SendEventBean(@event);
		}

		private static SupportBean MakeSupportBeanEvent(double? doubleBoxed)
		{
			var theEvent = new SupportBean();
			theEvent.DoubleBoxed = doubleBoxed;
			return theEvent;
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
			env.SendEventBean(MakeSupportBeanEvent(boolBoxed));
		}

		private static SupportBean MakeSupportBeanEvent(bool? boolBoxed)
		{
			var theEvent = new SupportBean();
			theEvent.BoolBoxed = boolBoxed;
			return theEvent;
		}
	}
} // end of namespace
