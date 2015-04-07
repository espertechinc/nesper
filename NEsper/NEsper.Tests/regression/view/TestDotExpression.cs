///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestDotExpression 
	{
		private EPServiceProvider _epService;
		private SupportUpdateListener _listener;

        [SetUp]
		public void SetUp()
		{
		    _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
		    _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
	        _listener = new SupportUpdateListener();
		}

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
        public void TestDotObjectEquals()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.CreateEPL("select sb.Equals(maxBy(IntPrimitive)) as c0 from SupportBean as sb").AddListener(_listener);

            SendAssertDotObjectEquals(10, true);
            SendAssertDotObjectEquals(9, false);
            SendAssertDotObjectEquals(11, true);
            SendAssertDotObjectEquals(8, false);
            SendAssertDotObjectEquals(11, false);
            SendAssertDotObjectEquals(12, true);
        }

        private void SendAssertDotObjectEquals(int intPrimitive, bool expected)
        {
            _epService.EPRuntime.SendEvent(new SupportBean(null, intPrimitive));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[] {expected});
        }


        [Test]
	    public void TestDotExpressionEnumValue()
        {
	        _epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnumTwo));
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));

	        var fields = "c0,c1,c2,c3".Split(',');
	        _epService.EPAdministrator.CreateEPL("select " +
	                "IntPrimitive = SupportEnumTwo.ENUM_VALUE_1.GetAssociatedValue() as c0," +
	                "SupportEnumTwo.ENUM_VALUE_2.CheckAssociatedValue(IntPrimitive) as c1, " +
	                "SupportEnumTwo.ENUM_VALUE_3.GetNested().get_Value() as c2," +
	                "SupportEnumTwo.ENUM_VALUE_2.CheckEventBeanPropInt(sb, 'IntPrimitive') as c3, " +
	                "SupportEnumTwo.ENUM_VALUE_2.CheckEventBeanPropInt(*, 'IntPrimitive') as c4 " +
	                "from SupportBean as sb").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {true, false, 300, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 200));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {false, true, 300, true});

	        // test "events" reserved keyword in package name
	        _epService.EPAdministrator.CreateEPL("select com.espertech.esper.support.events.SampleEnumInEventsPackage.A from SupportBean");
	    }

        [Test]
	    public void TestMapIndexPropertyRooted()
        {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(MyTypeErasure));
	        var stmt = _epService.EPAdministrator.CreateEPL("select " +
                    "InnerTypes('key1') as c0,\n" +
	                "InnerTypes(key) as c1,\n" +
                    "InnerTypes('key1').ids[1] as c2,\n" +
                    "InnerTypes(key).GetIds(subkey) as c3,\n" +
	                "InnerTypesArray[1].ids[1] as c4,\n" +
                    "InnerTypesArray(subkey).GetIds(subkey) as c5,\n" +
                    "InnerTypesArray(subkey).GetIds(s0, 'xyz') as c6,\n" +
                    "InnerTypesArray(subkey).GetIds(*, 'xyz') as c7\n" +
	                "from MyTypeErasure as s0");
	        stmt.AddListener(_listener);
            Assert.That(stmt.EventType.GetPropertyType("c0"), Is.EqualTo(typeof(InnerType)));
            Assert.That(stmt.EventType.GetPropertyType("c1"), Is.EqualTo(typeof(InnerType)));
            Assert.That(stmt.EventType.GetPropertyType("c2"), Is.EqualTo(typeof(int?)));
            Assert.That(stmt.EventType.GetPropertyType("c3"), Is.EqualTo(typeof(int)));

	        var @event = new MyTypeErasure("key1", 2, Collections.SingletonMap("key1", new InnerType(new int[] {20, 30, 40})), new InnerType[] {new InnerType(new int[] {2, 3}), new InnerType(new int[] {4, 5}), new InnerType(new int[] {6, 7, 8})});
	        _epService.EPRuntime.SendEvent(@event);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3,c4,c5,c6,c7".Split(','), new object[] {@event.InnerTypes.Get("key1"), @event.InnerTypes.Get("key1"), 30, 40, 5, 8, 999999, 999999});
	    }

        [Test]
	    public void TestInvalid()
        {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportChainTop", typeof(SupportChainTop));

	        TryInvalid("select abc.noSuchMethod() from SupportBean abc",
	                "Error starting statement: Failed to validate select-clause expression 'abc.noSuchMethod()': Failed to solve 'noSuchMethod' to either a date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'noSuchMethod': Could not find enumeration method, date-time method or instance method named 'noSuchMethod' in class 'com.espertech.esper.support.bean.SupportBean' taking no parameters [select abc.noSuchMethod() from SupportBean abc]");
	        TryInvalid("select abc.GetChildOne(\"abc\", 10).noSuchMethod() from SupportChainTop abc",
	                "Error starting statement: Failed to validate select-clause expression 'abc.GetChildOne(\"abc\",10).noSuchMethod()': Failed to solve 'GetChildOne' to either a date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'noSuchMethod': Could not find enumeration method, date-time method or instance method named 'noSuchMethod' in class 'com.espertech.esper.support.bean.SupportChainChildOne' taking no parameters [select abc.GetChildOne(\"abc\", 10).noSuchMethod() from SupportChainTop abc]");
	    }

        [Test]
	    public void TestNestedPropertyInstanceExpr()
        {
	        _epService.EPAdministrator.Configuration.AddEventType("LevelZero", typeof(LevelZero));
	        _epService.EPAdministrator.CreateEPL("select " +
	                "levelOne.GetCustomLevelOne(10) as val0, " +
	                "levelOne.levelTwo.GetCustomLevelTwo(20) as val1, " +
	                "levelOne.levelTwo.levelThree.GetCustomLevelThree(30) as val2 " +
	                "from LevelZero").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new LevelZero(new LevelOne(new LevelTwo(new LevelThree()))));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0,val1,val2".Split(','), new object[]{"level1:10", "level2:20", "level3:30"});

	        // ESPER-772
	        _epService.EPAdministrator.Configuration.AddEventType<Node>();
	        _epService.EPAdministrator.Configuration.AddEventType<NodeData>();

	        _epService.EPAdministrator.CreateEPL("create window NodeWindow.std:unique(id) as Node");
	        _epService.EPAdministrator.CreateEPL("insert into NodeWindow select * from Node");

	        _epService.EPAdministrator.CreateEPL("create window NodeDataWindow.std:unique(nodeId) as NodeData");
	        _epService.EPAdministrator.CreateEPL("insert into NodeDataWindow select * from NodeData");

	        _epService.EPAdministrator.CreateEPL("create schema NodeWithData(node Node, data NodeData)");
	        _epService.EPAdministrator.CreateEPL("create window NodeWithDataWindow.std:unique(node.id) as NodeWithData");
	        _epService.EPAdministrator.CreateEPL("insert into NodeWithDataWindow " +
	                "select node, data from NodeWindow node join NodeDataWindow as data on node.id = data.nodeId");

	        var stmt = _epService.EPAdministrator.CreateEPL("select node.id, data.nodeId, data.value, node.Compute(data) from NodeWithDataWindow");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new Node("1"));
	        _epService.EPRuntime.SendEvent(new Node("2"));
	        _epService.EPRuntime.SendEvent(new NodeData("1", "xxx"));
	    }

        [Test]
	    public void TestChainedUnparameterized()
        {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBeanComplexProps", typeof(SupportBeanComplexProps));

	        var epl = "select " +
	                "nested.GetNestedValue(), " +
	                "nested.GetNestedNested().GetNestedNestedValue() " +
	                "from SupportBeanComplexProps";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var bean = SupportBeanComplexProps.MakeDefaultBean();
	        var rows = new object[][] {
	                new object[]{"nested.GetNestedValue()", typeof(string)}
	                };
	        for (var i = 0; i < rows.Length; i++) {
	            var prop = stmt.EventType.PropertyDescriptors[i];
	            Assert.AreEqual(rows[i][0], prop.PropertyName);
	            Assert.AreEqual(rows[i][1], prop.PropertyType);
	        }

	        _epService.EPRuntime.SendEvent(bean);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), "nested.GetNestedValue()".Split(','), new object[]{bean.Nested.NestedValue});
	    }

        [Test]
	    public void TestChainedParameterized()
        {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportChainTop", typeof(SupportChainTop));

	        var subexpr = "top.GetChildOne(\"abc\",10).GetChildTwo(\"append\")";
	        var epl = "select " +
	                subexpr +
	                " from SupportChainTop as top";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertionChainedParam(stmt, subexpr);

	        _listener.Reset();
	        stmt.Dispose();
	        var model = _epService.EPAdministrator.CompileEPL(epl);
	        Assert.AreEqual(epl, model.ToEPL());
	        stmt = _epService.EPAdministrator.Create(model);
	        stmt.AddListener(_listener);

	        RunAssertionChainedParam(stmt, subexpr);
	    }

        [Test]
	    public void TestArrayPropertySizeAndGet()
        {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBeanComplexProps", typeof(SupportBeanComplexProps));

	        var epl = "select " +
	                "(ArrayProperty).size() as size, " +
                    "(ArrayProperty).get(0) as get0, " +
                    "(ArrayProperty).get(1) as get1, " +
                    "(ArrayProperty).get(2) as get2, " +
                    "(ArrayProperty).get(3) as get3 " +
	                "from SupportBeanComplexProps";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var bean = SupportBeanComplexProps.MakeDefaultBean();
	        var rows = new object[][] {
	                new object[]{"size", typeof(int)},
	                new object[]{"get0", typeof(int)},
	                new object[]{"get1", typeof(int)},
	                new object[]{"get2", typeof(int)},
	                new object[]{"get3", typeof(int)}
	                };
	        for (var i = 0; i < rows.Length; i++) {
	            var prop = stmt.EventType.PropertyDescriptors[i];
	            Assert.AreEqual(rows[i][0], prop.PropertyName, "failed for " + rows[i][0]);
	            Assert.AreEqual(rows[i][1], prop.PropertyType, "failed for " + rows[i][0]);
	        }

	        _epService.EPRuntime.SendEvent(bean);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), "size,get0,get1,get2,get3".Split(','),
	                new object[]{bean.ArrayProperty.Length, bean.ArrayProperty[0], bean.ArrayProperty[1], bean.ArrayProperty[2], null});
	    }

        [Test]
	    public void TestArrayPropertySizeAndGetChained()
        {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBeanCombinedProps", typeof(SupportBeanCombinedProps));

	        var epl = "select " +
	                "(abc).GetArray().size() as size, " +
	                "(abc).GetArray().get(0).GetNestLevOneVal() as get0 " +
	                "from SupportBeanCombinedProps as abc";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var bean = SupportBeanCombinedProps.MakeDefaultBean();
	        var rows = new object[][] {
	                new object[] {"size", typeof(int)},
	                new object[] {"get0", typeof(string)},
	        };
	        for (var i = 0; i < rows.Length; i++) {
	            var prop = stmt.EventType.PropertyDescriptors[i];
	            Assert.AreEqual(rows[i][0], prop.PropertyName);
	            Assert.AreEqual(rows[i][1], prop.PropertyType);
	        }

	        _epService.EPRuntime.SendEvent(bean);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), "size,get0".Split(','),
	                new object[]{bean.Array.Length, bean.Array[0].NestLevOneVal});
	    }

	    private void RunAssertionChainedParam(EPStatement stmt, string subexpr)
        {
	        var rows = new object[][] {
	                new object[] {subexpr, typeof(SupportChainChildTwo)}
            };
	        for (var i = 0; i < rows.Length; i++) {
	            var prop = stmt.EventType.PropertyDescriptors[i];
	            Assert.AreEqual(rows[i][0], prop.PropertyName);
	            Assert.AreEqual(rows[i][1], prop.PropertyType);
	        }

	        _epService.EPRuntime.SendEvent(new SupportChainTop());
	        var result = _listener.AssertOneGetNewAndReset().Get(subexpr);
	        Assert.AreEqual("abcappend", ((SupportChainChildTwo)result).GetText());
	    }

	    private void TryInvalid(string epl, string message)
	    {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

        public class LevelZero
        {
            private readonly LevelOne _levelOne;

            public LevelZero(LevelOne levelOne)
            {
                this._levelOne = levelOne;
            }

            public LevelOne GetLevelOne()
            {
                return _levelOne;
            }
        }

        public class LevelOne
        {
            private readonly LevelTwo _levelTwo;

            public LevelOne(LevelTwo levelTwo)
            {
                this._levelTwo = levelTwo;
            }

            public LevelTwo LevelTwo
            {
                get { return _levelTwo; }
            }

            public string GetCustomLevelOne(int val)
            {
                return "level1:" + val;
            }
        }

        public class LevelTwo
        {
            private readonly LevelThree _levelThree;

            public LevelTwo(LevelThree levelThree)
            {
                this._levelThree = levelThree;
            }

            public LevelThree LevelThree
            {
                get { return _levelThree; }
            }

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
	        public MyTypeErasure(string key, int subkey, IDictionary<string, InnerType> innerTypes, InnerType[] innerTypesArray)
            {
	            Key = key;
	            Subkey = subkey;
	            InnerTypes = innerTypes;
	            InnerTypesArray = innerTypesArray;
	        }

	        public IDictionary<string, InnerType> InnerTypes { get; private set; }

	        public string Key { get; private set; }

	        public int Subkey { get; private set; }

	        public InnerType[] InnerTypesArray { get; private set; }
	    }

        public class InnerType
        {
            public InnerType(int[] ids)
            {
                Ids = ids;
            }

            public int[] Ids { get; private set; }

            public int GetIds(int subkey)
            {
                return Ids[subkey];
            }

            public int GetIds(EventBean @event, string name)
            {
                return 999999;
            }
        }

        public class Node
        {
            public Node(string id)
            {
                Id = id;
            }

            public string Id { get; set; }

            public string Compute(object data)
            {
                if (data == null)
                {
                    return null;
                }
                var nodeData = (NodeData) ((EventBean) data).Underlying;
                return Id + nodeData.Value;
            }
        }

        public class NodeData
        {
            public NodeData(string nodeId, string value)
            {
                NodeId = nodeId;
                Value = value;
            }

            public string NodeId { get; set; }

            public string Value { get; set; }
        }
	}
} // end of namespace
