///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreDotExpression
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprCoreDotObjectEquals());
            execs.Add(new ExprCoreDotExpressionEnumValue());
            execs.Add(new ExprCoreDotMapIndexPropertyRooted());
            execs.Add(new ExprCoreDotInvalid());
            execs.Add(new ExprCoreDotChainedUnparameterized());
            execs.Add(new ExprCoreDotChainedParameterized());
            execs.Add(new ExprCoreDotArrayPropertySizeAndGet());
            execs.Add(new ExprCoreDotArrayPropertySizeAndGetChained());
            execs.Add(new ExprCoreDotNestedPropertyInstanceExpr());
            execs.Add(new ExprCoreDotNestedPropertyInstanceNW());
            return execs;
        }

        private static void AssertChainedParam(
            RegressionEnvironment env,
            string subexpr)
        {
            object[][] rows = {
                new object[] {subexpr, typeof(SupportChainChildTwo)}
            };
            for (var i = 0; i < rows.Length; i++) {
                var prop = env.Statement("s0").EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }

            env.SendEventBean(new SupportChainTop());
            var result = env.Listener("s0").AssertOneGetNewAndReset().Get(subexpr);
            Assert.AreEqual("abcappend", ((SupportChainChildTwo) result).Text);
        }

        private static void SendAssertDotObjectEquals(
            RegressionEnvironment env,
            int intPrimitive,
            bool expected)
        {
            env.SendEventBean(new SupportBean(UuidGenerator.Generate(), intPrimitive));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "c0" },
                new object[] {expected});
        }

        internal class ExprCoreDotObjectEquals : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select sb.equals(maxBy(IntPrimitive)) as c0 from SupportBean as sb";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertDotObjectEquals(env, 10, true);
                SendAssertDotObjectEquals(env, 9, false);
                SendAssertDotObjectEquals(env, 11, true);
                SendAssertDotObjectEquals(env, 8, false);
                SendAssertDotObjectEquals(env, 11, false);
                SendAssertDotObjectEquals(env, 12, true);

                env.UndeployAll();
            }
        }

        internal class ExprCoreDotExpressionEnumValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1", "c2", "c3" };
                var epl = "@Name('s0') select " +
                          "IntPrimitive = SupportEnumTwo.ENUM_VALUE_1.GetAssociatedValue() as c0," +
                          "SupportEnumTwo.ENUM_VALUE_2.CheckAssociatedValue(IntPrimitive) as c1," +
                          "SupportEnumTwo.ENUM_VALUE_3.GetNested().GetValue() as c2," +
                          "SupportEnumTwo.ENUM_VALUE_2.CheckEventBeanPropInt(sb, 'IntPrimitive') as c3," +
                          "SupportEnumTwo.ENUM_VALUE_2.CheckEventBeanPropInt(*, 'IntPrimitive') as c4 " +
                          "from SupportBean as sb";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false, 300, false});

                env.SendEventBean(new SupportBean("E1", 200));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, 300, true});

                env.UndeployAll();

                // test "events" reserved keyword in package name
                env.CompileDeploy("select " + typeof(SampleEnumInEventsPackage).Name + ".A from SupportBean");

                env.UndeployAll();
            }
        }

        internal class ExprCoreDotMapIndexPropertyRooted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "InnerTypes('key1') as c0,\n" +
                          "InnerTypes(key) as c1,\n" +
                          "InnerTypes('key1').Ids[1] as c2,\n" +
                          "InnerTypes(key).GetIds(subkey) as c3,\n" +
                          "InnerTypesArray[1].Ids[1] as c4,\n" +
                          "InnerTypesArray(Subkey).GetIds(Subkey) as c5,\n" +
                          "InnerTypesArray(Subkey).GetIds(s0, 'xyz') as c6,\n" +
                          "InnerTypesArray(Subkey).GetIds(*, 'xyz') as c7\n" +
                          "from SupportEventTypeErasure as S0";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(
                    typeof(SupportEventInnerTypeWGetIds),
                    env.Statement("s0").EventType.GetPropertyType("c0"));
                Assert.AreEqual(
                    typeof(SupportEventInnerTypeWGetIds),
                    env.Statement("s0").EventType.GetPropertyType("c1"));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("c2"));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("c3"));

                var @event = new SupportEventTypeErasure(
                    "key1",
                    2,
                    Collections.SingletonMap("key1", new SupportEventInnerTypeWGetIds(new[] {20, 30, 40})),
                    new[] {
                        new SupportEventInnerTypeWGetIds(new[] {2, 3}), new SupportEventInnerTypeWGetIds(new[] {4, 5}),
                        new SupportEventInnerTypeWGetIds(new[] {6, 7, 8})
                    });
                env.SendEventBean(@event);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7" },
                    new object[] {
                        @event.InnerTypes.Get("key1"),
                        @event.InnerTypes.Get("key1"),
                        30, 40, 5, 8, 999999, 999999
                    });

                env.UndeployAll();
            }
        }

        internal class ExprCoreDotInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select abc.noSuchMethod() from SupportBean abc",
                    "Failed to validate select-clause expression 'abc.noSuchMethod()': Failed to solve 'noSuchMethod' to either an date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'noSuchMethod': Could not find enumeration method, date-time method or instance method named 'noSuchMethod' in class '" +
                    typeof(SupportBean).Name +
                    "' taking no parameters [select abc.noSuchMethod() from SupportBean abc]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select abc.GetChildOne(\"abc\", 10).noSuchMethod() from SupportChainTop abc",
                    "Failed to validate select-clause expression 'abc.GetChildOne(\"abc\",10).noSuchMethod()': Failed to solve 'getChildOne' to either an date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'noSuchMethod': Could not find enumeration method, date-time method or instance method named 'noSuchMethod' in class '" +
                    typeof(SupportChainChildOne).Name +
                    "' taking no parameters [select abc.GetChildOne(\"abc\", 10).noSuchMethod() from SupportChainTop abc]");
            }
        }

        internal class ExprCoreDotNestedPropertyInstanceExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "LevelOne.GetCustomLevelOne(10) as val0, " +
                          "LevelOne.LevelTwo.GetCustomLevelTwo(20) as val1, " +
                          "LevelOne.LevelTwo.LevelThree.GetCustomLevelThree(30) as val2 " +
                          "from SupportLevelZero";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(
                    new SupportLevelZero(new SupportLevelOne(new SupportLevelTwo(new SupportLevelThree()))));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "val0","val1","val2" },
                    new object[] {"level1:10", "level2:20", "level3:30"});

                env.UndeployAll();
            }
        }

        internal class ExprCoreDotNestedPropertyInstanceNW : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window NodeWindow#unique(Id) as SupportEventNode;\n";
                epl += "insert into NodeWindow select * from SupportEventNode;\n";
                epl += "create window NodeDataWindow#unique(nodeId) as SupportEventNodeData;\n";
                epl += "insert into NodeDataWindow select * from SupportEventNodeData;\n";
                epl += "create schema NodeWithData(node SupportEventNode, data SupportEventNodeData);\n";
                epl += "create window NodeWithDataWindow#unique(node.Id) as NodeWithData;\n";
                epl += "insert into NodeWithDataWindow " +
                       "select node, data from NodeWindow node join NodeDataWindow as data on node.Id = data.nodeId;\n";
                epl +=
                    "@Name('s0') select node.Id, data.nodeId, data.Value, node.compute(data) from NodeWithDataWindow;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEventNode("1"));
                env.SendEventBean(new SupportEventNode("2"));
                env.SendEventBean(new SupportEventNodeData("1", "xxx"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreDotChainedUnparameterized : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "nested.getNestedValue(), " +
                          "nested.getNestedNested().getNestedNestedValue() " +
                          "from SupportBeanComplexProps";
                env.CompileDeploy(epl).AddListener("s0");

                var bean = SupportBeanComplexProps.MakeDefaultBean();
                object[][] rows = {
                    new object[] {"nested.getNestedValue()", typeof(string)}
                };
                for (var i = 0; i < rows.Length; i++) {
                    var prop = env.Statement("s0").EventType.PropertyDescriptors[i];
                    Assert.AreEqual(rows[i][0], prop.PropertyName);
                    Assert.AreEqual(rows[i][1], prop.PropertyType);
                }

                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "nested.getNestedValue()" },
                    new object[] {bean.Nested.NestedValue});

                env.UndeployAll();
            }
        }

        internal class ExprCoreDotChainedParameterized : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subexpr = "top.GetChildOne(\"abc\",10).GetChildTwo(\"append\")";
                var epl = "@Name('s0') select " + subexpr + " from SupportChainTop as top";
                env.CompileDeploy(epl).AddListener("s0");
                AssertChainedParam(env, subexpr);
                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");
                AssertChainedParam(env, subexpr);
                env.UndeployAll();
            }
        }

        internal class ExprCoreDotArrayPropertySizeAndGet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "(ArrayProperty).size() as size, " +
                          "(ArrayProperty)[0] as get0, " +
                          "(ArrayProperty)[1] as get1, " +
                          "(ArrayProperty)[2] as get2, " +
                          "(ArrayProperty)[3] as get3 " +
                          "from SupportBeanComplexProps";
                env.CompileDeploy(epl).AddListener("s0");

                var bean = SupportBeanComplexProps.MakeDefaultBean();
                object[][] rows = {
                    new object[] {"size", typeof(int?)},
                    new object[] {"get0", typeof(int?)},
                    new object[] {"get1", typeof(int?)},
                    new object[] {"get2", typeof(int?)},
                    new object[] {"get3", typeof(int?)}
                };
                for (var i = 0; i < rows.Length; i++) {
                    var prop = env.Statement("s0").EventType.PropertyDescriptors[i];
                    Assert.AreEqual(rows[i][0], prop.PropertyName, "failed for " + rows[i][0]);
                    Assert.AreEqual(rows[i][1], prop.PropertyType, "failed for " + rows[i][0]);
                }

                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "size","get0","get1","get2","get3" },
                    new object[] {
                        bean.ArrayProperty.Length, bean.ArrayProperty[0], bean.ArrayProperty[1], bean.ArrayProperty[2],
                        null
                    });

                env.UndeployAll();
            }
        }

        internal class ExprCoreDotArrayPropertySizeAndGetChained : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "(abc).GetArray().Size() as size, " +
                          "(abc).GetArray()[0].GetNestLevOneVal() as get0 " +
                          "from SupportBeanCombinedProps as abc";
                env.CompileDeploy(epl).AddListener("s0");

                var bean = SupportBeanCombinedProps.MakeDefaultBean();
                object[][] rows = {
                    new object[] {"size", typeof(int?)},
                    new object[] {"get0", typeof(string)}
                };
                for (var i = 0; i < rows.Length; i++) {
                    var prop = env.Statement("s0").EventType.PropertyDescriptors[i];
                    Assert.AreEqual(rows[i][0], prop.PropertyName);
                    Assert.AreEqual(rows[i][1], prop.PropertyType);
                }

                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "size","get0" },
                    new object[] {bean.Array.Length, bean.Array[0].NestLevOneVal});

                env.UndeployAll();
            }
        }
    }
} // end of namespace