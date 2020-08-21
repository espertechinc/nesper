///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
	public class EPLVariablesOnSet
	{

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLVariableOnSetSimple());
			execs.Add(new EPLVariableOnSetSimpleSceneTwo());
			execs.Add(new EPLVariableOnSetCompile());
			execs.Add(new EPLVariableOnSetObjectModel());
			execs.Add(new EPLVariableOnSetWithFilter());
			execs.Add(new EPLVariableOnSetSubquery());
			execs.Add(new EPLVariableOnSetWDeploy());
			execs.Add(new EPLVariableOnSetAssignmentOrderNoDup());
			execs.Add(new EPLVariableOnSetAssignmentOrderDup());
			execs.Add(new EPLVariableOnSetRuntimeOrderMultiple());
			execs.Add(new EPLVariableOnSetCoercion());
			execs.Add(new EPLVariableOnSetInvalid());
			execs.Add(new EPLVariableOnSetSubqueryMultikeyWArray());
			execs.Add(new EPLVariableOnSetArrayAtIndex(false));
			execs.Add(new EPLVariableOnSetArrayAtIndex(true));
			execs.Add(new EPLVariableOnSetArrayBoxed());
			execs.Add(new EPLVariableOnSetArrayInvalid());
			execs.Add(new EPLVariableOnSetExpression());
			return execs;
		}

		private class EPLVariableOnSetExpression : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"import " +
					typeof(MyLocalVariable).FullName +
					";\n" +
					"@Name('var') create variable MyLocalVariable VAR = new MyLocalVariable(1, 10);\n" +
					"" +
					"inlined_class \"\"\"\n" +
					"  import " +
					typeof(MyLocalVariable).FullName.Replace("$", ".") +
					";\n" +
					"  public class Helper {\n" +
					"    public static void swap(MyLocalVariable var) {\n" +
					"      int temp = var.a;\n" +
					"      var.a = var.b;\n" +
					"      var.b = temp;\n" +
					"    }\n" +
					"  }\n" +
					"\"\"\"\n" +
					"@Name('s0') on SupportBean set Helper.swap(VAR);\n";
				env.CompileDeploy(epl).AddListener("s0");

				AssertVariable(env, 1, 10);

				env.SendEventBean(new SupportBean());

				AssertVariable(env, 10, 1);

				env.UndeployAll();

				string eplInvalid =
					"import " +
					typeof(MyLocalVariable).FullName +
					";\n" +
					"@Name('var') create variable MyLocalVariable VARONE = new MyLocalVariable(1, 10);\n" +
					"@Name('var') create variable MyLocalVariable VARTWO = new MyLocalVariable(1, 10);\n" +
					"" +
					"inlined_class \"\"\"\n" +
					"  import " +
					typeof(MyLocalVariable).FullName.Replace("$", ".") +
					";\n" +
					"  public class Helper {\n" +
					"    public static void swap(MyLocalVariable varOne, MyLocalVariable varTwo) {\n" +
					"    }\n" +
					"  }\n" +
					"\"\"\"\n" +
					"@Name('s0') on SupportBean set Helper.swap(VARONE, VARTWO);\n";
				TryInvalidCompile(
					env,
					eplInvalid,
					"Failed to validate assignment expression 'Helper.swap(VARONE,VARTWO)': Assignment expression must receive a single variable value");

				string eplConstant =
					"import " +
					typeof(MyLocalVariable).FullName +
					";\n" +
					"@Name('var') create constant variable MyLocalVariable VAR = new MyLocalVariable(1, 10);\n" +
					"@Name('s0') on SupportBean set VAR.reset();\n";
				TryInvalidCompile(
					env,
					eplConstant,
					"Failed to validate assignment expression 'VAR.reset()': Variable by name 'VAR' is declared constant and may not be set");
			}

			private void AssertVariable(
				RegressionEnvironment env,
				int aExpected,
				int bExpected)
			{
				MyLocalVariable value = (MyLocalVariable) env.Runtime.VariableService.GetVariableValue(env.DeploymentId("var"), "VAR");
				Assert.AreEqual(aExpected, value.a);
				Assert.AreEqual(bExpected, value.b);
			}
		}

		private class EPLVariableOnSetArrayBoxed : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"create variable System.Double[] dbls = new System.Double[3];\n" +
					"@priority(1) on SupportBean set dbls[IntPrimitive] = 1;\n" +
					"@Name('s0') select dbls as c0 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				CollectionAssert.AreEqual(new double?[] {null, 1d, null}, (Double[]) env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetArrayInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string eplVariables = "@Name('create') create variable double[primitive] doublearray;\n" +
				                      "create variable int[primitive] intarray;\n" +
				                      "create variable int notAnArray;";
				env.Compile(eplVariables, path);

				// invalid property
				TryInvalidCompile(
					env,
					path,
					"on SupportBean set xxx[IntPrimitive]=1d",
					"Failed to validate assignment expression 'xxx[IntPrimitive]=1.0': Variable by name 'xxx' has not been created or configured");

				// index expression is not Integer
				TryInvalidCompile(
					env,
					path,
					"on SupportBean set doublearray[null]=1d",
					"Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'null' returns 'null' for expression 'doublearray'");

				// type incompatible cannot assign
				TryInvalidCompile(
					env,
					path,
					"on SupportBean set intarray[IntPrimitive]='x'",
					"Failed to validate assignment expression 'intarray[IntPrimitive]=\"x\"': Invalid assignment of column '\"x\"' of type 'System.String' to event property 'intarray' typed as 'int', column and parameter types mismatch");

				// not-an-array
				TryInvalidCompile(
					env,
					path,
					"on SupportBean set notAnArray[IntPrimitive]=1",
					"Failed to validate assignment expression 'notAnArray[IntPrimitive]=1': Variable 'notAnArray' is not an array");

				path.Clear();

				// runtime-behavior for index-overflow and null-array and null-index and
				string epl = "@Name('create') create variable double[primitive] doublearray = new double[3];\n" +
				             "on SupportBean set doublearray[IntBoxed]=DoubleBoxed;\n";
				env.CompileDeploy(epl);

				// index returned is too large
				try {
					SupportBean sb = new SupportBean();
					sb.IntBoxed = 10;
					sb.DoubleBoxed = 10d;
					env.SendEventBean(sb);
					Assert.Fail();
				}
				catch (Exception ex) {
					Assert.IsTrue(ex.Message.Contains("Array length 3 less than index 10 for variable 'doublearray'"));
				}

				// index returned null
				SupportBean sbIndexNull = new SupportBean();
				sbIndexNull.DoubleBoxed = 10d;
				env.SendEventBean(sbIndexNull);

				// rhs returned null for array-of-primitive
				SupportBean sbRHSNull = new SupportBean();
				sbRHSNull.IntBoxed = 1;
				env.SendEventBean(sbRHSNull);

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetArrayAtIndex : RegressionExecution
		{
			private readonly bool soda;

			public EPLVariableOnSetArrayAtIndex(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string eplCreate = "@Name('vars') @public create variable double[primitive] doublearray = new double[3];\n" +
				                   "@public create variable String[] stringarray = new String[] {'a', 'b', 'c'};\n";
				env.CompileDeploy(eplCreate, path);

				string epl = "on SupportBean set doublearray[IntPrimitive] = 1, stringarray[IntPrimitive] = 'x'";
				env.CompileDeploy(soda, epl, path);

				AssertVariables(env, new double[3], "a,b,c".SplitCsv());

				env.SendEventBean(new SupportBean("E1", 1));

				AssertVariables(env, new double[] {0, 1, 0}, "a,x,c".SplitCsv());

				env.UndeployAll();
			}

			private void AssertVariables(
				RegressionEnvironment env,
				double[] doubleExpected,
				string[] stringExpected)
			{
				IDictionary<DeploymentIdNamePair, object> vals = env.Runtime.VariableService.GetVariableValueAll();
				string deploymentId = env.DeploymentId("vars");
				double[] doubleArray = (double[]) vals.Get(new DeploymentIdNamePair(deploymentId, "doublearray"));
				string[] stringArray = (string[]) vals.Get(new DeploymentIdNamePair(deploymentId, "stringarray"));
				CollectionAssert.AreEqual(doubleExpected, doubleArray);
				CollectionAssert.AreEqual(stringExpected, stringArray);
			}
		}

		private class EPLVariableOnSetSubqueryMultikeyWArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('var') @public create variable int total_sum = -1;\n" +
				             "on SupportBean set total_sum = (select sum(value) as c0 from SupportEventWithIntArray#keepall group by array)";
				env.CompileDeploy(epl);

				env.SendEventBean(new SupportEventWithIntArray("E1", new int[] {1, 2}, 10));
				env.SendEventBean(new SupportEventWithIntArray("E2", new int[] {1, 2}, 11));

				env.Milestone(0);
				AssertVariable(env, -1);

				env.SendEventBean(new SupportBean());
				AssertVariable(env, 21);

				env.SendEventBean(new SupportEventWithIntArray("E3", new int[] {1, 2}, 12));
				env.SendEventBean(new SupportBean());
				AssertVariable(env, 33);

				env.Milestone(1);

				env.SendEventBean(new SupportEventWithIntArray("E4", new int[] {1}, 13));
				env.SendEventBean(new SupportBean());
				AssertVariable(env, null);

				env.UndeployAll();
			}

			private void AssertVariable(
				RegressionEnvironment env,
				int? expected)
			{
				Assert.AreEqual(expected, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("var"), "total_sum"));
			}
		}

		private class EPLVariableOnSetSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create variable boolean var_simple_set = true;\n" +
				             "@Name('set') on SupportBean_S0 set var_simple_set = false;\n" +
				             "@Name('s0') select var_simple_set as c0 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 0));
				Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.Milestone(0);

				env.SendEventBean(new SupportBean_S0(0));

				env.Milestone(1);

				env.SendEventBean(new SupportBean("E2", 0));
				Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetSubquery : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				string stmtTextSet =
					"@Name('s0') on SupportBean_S0 as s0str set var1SS = (select p10 from SupportBean_S1#lastevent), var2SS = (select p11||s0str.p01 from SupportBean_S1#lastevent)";
				env.CompileDeploy(stmtTextSet);
				string[] fieldsVar = new string[] {"var1SS", "var2SS"};
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("s0"),
					fieldsVar,
					new object[][] {
						new object[] {"a", "b"}
					});

				env.SendEventBean(new SupportBean_S0(1));
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("s0"),
					fieldsVar,
					new object[][] {
						new object[] {null, null}
					});

				env.Milestone(0);

				env.SendEventBean(new SupportBean_S1(0, "x", "y"));
				env.SendEventBean(new SupportBean_S0(1, "1", "2"));
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("s0"),
					fieldsVar,
					new object[][] {
						new object[] {"x", "y2"}
					});

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetAssignmentOrderNoDup : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string stmtTextSet = "@Name('set') on SupportBean set var1OND = IntPrimitive, var2OND = var1OND + 1, var3OND = var1OND + var2OND";
				env.CompileDeploy(stmtTextSet).AddListener("set");
				string[] fieldsVar = new string[] {"var1OND", "var2OND", "var3OND"};
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {12, 2, null}
					});

				SendSupportBean(env, "S1", 3);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {3, 4, 7});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {3, 4, 7}
					});

				env.Milestone(0);

				SendSupportBean(env, "S1", -1);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {-1, 0, -1});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {-1, 0, -1}
					});

				SendSupportBean(env, "S1", 90);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {90, 91, 181});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {90, 91, 181}
					});

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetAssignmentOrderDup : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				string stmtTextSet = "@Name('set') on SupportBean set var1OD = IntPrimitive, var2OD = var2OD, var1OD = IntBoxed, var3OD = var3OD + 1";
				env.CompileDeploy(stmtTextSet).AddListener("set");
				string[] fieldsVar = new string[] {"var1OD", "var2OD", "var3OD"};
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {0, 1, 2}
					});

				SendSupportBean(env, "S1", -1, 10);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {10, 1, 3});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {10, 1, 3}
					});

				SendSupportBean(env, "S2", -2, 20);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {20, 1, 4});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {20, 1, 4}
					});

				env.Milestone(0);

				SendSupportBeanNewThread(env, "S3", -3, 30);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {30, 1, 5});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {30, 1, 5}
					});

				SendSupportBeanNewThread(env, "S4", -4, 40);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {40, 1, 6});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {40, 1, 6}
					});

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetObjectModel : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				EPStatementObjectModel model = new EPStatementObjectModel();
				model.SelectClause = SelectClause.Create("var1OM", "var2OM", "id");
				model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_A"));

				RegressionPath path = new RegressionPath();
				model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				env.CompileDeploy(model, path);
				string stmtText = "@Name('s0') select var1OM, var2OM, id from SupportBean_A";
				Assert.AreEqual(stmtText, model.ToEPL());
				env.AddListener("s0");

				string[] fieldsSelect = new string[] {"var1OM", "var2OM", "id"};
				SendSupportBean_A(env, "E1");
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {10d, 11L, "E1"});

				model = new EPStatementObjectModel();
				model.OnExpr = OnClause.CreateOnSet(Expressions.Eq(Expressions.Property("var1OM"), Expressions.Property("IntPrimitive")))
					.AddAssignment(Expressions.Eq(Expressions.Property("var2OM"), Expressions.Property("IntBoxed")));
				model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
				model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("set"));
				string stmtTextSet = "@Name('set') on SupportBean set var1OM=IntPrimitive, var2OM=IntBoxed";
				env.CompileDeploy(model, path).AddListener("set");
				Assert.AreEqual(stmtTextSet, model.ToEPL());

				EventType typeSet = env.Statement("set").EventType;
				Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1OM"));
				Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2OM"));
				Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
				string[] fieldsVar = new string[] {"var1OM", "var2OM"};
				EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);

				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {10d, 11L}
					});
				SendSupportBean(env, "S1", 3, 4);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {3d, 4L});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {3d, 4L}
					});

				SendSupportBean_A(env, "E2");
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {3d, 4L, "E2"});

				env.UndeployModuleContaining("set");
				env.UndeployModuleContaining("s0");
			}
		}

		private class EPLVariableOnSetSimpleSceneTwo : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();

				string textVar = "@Name('s0_0') create variable int resvar = 1";
				env.CompileDeploy(textVar, path).AddListener("s0_0");
				string[] fieldsVarOne = new string[] {"resvar"};

				textVar = "@Name('s0_1') create variable int durvar = 10";
				env.CompileDeploy(textVar, path).AddListener("s0_1");
				string[] fieldsVarTwo = new string[] {"durvar"};

				string textSet = "@Name('s1') on SupportBean set resvar = IntPrimitive, durvar = IntPrimitive";
				env.CompileDeploy(textSet, path).AddListener("s1");
				string[] fieldsVarSet = new string[] {"resvar", "durvar"};

				string textSelect = "@Name('s2') select irstream resvar, durvar, symbol from SupportMarketDataBean";
				env.CompileDeploy(textSelect, path).AddListener("s2");
				string[] fieldsSelect = new string[] {"resvar", "durvar", "symbol"};

				env.Milestone(0);

				// read values
				SendMarketDataEvent(env, "E1");
				EPAssertionUtil.AssertProps(env.Listener("s2").AssertOneGetNewAndReset(), fieldsSelect, new object[] {1, 10, "E1"});

				env.Milestone(1);

				// set new value
				SendSupportBean(env, 20);
				EPAssertionUtil.AssertProps(env.Listener("s0_0").LastNewData[0], fieldsVarOne, new object[] {20});
				EPAssertionUtil.AssertProps(env.Listener("s0_1").LastNewData[0], fieldsVarTwo, new object[] {20});
				EPAssertionUtil.AssertProps(env.Listener("s1").AssertOneGetNewAndReset(), fieldsVarSet, new object[] {20, 20});
				env.Listener("s0_0").Reset();

				env.Milestone(2);

				// read values
				SendMarketDataEvent(env, "E2");
				EPAssertionUtil.AssertProps(env.Listener("s2").AssertOneGetNewAndReset(), fieldsSelect, new object[] {20, 20, "E2"});

				env.Milestone(3);

				// set new value
				SendSupportBean(env, 1000);

				env.Milestone(4);

				// read values
				SendMarketDataEvent(env, "E3");
				EPAssertionUtil.AssertProps(env.Listener("s2").AssertOneGetNewAndReset(), fieldsSelect, new object[] {1000, 1000, "E3"});

				env.Milestone(5);

				env.UndeployModuleContaining("s1");
				env.UndeployModuleContaining("s2");
				env.UndeployModuleContaining("s0_0");
				env.UndeployModuleContaining("s0_1");
			}

			private static void SendMarketDataEvent(
				RegressionEnvironment env,
				string symbol)
			{
				SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0, 0L, null);
				env.SendEventBean(bean);
			}

			private static void SendSupportBean(
				RegressionEnvironment env,
				int intPrimitive)
			{
				SupportBean bean = new SupportBean("", intPrimitive);
				env.SendEventBean(bean);
			}
		}

		private class EPLVariableOnSetCompile : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				string stmtText = "@Name('s0') select var1C, var2C, id from SupportBean_A";
				env.EplToModelCompileDeploy(stmtText).AddListener("s0");

				string[] fieldsSelect = new string[] {"var1C", "var2C", "id"};
				SendSupportBean_A(env, "E1");
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {10d, 11L, "E1"});

				string stmtTextSet = "@Name('set') on SupportBean set var1C=IntPrimitive, var2C=IntBoxed";
				env.EplToModelCompileDeploy(stmtTextSet).AddListener("set");

				EventType typeSet = env.Statement("set").EventType;
				Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1C"));
				Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2C"));
				Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
				string[] fieldsVar = new string[] {"var1C", "var2C"};
				EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);

				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {10d, 11L}
					});
				SendSupportBean(env, "S1", 3, 4);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {3d, 4L});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {3d, 4L}
					});

				SendSupportBean_A(env, "E2");
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {3d, 4L, "E2"});

				env.UndeployModuleContaining("set");
				env.UndeployModuleContaining("s0");
			}
		}

		private class EPLVariableOnSetWDeploy : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				string stmtText = "@Name('s0') select var1RTC, TheString from SupportBean(TheString like 'E%')";
				env.CompileDeploy(stmtText).AddListener("s0");

				string[] fieldsSelect = new string[] {"var1RTC", "TheString"};
				SendSupportBean(env, "E1", 1);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {10, "E1"});

				env.Milestone(0);

				SendSupportBean(env, "E2", 2);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {10, "E2"});

				string stmtTextSet = "@Name('set') on SupportBean(TheString like 'S%') set var1RTC = IntPrimitive";
				env.CompileDeploy(stmtTextSet).AddListener("set");

				EventType typeSet = env.Statement("set").EventType;
				Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1RTC"));
				Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
				Assert.IsTrue(Arrays.Equals(typeSet.PropertyNames, new string[] {"var1RTC"}));

				string[] fieldsVar = new string[] {"var1RTC"};
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {10}
					});

				SendSupportBean(env, "S1", 3);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {3});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {3}
					});

				env.Milestone(0);

				SendSupportBean(env, "E3", 4);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {3, "E3"});

				SendSupportBean(env, "S2", -1);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {-1});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {-1}
					});

				SendSupportBean(env, "E4", 5);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {-1, "E4"});

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetRuntimeOrderMultiple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				string stmtTextSet = "@Name('set') on SupportBean(TheString like 'S%' or TheString like 'B%') set var1ROM = IntPrimitive, var2ROM = IntBoxed";
				env.CompileDeploy(stmtTextSet).AddListener("set");
				string[] fieldsVar = new string[] {"var1ROM", "var2ROM"};
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {null, 1}
					});

				EventType typeSet = env.Statement("set").EventType;
				Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1ROM"));
				Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var2ROM"));
				Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
				EPAssertionUtil.AssertEqualsAnyOrder(new string[] {"var1ROM", "var2ROM"}, typeSet.PropertyNames);

				SendSupportBean(env, "S1", 3, null);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {3, null});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {3, null}
					});

				env.Milestone(0);

				SendSupportBean(env, "S1", -1, -2);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {-1, -2});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {-1, -2}
					});

				string stmtText = "@Name('s0') select var1ROM, var2ROM, TheString from SupportBean(TheString like 'E%' or TheString like 'B%')";
				env.CompileDeploy(stmtText).AddListener("s0");
				string[] fieldsSelect = new string[] {"var1ROM", "var2ROM", "TheString"};
				EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fieldsSelect, null);

				env.Milestone(1);

				SendSupportBean(env, "E1", 1);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {-1, -2, "E1"});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {-1, -2}
					});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("s0"),
					fieldsSelect,
					new object[][] {
						new object[] {-1, -2, "E1"}
					});

				SendSupportBean(env, "S1", 11, 12);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {11, 12});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {11, 12}
					});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("s0"),
					fieldsSelect,
					new object[][] {
						new object[] {11, 12, "E1"}
					});

				SendSupportBean(env, "E2", 2);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {11, 12, "E2"});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("s0"),
					fieldsSelect,
					new object[][] {
						new object[] {11, 12, "E2"}
					});

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetWithFilter : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string stmtTextSet = "@Name('set') on SupportBean(TheString like 'S%') set papi_1 = 'end', papi_2 = false, papi_3 = null";
				env.CompileDeploy(stmtTextSet).AddListener("set");
				string[] fieldsVar = new string[] {"papi_1", "papi_2", "papi_3"};
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {"begin", true, "value"}
					});

				EventType typeSet = env.Statement("set").EventType;
				Assert.AreEqual(typeof(string), typeSet.GetPropertyType("papi_1"));
				Assert.AreEqual(typeof(bool?), typeSet.GetPropertyType("papi_2"));
				Assert.AreEqual(typeof(string), typeSet.GetPropertyType("papi_3"));
				Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
				Array.Sort(typeSet.PropertyNames);
				Assert.IsTrue(Arrays.Equals(typeSet.PropertyNames, fieldsVar));

				SendSupportBean(env, "S1", 3);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {"end", false, null});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {"end", false, null}
					});

				env.Milestone(0);

				SendSupportBean(env, "S2", 4);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {"end", false, null});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {"end", false, null}
					});

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetCoercion : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				string stmtTextSet = "@Name('set') on SupportBean set var1COE = IntPrimitive, var2COE = IntPrimitive, var3COE=IntBoxed";
				env.CompileDeploy(stmtTextSet).AddListener("set");
				string[] fieldsVar = new string[] {"var1COE", "var2COE", "var3COE"};
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {null, null, null}
					});

				EventType typeSet = env.Statement("set").EventType;
				Assert.AreEqual(typeof(float?), typeSet.GetPropertyType("var1COE"));
				Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var2COE"));
				Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var3COE"));
				Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
				EPAssertionUtil.AssertEqualsAnyOrder(typeSet.PropertyNames, fieldsVar);

				string stmtText = "@Name('s0') select irstream var1COE, var2COE, var3COE, id from SupportBean_A#length(2)";
				env.CompileDeploy(stmtText).AddListener("s0");
				string[] fieldsSelect = new string[] {"var1COE", "var2COE", "var3COE", "id"};
				EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fieldsSelect, null);

				SendSupportBean_A(env, "A1");
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {null, null, null, "A1"});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("s0"),
					fieldsSelect,
					new object[][] {
						new object[] {null, null, null, "A1"}
					});

				SendSupportBean(env, "S1", 1, 2);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {1f, 1d, 2L});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {1f, 1d, 2L}
					});

				env.Milestone(0);

				SendSupportBean_A(env, "A2");
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldsSelect, new object[] {1f, 1d, 2L, "A2"});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("s0"),
					fieldsSelect,
					new object[][] {
						new object[] {1f, 1d, 2L, "A1"},
						new object[] {1f, 1d, 2L, "A2"}
					});

				SendSupportBean(env, "S1", 10, 20);
				EPAssertionUtil.AssertProps(env.Listener("set").AssertOneGetNewAndReset(), fieldsVar, new object[] {10f, 10d, 20L});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("set"),
					fieldsVar,
					new object[][] {
						new object[] {10f, 10d, 20L}
					});

				SendSupportBean_A(env, "A3");
				EPAssertionUtil.AssertProps(env.Listener("s0").LastNewData[0], fieldsSelect, new object[] {10f, 10d, 20L, "A3"});
				EPAssertionUtil.AssertProps(env.Listener("s0").LastOldData[0], fieldsSelect, new object[] {10f, 10d, 20L, "A1"});
				EPAssertionUtil.AssertPropsPerRow(
					env.GetEnumerator("s0"),
					fieldsSelect,
					new object[][] {
						new object[] {10f, 10d, 20L, "A2"},
						new object[] {10f, 10d, 20L, "A3"}
					});

				env.UndeployAll();
			}
		}

		private class EPLVariableOnSetInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				TryInvalidCompile(
					env,
					"on SupportBean set dummy = 100",
					"Failed to validate assignment expression 'dummy=100': Variable by name 'dummy' has not been created or configured");

				TryInvalidCompile(
					env,
					"on SupportBean set var1IS = 1",
					"Failed to validate assignment expression 'var1IS=1': Variable 'var1IS' of declared type System.String cannot be assigned a value of type int");

				TryInvalidCompile(
					env,
					"on SupportBean set var3IS = 'abc'",
					"Failed to validate assignment expression 'var3IS=\"abc\"': Variable 'var3IS' of declared type System.Int32 cannot be assigned a value of type System.String");

				TryInvalidCompile(
					env,
					"on SupportBean set var3IS = DoublePrimitive",
					"Failed to validate assignment expression 'var3IS=DoublePrimitive': Variable 'var3IS' of declared type System.Int32 cannot be assigned a value of type System.Double");

				TryInvalidCompile(env, "on SupportBean set var2IS = 'false'", "skip");
				TryInvalidCompile(env, "on SupportBean set var3IS = 1.1", "skip");
				TryInvalidCompile(env, "on SupportBean set var3IS = 22222222222222", "skip");
				TryInvalidCompile(
					env,
					"on SupportBean set var3IS",
					"Failed to validate assignment expression 'var3IS': Missing variable assignment expression in assignment number 0");
			}
		}

		private static SupportBean_A SendSupportBean_A(
			RegressionEnvironment env,
			string id)
		{
			SupportBean_A bean = new SupportBean_A(id);
			env.SendEventBean(bean);
			return bean;
		}

		private static SupportBean SendSupportBean(
			RegressionEnvironment env,
			string theString,
			int intPrimitive)
		{
			SupportBean bean = new SupportBean();
			bean.TheString = theString;
			bean.IntPrimitive = intPrimitive;
			env.SendEventBean(bean);
			return bean;
		}

		private static SupportBean SendSupportBean(
			RegressionEnvironment env,
			string theString,
			int intPrimitive,
			int? intBoxed)
		{
			SupportBean bean = MakeSupportBean(theString, intPrimitive, intBoxed);
			env.SendEventBean(bean);
			return bean;
		}

		private static void SendSupportBeanNewThread(
			RegressionEnvironment env,
			string theString,
			int intPrimitive,
			int? intBoxed)
		{
			var t = new Thread(
				() => {
					SupportBean bean = MakeSupportBean(theString, intPrimitive, intBoxed);
					env.SendEventBean(bean);
				});
			t.Start();
			t.Join();
		}

		private static SupportBean MakeSupportBean(
			string theString,
			int intPrimitive,
			int? intBoxed)
		{
			SupportBean bean = new SupportBean();
			bean.TheString = theString;
			bean.IntPrimitive = intPrimitive;
			bean.IntBoxed = intBoxed;
			return bean;
		}

		public class MyLocalVariable
		{
			public int a;
			public int b;

			public MyLocalVariable(
				int a,
				int b)
			{
				this.a = a;
				this.b = b;
			}

			public void Reset()
			{
				throw new UnsupportedOperationException("reset not supported");
			}
		}
	}
} // end of namespace
