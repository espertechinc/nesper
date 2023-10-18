///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.@event.map
{
	public class EventMapPropertyDynamic : RegressionExecution {
	    public void Run(RegressionEnvironment env) {
	        RunAssertionMapWithinMap(env);
	        RunAssertionMapWithinMapExists(env);
	        RunAssertionMapWithinMap2LevelsInvalid(env);
	    }

	    public ISet<RegressionFlag> Flags() {
	        return Collections.Set(RegressionFlag.SERDEREQUIRED);
	    }

	    private void RunAssertionMapWithinMap(RegressionEnvironment env) {
	        var statementText = "@name('s0') select " +
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
	        env.CompileDeploy(statementText).AddListener("s0");

	        var map = new Dictionary<string, object>();
	        map.Put("dynamicTwo", 20L);
	        map.Put("innermap", MakeMap(
	            "int", 10,
	            "indexedTwo", new int[]{-10},
	            "mappedTwo", MakeMap("keyTwo", "def"),
	            "innerTwo", MakeMap("nested", 30d,
	                "innerThree", MakeMap("nestedTwo", 99))));
	        map.Put("indexed", new float[]{-1, -2, -3});
	        map.Put("mapped", MakeMap("keyOne", "abc"));
	        env.SendEventMap(map, "MyLevel2");
	        AssertResults(env, new object[]{10, 30d, 99, null, 20L, -2.0f, "abc", -10, "def"});

	        map = new Dictionary<string, object>();
	        map.Put("innermap", MakeMap(
	            "indexedTwo", new int[]{},
	            "mappedTwo", MakeMap("yyy", "xxx"),
	            "innerTwo", null));
	        map.Put("indexed", new float[]{});
	        map.Put("mapped", MakeMap("xxx", "yyy"));
	        env.SendEventMap(map, "MyLevel2");
	        AssertResults(env, new object[]{null, null, null, null, null, null, null, null, null});

	        env.SendEventMap(new Dictionary<string, object>(), "MyLevel2");
	        AssertResults(env, new object[]{null, null, null, null, null, null, null, null, null});

	        map = new Dictionary<string, object>();
	        map.Put("innermap", "xxx");
	        map.Put("indexed", null);
	        map.Put("mapped", "xxx");
	        env.SendEventMap(map, "MyLevel2");
	        AssertResults(env, new object[]{null, null, null, null, null, null, null, null, null});

	        env.UndeployAll();
	    }

	    private void RunAssertionMapWithinMapExists(RegressionEnvironment env) {
	        var statementText = "@name('s0') select " +
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
	        env.CompileDeploy(statementText).AddListener("s0");

	        var map = new Dictionary<string, object>();
	        map.Put("dynamicTwo", 20L);
	        map.Put("innermap", MakeMap(
	            "int", 10,
	            "indexedTwo", new int[]{-10},
	            "mappedTwo", MakeMap("keyTwo", "def"),
	            "innerTwo", MakeMap("nested", 30d,
	                "innerThree", MakeMap("nestedTwo", 99))));
	        map.Put("indexed", new float[]{-1, -2, -3});
	        map.Put("mapped", MakeMap("keyOne", "abc"));
	        env.SendEventMap(map, "MyLevel2");
	        AssertResults(env, new object[]{true, true, true, false, true, true, true, true, true});

	        map = new Dictionary<string, object>();
	        map.Put("innermap", MakeMap(
	            "indexedTwo", new int[]{},
	            "mappedTwo", MakeMap("yyy", "xxx"),
	            "innerTwo", null));
	        map.Put("indexed", new float[]{});
	        map.Put("mapped", MakeMap("xxx", "yyy"));
	        env.SendEventMap(map, "MyLevel2");
	        AssertResults(env, new object[]{false, false, false, false, false, false, false, false, false});

	        env.SendEventMap(new Dictionary<string, object>(), "MyLevel2");
	        AssertResults(env, new object[]{false, false, false, false, false, false, false, false, false});

	        map = new Dictionary<string, object>();
	        map.Put("innermap", "xxx");
	        map.Put("indexed", null);
	        map.Put("mapped", "xxx");
	        env.SendEventMap(map, "MyLevel2");
	        AssertResults(env, new object[]{false, false, false, false, false, false, false, false, false});

	        env.UndeployAll();
	    }

	    private void RunAssertionMapWithinMap2LevelsInvalid(RegressionEnvironment env) {
	        env.TryInvalidCompile("select innermap.int as t0 from MyLevel2#length(5)", "skip");
	        env.TryInvalidCompile("select innermap.int.inner2? as t0 from MyLevel2#length(5)", "skip");
	        env.TryInvalidCompile("select innermap.int.inner2? as t0 from MyLevel2#length(5)", "skip");
	    }

	    private void AssertResults(RegressionEnvironment env, object[] result) {
	        env.AssertEventNew("s0", @event => {
	            for (var i = 0; i < result.Length; i++) {
	                Assert.AreEqual(result[i], @event.Get("t" + i), "failed for index " + i);
	            }
	        });
	    }

	    private IDictionary<object, object> MakeMap(params object[] keysAndValues) {
	        if (keysAndValues.Length % 2 != 0) {
	            throw new ArgumentException();
	        }

	        var pairs = new object[keysAndValues.Length / 2][];
	        for (var ii = 0; ii < pairs.Length; ii++) {
		        pairs[ii] = new object[2];
	        }
	        
	        for (var i = 0; i < keysAndValues.Length; i++) {
	            var index = i / 2;
	            if (i % 2 == 0) {
	                pairs[index][0] = keysAndValues[i];
	            } else {
	                pairs[index][1] = keysAndValues[i];
	            }
	        }
	        return MakeMap(pairs);
	    }

	    private IDictionary<object, object> MakeMap(object[][] pairs) {
	        IDictionary<object, object> map = new Dictionary<object, object>();
	        for (var i = 0; i < pairs.Length; i++) {
	            map.Put(pairs[i][0], pairs[i][1]);
	        }
	        return map;
	    }
	}
} // end of namespace
