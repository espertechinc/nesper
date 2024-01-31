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
using System.Threading;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariablesOnSet
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithSimpleSceneTwo(execs);
            WithCompile(execs);
            WithObjectModel(execs);
            WithWithFilter(execs);
            WithSubquery(execs);
            WithWDeploy(execs);
            WithAssignmentOrderNoDup(execs);
            WithAssignmentOrderDup(execs);
            WithRuntimeOrderMultiple(execs);
            WithCoercion(execs);
            WithInvalid(execs);
            WithSubqueryMultikeyWArray(execs);
            WithArrayAtIndex(execs);
            WithArrayBoxed(execs);
            WithArrayInvalid(execs);
            WithExpression(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetArrayInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayBoxed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetArrayBoxed());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayAtIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetArrayAtIndex(false));
            execs.Add(new EPLVariableOnSetArrayAtIndex(true));
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetSubqueryMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithCoercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithRuntimeOrderMultiple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetRuntimeOrderMultiple());
            return execs;
        }

        public static IList<RegressionExecution> WithAssignmentOrderDup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetAssignmentOrderDup());
            return execs;
        }

        public static IList<RegressionExecution> WithAssignmentOrderNoDup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetAssignmentOrderNoDup());
            return execs;
        }

        public static IList<RegressionExecution> WithWDeploy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetWDeploy());
            return execs;
        }

        public static IList<RegressionExecution> WithSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithWithFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetWithFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectModel(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetObjectModel());
            return execs;
        }

        public static IList<RegressionExecution> WithCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetSimpleSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOnSetSimple());
            return execs;
        }

        private class EPLVariableOnSetExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "import " +
                    typeof(MyLocalVariable).FullName +
                    ";\n" +
                    "@name('var') create variable MyLocalVariable VAR = new MyLocalVariable(1, 10);\n" +
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
                    "@name('s0') on SupportBean set Helper.swap(VAR);\n";
                env.CompileDeploy(epl).AddListener("s0");

                AssertVariable(env, 1, 10);

                env.SendEventBean(new SupportBean());

                AssertVariable(env, 10, 1);

                env.UndeployAll();

                var eplInvalid =
                    "import " +
                    typeof(MyLocalVariable).FullName +
                    ";\n" +
                    "@name('var') create variable MyLocalVariable VARONE = new MyLocalVariable(1, 10);\n" +
                    "@name('var') create variable MyLocalVariable VARTWO = new MyLocalVariable(1, 10);\n" +
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
                    "@name('s0') on SupportBean set Helper.swap(VARONE, VARTWO);\n";
                env.TryInvalidCompile(
                    eplInvalid,
                    "Failed to validate assignment expression 'Helper.swap(VARONE,VARTWO)': Assignment expression must receive a single variable value");

                var eplConstant =
                    "import " +
                    typeof(MyLocalVariable).FullName +
                    ";\n" +
                    "@name('var') create constant variable MyLocalVariable VAR = new MyLocalVariable(1, 10);\n" +
                    "@name('s0') on SupportBean set VAR.reset();\n";
                env.TryInvalidCompile(
                    eplConstant,
                    "Failed to validate assignment expression 'VAR.reset()': Variable by name 'VAR' is declared constant and may not be set");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }

            private void AssertVariable(
                RegressionEnvironment env,
                int aExpected,
                int bExpected)
            {
                var value = (MyLocalVariable)env.Runtime.VariableService.GetVariableValue(
                    env.DeploymentId("var"),
                    "VAR");
                ClassicAssert.AreEqual(aExpected, value.a);
                ClassicAssert.AreEqual(bExpected, value.b);
            }
        }

        private class EPLVariableOnSetArrayBoxed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create variable System.Double[] dbls = new System.Double[3];\n" +
                    "@priority(1) on SupportBean set dbls[IntPrimitive] = 1;\n" +
                    "@name('s0') select dbls as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew(
                    "s0",
                    @event =>
                        CollectionAssert.AreEqual(new double?[] { null, 1d, null }, (double?[])@event.Get("c0")));

                env.UndeployAll();
            }
        }

        private class EPLVariableOnSetArrayInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplVariables = "@name('create') @public create variable double[primitive] doublearray;\n" +
                                   "@public create variable int[primitive] intarray;\n" +
                                   "@public create variable int notAnArray;";
                env.Compile(eplVariables, path);

                // invalid property
                env.TryInvalidCompile(
                    path,
                    "on SupportBean set xxx[IntPrimitive]=1d",
                    "Failed to validate assignment expression 'xxx[IntPrimitive]=1.0': Variable by name 'xxx' has not been created or configured");

                // index expression is not Integer
                env.TryInvalidCompile(
                    path,
                    "on SupportBean set doublearray[null]=1d",
                    "Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'null' returns 'null' for expression 'doublearray'");

                // type incompatible cannot assign
                env.TryInvalidCompile(
                    path,
                    "on SupportBean set intarray[IntPrimitive]='x'",
                    "Failed to validate assignment expression 'intarray[IntPrimitive]=\"x\"': Invalid assignment of column '\"x\"' of type 'System.String' to event property 'intarray' typed as 'int', column and parameter types mismatch");

                // not-an-array
                env.TryInvalidCompile(
                    path,
                    "on SupportBean set notAnArray[IntPrimitive]=1",
                    "Failed to validate assignment expression 'notAnArray[IntPrimitive]=1': Variable 'notAnArray' is not an array");

                path.Clear();

                // runtime-behavior for index-overflow and null-array and null-index and
                var epl = "@name('create') create variable double[primitive] doublearray = new double[3];\n" +
                          "on SupportBean set doublearray[IntBoxed]=DoubleBoxed;\n";
                env.CompileDeploy(epl);

                // index returned is too large
                try {
                    var sb = new SupportBean();
                    sb.IntBoxed = 10;
                    sb.DoubleBoxed = 10d;
                    env.SendEventBean(sb);
                    Assert.Fail();
                }
                catch (Exception ex) {
                    ClassicAssert.IsTrue(ex.Message.Contains("Array length 3 less than index 10 for variable 'doublearray'"));
                }

                // index returned null
                var sbIndexNull = new SupportBean();
                sbIndexNull.DoubleBoxed = 10d;
                env.SendEventBean(sbIndexNull);

                // rhs returned null for array-of-primitive
                var sbRHSNull = new SupportBean();
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
                var path = new RegressionPath();
                var eplCreate =
                    "@name('vars') @public create variable double[primitive] doublearray = new double[3];\n" +
                    "@public create variable String[] stringarray = new String[] {'a', 'b', 'c'};\n";
                env.CompileDeploy(eplCreate, path);

                var epl = "on SupportBean set (doublearray[IntPrimitive])=1, (stringarray[IntPrimitive])=\"x\"";
                env.CompileDeploy(soda, epl, path);

                AssertVariables(env, new double[3], "a,b,c".SplitCsv());

                env.SendEventBean(new SupportBean("E1", 1));

                AssertVariables(env, new double[] { 0, 1, 0 }, "a,x,c".SplitCsv());

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }

            private void AssertVariables(
                RegressionEnvironment env,
                double[] doubleExpected,
                string[] stringExpected)
            {
                var vals = env.Runtime.VariableService.GetVariableValueAll();
                var deploymentId = env.DeploymentId("vars");
                var doubleArray = (double[])vals.Get(new DeploymentIdNamePair(deploymentId, "doublearray"));
                var stringArray = (string[])vals.Get(new DeploymentIdNamePair(deploymentId, "stringarray"));
                CollectionAssert.AreEqual(doubleExpected, doubleArray);
                CollectionAssert.AreEqual(stringExpected, stringArray);
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

        private class EPLVariableOnSetSubqueryMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('var') @public create variable int total_sum = -1;\n" +
                          "on SupportBean set total_sum = (select sum(value) as c0 from SupportEventWithIntArray#keepall group by array)";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportEventWithIntArray("E1", new int[] { 1, 2 }, 10));
                env.SendEventBean(new SupportEventWithIntArray("E2", new int[] { 1, 2 }, 11));

                env.Milestone(0);
                AssertVariable(env, -1);

                env.SendEventBean(new SupportBean());
                AssertVariable(env, 21);

                env.SendEventBean(new SupportEventWithIntArray("E3", new int[] { 1, 2 }, 12));
                env.SendEventBean(new SupportBean());
                AssertVariable(env, 33);

                env.Milestone(1);

                env.SendEventBean(new SupportEventWithIntArray("E4", new int[] { 1 }, 13));
                env.SendEventBean(new SupportBean());
                AssertVariable(env, null);

                env.UndeployAll();
            }

            private void AssertVariable(
                RegressionEnvironment env,
                int? expected)
            {
                ClassicAssert.AreEqual(
                    expected,
                    env.Runtime.VariableService.GetVariableValue(env.DeploymentId("var"), "total_sum"));
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class EPLVariableOnSetSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variable boolean var_simple_set = true;\n" +
                          "@name('set') on SupportBean_S0 set var_simple_set = false;\n" +
                          "@name('s0') select var_simple_set as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertEqualsNew("s0", "c0", true);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertEqualsNew("s0", "c0", false);

                env.UndeployAll();
            }
        }

        private class EPLVariableOnSetSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@name('s0') on SupportBean_S0 as s0str set var1SS = (select P10 from SupportBean_S1#lastevent), var2SS = (select P11||s0str.P01 from SupportBean_S1#lastevent)";
                env.CompileDeploy(stmtTextSet);
                var fieldsVar = new string[] { "var1SS", "var2SS" };
                env.AssertPropsPerRowIterator("s0", fieldsVar, new object[][] { new object[] { "a", "b" } });

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsPerRowIterator("s0", fieldsVar, new object[][] { new object[] { null, null } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(0, "x", "y"));
                env.SendEventBean(new SupportBean_S0(1, "1", "2"));
                env.AssertPropsPerRowIterator("s0", fieldsVar, new object[][] { new object[] { "x", "y2" } });

                env.UndeployAll();
            }
        }

        private class EPLVariableOnSetAssignmentOrderNoDup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@name('set') on SupportBean set var1OND = IntPrimitive, var2OND = var1OND + 1, var3OND = var1OND + var2OND";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                var fieldsVar = new string[] { "var1OND", "var2OND", "var3OND" };
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 12, 2, null } });

                SendSupportBean(env, "S1", 3);
                env.AssertPropsNew("set", fieldsVar, new object[] { 3, 4, 7 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 3, 4, 7 } });

                env.Milestone(0);

                SendSupportBean(env, "S1", -1);
                env.AssertPropsNew("set", fieldsVar, new object[] { -1, 0, -1 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { -1, 0, -1 } });

                SendSupportBean(env, "S1", 90);
                env.AssertPropsNew("set", fieldsVar, new object[] { 90, 91, 181 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 90, 91, 181 } });

                env.UndeployAll();
            }
        }

        private class EPLVariableOnSetAssignmentOrderDup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@name('set') on SupportBean set var1OD = IntPrimitive, var2OD = var2OD, var1OD = IntBoxed, var3OD = var3OD + 1";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                var fieldsVar = new string[] { "var1OD", "var2OD", "var3OD" };
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 0, 1, 2 } });

                SendSupportBean(env, "S1", -1, 10);
                env.AssertPropsNew("set", fieldsVar, new object[] { 10, 1, 3 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 10, 1, 3 } });

                SendSupportBean(env, "S2", -2, 20);
                env.AssertPropsNew("set", fieldsVar, new object[] { 20, 1, 4 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 20, 1, 4 } });

                env.Milestone(0);

                SendSupportBeanNewThread(env, "S3", -3, 30);
                env.AssertPropsNew("set", fieldsVar, new object[] { 30, 1, 5 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 30, 1, 5 } });

                SendSupportBeanNewThread(env, "S4", -4, 40);
                env.AssertPropsNew("set", fieldsVar, new object[] { 40, 1, 6 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 40, 1, 6 } });

                env.UndeployAll();
            }
        }

        private class EPLVariableOnSetObjectModel : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("var1OM", "var2OM", "Id");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_A"));

                var path = new RegressionPath();
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model, path);
                var stmtText = "@name('s0') select var1OM, var2OM, Id from SupportBean_A";
                ClassicAssert.AreEqual(stmtText, model.ToEPL());
                env.AddListener("s0");

                var fieldsSelect = new string[] { "var1OM", "var2OM", "Id" };
                SendSupportBean_A(env, "E1");
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 10d, 11L, "E1" });

                model = new EPStatementObjectModel();
                model.OnExpr = OnClause
                    .CreateOnSet(Expressions.Eq(Expressions.Property("var1OM"), Expressions.Property("IntPrimitive")))
                    .AddAssignment(Expressions.Eq(Expressions.Property("var2OM"), Expressions.Property("IntBoxed")));
                model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("set"));
                var stmtTextSet = "@name('set') on SupportBean set var1OM=IntPrimitive, var2OM=IntBoxed";
                env.CompileDeploy(model, path).AddListener("set");
                ClassicAssert.AreEqual(stmtTextSet, model.ToEPL());

                var fieldsVar = new string[] { "var1OM", "var2OM" };
                env.AssertStatement(
                    "set",
                    statement => {
                        var typeSet = statement.EventType;
                        ClassicAssert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1OM"));
                        ClassicAssert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2OM"));
                        ClassicAssert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                        EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);
                    });

                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 10d, 11L } });
                SendSupportBean(env, "S1", 3, 4);
                env.AssertPropsNew("set", fieldsVar, new object[] { 3d, 4L });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 3d, 4L } });

                SendSupportBean_A(env, "E2");
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 3d, 4L, "E2" });

                env.UndeployModuleContaining("set");
                env.UndeployModuleContaining("s0");
            }
        }

        private class EPLVariableOnSetSimpleSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var textVar = "@name('s0_0') @public create variable int resvar = 1";
                env.CompileDeploy(textVar, path).AddListener("s0_0");
                var fieldsVarOne = new string[] { "resvar" };

                textVar = "@name('s0_1') @public create variable int durvar = 10";
                env.CompileDeploy(textVar, path).AddListener("s0_1");
                var fieldsVarTwo = new string[] { "durvar" };

                var textSet = "@name('s1') on SupportBean set resvar = IntPrimitive, durvar = IntPrimitive";
                env.CompileDeploy(textSet, path).AddListener("s1");
                var fieldsVarSet = new string[] { "resvar", "durvar" };

                var textSelect = "@name('s2') select irstream resvar, durvar, Symbol from SupportMarketDataBean";
                env.CompileDeploy(textSelect, path).AddListener("s2");
                var fieldsSelect = new string[] { "resvar", "durvar", "Symbol" };

                env.Milestone(0);

                // read values
                SendMarketDataEvent(env, "E1");
                env.AssertPropsNew("s2", fieldsSelect, new object[] { 1, 10, "E1" });

                env.Milestone(1);

                // set new value
                SendSupportBean(env, 20);
                env.AssertListener(
                    "s0_0",
                    listener => EPAssertionUtil.AssertProps(
                        listener.LastNewData[0],
                        fieldsVarOne,
                        new object[] { 20 }));
                env.AssertListener(
                    "s0_1",
                    listener => EPAssertionUtil.AssertProps(
                        listener.LastNewData[0],
                        fieldsVarTwo,
                        new object[] { 20 }));
                env.AssertPropsNew("s1", fieldsVarSet, new object[] { 20, 20 });

                env.Milestone(2);

                // read values
                SendMarketDataEvent(env, "E2");
                env.AssertPropsNew("s2", fieldsSelect, new object[] { 20, 20, "E2" });

                env.Milestone(3);

                // set new value
                SendSupportBean(env, 1000);

                env.Milestone(4);

                // read values
                SendMarketDataEvent(env, "E3");
                env.AssertPropsNew("s2", fieldsSelect, new object[] { 1000, 1000, "E3" });

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
                var bean = new SupportMarketDataBean(symbol, 0, 0L, null);
                env.SendEventBean(bean);
            }

            private static void SendSupportBean(
                RegressionEnvironment env,
                int intPrimitive)
            {
                var bean = new SupportBean("", intPrimitive);
                env.SendEventBean(bean);
            }
        }

        private class EPLVariableOnSetCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select var1C, var2C, Id from SupportBean_A";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");

                var fieldsSelect = new string[] { "var1C", "var2C", "Id" };
                SendSupportBean_A(env, "E1");
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 10d, 11L, "E1" });

                var stmtTextSet = "@name('set') on SupportBean set var1C=IntPrimitive, var2C=IntBoxed";
                env.EplToModelCompileDeploy(stmtTextSet).AddListener("set");

                var fieldsVar = new string[] { "var1C", "var2C" };
                env.AssertStatement(
                    "set",
                    statement => {
                        var typeSet = statement.EventType;
                        ClassicAssert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1C"));
                        ClassicAssert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2C"));
                        ClassicAssert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                        EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);
                    });

                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 10d, 11L } });
                SendSupportBean(env, "S1", 3, 4);
                env.AssertPropsNew("set", fieldsVar, new object[] { 3d, 4L });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 3d, 4L } });

                SendSupportBean_A(env, "E2");
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 3d, 4L, "E2" });

                env.UndeployModuleContaining("set");
                env.UndeployModuleContaining("s0");
            }
        }

        private class EPLVariableOnSetWDeploy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select var1RTC, TheString from SupportBean(TheString like 'E%')";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fieldsSelect = new string[] { "var1RTC", "TheString" };
                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 10, "E1" });

                env.Milestone(0);

                SendSupportBean(env, "E2", 2);
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 10, "E2" });

                var stmtTextSet = "@name('set') on SupportBean(TheString like 'S%') set var1RTC = IntPrimitive";
                env.CompileDeploy(stmtTextSet).AddListener("set");

                env.AssertStatement(
                    "set",
                    statement => {
                        var typeSet = statement.EventType;
                        ClassicAssert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1RTC"));
                        ClassicAssert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                        ClassicAssert.IsTrue(Arrays.AreEqual(typeSet.PropertyNames, new string[] { "var1RTC" }));
                    });

                var fieldsVar = new string[] { "var1RTC" };
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 10 } });

                SendSupportBean(env, "S1", 3);
                env.AssertPropsNew("set", fieldsVar, new object[] { 3 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 3 } });

                env.Milestone(1);

                SendSupportBean(env, "E3", 4);
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 3, "E3" });

                SendSupportBean(env, "S2", -1);
                env.AssertPropsNew("set", fieldsVar, new object[] { -1 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { -1 } });

                SendSupportBean(env, "E4", 5);
                env.AssertPropsNew("s0", fieldsSelect, new object[] { -1, "E4" });

                env.UndeployAll();
            }
        }

        private class EPLVariableOnSetRuntimeOrderMultiple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@name('set') on SupportBean(TheString like 'S%' or TheString like 'B%') set var1ROM = IntPrimitive, var2ROM = IntBoxed";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                var fieldsVar = new string[] { "var1ROM", "var2ROM" };
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { null, 1 } });

                env.AssertStatement(
                    "set",
                    statement => {
                        var typeSet = statement.EventType;
                        ClassicAssert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1ROM"));
                        ClassicAssert.AreEqual(typeof(int?), typeSet.GetPropertyType("var2ROM"));
                        ClassicAssert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                        EPAssertionUtil.AssertEqualsAnyOrder(
                            new string[] { "var1ROM", "var2ROM" },
                            typeSet.PropertyNames);
                    });

                SendSupportBean(env, "S1", 3, null);
                env.AssertPropsNew("set", fieldsVar, new object[] { 3, null });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 3, null } });

                env.Milestone(0);

                SendSupportBean(env, "S1", -1, -2);
                env.AssertPropsNew("set", fieldsVar, new object[] { -1, -2 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { -1, -2 } });

                var stmtText =
                    "@name('s0') select var1ROM, var2ROM, TheString from SupportBean(TheString like 'E%' or TheString like 'B%')";
                env.CompileDeploy(stmtText).AddListener("s0");
                var fieldsSelect = new string[] { "var1ROM", "var2ROM", "TheString" };
                env.AssertPropsPerRowIterator("s0", fieldsSelect, null);

                env.Milestone(1);

                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("s0", fieldsSelect, new object[] { -1, -2, "E1" });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { -1, -2 } });
                env.AssertPropsPerRowIterator("s0", fieldsSelect, new object[][] { new object[] { -1, -2, "E1" } });

                SendSupportBean(env, "S1", 11, 12);
                env.AssertPropsNew("set", fieldsVar, new object[] { 11, 12 });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 11, 12 } });
                env.AssertPropsPerRowIterator("s0", fieldsSelect, new object[][] { new object[] { 11, 12, "E1" } });

                SendSupportBean(env, "E2", 2);
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 11, 12, "E2" });
                env.AssertPropsPerRowIterator("s0", fieldsSelect, new object[][] { new object[] { 11, 12, "E2" } });

                env.UndeployAll();
            }
        }

        private class EPLVariableOnSetWithFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@name('set') on SupportBean(TheString like 'S%') set papi_1 = 'end', papi_2 = false, papi_3 = null";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                var fieldsVar = new string[] { "papi_1", "papi_2", "papi_3" };
                env.AssertPropsPerRowIterator(
                    "set",
                    fieldsVar,
                    new object[][] { new object[] { "begin", true, "value" } });

                env.AssertStatement(
                    "set",
                    statement => {
                        var typeSet = statement.EventType;
                        ClassicAssert.AreEqual(typeof(string), typeSet.GetPropertyType("papi_1"));
                        ClassicAssert.AreEqual(typeof(bool?), typeSet.GetPropertyType("papi_2"));
                        ClassicAssert.AreEqual(typeof(string), typeSet.GetPropertyType("papi_3"));
                        ClassicAssert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);

                        CollectionAssert.AreEqual(typeSet.PropertyNames.OrderBy(_ => _), fieldsVar);
                    });

                SendSupportBean(env, "S1", 3);
                env.AssertPropsNew("set", fieldsVar, new object[] { "end", false, null });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { "end", false, null } });

                env.Milestone(0);

                SendSupportBean(env, "S2", 4);
                env.AssertPropsNew("set", fieldsVar, new object[] { "end", false, null });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { "end", false, null } });

                env.UndeployAll();
            }
        }

        private class EPLVariableOnSetCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextSet =
                    "@name('set') on SupportBean set var1COE = IntPrimitive, var2COE = IntPrimitive, var3COE=IntBoxed";
                env.CompileDeploy(stmtTextSet).AddListener("set");
                var fieldsVar = new string[] { "var1COE", "var2COE", "var3COE" };
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { null, null, null } });

                env.AssertStatement(
                    "set",
                    statement => {
                        var typeSet = statement.EventType;
                        ClassicAssert.AreEqual(typeof(float?), typeSet.GetPropertyType("var1COE"));
                        ClassicAssert.AreEqual(typeof(double?), typeSet.GetPropertyType("var2COE"));
                        ClassicAssert.AreEqual(typeof(long?), typeSet.GetPropertyType("var3COE"));
                        ClassicAssert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
                        EPAssertionUtil.AssertEqualsAnyOrder(typeSet.PropertyNames, fieldsVar);
                    });

                var stmtText = "@name('s0') select irstream var1COE, var2COE, var3COE, Id from SupportBean_A#length(2)";
                env.CompileDeploy(stmtText).AddListener("s0");
                var fieldsSelect = new string[] { "var1COE", "var2COE", "var3COE", "Id" };
                env.AssertPropsPerRowIterator("s0", fieldsSelect, null);

                SendSupportBean_A(env, "A1");
                env.AssertPropsNew("s0", fieldsSelect, new object[] { null, null, null, "A1" });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fieldsSelect,
                    new object[][] { new object[] { null, null, null, "A1" } });

                SendSupportBean(env, "S1", 1, 2);
                env.AssertPropsNew("set", fieldsVar, new object[] { 1f, 1d, 2L });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 1f, 1d, 2L } });

                env.Milestone(0);

                SendSupportBean_A(env, "A2");
                env.AssertPropsNew("s0", fieldsSelect, new object[] { 1f, 1d, 2L, "A2" });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fieldsSelect,
                    new object[][] { new object[] { 1f, 1d, 2L, "A1" }, new object[] { 1f, 1d, 2L, "A2" } });

                SendSupportBean(env, "S1", 10, 20);
                env.AssertPropsNew("set", fieldsVar, new object[] { 10f, 10d, 20L });
                env.AssertPropsPerRowIterator("set", fieldsVar, new object[][] { new object[] { 10f, 10d, 20L } });

                SendSupportBean_A(env, "A3");
                env.AssertListener(
                    "s0",
                    listener => {
                        EPAssertionUtil.AssertProps(
                            listener.LastNewData[0],
                            fieldsSelect,
                            new object[] { 10f, 10d, 20L, "A3" });
                        EPAssertionUtil.AssertProps(
                            listener.LastOldData[0],
                            fieldsSelect,
                            new object[] { 10f, 10d, 20L, "A1" });
                    });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fieldsSelect,
                    new object[][] { new object[] { 10f, 10d, 20L, "A2" }, new object[] { 10f, 10d, 20L, "A3" } });

                env.UndeployAll();
            }
        }

        private class EPLVariableOnSetInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "on SupportBean set dummy = 100",
                    "Failed to validate assignment expression 'dummy=100': Variable by name 'dummy' has not been created or configured");

                env.TryInvalidCompile(
                    "on SupportBean set var1IS = 1",
                    "Failed to validate assignment expression 'var1IS=1': Variable 'var1IS' of declared type String cannot be assigned a value of type int");

                env.TryInvalidCompile(
                    "on SupportBean set var3IS = 'abc'",
                    "Failed to validate assignment expression 'var3IS=\"abc\"': Variable 'var3IS' of declared type Integer cannot be assigned a value of type String");

                env.TryInvalidCompile(
                    "on SupportBean set var3IS = DoublePrimitive",
                    "Failed to validate assignment expression 'var3IS=DoublePrimitive': Variable 'var3IS' of declared type Integer cannot be assigned a value of type Double");

                env.TryInvalidCompile("on SupportBean set var2IS = 'false'", "skip");
                env.TryInvalidCompile("on SupportBean set var3IS = 1.1", "skip");
                env.TryInvalidCompile("on SupportBean set var3IS = 22222222222222", "skip");
                env.TryInvalidCompile(
                    "on SupportBean set var3IS",
                    "Failed to validate assignment expression 'var3IS': Missing variable assignment expression in assignment number 0");
            }
        }

        private static SupportBean_A SendSupportBean_A(
            RegressionEnvironment env,
            string id)
        {
            var bean = new SupportBean_A(id);
            env.SendEventBean(bean);
            return bean;
        }

        private static SupportBean SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
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
            var bean = MakeSupportBean(theString, intPrimitive, intBoxed);
            env.SendEventBean(bean);
            return bean;
        }

        private static void SendSupportBeanNewThread(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int? intBoxed)
        {
            try {
                var t = new Thread(
                    () => {
                        var bean = MakeSupportBean(theString, intPrimitive, intBoxed);
                        env.SendEventBean(bean);
                    });
                t.Start();
                t.Join();
            }
            catch (ThreadInterruptedException ex) {
                Assert.Fail(ex.Message);
            }
        }

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            int? intBoxed)
        {
            var bean = new SupportBean();
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