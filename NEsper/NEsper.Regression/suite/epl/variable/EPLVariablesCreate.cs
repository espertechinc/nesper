///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariablesCreate
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLVariableOM());
            execs.Add(new EPLVariableCompileStartStop());
            execs.Add(new EPLVariableSubscribeAndIterate());
            execs.Add(new EPLVariableDeclarationAndSelect());
            execs.Add(new EPLVariableInvalid());
            execs.Add(new EPLVariableDimensionAndPrimitive());
            return execs;
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

        internal class EPLVariableDimensionAndPrimitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('vars') create variable int[primitive] int_prim = null;\n" +
                    "create variable int[] int_boxed = null;\n" +
                    "create variable System.Object[] objectarray = null;\n" +
                    "create variable System.Object[][] objectarray_2dim = null;\n";
                var id = env.CompileDeploy(epl).DeploymentId("vars");

                RunAssertionSetGet(
                    env,
                    id,
                    "int_prim",
                    new[] {1, 2},
                    (
                        expected,
                        received) => CollectionAssert.AreEqual(
                        (IEnumerable) expected,
                        (IEnumerable) received));
                RunAssertionGetSetInvalid(env, id, "int_prim", new string[0]);

                RunAssertionSetGet(
                    env,
                    id,
                    "int_boxed",
                    new int?[] {1, 2},
                    (
                        expected,
                        received) => CollectionAssert.AreEqual(
                        (IEnumerable) expected,
                        (IEnumerable) received));
                RunAssertionGetSetInvalid(env, id, "int_boxed", new int[0]);

                RunAssertionSetGet(
                    env,
                    id,
                    "objectarray",
                    new object[] {1, 2},
                    (
                        expected,
                        received) => CollectionAssert.AreEqual(
                        (IEnumerable) expected,
                        (IEnumerable) received));
                RunAssertionGetSetInvalid(env, id, "objectarray", new int[0]);

                RunAssertionSetGet(
                    env,
                    id,
                    "objectarray_2dim",
                    new[] {new object[] {1, 2}},
                    (
                        expected,
                        received) => CollectionAssert.AreEqual(
                        (IEnumerable) expected,
                        (IEnumerable) received));
                RunAssertionGetSetInvalid(env, id, "objectarray_2dim", new int[0]);

                env.UndeployAll();
            }
        }

        internal class EPLVariableOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var model = new EPStatementObjectModel();
                model.CreateVariable = CreateVariableClause.Create("long", "var1OMCreate", null);
                env.CompileDeploy(model, path);
                Assert.AreEqual("create variable long var1OMCreate", model.ToEPL());

                model = new EPStatementObjectModel();
                model.CreateVariable = CreateVariableClause.Create(
                    "string",
                    "var2OMCreate",
                    Expressions.Constant("abc"));
                env.CompileDeploy(model, path);
                Assert.AreEqual("create variable string var2OMCreate = \"abc\"", model.ToEPL());

                var stmtTextSelect = "@Name('s0') select var1OMCreate, var2OMCreate from SupportBean";
                env.CompileDeploy(stmtTextSelect, path).AddListener("s0");

                string[] fieldsVar = {"var1OMCreate", "var2OMCreate"};
                SendSupportBean(env, "E1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {null, "abc"});

                env.CompileDeploy("create variable double[] arrdouble = {1.0d,2.0d}");

                env.UndeployAll();
            }
        }

        internal class EPLVariableCompileStartStop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var text = "create variable long var1CSS";
                env.EplToModelCompileDeploy(text, path);

                text = "create variable string var2CSS = \"abc\"";
                env.EplToModelCompileDeploy(text, path);

                var stmtTextSelect = "@Name('s0') select var1CSS, var2CSS from SupportBean";
                env.CompileDeploy(stmtTextSelect, path).AddListener("s0");

                string[] fieldsVar = {"var1CSS", "var2CSS"};
                SendSupportBean(env, "E1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsVar,
                    new object[] {null, "abc"});

                // ESPER-545
                var createText = "@Name('create') create variable int FOO = 0";
                env.CompileDeploy(createText, path);
                env.CompileDeploy("on pattern [every SupportBean] set FOO = FOO + 1", path);
                env.SendEventBean(new SupportBean());
                Assert.AreEqual(1, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "FOO"));

                env.UndeployAll();

                env.CompileDeploy(createText);
                Assert.AreEqual(0, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "FOO"));

                // cleanup of variable when statement exception occurs
                env.CompileDeploy("create variable int x = 123");
                TryInvalidCompile(env, "select missingScript(x) from SupportBean", "skip");
                env.CompileDeploy("create variable int x = 123");

                env.UndeployAll();
            }
        }

        internal class EPLVariableSubscribeAndIterate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtCreateTextOne = "@Name('create-one') create variable long var1SAI = null";
                env.CompileDeploy(stmtCreateTextOne, path);
                Assert.AreEqual(
                    StatementType.CREATE_VARIABLE,
                    env.Statement("create-one").GetProperty(StatementProperty.STATEMENTTYPE));
                env.AddListener("create-one");

                string[] fieldsVar1 = {"var1SAI"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create-one"),
                    fieldsVar1,
                    new[] {new object[] {null}});
                Assert.IsFalse(env.Listener("create-one").IsInvoked);

                var typeCreateOne = env.Statement("create-one").EventType;
                Assert.AreEqual(typeof(long?), typeCreateOne.GetPropertyType("var1SAI"));
                Assert.AreEqual(typeof(IDictionary<string, object>), typeCreateOne.UnderlyingType);
                CollectionAssert.AreEquivalent(new[] {"var1SAI"}, typeCreateOne.PropertyNames);

                var stmtCreateTextTwo = "@Name('create-two') create variable long var2SAI = 20";
                env.CompileDeploy(stmtCreateTextTwo, path).AddListener("create-two");
                string[] fieldsVar2 = {"var2SAI"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create-two"),
                    fieldsVar2,
                    new[] {new object[] {20L}});
                Assert.IsFalse(env.Listener("create-two").IsInvoked);

                var stmtTextSet = "@Name('set') on SupportBean set var1SAI = IntPrimitive * 2, var2SAI = var1SAI + 1";
                env.CompileDeploy(stmtTextSet, path);

                SendSupportBean(env, "E1", 100);
                EPAssertionUtil.AssertProps(
                    env.Listener("create-one").LastNewData[0],
                    fieldsVar1,
                    new object[] {200L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create-one").LastOldData[0],
                    fieldsVar1,
                    new object[] {null});
                env.Listener("create-one").Reset();
                EPAssertionUtil.AssertProps(
                    env.Listener("create-two").LastNewData[0],
                    fieldsVar2,
                    new object[] {201L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create-two").LastOldData[0],
                    fieldsVar2,
                    new object[] {20L});
                env.Listener("create-one").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create-one").GetEnumerator(),
                    fieldsVar1,
                    new[] {new object[] {200L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create-two").GetEnumerator(),
                    fieldsVar2,
                    new[] {new object[] {201L}});

                env.Milestone(0);

                SendSupportBean(env, "E2", 200);
                EPAssertionUtil.AssertProps(
                    env.Listener("create-one").LastNewData[0],
                    fieldsVar1,
                    new object[] {400L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create-one").LastOldData[0],
                    fieldsVar1,
                    new object[] {200L});
                env.Listener("create-one").Reset();
                EPAssertionUtil.AssertProps(
                    env.Listener("create-two").LastNewData[0],
                    fieldsVar2,
                    new object[] {401L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create-two").LastOldData[0],
                    fieldsVar2,
                    new object[] {201L});
                env.Listener("create-one").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create-one").GetEnumerator(),
                    fieldsVar1,
                    new[] {new object[] {400L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create-two").GetEnumerator(),
                    fieldsVar2,
                    new[] {new object[] {401L}});

                env.UndeployModuleContaining("set");
                env.UndeployModuleContaining("create-two");
                env.CompileDeploy(stmtCreateTextTwo);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create-one").GetEnumerator(),
                    fieldsVar1,
                    new[] {new object[] {400L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create-two").GetEnumerator(),
                    fieldsVar2,
                    new[] {new object[] {20L}});
                env.UndeployAll();
            }
        }

        internal class EPLVariableDeclarationAndSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                object[][] variables = {
                    new object[] {"varX1", "int", "1", 1},
                    new object[] {"varX2", "int", "'2'", 2},
                    new object[] {"varX3", "INTEGER", " 3+2 ", 5},
                    new object[] {"varX4", "bool", " true|false ", true},
                    new object[] {"varX5", "boolean", " varX1=1 ", true},
                    new object[] {"varX6", "double", " 1.11 ", 1.11d},
                    new object[] {"varX7", "double", " 1.20d ", 1.20d},
                    new object[] {"varX8", "Double", " ' 1.12 ' ", 1.12d},
                    new object[] {"varX9", "float", " 1.13f*2f ", 2.26f},
                    new object[] {"varX10", "FLOAT", " -1.14f ", -1.14f},
                    new object[] {"varX11", "string", " ' XXXX ' ", " XXXX "},
                    new object[] {"varX12", "string", " \"a\" ", "a"},
                    new object[] {"varX13", "character", "'a'", 'a'},
                    new object[] {"varX14", "char", "'x'", 'x'},
                    new object[] {"varX15", "short", " 20 ", (short) 20},
                    new object[] {"varX16", "SHORT", " ' 9 ' ", (short) 9},
                    new object[] {"varX17", "long", " 20*2 ", (long) 40},
                    new object[] {"varX18", "LONG", " ' 9 ' ", (long) 9},
                    new object[] {"varX19", "byte", " 20*2 ", (byte) 40},
                    new object[] {"varX20", "BYTE", "9+1", (byte) 10},
                    new object[] {"varX21", "int", null, null},
                    new object[] {"varX22", "bool", null, null},
                    new object[] {"varX23", "double", null, null},
                    new object[] {"varX24", "float", null, null},
                    new object[] {"varX25", "string", null, null},
                    new object[] {"varX26", "char", null, null},
                    new object[] {"varX27", "short", null, null},
                    new object[] {"varX28", "long", null, null},
                    new object[] {"varX29", "BYTE", null, null}
                };

                var path = new RegressionPath();
                for (var i = 0; i < variables.Length; i++) {
                    var text = "create variable " + variables[i][1] + " " + variables[i][0];
                    if (variables[i][2] != null) {
                        text += " = " + variables[i][2];
                    }

                    env.CompileDeploy(text, path);
                }

                env.Milestone(0);

                // select all variables
                var buf = new StringBuilder();
                var delimiter = "";
                buf.Append("@Name('s0') select ");
                for (var i = 0; i < variables.Length; i++) {
                    buf.Append(delimiter);
                    buf.Append(variables[i][0]);
                    delimiter = ",";
                }

                buf.Append(" from SupportBean");
                env.CompileDeploy(buf.ToString(), path).AddListener("s0");

                // assert initialization values
                SendSupportBean(env, "E1", 1);
                var received = env.Listener("s0").AssertOneGetNewAndReset();
                for (var i = 0; i < variables.Length; i++) {
                    Assert.AreEqual(variables[i][3], received.Get((string) variables[i][0]));
                }

                env.UndeployAll();
            }
        }

        internal class EPLVariableInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = "create variable somedummy myvar = 10";
                TryInvalidCompile(
                    env,
                    stmt,
                    "Cannot create variable 'myvar', type 'somedummy' is not a recognized type [create variable somedummy myvar = 10]");

                stmt = "create variable string myvar = 5";
                TryInvalidCompile(
                    env,
                    stmt,
                    "Variable 'myvar' of declared type System.String cannot be initialized by a value of type System.Int32 [create variable string myvar = 5]");

                stmt = "create variable string myvar = 'a'";
                var path = new RegressionPath();
                env.CompileDeploy("create variable string myvar = 'a'", path);
                TryInvalidCompile(env, path, stmt, "A variable by name 'myvar' has already been declared");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean output every somevar events",
                    "Error in the output rate limiting clause: Variable named 'somevar' has not been declared [");

                env.UndeployAll();
            }
        }
    }
} // end of namespace