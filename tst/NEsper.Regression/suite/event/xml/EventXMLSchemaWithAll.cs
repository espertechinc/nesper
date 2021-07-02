///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaWithAll
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaWithAllCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaWithAllPreconfig());
            return execs;
        }

        public class EventXMLSchemaWithAllPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "PageVisitEvent", new RegressionPath());
            }
        }

        public class EventXMLSchemaWithAllCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSimpleSchemaWithAll = resourceManager.ResolveResourceURL("regression/simpleSchemaWithAll.xsd");
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='event-page-visit', SchemaResource='" +
                          schemaUriSimpleSchemaWithAll +
                          "')" +
                          "@XMLSchemaNamespacePrefix(Prefix='ss', Namespace='samples:schemas:simpleSchemaWithAll')" +
                          "@XMLSchemaField(Name='url', XPath='/ss:event-page-visit/ss:url', Type='string')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            String eventTypeName,
            RegressionPath path)
        {
            // url='page4'
            var text = "@Name('s0') select a.url as sesja from pattern [ every a=" + eventTypeName + "(url='page1') ]";
            env.CompileDeploy(text, path).AddListener("s0");

            SupportXML.SendXMLEvent(
                env,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<event-page-visit xmlns=\"samples:schemas:simpleSchemaWithAll\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"samples:schemas:simpleSchemaWithAll simpleSchemaWithAll.xsd\">\n" +
                "<url>page1</url>" +
                "</event-page-visit>",
                eventTypeName);
            var theEvent = env.Listener("s0").LastNewData[0];
            Assert.AreEqual("page1", theEvent.Get("sesja"));
            env.Listener("s0").Reset();

            SupportXML.SendXMLEvent(
                env,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<event-page-visit xmlns=\"samples:schemas:simpleSchemaWithAll\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"samples:schemas:simpleSchemaWithAll simpleSchemaWithAll.xsd\">\n" +
                "<url>page2</url>" +
                "</event-page-visit>",
                eventTypeName);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            var type = env.CompileDeploy("@Name('s1') select * from " + eventTypeName, path).Statement("s1").EventType;
            SupportEventPropUtil.AssertPropsEquals(
                type.PropertyDescriptors.ToArray(),
                new SupportEventPropDesc("sessionId", typeof(XmlNode)).WithFragment(),
                new SupportEventPropDesc("customerId", typeof(XmlNode)).WithFragment(),
                new SupportEventPropDesc("url", typeof(string)).WithComponentType(typeof(char)).WithIndexed(false),
                new SupportEventPropDesc("method", typeof(XmlNode)).WithFragment());

            env.UndeployAll();
        }
    }
} // end of namespace