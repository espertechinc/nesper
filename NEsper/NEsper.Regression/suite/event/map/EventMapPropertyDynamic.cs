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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapPropertyDynamic : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionMapWithinMap(env);
            RunAssertionMapWithinMapExists(env);
            RunAssertionMapWithinMap2LevelsInvalid(env);
        }

        private void RunAssertionMapWithinMap(RegressionEnvironment env)
        {
            var statementText = "@Name('s0') select " +
                                "innermap.int? as t0, " +
                                "innermap.innerTwo?.Nested as t1, " +
                                "innermap.innerTwo?.innerThree.NestedTwo as t2, " +
                                "dynamicOne? as t3, " +
                                "dynamicTwo? as t4, " +
                                "indexed[1]? as t5, " +
                                "mapped('keyOne')? as t6, " +
                                "innermap.IndexedTwo[0]? as t7, " +
                                "innermap.mappedTwo('keyTwo')? as t8 " +
                                "from MyLevel2#length(5)";
            env.CompileDeploy(statementText).AddListener("s0");

            var map = new Dictionary<string, object>();
            map.Put("dynamicTwo", 20L);
            map.Put(
                "innermap",
                MakeMap(
                    "int",
                    10,
                    "IndexedTwo",
                    new[] {-10},
                    "MappedTwo",
                    MakeMap("keyTwo", "def"),
                    "innerTwo",
                    MakeMap(
                        "nested", 
                        30d, 
                        "innerThree",
                        MakeMap("nestedTwo", 99))));
            map.Put("Indexed", new float[] {-1, -2, -3});
            map.Put("Mapped", MakeMap("keyOne", "abc"));
            env.SendEventMap(map, "MyLevel2");
            AssertResults(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new object[] {10, 30d, 99, null, 20L, -2.0f, "abc", -10, "def"});

            map = new Dictionary<string, object>();
            map.Put(
                "innermap",
                MakeMap(
                    "IndexedTwo",
                    new int[] { },
                    "MappedTwo",
                    MakeMap("yyy", "xxx"),
                    "innerTwo",
                    null));
            map.Put("Indexed", new float[] { });
            map.Put("Mapped", MakeMap("xxx", "yyy"));
            env.SendEventMap(map, "MyLevel2");
            AssertResults(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new object[] {null, null, null, null, null, null, null, null, null});

            env.SendEventMap(new Dictionary<string, object>(), "MyLevel2");
            AssertResults(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new object[] {null, null, null, null, null, null, null, null, null});

            map = new Dictionary<string, object>();
            map.Put("innermap", "xxx");
            map.Put("Indexed", null);
            map.Put("Mapped", "xxx");
            env.SendEventMap(map, "MyLevel2");
            AssertResults(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new object[] {null, null, null, null, null, null, null, null, null});

            env.UndeployAll();
        }

        private void RunAssertionMapWithinMapExists(RegressionEnvironment env)
        {
            var statementText = "@Name('s0') select " +
                                "exists(innermap.int?) as t0, " +
                                "exists(innermap.innerTwo?.Nested) as t1, " +
                                "exists(innermap.innerTwo?.innerThree.NestedTwo) as t2, " +
                                "exists(dynamicOne?) as t3, " +
                                "exists(dynamicTwo?) as t4, " +
                                "exists(indexed[1]?) as t5, " +
                                "exists(mapped('keyOne')?) as t6, " +
                                "exists(innermap.IndexedTwo[0]?) as t7, " +
                                "exists(innermap.mappedTwo('keyTwo')?) as t8 " +
                                "from MyLevel2#length(5)";
            env.CompileDeploy(statementText).AddListener("s0");

            var map = new Dictionary<string, object>();
            map.Put("dynamicTwo", 20L);
            map.Put(
                "innermap",
                MakeMap(
                    "int",
                    10,
                    "IndexedTwo",
                    new[] {-10},
                    "MappedTwo",
                    MakeMap("keyTwo", "def"),
                    "innerTwo",
                    MakeMap(
                        "nested",
                        30d,
                        "innerThree",
                        MakeMap("nestedTwo", 99))));
            map.Put("Indexed", new float[] {-1, -2, -3});
            map.Put("Mapped", MakeMap("keyOne", "abc"));
            env.SendEventMap(map, "MyLevel2");
            AssertResults(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new object[] {true, true, true, false, true, true, true, true, true});

            map = new Dictionary<string, object>();
            map.Put(
                "innermap",
                MakeMap(
                    "IndexedTwo",
                    new int[] { },
                    "MappedTwo",
                    MakeMap("yyy", "xxx"),
                    "innerTwo",
                    null));
            map.Put("Indexed", new float[] { });
            map.Put("Mapped", MakeMap("xxx", "yyy"));
            env.SendEventMap(map, "MyLevel2");
            AssertResults(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new object[] {false, false, false, false, false, false, false, false, false});

            env.SendEventMap(new Dictionary<string, object>(), "MyLevel2");
            AssertResults(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new object[] {false, false, false, false, false, false, false, false, false});

            map = new Dictionary<string, object>();
            map.Put("innermap", "xxx");
            map.Put("Indexed", null);
            map.Put("Mapped", "xxx");
            env.SendEventMap(map, "MyLevel2");
            AssertResults(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new object[] {false, false, false, false, false, false, false, false, false});

            env.UndeployAll();
        }

        private void RunAssertionMapWithinMap2LevelsInvalid(RegressionEnvironment env)
        {
            TryInvalidCompile(env, "select innermap.int as t0 from MyLevel2#length(5)", "skip");
            TryInvalidCompile(env, "select innermap.int.inner2? as t0 from MyLevel2#length(5)", "skip");
            TryInvalidCompile(env, "select innermap.int.inner2? as t0 from MyLevel2#length(5)", "skip");
        }

        private void AssertResults(
            EventBean theEvent,
            object[] result)
        {
            for (var i = 0; i < result.Length; i++) {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }

        private IDictionary<string, object> MakeMap(params object[] keysAndValues)
        {
            if (keysAndValues.Length % 2 != 0) {
                throw new ArgumentException();
            }

            var pairs = new object[keysAndValues.Length / 2][];
            pairs.Fill(() => new object[2]);

            for (var i = 0; i < keysAndValues.Length; i++) {
                var index = i / 2;
                if (i % 2 == 0) {
                    pairs[index][0] = keysAndValues[i];
                }
                else {
                    pairs[index][1] = keysAndValues[i];
                }
            }

            return MakeMap(pairs);
        }

        private IDictionary<string, object> MakeMap(object[][] pairs)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            for (var i = 0; i < pairs.Length; i++) {
                map.Put((string) pairs[i][0], pairs[i][1]);
            }

            return map;
        }
    }
} // end of namespace