///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;


namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreArrayAtElement {

	    public static ICollection<RegressionExecution> Executions() {
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

	    private class ExprCoreAAEWithStringSplit : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select new String('a,b').split(',')[intPrimitive] as c0 from SupportBean";
	            env.CompileDeploy(epl).AddListener("s0");
	            env.AssertStatement("s0", statement => Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("c0")));

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] {"b"});

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreAAEAdditionalInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var eplNoAnArrayIsString =
	                    "create schema Lvl3 (id string);\n" +
	                    "create schema Lvl2 (lvl3 Lvl3);\n" +
	                    "create schema Lvl1 (lvl2 Lvl2);\n" +
	                    "create schema Lvl0 (lvl1 Lvl1, indexNumber int);\n" +
	                    "select lvl1.lvl2.lvl3.id[indexNumber] from Lvl0;\n";
	            env.TryInvalidCompile(eplNoAnArrayIsString,
	                "Failed to validate select-clause expression 'lvl1.lvl2.lvl3.id[indexNumber]': Could not perform array operation on type String");

	            var eplNoAnArrayIsType =
	                "create schema Lvl3 (id string);\n" +
	                    "create schema Lvl2 (lvl3 Lvl3);\n" +
	                    "create schema Lvl1 (lvl2 Lvl2);\n" +
	                    "create schema Lvl0 (lvl1 Lvl1, indexNumber int);\n" +
	                    "select lvl1.lvl2.lvl3[indexNumber] from Lvl0;\n";
	            env.TryInvalidCompile(eplNoAnArrayIsType,
	                "Failed to validate select-clause expression 'lvl1.lvl2.lvl3[indexNumber]': Could not perform array operation on type event type 'Lvl3'");
	        }
	    }

	    private class ExprCoreAAEWithStaticMethodAndUDF : RegressionExecution {
	        private bool soda;

	        public ExprCoreAAEWithStaticMethodAndUDF(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
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
	                      typeof(ExprCoreArrayAtElement).FullName + ".getIntArray()[intPrimitive] as c0, " +
	                      typeof(ExprCoreArrayAtElement).FullName + ".getIntArray2Dim()[intPrimitive][intPrimitive] as c1, " +
	                      "toArray(3,30)[intPrimitive] as c2 " +
	                      "from SupportBean";
	            env.CompileDeploy(soda, epl).AddListener("s0");
	            var fields = "c0,c1,c2".SplitCsv();
	            env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsNew("s0", fields, new object[] {10, 20, 30});

	            env.SendEventBean(new SupportBean("E2", 0));
	            env.AssertPropsNew("s0", fields, new object[] {1, 1, 3});

	            env.SendEventBean(new SupportBean("E3", 2));
	            env.AssertPropsNew("s0", fields, new object[] {null, null, null});

	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    private class ExprCoreAAEVariableRootedChained : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "import " + typeof(MyHolder).FullName + ";\n" +
	                      "create variable MyHolder[] var_mh = new MyHolder[] {new MyHolder('a'), new MyHolder('b')};\n" +
	                      "@name('s0') select var_mh[intPrimitive].getId() as c0 from SupportBean";
	            env.CompileDeploy(epl).AddListener("s0");
	            var fields = "c0".SplitCsv();
	            env.AssertStmtTypesAllSame("s0", fields, typeof(string));

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsNew("s0", fields, new object[]{"b"});

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreAAEVariableRootedTopLevelProp : RegressionExecution {
	        private readonly bool soda;

	        public ExprCoreAAEVariableRootedTopLevelProp(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var eplVariableIntArray = "@public create variable int[primitive] var_intarr = new int[] {1,2,3}";
	            env.CompileDeploy(soda, eplVariableIntArray, path);
	            var eplVariableSBArray = "@public create variable " + typeof(MyHolder).FullName + " var_ = null";
	            env.CompileDeploy(soda, eplVariableSBArray, path);

	            var epl = "@name('s0') select var_intarr[intPrimitive] as c0 from SupportBean";
	            env.CompileDeploy(soda, epl, path).AddListener("s0");
	            var fields = "c0".SplitCsv();
	            env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsNew("s0", fields, new object[]{2});

	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    private class ExprCoreAAEPropRootedNestedNestedArrayProp : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var eplSchema = "@public create schema Lvl2(id string);\n" +
	                            "@public create schema Lvl1(lvl2 Lvl2[]);\n" +
	                            "@public @buseventtype create schema Lvl0(lvl1 Lvl1, indexNumber int, lvl0id string);\n";
	            env.CompileDeploy(eplSchema, path);

	            var epl = "@name('s0') select lvl1.lvl2[indexNumber].id as c0, me.lvl1.lvl2[indexNumber].id as c1 from Lvl0 as me";
	            env.CompileDeploy(epl, path).AddListener("s0");
	            var fields = "c0,c1".SplitCsv();
	            env.AssertStmtTypesAllSame("s0", fields, typeof(string));

	            var lvl2One = CollectionUtil.BuildMap("id", "a");
	            var lvl2Two = CollectionUtil.BuildMap("id", "b");
	            var lvl1 = CollectionUtil.BuildMap("lvl2", new IDictionary<string, object>[] {lvl2One, lvl2Two});
	            var lvl0 = CollectionUtil.BuildMap("lvl1", lvl1, "indexNumber", 1);
	            env.SendEventMap(lvl0, "Lvl0");
	            env.AssertPropsNew("s0", fields, new object[]{"b", "b"});

	            // Invalid tests
	            // array value but no array provided
	            env.TryInvalidCompile(path, "select lvl1.lvl2.id from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.lvl2.id': Failed to find a stream named 'lvl1' (did you mean 'Lvl0'?)");
	            env.TryInvalidCompile(path, "select me.lvl1.lvl2.id from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.lvl2.id': Failed to resolve property 'me.lvl1.lvl2.id' to a stream or nested property in a stream");

	            // two index expressions
	            env.TryInvalidCompile(path, "select lvl1.lvl2[indexNumber, indexNumber].id from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.lvl2[indexNumber,indexNumber].id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type collection of events of type 'Lvl2'");
	            env.TryInvalidCompile(path, "select me.lvl1.lvl2[indexNumber, indexNumber].id from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.lvl2[indexNumber,indexNumber].id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type collection of events of type 'Lvl2'");

	            // double-array
	            env.TryInvalidCompile(path, "select lvl1.lvl2[indexNumber][indexNumber].id from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.lvl2[indexNumber][indexNumber].id': Could not perform array operation on type event type 'Lvl2'");
	            env.TryInvalidCompile(path, "select me.lvl1.lvl2[indexNumber][indexNumber].id from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.lvl2[indexNumber][indexNumb...(41 chars)': Could not perform array operation on type event type 'Lvl2'");

	            // wrong index expression type
	            env.TryInvalidCompile(path, "select lvl1.lvl2[lvl0id].id from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.lvl2[lvl0id].id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'lvl0id' returns 'String' for operation on type collection of events of type 'Lvl2'");
	            env.TryInvalidCompile(path, "select me.lvl1.lvl2[lvl0id].id from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.lvl2[lvl0id].id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'lvl0id' returns 'String' for operation on type collection of events of type 'Lvl2'");

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreAAEPropRootedNestedNestedProp : RegressionExecution {
	        private readonly bool soda;

	        public ExprCoreAAEPropRootedNestedNestedProp(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var eplSchema = "@public create schema Lvl2(intarr int[]);\n" +
	                            "@public create schema Lvl1(lvl2 Lvl2);\n" +
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
	            env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

	            var lvl2 = CollectionUtil.BuildMap("intarr", new int?[]{1, 2, 3});
	            var lvl1 = CollectionUtil.BuildMap("lvl2", lvl2);
	            var lvl0 = CollectionUtil.BuildMap("lvl1", lvl1, "indexNumber", 2);
	            env.SendEventMap(lvl0, "Lvl0");
	            env.AssertPropsNew("s0", fields, new object[]{3, 3, 3, 3});

	            // Invalid tests
	            // not an index expression
	            env.TryInvalidCompile(path, "select lvl1.lvl2[indexNumber] from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.lvl2[indexNumber]': Could not perform array operation on type event type 'Lvl2'");
	            env.TryInvalidCompile(path, "select me.lvl1.lvl2[indexNumber] from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.lvl2[indexNumber]': Could not perform array operation on type event type 'Lvl2'");

	            // two index expressions
	            env.TryInvalidCompile(path, "select lvl1.lvl2.intarr[indexNumber, indexNumber] from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.lvl2.intarr[indexNumber,indexN...(41 chars)': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type Integer[]");
	            env.TryInvalidCompile(path, "select me.lvl1.lvl2.intarr[indexNumber, indexNumber] from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.lvl2.intarr[indexNumber,ind...(44 chars)': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type Integer[]");

	            // double-array
	            env.TryInvalidCompile(path, "select lvl1.lvl2.intarr[indexNumber][indexNumber] from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.lvl2.intarr[indexNumber][index...(42 chars)': Could not perform array operation on type Integer");
	            env.TryInvalidCompile(path, "select me.lvl1.lvl2.intarr[indexNumber][indexNumber] from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.lvl2.intarr[indexNumber][in...(45 chars)': Could not perform array operation on type Integer");

	            // wrong index expression type
	            env.TryInvalidCompile(path, "select lvl1.lvl2.intarr[id] from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.lvl2.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'String' for operation on type Integer[]");
	            env.TryInvalidCompile(path, "select me.lvl1.lvl2.intarr[id] from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.lvl2.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'String' for operation on type Integer[]");

	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    private class ExprCoreAAEPropRootedNestedArrayProp : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "create schema Lvl1(id string);\n" +
	                "@public @buseventtype create schema Lvl0(lvl1 Lvl1[], indexNumber int, lvl0id string);\n" +
	                "@name('s0') select lvl1[indexNumber].id as c0, me.lvl1[indexNumber].id as c1 from Lvl0 as me";
	            env.CompileDeploy(epl, path).AddListener("s0");
	            var fields = "c0,c1".SplitCsv();
	            env.AssertStmtTypesAllSame("s0", fields, typeof(string));

	            var lvl1One = CollectionUtil.BuildMap("id", "a");
	            var lvl1Two = CollectionUtil.BuildMap("id", "b");
	            var lvl0 = CollectionUtil.BuildMap("lvl1", new IDictionary<string, object>[] {lvl1One, lvl1Two}, "indexNumber", 1);
	            env.SendEventMap(lvl0, "Lvl0");
	            env.AssertPropsNew("s0", fields, new object[]{"b", "b"});

	            // Invalid tests
	            // array value but no array provided
	            env.TryInvalidCompile(path, "select lvl1.id from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.id': Failed to resolve property 'lvl1.id' (property 'lvl1' is an indexed property and requires an index or enumeration method to access values)");
	            env.TryInvalidCompile(path, "select me.lvl1.id from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.id': Property named 'lvl1.id' is not valid in stream 'me' (did you mean 'lvl0id'?)");

	            // not an index expression
	            env.TryInvalidCompile(path, "select lvl1.id[indexNumber] from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.id[indexNumber]': Could not find event property or method named 'id' in collection of events of type 'Lvl1'");
	            env.TryInvalidCompile(path, "select me.lvl1.id[indexNumber] from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.id[indexNumber]': Could not find event property or method named 'id' in collection of events of type 'Lvl1'");

	            // two index expressions
	            env.TryInvalidCompile(path, "select lvl1[indexNumber, indexNumber].id from Lvl0",
	                "Failed to validate select-clause expression 'lvl1[indexNumber,indexNumber].id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'lvl1'");
	            env.TryInvalidCompile(path, "select me.lvl1[indexNumber, indexNumber].id from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1[indexNumber,indexNumber].id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'lvl1'");

	            // double-array
	            env.TryInvalidCompile(path, "select lvl1[indexNumber][indexNumber].id from Lvl0",
	                "Failed to validate select-clause expression 'lvl1[indexNumber][indexNumber].id': Could not perform array operation on type event type 'Lvl1'");
	            env.TryInvalidCompile(path, "select me.lvl1[indexNumber][indexNumber].id from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1[indexNumber][indexNumber].id': Could not perform array operation on type event type 'Lvl1'");

	            // wrong index expression type
	            env.TryInvalidCompile(path, "select lvl1[lvl0id].id from Lvl0",
	                "Failed to validate select-clause expression 'lvl1[lvl0id].id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'lvl0id' returns 'String' for property 'lvl1'");
	            env.TryInvalidCompile(path, "select me.lvl1[lvl0id].id from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1[lvl0id].id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'lvl0id' returns 'String' for property 'lvl1'");

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreAAEPropRootedNestedProp : RegressionExecution {
	        private readonly bool soda;

	        public ExprCoreAAEPropRootedNestedProp(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var eplSchema =
	                "@public create schema Lvl1(intarr int[]);\n" +
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
	            env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

	            var lvl1 = CollectionUtil.BuildMap("intarr", new int?[]{1, 2, 3});
	            var lvl0 = CollectionUtil.BuildMap("lvl1", lvl1, "indexNumber", 2);
	            env.SendEventMap(lvl0, "Lvl0");
	            env.AssertPropsNew("s0", fields, new object[]{3, 3, 3, 3});

	            // Invalid tests
	            // not an index expression
	            env.TryInvalidCompile(path, "select lvl1[indexNumber] from Lvl0",
	                "Failed to validate select-clause expression 'lvl1[indexNumber]': Invalid array operation for property 'lvl1'");
	            env.TryInvalidCompile(path, "select me.lvl1[indexNumber] from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1[indexNumber]': Invalid array operation for property 'lvl1'");

	            // two index expressions
	            env.TryInvalidCompile(path, "select lvl1.intarr[indexNumber, indexNumber] from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.intarr[indexNumber,indexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type Integer[]");
	            env.TryInvalidCompile(path, "select me.lvl1.intarr[indexNumber, indexNumber] from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.intarr[indexNumber,indexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type Integer[]");

	            // double-array
	            env.TryInvalidCompile(path, "select lvl1.intarr[indexNumber][indexNumber] from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.intarr[indexNumber][indexNumber]': Could not perform array operation on type Integer");
	            env.TryInvalidCompile(path, "select me.lvl1.intarr[indexNumber][indexNumber] from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.intarr[indexNumber][indexNumber]': Could not perform array operation on type Integer");

	            // wrong index expression type
	            env.TryInvalidCompile(path, "select lvl1.intarr[id] from Lvl0",
	                "Failed to validate select-clause expression 'lvl1.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'String' for operation on type Integer[]");
	            env.TryInvalidCompile(path, "select me.lvl1.intarr[id] from Lvl0 as me",
	                "Failed to validate select-clause expression 'me.lvl1.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'String' for operation on type Integer[]");

	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    private class ExprCoreAAEPropRootedTopLevelProp : RegressionExecution {
	        private readonly bool soda;

	        public ExprCoreAAEPropRootedTopLevelProp(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select " +
	                      "intarr[indexNumber] as c0, " +
	                      "intarr.size() as c1, " +
	                      "me.intarr[indexNumber] as c2, " +
	                      "me.intarr.size() as c3 " +
	                      "from SupportBeanWithArray as me";
	            env.CompileDeploy(soda, epl).AddListener("s0");
	            var fields = "c0,c1,c2,c3".SplitCsv();
	            env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

	            env.SendEventBean(new SupportBeanWithArray(1, new int[]{1, 2}));
	            env.AssertPropsNew("s0", fields, new object[]{2, 2, 2, 2});

	            // Invalid tests
	            // two index expressions
	            env.TryInvalidCompile("select intarr[indexNumber, indexNumber] from SupportBeanWithArray",
	                "Failed to validate select-clause expression 'intarr[indexNumber,indexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'intarr'");
	            env.TryInvalidCompile("select me.intarr[indexNumber, indexNumber] from SupportBeanWithArray as me",
	                "Failed to validate select-clause expression 'me.intarr[indexNumber,indexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'intarr'");

	            // double-array
	            env.TryInvalidCompile("select intarr[indexNumber][indexNumber] from SupportBeanWithArray",
	                "Failed to validate select-clause expression 'intarr[indexNumber][indexNumber]': Could not perform array operation on type Integer");
	            env.TryInvalidCompile("select me.intarr[indexNumber][indexNumber] from SupportBeanWithArray as me",
	                "Failed to validate select-clause expression 'me.intarr[indexNumber][indexNumber]': Could not perform array operation on type Integer");

	            // wrong index expression type
	            env.TryInvalidCompile("select intarr[id] from SupportBeanWithArray",
	                "Failed to validate select-clause expression 'intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'String' for property 'intarr'");
	            env.TryInvalidCompile("select me.intarr[id] from SupportBeanWithArray as me",
	                "Failed to validate select-clause expression 'me.intarr[id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'id' returns 'String' for property 'intarr'");

	            // not an array
	            env.TryInvalidCompile("select indexNumber[indexNumber] from SupportBeanWithArray",
	                "Failed to validate select-clause expression 'indexNumber[indexNumber]': Invalid array operation for property 'indexNumber'");
	            env.TryInvalidCompile("select me.indexNumber[indexNumber] from SupportBeanWithArray as me",
	                "Failed to validate select-clause expression 'me.indexNumber[indexNumber]': Invalid array operation for property 'indexNumber'");

	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    public static int[] GetIntArray() {
	        return new int[] {1, 10};
	    }

	    public static int[][] GetIntArray2Dim() {
	        return new int[][] {new int[]{1, 10}, new int[] {2, 20}};
	    }

	    [Serializable] public class MyHolder {
	        private readonly string id;

	        public MyHolder(string id) {
	            this.id = id;
	        }

	        public string GetId() {
	            return id;
	        }
	    }
	}
} // end of namespace
