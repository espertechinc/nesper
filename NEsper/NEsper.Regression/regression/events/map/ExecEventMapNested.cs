///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    using Map = IDictionary<string, object>;

    public class ExecEventMapNested : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("NestedMap", GetTestDef());
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInsertInto(epService);
            RunAssertionEventType(epService);
            RunAssertionNestedPojo(epService);
            RunAssertionIsExists(epService);
        }
    
        private void RunAssertionInsertInto(EPServiceProvider epService) {
            string statementText = "insert into MyStream select " +
                    "map.mapOne as val1" +
                    " from NestedMap#length(5)";
            epService.EPAdministrator.CreateEPL(statementText);
    
            statementText = "select val1 as a from MyStream";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            IDictionary<string, Object> testdata = GetTestData();
            epService.EPRuntime.SendEvent(testdata, "NestedMap");
    
            // test all properties exist
            string[] fields = "a".Split(',');
            EventBean received = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[]{ExecEventMap.GetNestedKeyMap(testdata, "map", "mapOne")});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventType(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from NestedMap");
            EventType eventType = stmt.EventType;
    
            string[] propertiesReceived = eventType.PropertyNames;
            var propertiesExpected = new string[]{"simple", "object", "nodefmap", "map"};
            EPAssertionUtil.AssertEqualsAnyOrder(propertiesReceived, propertiesExpected);
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("simple"));
            Assert.AreEqual(typeof(Map), eventType.GetPropertyType("map"));
            Assert.AreEqual(typeof(Map), eventType.GetPropertyType("nodefmap"));
            Assert.AreEqual(typeof(SupportBean_A), eventType.GetPropertyType("object"));
    
            Assert.IsNull(eventType.GetPropertyType("map.mapOne.simpleOne"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNestedPojo(EPServiceProvider epService) {
            string statementText = "select " +
                    "simple, object, nodefmap, map, " +
                    "object.id as a1, nodefmap.key1? as a2, nodefmap.key2? as a3, nodefmap.key3?.key4 as a4, " +
                    "map.objectOne as b1, map.simpleOne as b2, map.nodefmapOne.key2? as b3, map.mapOne.simpleTwo? as b4, " +
                    "map.objectOne.indexed[1] as c1, map.objectOne.nested.nestedValue as c2," +
                    "map.mapOne.simpleTwo as d1, map.mapOne.objectTwo as d2, map.mapOne.nodefmapTwo as d3, " +
                    "map.mapOne.mapTwo as e1, map.mapOne.mapTwo.simpleThree as e2, map.mapOne.mapTwo.objectThree as e3, " +
                    "map.mapOne.objectTwo.array[1].Mapped('1ma').value as f1, map.mapOne.mapTwo.objectThree.id as f2" +
                    " from NestedMap#length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            IDictionary<string, Object> testdata = GetTestData();
            epService.EPRuntime.SendEvent(testdata, "NestedMap");
    
            // test all properties exist
            EventBean received = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, "simple,object,nodefmap,map".Split(','),
                    new object[]{"abc", new SupportBean_A("A1"), testdata.Get("nodefmap"), testdata.Get("map")});
            EPAssertionUtil.AssertProps(received, "a1,a2,a3,a4".Split(','),
                    new object[]{"A1", "val1", null, null});
            EPAssertionUtil.AssertProps(received, "b1,b2,b3,b4".Split(','),
                    new object[]{ExecEventMap.GetNestedKeyMap(testdata, "map", "objectOne"), 10, "val2", 300});
            EPAssertionUtil.AssertProps(received, "c1,c2".Split(','), new object[]{2, "NestedValue"});
            EPAssertionUtil.AssertProps(received, "d1,d2,d3".Split(','),
                    new object[]{300, ExecEventMap.GetNestedKeyMap(testdata, "map", "mapOne", "objectTwo"), ExecEventMap.GetNestedKeyMap(testdata, "map", "mapOne", "nodefmapTwo")});
            EPAssertionUtil.AssertProps(received, "e1,e2,e3".Split(','),
                    new object[]{ExecEventMap.GetNestedKeyMap(testdata, "map", "mapOne", "mapTwo"), 4000L, new SupportBean_B("B1")});
            EPAssertionUtil.AssertProps(received, "f1,f2".Split(','),
                    new object[]{"1ma0", "B1"});
    
            // test partial properties exist
            testdata = GetTestDataThree();
            epService.EPRuntime.SendEvent(testdata, "NestedMap");
    
            received = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, "simple,object,nodefmap,map".Split(','),
                    new object[]{"abc", new SupportBean_A("A1"), testdata.Get("nodefmap"), testdata.Get("map")});
            EPAssertionUtil.AssertProps(received, "a1,a2,a3,a4".Split(','),
                    new object[]{"A1", "val1", null, null});
            EPAssertionUtil.AssertProps(received, "b1,b2,b3,b4".Split(','),
                    new object[]{ExecEventMap.GetNestedKeyMap(testdata, "map", "objectOne"), null, null, null});
            EPAssertionUtil.AssertProps(received, "c1,c2".Split(','), new object[]{null, null});
            EPAssertionUtil.AssertProps(received, "d1,d2,d3".Split(','),
                    new object[]{null, ExecEventMap.GetNestedKeyMap(testdata, "map", "mapOne", "objectTwo"), ExecEventMap.GetNestedKeyMap(testdata, "map", "mapOne", "nodefmapTwo")});
            EPAssertionUtil.AssertProps(received, "e1,e2,e3".Split(','),
                    new object[]{ExecEventMap.GetNestedKeyMap(testdata, "map", "mapOne", "mapTwo"), 4000L, null});
            EPAssertionUtil.AssertProps(received, "f1,f2".Split(','),
                    new object[]{"1ma0", null});
        }
    
        private void RunAssertionIsExists(EPServiceProvider epService) {
            string statementText = "select " +
                    "exists(map.mapOne?) as a," +
                    "exists(map.mapOne?.simpleOne) as b," +
                    "exists(map.mapOne?.simpleTwo) as c," +
                    "exists(map.mapOne?.mapTwo) as d," +
                    "exists(map.mapOne.mapTwo?) as e," +
                    "exists(map.mapOne.mapTwo.simpleThree?) as f," +
                    "exists(map.mapOne.mapTwo.objectThree?) as g " +
                    " from NestedMap#length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            IDictionary<string, Object> testdata = GetTestData();
            epService.EPRuntime.SendEvent(testdata, "NestedMap");
    
            // test all properties exist
            string[] fields = "a,b,c,d,e,f,g".Split(',');
            EventBean received = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields,
                    new object[]{true, false, true, true, true, true, true});
    
            // test partial properties exist
            testdata = GetTestDataThree();
            epService.EPRuntime.SendEvent(testdata, "NestedMap");
    
            received = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields,
                    new object[]{true, false, false, true, true, true, false});
        }
    
        private IDictionary<string, Object> GetTestDef() {
            IDictionary<string, Object> levelThree = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simpleThree", typeof(long)},
                    new object[] {"objectThree", typeof(SupportBean_B)},
            });
    
            IDictionary<string, Object> levelTwo = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simpleTwo", typeof(int?)},
                    new object[] {"objectTwo", typeof(SupportBeanCombinedProps)},
                    new object[] {"nodefmapTwo", typeof(Map)},
                    new object[] {"mapTwo", levelThree},
            });
    
            IDictionary<string, Object> levelOne = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simpleOne", typeof(int?)},
                    new object[] {"objectOne", typeof(SupportBeanComplexProps)},
                    new object[] {"nodefmapOne", typeof(Map)},
                    new object[] {"mapOne", levelTwo}
            });
    
            IDictionary<string, Object> levelZero = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simple", typeof(string)},
                    new object[] {"object", typeof(SupportBean_A)},
                    new object[] {"nodefmap", typeof(Map)},
                    new object[] {"map", levelOne}
            });
    
            return levelZero;
        }
    
        private IDictionary<string, Object> GetTestData() {
            IDictionary<string, Object> levelThree = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simpleThree", 4000L},
                    new object[] {"objectThree", new SupportBean_B("B1")},
            });
    
            IDictionary<string, Object> levelTwo = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simpleTwo", 300},
                    new object[] {"objectTwo", SupportBeanCombinedProps.MakeDefaultBean()},
                    new object[] {"nodefmapTwo", ExecEventMap.MakeMap(new object[][]{new object[] {"key3", "val3"}})},
                    new object[] {"mapTwo", levelThree},
            });
    
            IDictionary<string, Object> levelOne = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simpleOne", 10},
                    new object[] {"objectOne", SupportBeanComplexProps.MakeDefaultBean()},
                    new object[] {"nodefmapOne", ExecEventMap.MakeMap(new object[][]{new object[] {"key2", "val2"}})},
                    new object[] {"mapOne", levelTwo}
            });
    
            IDictionary<string, Object> levelZero = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simple", "abc"},
                    new object[] {"object", new SupportBean_A("A1")},
                    new object[] {"nodefmap", ExecEventMap.MakeMap(new object[][]{new object[] {"key1", "val1"}})},
                    new object[] {"map", levelOne}
            });
    
            return levelZero;
        }
    
        private IDictionary<string, Object> GetTestDataThree() {
            IDictionary<string, Object> levelThree = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simpleThree", 4000L},
            });
    
            IDictionary<string, Object> levelTwo = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"objectTwo", SupportBeanCombinedProps.MakeDefaultBean()},
                    new object[] {"nodefmapTwo", ExecEventMap.MakeMap(new object[][]{new object[] {"key3", "val3"}})},
                    new object[] {"mapTwo", levelThree},
            });
    
            IDictionary<string, Object> levelOne = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simpleOne", null},
                    new object[] {"objectOne", null},
                    new object[] {"mapOne", levelTwo}
            });
    
            IDictionary<string, Object> levelZero = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"simple", "abc"},
                    new object[] {"object", new SupportBean_A("A1")},
                    new object[] {"nodefmap", ExecEventMap.MakeMap(new object[][]{new object[] {"key1", "val1"}})},
                    new object[] {"map", levelOne}
            });
    
            return levelZero;
        }
    }
} // end of namespace
