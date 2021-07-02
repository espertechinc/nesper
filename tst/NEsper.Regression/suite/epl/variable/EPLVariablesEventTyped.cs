///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariablesEventTyped
    {
        public static readonly NonSerializable NON_SERIALIZABLE = new NonSerializable("abc");

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEventTypedSceneOne(execs);
            WithEventTypedSceneTwo(execs);
            WithConfig(execs);
            WithEventTypedSetProp(execs);
            WithInvalid(execs);
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

        internal class EPLVariableInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                try {
                    env.Runtime.VariableService.SetVariableValue(null, "vars0_A", new SupportBean_S1(1));
                    Assert.Fail();
                }
                catch (VariableValueException ex) {
                    Assert.AreEqual(
                        "Variable 'vars0_A' of declared event type 'SupportBean_S0' underlying type '" +
                        typeof(SupportBean_S0).FullName +
                        "' cannot be assigned a value of type '" +
                        nameof(SupportBean_S1) +
                        "'",
                        ex.Message);
                }

                TryInvalidCompile(
                    env,
                    "on SupportBean_S0 arrival set vars1_A = arrival",
                    "Failed to validate assignment expression 'vars1_A=arrival': Variable 'vars1_A' of declared event type '" +
                    typeof(SupportBean_S1).FullName +
                    "' underlying type '" +
                    nameof(SupportBean_S1) +
                    "' cannot be assigned a value of type '" +
                    nameof(SupportBean_S0) +
                    "'");

                TryInvalidCompile(
                    env,
                    "on SupportBean_S0 arrival set vars0_A = 1",
                    "Failed to validate assignment expression 'vars0_A=1': Variable 'vars0_A' of declared event type 'SupportBean_S0' underlying type '" +
                    nameof(SupportBean_S0) +
                    "' cannot be assigned a value of type 'Int32'");
            }
        }

        internal class EPLVariableEventTypedSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var vars = "@Name('vars') create variable " +
                           nameof(SupportBean) +
                           " varbeannull;\n" +
                           "create variable " +
                           nameof(SupportBean) +
                           " varbean;\n" +
                           "create variable SupportBean_S0 vars0;\n" +
                           "create variable long varobj;\n" +
                           "create variable long varobjnull;\n";
                env.CompileDeploy(vars, path);
                var deploymentId = env.DeploymentId("vars");

                var fields = new[] {"c0", "c1", "c2", "c3", "c4", "c5", "c6"};
                var stmtSelectText =
                    "@Name('select') select varbean.TheString as c0,varbean.IntPrimitive as c1,vars0.Id as c2,vars0.P00 as c3,varobj as c4,varbeannull.TheString as c5, varobjnull as c6 from SupportBean_A";
                env.CompileDeploy(stmtSelectText, path).AddListener("select");

                env.SendEventBean(new SupportBean_A("A1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null, null, null});

                // update via API
                env.Runtime.VariableService.SetVariableValue(deploymentId, "varobj", 101L);
                env.Runtime.VariableService.SetVariableValue(deploymentId, "vars0", new SupportBean_S0(1, "S01"));
                env.Runtime.VariableService.SetVariableValue(deploymentId, "varbean", new SupportBean("E1", -1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("A2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", -1, 1, "S01", 101L, null, null});

                env.Milestone(1);

                // update properties via on-set
                var stmtUpdateText =
                    "@Name('update') on SupportBean_B set varbean.TheString = 'EX', varbean.IntPrimitive = -999";
                env.CompileDeploy(stmtUpdateText, path);
                Assert.AreEqual(
                    StatementType.ON_SET,
                    env.Statement("update").GetProperty(StatementProperty.STATEMENTTYPE));
                env.SendEventBean(new SupportBean_B("B1"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_A("A3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"EX", -999, 1, "S01", 101L, null, null});

                // update full bean via on-set
                stmtUpdateText = "@Name('update2') on SupportBean(IntPrimitive = 0) as sb set varbean = sb";
                env.CompileDeploy(stmtUpdateText, path);

                var bean = new SupportBean("E2", 0);
                env.SendEventBean(bean);

                env.Milestone(3);

                env.SendEventBean(new SupportBean_A("A4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 0, 1, "S01", 101L, null, null});

                env.UndeployAll();
            }
        }

        internal class EPLVariableConfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Assert.AreEqual(
                    10,
                    ((SupportBean_S0) env.Runtime.VariableService.GetVariableValue(null, "vars0_A")).Id);
                Assert.AreEqual(
                    20,
                    ((SupportBean_S1) env.Runtime.VariableService.GetVariableValue(null, "vars1_A")).Id);
                Assert.AreEqual(123, env.Runtime.VariableService.GetVariableValue(null, "varsobj1"));
                var value = env.Runtime.VariableService.GetVariableValue(null, "myNonSerializable");
                if (!env.IsHA) {
                    Assert.AreSame(NON_SERIALIZABLE, value);
                }
                else {
                    Assert.AreEqual(NON_SERIALIZABLE, value);
                }

                env.Milestone(0);

                Assert.AreEqual(30, ((SupportBean_S2) env.Runtime.VariableService.GetVariableValue(null, "vars2")).Id);
                Assert.AreEqual(40, ((SupportBean_S3) env.Runtime.VariableService.GetVariableValue(null, "vars3")).Id);
                Assert.AreEqual("ABC", env.Runtime.VariableService.GetVariableValue(null, "varsobj2"));

                env.CompileDeploy("@Name('create') create variable object varsobj3=222");
                Assert.AreEqual(
                    222,
                    env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varsobj3"));

                env.UndeployAll();
            }
        }

        internal class EPLVariableEventTypedSetProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('create') create variable SupportBean varbean", path);

                var fields = new[] {"varbean.TheString", "varbean.IntPrimitive", "varbean.GetTheString()"};
                env.CompileDeploy(
                    "@Name('s0') select varbean.TheString,varbean.IntPrimitive,varbean.GetTheString() from SupportBean_S0",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null});

                env.CompileDeploy(
                    "@Name('set') on SupportBean_A set varbean.TheString = 'A', varbean.IntPrimitive = 1",
                    path);
                env.AddListener("set");
                env.SendEventBean(new SupportBean_A("E1"));
                env.Listener("set").Reset();

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null});

                var setBean = new SupportBean();
                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("create"), "varbean", setBean);
                env.SendEventBean(new SupportBean_A("E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    new[] {"varbean.TheString", "varbean.IntPrimitive"},
                    new object[] {"A", 1});
                EPAssertionUtil.AssertProps(
                    env.GetEnumerator("set").Advance(),
                    new[] {"varbean.TheString", "varbean.IntPrimitive"},
                    new object[] {"A", 1});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 1, "A"});
                Assert.AreNotSame(
                    setBean,
                    env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varbean"));
                Assert.AreEqual(
                    1,
                    ((SupportBean) env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varbean"))
                    .IntPrimitive);

                // test self evaluate
                env.UndeployModuleContaining("set");
                env.CompileDeploy(
                    "@Name('set') on SupportBean_A set varbean.TheString = SupportBean_A.Id, varbean.TheString = '>'||varbean.TheString||'<'",
                    path);
                env.AddListener("set");
                env.SendEventBean(new SupportBean_A("E3"));
                Assert.AreEqual(
                    ">E3<",
                    ((SupportBean) env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varbean"))
                    .TheString);
                env.UndeployModuleContaining("set");

                // test widen
                env.CompileDeploy("@Name('set') on SupportBean_A set varbean.LongPrimitive = 1", path);
                env.AddListener("set");
                env.SendEventBean(new SupportBean_A("E4"));
                Assert.AreEqual(
                    1,
                    ((SupportBean) env.Runtime.VariableService.GetVariableValue(env.DeploymentId("create"), "varbean"))
                    .LongPrimitive);

                env.UndeployAll();
            }
        }

        internal class EPLVariableEventTypedSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('v0') create variable Object varobject = null", path);
                env.CompileDeploy(
                    "@Name('v1') create variable " + typeof(SupportBean_A).FullName + " varbean = null",
                    path);
                env.CompileDeploy("@Name('v2') create variable SupportBean_S0 vartype = null", path);
                var depIdVarobject = env.DeploymentId("v0");
                var depIdVarbean = env.DeploymentId("v1");
                var depIdVartype = env.DeploymentId("v2");

                var fields = new[] {"varobject", "varbean", "varbean.Id", "vartype", "vartype.Id"};
                env.CompileDeploy(
                    "@Name('s0') select varobject, varbean, varbean.Id, vartype, vartype.Id from SupportBean",
                    path);
                env.AddListener("s0");

                // test null
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});

                env.Milestone(0);

                // test objects
                var a1objectOne = new SupportBean_A("A1");
                var s0objectOne = new SupportBean_S0(1);
                env.Runtime.VariableService.SetVariableValue(depIdVarobject, "varobject", "abc");
                env.Runtime.VariableService.SetVariableValue(depIdVarbean, "varbean", a1objectOne);
                env.Runtime.VariableService.SetVariableValue(depIdVartype, "vartype", s0objectOne);

                env.Milestone(1);

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"abc", a1objectOne, a1objectOne.Id, s0objectOne, s0objectOne.Id});

                // test on-set for Object and EventType
                var fieldsTop = new[] {"varobject", "vartype", "varbean"};
                env.CompileDeploy(
                    "@Name('set') on SupportBean_S0(P00='X') arrival set varobject=1, vartype=arrival, varbean=null",
                    path);
                env.AddListener("set");

                var s0objectTwo = new SupportBean_S0(2, "X");
                env.SendEventBean(s0objectTwo);
                Assert.AreEqual(1, env.Runtime.VariableService.GetVariableValue(depIdVarobject, "varobject"));
                Assert.AreEqual(s0objectTwo, env.Runtime.VariableService.GetVariableValue(depIdVartype, "vartype"));
                Assert.AreEqual(
                    s0objectTwo,
                    env.Runtime
                        .VariableService.GetVariableValue(
                            Collections.SingletonSet(new DeploymentIdNamePair(depIdVartype, "vartype")))
                        .Get(new DeploymentIdNamePair(depIdVartype, "vartype")));
                EPAssertionUtil.AssertProps(
                    env.Listener("set").AssertOneGetNewAndReset(),
                    fieldsTop,
                    new object[] {1, s0objectTwo, null});
                EPAssertionUtil.AssertProps(
                    env.GetEnumerator("set").Advance(),
                    fieldsTop,
                    new object[] {1, s0objectTwo, null});

                // set via API to null
                IDictionary<DeploymentIdNamePair, object> newValues = new Dictionary<DeploymentIdNamePair, object>();
                newValues.Put(new DeploymentIdNamePair(depIdVarobject, "varobject"), null);
                newValues.Put(new DeploymentIdNamePair(depIdVartype, "vartype"), null);
                newValues.Put(new DeploymentIdNamePair(depIdVarbean, "varbean"), null);
                env.Runtime.VariableService.SetVariableValue(newValues);
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});

                // set via API to values
                newValues.Put(new DeploymentIdNamePair(depIdVarobject, "varobject"), 10L);
                newValues.Put(new DeploymentIdNamePair(depIdVartype, "vartype"), s0objectTwo);
                newValues.Put(new DeploymentIdNamePair(depIdVarbean, "varbean"), a1objectOne);
                env.Runtime.VariableService.SetVariableValue(newValues);
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10L, a1objectOne, a1objectOne.Id, s0objectTwo, s0objectTwo.Id});

                // test on-set for Bean class
                env.CompileDeploy(
                    "@Name('set-two') on SupportBean_A(Id='Y') arrival set varobject=null, vartype=null, varbean=arrival",
                    path);
                env.AddListener("set-two");
                var a1objectTwo = new SupportBean_A("Y");
                env.SendEventBean(new SupportBean_A("Y"));
                Assert.AreEqual(null, env.Runtime.VariableService.GetVariableValue(depIdVarobject, "varobject"));
                Assert.AreEqual(null, env.Runtime.VariableService.GetVariableValue(depIdVartype, "vartype"));
                Assert.AreEqual(
                    a1objectTwo,
                    env.Runtime
                        .VariableService.GetVariableValue(
                            Collections.SingletonSet(new DeploymentIdNamePair(depIdVarbean, "varbean")))
                        .Get(new DeploymentIdNamePair(depIdVarbean, "varbean")));
                EPAssertionUtil.AssertProps(
                    env.Listener("set-two").AssertOneGetNewAndReset(),
                    fieldsTop,
                    new object[] {null, null, a1objectTwo});
                EPAssertionUtil.AssertProps(
                    env.GetEnumerator("set-two").Advance(),
                    fieldsTop,
                    new object[] {null, null, a1objectTwo});

                env.UndeployAll();
            }
        }

        public class NonSerializable
        {
            public NonSerializable(string myString)
            {
                MyString = myString;
            }

            public string MyString { get; }

            public override bool Equals(object o)
            {
                if (this == o) {
                    return true;
                }

                if (o == null || GetType() != o.GetType()) {
                    return false;
                }

                var that = (NonSerializable) o;

                return MyString.Equals(that.MyString);
            }

            public override int GetHashCode()
            {
                return MyString.GetHashCode();
            }
        }
    }
} // end of namespace