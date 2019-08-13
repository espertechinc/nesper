///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapNested
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EventMapNestedInsertInto());
            execs.Add(new EventMapNestedEventType());
            execs.Add(new EventMapNestedNestedPono());
            execs.Add(new EventMapNestedIsExists());
            return execs;
        }

        private static IDictionary<string, object> GetTestData()
        {
            var levelThree = EventMapCore.MakeMap(
                new[] {
                    new object[] {"simpleThree", 4000L},
                    new object[] {"objectThree", new SupportBean_B("B1")}
                });

            var levelTwo = EventMapCore.MakeMap(
                new[] {
                    new object[] {"simpleTwo", 300},
                    new object[] {"objectTwo", SupportBeanCombinedProps.MakeDefaultBean()},
                    new object[] {
                        "nodefmapTwo", EventMapCore.MakeMap(
                            new[] {
                                new object[] {"key3", "val3"}
                            })
                    },
                    new object[] {"mapTwo", levelThree}
                });

            var levelOne = EventMapCore.MakeMap(
                new[] {
                    new object[] {"simpleOne", 10},
                    new object[] {"objectOne", SupportBeanComplexProps.MakeDefaultBean()},
                    new object[] {
                        "nodefmapOne", EventMapCore.MakeMap(
                            new[] {
                                new object[] {"key2", "val2"}
                            })
                    },
                    new object[] {"mapOne", levelTwo}
                });

            var levelZero = EventMapCore.MakeMap(
                new[] {
                    new object[] {"simple", "abc"},
                    new object[] {"object", new SupportBean_A("A1")},
                    new object[] {
                        "nodefmap", EventMapCore.MakeMap(
                            new[] {
                                new object[] {"key1", "val1"}
                            })
                    },
                    new object[] {"map", levelOne}
                });

            return levelZero;
        }

        private static IDictionary<string, object> GetTestDataThree()
        {
            var levelThree = EventMapCore.MakeMap(
                new[] {
                    new object[] {"simpleThree", 4000L}
                });

            var levelTwo = EventMapCore.MakeMap(
                new[] {
                    new object[] {"objectTwo", SupportBeanCombinedProps.MakeDefaultBean()},
                    new object[] {
                        "nodefmapTwo", EventMapCore.MakeMap(
                            new[] {
                                new object[] {"key3", "val3"}
                            })
                    },
                    new object[] {"mapTwo", levelThree}
                });

            var levelOne = EventMapCore.MakeMap(
                new[] {
                    new object[] {"simpleOne", null},
                    new object[] {"objectOne", null},
                    new object[] {"mapOne", levelTwo}
                });

            var levelZero = EventMapCore.MakeMap(
                new[] {
                    new object[] {"simple", "abc"},
                    new object[] {"object", new SupportBean_A("A1")},
                    new object[] {
                        "nodefmap", EventMapCore.MakeMap(
                            new[] {
                                new object[] {"key1", "val1"}
                            })
                    },
                    new object[] {"map", levelOne}
                });

            return levelZero;
        }

        internal class EventMapNestedInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var statementText = "insert into MyStream select map.mapOne as val1 from NestedMap#length(5)";
                env.CompileDeploy(statementText, path);

                statementText = "@Name('s0') select val1 as a from MyStream";
                env.CompileDeploy(statementText, path).AddListener("s0");

                var testdata = GetTestData();
                env.SendEventMap(testdata, "NestedMap");

                // test all properties exist
                var fields = "a".SplitCsv();
                var received = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new[] {EventMapCore.GetNestedKeyMap(testdata, "map", "mapOne")});

                env.UndeployAll();
            }
        }

        internal class EventMapNestedEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from NestedMap");
                var eventType = env.Statement("s0").EventType;

                var propertiesReceived = eventType.PropertyNames;
                string[] propertiesExpected = {"simple", "object", "nodefmap", "map"};
                EPAssertionUtil.AssertEqualsAnyOrder(propertiesReceived, propertiesExpected);
                Assert.AreEqual(typeof(string), eventType.GetPropertyType("simple"));
                Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("map"));
                Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("nodefmap"));
                Assert.AreEqual(typeof(SupportBean_A), eventType.GetPropertyType("object"));

                Assert.IsNull(eventType.GetPropertyType("map.mapOne.simpleOne"));

                env.UndeployAll();
            }
        }

        internal class EventMapNestedNestedPono : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@Name('s0') select " +
                                    "simple, object, nodefmap, map, " +
                                    "object.Id as a1, nodefmap.Key1? as a2, nodefmap.Key2? as a3, nodefmap.Key3?.Key4 as a4, " +
                                    "map.objectOne as b1, map.simpleOne as b2, map.nodefmapOne.Key2? as b3, map.mapOne.simpleTwo? as b4, " +
                                    "map.objectOne.indexed[1] as c1, map.objectOne.Nested.NestedValue as c2," +
                                    "map.mapOne.simpleTwo as d1, map.mapOne.objectTwo as d2, map.mapOne.nodefmapTwo as d3, " +
                                    "map.mapOne.mapTwo as e1, map.mapOne.mapTwo.simpleThree as e2, map.mapOne.mapTwo.objectThree as e3, " +
                                    "map.mapOne.objectTwo.array[1].mapped('1ma').Value as f1, map.mapOne.mapTwo.objectThree.Id as f2" +
                                    " from NestedMap#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                var testdata = GetTestData();
                env.SendEventMap(testdata, "NestedMap");

                // test all properties exist
                var received = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    "simple,object,nodefmap,map".SplitCsv(),
                    new[] {"abc", new SupportBean_A("A1"), testdata.Get("nodefmap"), testdata.Get("map")});
                EPAssertionUtil.AssertProps(
                    received,
                    "a1,a2,a3,a4".SplitCsv(),
                    new object[] {"A1", "val1", null, null});
                EPAssertionUtil.AssertProps(
                    received,
                    "b1,b2,b3,b4".SplitCsv(),
                    new[] {EventMapCore.GetNestedKeyMap(testdata, "map", "objectOne"), 10, "val2", 300});
                EPAssertionUtil.AssertProps(
                    received,
                    "c1,c2".SplitCsv(),
                    new object[] {2, "NestedValue"});
                EPAssertionUtil.AssertProps(
                    received,
                    "d1,d2,d3".SplitCsv(),
                    new[] {
                        300, EventMapCore.GetNestedKeyMap(testdata, "map", "mapOne", "objectTwo"),
                        EventMapCore.GetNestedKeyMap(testdata, "map", "mapOne", "nodefmapTwo")
                    });
                EPAssertionUtil.AssertProps(
                    received,
                    "e1,e2,e3".SplitCsv(),
                    new[] {
                        EventMapCore.GetNestedKeyMap(testdata, "map", "mapOne", "mapTwo"), 4000L,
                        new SupportBean_B("B1")
                    });
                EPAssertionUtil.AssertProps(
                    received,
                    "f1,f2".SplitCsv(),
                    new object[] {"1ma0", "B1"});

                // test partial properties exist
                testdata = GetTestDataThree();
                env.SendEventMap(testdata, "NestedMap");

                received = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    "simple,object,nodefmap,map".SplitCsv(),
                    new[] {"abc", new SupportBean_A("A1"), testdata.Get("nodefmap"), testdata.Get("map")});
                EPAssertionUtil.AssertProps(
                    received,
                    "a1,a2,a3,a4".SplitCsv(),
                    new object[] {"A1", "val1", null, null});
                EPAssertionUtil.AssertProps(
                    received,
                    "b1,b2,b3,b4".SplitCsv(),
                    new[] {EventMapCore.GetNestedKeyMap(testdata, "map", "objectOne"), null, null, null});
                EPAssertionUtil.AssertProps(
                    received,
                    "c1,c2".SplitCsv(),
                    new object[] {null, null});
                EPAssertionUtil.AssertProps(
                    received,
                    "d1,d2,d3".SplitCsv(),
                    new[] {
                        null, EventMapCore.GetNestedKeyMap(testdata, "map", "mapOne", "objectTwo"),
                        EventMapCore.GetNestedKeyMap(testdata, "map", "mapOne", "nodefmapTwo")
                    });
                EPAssertionUtil.AssertProps(
                    received,
                    "e1,e2,e3".SplitCsv(),
                    new[] {EventMapCore.GetNestedKeyMap(testdata, "map", "mapOne", "mapTwo"), 4000L, null});
                EPAssertionUtil.AssertProps(
                    received,
                    "f1,f2".SplitCsv(),
                    new object[] {"1ma0", null});

                env.UndeployAll();
            }
        }

        internal class EventMapNestedIsExists : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@Name('s0') select " +
                                    "exists(map.mapOne?) as a," +
                                    "exists(map.mapOne?.simpleOne) as b," +
                                    "exists(map.mapOne?.simpleTwo) as c," +
                                    "exists(map.mapOne?.mapTwo) as d," +
                                    "exists(map.mapOne.mapTwo?) as e," +
                                    "exists(map.mapOne.mapTwo.simpleThree?) as f," +
                                    "exists(map.mapOne.mapTwo.objectThree?) as g " +
                                    " from NestedMap#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                var testdata = GetTestData();
                env.SendEventMap(testdata, "NestedMap");

                // test all properties exist
                var fields = "a,b,c,d,e,f,g".SplitCsv();
                var received = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new object[] {true, false, true, true, true, true, true});

                // test partial properties exist
                testdata = GetTestDataThree();
                env.SendEventMap(testdata, "NestedMap");

                received = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new object[] {true, false, false, true, true, true, false});

                env.UndeployAll();
            }
        }
    }
} // end of namespace