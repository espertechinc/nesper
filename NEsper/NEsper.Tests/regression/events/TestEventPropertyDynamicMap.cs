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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestEventPropertyDynamicMap 
    {
        private SupportUpdateListener _listener;
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestMapWithinMap()
        {
            Properties properties = new Properties();
            properties["InnerMap"] = typeof(IDictionary<string, object>).FullName;
            _epService.EPAdministrator.Configuration.AddEventType("MyLevel2", properties);
    
            String statementText = "select " +
                                   "InnerMap.Int? as t0, " +
                                   "InnerMap.InnerTwo?.Nested as t1, " +
                                   "InnerMap.InnerTwo?.InnerThree.NestedTwo as t2, " +
                                   "DynamicOne? as t3, " +
                                   "DynamicTwo? as t4, " +
                                   "Indexed[1]? as t5, " +
                                   "Mapped('KeyOne')? as t6, " +
                                   "InnerMap.IndexedTwo[0]? as t7, " +
                                   "InnerMap.MappedTwo('KeyTwo')? as t8 " +
                        "from MyLevel2.win:length(5)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += _listener.Update;

            IDictionary<String, Object> map = new Dictionary<String, Object>();
            map["DynamicTwo"] = 20l;
            map.Put("InnerMap", MakeMap(
                    "Int", 10,
                    "IndexedTwo", new int[] {-10},
                    "MappedTwo", MakeMap("KeyTwo", "def"),
                    "InnerTwo", MakeMap("Nested", 30d, "InnerThree", MakeMap("NestedTwo", 99))));
            map["Indexed"] = new float[] {-1, -2, -3};
            map["Mapped"] = MakeMap("KeyOne", "abc");
            _epService.EPRuntime.SendEvent(map, "MyLevel2");
            AssertResults(_listener.AssertOneGetNewAndReset(), new Object[] {10, 30d, 99, null, 20l, -2.0f, "abc", -10, "def"});
    
            map = new Dictionary<String, Object>();
            map.Put("InnerMap", MakeMap(
                    "IndexedTwo", new int[] {},
                    "MappedTwo", MakeMap("yyy", "xxx"),
                    "InnerTwo", null));
            map["Indexed"] = new float[] {};
            map["Mapped"] = MakeMap("xxx", "yyy");
            _epService.EPRuntime.SendEvent(map, "MyLevel2");
            AssertResults(_listener.AssertOneGetNewAndReset(), new Object[] {null, null, null, null, null, null, null, null, null});
    
            _epService.EPRuntime.SendEvent(new Dictionary<String, Object>(), "MyLevel2");
            AssertResults(_listener.AssertOneGetNewAndReset(), new Object[] {null, null, null, null, null, null, null, null, null});
    
            map = new Dictionary<String, Object>();
            map["InnerMap"] = "xxx";
            map["Indexed"] = null;
            map["Mapped"] = "xxx";
            _epService.EPRuntime.SendEvent(map, "MyLevel2");
            AssertResults(_listener.AssertOneGetNewAndReset(), new Object[] {null, null, null, null, null, null, null, null, null});
        }
    
        [Test]
        public void TestMapWithinMapExists()
        {
            Properties properties = new Properties();
            properties["InnerMap"] = typeof(IDictionary<string, object>).FullName;
            _epService.EPAdministrator.Configuration.AddEventType("MyLevel2", properties);
    
            String statementText = "select " +
                                   "exists(InnerMap.Int?) as t0, " +
                                   "exists(InnerMap.InnerTwo?.Nested) as t1, " +
                                   "exists(InnerMap.InnerTwo?.InnerThree.NestedTwo) as t2, " +
                                   "exists(DynamicOne?) as t3, " +
                                   "exists(DynamicTwo?) as t4, " +
                                   "exists(Indexed[1]?) as t5, " +
                                   "exists(Mapped('KeyOne')?) as t6, " +
                                   "exists(InnerMap.IndexedTwo[0]?) as t7, " +
                                   "exists(InnerMap.MappedTwo('KeyTwo')?) as t8 " +
                        "from MyLevel2.win:length(5)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += _listener.Update;

            IDictionary<string, object> map = new Dictionary<String, Object>();
            map["DynamicTwo"] = 20l;
            map.Put("InnerMap", MakeMap(
                    "Int", 10,
                    "IndexedTwo", new int[] {-10},
                    "MappedTwo", MakeMap("KeyTwo", "def"),
                    "InnerTwo", MakeMap("Nested", 30d, "InnerThree", MakeMap("NestedTwo", 99))));
            map["Indexed"] = new float[] {-1, -2, -3};
            map["Mapped"] = MakeMap("KeyOne", "abc");
            _epService.EPRuntime.SendEvent(map, "MyLevel2");
            AssertResults(_listener.AssertOneGetNewAndReset(), new Object[] {true, true,true,false,true,true,true,true,true});
    
            map = new Dictionary<String, Object>();
            map.Put("InnerMap", MakeMap(
                    "IndexedTwo", new int[] {},
                    "MappedTwo", MakeMap("yyy", "xxx"),
                    "InnerTwo", null));
            map["Indexed"] = new float[] {};
            map["Mapped"] = MakeMap("xxx", "yyy");
            _epService.EPRuntime.SendEvent(map, "MyLevel2");
            AssertResults(_listener.AssertOneGetNewAndReset(), new Object[] {false, false,false,false,false,false,false,false,false});
    
            _epService.EPRuntime.SendEvent(new Dictionary<String, Object>(), "MyLevel2");
            AssertResults(_listener.AssertOneGetNewAndReset(), new Object[] {false, false,false,false,false,false,false,false,false});
    
            map = new Dictionary<String, Object>();
            map["InnerMap"] = "xxx";
            map["Indexed"] = null;
            map["Mapped"] = "xxx";
            _epService.EPRuntime.SendEvent(map, "MyLevel2");
            AssertResults(_listener.AssertOneGetNewAndReset(), new Object[] {false, false,false,false,false,false,false,false,false});
        }
    
        [Test]
        public void TestMapWithinMap2LevelsInvalid()
        {
            Properties properties = new Properties();
            properties["InnerMap"] = typeof(IDictionary<string,object>).FullName;
            _epService.EPAdministrator.Configuration.AddEventType("MyLevel2", properties);
    
            String statementText = "select InnerMap.int as t0 from MyLevel2.win:length(5)";
            try
            {
                _epService.EPAdministrator.CreateEPL(statementText);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }
    
            statementText = "select InnerMap.int.inner2? as t0 from MyLevel2.win:length(5)";
            try
            {
                _epService.EPAdministrator.CreateEPL(statementText);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }
    
            statementText = "select InnerMap.int.inner2? as t0 from MyLevel2.win:length(5)";
            try
            {
                _epService.EPAdministrator.CreateEPL(statementText);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }
        }
    
        private void AssertResults(EventBean theEvent, Object[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    
        private IDictionary<string,object> MakeMap(params object[] keysAndValues)
        {
            if (keysAndValues.Length % 2 != 0)
            {
                throw new ArgumentException();
            }
            Object[][] pairs = new Object[keysAndValues.Length / 2][];
            for (int i = 0; i < keysAndValues.Length; i++)
            {
                int index = i / 2;

                pairs[index] = pairs[index] ?? new object[2];

                if (i % 2 == 0)
                {
                    pairs[index][0] = keysAndValues[i];
                }
                else
                {
                    pairs[index][1] = keysAndValues[i];
                }
            }
            return MakeMap(pairs);
        }
    
        private IDictionary<string,object> MakeMap(Object[][] pairs)
        {
            var map = new Dictionary<string, object>();
            for (int i = 0; i < pairs.Length; i++)
            {
                map.Put((string) pairs[i][0], pairs[i][1]);
            }
            return map;
        }
    }
}
