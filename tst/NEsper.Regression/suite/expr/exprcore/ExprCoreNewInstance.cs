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

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil; // assertProps
using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreNewInstance {

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> executions = new List<RegressionExecution>();
	        executions.Add(new ExecCoreNewInstanceKeyword(true));
	        executions.Add(new ExecCoreNewInstanceKeyword(false));
	        executions.Add(new ExecCoreNewInstanceStreamAlias());
	        executions.Add(new ExecCoreNewInstanceGeneric(false));
	        executions.Add(new ExecCoreNewInstanceGeneric(true));
	        executions.Add(new ExecCoreNewInstanceInvalid());
	        executions.Add(new ExecCoreNewInstanceArraySized(false));
	        executions.Add(new ExecCoreNewInstanceArraySized(true));
	        executions.Add(new ExecCoreNewInstanceArrayInitOneDim(false));
	        executions.Add(new ExecCoreNewInstanceArrayInitOneDim(true));
	        executions.Add(new ExecCoreNewInstanceArrayInitTwoDim(false));
	        executions.Add(new ExecCoreNewInstanceArrayInitTwoDim(true));
	        executions.Add(new ExecCoreNewInstanceArrayInvalid());
	        return executions;
	    }

	    private class ExecCoreNewInstanceArrayInitTwoDim : RegressionExecution {
	        bool soda;

	        public ExecCoreNewInstanceArrayInitTwoDim(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select " +
	                      "new char[][] {} as c0, " +
	                      "new double[][] {{1}} as c1, " +
	                      "new int[][] {{1},{intPrimitive,10}} as c2, " +
	                      "new float[][] {{},{1},{2.0f}} as c3, " +
	                      "new long[][] {{1L,Long.MaxValue,-1L}} as c4, " +
	                      "new String[][] {} as c5, " +
	                      "new String[][] {{},{},{\"x\"},{}} as c6, " +
	                      "new String[][] {{\"x\",\"y\"},{\"z\"}} as c7, " +
	                      "new Integer[][] {{intPrimitive,intPrimitive+1},{intPrimitive+2,intPrimitive+3}} as c8, " +
	                      $"new {typeof(DateTimeEx).FullName}[][] {{}} as c9, " +
	                      "new Object[][] {{}} as c10, " +
	                      "new Object[][] {{1}} as c11, " +
	                      "new Object[][] {{\"x\"},{1},{10L}} as c12 " +
	                      "from SupportBean";
	            env.CompileDeploy(soda, epl).AddListener("s0");

	            env.AssertStatement("s0", statement => {
	                var @out = statement.EventType;
	                Assert.AreEqual(typeof(char[][]), @out.GetPropertyType("c0"));
	                Assert.AreEqual(typeof(double[][]), @out.GetPropertyType("c1"));
	                Assert.AreEqual(typeof(int[][]), @out.GetPropertyType("c2"));
	                Assert.AreEqual(typeof(float[][]), @out.GetPropertyType("c3"));
	                Assert.AreEqual(typeof(long[][]), @out.GetPropertyType("c4"));
	                Assert.AreEqual(typeof(string[][]), @out.GetPropertyType("c5"));
	                Assert.AreEqual(typeof(string[][]), @out.GetPropertyType("c6"));
	                Assert.AreEqual(typeof(string[][]), @out.GetPropertyType("c7"));
	                Assert.AreEqual(typeof(int?[][]), @out.GetPropertyType("c8"));
	                Assert.AreEqual(typeof(DateTimeEx[][]), @out.GetPropertyType("c9"));
	                Assert.AreEqual(typeof(object[][]), @out.GetPropertyType("c10"));
	                Assert.AreEqual(typeof(object[][]), @out.GetPropertyType("c11"));
	                Assert.AreEqual(typeof(object[][]), @out.GetPropertyType("c12"));
	            });

	            env.SendEventBean(new SupportBean("E1", 2));
	            env.AssertEventNew("s0", @event => {
	                AssertProps(@event, "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9,c10,c11,c12".SplitCsv(),
	                    new object[]{
		                    new char[][]{},
		                    new double[][]{new double[] {1}},
		                    new int[][]{new int[]{1}, new int[] {2, 10}},
	                        new float[][]{new float[]{}, new float[] {1}, new float[] {2.0f}},
	                        new long[][]{new long[]{1L, long.MaxValue, -1L}},
	                        new string[][]{},
	                        new string[][]{new string[] {}, new string[] {}, new string[] {"x"}, new string[] {}},
	                        new string[][]{new string[]{"x", "y"}, new string[] {"z"}},
	                        new int?[][]{new int?[]{2, 2 + 1}, new int?[] {2 + 2, 2 + 3}},
	                        new DateTimeEx[][]{},
	                        new object[][]{new object[] {}}, new object[][]{new object[] {1}},
	                        new object[][]{new object[] {"x"}, new object[] {1}, new object[] {10L}}
	                    });
	            });

	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    private class ExecCoreNewInstanceArrayInitOneDim : RegressionExecution {
	        bool soda;

	        public ExecCoreNewInstanceArrayInitOneDim(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9,c10,c11,c12".SplitCsv();
	            var builder = new SupportEvalBuilder("SupportBean")
	                .WithExpression(fields[0], "new char[] {}")
	                .WithExpression(fields[1], "new double[] {1}")
	                .WithExpression(fields[2], "new int[] {1,intPrimitive,10}")
	                .WithExpression(fields[3], "new float[] {1,2.0f}")
	                .WithExpression(fields[4], "new long[] {1L,Long.MAX_VALUE,-1L}")
	                .WithExpression(fields[5], "new String[] {}")
	                .WithExpression(fields[6], "new String[] {\"x\"}")
	                .WithExpression(fields[7], "new String[] {\"x\",\"y\"}")
	                .WithExpression(fields[8], "new Integer[] {intPrimitive,intPrimitive+1,intPrimitive+2,intPrimitive+3}")
	                .WithExpression(fields[9], "new java.util.Calendar[] {}")
	                .WithExpression(fields[10], "new Object[] {}")
	                .WithExpression(fields[11], "new Object[] {1}")
	                .WithExpression(fields[12], "new Object[] {\"x\",1,10L}");

	            builder.WithStatementConsumer(stmt => {
	                var @out = stmt.EventType;
	                Assert.AreEqual(typeof(char[]), @out.GetPropertyType("c0"));
	                Assert.AreEqual(typeof(double[]), @out.GetPropertyType("c1"));
	                Assert.AreEqual(typeof(int[]), @out.GetPropertyType("c2"));
	                Assert.AreEqual(typeof(float[]), @out.GetPropertyType("c3"));
	                Assert.AreEqual(typeof(long[]), @out.GetPropertyType("c4"));
	                Assert.AreEqual(typeof(string[]), @out.GetPropertyType("c5"));
	                Assert.AreEqual(typeof(string[]), @out.GetPropertyType("c6"));
	                Assert.AreEqual(typeof(string[]), @out.GetPropertyType("c7"));
	                Assert.AreEqual(typeof(int?[]), @out.GetPropertyType("c8"));
	                Assert.AreEqual(typeof(DateTimeEx[]), @out.GetPropertyType("c9"));
	                Assert.AreEqual(typeof(object[]), @out.GetPropertyType("c10"));
	                Assert.AreEqual(typeof(object[]), @out.GetPropertyType("c11"));
	                Assert.AreEqual(typeof(object[]), @out.GetPropertyType("c12"));
	            });

	            builder
		            .WithAssertion(new SupportBean("E1", 2))
		            .Expect(fields,
			            Array.Empty<char>(), new double[]{1}, new int[]{1, 2, 10},
		                new float[]{1, 2}, new long[]{1, long.MaxValue, -1}, Array.Empty<string>(),
		                new string[]{"x"}, new string[]{"x", "y"}, new int?[]{2, 3, 4, 5},
		                Array.Empty<DateTimeEx>(), Array.Empty<object>(), new object[]{1}, new object[]{"x", 1, 10L});

	            builder.Run(env, soda);
	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    private class ExecCoreNewInstanceGeneric : RegressionExecution {
	        bool soda;

	        public ExecCoreNewInstanceGeneric(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
	            var builder = new SupportEvalBuilder("SupportBean")
	                .WithExpression(fields[0], "new ArrayList<String>()")
	                .WithExpression(fields[1], "new HashMap<String,Integer>()")
	                .WithExpression(fields[2], "new ArrayList<String>(20)")
	                .WithExpression(fields[3], "new ArrayList<String>[5]")
	                .WithExpression(fields[4], "new ArrayList<String>[] {new ArrayList<String>(),new ArrayList<String>()}")
	                .WithExpression(fields[5], "new ArrayList<String[][]>[2][]");

	            builder.WithStatementConsumer(stmt => {
	                var @out = stmt.EventType;
	                Assert.AreEqual(typeof(List<string>), @out.GetPropertyType("c0"));
	                Assert.AreEqual(typeof(Dictionary<string, int>), @out.GetPropertyType("c1"));
	                Assert.AreEqual(typeof(List<string>), @out.GetPropertyType("c2"));
	                Assert.AreEqual(typeof(List<string>[]), @out.GetPropertyType("c3"));
	                Assert.AreEqual(typeof(List<string>[]), @out.GetPropertyType("c4"));
	                Assert.AreEqual(typeof(List<string>[][]), @out.GetPropertyType("c5"));
	            });

	            builder.WithAssertion(new SupportBean("E1", 2))
	                .Verify("c0", value => Assert.IsTrue(value is List<object>))
	                .Verify("c1", value => Assert.IsTrue(value is Dictionary<string, object>))
	                .Verify("c2", value => Assert.IsTrue(value is List))
	                .Verify("c3", value => {
	                    var array = (List<string>[]) value;
	                    Assert.AreEqual(5, array.Length);
	                })
	                .Verify("c4", value => {
	                    var array = (List<string>[]) value;
	                    Assert.AreEqual(2, array.Length);
	                    for (var i = 0; i < 2; i++) {
	                        Assert.IsTrue(array[i] is List<string>);
	                    }
	                })
	                .Verify("c5", value => {
	                    var array = (List<string>[][]) value;
	                    Assert.AreEqual(2, array.Length);
	                });

	            builder.Run(env, soda);
	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    private class ExecCoreNewInstanceArraySized : RegressionExecution {
	        bool soda;

	        public ExecCoreNewInstanceArraySized(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env)
	        {
		        var dateTimeEx = typeof(DateTimeEx).FullName;
	            var fields = "new double[1],c1,c2,new double[1][2],c4".SplitCsv();
	            var builder = new SupportEvalBuilder("SupportBean")
	                .WithExpression(fields[0], "new double[1]")
	                .WithExpression(fields[1], "new Integer[2*2]")
	                .WithExpression(fields[2], $"new {dateTimeEx}[intPrimitive]")
	                .WithExpression(fields[3], "new double[1][2]")
	                .WithExpression(fields[4], $"new {dateTimeEx}[intPrimitive][intPrimitive]");

	            builder.WithStatementConsumer(stmt => {
	                var @out = stmt.EventType;
	                Assert.AreEqual(typeof(double[]), @out.GetPropertyType("new double[1]"));
	                Assert.AreEqual(typeof(int?[]), @out.GetPropertyType("c1"));
	                Assert.AreEqual(typeof(DateTimeEx[]), @out.GetPropertyType("c2"));
	                Assert.AreEqual(typeof(double[][]), @out.GetPropertyType("new double[1][2]"));
	                Assert.AreEqual(typeof(DateTimeEx[][]), @out.GetPropertyType("c4"));
	            });

	            builder.WithAssertion(new SupportBean("E1", 2)).Expect(fields, new double[1], new int?[4], new DateTimeEx[2], new double[1][], new DateTimeEx[2][]);

	            builder.Run(env, soda);
	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    private class ExecCoreNewInstanceArrayInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // Dimension-provided
	            //
	            env.TryInvalidCompile("select new double[] from SupportBean",
	                "Incorrect syntax near 'from' (a reserved keyword) expecting a left curly bracket '{' but found 'from' at line 1 column 20");

	            env.TryInvalidCompile("select new double[1, 2, 3] from SupportBean",
	                "Incorrect syntax near ',' expecting a right angle bracket ']'");

	            env.TryInvalidCompile("select new double['a'] from SupportBean",
	                "Failed to validate select-clause expression 'new double[\"a\"]': New-keyword with an array-type result requires an Integer-typed dimension but received type 'String'");
	            env.TryInvalidCompile("select new double[1]['a'] from SupportBean", "skip");

	            // Initializers-provided
	            //
	            env.TryInvalidCompile("select new double[] {null} from SupportBean",
	                "Failed to validate select-clause expression 'new double[] {null}': Array element type mismatch: Expecting type double but received null");

	            env.TryInvalidCompile("select new String[] {1} from SupportBean",
	                "Failed to validate select-clause expression 'new String[] {1}': Array element type mismatch: Expecting type String but received type int");

	            env.TryInvalidCompile("select new String[] {intPrimitive} from SupportBean",
	                "Failed to validate select-clause expression 'new String[] {intPrimitive}': Array element type mismatch: Expecting type String but received type Integer");

	            env.TryInvalidCompile("select new String[][] {intPrimitive} from SupportBean",
	                "Failed to validate select-clause expression 'new String[] {intPrimitive}': Two-dimensional array element does not allow element expression 'intPrimitive'");

	            env.TryInvalidCompile("select new String[][] {{intPrimitive}} from SupportBean",
	                "Failed to validate select-clause expression 'new String[] {{intPrimitive}}': Array element type mismatch: Expecting type String but received type Integer");

	            env.TryInvalidCompile("select new String[] {{'x'}} from SupportBean",
	                "Failed to validate select-clause expression 'new String[] {{\"x\"}}': Array element type mismatch: Expecting type String but received type String[]");

	            // Runtime null handling
	            //
	            var eplNullDimension = "@name('s0') select new double[intBoxed] from SupportBean";
	            env.CompileDeploy(eplNullDimension).AddListener("s0");
	            try {
	                env.SendEventBean(new SupportBean());
	                Assert.Fail();
	            } catch (Exception ex) {
	                // expected, rethrown
	                Assert.IsTrue(ex.Message.Contains("new-array received a null value for dimension"));
	            }
	            env.UndeployAll();

	            var eplNullValuePrimitiveArray = "@name('s0') select new double[] {intBoxed} from SupportBean";
	            env.CompileDeploy(eplNullValuePrimitiveArray).AddListener("s0");
	            try {
	                env.SendEventBean(new SupportBean());
	                Assert.Fail();
	            } catch (Exception ex) {
	                // expected, rethrown
	                Assert.IsTrue(ex.Message.Contains("new-array received a null value"));
	            }
	            env.UndeployAll();
	        }
	    }

	    private class ExecCoreNewInstanceInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // try variable
	            env.CompileDeploy("create constant variable java.util.concurrent.atomic.AtomicLong cnt = new java.util.concurrent.atomic.AtomicLong(1)");

	            // try shallow invalid cases
	            env.TryInvalidCompile("select new Dummy() from SupportBean",
	                "Failed to validate select-clause expression 'new Dummy()': Failed to resolve type parameter 'Dummy'");

	            env.TryInvalidCompile("select new SupportPrivateCtor() from SupportBean",
	                "Failed to validate select-clause expression 'new SupportPrivateCtor()': Failed to find a suitable constructor for class ");

	            env.UndeployAll();
	        }
	    }

	    private class ExecCoreNewInstanceStreamAlias : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "c0".SplitCsv();
	            var builder = new SupportEvalBuilder("SupportBean", "sb")
	                .WithExpressions(fields, "new SupportObjectCtor(sb)");

	            var sb = new SupportBean();
	            builder.WithAssertion(sb).Verify("c0", result => Assert.AreSame(sb, ((SupportObjectCtor) result).Object));

	            builder.Run(env);
	            env.UndeployAll();
	        }
	    }

	    private class ExecCoreNewInstanceKeyword : RegressionExecution {
	        private readonly bool soda;

	        public ExecCoreNewInstanceKeyword(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select " +
	                      "new SupportBean(\"A\",intPrimitive) as c0, " +
	                      "new SupportBean(\"B\",intPrimitive+10), " +
	                      "new SupportBean() as c2, " +
	                      "new SupportBean(\"ABC\",0).getTheString() as c3 " +
	                      "from SupportBean";
	            env.CompileDeploy(soda, epl).AddListener("s0");
	            var expectedAggType = new object[][]{new object[] {"c0", typeof(SupportBean)}, new object[] {"new SupportBean(\"B\",intPrimitive+10)", typeof(SupportBean)}};
	            env.AssertStatement("s0", statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, statement.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE));

	            env.SendEventBean(new SupportBean("E1", 10));
	            env.AssertEventNew("s0", @event => {
	                AssertSupportBean(@event.Get("c0"), new object[]{"A", 10});
	                AssertSupportBean(((IDictionary<string, object>) @event.Underlying).Get("new SupportBean(\"B\",intPrimitive+10)"), new object[]{"B", 20});
	                AssertSupportBean(@event.Get("c2"), new object[]{null, 0});
	                Assert.AreEqual("ABC", @event.Get("c3"));
	            });

	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }

	        private void AssertSupportBean(object bean, object[] objects) {
	            var b = (SupportBean) bean;
	            Assert.AreEqual(objects[0], b.TheString);
	            Assert.AreEqual(objects[1], b.IntPrimitive);
	        }
	    }
	}
} // end of namespace
