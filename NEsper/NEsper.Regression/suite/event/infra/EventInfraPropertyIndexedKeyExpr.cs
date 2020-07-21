///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyIndexedKeyExpr : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionOA(env);
            RunAssertionMap(env);
            RunAssertionWrapper(env);
            RunAssertionBean(env);
            RunAssertionJson(env);
            RunAssertionJsonClassProvided(env);
        }

        private void RunAssertionJsonClassProvided(RegressionEnvironment env)
        {
            env.CompileDeploy(
                    "@JsonSchema(className='" +
                    typeof(MyLocalJsonProvided).MaskTypeName() +
                    "') @public @buseventtype create json schema JsonSchema();\n" +
                    "@name('s0') select * from JsonSchema;\n")
                .AddListener("s0");
            env.SendEventJson("{ \"indexed\": [1, 2], \"mapped\" : { \"keyOne\": 20 }}", "JsonSchema");
            var @event = env.Listener("s0").AssertOneGetNewAndReset();

            Assert.AreEqual(2, @event.EventType.GetGetterIndexed("indexed").Get(@event, 1));
            Assert.AreEqual(20, @event.EventType.GetGetterMapped("mapped").Get(@event, "keyOne"));

            env.UndeployAll();
        }

        private void RunAssertionJson(RegressionEnvironment env)
        {
            env.CompileDeploy(
                    "@public @buseventtype create json schema JsonSchema(indexed int[], mapped System.Collections.Generic.IDictionary<string, object>);\n" +
                    "@name('s0') select * from JsonSchema;\n")
                .AddListener("s0");
            env.SendEventJson("{ \"indexed\": [1, 2], \"mapped\" : { \"keyOne\": 20 }}", "JsonSchema");
            var @event = env.Listener("s0").AssertOneGetNewAndReset();

            Assert.AreEqual(2, @event.EventType.GetGetterIndexed("indexed").Get(@event, 1));
            Assert.AreEqual(20, @event.EventType.GetGetterMapped("mapped").Get(@event, "keyOne"));

            env.UndeployAll();
        }

        private void RunAssertionBean(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeployWBusPublicType(
                "create schema MyIndexMappedSamplerBean as " + typeof(MyIndexMappedSamplerBean).MaskTypeName(),
                path);

            env.CompileDeploy("@name('s0') select * from MyIndexMappedSamplerBean", path).AddListener("s0");

            env.SendEventBean(new MyIndexMappedSamplerBean());

            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            var type = @event.EventType;
            Assert.AreEqual(2, type.GetGetterIndexed("ListOfInt").Get(@event, 1));
            Assert.AreEqual(2, type.GetGetterIndexed("IterableOfInt").Get(@event, 1));

            env.UndeployAll();
        }

        private void RunAssertionWrapper(RegressionEnvironment env)
        {
            var collections = typeof(Collections).FullName;
            env.CompileDeploy(
                $"@name('s0') select {{1, 2}} as Arr, *, {collections}.SingletonDataMap('A', 2) as Mapped from SupportBean");
            env.AddListener("s0");

            env.SendEventBean(new SupportBean());
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            var type = @event.EventType;
            Assert.AreEqual(2, type.GetGetterIndexed("Arr").Get(@event, 1));
            Assert.AreEqual(2, type.GetGetterMapped("Mapped").Get(@event, "A"));

            env.UndeployAll();
        }

        private void RunAssertionMap(RegressionEnvironment env)
        {
            var epl = "create schema MapEventInner(P0 string);\n" +
                      "create schema MapEvent(intarray int[], mapinner MapEventInner[]);\n" +
                      "@name('s0') select * from MapEvent;\n";
            env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

            IDictionary<string, object>[] mapinner = {
                Collections.SingletonDataMap("P0", "A"),
                Collections.SingletonDataMap("P0", "B")
            };
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("intarray", new[] {1, 2});
            map.Put("mapinner", mapinner);
            env.SendEventMap(map, "MapEvent");
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            var type = @event.EventType;
            Assert.AreEqual(2, type.GetGetterIndexed("intarray").Get(@event, 1));
            Assert.IsNull(type.GetGetterIndexed("dummy"));
            Assert.AreEqual(mapinner[1], type.GetGetterIndexed("mapinner").Get(@event, 1));

            env.UndeployAll();
        }

        private void RunAssertionOA(RegressionEnvironment env)
        {
            var epl = "create objectarray schema OAEventInner(p0 string);\n" +
                      "create objectarray schema OAEvent(intarray int[], oainner OAEventInner[]);\n" +
                      "@name('s0') select * from OAEvent;\n";
            env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

            object[] oainner = {
                new object[] {"A"},
                new object[] {"B"}
            };
            env.SendEventObjectArray(new object[] {new[] {1, 2}, oainner}, "OAEvent");
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            var type = @event.EventType;
            Assert.AreEqual(2, type.GetGetterIndexed("intarray").Get(@event, 1));
            Assert.IsNull(type.GetGetterIndexed("dummy"));
            Assert.AreEqual(oainner[1], type.GetGetterIndexed("oainner").Get(@event, 1));

            env.UndeployAll();
        }

        public class MyIndexMappedSamplerBean
        {
            public IList<int> ListOfInt { get; } = Arrays.AsList(1, 2);

            public IEnumerable<int> IterableOfInt => ListOfInt;
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public int[] indexed;
            public IDictionary<string, object> mapped;
        }
    }
} // end of namespace