///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.@event.render;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventRender
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestEventRender()
        {
            RegressionRunner.Run(session, EventRender.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEventRenderJSON()
        {
            RegressionRunner.Run(session, EventRenderJSON.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEventRenderXML()
        {
            RegressionRunner.Run(session, EventRenderXML.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{typeof(SupportBean), typeof(EventRender.MyRendererEvent),
                typeof(SupportBeanRendererOne), typeof(SupportBeanRendererThree),
                typeof(EventRenderJSON.EmptyMapEvent)})
            {
                configuration.Common.AddEventType(clazz);
            }

            string[] props = { "P0", "P1", "P2", "P3", "P4" };
            object[] types = { typeof(string), typeof(int), typeof(SupportBean_S0), typeof(long), typeof(double?) };
            configuration.Common.AddEventType("MyObjectArrayType", props, types);

            IDictionary<string, object> outerMap = new LinkedHashMap<string, object>();
            outerMap.Put("intarr", typeof(int[]));
            outerMap.Put("innersimple", "InnerMap");
            outerMap.Put("innerarray", "InnerMap[]");
            outerMap.Put("prop0", typeof(SupportBean_A));

            IDictionary<string, object> innerMap = new LinkedHashMap<string, object>();
            innerMap.Put("stringarr", typeof(string[]));
            innerMap.Put("prop1", typeof(string));

            configuration.Common.AddEventType("InnerMap", innerMap);
            configuration.Common.AddEventType("OuterMap", outerMap);

            configuration.Compiler.ViewResources.IterableUnbound = true;
        }
    }
} // end of namespace