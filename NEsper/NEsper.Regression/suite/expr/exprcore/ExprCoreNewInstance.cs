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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreNewInstance
	{
		public static ICollection<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			WithKeyword(execs);
			WithStreamAlias(execs);
			WithInvalid(execs);
			WithArraySized(execs);
			WithArrayInitOneDim(execs);
			WithArrayInitTwoDim(execs);
			WithArrayInvalid(execs);
			return execs;
		}

		public static IList<RegressionExecution> WithArrayInvalid(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExecCoreNewInstanceArrayInvalid());
			return execs;
		}

		public static IList<RegressionExecution> WithArrayInitTwoDim(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExecCoreNewInstanceArrayInitTwoDim(false));
			execs.Add(new ExecCoreNewInstanceArrayInitTwoDim(true));
			return execs;
		}

		public static IList<RegressionExecution> WithArrayInitOneDim(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExecCoreNewInstanceArrayInitOneDim(false));
			execs.Add(new ExecCoreNewInstanceArrayInitOneDim(true));
			return execs;
		}

		public static IList<RegressionExecution> WithArraySized(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExecCoreNewInstanceArraySized(false));
			execs.Add(new ExecCoreNewInstanceArraySized(true));
			return execs;
		}

		public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExecCoreNewInstanceInvalid());
			return execs;
		}

		public static IList<RegressionExecution> WithStreamAlias(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExecCoreNewInstanceStreamAlias());
			return execs;
		}

		public static IList<RegressionExecution> WithKeyword(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExecCoreNewInstanceKeyword(true));
			execs.Add(new ExecCoreNewInstanceKeyword(false));
			return execs;
		}

		private class ExecCoreNewInstanceArrayInitTwoDim : RegressionExecution
		{
			bool soda;

			public ExecCoreNewInstanceArrayInitTwoDim(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select " +
				          "new char[][] {} as c0, " +
				          "new double[][] {{1}} as c1, " +
				          "new int[][] {{1},{IntPrimitive,10}} as c2, " +
				          "new float[][] {{},{1},{2.0f}} as c3, " +
				          "new long[][] {{1L,Int64.MaxValue,-1L}} as c4, " +
				          "new String[][] {} as c5, " +
				          "new String[][] {{},{},{\"x\"},{}} as c6, " +
				          "new String[][] {{\"x\",\"y\"},{\"z\"}} as c7, " +
				          "new Integer[][] {{IntPrimitive,IntPrimitive+1},{IntPrimitive+2,IntPrimitive+3}} as c8, " +
				          "new " +
				          typeof(DateTimeEx).FullName +
				          "[][] {} as c9, " +
				          "new Object[][] {{}} as c10, " +
				          "new Object[][] {{1}} as c11, " +
				          "new Object[][] {{\"x\"},{1},{10L}} as c12 " +
				          "from SupportBean";
				env.CompileDeploy(soda, epl).AddListener("s0");

				var @out = env.Statement("s0").EventType;
				Assert.AreEqual(typeof(char[][]), @out.GetPropertyType("c0"));
				Assert.AreEqual(typeof(double[][]), @out.GetPropertyType("c1"));
				Assert.AreEqual(typeof(int[][]), @out.GetPropertyType("c2"));
				Assert.AreEqual(typeof(float[][]), @out.GetPropertyType("c3"));
				Assert.AreEqual(typeof(long[][]), @out.GetPropertyType("c4"));
				Assert.AreEqual(typeof(string[][]), @out.GetPropertyType("c5"));
				Assert.AreEqual(typeof(string[][]), @out.GetPropertyType("c6"));
				Assert.AreEqual(typeof(string[][]), @out.GetPropertyType("c7"));
				Assert.AreEqual(typeof(int[][]), @out.GetPropertyType("c8"));
				Assert.AreEqual(typeof(DateTimeEx[][]), @out.GetPropertyType("c9"));
				Assert.AreEqual(typeof(object[][]), @out.GetPropertyType("c10"));
				Assert.AreEqual(typeof(object[][]), @out.GetPropertyType("c11"));
				Assert.AreEqual(typeof(object[][]), @out.GetPropertyType("c12"));

				env.SendEventBean(new SupportBean("E1", 2));
				var @event = env.Listener("s0").AssertOneGetNewAndReset();
				AssertProps(
					@event,
					"c0,c1,c2,c3,c4,c5,c6,c7,c8,c9,c10,c11,c12".SplitCsv(),
					new char[][] { },
					new[] {
						new double[] {1}
					},
					new[] {
						new[] {1},
						new[] {2, 10}
					},
					new[] {
						new float[] { },
						new float[] {1},
						new[] {2.0f}
					},
					new[] {
						new[] {1L, long.MaxValue, -1L}
					},
					new string[][] { },
					new[] {
						new string[] { },
						new string[] { },
						new[] {"x"},
						new string[] { }
					},
					new[] {
						new[] {"x", "y"},
						new[] {"z"}
					},
					new[] {
						new int?[] {2, 2 + 1},
						new int?[] {2 + 2, 2 + 3}
					},
					new DateTimeEx[][] { },
					new[] {
						new object[] { }
					},
					new[] {
						new object[] {1}
					},
					new[] {
						new object[] {"x"},
						new object[] {1},
						new object[] {10L}
					});

				env.UndeployAll();
			}
		}

		private class ExecCoreNewInstanceArrayInitOneDim : RegressionExecution
		{
			bool soda;

			public ExecCoreNewInstanceArrayInitOneDim(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9,c10,c11,c12".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpression(fields[0], "new char[] {}")
					.WithExpression(fields[1], "new double[] {1}")
					.WithExpression(fields[2], "new int[] {1,IntPrimitive,10}")
					.WithExpression(fields[3], "new float[] {1,2.0f}")
					.WithExpression(fields[4], "new long[] {1L,Int64.MaxValue,-1L}")
					.WithExpression(fields[5], "new String[] {}")
					.WithExpression(fields[6], "new String[] {\"x\"}")
					.WithExpression(fields[7], "new String[] {\"x\",\"y\"}")
					.WithExpression(fields[8], "new Integer[] {IntPrimitive,IntPrimitive+1,IntPrimitive+2,IntPrimitive+3}")
					.WithExpression(fields[9], "new " + typeof(DateTimeEx).FullName + "[] {}")
					.WithExpression(fields[10], "new Object[] {}")
					.WithExpression(fields[11], "new Object[] {1}")
					.WithExpression(fields[12], "new Object[] {\"x\",1,10L}");

				builder.WithStatementConsumer(
					stmt => {
						var @out = stmt.EventType;
						Assert.AreEqual(typeof(char[]), @out.GetPropertyType("c0"));
						Assert.AreEqual(typeof(double[]), @out.GetPropertyType("c1"));
						Assert.AreEqual(typeof(int[]), @out.GetPropertyType("c2"));
						Assert.AreEqual(typeof(float[]), @out.GetPropertyType("c3"));
						Assert.AreEqual(typeof(long[]), @out.GetPropertyType("c4"));
						Assert.AreEqual(typeof(string[]), @out.GetPropertyType("c5"));
						Assert.AreEqual(typeof(string[]), @out.GetPropertyType("c6"));
						Assert.AreEqual(typeof(string[]), @out.GetPropertyType("c7"));
						Assert.AreEqual(typeof(int[]), @out.GetPropertyType("c8"));
						Assert.AreEqual(typeof(DateTimeEx[]), @out.GetPropertyType("c9"));
						Assert.AreEqual(typeof(object[]), @out.GetPropertyType("c10"));
						Assert.AreEqual(typeof(object[]), @out.GetPropertyType("c11"));
						Assert.AreEqual(typeof(object[]), @out.GetPropertyType("c12"));
					});

				builder.WithAssertion(new SupportBean("E1", 2))
					.Expect(
						fields,
						new char[0],
						new double[] {1},
						new[] {1, 2, 10},
						new float[] {1, 2},
						new[] {1, long.MaxValue, -1},
						new string[0],
						new[] {"x"},
						new[] {"x", "y"},
						new int?[] {2, 3, 4, 5},
						new DateTimeEx[0],
						new object[0],
						new object[] {1},
						new object[] {"x", 1, 10L});

				builder.Run(env, soda);
				env.UndeployAll();
			}
		}

		private class ExecCoreNewInstanceArraySized : RegressionExecution
		{
			bool soda;

			public ExecCoreNewInstanceArraySized(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var fields = "new double[1],c1,c2,new double[1][2],c4".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpression(fields[0], "new double[1]")
					.WithExpression(fields[1], "new Integer[2*2]")
					.WithExpression(fields[2], "new " + typeof(DateTimeEx).FullName + "[IntPrimitive]")
					.WithExpression(fields[3], "new double[1][2]")
					.WithExpression(fields[4], "new " + typeof(DateTimeEx).FullName + "[IntPrimitive][IntPrimitive]");

				builder.WithStatementConsumer(
					stmt => {
						var @out = stmt.EventType;
						Assert.AreEqual(typeof(double[]), @out.GetPropertyType("new double[1]"));
						Assert.AreEqual(typeof(int[]), @out.GetPropertyType("c1"));
						Assert.AreEqual(typeof(DateTimeEx[]), @out.GetPropertyType("c2"));
						Assert.AreEqual(typeof(double[][]), @out.GetPropertyType("new double[1][2]"));
						Assert.AreEqual(typeof(DateTimeEx[][]), @out.GetPropertyType("c4"));
					});

				builder
					.WithAssertion(new SupportBean("E1", 2))
					.Expect(
						fields,
						new double[1],
						new int?[4],
						new DateTimeEx[2],
						new double[1][],
						new DateTimeEx[2][]);

				builder.Run(env, soda);
				env.UndeployAll();
			}
		}

		private class ExecCoreNewInstanceArrayInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// Dimension-provided
				//
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new double[] from SupportBean",
					"Incorrect syntax near 'from' (a reserved keyword) expecting a left curly bracket '{' but found 'from' at line 1 column 20");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new double[1, 2, 3] from SupportBean",
					"Incorrect syntax near ',' expecting a right angle bracket ']'");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new double['a'] from SupportBean",
					"Failed to validate select-clause expression 'new double[\"a\"]': New-keyword with an array-type result requires an Integer-typed dimension but received type 'System.String'");
				SupportMessageAssertUtil.TryInvalidCompile(env, "select new double[1]['a'] from SupportBean", "skip");

				// Initializers-provided
				//
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new double[] {null} from SupportBean",
					"Failed to validate select-clause expression 'new double[] {null}': Array element type mismatch: Expecting type System.Double but received null");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new String[] {1} from SupportBean",
					"Failed to validate select-clause expression 'new String[] {1}': Array element type mismatch: Expecting type System.String but received type System.Int32");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new String[] {IntPrimitive} from SupportBean",
					"Failed to validate select-clause expression 'new String[] {IntPrimitive}': Array element type mismatch: Expecting type System.String but received type System.Nullable<System.Int32>");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new String[][] {IntPrimitive} from SupportBean",
					"Failed to validate select-clause expression 'new String[] {IntPrimitive}': Two-dimensional array element does not allow element expression 'IntPrimitive'");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new String[][] {{IntPrimitive}} from SupportBean",
					"Failed to validate select-clause expression 'new String[] {{IntPrimitive}}': Array element type mismatch: Expecting type System.String but received type System.Nullable<System.Int32>");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new String[] {{'x'}} from SupportBean",
					"Failed to validate select-clause expression 'new String[] {{\"x\"}}': Array element type mismatch: Expecting type System.String but received type System.String[]");

				// Runtime null handling
				//
				var eplNullDimension = "@Name('s0') select new double[IntBoxed] from SupportBean";
				env.CompileDeploy(eplNullDimension).AddListener("s0");
				try {
					env.SendEventBean(new SupportBean());
					Assert.Fail();
				}
				catch (Exception ex) {
					// expected, rethrown
					Assert.IsTrue(ex.Message.Contains("new-array received a null value for dimension"));
				}

				env.UndeployAll();

				var eplNullValuePrimitiveArray = "@Name('s0') select new double[] {IntBoxed} from SupportBean";
				env.CompileDeploy(eplNullValuePrimitiveArray).AddListener("s0");
				try {
					env.SendEventBean(new SupportBean());
					Assert.Fail();
				}
				catch (Exception ex) {
					// expected, rethrown
					Assert.IsTrue(ex.Message.Contains("new-array received a null value"));
				}

				env.UndeployAll();
			}
		}

		private class ExecCoreNewInstanceInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var atomicLong = typeof(AtomicLong).FullName;

				// try variable
				env.CompileDeploy($"create constant variable {atomicLong} cnt = new {atomicLong}(1)");

				// try shallow invalid cases
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new Dummy() from SupportBean",
					"Failed to validate select-clause expression 'new Dummy()': Failed to resolve new-operator class name 'Dummy'");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select new SupportPrivateCtor() from SupportBean",
					"Failed to validate select-clause expression 'new SupportPrivateCtor()': Failed to find a suitable constructor for class ");

				env.UndeployAll();
			}
		}

		private class ExecCoreNewInstanceStreamAlias : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean", "sb")
					.WithExpressions(fields, "new SupportObjectCtor(sb)");

				var sb = new SupportBean();
				builder.WithAssertion(sb).Verify("c0", result => Assert.AreSame(sb, ((SupportObjectCtor) result).Object));

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private class ExecCoreNewInstanceKeyword : RegressionExecution
		{
			private readonly bool soda;

			public ExecCoreNewInstanceKeyword(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select " +
				          "new SupportBean(\"A\",IntPrimitive) as c0, " +
				          "new SupportBean(\"B\",IntPrimitive+10), " +
				          "new SupportBean() as c2, " +
				          "new SupportBean(\"ABC\",0).GetTheString() as c3 " +
				          "from SupportBean";
				env.CompileDeploy(soda, epl).AddListener("s0");
				var expectedAggType = new[] {
					new object[] {"c0", typeof(SupportBean)},
					new object[] {"new SupportBean(\"B\",IntPrimitive+10)", typeof(SupportBean)}
				};
				SupportEventTypeAssertionUtil.AssertEventTypeProperties(
					expectedAggType,
					env.Statement("s0").EventType,
					SupportEventTypeAssertionEnum.NAME,
					SupportEventTypeAssertionEnum.TYPE);

				env.SendEventBean(new SupportBean("E1", 10));
				var @event = env.Listener("s0").AssertOneGetNewAndReset();
				AssertSupportBean(@event.Get("c0"), new object[] {"A", 10});
				AssertSupportBean(@event.Underlying.AsStringDictionary().Get("new SupportBean(\"B\",IntPrimitive+10)"), new object[] {"B", 20});
				AssertSupportBean(@event.Get("c2"), new object[] {null, 0});
				Assert.AreEqual("ABC", @event.Get("c3"));

				env.UndeployAll();
			}

			private void AssertSupportBean(
				object bean,
				object[] objects)
			{
				var b = (SupportBean) bean;
				Assert.AreEqual(objects[0], b.TheString);
				Assert.AreEqual(objects[1], b.IntPrimitive);
			}
		}
	}
} // end of namespace
