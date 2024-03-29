///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.objectarray
{
    public class EventObjectArrayConfiguredStatic : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.AssertThat(
                () => {
                    var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured("MyOAType");
                    ClassicAssert.AreEqual(typeof(object[]), eventType.UnderlyingType);
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("TheString"));
                    ClassicAssert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("map"));
                    ClassicAssert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("bean"));
                });

            env.CompileDeploy("@name('s0') select bean, TheString, map('key'), bean.TheString from MyOAType");
            env.AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => ClassicAssert.AreEqual(typeof(object[]), statement.EventType.UnderlyingType));

            var bean = new SupportBean("E1", 1);
            env.SendEventObjectArray(
                new object[] { bean, "abc", Collections.SingletonDataMap("key", "value") },
                "MyOAType");
            env.AssertPropsNew(
                "s0",
                new[] { "bean", "TheString", "map('key')", "bean.TheString" },
                new object[] { bean, "abc", "value", "E1" });

            env.UndeployAll();
        }
    }
} // end of namespace