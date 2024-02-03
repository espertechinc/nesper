///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyIndexedKeyExpr : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            //RunAssertionOA(env);
            //RunAssertionMap(env);
            //RunAssertionWrapper(env);
            //RunAssertionBean(env);
            RunAssertionJson(env);
            RunAssertionJsonClassProvided(env);
        }

        private void RunAssertionJsonClassProvided(RegressionEnvironment env)
        {
            env.CompileDeploy(
                    "@JsonSchema(ClassName='" +
                    typeof(MyLocalJsonProvided).MaskTypeName() +
                    "') @public @buseventtype create json schema JsonSchema();\n" +
                    "@name('s0') select * from JsonSchema;\n")
                .AddListener("s0");
            env.SendEventJson("{ \"indexed\": [1, 2], \"mapped\" : { \"keyOne\": 20 }}", "JsonSchema");

            env.AssertEventNew(
                "s0",
                @event => {
                    ClassicAssert.AreEqual(2, @event.EventType.GetGetterIndexed("indexed").Get(@event, 1));
                    ClassicAssert.AreEqual(20, @event.EventType.GetGetterMapped("mapped").Get(@event, "keyOne"));
                });

            env.UndeployAll();
        }

        private void RunAssertionJson(RegressionEnvironment env)
        {
            var mapType = typeof(IDictionary<string, object>).CleanName();
            env.CompileDeploy(
                    $"@public @buseventtype create json schema JsonSchema(indexed int[], mapped `{mapType}`);\n" +
                    "@name('s0') select * from JsonSchema;\n")
                .AddListener("s0");
            env.SendEventJson("{ \"indexed\": [1, 2], \"mapped\" : { \"keyOne\": 20 }}", "JsonSchema");

            env.AssertEventNew(
                "s0",
                @event => {
                    ClassicAssert.AreEqual(2, @event.EventType.GetGetterIndexed("indexed").Get(@event, 1));
                    ClassicAssert.AreEqual(20, @event.EventType.GetGetterMapped("mapped").Get(@event, "keyOne"));
                });

            env.UndeployAll();
        }

        private void RunAssertionBean(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@public @buseventtype create schema MyIndexMappedSamplerBean as " +
                typeof(MyIndexMappedSamplerBean).MaskTypeName(),
                path);

            env.CompileDeploy("@name('s0') select * from MyIndexMappedSamplerBean", path).AddListener("s0");

            env.SendEventBean(new MyIndexMappedSamplerBean());

            env.AssertEventNew(
                "s0",
                @event => {
                    var type = @event.EventType;
                    ClassicAssert.AreEqual(2, type.GetGetterIndexed("ListOfInt").Get(@event, 1));
                    ClassicAssert.AreEqual(2, type.GetGetterIndexed("IterableOfInt").Get(@event, 1));
                });

            env.UndeployAll();
        }

        private void RunAssertionWrapper(RegressionEnvironment env)
        {
            var collections = typeof(Collections).FullName;
            env.CompileDeploy(
                $"@name('s0') select {{1, 2}} as Arr, *, {collections}.SingletonDataMap('A', 2) as Mapped from SupportBean");
            env.AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertEventNew(
                "s0",
                @event => {
                    var type = @event.EventType;
                    ClassicAssert.AreEqual(2, type.GetGetterIndexed("Arr").Get(@event, 1));
                    ClassicAssert.AreEqual(2, type.GetGetterMapped("Mapped").Get(@event, "A"));
                });

            env.UndeployAll();
        }

        private void RunAssertionMap(RegressionEnvironment env)
        {
            var epl = "create schema MapEventInner(p0 string);\n" +
                      "@public @buseventtype create schema MapEvent(intarray int[], mapinner MapEventInner[]);\n" +
                      "@name('s0') select * from MapEvent;\n";
            env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

            var mapinner = new IDictionary<string, object>[] {
                Collections.SingletonDataMap("p0", "A"),
                Collections.SingletonDataMap("p0", "B")
            };
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("intarray", new int[] { 1, 2 });
            map.Put("mapinner", mapinner);
            env.SendEventMap(map, "MapEvent");
            env.AssertEventNew(
                "s0",
                @event => {
                    var type = @event.EventType;
                    ClassicAssert.AreEqual(2, type.GetGetterIndexed("intarray").Get(@event, 1));
                    ClassicAssert.IsNull(type.GetGetterIndexed("dummy"));
                    ClassicAssert.AreEqual(mapinner[1], type.GetGetterIndexed("mapinner").Get(@event, 1));
                });

            env.UndeployAll();
        }

        private void RunAssertionOA(RegressionEnvironment env)
        {
            var epl =
                "@public create objectarray schema OAEventInner(p0 string);\n" +
                "@buseventtype @public create objectarray schema OAEvent(intarray int[], oainner OAEventInner[]);\n" +
                "@name('s0') select * from OAEvent;\n";
            env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

            var oainner = new object[] {
                new object[] { "A" },
                new object[] { "B" }
            };
            
            env.SendEventObjectArray(new object[] { new int[] { 1, 2 }, oainner }, "OAEvent");
            env.AssertEventNew(
                "s0",
                @event => {
                    var type = @event.EventType;
                    ClassicAssert.AreEqual(2, type.GetGetterIndexed("intarray").Get(@event, 1));
                    ClassicAssert.IsNull(type.GetGetterIndexed("dummy"));
                    ClassicAssert.AreEqual(oainner[1], type.GetGetterIndexed("oainner").Get(@event, 1));
                });

            env.UndeployAll();
        }

        public class MyIndexMappedSamplerBean
        {
            public IList<int> ListOfInt { get; } = Arrays.AsList(1, 2);

            public IEnumerable<int> IterableOfInt => ListOfInt;
        }

        public class MyLocalJsonProvided
        {
            public int[] indexed;
            public IDictionary<string, object> mapped;
        }
    }
} // end of namespace