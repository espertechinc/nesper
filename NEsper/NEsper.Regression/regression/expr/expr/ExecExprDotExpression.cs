///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprDotExpression : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            RunAssertionDotObjectEquals(epService);
            RunAssertionDotExpressionEnumValue(epService);
            RunAssertionMapIndexPropertyRooted(epService);
            RunAssertionInvalid(epService);
            RunAssertionNestedPropertyInstanceExpr(epService);
            RunAssertionChainedUnparameterized(epService);
            RunAssertionChainedParameterized(epService);
            RunAssertionArrayPropertySizeAndGet(epService);
            RunAssertionArrayPropertySizeAndGetChained(epService);
        }

        private void RunAssertionDotObjectEquals(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var listener = new SupportUpdateListener();
            var stmt = epService.EPAdministrator.CreateEPL(
                "select sb.Equals(maxBy(IntPrimitive)) as c0 from SupportBean as sb");
            stmt.Events += listener.Update;

            SendAssertDotObjectEquals(epService, listener, 10, true);
            SendAssertDotObjectEquals(epService, listener, 9, false);
            SendAssertDotObjectEquals(epService, listener, 11, true);
            SendAssertDotObjectEquals(epService, listener, 8, false);
            SendAssertDotObjectEquals(epService, listener, 11, false);
            SendAssertDotObjectEquals(epService, listener, 12, true);

            stmt.Dispose();
        }

        private void RunAssertionDotExpressionEnumValue(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnumTwo));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            var listener = new SupportUpdateListener();
            var fields = "c0,c1,c2,c3".Split(',');
            var stmt = epService.EPAdministrator.CreateEPL(
                "select " +
                "IntPrimitive = SupportEnumTwo.ENUM_VALUE_1.GetAssociatedValue() as c0," +
                "SupportEnumTwo.ENUM_VALUE_2.CheckAssociatedValue(IntPrimitive) as c1, " +
                "SupportEnumTwo.ENUM_VALUE_3.GetNested().get_Value() as c2," +
                "SupportEnumTwo.ENUM_VALUE_2.CheckEventBeanPropInt(sb, 'IntPrimitive') as c3, " +
                "SupportEnumTwo.ENUM_VALUE_2.CheckEventBeanPropInt(*, 'IntPrimitive') as c4 " +
                "from SupportBean as sb");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {true, false, 300, false});

            epService.EPRuntime.SendEvent(new SupportBean("E1", 200));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {false, true, 300, true});

            // test "events" reserved keyword in package name
            epService.EPAdministrator.CreateEPL(
                "select " + typeof(SampleEnumInEventsPackage).FullName + ".A from SupportBean");

            stmt.Dispose();
        }

        private void RunAssertionMapIndexPropertyRooted(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyTypeErasure));
            var stmt = epService.EPAdministrator.CreateEPL(
                "select " +
                "InnerTypes('key1') as c0,\n" +
                "InnerTypes(key) as c1,\n" +
                "InnerTypes('key1').ids[1] as c2,\n" +
                "InnerTypes(key).GetIds(subkey) as c3,\n" +
                "innerTypesArray[1].ids[1] as c4,\n" +
                "InnerTypesArray(subkey).GetIds(subkey) as c5,\n" +
                "InnerTypesArray(subkey).GetIds(s0, 'xyz') as c6,\n" +
                "InnerTypesArray(subkey).GetIds(*, 'xyz') as c7\n" +
                "from MyTypeErasure as s0");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(InnerType), stmt.EventType.GetPropertyType("c0"));
            Assert.AreEqual(typeof(InnerType), stmt.EventType.GetPropertyType("c1"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("c2"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("c3"));

            var @event = new MyTypeErasure(
                "key1", 2, Collections.SingletonMap("key1", new InnerType(new[] {20, 30, 40})),
                new[] {new InnerType(new[] {2, 3}), new InnerType(new[] {4, 5}), new InnerType(new[] {6, 7, 8})});
            epService.EPRuntime.SendEvent(@event);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3,c4,c5,c6,c7".Split(','), new object[]
                {
                    @event.InnerTypes.Get("key1"), @event.InnerTypes.Get("key1"), 30, 40, 5, 8, 999999, 999999
                });

            stmt.Dispose();
        }

        private void RunAssertionInvalid(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportChainTop", typeof(SupportChainTop));

            TryInvalid(
                epService, "select abc.NoSuchMethod() from SupportBean abc",
                "Error starting statement: Failed to validate select-clause expression 'abc.NoSuchMethod()': Failed to solve 'NoSuchMethod' to either a date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'NoSuchMethod': Could not find enumeration method, date-time method or instance method named 'NoSuchMethod' in class '" +
                typeof(SupportBean).FullName +
                "' taking no parameters [select abc.NoSuchMethod() from SupportBean abc]");
            TryInvalid(
                epService, "select abc.GetChildOne(\"abc\", 10).NoSuchMethod() from SupportChainTop abc",
                "Error starting statement: Failed to validate select-clause expression 'abc.GetChildOne(\"abc\",10).NoSuchMethod()': Failed to solve 'GetChildOne' to either a date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'NoSuchMethod': Could not find enumeration method, date-time method or instance method named 'NoSuchMethod' in class '" +
                typeof(SupportChainChildOne).FullName +
                "' taking no parameters [select abc.GetChildOne(\"abc\", 10).NoSuchMethod() from SupportChainTop abc]");
        }

        private void RunAssertionNestedPropertyInstanceExpr(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType("LevelZero", typeof(LevelZero));
            var listener = new SupportUpdateListener();
            var stmt = epService.EPAdministrator.CreateEPL(
                "select " +
                "levelOne.GetCustomLevelOne(10) as val0, " +
                "levelOne.LevelTwo.GetCustomLevelTwo(20) as val1, " +
                "levelOne.LevelTwo.LevelThree.GetCustomLevelThree(30) as val2 " +
                "from LevelZero");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new LevelZero(new LevelOne(new LevelTwo(new LevelThree()))));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "val0,val1,val2".Split(','),
                new object[] {"level1:10", "level2:20", "level3:30"});

            // ESPER-772
            epService.EPAdministrator.Configuration.AddEventType<Node>();
            epService.EPAdministrator.Configuration.AddEventType<NodeData>();

            epService.EPAdministrator.CreateEPL("create window NodeWindow#unique(id) as Node");
            epService.EPAdministrator.CreateEPL("insert into NodeWindow select * from Node");

            epService.EPAdministrator.CreateEPL("create window NodeDataWindow#unique(nodeId) as NodeData");
            epService.EPAdministrator.CreateEPL("insert into NodeDataWindow select * from NodeData");

            epService.EPAdministrator.CreateEPL("create schema NodeWithData(node Node, data NodeData)");
            epService.EPAdministrator.CreateEPL("create window NodeWithDataWindow#unique(node.id) as NodeWithData");
            epService.EPAdministrator.CreateEPL(
                "insert into NodeWithDataWindow " +
                "select node, data from NodeWindow node join NodeDataWindow as data on node.id = data.nodeId");
            stmt.Dispose();

            stmt = epService.EPAdministrator.CreateEPL(
                "select node.id, data.nodeId, data.value, node.Compute(data) from NodeWithDataWindow");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new Node("1"));
            epService.EPRuntime.SendEvent(new Node("2"));
            epService.EPRuntime.SendEvent(new NodeData("1", "xxx"));

            stmt.Dispose();
        }

        private void RunAssertionChainedUnparameterized(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanComplexProps", typeof(SupportBeanComplexProps));

            var epl = "select " +
                      "nested.NestedValue, " +
                      "nested.NestedNested.NestedNestedValue " +
                      "from SupportBeanComplexProps";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var bean = SupportBeanComplexProps.MakeDefaultBean();
            var rows = new[]
            {
                new object[] {"nested.NestedValue", typeof(string)}
            };
            for (var i = 0; i < rows.Length; i++)
            {
                var prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }

            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNew(), "nested.NestedValue".Split(','), new object[] {bean.Nested.NestedValue});

            stmt.Dispose();
        }

        private void RunAssertionChainedParameterized(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType("SupportChainTop", typeof(SupportChainTop));

            var subexpr = "top.GetChildOne(\"abc\",10).GetChildTwo(\"append\")";
            var epl = "select " +
                      subexpr +
                      " from SupportChainTop as top";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            RunAssertionChainedParam(epService, stmt, listener, subexpr);

            listener.Reset();
            stmt.Dispose();
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;

            RunAssertionChainedParam(epService, stmt, listener, subexpr);

            stmt.Dispose();
        }

        private void RunAssertionArrayPropertySizeAndGet(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanComplexProps", typeof(SupportBeanComplexProps));

            var epl = "select " +
                      "(arrayProperty).size() as size, " +
                      "(arrayProperty).get(0) as get0, " +
                      "(arrayProperty).get(1) as get1, " +
                      "(arrayProperty).get(2) as get2, " +
                      "(arrayProperty).get(3) as get3 " +
                      "from SupportBeanComplexProps";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var bean = SupportBeanComplexProps.MakeDefaultBean();
            var rows = new[]
            {
                new object[] {"size", typeof(int?)},
                new object[] {"get0", typeof(int)},
                new object[] {"get1", typeof(int)},
                new object[] {"get2", typeof(int)},
                new object[] {"get3", typeof(int)}
            };
            for (var i = 0; i < rows.Length; i++)
            {
                var prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, "failed for " + rows[i][0]);
                Assert.AreEqual(rows[i][1], prop.PropertyType, "failed for " + rows[i][0]);
            }

            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNew(), "size,get0,get1,get2,get3".Split(','),
                new object[]
                {
                    bean.ArrayProperty.Length, bean.ArrayProperty[0], bean.ArrayProperty[1], bean.ArrayProperty[2], null
                });

            stmt.Dispose();
        }

        private void RunAssertionArrayPropertySizeAndGetChained(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanCombinedProps", typeof(SupportBeanCombinedProps));

            var epl = "select " +
                      "(abc).GetArray().size() as size, " +
                      "(abc).GetArray().get(0).GetNestLevOneVal() as get0 " +
                      "from SupportBeanCombinedProps as abc";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var bean = SupportBeanCombinedProps.MakeDefaultBean();
            var rows = new[]
            {
                new object[] {"size", typeof(int?)},
                new object[] {"get0", typeof(string)}
            };
            for (var i = 0; i < rows.Length; i++)
            {
                var prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }

            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNew(), "size,get0".Split(','),
                new object[] {bean.Array.Length, bean.Array[0].NestLevOneVal});

            stmt.Dispose();
        }

        private void RunAssertionChainedParam(
            EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener, string subexpr)
        {
            var rows = new[]
            {
                new object[] {subexpr, typeof(SupportChainChildTwo)}
            };
            for (var i = 0; i < rows.Length; i++)
            {
                var prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }

            epService.EPRuntime.SendEvent(new SupportChainTop());
            var result = listener.AssertOneGetNewAndReset().Get(subexpr);
            Assert.AreEqual("abcappend", ((SupportChainChildTwo) result).GetText());
        }

        private void SendAssertDotObjectEquals(
            EPServiceProvider epService, SupportUpdateListener listener, int intPrimitive, bool expected)
        {
            epService.EPRuntime.SendEvent(new SupportBean(null, intPrimitive));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[] {expected});
        }

        public class LevelZero
        {
            internal LevelZero(LevelOne levelOne)
            {
                LevelOne = levelOne;
            }

            public LevelOne LevelOne { get; }
        }

        public class LevelOne
        {
            public LevelOne(LevelTwo levelTwo)
            {
                LevelTwo = levelTwo;
            }

            public LevelTwo LevelTwo { get; }

            public string GetCustomLevelOne(int val)
            {
                return "level1:" + val;
            }
        }

        public class LevelTwo
        {
            public LevelTwo(LevelThree levelThree)
            {
                LevelThree = levelThree;
            }

            public LevelThree LevelThree { get; }

            public string GetCustomLevelTwo(int val)
            {
                return "level2:" + val;
            }
        }

        public class LevelThree
        {
            public string GetCustomLevelThree(int val)
            {
                return "level3:" + val;
            }
        }

        public class MyTypeErasure
        {
            public MyTypeErasure(
                string key, int subkey, IDictionary<string, InnerType> innerTypes, InnerType[] innerTypesArray)
            {
                Key = key;
                Subkey = subkey;
                InnerTypes = innerTypes;
                InnerTypesArray = innerTypesArray;
            }

            public string Key { get; }

            public int Subkey { get; }

            public IDictionary<string, InnerType> InnerTypes { get; }

            public InnerType[] InnerTypesArray { get; }
        }

        public class InnerType
        {
            private readonly int[] _ids;

            public InnerType(int[] ids)
            {
                _ids = ids;
            }

            public int[] GetIds()
            {
                return _ids;
            }

            public int GetIds(int subkey)
            {
                return _ids[subkey];
            }

            public int GetIds(EventBean @event, string name)
            {
                return 999999;
            }
        }

        public class Node
        {
            private readonly string _id;

            public Node(string id)
            {
                this._id = id;
            }

            public string Id => _id;

            public string Compute(object data)
            {
                if (data == null)
                {
                    return null;
                }

                var nodeData = (NodeData) ((EventBean) data).Underlying;
                return _id + nodeData.Value;
            }
        }

        public class NodeData
        {
            private readonly string _nodeId;
            private readonly string _value;

            public NodeData(string nodeId, string value)
            {
                _nodeId = nodeId;
                _value = value;
            }

            public string NodeId => _nodeId;

            public string Value => _value;
        }
    }
} // end of namespace