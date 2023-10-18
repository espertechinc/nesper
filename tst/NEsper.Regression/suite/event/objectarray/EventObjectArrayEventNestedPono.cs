///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using static com.espertech.esper.regressionlib.suite.@event.map.EventMapCore; // makeMap
using static com.espertech.esper.regressionlib.suite.@event.objectarray.EventObjectArrayCore; // getNestedKeyOA
using NUnit.Framework; // assertEquals

// assertNull

namespace com.espertech.esper.regressionlib.suite.@event.objectarray
{
	public class EventObjectArrayEventNestedPono : RegressionExecution
	{

		public void Run(RegressionEnvironment env)
		{
			var statementText = "@name('s0') select " +
			                    "simple, object, nodefmap, map, " +
			                    "object.id as a1, nodefmap.key1? as a2, nodefmap.key2? as a3, nodefmap.key3?.key4 as a4, " +
			                    "map.objectOne as b1, map.simpleOne as b2, map.nodefmapOne.key2? as b3, map.mapOne.simpleTwo? as b4, " +
			                    "map.objectOne.indexed[1] as c1, map.objectOne.nested.nestedValue as c2," +
			                    "map.mapOne.simpleTwo as d1, map.mapOne.objectTwo as d2, map.mapOne.nodefmapTwo as d3, " +
			                    "map.mapOne.mapTwo as e1, map.mapOne.mapTwo.simpleThree as e2, map.mapOne.mapTwo.objectThree as e3, " +
			                    "map.mapOne.objectTwo.array[1].mapped('1ma').value as f1, map.mapOne.mapTwo.objectThree.id as f2" +
			                    " from NestedObjectArr";
			env.CompileDeploy(statementText).AddListener("s0");

			var testdata = GetTestData();
			env.SendEventObjectArray(testdata, "NestedObjectArr");

			// test all properties exist
			env.AssertEventNew(
				"s0",
				received => {
					EPAssertionUtil.AssertProps(
						received,
						"simple,object,nodefmap,map".SplitCsv(),
						new object[] { "abc", new SupportBean_A("A1"), testdata[2], testdata[3] });
					EPAssertionUtil.AssertProps(
						received,
						"a1,a2,a3,a4".SplitCsv(),
						new object[] { "A1", "val1", null, null });
					EPAssertionUtil.AssertProps(
						received,
						"b1,b2,b3,b4".SplitCsv(),
						new object[] { GetNestedKeyOA(testdata, 3, "objectOne"), 10, "val2", 300 });
					EPAssertionUtil.AssertProps(received, "c1,c2".SplitCsv(), new object[] { 2, "nestedValue" });
					EPAssertionUtil.AssertProps(
						received,
						"d1,d2,d3".SplitCsv(),
						new object[] {
							300, GetNestedKeyOA(testdata, 3, "mapOne", "objectTwo"),
							GetNestedKeyOA(testdata, 3, "mapOne", "nodefmapTwo")
						});
					EPAssertionUtil.AssertProps(
						received,
						"e1,e2,e3".SplitCsv(),
						new object[]
							{ GetNestedKeyOA(testdata, 3, "mapOne", "mapTwo"), 4000L, new SupportBean_B("B1") });
					EPAssertionUtil.AssertProps(
						received,
						"f1,f2".SplitCsv(),
						new object[] { "1ma0", "B1" });
				});
			env.UndeployModuleContaining("s0");

			// assert type info
			env.CompileDeploy("@name('s0') select * from NestedObjectArr").AddListener("s0");
			env.AssertStatement(
				"s0",
				statement => {
					var eventType = statement.EventType;

					var propertiesReceived = eventType.PropertyNames;
					var propertiesExpected = new string[] { "simple", "object", "nodefmap", "map" };
					EPAssertionUtil.AssertEqualsAnyOrder(propertiesReceived, propertiesExpected);
					Assert.AreEqual(typeof(string), eventType.GetPropertyType("simple"));
					Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("map"));
					Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("nodefmap"));
					Assert.AreEqual(typeof(SupportBean_A), eventType.GetPropertyType("object"));

					Assert.IsNull(eventType.GetPropertyType("map.mapOne.simpleOne"));
				});

			// nested PONO with generic return type
			env.UndeployModuleContaining("s0");
			env.CompileDeploy("@name('s0') select * from MyNested(bean.insides.anyOf(i=>id = 'A'))").AddListener("s0");

			env.SendEventObjectArray(
				new object[] {
					new MyNested(Arrays.AsList(new MyInside[] { new MyInside("A") }))
				},
				"MyNested");
			env.AssertListenerInvoked("s0");

			env.UndeployAll();
		}

		private object[] GetTestData()
		{
			var levelThree = MakeMap(
				new object[][] {
					new object[] { "simpleThree", 4000L },
					new object[] { "objectThree", new SupportBean_B("B1") },
				});

			var levelTwo = MakeMap(
				new object[][] {
					new object[] { "simpleTwo", 300 },
					new object[] { "objectTwo", SupportBeanCombinedProps.MakeDefaultBean() },
					new object[] { "nodefmapTwo", MakeMap(new object[][] { new object[] { "key3", "val3" } }) },
					new object[] { "mapTwo", levelThree },
				});

			var levelOne = MakeMap(
				new object[][] {
					new object[] { "simpleOne", 10 },
					new object[] { "objectOne", SupportBeanComplexProps.MakeDefaultBean() },
					new object[] { "nodefmapOne", MakeMap(new object[][] { new object[] { "key2", "val2" } }) },
					new object[] { "mapOne", levelTwo }
				});

			object[] levelZero = {
				"abc", new SupportBean_A("A1"), MakeMap(new object[][] { new object[] { "key1", "val1" } }), levelOne
			};
			return levelZero;
		}

		[Serializable]
		public class MyNested
		{
			private readonly IList<MyInside> insides;

			public MyNested(IList<MyInside> insides)
			{
				this.insides = insides;
			}

			public IList<MyInside> Insides => insides;
		}

		[Serializable]
		public class MyInside
		{
			private readonly string id;

			public MyInside(string id)
			{
				this.id = id;
			}

			public string Id => id;
		}
	}
} // end of namespace
