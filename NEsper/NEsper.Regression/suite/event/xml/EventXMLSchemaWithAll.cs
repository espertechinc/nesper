///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaWithAll : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // url='page4'
            var text = "@Name('s0') select a.url as sesja from pattern [ every a=PageVisitEvent(url='page1') ]";
            env.CompileDeploy(text).AddListener("s0");

            SupportXML.SendXMLEvent(
                env,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<event-page-visit xmlns=\"samples:schemas:simpleSchemaWithAll\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"samples:schemas:simpleSchemaWithAll simpleSchemaWithAll.xsd\">\n" +
                "<url>page1</url>" +
                "</event-page-visit>",
                "PageVisitEvent");
            var theEvent = env.Listener("s0").LastNewData[0];
            Assert.AreEqual("page1", theEvent.Get("sesja"));
            env.Listener("s0").Reset();

            SupportXML.SendXMLEvent(
                env,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<event-page-visit xmlns=\"samples:schemas:simpleSchemaWithAll\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"samples:schemas:simpleSchemaWithAll simpleSchemaWithAll.xsd\">\n" +
                "<url>page2</url>" +
                "</event-page-visit>",
                "PageVisitEvent");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            var type = env.CompileDeploy("@Name('s1') select * from PageVisitEvent").Statement("s1").EventType;
            CollectionAssert.AreEquivalent(
                new EventPropertyDescriptor[] {
                    new EventPropertyDescriptor("sessionId", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("customerId", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("url", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("method", typeof(XmlNode), null, false, false, false, false, true)
                },
                type.PropertyDescriptors);

            env.UndeployAll();
        }
    }
} // end of namespace