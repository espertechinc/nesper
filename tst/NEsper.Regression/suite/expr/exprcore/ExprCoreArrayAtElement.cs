///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;


namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreArrayAtElement
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPropRootedTopLevelProp(execs);
            WithPropRootedNestedProp(execs);
            WithPropRootedNestedNestedProp(execs);
            WithPropRootedNestedArrayProp(execs);
            WithPropRootedNestedNestedArrayProp(execs);
            WithVariableRootedTopLevelProp(execs);
            WithVariableRootedChained(execs);
            WithWithStaticMethodAndUDF(execs);
            WithAdditionalInvalid(execs);
            WithWithStringSplit(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWithStringSplit(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEWithStringSplit());
            return execs;
        }

        public static IList<RegressionExecution> WithAdditionalInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEAdditionalInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithWithStaticMethodAndUDF(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEWithStaticMethodAndUDF(false));
            execs.Add(new ExprCoreAAEWithStaticMethodAndUDF(true));
            return execs;
        }

        public static IList<RegressionExecution> WithVariableRootedChained(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEVariableRootedChained());
            return execs;
        }

        public static IList<RegressionExecution> WithVariableRootedTopLevelProp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEVariableRootedTopLevelProp(false));
            execs.Add(new ExprCoreAAEVariableRootedTopLevelProp(true));
            return execs;
        }

        public static IList<RegressionExecution> WithPropRootedNestedNestedArrayProp(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEPropRootedNestedNestedArrayProp());
            return execs;
        }

        public static IList<RegressionExecution> WithPropRootedNestedArrayProp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEPropRootedNestedArrayProp());
            return execs;
        }

        public static IList<RegressionExecution> WithPropRootedNestedNestedProp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEPropRootedNestedNestedProp(false));
            execs.Add(new ExprCoreAAEPropRootedNestedNestedProp(true));
            return execs;
        }

        public static IList<RegressionExecution> WithPropRootedNestedProp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEPropRootedNestedProp(false));
            execs.Add(new ExprCoreAAEPropRootedNestedProp(true));
            return execs;
        }

        public static IList<RegressionExecution> WithPropRootedTopLevelProp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAAEPropRootedTopLevelProp(false));
            execs.Add(new ExprCoreAAEPropRootedTopLevelProp(true));
            return execs;
        }

        private class ExprCoreAAEWithStringSplit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stringExtensions = typeof(StringExtensions).FullName;
                var epl = $"@Name('s0') select {stringExtensions}.SplitCsv('a,b')[IntPrimitive] as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("c0")));

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { "b" });

                env.UndeployAll();
            }
        }

        private class ExprCoreAAEAdditionalInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplNoAnArrayIsString =
                    "create schema Lvl3 (Id string);\n" +
                    "create schema Lvl2 (Lvl3 Lvl3);\n" +
                    "create schema Lvl1 (Lvl2 Lvl2);\n" +
                    "create schema Lvl0 (Lvl1 Lvl1, IndexNumber int);\n" +
                    "select Lvl1.Lvl2.Lvl3.Id[IndexNumber] from Lvl0;\n";
                env.TryInvalidCompile(
                    eplNoAnArrayIsString,
                    "Failed to validate select-clause expression 'Lvl1.Lvl2.Lvl3.Id[IndexNumber]': Could not perform array operation on type System.String");

                var eplNoAnArrayIsType =
                    "create schema Lvl3 (Id string);\n" +
                    "create schema Lvl2 (Lvl3 Lvl3);\n" +
                    "create schema Lvl1 (Lvl2 Lvl2);\n" +
                    "create schema Lvl0 (Lvl1 Lvl1, IndexNumber int);\n" +
                    "select Lvl1.Lvl2.Lvl3[IndexNumber] from Lvl0;\n";
                env.TryInvalidCompile(
                    eplNoAnArrayIsType,
                    "Failed to validate select-clause expression 'Lvl1.Lvl2.Lvl3[IndexNumber]': Could not perform array operation on type event type 'Lvl3'");
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
                var namespc = NamespaceGenerator.Create();
                var epl = "@name('s0') inlined_class \"\"\"\n" +
                          "  using com.espertech.esper.common.client.hook.singlerowfunc;\n" +
                          "  namespace " +
                          namespc +
                          " {\n" +
                          "    [ExtensionSingleRowFunction(Name=\"toArray\", MethodName=\"ToArray\")]\n" +
                          "    public class Helper {\n" +
                          "      public static int[] ToArray(int a, int b) {\n" +
                          "        return new int[] {a, b};\n" +
                          "      }\n" +
                          "    }\n" +
                          "  }\n" +
                          "\"\"\" " +
                          "select " +
                          typeof(ExprCoreArrayAtElement).FullName +
                          ".GetIntArray()[IntPrimitive] as c0, " +
                          typeof(ExprCoreArrayAtElement).FullName +
                          ".GetIntArray2Dim()[IntPrimitive][IntPrimitive] as c1, " +
                          "toArray(3,30)[IntPrimitive] as c2 " +
                          "from SupportBean";
                env.CompileDeploy(soda, epl).AddListener("s0");
                var fields = "c0,c1,c2".SplitCsv();
                env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { 10, 20, 30 });

                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertPropsNew("s0", fields, new object[] { 1, 1, 3 });

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        private class ExprCoreAAEVariableRootedChained : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    $"import {typeof(MyHolder).MaskTypeName()};\n" +
                    "create variable MyHolder[] var_mh = new MyHolder[] {new MyHolder('a'), new MyHolder('b')};\n" +
                    "@name('s0') select var_mh[IntPrimitive].get_Id() as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = "c0".SplitCsv();
                env.AssertStmtTypesAllSame("s0", fields, typeof(string));

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { "b" });

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
                var eplVariableIntArray = "@public create variable int[primitive] var_intarr = new int[] {1,2,3}";
                env.CompileDeploy(soda, eplVariableIntArray, path);
                var eplVariableSBArray = "@public create variable " + typeof(MyHolder).MaskTypeName() + " var_ = null";
                env.CompileDeploy(soda, eplVariableSBArray, path);

                var epl = "@name('s0') select var_intarr[IntPrimitive] as c0 from SupportBean";
                env.CompileDeploy(soda, epl, path).AddListener("s0");
                var fields = "c0".SplitCsv();
                env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { 2 });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        private class ExprCoreAAEPropRootedNestedNestedArrayProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplSchema =
                    "@public create schema Lvl2(Id string);\n" +
                    "@public create schema Lvl1(Lvl2 Lvl2[]);\n" +
                    "@public @buseventtype create schema Lvl0(Lvl1 Lvl1, IndexNumber int, Lvl0id string);\n";
                env.CompileDeploy(eplSchema, path);

                var epl =
                    "@name('s0') select Lvl1.Lvl2[IndexNumber].Id as c0, me.Lvl1.Lvl2[IndexNumber].Id as c1 from Lvl0 as me";
                env.CompileDeploy(epl, path).AddListener("s0");
                var fields = "c0,c1".SplitCsv();
                env.AssertStmtTypesAllSame("s0", fields, typeof(string));

                var lvl2One = CollectionUtil.BuildMap("Id", "a");
                var lvl2Two = CollectionUtil.BuildMap("Id", "b");
                var lvl1 = CollectionUtil.BuildMap("Lvl2", new IDictionary<string, object>[] { lvl2One, lvl2Two });
                var lvl0 = CollectionUtil.BuildMap("Lvl1", lvl1, "IndexNumber", 1);
                env.SendEventMap(lvl0, "Lvl0");
                env.AssertPropsNew("s0", fields, new object[] { "b", "b" });

                // Invalid tests
                // array value but no array provided
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Lvl2.Id from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Lvl2.Id': Failed to find a stream named 'Lvl1' (did you mean 'Lvl0'?)");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Lvl2.Id from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Lvl2.Id': Failed to resolve property 'me.Lvl1.Lvl2.Id' to a stream or nested property in a stream");

                // two index expressions
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Lvl2[IndexNumber, IndexNumber].Id from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Lvl2[IndexNumber,IndexNumber].Id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type collection of events of type 'Lvl2'");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Lvl2[IndexNumber, IndexNumber].Id from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Lvl2[IndexNumber,IndexNumber].Id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type collection of events of type 'Lvl2'");

                // double-array
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Lvl2[IndexNumber][IndexNumber].Id from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Lvl2[IndexNumber][IndexNumber].Id': Could not perform array operation on type event type 'Lvl2'");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Lvl2[IndexNumber][IndexNumber].Id from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Lvl2[IndexNumber][IndexNumb...(41 chars)': Could not perform array operation on type event type 'Lvl2'");

                // wrong index expression type
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Lvl2[Lvl0id].Id from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Lvl2[Lvl0id].Id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Lvl0id' returns 'System.String' for operation on type collection of events of type 'Lvl2'");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Lvl2[Lvl0id].Id from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Lvl2[Lvl0id].Id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Lvl0id' returns 'System.String' for operation on type collection of events of type 'Lvl2'");

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
                var eplSchema = "@public create schema Lvl2(Intarr int[]);\n" +
                                "@public create schema Lvl1(Lvl2 Lvl2);\n" +
                                "@public @buseventtype create schema Lvl0(Lvl1 Lvl1, IndexNumber int, Id string);\n";
                env.CompileDeploy(eplSchema, path);

                var epl = "@name('s0') select " +
                          "Lvl1.Lvl2.Intarr[IndexNumber] as c0, " +
                          "Lvl1.Lvl2.Intarr.size() as c1, " +
                          "me.Lvl1.Lvl2.Intarr[IndexNumber] as c2, " +
                          "me.Lvl1.Lvl2.Intarr.size() as c3 " +
                          "from Lvl0 as me";
                env.CompileDeploy(soda, epl, path).AddListener("s0");
                var fields = "c0,c1,c2,c3".SplitCsv();
                env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

                var lvl2 = CollectionUtil.BuildMap("Intarr", new int?[] { 1, 2, 3 });
                var lvl1 = CollectionUtil.BuildMap("Lvl2", lvl2);
                var lvl0 = CollectionUtil.BuildMap("Lvl1", lvl1, "IndexNumber", 2);
                env.SendEventMap(lvl0, "Lvl0");
                env.AssertPropsNew("s0", fields, new object[] { 3, 3, 3, 3 });

                // Invalid tests
                // not an index expression
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Lvl2[IndexNumber] from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Lvl2[IndexNumber]': Could not perform array operation on type event type 'Lvl2'");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Lvl2[IndexNumber] from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Lvl2[IndexNumber]': Could not perform array operation on type event type 'Lvl2'");

                // two index expressions
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Lvl2.Intarr[IndexNumber, IndexNumber] from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Lvl2.Intarr[IndexNumber,IndexN...(41 chars)': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type System.Int32[]");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Lvl2.Intarr[IndexNumber, IndexNumber] from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Lvl2.Intarr[IndexNumber,Ind...(44 chars)': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type System.Int32[]");

                // double-array
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Lvl2.Intarr[IndexNumber][IndexNumber] from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Lvl2.Intarr[IndexNumber][Index...(42 chars)': Could not perform array operation on type System.Nullable<System.Int32>");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Lvl2.Intarr[IndexNumber][IndexNumber] from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Lvl2.Intarr[IndexNumber][In...(45 chars)': Could not perform array operation on type System.Nullable<System.Int32>");

                // wrong index expression type
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Lvl2.Intarr[Id] from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Lvl2.Intarr[Id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Id' returns 'System.String' for operation on type System.Int32[]");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Lvl2.Intarr[Id] from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Lvl2.Intarr[Id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Id' returns 'System.String' for operation on type System.Int32[]");

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        private class ExprCoreAAEPropRootedNestedArrayProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create schema Lvl1(Id string);\n" +
                    "@public @buseventtype create schema Lvl0(Lvl1 Lvl1[], IndexNumber int, Lvl0id string);\n" +
                    "@name('s0') select Lvl1[IndexNumber].Id as c0, me.Lvl1[IndexNumber].Id as c1 from Lvl0 as me";
                env.CompileDeploy(epl, path).AddListener("s0");
                var fields = "c0,c1".SplitCsv();
                env.AssertStmtTypesAllSame("s0", fields, typeof(string));

                var lvl1One = CollectionUtil.BuildMap("Id", "a");
                var lvl1Two = CollectionUtil.BuildMap("Id", "b");
                var lvl0 = CollectionUtil.BuildMap(
                    "Lvl1",
                    new IDictionary<string, object>[] { lvl1One, lvl1Two },
                    "IndexNumber",
                    1);
                env.SendEventMap(lvl0, "Lvl0");
                env.AssertPropsNew("s0", fields, new object[] { "b", "b" });

                #if false
                // Invalid tests
                // array value but no array provided
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Id from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Id': Failed to resolve property 'Lvl1.Id' (property 'Lvl1' is an indexed property and requires an index or enumeration method to access values)");
                #endif
                
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Id from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Id': Property named 'Lvl1.Id' is not valid in stream 'me' (did you mean 'Lvl1'?)");
                    //"Failed to validate select-clause expression 'me.Lvl1.Id': Property named 'Lvl1.Id' is not valid in stream 'me' (did you mean 'Lvl0Id'?)");

                // not an index expression
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Id[IndexNumber] from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Id[IndexNumber]': Could not find event property or method named 'Id' in collection of events of type 'Lvl1'");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Id[IndexNumber] from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Id[IndexNumber]': Could not find event property or method named 'Id' in collection of events of type 'Lvl1'");

                // two index expressions
                env.TryInvalidCompile(
                    path,
                    "select Lvl1[IndexNumber, IndexNumber].Id from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1[IndexNumber,IndexNumber].Id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'Lvl1'");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1[IndexNumber, IndexNumber].Id from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1[IndexNumber,IndexNumber].Id': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'Lvl1'");

                // double-array
                env.TryInvalidCompile(
                    path,
                    "select Lvl1[IndexNumber][IndexNumber].Id from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1[IndexNumber][IndexNumber].Id': Could not perform array operation on type event type 'Lvl1'");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1[IndexNumber][IndexNumber].Id from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1[IndexNumber][IndexNumber].Id': Could not perform array operation on type event type 'Lvl1'");

                // wrong index expression type
                env.TryInvalidCompile(
                    path,
                    "select Lvl1[Lvl0id].Id from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1[Lvl0id].Id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Lvl0id' returns 'System.String' for property 'Lvl1'");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1[Lvl0id].Id from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1[Lvl0id].Id': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Lvl0id' returns 'System.String' for property 'Lvl1'");

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
                    "@public create schema Lvl1(Intarr int[]);\n" +
                    "@public @buseventtype create schema Lvl0(Lvl1 Lvl1, IndexNumber int, Id string);\n";
                env.CompileDeploy(eplSchema, path);

                var epl = "@name('s0') select " +
                          "Lvl1.Intarr[IndexNumber] as c0, " +
                          "Lvl1.Intarr.size() as c1, " +
                          "me.Lvl1.Intarr[IndexNumber] as c2, " +
                          "me.Lvl1.Intarr.size() as c3 " +
                          "from Lvl0 as me";
                env.CompileDeploy(soda, epl, path).AddListener("s0");
                var fields = "c0,c1,c2,c3".SplitCsv();
                env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

                var lvl1 = CollectionUtil.BuildMap("Intarr", new int?[] { 1, 2, 3 });
                var lvl0 = CollectionUtil.BuildMap("Lvl1", lvl1, "IndexNumber", 2);
                env.SendEventMap(lvl0, "Lvl0");
                env.AssertPropsNew("s0", fields, new object[] { 3, 3, 3, 3 });

                // Invalid tests
                // not an index expression
                env.TryInvalidCompile(
                    path,
                    "select Lvl1[IndexNumber] from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1[IndexNumber]': Invalid array operation for property 'Lvl1'");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1[IndexNumber] from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1[IndexNumber]': Invalid array operation for property 'Lvl1'");

                // two index expressions
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Intarr[IndexNumber, IndexNumber] from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Intarr[IndexNumber,IndexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type System.Int32[]");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Intarr[IndexNumber, IndexNumber] from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Intarr[IndexNumber,IndexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for operation on type System.Int32[]");

                // double-array
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Intarr[IndexNumber][IndexNumber] from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Intarr[IndexNumber][IndexNumber]': Could not perform array operation on type System.Nullable<System.Int32>");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Intarr[IndexNumber][IndexNumber] from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Intarr[IndexNumber][IndexNumber]': Could not perform array operation on type System.Nullable<System.Int32>");

                // wrong index expression type
                env.TryInvalidCompile(
                    path,
                    "select Lvl1.Intarr[Id] from Lvl0",
                    "Failed to validate select-clause expression 'Lvl1.Intarr[Id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Id' returns 'System.String' for operation on type System.Int32[]");
                env.TryInvalidCompile(
                    path,
                    "select me.Lvl1.Intarr[Id] from Lvl0 as me",
                    "Failed to validate select-clause expression 'me.Lvl1.Intarr[Id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Id' returns 'System.String' for operation on type System.Int32[]");

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
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
                          "Intarr[IndexNumber] as c0, " +
                          "Intarr.size() as c1, " +
                          "me.Intarr[IndexNumber] as c2, " +
                          "me.Intarr.size() as c3 " +
                          "from SupportBeanWithArray as me";
                env.CompileDeploy(soda, epl).AddListener("s0");
                var fields = "c0,c1,c2,c3".SplitCsv();
                env.AssertStmtTypesAllSame("s0", fields, typeof(int?));

                env.SendEventBean(new SupportBeanWithArray(1, new int[] { 1, 2 }));
                env.AssertPropsNew("s0", fields, new object[] { 2, 2, 2, 2 });

                // Invalid tests
                // two index expressions
                env.TryInvalidCompile(
                    "select Intarr[IndexNumber, IndexNumber] from SupportBeanWithArray",
                    "Failed to validate select-clause expression 'Intarr[IndexNumber,IndexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'Intarr'");
                env.TryInvalidCompile(
                    "select me.Intarr[IndexNumber, IndexNumber] from SupportBeanWithArray as me",
                    "Failed to validate select-clause expression 'me.Intarr[IndexNumber,IndexNumber]': Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received 2 expressions for property 'Intarr'");

                // double-array
                env.TryInvalidCompile(
                    "select Intarr[IndexNumber][IndexNumber] from SupportBeanWithArray",
                    "Failed to validate select-clause expression 'Intarr[IndexNumber][IndexNumber]': Could not perform array operation on type System.Nullable<System.Int32>");
                env.TryInvalidCompile(
                    "select me.Intarr[IndexNumber][IndexNumber] from SupportBeanWithArray as me",
                    "Failed to validate select-clause expression 'me.Intarr[IndexNumber][IndexNumber]': Could not perform array operation on type System.Nullable<System.Int32>");

                // wrong index expression type
                env.TryInvalidCompile(
                    "select Intarr[Id] from SupportBeanWithArray",
                    "Failed to validate select-clause expression 'Intarr[Id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Id' returns 'System.String' for property 'Intarr'");
                env.TryInvalidCompile(
                    "select me.Intarr[Id] from SupportBeanWithArray as me",
                    "Failed to validate select-clause expression 'me.Intarr[Id]': Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'Id' returns 'System.String' for property 'Intarr'");

                // not an array
                env.TryInvalidCompile(
                    "select IndexNumber[IndexNumber] from SupportBeanWithArray",
                    "Failed to validate select-clause expression 'IndexNumber[IndexNumber]': Invalid array operation for property 'IndexNumber'");
                env.TryInvalidCompile(
                    "select me.IndexNumber[IndexNumber] from SupportBeanWithArray as me",
                    "Failed to validate select-clause expression 'me.IndexNumber[IndexNumber]': Invalid array operation for property 'IndexNumber'");

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        public static int[] GetIntArray()
        {
            return new int[] { 1, 10 };
        }

        public static int[][] GetIntArray2Dim()
        {
            return new int[][] { new int[] { 1, 10 }, new int[] { 2, 20 } };
        }

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