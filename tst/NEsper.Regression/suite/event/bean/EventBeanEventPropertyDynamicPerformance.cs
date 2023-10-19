///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanEventPropertyDynamicPerformance : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
        }

        public void Run(RegressionEnvironment env)
        {
            var stmtText = "@name('s0') select simpleProperty?, " +
                           "indexed[1]? as indexed, " +
                           "mapped('keyOne')? as mapped " +
                           "from SupportBeanComplexProps";
            env.CompileDeploy(stmtText).AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => {
                    var type = statement.EventType;
                    Assert.AreEqual(typeof(object), type.GetPropertyType("simpleProperty?"));
                    Assert.AreEqual(typeof(object), type.GetPropertyType("indexed"));
                    Assert.AreEqual(typeof(object), type.GetPropertyType("mapped"));
                });

            SupportBeanComplexProps inner = SupportBeanComplexProps.MakeDefaultBean();
            env.SendEventBean(inner);
            env.AssertEventNew(
                "s0",
                theEvent => {
                    Assert.AreEqual(inner.SimpleProperty, theEvent.Get("simpleProperty?"));
                    Assert.AreEqual(inner.GetIndexed(1), theEvent.Get("indexed"));
                    Assert.AreEqual(inner.GetMapped("keyOne"), theEvent.Get("mapped"));
                });

            var start = PerformanceObserver.MilliTime;
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(inner);
                if (i % 1000 == 0) {
                    env.ListenerReset("s0");
                }
            }

            var end = PerformanceObserver.MilliTime;
            var delta = end - start;
            Assert.That(delta, Is.LessThan(1000), "delta=" + delta);

            env.UndeployAll();
        }
    }
} // end of namespace