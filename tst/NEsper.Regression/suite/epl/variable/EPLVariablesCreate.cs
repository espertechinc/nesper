///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariablesCreate
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOM(execs);
            WithCompileStartStop(execs);
            WithSubscribeAndIterate(execs);
            WithDeclarationAndSelect(execs);
            WithInvalid(execs);
            WithDimensionAndPrimitive(execs);
            WithGenericType(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGenericType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableGenericType());
            return execs;
        }

        public static IList<RegressionExecution> WithDimensionAndPrimitive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableDimensionAndPrimitive());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithDeclarationAndSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableDeclarationAndSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithSubscribeAndIterate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableSubscribeAndIterate());
            return execs;
        }

        public static IList<RegressionExecution> WithCompileStartStop(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableCompileStartStop());
            return execs;
        }

        public static IList<RegressionExecution> WithOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOM());
            return execs;
        }

        private class EPLVariableGenericType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('var') create variable List<String> mylist = Arrays.asList('a', 'b');\n" +
                          "@name('s0') select mylist as c0, mylist.where(v => v = 'a') as c1 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStmtTypes(
                    "s0",
                    "c0,c1".SplitCsv(),
                    new Type[] {
                        typeof(IList<string>), typeof(ICollection<string>)
                    });

                env.Milestone(0);

                env.AssertThat(
                    () => {
                        var list = (IList<string>)env.Runtime.VariableService.GetVariableValue(
                            env.DeploymentId("var"),
                            "mylist");
                        EPAssertionUtil.AssertEqualsExactOrder("a,b".SplitCsv(), list.ToArray());
                    });

                env.SendEventBean(new SupportBean());
                env.AssertEventNew(
                    "s0",
                    received => {
                        EPAssertionUtil.AssertEqualsExactOrder(
                            "a,b".SplitCsv(),
                            received.Get("c0").UnwrapIntoArray<object>());
                        EPAssertionUtil.AssertEqualsExactOrder(
                            "a".SplitCsv(),
                            received.Get("c1").UnwrapIntoArray<object>());
                    });

                env.UndeployAll();
            }
        }

        private class EPLVariableDimensionAndPrimitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('vars') create variable int[primitive] int_prim = null;\n" +
                    "create variable int[] int_boxed = null;\n" +
                    "create variable System.Object[] objectarray = null;\n" +
                    "create variable System.Object[][] objectarray_2dim = null;\n";
                var id = env.CompileDeploy(epl).DeploymentId("vars");

                RunAssertionSetGet(
                    env,
                    id,
                    "int_prim",
                    new int[] { 1, 2 },
                    (
                        expected,
                        received) => EPAssertionUtil.AssertEqualsExactOrder((int[])expected, (int[])received));
                RunAssertionGetSetInvalid(env, id, "int_prim", Array.Empty<string>());

                RunAssertionSetGet(
                    env,
                    id,
                    "int_boxed",
                    new int?[] { 1, 2 },
                    (
                        expected,
                        received) => EPAssertionUtil.AssertEqualsExactOrder((object[])expected, (object[])received));
                RunAssertionGetSetInvalid(env, id, "int_boxed", Array.Empty<int>());

                RunAssertionSetGet(
                    env,
                    id,
                    "objectarray",
                    new int?[] { 1, 2 },
                    (
                        expected,
                        received) => EPAssertionUtil.AssertEqualsExactOrder((object[])expected, (object[])received));
                RunAssertionGetSetInvalid(env, id, "objectarray", Array.Empty<int>());

                RunAssertionSetGet(
                    env,
                    id,
                    "objectarray_2dim",
                    new object[][] { new object[] { 1, 2 } },
                    (
                        expected,
                        received) => EPAssertionUtil.AssertEqualsExactOrder((object[])expected, (object[])received));
                RunAssertionGetSetInvalid(env, id, "objectarray_2dim", Array.Empty<int>());

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private static void RunAssertionSetGet(
            RegressionEnvironment env,
            string deploymentId,
            string variableName,
            object value,
            BiConsumer<object, object> assertion)
        {
            env.Runtime.VariableService.SetVariableValue(deploymentId, variableName, value);
            var returned = env.Runtime.VariableService.GetVariableValue(deploymentId, variableName);
            assertion.Invoke(value, returned);
        }

        private static void RunAssertionGetSetInvalid(
            RegressionEnvironment env,
            string id,
            string variableName,
            object value)
        {
            try {
                env.Runtime.VariableService.SetVariableValue(id, variableName, value);
                Assert.Fail();
            }
            catch (VariableValueException) {
                // expected
            }
        }

        private class EPLVariableOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var model = new EPStatementObjectModel();
                model.Annotations = Collections.SingletonList(new AnnotationPart("public"));
                model.CreateVariable = CreateVariableClause.Create("long", "var1OMCreate", null);
                env.CompileDeploy(model, path);
                ClassicAssert.AreEqual("@public create variable long var1OMCreate", model.ToEPL());

                model = new EPStatementObjectModel();
                model.Annotations = Collections.SingletonList(new AnnotationPart("public"));
                model.CreateVariable = CreateVariableClause.Create(
                    "string",
                    "var2OMCreate",
                    Expressions.Constant("abc"));
                env.CompileDeploy(model, path);
                ClassicAssert.AreEqual("@public create variable string var2OMCreate = \"abc\"", model.ToEPL());

                var stmtTextSelect = "@name('s0') select var1OMCreate, var2OMCreate from SupportBean";
                env.CompileDeploy(stmtTextSelect, path).AddListener("s0");

                var fieldsVar = new string[] { "var1OMCreate", "var2OMCreate" };
                SendSupportBean(env, "E1", 10);
                env.AssertPropsNew("s0", fieldsVar, new object[] { null, "abc" });

                env.CompileDeploy("create variable double[] arrdouble = {1.0d,2.0d}");

                env.UndeployAll();
            }
        }

        private class EPLVariableCompileStartStop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var text = "@public create variable long var1CSS";
                env.EplToModelCompileDeploy(text, path);

                text = "@public create variable string var2CSS = \"abc\"";
                env.EplToModelCompileDeploy(text, path);

                var stmtTextSelect = "@name('s0') select var1CSS, var2CSS from SupportBean";
                env.CompileDeploy(stmtTextSelect, path).AddListener("s0");

                var fieldsVar = new string[] { "var1CSS", "var2CSS" };
                SendSupportBean(env, "E1", 10);
                env.AssertPropsNew("s0", fieldsVar, new object[] { null, "abc" });

                // ESPER-545
                var createText = "@name('create') @public create variable int FOO = 0";
                env.CompileDeploy(createText, path);
                env.CompileDeploy("on pattern [every SupportBean] set FOO = FOO + 1", path);
                env.SendEventBean(new SupportBean());
                env.AssertThat(
                    () => ClassicAssert.AreEqual(
                        1,
                        env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "FOO")));

                env.UndeployAll();

                env.CompileDeploy(createText);
                env.AssertThat(
                    () => ClassicAssert.AreEqual(
                        0,
                        env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "FOO")));

                // cleanup of variable when statement exception occurs
                env.CompileDeploy("@private create variable int x = 123");
                env.TryInvalidCompile("select missingScript(x) from SupportBean", "skip");
                env.CompileDeploy("@private create variable int x = 123");

                env.UndeployAll();
            }
        }

        private class EPLVariableSubscribeAndIterate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtCreateTextOne = "@name('create-one') @public create variable long var1SAI = null";
                env.CompileDeploy(stmtCreateTextOne, path).AddListener("create-one");
                env.AssertStatement(
                    "create-one",
                    statement => {
                        ClassicAssert.AreEqual(
                            StatementType.CREATE_VARIABLE,
                            statement.GetProperty(StatementProperty.STATEMENTTYPE));
                        ClassicAssert.AreEqual("var1SAI", statement.GetProperty(StatementProperty.CREATEOBJECTNAME));
                    });

                var fieldsVar1 = new string[] { "var1SAI" };
                env.AssertPropsPerRowIterator("create-one", fieldsVar1, new object[][] { new object[] { null } });
                env.AssertListenerNotInvoked("create-one");

                env.AssertStatement(
                    "create-one",
                    statement => {
                        var typeCreateOne = statement.EventType;
                        ClassicAssert.AreEqual(typeof(long?), typeCreateOne.GetPropertyType("var1SAI"));
                        ClassicAssert.AreEqual(typeof(IDictionary<string, object>), typeCreateOne.UnderlyingType);
                        CollectionAssert.AreEqual(typeCreateOne.PropertyNames, new string[] { "var1SAI" });
                    });

                var stmtCreateTextTwo = "@name('create-two') @public create variable long var2SAI = 20";
                env.CompileDeploy(stmtCreateTextTwo, path).AddListener("create-two");
                var fieldsVar2 = new string[] { "var2SAI" };
                env.AssertPropsPerRowIterator("create-two", fieldsVar2, new object[][] { new object[] { 20L } });
                env.AssertListenerNotInvoked("create-two");

                var stmtTextSet = "@name('set') on SupportBean set var1SAI = IntPrimitive * 2, var2SAI = var1SAI + 1";
                env.CompileDeploy(stmtTextSet, path);

                SendSupportBean(env, "E1", 100);
                env.AssertPropsIRPair("create-one", fieldsVar1, new object[] { 200L }, new object[] { null });
                env.AssertPropsIRPair("create-two", fieldsVar2, new object[] { 201L }, new object[] { 20L });
                env.AssertPropsPerRowIterator("create-one", fieldsVar1, new object[][] { new object[] { 200L } });
                env.AssertPropsPerRowIterator("create-two", fieldsVar2, new object[][] { new object[] { 201L } });

                env.Milestone(0);

                SendSupportBean(env, "E2", 200);
                env.AssertPropsIRPair("create-one", fieldsVar1, new object[] { 400L }, new object[] { 200L });
                env.AssertPropsIRPair("create-two", fieldsVar2, new object[] { 401L }, new object[] { 201L });
                env.AssertPropsPerRowIterator("create-one", fieldsVar1, new object[][] { new object[] { 400L } });
                env.AssertPropsPerRowIterator("create-two", fieldsVar2, new object[][] { new object[] { 401L } });

                env.UndeployModuleContaining("set");
                env.UndeployModuleContaining("create-two");
                env.CompileDeploy(stmtCreateTextTwo);

                env.AssertPropsPerRowIterator("create-one", fieldsVar1, new object[][] { new object[] { 400L } });
                env.AssertPropsPerRowIterator("create-two", fieldsVar2, new object[][] { new object[] { 20L } });
                env.UndeployAll();
            }
        }

        private class EPLVariableDeclarationAndSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var variables = new object[][] {
                    new object[] { "varX1", "int", "1", 1 },
                    new object[] { "varX2", "int", "'2'", 2 },
                    new object[] { "varX3", "INTEGER", " 3+2 ", 5 },
                    new object[] { "varX4", "bool", " true|false ", true },
                    new object[] { "varX5", "boolean", " varX1=1 ", true },
                    new object[] { "varX6", "double", " 1.11 ", 1.11d },
                    new object[] { "varX7", "double", " 1.20d ", 1.20d },
                    new object[] { "varX8", "Double", " ' 1.12 ' ", 1.12d },
                    new object[] { "varX9", "float", " 1.13f*2f ", 2.26f },
                    new object[] { "varX10", "FLOAT", " -1.14f ", -1.14f },
                    new object[] { "varX11", "string", " ' XXXX ' ", " XXXX " },
                    new object[] { "varX12", "string", " \"a\" ", "a" },
                    new object[] { "varX13", "character", "'a'", 'a' },
                    new object[] { "varX14", "char", "'x'", 'x' },
                    new object[] { "varX15", "short", " 20 ", (short)20 },
                    new object[] { "varX16", "SHORT", " ' 9 ' ", (short)9 },
                    new object[] { "varX17", "long", " 20*2 ", (long)40 },
                    new object[] { "varX18", "LONG", " ' 9 ' ", (long)9 },
                    new object[] { "varX19", "byte", " 20*2 ", (byte)40 },
                    new object[] { "varX20", "BYTE", "9+1", (byte)10 },
                    new object[] { "varX21", "int", null, null },
                    new object[] { "varX22", "bool", null, null },
                    new object[] { "varX23", "double", null, null },
                    new object[] { "varX24", "float", null, null },
                    new object[] { "varX25", "string", null, null },
                    new object[] { "varX26", "char", null, null },
                    new object[] { "varX27", "short", null, null },
                    new object[] { "varX28", "long", null, null },
                    new object[] { "varX29", "BYTE", null, null },
                };

                var path = new RegressionPath();
                for (var i = 0; i < variables.Length; i++) {
                    var text = "@public create variable " + variables[i][1] + " " + variables[i][0];
                    if (variables[i][2] != null) {
                        text += " = " + variables[i][2];
                    }

                    env.CompileDeploy(text, path);
                }

                env.Milestone(0);

                // select all variables
                var buf = new StringBuilder();
                var delimiter = "";
                buf.Append("@name('s0') select ");
                for (var i = 0; i < variables.Length; i++) {
                    buf.Append(delimiter);
                    buf.Append(variables[i][0]);
                    delimiter = ",";
                }

                buf.Append(" from SupportBean");
                env.CompileDeploy(buf.ToString(), path).AddListener("s0");

                // assert initialization values
                SendSupportBean(env, "E1", 1);
                env.AssertEventNew(
                    "s0",
                    received => {
                        for (var i = 0; i < variables.Length; i++) {
                            ClassicAssert.AreEqual(
                                variables[i][3],
                                received.Get((string)variables[i][0]),
                                "Failed for " + CompatExtensions.Render(variables[i]));
                        }
                    });

                env.UndeployAll();
            }
        }

        private class EPLVariableInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = "create variable somedummy myvar = 10";
                env.TryInvalidCompile(
                    stmt,
                    "Cannot create variable 'myvar', type 'somedummy' is not a recognized type [create variable somedummy myvar = 10]");

                stmt = "create variable string myvar = 5";
                env.TryInvalidCompile(
                    stmt,
                    "Variable 'myvar' of declared type String cannot be initialized by a value of type Integer [create variable string myvar = 5]");

                stmt = "@public create variable string myvar = 'a'";
                var path = new RegressionPath();
                env.CompileDeploy("@public create variable string myvar = 'a'", path);
                env.TryInvalidCompile(path, stmt, "A variable by name 'myvar' has already been declared");

                env.TryInvalidCompile(
                    "select * from SupportBean output every somevar events",
                    "Failed to validate the output rate limiting clause: Variable named 'somevar' has not been declared [");

                env.TryInvalidCompile(
                    "create variable SupportBean<Integer> sb",
                    "Cannot create variable 'sb', type 'SupportBean' cannot be declared as an array type and cannot receive type parameters as it is an event type");

                env.UndeployAll();
            }
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }
    }
} // end of namespace