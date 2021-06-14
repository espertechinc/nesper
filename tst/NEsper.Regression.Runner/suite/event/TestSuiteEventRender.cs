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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

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
            session.Dispose();
            session = null;
        }

        /// <summary>
        /// Auto-test(s): EventRenderJSON
        /// <code>
        /// RegressionRunner.Run(_session, EventRenderJSON.Executions());
        /// </code>
        /// </summary>

        public class TestEventRenderJSON : AbstractTestBase
        {
            public TestEventRenderJSON() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithJsonEventType() => RegressionRunner.Run(_session, EventRenderJSON.WithJsonEventType());

            [Test, RunInApplicationDomain]
            public void WithEnquote() => RegressionRunner.Run(_session, EventRenderJSON.WithEnquote());

            [Test, RunInApplicationDomain]
            public void WithEmptyMap() => RegressionRunner.Run(_session, EventRenderJSON.WithEmptyMap());

            [Test, RunInApplicationDomain]
            public void WithMapAndNestedArray() => RegressionRunner.Run(_session, EventRenderJSON.WithMapAndNestedArray());

            [Test, RunInApplicationDomain]
            public void WithRenderSimple() => RegressionRunner.Run(_session, EventRenderJSON.WithRenderSimple());
        }

        /// <summary>
        /// Auto-test(s): EventRender
        /// <code>
        /// RegressionRunner.Run(_session, EventRender.Executions());
        /// </code>
        /// </summary>

        public class TestEventRender : AbstractTestBase
        {
            public TestEventRender() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithPONOMap() => RegressionRunner.Run(_session, EventRender.WithPONOMap());

            [Test, RunInApplicationDomain]
            public void WithObjectArray() => RegressionRunner.Run(_session, EventRender.WithObjectArray());

            [Test, RunInApplicationDomain]
            public void WithPropertyCustomRenderer() => RegressionRunner.Run(_session, EventRender.WithPropertyCustomRenderer());
        }
        
        /// <summary>
        /// Auto-test(s): EventRenderXML
        /// <code>
        /// RegressionRunner.Run(_session, EventRenderXML.Executions());
        /// </code>
        /// </summary>

        public class TestEventRenderXML : AbstractTestBase
        {
            public TestEventRenderXML() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithEnquote() => RegressionRunner.Run(_session, EventRenderXML.WithEnquote());

            [Test, RunInApplicationDomain]
            public void WithSQLDate() => RegressionRunner.Run(_session, EventRenderXML.WithSQLDate());

            [Test, RunInApplicationDomain]
            public void WithMapAndNestedArray() => RegressionRunner.Run(_session, EventRenderXML.WithMapAndNestedArray());

            [Test, RunInApplicationDomain]
            public void WithRenderSimple() => RegressionRunner.Run(_session, EventRenderXML.WithRenderSimple());
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