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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil; // AssertProps
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // TryInvalidCompile

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreArrayAtElement
	{

		public static ICollection<RegressionExecution> Executions()
		{
			var executions = new List<RegressionExecution>();
			executions.Add(new ExprCoreAAEPropRootedTopLevelProp(false));
			executions.Add(new ExprCoreAAEPropRootedTopLevelProp(true));
			executions.Add(new ExprCoreAAEPropRootedNestedProp(false));
			executions.Add(new ExprCoreAAEPropRootedNestedProp(true));
			executions.Add(new ExprCoreAAEPropRootedNestedNestedProp(false));
			executions.Add(new ExprCoreAAEPropRootedNestedNestedProp(true));
			executions.Add(new ExprCoreAAEPropRootedNestedArrayProp());
			executions.Add(new ExprCoreAAEPropRootedNestedNestedArrayProp());
			executions.Add(new ExprCoreAAEVariableRootedTopLevelProp(false));
			executions.Add(new ExprCoreAAEVariableRootedTopLevelProp(true));
			executions.Add(new ExprCoreAAEVariableRootedChained());
			executions.Add(new ExprCoreAAEWithStaticMethodAndUDF(false));
			executions.Add(new ExprCoreAAEWithStaticMethodAndUDF(true));
			executions.Add(new ExprCoreAAEAdditionalInvalid());
			executions.Add(new ExprCoreAAEWithStringSplit());
			return executions;
		}

		private class ExprCoreAAEWithStringSplit : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select new String('a,b').split(',')[IntPrimitive] as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");
				var eventType = env.Statement("s0").EventType;
				Assert.AreEqual(typeof(string), eventType.GetPropertyType("c0"));

				env.SendEventBean(new SupportBean("E1", 1));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "c0".SplitCsv(), "b");

				env.UndeployAll();
			}
		}

		private class ExprCoreAAEAdditionalInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var eplNoAnArrayIsString =
					"create schema Lvl3 (id string);\n" +
					"create schema Lvl2 (lvl3 Lvl3);\n" +
					"create schema Lvl1 (lvl2 Lvl2);\n" +
					"create schema Lvl0 (lvl1 Lvl1, indexNumber int);\n" +
					"select lvl1.lvl2.lvl3.id[indexNumber] from Lvl0;\n";
				TryInvalidCompile(
					env,
					eplNoAnArrayIsString,
					"Failed to validate select-clause expression 'lvl1.lvl2.lvl3.id[indexNumber]': Could not perform array operation on type class System.String");

				var eplNoAnArrayIsType =
					"create schema Lvl3 (id string);\n" +
					"create schema Lvl2 (lvl3 Lvl3);\n" +
					"create schema Lvl1 (lvl2 Lvl2);\n" +
					"create schema Lvl0 (lvl1 Lvl1, indexNumber int);\n" +
					"select lvl1.lvl2.lvl3[indexNumber] from Lvl0;\n";
				TryInvalidCompile(
					env,
					eplNoAnArrayIsType,
					"Failed to validate select-clause expression 'lvl1.lvl2.lvl3[indexNumber]': Could not perform array operation on type event type 'Lvl3'");
			}
		}

		private class ExprCoreAAEWithStaticMethodAndUDF : RegressionExecution
		{
			private bool soda;

			public ExprCoreAAEWithStaticMethodAndUDF(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') inlined_class \"\"\"\n" +
				          "  import com.espertech.esper.common.client.hook.singlerowfunc.*;\n" +
				          "  @ExtensionSingleRowFunction(name=\"toArray\", methodName=\"toArray\")\n" +
				          "  public class Helper {\n" +
				          "    public static int[] toArray(int a, int b) {\n" +
				          "      return new int[] {a, b};\n" +
				          "    }\n" +
				          "  }\n" +
				          "\"\"\" " +
				          "select " +
				          typeof(ExprCoreArrayAtElement).Name +
				          ".getIntArray()[IntPrimitive] as c0, " +
				          typeof(ExprCoreArrayAtElement).Name +
				          ".getIntArray2Dim()[IntPrimitive][IntPrimitive] as c1, " +
				          "toArray(3,30)[IntPrimitive] as c2 " +
				          "from SupportBean";
				env.CompileDeploy(soda, epl).AddListener("s0");
				var fields = "c0,c1,c2".SplitCsv();
				var eventType = env.Statement("s0").EventType;
				foreach (var field in fields) {
					Assert.AreEqual(typeof(int?), eventType.GetPropertyType(field));
				}

				env.SendEventBean(new SupportBean("E1", 1));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, 10, 20, 30);

				env.SendEventBean(new SupportBean("E2", 0));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, 1, 1, 3);

				env.SendEventBean(new SupportBean("E3", 2));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, null, null, null);

				env.UndeployAll();
			}
		}

		private class ExprCoreAAEVariableRootedChained : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "import " +
				          typeof(MyHolder).Name +
				          ";\n" +
				          "create variable MyHolder[] var_mh = new MyHolder[] {new MyHolder('a'), new MyHolder('b')};\n" +
				          "@name('s0') select var_mh[IntPrimitive].getId() as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");
				var fields = "c0".SplitCsv();
				var @out = env.Statement("s0").EventType;
				foreach (var field in fields) {
					Assert.AreEqual(typeof(string), @out.GetPropertyType(field));
				}

				env.SendEventBean(new SupportBean("E1", 1));
				AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "b");

				env.UndeployAll();
			}
		}

		private class ExprCoreAAEVariableRootedTopLevelProp : RegressionExecution
		{
			private readonly bool soda;

			public ExprCoreAAEVariableRootedTopLevelProp(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var eplVariableIntArray = "create variable int[primitive] var_intarr = new int[] {1, 2, 3}";
				env.CompileDeploy(soda, eplVariableIntArray, path);
				var eplVariableSBArray = "create variable " + typeof(MyHolder).Name + " var_ = null";
				env.CompileDeploy(soda, eplVariableSBArray, path);

				var epl = "@name('s0') select var_intarr[IntPrimitive] as c0 from SupportBean";
				env.CompileDeploy(soda, epl, path).AddListener("s0");
				var fields = "c0".SplitCsv();
				var @out = env.Statement("s0").EventType;
				foreach (var field in fields) {
					Assert.AreEqual(typeof(int?), @out.GetPropertyType(field));
				}

				env.SendEventBean(new SupportBean("E1", 1));
				AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, 2);

				env.UndeployAll();
			}
		}

		private class ExprCoreAAEPropRootedNestedNestedArrayProp : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var eplSchema = "create schema Lvl2(id string);\n" +
				                "create schema Lvl1(lvl2 Lvl2[]);\n" +
				                "@public @buseventtype create schema Lvl0(lvl1 Lvl1, indexNumber int, lvl0id string);\n";
				env.CompileDeploy(eplSchema, path);

				var epl = "@name('s0') select lvl1.lvl2[indexNumber].id as c0, me.lvl1.lvl2[indexNumber].id as c1 from Lvl0 as me";
				env.CompileDeploy(epl, path).AddListener("s0");
				var fields = "c0,c1".SplitCsv();
				var @out = env.Statement("s0").EventType;
				foreach (var field in fields) {
					Assert.AreEqual(typeof(string), @out.GetPropertyType(field));
				}

				var lvl2One = CollectionUtil.BuildMap("id", "a");
				var lvl2Two = CollectionUtil.BuildMap("id", "b");
				var lvl1 = CollectionUtil.BuildMap("lvl2", new[] {lvl2One, lvl2Two});
				var lvl0 = CollectionUtil.BuildMap("lvl1", lvl1, "indexNumber", 1);
				env.SendEventMap(lvl0, "Lvl0");
				AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "b", "b");

				// Invalid tests
				// array value but no array provided
				TryInvalidCompile(
					env,
					path,
					"select lvl1.lvl2.id from Lvl0",
					"Failed to validate select-clause expression 'lvl1.lvl2.id': Failed to find a stream named 'lvl1' (did you mean 'Lvl0'?)");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.lvl2.id from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.lvl2.id': Failed to resolve property 'me.lvl1.lvl2.id' to a stream or nested property in a stream");

				// two index expressions
				TryInvalidCompile(
					env,
					path,
					"select lvl1.lvl2[indexNumber, indexNumber].id from Lvl0",
					"Failed to validate select-clause expression 'lvl1.lvl2[indexNumber,indexNumber].id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type collection of events of type 'Lvl2'");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.lvl2[indexNumber, indexNumber].id from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.lvl2[indexNumber,indexNumber].id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type collection of events of type 'Lvl2'");

				// double-array
				TryInvalidCompile(
					env,
					path,
					"select lvl1.lvl2[indexNumber][indexNumber].id from Lvl0",
					"Failed to validate select-clause expression 'lvl1.lvl2[indexNumber][indexNumber].id': Could not perform array operation on type event type 'Lvl2'");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.lvl2[indexNumber][indexNumber].id from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.lvl2[indexNumber][indexNumb...(41 chars)': Could not perform array operation on type event type 'Lvl2'");

				// wrong index expression type
				TryInvalidCompile(
					env,
					path,
					"select lvl1.lvl2[lvl0id].id from Lvl0",
					"Failed to validate select-clause expression 'lvl1.lvl2[lvl0id].id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'lvl0id' returns 'System.String' for operation on type collection of events of type 'Lvl2'");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.lvl2[lvl0id].id from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.lvl2[lvl0id].id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'lvl0id' returns 'System.String' for operation on type collection of events of type 'Lvl2'");

				env.UndeployAll();
			}
		}

		private class ExprCoreAAEPropRootedNestedNestedProp : RegressionExecution
		{
			private readonly bool soda;

			public ExprCoreAAEPropRootedNestedNestedProp(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var eplSchema = "create schema Lvl2(intarr int[]);\n" +
				                "create schema Lvl1(lvl2 Lvl2);\n" +
				                "@public @buseventtype create schema Lvl0(lvl1 Lvl1, indexNumber int, id string);\n";
				env.CompileDeploy(eplSchema, path);

				var epl = "@name('s0') select " +
				          "lvl1.lvl2.intarr[indexNumber] as c0, " +
				          "lvl1.lvl2.intarr.size() as c1, " +
				          "me.lvl1.lvl2.intarr[indexNumber] as c2, " +
				          "me.lvl1.lvl2.intarr.size() as c3 " +
				          "from Lvl0 as me";
				env.CompileDeploy(soda, epl, path).AddListener("s0");
				var fields = "c0,c1,c2,c3".SplitCsv();
				var @out = env.Statement("s0").EventType;
				foreach (var field in fields) {
					Assert.AreEqual(typeof(int?), @out.GetPropertyType(field));
				}

				var lvl2 = CollectionUtil.BuildMap("intarr", new int?[] {1, 2, 3});
				var lvl1 = CollectionUtil.BuildMap("lvl2", lvl2);
				var lvl0 = CollectionUtil.BuildMap("lvl1", lvl1, "indexNumber", 2);
				env.SendEventMap(lvl0, "Lvl0");
				AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, 3, 3, 3, 3);

				// Invalid tests
				// not an index expression
				TryInvalidCompile(
					env,
					path,
					"select lvl1.lvl2[indexNumber] from Lvl0",
					"Failed to validate select-clause expression 'lvl1.lvl2[indexNumber]': Could not perform array operation on type event type 'Lvl2'");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.lvl2[indexNumber] from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.lvl2[indexNumber]': Could not perform array operation on type event type 'Lvl2'");

				// two index expressions
				TryInvalidCompile(
					env,
					path,
					"select lvl1.lvl2.intarr[indexNumber, indexNumber] from Lvl0",
					"Failed to validate select-clause expression 'lvl1.lvl2.intarr[indexNumber,indexN...(41 chars)': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type array of Integer");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.lvl2.intarr[indexNumber, indexNumber] from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.lvl2.intarr[indexNumber,ind...(44 chars)': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type array of Integer");

				// double-array
				TryInvalidCompile(
					env,
					path,
					"select lvl1.lvl2.intarr[indexNumber][indexNumber] from Lvl0",
					"Failed to validate select-clause expression 'lvl1.lvl2.intarr[indexNumber][index...(42 chars)': Could not perform array operation on type class System.Int32");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.lvl2.intarr[indexNumber][indexNumber] from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.lvl2.intarr[indexNumber][in...(45 chars)': Could not perform array operation on type class System.Int32");

				// wrong index expression type
				TryInvalidCompile(
					env,
					path,
					"select lvl1.lvl2.intarr[id] from Lvl0",
					"Failed to validate select-clause expression 'lvl1.lvl2.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'System.String' for operation on type array of Integer");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.lvl2.intarr[id] from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.lvl2.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'System.String' for operation on type array of Integer");

				env.UndeployAll();
			}
		}

		private class ExprCoreAAEPropRootedNestedArrayProp : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl =
					"create schema Lvl1(id string);\n" +
					"@public @buseventtype create schema Lvl0(lvl1 Lvl1[], indexNumber int, lvl0id string);\n" +
					"@name('s0') select lvl1[indexNumber].id as c0, me.lvl1[indexNumber].id as c1 from Lvl0 as me";
				env.CompileDeploy(epl, path).AddListener("s0");
				var fields = "c0,c1".SplitCsv();
				var @out = env.Statement("s0").EventType;
				foreach (var field in fields) {
					Assert.AreEqual(typeof(string), @out.GetPropertyType(field));
				}

				var lvl1One = CollectionUtil.BuildMap("id", "a");
				var lvl1Two = CollectionUtil.BuildMap("id", "b");
				var lvl0 = CollectionUtil.BuildMap("lvl1", new[] {lvl1One, lvl1Two}, "indexNumber", 1);
				env.SendEventMap(lvl0, "Lvl0");
				AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "b", "b");

				// Invalid tests
				// array value but no array provided
				TryInvalidCompile(
					env,
					path,
					"select lvl1.id from Lvl0",
					"Failed to validate select-clause expression 'lvl1.id': Failed to resolve property 'lvl1.id' (property 'lvl1' is an indexed property and requires an index or enumeration method to access values)");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.id from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.id': Property named 'lvl1.id' is not valid in stream 'me' (did you mean 'lvl0id'?)");

				// not an index expression
				TryInvalidCompile(
					env,
					path,
					"select lvl1.id[indexNumber] from Lvl0",
					"Failed to validate select-clause expression 'lvl1.id[indexNumber]': Could not find event property or method named 'id' in collection of events of type 'Lvl1'");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.id[indexNumber] from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.id[indexNumber]': Could not find event property or method named 'id' in collection of events of type 'Lvl1'");

				// two index expressions
				TryInvalidCompile(
					env,
					path,
					"select lvl1[indexNumber, indexNumber].id from Lvl0",
					"Failed to validate select-clause expression 'lvl1[indexNumber,indexNumber].id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'lvl1'");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1[indexNumber, indexNumber].id from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1[indexNumber,indexNumber].id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'lvl1'");

				// double-array
				TryInvalidCompile(
					env,
					path,
					"select lvl1[indexNumber][indexNumber].id from Lvl0",
					"Failed to validate select-clause expression 'lvl1[indexNumber][indexNumber].id': Could not perform array operation on type event type 'Lvl1'");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1[indexNumber][indexNumber].id from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1[indexNumber][indexNumber].id': Could not perform array operation on type event type 'Lvl1'");

				// wrong index expression type
				TryInvalidCompile(
					env,
					path,
					"select lvl1[lvl0id].id from Lvl0",
					"Failed to validate select-clause expression 'lvl1[lvl0id].id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'lvl0id' returns 'System.String' for property 'lvl1'");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1[lvl0id].id from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1[lvl0id].id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'lvl0id' returns 'System.String' for property 'lvl1'");

				env.UndeployAll();
			}
		}

		private class ExprCoreAAEPropRootedNestedProp : RegressionExecution
		{
			private readonly bool soda;

			public ExprCoreAAEPropRootedNestedProp(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var eplSchema =
					"create schema Lvl1(intarr int[]);\n" +
					"@public @buseventtype create schema Lvl0(lvl1 Lvl1, indexNumber int, id string);\n";
				env.CompileDeploy(eplSchema, path);

				var epl = "@name('s0') select " +
				          "lvl1.intarr[indexNumber] as c0, " +
				          "lvl1.intarr.size() as c1, " +
				          "me.lvl1.intarr[indexNumber] as c2, " +
				          "me.lvl1.intarr.size() as c3 " +
				          "from Lvl0 as me";
				env.CompileDeploy(soda, epl, path).AddListener("s0");
				var fields = "c0,c1,c2,c3".SplitCsv();
				var @out = env.Statement("s0").EventType;
				foreach (var field in fields) {
					Assert.AreEqual(typeof(int?), @out.GetPropertyType(field));
				}

				var lvl1 = CollectionUtil.BuildMap("intarr", new int?[] {1, 2, 3});
				var lvl0 = CollectionUtil.BuildMap("lvl1", lvl1, "indexNumber", 2);
				env.SendEventMap(lvl0, "Lvl0");
				AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, 3, 3, 3, 3);

				// Invalid tests
				// not an index expression
				TryInvalidCompile(
					env,
					path,
					"select lvl1[indexNumber] from Lvl0",
					"Failed to validate select-clause expression 'lvl1[indexNumber]': Invalid array operation for property 'lvl1'");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1[indexNumber] from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1[indexNumber]': Invalid array operation for property 'lvl1'");

				// two index expressions
				TryInvalidCompile(
					env,
					path,
					"select lvl1.intarr[indexNumber, indexNumber] from Lvl0",
					"Failed to validate select-clause expression 'lvl1.intarr[indexNumber,indexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type array of Integer");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.intarr[indexNumber, indexNumber] from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.intarr[indexNumber,indexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type array of Integer");

				// double-array
				TryInvalidCompile(
					env,
					path,
					"select lvl1.intarr[indexNumber][indexNumber] from Lvl0",
					"Failed to validate select-clause expression 'lvl1.intarr[indexNumber][indexNumber]': Could not perform array operation on type class System.Int32");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.intarr[indexNumber][indexNumber] from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.intarr[indexNumber][indexNumber]': Could not perform array operation on type class System.Int32");

				// wrong index expression type
				TryInvalidCompile(
					env,
					path,
					"select lvl1.intarr[id] from Lvl0",
					"Failed to validate select-clause expression 'lvl1.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'System.String' for operation on type array of Integer");
				TryInvalidCompile(
					env,
					path,
					"select me.lvl1.intarr[id] from Lvl0 as me",
					"Failed to validate select-clause expression 'me.lvl1.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'System.String' for operation on type array of Integer");

				env.UndeployAll();
			}
		}

		private class ExprCoreAAEPropRootedTopLevelProp : RegressionExecution
		{
			private readonly bool soda;

			public ExprCoreAAEPropRootedTopLevelProp(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select " +
				          "intarr[indexNumber] as c0, " +
				          "intarr.size() as c1, " +
				          "me.intarr[indexNumber] as c2, " +
				          "me.intarr.size() as c3 " +
				          "from SupportBeanWithArray as me";
				env.CompileDeploy(soda, epl).AddListener("s0");
				var fields = "c0,c1,c2,c3".SplitCsv();
				var @out = env.Statement("s0").EventType;
				foreach (var field in fields) {
					Assert.AreEqual(typeof(int?), @out.GetPropertyType(field));
				}

				env.SendEventBean(new SupportBeanWithArray(1, new[] {1, 2}));
				AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, 2, 2, 2, 2);

				// Invalid tests
				// two index expressions
				TryInvalidCompile(
					env,
					"select intarr[indexNumber, indexNumber] from SupportBeanWithArray",
					"Failed to validate select-clause expression 'intarr[indexNumber,indexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'intarr'");
				TryInvalidCompile(
					env,
					"select me.intarr[indexNumber, indexNumber] from SupportBeanWithArray as me",
					"Failed to validate select-clause expression 'me.intarr[indexNumber,indexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'intarr'");

				// double-array
				TryInvalidCompile(
					env,
					"select intarr[indexNumber][indexNumber] from SupportBeanWithArray",
					"Failed to validate select-clause expression 'intarr[indexNumber][indexNumber]': Could not perform array operation on type class System.Int32");
				TryInvalidCompile(
					env,
					"select me.intarr[indexNumber][indexNumber] from SupportBeanWithArray as me",
					"Failed to validate select-clause expression 'me.intarr[indexNumber][indexNumber]': Could not perform array operation on type class System.Int32");

				// wrong index expression type
				TryInvalidCompile(
					env,
					"select intarr[id] from SupportBeanWithArray",
					"Failed to validate select-clause expression 'intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'System.String' for property 'intarr'");
				TryInvalidCompile(
					env,
					"select me.intarr[id] from SupportBeanWithArray as me",
					"Failed to validate select-clause expression 'me.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'System.String' for property 'intarr'");

				// not an array
				TryInvalidCompile(
					env,
					"select indexNumber[indexNumber] from SupportBeanWithArray",
					"Failed to validate select-clause expression 'indexNumber[indexNumber]': Invalid array operation for property 'indexNumber'");
				TryInvalidCompile(
					env,
					"select me.indexNumber[indexNumber] from SupportBeanWithArray as me",
					"Failed to validate select-clause expression 'me.indexNumber[indexNumber]': Invalid array operation for property 'indexNumber'");

				env.UndeployAll();
			}
		}

		public static int[] IntArray {
			get { return new[] {1, 10}; }
		}

		public static int[][] IntArray2Dim {
			get {
				return new[] {
					new[] {1, 10},
					new[] {2, 20}
				};
			}
		}

		[Serializable]
		public class MyHolder
		{
			public MyHolder(string id)
			{
				this.Id = id;
			}

			public string Id { get; }
		}
	}
} // end of namespace
