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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestMapEventPropertyDynamic
    {
	    private SupportUpdateListener _listener;
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

        [Test]
	    public void TestMapWithinMap() {
	        var properties = new Properties();
	        properties.Put("innermap", typeof(IDictionary<string, object>).FullName);
	        _epService.EPAdministrator.Configuration.AddEventType("MyLevel2", properties);

	        var statementText = "select " +
	                               "innermap.int? as t0, " +
	                               "innermap.innerTwo?.nested as t1, " +
	                               "innermap.innerTwo?.innerThree.nestedTwo as t2, " +
	                               "dynamicOne? as t3, " +
	                               "dynamicTwo? as t4, " +
	                               "indexed[1]? as t5, " +
	                               "mapped('keyOne')? as t6, " +
	                               "innermap.indexedTwo[0]? as t7, " +
	                               "innermap.mappedTwo('keyTwo')? as t8 " +
	                               "from MyLevel2#length(5)";
	        var statement = _epService.EPAdministrator.CreateEPL(statementText);
	        statement.AddListener(_listener);

	        var map = new Dictionary<string, object>();
	        map.Put("dynamicTwo", 20L);
	        map.Put("innermap", MakeMap(
	                    "int", 10,
	                    "indexedTwo", new int[] {-10},
	                    "mappedTwo", MakeMap("keyTwo", "def"),
	                    "innerTwo", MakeMap("nested", 30d,
	                                        "innerThree", MakeMap("nestedTwo", 99))));
	        map.Put("indexed", new float[] {-1, -2, -3});
	        map.Put("mapped", MakeMap("keyOne", "abc"));
	        _epService.EPRuntime.SendEvent(map, "MyLevel2");
	        AssertResults(_listener.AssertOneGetNewAndReset(), new object[] {10, 30d, 99, null, 20L, -2.0f, "abc", -10, "def"});

	        map = new Dictionary<string, object>();
	        map.Put("innermap", MakeMap(
	                    "indexedTwo", new int[] {},
	                    "mappedTwo", MakeMap("yyy", "xxx"),
	                    "innerTwo", null));
	        map.Put("indexed", new float[] {});
	        map.Put("mapped", MakeMap("xxx", "yyy"));
	        _epService.EPRuntime.SendEvent(map, "MyLevel2");
	        AssertResults(_listener.AssertOneGetNewAndReset(), new object[] {null, null, null, null, null, null, null, null, null});

	        _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyLevel2");
	        AssertResults(_listener.AssertOneGetNewAndReset(), new object[] {null, null, null, null, null, null, null, null, null});

	        map = new Dictionary<string, object>();
	        map.Put("innermap", "xxx");
	        map.Put("indexed", null);
	        map.Put("mapped", "xxx");
	        _epService.EPRuntime.SendEvent(map, "MyLevel2");
	        AssertResults(_listener.AssertOneGetNewAndReset(), new object[] {null, null, null, null, null, null, null, null, null});
	    }

        [Test]
	    public void TestMapWithinMapExists() {
	        var properties = new Properties();
	        properties.Put("innermap", typeof(IDictionary<string, object>).FullName);
	        _epService.EPAdministrator.Configuration.AddEventType("MyLevel2", properties);

	        var statementText = "select " +
	                               "exists(innermap.int?) as t0, " +
	                               "exists(innermap.innerTwo?.nested) as t1, " +
	                               "exists(innermap.innerTwo?.innerThree.nestedTwo) as t2, " +
	                               "exists(dynamicOne?) as t3, " +
	                               "exists(dynamicTwo?) as t4, " +
	                               "exists(indexed[1]?) as t5, " +
	                               "exists(mapped('keyOne')?) as t6, " +
	                               "exists(innermap.indexedTwo[0]?) as t7, " +
	                               "exists(innermap.mappedTwo('keyTwo')?) as t8 " +
	                               "from MyLevel2#length(5)";
	        var statement = _epService.EPAdministrator.CreateEPL(statementText);
	        statement.AddListener(_listener);

	        var map = new Dictionary<string, object>();
	        map.Put("dynamicTwo", 20L);
	        map.Put("innermap", MakeMap(
	                    "int", 10,
	                    "indexedTwo", new int[] {-10},
	                    "mappedTwo", MakeMap("keyTwo", "def"),
	                    "innerTwo", MakeMap("nested", 30d,
	                                        "innerThree", MakeMap("nestedTwo", 99))));
	        map.Put("indexed", new float[] {-1, -2, -3});
	        map.Put("mapped", MakeMap("keyOne", "abc"));
	        _epService.EPRuntime.SendEvent(map, "MyLevel2");
	        AssertResults(_listener.AssertOneGetNewAndReset(), new object[] {true, true,true,false,true,true,true,true,true});

	        map = new Dictionary<string, object>();
	        map.Put("innermap", MakeMap(
	                    "indexedTwo", new int[] {},
	                    "mappedTwo", MakeMap("yyy", "xxx"),
	                    "innerTwo", null));
	        map.Put("indexed", new float[] {});
	        map.Put("mapped", MakeMap("xxx", "yyy"));
	        _epService.EPRuntime.SendEvent(map, "MyLevel2");
	        AssertResults(_listener.AssertOneGetNewAndReset(), new object[] {false, false,false,false,false,false,false,false,false});

	        _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyLevel2");
	        AssertResults(_listener.AssertOneGetNewAndReset(), new object[] {false, false,false,false,false,false,false,false,false});

	        map = new Dictionary<string, object>();
	        map.Put("innermap", "xxx");
	        map.Put("indexed", null);
	        map.Put("mapped", "xxx");
	        _epService.EPRuntime.SendEvent(map, "MyLevel2");
	        AssertResults(_listener.AssertOneGetNewAndReset(), new object[] {false, false,false,false,false,false,false,false,false});
	    }

        [Test]
	    public void TestMapWithinMap2LevelsInvalid() {
	        var properties = new Properties();
	        properties.Put("innermap", typeof(IDictionary<string, object>).FullName);
	        _epService.EPAdministrator.Configuration.AddEventType("MyLevel2", properties);

	        var statementText = "select innermap.int as t0 from MyLevel2#length(5)";
	        try {
	            _epService.EPAdministrator.CreateEPL(statementText);
                Assert.Fail();
	        } catch (EPException) {
	            // expected
	        }

	        statementText = "select innermap.int.inner2? as t0 from MyLevel2#length(5)";
	        try {
	            _epService.EPAdministrator.CreateEPL(statementText);
                Assert.Fail();
	        } catch (EPException) {
	            // expected
	        }

	        statementText = "select innermap.int.inner2? as t0 from MyLevel2#length(5)";
	        try {
	            _epService.EPAdministrator.CreateEPL(statementText);
	            Assert.Fail();
	        } catch (EPException) {
	            // expected
	        }
	    }

	    private void AssertResults(EventBean theEvent, object[] result) {
	        for (var i = 0; i < result.Length; i++) {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
	        }
	    }

	    private IDictionary<string, object> MakeMap(params object[] keysAndValues) {
	        if (keysAndValues.Length % 2 != 0) {
	            throw new ArgumentException();
	        }

	        var pairs = new object[keysAndValues.Length / 2,2];
	        for (var i = 0; i < keysAndValues.Length; i++) {
	            var index = i / 2;
	            if (i % 2 == 0) {
	                pairs[index,0] = keysAndValues[i];
	            } else {
	                pairs[index,1] = keysAndValues[i];
	            }
	        }
	        return MakeMap(pairs);
	    }

        private IDictionary<string, object> MakeMap(object[,] pairs)
        {
            var map = new Dictionary<string, object>();
            var len = pairs.GetLength(0);
            for (var i = 0; i < len; i++)
            {
                map.Put((string) pairs[i,0], pairs[i,1]);
            }
            return map;
        }
	}
} // end of namespace
