///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

using static com.espertech.esper.regression.events.map.ExecEventMap;
using static com.espertech.esper.regression.events.objectarray.ExecEventObjectArray;

namespace com.espertech.esper.regression.events.objectarray
{
    using Map = IDictionary<string, object>;

    public class ExecEventObjectArrayEventNestedPono : RegressionExecution {
        public override void Configure(Configuration configuration) {
            Pair<string[], object[]> pair = GetTestDef();
            configuration.AddEventType("NestedObjectArr", pair.First, pair.Second);
        }
    
        public override void Run(EPServiceProvider epService) {
            string statementText = "select " +
                    "simple, object, nodefmap, map, " +
                    "object.id as a1, nodefmap.key1? as a2, nodefmap.key2? as a3, nodefmap.key3?.key4 as a4, " +
                    "map.objectOne as b1, map.simpleOne as b2, map.nodefmapOne.key2? as b3, map.mapOne.simpleTwo? as b4, " +
                    "map.objectOne.indexed[1] as c1, map.objectOne.nested.nestedValue as c2," +
                    "map.mapOne.simpleTwo as d1, map.mapOne.objectTwo as d2, map.mapOne.nodefmapTwo as d3, " +
                    "map.mapOne.mapTwo as e1, map.mapOne.mapTwo.simpleThree as e2, map.mapOne.mapTwo.objectThree as e3, " +
                    "map.mapOne.objectTwo.array[1].Mapped('1ma').value as f1, map.mapOne.mapTwo.objectThree.id as f2" +
                    " from NestedObjectArr";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            object[] testdata = GetTestData();
            epService.EPRuntime.SendEvent(testdata, "NestedObjectArr");
    
            // test all properties exist
            EventBean received = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, "simple,object,nodefmap,map".Split(','),
                    new[]{"abc", new SupportBean_A("A1"), testdata[2], testdata[3]});
            EPAssertionUtil.AssertProps(received, "a1,a2,a3,a4".Split(','),
                    new object[]{"A1", "val1", null, null});
            EPAssertionUtil.AssertProps(received, "b1,b2,b3,b4".Split(','),
                    new object[]{ GetNestedKeyOA(testdata, 3, "objectOne"), 10, "val2", 300 });
            EPAssertionUtil.AssertProps(received, "c1,c2".Split(','), new object[]{2, "NestedValue"});
            EPAssertionUtil.AssertProps(received, "d1,d2,d3".Split(','),
                    new object[]{300, GetNestedKeyOA(testdata, 3, "mapOne", "objectTwo"), GetNestedKeyOA(testdata, 3, "mapOne", "nodefmapTwo")});
            EPAssertionUtil.AssertProps(received, "e1,e2,e3".Split(','),
                    new object[]{GetNestedKeyOA(testdata, 3, "mapOne", "mapTwo"), 4000L, new SupportBean_B("B1")});
            EPAssertionUtil.AssertProps(received, "f1,f2".Split(','),
                    new object[]{"1ma0", "B1"});
    
            // assert type info
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from NestedObjectArr");
            EventType eventType = stmt.EventType;
    
            string[] propertiesReceived = eventType.PropertyNames;
            var propertiesExpected = new[]{"simple", "object", "nodefmap", "map"};
            EPAssertionUtil.AssertEqualsAnyOrder(propertiesReceived, propertiesExpected);
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("simple"));
            Assert.AreEqual(typeof(Map), eventType.GetPropertyType("map"));
            Assert.AreEqual(typeof(Map), eventType.GetPropertyType("nodefmap"));
            Assert.AreEqual(typeof(SupportBean_A), eventType.GetPropertyType("object"));
    
            Assert.IsNull(eventType.GetPropertyType("map.mapOne.simpleOne"));
    
            // nested POJO with generic return type
            listener.Reset();
            epService.EPAdministrator.Configuration.AddEventType("MyNested", new[]{"bean"}, new object[]{typeof(MyNested)});
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select * from MyNested(bean.insides.anyOf(i=>id = 'A'))");
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new object[]{new MyNested(Collections.List(new MyInside("A")))}, "MyNested");
            Assert.IsTrue(listener.IsInvoked);
        }
    
        private object[] GetTestData() {
            IDictionary<string, object> levelThree = MakeMap(new[]
            {
                    new object[] {"simpleThree", 4000L},
                    new object[] {"objectThree", new SupportBean_B("B1")}
            });
    
            IDictionary<string, object> levelTwo = MakeMap(new[]
            {
                    new object[] {"simpleTwo", 300},
                    new object[] {"objectTwo", SupportBeanCombinedProps.MakeDefaultBean()},
                    new object[] {"nodefmapTwo", MakeMap(new[] {new object[] {"key3", "val3"}})},
                    new object[] {"mapTwo", levelThree}
            });
    
            IDictionary<string, object> levelOne = MakeMap(new[]
            {
                    new object[] {"simpleOne", 10},
                    new object[] {"objectOne", SupportBeanComplexProps.MakeDefaultBean()},
                    new object[] {"nodefmapOne", MakeMap(new[] {new object[] {"key2", "val2"}})},
                    new object[] {"mapOne", levelTwo}
            });
    
            object[] levelZero = {"abc", new SupportBean_A("A1"), MakeMap(new[] {new object[] {"key1", "val1"}}), levelOne};
            return levelZero;
        }
    
        private Pair<string[], object[]> GetTestDef() {
            IDictionary<string, object> levelThree = MakeMap(new[]
            {
                    new object[] {"simpleThree", typeof(long)},
                    new object[] {"objectThree", typeof(SupportBean_B)}
            });
    
            IDictionary<string, object> levelTwo = MakeMap(new[]
            {
                    new object[] {"simpleTwo", typeof(int?)},
                    new object[] {"objectTwo", typeof(SupportBeanCombinedProps)},
                    new object[] {"nodefmapTwo", typeof(Map)},
                    new object[] {"mapTwo", levelThree}
            });
    
            IDictionary<string, object> levelOne = MakeMap(new[]
            {
                    new object[] {"simpleOne", typeof(int?)},
                    new object[] {"objectOne", typeof(SupportBeanComplexProps)},
                    new object[] {"nodefmapOne", typeof(Map)},
                    new object[] {"mapOne", levelTwo}
            });
    
            string[] levelZeroProps = {"simple", "object", "nodefmap", "map"};
            object[] levelZeroTypes = {typeof(string), typeof(SupportBean_A), typeof(Map), levelOne};
            return new Pair<string[], object[]>(levelZeroProps, levelZeroTypes);
        }
    
        public class MyNested {
            public IList<MyInside> Insides { get; }
            internal MyNested(IList<MyInside> insides) {
                Insides = insides;
            }

        }
    
        public class MyInside {
            public string Id { get; }
            internal MyInside(string id) {
                Id = id;
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
