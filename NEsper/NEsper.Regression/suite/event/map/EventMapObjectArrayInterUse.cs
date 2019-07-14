///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapObjectArrayInterUse : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionObjectArrayWithMap(env);
            RunAssertionMapWithObjectArray(env);
        }

        // test ObjectArray event with Map, Map[], MapType and MapType[] properties
        private void RunAssertionObjectArrayWithMap(RegressionEnvironment env)
        {
            env.CompileDeploy("@Name('s0') select p0 as c0, p1.im as c1, p2[0].im as c2, p3.om as c3 from OAType");
            env.AddListener("s0");

            env.SendEventObjectArray(
                new object[] {
                    "E1", Collections.SingletonMap("im", "IM1"), new[] {Collections.SingletonDataMap("im", "IM2")},
                    Collections.SingletonMap("om", "OM1")
                },
                "OAType");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0,c1,c2,c3".SplitCsv(),
                new object[] {"E1", "IM1", "IM2", "OM1"});

            env.UndeployAll();

            // test inserting from array to map
            env.CompileDeploy("@Name('s0') insert into MapType(im) select p0 from OAType").AddListener("s0");
            env.SendEventObjectArray(new object[] {"E1", null, null, null}, "OAType");
            Assert.IsTrue(env.Listener("s0").AssertOneGetNew() is MappedEventBean);
            Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("im"));

            env.UndeployAll();
        }

        // test Map event with ObjectArrayType and ObjectArrayType[] properties
        private void RunAssertionMapWithObjectArray(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var schema = "create objectarray schema OATypeInMap(p0 string, p1 int);\n" +
                         "create map schema MapTypeWOA(oa1 OATypeInMap, oa2 OATypeInMap[]);\n";
            env.CompileDeployWBusPublicType(schema, path);

            env.CompileDeploy(
                "@Name('s0') select oa1.p0 as c0, oa1.p1 as c1, oa2[0].p0 as c2, oa2[1].p1 as c3 from MapTypeWOA",
                path);
            env.AddListener("s0");

            IDictionary<string, object> data = new Dictionary<string, object>();
            data.Put(
                "oa1",
                new object[] {"A", 100});
            data.Put(
                "oa2",
                new[] {
                    new object[] {"B", 200}, new object[] {"C", 300}
                });
            env.SendEventMap(data, "MapTypeWOA");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0,c1,c2,c3".SplitCsv(),
                new object[] {"A", 100, "B", 300});
            env.UndeployModuleContaining("s0");

            // test inserting from map to array
            env.CompileDeploy("@Name('s0') insert into OATypeInMap select 'a' as p0, 1 as p1 from MapTypeWOA", path)
                .AddListener("s0");
            env.SendEventMap(data, "MapTypeWOA");
            Assert.IsTrue(env.Listener("s0").AssertOneGetNew() is ObjectArrayBackedEventBean);
            Assert.AreEqual("a", env.Listener("s0").AssertOneGetNew().Get("p0"));
            Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("p1"));

            env.UndeployAll();
        }
    }
} // end of namespace