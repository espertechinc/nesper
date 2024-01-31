///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using static com.espertech.esper.regressionlib.suite.@event.map.EventMapCore;
using static com.espertech.esper.regressionlib.suite.@event.objectarray.EventObjectArrayCore;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.objectarray
{
    public class EventObjectArrayEventNestedPono : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var statementText = "@name('s0') select " +
                                "Simple, Object, Nodefmap, Map, " +
                                "Object.Id as a1, Nodefmap.key1? as a2, Nodefmap.key2? as a3, Nodefmap.key3?.key4 as a4, " +
                                "Map.ObjectOne as b1, Map.SimpleOne as b2, Map.NodefmapOne.key2? as b3, Map.MapOne.SimpleTwo? as b4, " +
                                "Map.ObjectOne.Indexed[1] as c1, Map.ObjectOne.Nested.NestedValue as c2," +
                                "Map.MapOne.SimpleTwo as d1, Map.MapOne.ObjectTwo as d2, Map.MapOne.NodefmapTwo as d3, " +
                                "Map.MapOne.MapTwo as e1, Map.MapOne.MapTwo.SimpleThree as e2, Map.MapOne.MapTwo.ObjectThree as e3, " +
                                "Map.MapOne.ObjectTwo.Array[1].Mapped('1ma').Value as f1, Map.MapOne.MapTwo.ObjectThree.Id as f2" +
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
                        "Simple,Object,Nodefmap,Map".SplitCsv(),
                        new object[] { "abc", new SupportBean_A("A1"), testdata[2], testdata[3] });
                    EPAssertionUtil.AssertProps(
                        received,
                        "a1,a2,a3,a4".SplitCsv(),
                        new object[] { "A1", "val1", null, null });
                    EPAssertionUtil.AssertProps(
                        received,
                        "b1,b2,b3,b4".SplitCsv(),
                        new object[] { GetNestedKeyOA(testdata, 3, "ObjectOne"), 10, "val2", 300 });
                    EPAssertionUtil.AssertProps(received, "c1,c2".SplitCsv(), new object[] { 2, "NestedValue" });
                    EPAssertionUtil.AssertProps(
                        received,
                        "d1,d2,d3".SplitCsv(),
                        new object[] {
                            300, GetNestedKeyOA(testdata, 3, "MapOne", "ObjectTwo"),
                            GetNestedKeyOA(testdata, 3, "MapOne", "NodefmapTwo")
                        });
                    EPAssertionUtil.AssertProps(
                        received,
                        "e1,e2,e3".SplitCsv(),
                        new object[]
                            { GetNestedKeyOA(testdata, 3, "MapOne", "MapTwo"), 4000L, new SupportBean_B("B1") });
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
                    var propertiesExpected = new string[] { "Simple", "Object", "Nodefmap", "Map" };
                    EPAssertionUtil.AssertEqualsAnyOrder(propertiesReceived, propertiesExpected);
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("Simple"));
                    ClassicAssert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("Map"));
                    ClassicAssert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("Nodefmap"));
                    ClassicAssert.AreEqual(typeof(SupportBean_A), eventType.GetPropertyType("Object"));

                    ClassicAssert.IsNull(eventType.GetPropertyType("Map.MapOne.SimpleOne"));
                });

            // nested PONO with generic return type
            env.UndeployModuleContaining("s0");
            env.CompileDeploy("@name('s0') select * from MyNested(bean.Insides.anyOf(i=>Id = 'A'))").AddListener("s0");

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
                    new object[] { "SimpleThree", 4000L },
                    new object[] { "ObjectThree", new SupportBean_B("B1") },
                });

            var levelTwo = MakeMap(
                new object[][] {
                    new object[] { "SimpleTwo", 300 },
                    new object[] { "ObjectTwo", SupportBeanCombinedProps.MakeDefaultBean() },
                    new object[] { "NodefmapTwo", MakeMap(new object[][] { new object[] { "key3", "val3" } }) },
                    new object[] { "MapTwo", levelThree },
                });

            var levelOne = MakeMap(
                new object[][] {
                    new object[] { "SimpleOne", 10 },
                    new object[] { "ObjectOne", SupportBeanComplexProps.MakeDefaultBean() },
                    new object[] { "NodefmapOne", MakeMap(new object[][] { new object[] { "key2", "val2" } }) },
                    new object[] { "MapOne", levelTwo }
                });

            object[] levelZero = {
                "abc", new SupportBean_A("A1"), MakeMap(new object[][] { new object[] { "key1", "val1" } }), levelOne
            };
            return levelZero;
        }

        public class MyNested
        {
            private readonly IList<MyInside> insides;

            public MyNested(IList<MyInside> insides)
            {
                this.insides = insides;
            }

            public IList<MyInside> Insides => insides;
        }

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