///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.objectarray
{
    public class EventObjectArrayNestedMap : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            Assert.AreEqual(
                typeof(object[]),
                env.Runtime.EventTypeService.GetEventTypePreconfigured("MyMapNestedObjectArray").UnderlyingType);
            env.CompileDeploy("@Name('s0') select lev0name.lev1name.sb.TheString as val from MyMapNestedObjectArray")
                .AddListener("s0");

            IDictionary<string, object> lev2data = new Dictionary<string, object>();
            lev2data.Put("sb", new SupportBean("E1", 0));
            IDictionary<string, object> lev1data = new Dictionary<string, object>();
            lev1data.Put("lev1name", lev2data);

            env.SendEventObjectArray(new object[] {lev1data}, "MyMapNestedObjectArray");
            Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

            try {
                env.SendEventMap(new Dictionary<string, object>(), "MyMapNestedObjectArray");
                Assert.Fail();
            }
            catch (EPException ex) {
                Assert.AreEqual(
                    "Event type named 'MyMapNestedObjectArray' has not been defined or is not a Map-type event type, the name 'MyMapNestedObjectArray' refers to a System.Object(Array) event type",
                    ex.Message);
            }

            env.UndeployAll();
        }
    }
} // end of namespace