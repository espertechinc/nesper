///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapNestedConfigStatic : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertion(env, new RegressionPath());
        }

        internal static void RunAssertion(
            RegressionEnvironment env,
            RegressionPath path)
        {
            var statementText = "@name('s0') select" +
                                " nested as a," +
                                " nested.n1 as b," +
                                " nested.n2 as c," +
                                " nested.n2.n1n1 as d " +
                                " from NestedMapWithSimpleProps#length(5)";
            env.CompileDeploy(statementText, path).AddListener("s0");

            var mapEvent = GetTestData();
            env.SendEventMap(mapEvent, "NestedMapWithSimpleProps");

            env.AssertEventNew(
                "s0",
                theEvent => {
                    var mapEventNested = mapEvent.Get("nested");
                    Assert.That(mapEventNested, Is.InstanceOf<IDictionary<string, object>>());
                    Assert.That(theEvent.Get("a"), Is.SameAs(mapEventNested));
                    Assert.That(theEvent.Get("b"), Is.SameAs("abc"));

                    var mapEventNestedMap = (IDictionary<string, object>)mapEventNested;
                    Assert.That(theEvent.Get("c"), Is.SameAs(mapEventNestedMap.Get("n2")));
                    Assert.That(theEvent.Get("d"), Is.SameAs("def"));
                });

            env.UndeployAll();
        }

        private static IDictionary<string, object> GetTestData()
        {
            IDictionary<string, object> nestedNested = new Dictionary<string, object>();
            nestedNested.Put("n1n1", "def");

            IDictionary<string, object> nested = new Dictionary<string, object>();
            nested.Put("n1", "abc");
            nested.Put("n2", nestedNested);

            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("nested", nested);

            return map;
        }
    }
} // end of namespace