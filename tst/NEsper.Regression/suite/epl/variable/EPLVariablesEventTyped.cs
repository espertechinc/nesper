///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariablesEventTyped
    {
        public static readonly NonSerializable NON_SERIALIZABLE = new EPLVariablesEventTyped.NonSerializable("abc");

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEventTypedSceneOne(execs);
            WithEventTypedSceneTwo(execs);
            WithConfig(execs);
            WithEventTypedSetProp(execs);
            WithInvalid(execs);
            WithEventTypedCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithEventTypedCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableEventTypedCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithEventTypedSetProp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableEventTypedSetProp());
            return execs;
        }

        public static IList<RegressionExecution> WithConfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableConfig());
            return execs;
        }

        public static IList<RegressionExecution> WithEventTypedSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableEventTypedSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithEventTypedSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableEventTypedSceneOne());
            return execs;
        }

        private class EPLVariableEventTypedCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@buseventtype @public @name('schema') create schema OrderEvent(OrderId string);",
                    path);
                var deployIdSchema = env.DeploymentId("schema");

                env.CompileDeploy("@public @name('variable') create variable OrderEvent orderEvent;", path);
                var deployIdVariable = env.DeploymentId("variable");

                var consumed = env.Deployment.GetDeploymentDependenciesConsumed(deployIdVariable);
                CollectionAssert.AreEquivalent(
                    new EPDeploymentDependencyConsumed.Item[] {
                        new EPDeploymentDependencyConsumed.Item(deployIdSchema, EPObjectType.EVENTTYPE, "OrderEvent"),
                    },
                    consumed.Dependencies.ToArray());

                var provided = env.Deployment.GetDeploymentDependenciesProvided(deployIdSchema);
                CollectionAssert.AreEquivalent(
                    new EPDeploymentDependencyProvided.Item[] {
                        new EPDeploymentDependencyProvided.Item(
                            EPObjectType.EVENTTYPE,
                            "OrderEvent",
                            Collections.SingletonSet(deployIdVariable)),
                    },
                    provided.Dependencies.ToArray());

                env.CompileDeploy(
                    "on OrderEvent as oe set orderEvent = oe;\n" +
                    "@name('s0') select orderEvent.OrderId as c0 from SupportBean;\n",
                    path);
                env.AddListener("s0");

                env.Milestone(0);

                SendOrderEvent(env, "O1");

                env.Milestone(1);

                AssertSelect(env, "O1");

                SendOrderEvent(env, "O2");

                env.Milestone(2);

                AssertSelect(env, "O2");

                env.UndeployAll();
            }

            private void AssertSelect(
                RegressionEnvironment env,
                string orderId)
            {
                env.SendEventBean(new SupportBean());
                env.AssertEqualsNew("s0", "c0", orderId);
            }

            private void SendOrderEvent(
                RegressionEnvironment env,
                string orderId)
            {
                env.SendEventMap(Collections.SingletonDataMap("OrderId", orderId), "OrderEvent");
            }
        }

        private class EPLVariableInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                try {
                    env.RuntimeSetVariable(null, "vars0_A", new SupportBean_S1(1));
                    Assert.Fail();
                }
                catch (VariableValueException ex) {
                    Assert.AreEqual(
                        "Variable 'vars0_A' of declared event type 'SupportBean_S0' underlying type '" +
                        typeof(SupportBean_S0).FullName +
                        "' cannot be assigned a value of type '" +
                        typeof(SupportBean_S1).FullName +
                        "'",
                        ex.Message);
                }

                env.TryInvalidCompile(
                    "on SupportBean_S0 arrival set vars1_A = arrival",
                    "Failed to validate assignment expression 'vars1_A=arrival': Variable 'vars1_A' of declared event type '" +
                    typeof(SupportBean_S1).FullName +
                    "' underlying type '" +
                    typeof(SupportBean_S1).FullName +
                    "' cannot be assigned a value of type '" +
                    typeof(SupportBean_S0).FullName +
                    "'");

                env.TryInvalidCompile(
                    "on SupportBean_S0 arrival set vars0_A = 1",
                    "Failed to validate assignment expression 'vars0_A=1': Variable 'vars0_A' of declared event type 'SupportBean_S0' underlying type '" +
                    typeof(SupportBean_S0).FullName +
                    "' cannot be assigned a value of type 'int'");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLVariableEventTypedSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var vars = "@name('vars') @public create variable " +
                           typeof(SupportBean).FullName +
                           " varbeannull;\n" +
                           "@public create variable " +
                           typeof(SupportBean).FullName +
                           " varbean;\n" +
                           "@public create variable SupportBean_S0 vars0;\n" +
                           "@public create variable long varobj;\n" +
                           "@public create variable long varobjnull;\n";
                env.CompileDeploy(vars, path);

                var fields = "c0,c1,c2,c3,c4,c5,c6".SplitCsv();
                var stmtSelectText =
                    "@name('Select') select varbean.TheString as c0,varbean.IntPrimitive as c1,vars0.Id as c2,vars0.P00 as c3,varobj as c4,varbeannull.TheString as c5, varobjnull as c6 from SupportBean_A";
                env.CompileDeploy(stmtSelectText, path).AddListener("Select");

                env.SendEventBean(new SupportBean_A("A1"));
                env.AssertPropsNew("Select", fields, new object[] { null, null, null, null, null, null, null });

                // update via API
                env.RuntimeSetVariable("vars", "varobj", 101L);
                env.RuntimeSetVariable("vars", "vars0", new SupportBean_S0(1, "S01"));
                env.RuntimeSetVariable("vars", "varbean", new SupportBean("E1", -1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("A2"));
                env.AssertPropsNew("Select", fields, new object[] { "E1", -1, 1, "S01", 101L, null, null });

                env.Milestone(1);

                // update properties via on-set
                var stmtUpdateText =
                    "@name('Update') on SupportBean_B set varbean.TheString = 'EX', varbean.IntPrimitive = -999";
                env.CompileDeploy(stmtUpdateText, path);
                env.AssertStatement(
                    "Update",
                    statement => Assert.AreEqual(
                        StatementType.ON_SET,
                        statement.GetProperty(StatementProperty.STATEMENTTYPE)));
                env.SendEventBean(new SupportBean_B("B1"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_A("A3"));
                env.AssertPropsNew("Select", fields, new object[] { "EX", -999, 1, "S01", 101L, null, null });

                // update full bean via on-set
                stmtUpdateText = "@name('Update2') on SupportBean(IntPrimitive = 0) as sb set varbean = sb";
                env.CompileDeploy(stmtUpdateText, path);

                var bean = new SupportBean("E2", 0);
                env.SendEventBean(bean);

                env.Milestone(3);

                env.SendEventBean(new SupportBean_A("A4"));
                env.AssertPropsNew("Select", fields, new object[] { "E2", 0, 1, "S01", 101L, null, null });

                env.UndeployAll();
            }
        }

        private class EPLVariableConfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Assert.AreEqual(10, ((SupportBean_S0)env.Runtime.VariableService.GetVariableValue(null, "vars0_A")).Id);
                Assert.AreEqual(20, ((SupportBean_S1)env.Runtime.VariableService.GetVariableValue(null, "vars1_A")).Id);
                Assert.AreEqual(123, env.Runtime.VariableService.GetVariableValue(null, "varsobj1"));
                var value = env.Runtime.VariableService.GetVariableValue(null, "myNonSerializable");
                if (!env.IsHA) {
                    Assert.AreSame(NON_SERIALIZABLE, value);
                }
                else {
                    Assert.AreEqual(NON_SERIALIZABLE, value);
                }

                env.Milestone(0);

                Assert.AreEqual(30, ((SupportBean_S2)env.Runtime.VariableService.GetVariableValue(null, "vars2")).Id);
                Assert.AreEqual(40, ((SupportBean_S3)env.Runtime.VariableService.GetVariableValue(null, "vars3")).Id);
                Assert.AreEqual("ABC", env.Runtime.VariableService.GetVariableValue(null, "varsobj2"));

                env.CompileDeploy("@name('create') create variable object varsobj3=222");
                Assert.AreEqual(
                    222,
                    env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varsobj3"));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class EPLVariableEventTypedSetProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('create') @public create variable SupportBean varbean", path);

                var fields = "varbean.TheString,varbean.IntPrimitive,varbean.TheString".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') select varbean.TheString,varbean.IntPrimitive,varbean.TheString from SupportBean_S0",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null });

                env.CompileDeploy(
                    "@name('set') on SupportBean_A set varbean.TheString = 'A', varbean.IntPrimitive = 1",
                    path);
                env.AddListener("set");
                env.SendEventBean(new SupportBean_A("E1"));
                env.ListenerReset("set");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null });

                var setBean = new SupportBean();
                env.RuntimeSetVariable("create", "varbean", setBean);
                env.SendEventBean(new SupportBean_A("E2"));
                env.AssertPropsNew("set", "varbean.TheString,varbean.IntPrimitive".SplitCsv(), new object[] { "A", 1 });
                env.AssertIterator(
                    "s0",
                    iterator => EPAssertionUtil.AssertProps(
                        iterator.Advance(),
                        "varbean.TheString,varbean.IntPrimitive".SplitCsv(),
                        new object[] { "A", 1 }));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(3));
                env.AssertPropsNew("s0", fields, new object[] { "A", 1, "A" });
                Assert.AreNotSame(
                    setBean,
                    env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varbean"));
                Assert.AreEqual(
                    1,
                    ((SupportBean)env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varbean"))
                    .IntPrimitive);

                // test self evaluate
                env.UndeployModuleContaining("set");
                env.CompileDeploy(
                    "@name('set') on SupportBean_A set varbean.TheString = SupportBean_A.Id, varbean.TheString = '>'||varbean.TheString||'<'",
                    path);
                env.AddListener("set");
                env.SendEventBean(new SupportBean_A("E3"));
                Assert.AreEqual(
                    ">E3<",
                    ((SupportBean)env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varbean"))
                    .TheString);
                env.UndeployModuleContaining("set");

                // test widen
                env.CompileDeploy("@name('set') on SupportBean_A set varbean.LongPrimitive = 1", path);
                env.AddListener("set");
                env.SendEventBean(new SupportBean_A("E4"));
                Assert.AreEqual(
                    1,
                    ((SupportBean)env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varbean"))
                    .LongPrimitive);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class EPLVariableEventTypedSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('v0') @public create variable Object varobject = null", path);
                env.CompileDeploy(
                    "@name('v1') @public create variable " + typeof(SupportBean_A).FullName + " varbean = null",
                    path);
                env.CompileDeploy("@name('v2') @public create variable SupportBean_S0 vartype = null", path);
                var depIdVarobject = env.DeploymentId("v0");
                var depIdVarbean = env.DeploymentId("v1");
                var depIdVartype = env.DeploymentId("v2");

                var fields = "varobject,varbean,varbean.Id,vartype,vartype.Id".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') select varobject, varbean, varbean.Id, vartype, vartype.Id from SupportBean",
                    path);
                env.AddListener("s0");

                // test null
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });

                env.Milestone(0);

                // test objects
                var a1objectOne = new SupportBean_A("A1");
                var s0objectOne = new SupportBean_S0(1);
                env.Runtime.VariableService.SetVariableValue(depIdVarobject, "varobject", "abc");
                env.Runtime.VariableService.SetVariableValue(depIdVarbean, "varbean", a1objectOne);
                env.Runtime.VariableService.SetVariableValue(depIdVartype, "vartype", s0objectOne);

                env.Milestone(1);

                env.SendEventBean(new SupportBean());
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { "abc", a1objectOne, a1objectOne.Id, s0objectOne, s0objectOne.Id });

                // test on-set for Object and EventType
                var fieldsTop = "varobject,vartype,varbean".SplitCsv();
                env.CompileDeploy(
                    "@name('set') on SupportBean_S0(P00='X') arrival set varobject=1, vartype=arrival, varbean=null",
                    path);
                env.AddListener("set");

                var s0objectTwo = new SupportBean_S0(2, "X");
                env.SendEventBean(s0objectTwo);
                Assert.AreEqual(1, env.Runtime.VariableService.GetVariableValue(depIdVarobject, "varobject"));
                Assert.AreEqual(s0objectTwo, env.Runtime.VariableService.GetVariableValue(depIdVartype, "vartype"));
                Assert.AreEqual(
                    s0objectTwo,
                    env.Runtime.VariableService
                        .GetVariableValue(Collections.SingletonSet(new DeploymentIdNamePair(depIdVartype, "vartype")))
                        .Get(new DeploymentIdNamePair(depIdVartype, "vartype")));
                env.AssertPropsNew("set", fieldsTop, new object[] { 1, s0objectTwo, null });
                env.AssertIterator(
                    "set",
                    iterator => EPAssertionUtil.AssertProps(
                        iterator.Advance(),
                        fieldsTop,
                        new object[] { 1, s0objectTwo, null }));

                // set via API to null
                IDictionary<DeploymentIdNamePair, object> newValues = new Dictionary<DeploymentIdNamePair, object>();
                newValues.Put(new DeploymentIdNamePair(depIdVarobject, "varobject"), null);
                newValues.Put(new DeploymentIdNamePair(depIdVartype, "vartype"), null);
                newValues.Put(new DeploymentIdNamePair(depIdVarbean, "varbean"), null);
                env.Runtime.VariableService.SetVariableValue(newValues);
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });

                // set via API to values
                newValues.Put(new DeploymentIdNamePair(depIdVarobject, "varobject"), 10L);
                newValues.Put(new DeploymentIdNamePair(depIdVartype, "vartype"), s0objectTwo);
                newValues.Put(new DeploymentIdNamePair(depIdVarbean, "varbean"), a1objectOne);
                env.Runtime.VariableService.SetVariableValue(newValues);
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 10L, a1objectOne, a1objectOne.Id, s0objectTwo, s0objectTwo.Id });

                // test on-set for Bean class
                env.CompileDeploy(
                    "@name('set-two') on SupportBean_A(Id='Y') arrival set varobject=null, vartype=null, varbean=arrival",
                    path);
                env.AddListener("set-two");
                var a1objectTwo = new SupportBean_A("Y");
                env.SendEventBean(new SupportBean_A("Y"));
                Assert.AreEqual(null, env.Runtime.VariableService.GetVariableValue(depIdVarobject, "varobject"));
                Assert.AreEqual(null, env.Runtime.VariableService.GetVariableValue(depIdVartype, "vartype"));
                Assert.AreEqual(
                    a1objectTwo,
                    env.Runtime.VariableService
                        .GetVariableValue(Collections.SingletonSet(new DeploymentIdNamePair(depIdVarbean, "varbean")))
                        .Get(new DeploymentIdNamePair(depIdVarbean, "varbean")));
                env.AssertPropsNew("set-two", fieldsTop, new object[] { null, null, a1objectTwo });
                env.AssertIterator(
                    "set-two",
                    iterator => EPAssertionUtil.AssertProps(
                        iterator.Advance(),
                        fieldsTop,
                        new object[] { null, null, a1objectTwo }));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        public class NonSerializable
        {
            private readonly string myString;

            public NonSerializable(string myString)
            {
                this.myString = myString;
            }

            public string GetMyString()
            {
                return myString;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                var that = (NonSerializable)o;

                return myString.Equals(that.myString);
            }

            public override int GetHashCode()
            {
                return myString.GetHashCode();
            }
        }
    }
} // end of namespace