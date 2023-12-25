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

using NUnit.Framework;
namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapPropertyDynamic
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMapWithinMap(execs);
            WithMapWithinMapExists(execs);
            WithMapWithinMap2LevelsInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMapWithinMap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new MapWithinMap());
            return execs;
        }

        public static IList<RegressionExecution> WithMapWithinMapExists(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new MapWithinMapExists());
            return execs;
        }

        public static IList<RegressionExecution> WithMapWithinMap2LevelsInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new MapWithinMap2LevelsInvalid());
            return execs;
        }
        
        private class MapWithinMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@name('s0') select " +
                    "Innermap.int? as t0, " +
                    "Innermap.innerTwo?.Nested as t1, " +
                    "Innermap.innerTwo?.innerThree.NestedTwo as t2, " +
                    "dynamicOne? as t3, " +
                    "dynamicTwo? as t4, " +
                    "indexed[1]? as t5, " +
                    "mapped('keyOne')? as t6, " +
                    "Innermap.indexedTwo[0]? as t7, " +
                    "Innermap.mappedTwo('keyTwo')? as t8 " +
                    "from MyLevel2#length(5)";
                
                env.CompileDeploy(statementText).AddListener("s0");

                var map = new Dictionary<string, object>();
                map.Put("dynamicTwo", 20L);
                map.Put(
                    "Innermap",
                    MakeMap(
                        "int",
                        10,
                        "indexedTwo",
                        new int[] { -10 },
                        "mappedTwo",
                        MakeMap("keyTwo", "def"),
                        "innerTwo",
                        MakeMap(
                            "Nested",
                            30d,
                            "innerThree",
                            MakeMap("nestedTwo", 99))));
                map.Put("indexed", new float[] { -1, -2, -3 });
                map.Put("mapped", MakeMap("keyOne", "abc"));
                env.SendEventMap(map, "MyLevel2");
                AssertResults(env, new object[] { 10, 30d, 99, null, 20L, -2.0f, "abc", -10, "def" });

                map = new Dictionary<string, object>();
                map.Put(
                    "Innermap",
                    MakeMap(
                        "indexedTwo",
                        new int[] { },
                        "mappedTwo",
                        MakeMap("yyy", "xxx"),
                        "innerTwo",
                        null));
                map.Put("indexed", new float[] { });
                map.Put("mapped", MakeMap("xxx", "yyy"));
                env.SendEventMap(map, "MyLevel2");
                AssertResults(env, new object[] { null, null, null, null, null, null, null, null, null });

                env.SendEventMap(new Dictionary<string, object>(), "MyLevel2");
                AssertResults(env, new object[] { null, null, null, null, null, null, null, null, null });

                map = new Dictionary<string, object>();
                map.Put("Innermap", "xxx");
                map.Put("indexed", null);
                map.Put("mapped", "xxx");
                env.SendEventMap(map, "MyLevel2");
                AssertResults(env, new object[] { null, null, null, null, null, null, null, null, null });

                env.UndeployAll();
            }
            
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private class MapWithinMapExists : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@name('s0') select " +
                                    "exists(Innermap.int?) as t0, " +
                                    "exists(Innermap.innerTwo?.Nested) as t1, " +
                                    "exists(Innermap.innerTwo?.innerThree.NestedTwo) as t2, " +
                                    "exists(dynamicOne?) as t3, " +
                                    "exists(dynamicTwo?) as t4, " +
                                    "exists(indexed[1]?) as t5, " +
                                    "exists(mapped('keyOne')?) as t6, " +
                                    "exists(Innermap.indexedTwo[0]?) as t7, " +
                                    "exists(Innermap.mappedTwo('keyTwo')?) as t8 " +
                                    "from MyLevel2#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                var map = new Dictionary<string, object>();
                map.Put("dynamicTwo", 20L);
                map.Put(
                    "Innermap",
                    MakeMap(
                        "int",
                        10,
                        "indexedTwo",
                        new int[] { -10 },
                        "mappedTwo",
                        MakeMap("keyTwo", "def"),
                        "innerTwo",
                        MakeMap(
                            "Nested",
                            30d,
                            "innerThree",
                            MakeMap("nestedTwo", 99))));
                map.Put("indexed", new float[] { -1, -2, -3 });
                map.Put("mapped", MakeMap("keyOne", "abc"));
                env.SendEventMap(map, "MyLevel2");
                AssertResults(env, new object[] { true, true, true, false, true, true, true, true, true });

                map = new Dictionary<string, object>();
                map.Put(
                    "Innermap",
                    MakeMap(
                        "indexedTwo",
                        new int[] { },
                        "mappedTwo",
                        MakeMap("yyy", "xxx"),
                        "innerTwo",
                        null));
                map.Put("indexed", new float[] { });
                map.Put("mapped", MakeMap("xxx", "yyy"));
                env.SendEventMap(map, "MyLevel2");
                AssertResults(env, new object[] { false, false, false, false, false, false, false, false, false });

                env.SendEventMap(new Dictionary<string, object>(), "MyLevel2");
                AssertResults(env, new object[] { false, false, false, false, false, false, false, false, false });

                map = new Dictionary<string, object>();
                map.Put("Innermap", "xxx");
                map.Put("indexed", null);
                map.Put("mapped", "xxx");
                env.SendEventMap(map, "MyLevel2");
                AssertResults(env, new object[] { false, false, false, false, false, false, false, false, false });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private class MapWithinMap2LevelsInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile("select Innermap.int as t0 from MyLevel2#length(5)", "skip");
                env.TryInvalidCompile("select Innermap.int.inner2? as t0 from MyLevel2#length(5)", "skip");
                env.TryInvalidCompile("select Innermap.int.inner2? as t0 from MyLevel2#length(5)", "skip");
            }
            
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private static void AssertResults(
            RegressionEnvironment env,
            object[] result)
        {
            env.AssertEventNew(
                "s0",
                @event => {
                    for (var i = 0; i < result.Length; i++) {
                        Assert.AreEqual(result[i], @event.Get("t" + i), "failed for index " + i);
                    }
                });
        }

        private static IDictionary<object, object> MakeMap(params object[] keysAndValues)
        {
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
                }
                else {
                    pairs[index][1] = keysAndValues[i];
                }
            }

            return MakeMap(pairs);
        }

        private IDictionary<object, object> MakeMap(object[][] pairs)
        {
            IDictionary<object, object> map = new Dictionary<object, object>();
            for (var i = 0; i < pairs.Length; i++) {
                map.Put(pairs[i][0], pairs[i][1]);
            }

            return map;
        }
    }
} // end of namespace