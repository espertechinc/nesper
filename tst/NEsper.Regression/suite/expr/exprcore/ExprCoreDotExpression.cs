///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;


namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreDotExpression
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithObjectEquals(execs);
            WithExpressionEnumValue(execs);
            WithMapIndexPropertyRooted(execs);
            WithInvalid(execs);
            WithChainedUnparameterized(execs);
            WithChainedParameterized(execs);
            WithArrayPropertySizeAndGet(execs);
            WithArrayPropertySizeAndGetChained(execs);
            WithNestedPropertyInstanceExpr(execs);
            WithNestedPropertyInstanceNW(execs);
            WithCollectionSelectFromGetAndSize(execs);
            WithToArray(execs);
            WithAggregationSimpleValueMethod(execs);
            WithObjectSimpleEventProperty(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithObjectSimpleEventProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotObjectSimpleEventProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationSimpleValueMethod(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotAggregationSimpleValueMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithToArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotToArray());
            return execs;
        }

        public static IList<RegressionExecution> WithCollectionSelectFromGetAndSize(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotCollectionSelectFromGetAndSize());
            return execs;
        }

        public static IList<RegressionExecution> WithNestedPropertyInstanceNW(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotNestedPropertyInstanceNW());
            return execs;
        }

        public static IList<RegressionExecution> WithNestedPropertyInstanceExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotNestedPropertyInstanceExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayPropertySizeAndGetChained(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotArrayPropertySizeAndGetChained());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayPropertySizeAndGet(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotArrayPropertySizeAndGet());
            return execs;
        }

        public static IList<RegressionExecution> WithChainedParameterized(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotChainedParameterized());
            return execs;
        }

        public static IList<RegressionExecution> WithChainedUnparameterized(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotChainedUnparameterized());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithMapIndexPropertyRooted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotMapIndexPropertyRooted());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionEnumValue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotExpressionEnumValue());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectEquals(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreDotObjectEquals());
            return execs;
        }

        private class ExprCoreDotObjectSimpleEventProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create constant variable string MYVAR = " +
                          typeof(ExprCoreDotExpression).FullName +
                          ".supportDotExpressionReturningSB('X').TheString;\n" +
                          "@name('s0') select " +
                          typeof(ExprCoreDotExpression).FullName +
                          ".supportDotExpressionReturningSB(P00).TheString as c0, MYVAR as c1 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "a"));
                env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { "a", "X" });

                env.UndeployAll();
            }
        }

        private class ExprCoreDotAggregationSimpleValueMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema MyEvent(col decimal);\n" +
                          "@name('s0') select first(col).abs() as c0 from MyEvent#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventMap(Collections.SingletonDataMap("col", (-1m)), "MyEvent");
                env.AssertPropsNew(
                    "s0",
                    "c0".SplitCsv(),
                    new object[] {
                        1m
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreDotToArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema MyEvent(mycoll `System.Collections.Generic.ICollection<object>`);\n" +
                          "@name('s0') select mycoll.ToArray() as c0 " +
                          "from MyEvent";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventMap(Collections.SingletonDataMap("mycoll", Collections.List(1, 2)), "MyEvent");
                var expected = new object[] { 1, 2 };
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { expected });

                env.UndeployAll();
            }
        }

        private class ExprCoreDotCollectionSelectFromGetAndSize : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var ss = typeof(StringExtensions).FullName;
                var epl =
					$"@Name('s0') " +
					$"select {ss}.SplitCsv(P01).selectFrom(v -> v).size() as sz " +
					$"from SupportBean_S0(P00={ss}.SplitCsv(P01).selectFrom(v -> v).get(2))";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, "A", "A,B,C", null);
                SendAssert(env, "A", "C,B,A", 3);
                SendAssert(env, "A", "", null);
                SendAssert(env, "A", "A,B,C,A", null);
                SendAssert(env, "A", "A,B,A,B", 4);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string p00,
                string p01,
                int? sizeExpected)
            {
                env.SendEventBean(new SupportBean_S0(0, p00, p01));
                if (sizeExpected == null) {
                    env.AssertListenerNotInvoked("s0");
                }
                else {
                    env.AssertEqualsNew("s0", "sz", sizeExpected);
                }
            }
        }

        private class ExprCoreDotObjectEquals : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
				var epl = "@Name('s0') select sb.Equals(maxBy(IntPrimitive)) as c0 from SupportBean as sb";
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

        private class ExprCoreDotExpressionEnumValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean", "sb")
                    .WithExpression(fields[0], "IntPrimitive = SupportEnumTwo.ENUM_VALUE_1.GetAssociatedValue()")
                    .WithExpression(fields[1], "SupportEnumTwo.ENUM_VALUE_2.CheckAssociatedValue(IntPrimitive)")
                    .WithExpression(fields[2], "SupportEnumTwo.ENUM_VALUE_3.GetNested().GetValue()")
                    .WithExpression(fields[3], "SupportEnumTwo.ENUM_VALUE_2.CheckEventBeanPropInt(sb, 'IntPrimitive')")
                    .WithExpression(fields[4], "SupportEnumTwo.ENUM_VALUE_2.CheckEventBeanPropInt(*, 'IntPrimitive')")
                    .WithExpression(fields[5], "SupportEnumTwo.ENUM_VALUE_2.GetMyStringsAsList()")
                    .WithExpression(fields[6], "SupportEnumTwo.ENUM_VALUE_2.GetNested().GetMyStringsNestedAsList()");

                builder.WithStatementConsumer(
                    stmt => {
                        ClassicAssert.AreEqual(typeof(IList<string>), stmt.EventType.GetPropertyType("c5"));
                        ClassicAssert.AreEqual(typeof(ICollection<string>), stmt.EventType.GetPropertyType("c6"));
                    });

                var strings = Arrays.AsList("2", "0", "0");
                builder.WithAssertion(new SupportBean("E1", 100))
                    .Expect(fields, true, false, 300, false, false, strings, strings);
                builder.WithAssertion(new SupportBean("E1", 200))
                    .Expect(fields, false, true, 300, true, true, strings, strings);

                builder.Run(env);
                env.UndeployAll();

                // test "events" reserved keyword in package name
                env.CompileDeploy("select " + typeof(SampleEnumInEventsPackage).FullName + ".A from SupportBean");

                env.UndeployAll();
            }
        }

        private class ExprCoreDotMapIndexPropertyRooted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
				var epl = "@Name('s0') select " +
				          "InnerTypes('key1') as c0,\n" +
				          "InnerTypes(Key) as c1,\n" +
				          "InnerTypes('key1').Ids[1] as c2,\n" +
				          "InnerTypes(Key).GetIds(Subkey) as c3,\n" +
				          "InnerTypesArray[1].Ids[1] as c4,\n" +
				          "InnerTypesArray(Subkey).GetIds(Subkey) as c5,\n" +
				          "InnerTypesArray(Subkey).GetIds(s0, 'xyz') as c6,\n" +
				          "InnerTypesArray(Subkey).GetIds(*, 'xyz') as c7\n" +
                          "from SupportEventTypeErasure as s0";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        ClassicAssert.AreEqual(
                            typeof(SupportEventInnerTypeWGetIds),
                            statement.EventType.GetPropertyType("c0"));
                        ClassicAssert.AreEqual(
                            typeof(SupportEventInnerTypeWGetIds),
                            statement.EventType.GetPropertyType("c1"));
                        ClassicAssert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("c2"));
                        ClassicAssert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("c3"));
                    });

                var @event = new SupportEventTypeErasure(
                    "key1",
                    2,
                    Collections.SingletonMap("key1", new SupportEventInnerTypeWGetIds(new int[] { 20, 30, 40 })),
                    new SupportEventInnerTypeWGetIds[] {
                        new SupportEventInnerTypeWGetIds(new int[] { 2, 3 }),
                        new SupportEventInnerTypeWGetIds(new int[] { 4, 5 }),
                        new SupportEventInnerTypeWGetIds(new int[] { 6, 7, 8 })
                    });
                env.SendEventBean(@event);
                env.AssertPropsNew(
                    "s0",
                    "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv(),
                    new object[]
                        { @event.InnerTypes.Get("key1"), @event.InnerTypes.Get("key1"), 30, 40, 5, 8, 999999, 999999 });

                env.UndeployAll();
            }
        }

        private class ExprCoreDotInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select abc.noSuchMethod() from SupportBean abc",
                    "Failed to validate select-clause expression 'abc.noSuchMethod()': Failed to solve 'noSuchMethod' to either an date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'noSuchMethod': Could not find enumeration method, date-time method, instance method or property named 'noSuchMethod' in class '" +
					typeof(SupportBean).CleanName() +
                    "' taking no parameters [select abc.noSuchMethod() from SupportBean abc]");
                env.TryInvalidCompile(
                    "select abc.GetChildOne(\"abc\", 10).noSuchMethod() from SupportChainTop abc",
                    "Failed to validate select-clause expression 'abc.GetChildOne(\"abc\",10).noSuchMethod()': Failed to solve 'GetChildOne' to either an date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'noSuchMethod': Could not find enumeration method, date-time method, instance method or property named 'noSuchMethod' in class '" +
					typeof(SupportChainChildOne).CleanName() +
                    "' taking no parameters [select abc.GetChildOne(\"abc\", 10).noSuchMethod() from SupportChainTop abc]");

                var epl =
                    $"import {typeof(MyHelperWithPrivateModifierAndPublicMethod).MaskTypeName()};\n" +
                    $"select {typeof(MyHelperWithPrivateModifierAndPublicMethod).MaskTypeName()}.CallMe() from SupportBean;\n";
                env.TryInvalidCompile(
                    epl,
					"Failed to validate select-clause expression 'com.espertech.esper.regressionlib.s...(127 chars)': Failed to resolve 'com.espertech.esper.regressionlib.suite.expr.exprcore.ExprCoreDotExpression$MyHelperWithPrivateModifierAndPublicMethod.CallMe' to");
            }
        }

        private class ExprCoreDotNestedPropertyInstanceExpr : RegressionExecution
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
                env.AssertPropsNew(
                    "s0",
                    "val0,val1,val2".SplitCsv(),
                    new object[] { "level1:10", "level2:20", "level3:30" });

                env.UndeployAll();
            }
        }

        private class ExprCoreDotNestedPropertyInstanceNW : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window NodeWindow#unique(Id) as SupportEventNode;\n";
                epl += "insert into NodeWindow select * from SupportEventNode;\n";
				epl += "create window NodeDataWindow#unique(NodeId) as SupportEventNodeData;\n";
                epl += "insert into NodeDataWindow select * from SupportEventNodeData;\n";
                epl += "create schema NodeWithData(node SupportEventNode, data SupportEventNodeData);\n";
                epl += "create window NodeWithDataWindow#unique(node.Id) as NodeWithData;\n";
                epl += "insert into NodeWithDataWindow " +
				       "select node, data from NodeWindow node join NodeDataWindow as data on node.Id = data.NodeId;\n";
				epl += "@Name('s0') select node.Id, data.NodeId, data.Value, node.Compute(data) from NodeWithDataWindow;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEventNode("1"));
                env.SendEventBean(new SupportEventNode("2"));
                env.SendEventBean(new SupportEventNodeData("1", "xxx"));

                env.UndeployAll();
            }
        }

        private class ExprCoreDotChainedUnparameterized : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
				var epl = "@Name('s0') select " +
				          "Nested.GetNestedValue(), " +
				          "Nested.GetNestedNested().GetNestedNestedValue() " +
                          "from SupportBeanComplexProps";
                env.CompileDeploy(epl).AddListener("s0");

                var bean = SupportBeanComplexProps.MakeDefaultBean();
                var rows = new object[][] {
                    new object[] { "Nested.GetNestedValue()", typeof(string) }
                };
                env.AssertStatement(
                    "s0",
                    statement => {
                        for (var i = 0; i < rows.Length; i++) {
                            var prop = statement.EventType.PropertyDescriptors[i];
                            ClassicAssert.AreEqual(rows[i][0], prop.PropertyName);
                            ClassicAssert.AreEqual(rows[i][1], prop.PropertyType);
                        }
                    });

                env.SendEventBean(bean);
                env.AssertPropsNew(
                    "s0",
                    "Nested.GetNestedValue()".SplitCsv(),
                    new object[] { bean.Nested.NestedValue });

                env.UndeployAll();
            }
        }

        private class ExprCoreDotChainedParameterized : RegressionExecution
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

        private class ExprCoreDotArrayPropertySizeAndGet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "(ArrayProperty).size() as size, " +
                          "(ArrayProperty).get(0) as get0, " +
                          "(ArrayProperty).get(1) as get1, " +
                          "(ArrayProperty).get(2) as get2, " +
                          "(ArrayProperty).get(3) as get3 " +
                          "from SupportBeanComplexProps";
                env.CompileDeploy(epl).AddListener("s0");

                var bean = SupportBeanComplexProps.MakeDefaultBean();
                var rows = new object[][] {
                    new object[] { "size", typeof(int?) },
                    new object[] { "get0", typeof(int?) },
                    new object[] { "get1", typeof(int?) },
                    new object[] { "get2", typeof(int?) },
                    new object[] { "get3", typeof(int?) }
                };
                env.AssertStatement(
                    "s0",
                    statement => {
                        for (var i = 0; i < rows.Length; i++) {
                            var prop = statement.EventType.PropertyDescriptors[i];
                            ClassicAssert.AreEqual(rows[i][0], prop.PropertyName, "failed for " + rows[i][0]);
                            ClassicAssert.AreEqual(rows[i][1], prop.PropertyType, "failed for " + rows[i][0]);
                        }
                    });

                env.SendEventBean(bean);
                env.AssertPropsNew(
                    "s0",
                    "size,get0,get1,get2,get3".SplitCsv(),
                    new object[] {
                        bean.ArrayProperty.Length,
                        bean.ArrayProperty[0],
                        bean.ArrayProperty[1],
                        bean.ArrayProperty[2],
                        null
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreDotArrayPropertySizeAndGetChained : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "(abc).GetArray().size() as size, " +
                          "(abc).GetArray().get(0).GetNestLevOneVal() as get0 " +
                          "from SupportBeanCombinedProps as abc";
                env.CompileDeploy(epl).AddListener("s0");

                var bean = SupportBeanCombinedProps.MakeDefaultBean();
                var rows = new object[][] {
                    new object[] { "size", typeof(int?) },
                    new object[] { "get0", typeof(string) },
                };
                env.AssertStatement(
                    "s0",
                    statement => {
                        for (var i = 0; i < rows.Length; i++) {
                            var prop = statement.EventType.PropertyDescriptors[i];
                            ClassicAssert.AreEqual(rows[i][0], prop.PropertyName);
                            ClassicAssert.AreEqual(rows[i][1], prop.PropertyType);
                        }
                    });

                env.SendEventBean(bean);
                env.AssertPropsNew(
                    "s0",
                    "size,get0".SplitCsv(),
                    new object[] { bean.Array.Length, bean.Array[0].NestLevOneVal });

                env.UndeployAll();
            }
        }

        private static void AssertChainedParam(
            RegressionEnvironment env,
            string subexpr)
        {
            var rows = new object[][] {
                new object[] { subexpr, typeof(SupportChainChildTwo) }
            };
            env.AssertStatement(
                "s0",
                statement => {
                    for (var i = 0; i < rows.Length; i++) {
                        var prop = statement.EventType.PropertyDescriptors[i];
                        ClassicAssert.AreEqual(rows[i][0], prop.PropertyName);
                        ClassicAssert.AreEqual(rows[i][1], prop.PropertyType);
                    }
                });

            env.SendEventBean(new SupportChainTop());
            env.AssertEventNew(
                "s0",
                @event => {
                    var result = @event.Get(subexpr);
                    ClassicAssert.AreEqual("abcappend", ((SupportChainChildTwo)result).Text);
                });
        }

        private static void SendAssertDotObjectEquals(
            RegressionEnvironment env,
            int intPrimitive,
            bool expected)
        {
            env.SendEventBean(new SupportBean(UuidGenerator.Generate(), intPrimitive));
            env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { expected });
        }

        public static SupportBean SupportDotExpressionReturningSB(string theString)
        {
            return new SupportBean(theString, 0);
        }

        private class MyHelperWithPrivateModifierAndPublicMethod
        {
            public string CallMe()
            {
                return null;
            }
        }
    }
} // end of namespace