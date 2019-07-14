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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaPropertyDynamicDOMGetter : RegressionExecution
    {
        internal const string SCHEMA_XML = "<simpleEvent xmlns=\"samples:schemas:simpleSchema\" \n" +
                                           "  xmlns:ss=\"samples:schemas:simpleSchema\" \n" +
                                           "  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" \n" +
                                           "  xsi:schemaLocation=\"samples:schemas:simpleSchema simpleSchema.xsd\">" +
                                           "<type>abc</type>\n" +
                                           "<dyn>1</dyn>\n" +
                                           "<dyn>2</dyn>\n" +
                                           "<nested>\n" +
                                           "<nes2>3</nes2>\n" +
                                           "</nested>\n" +
                                           "<map id='a'>4</map>\n" +
                                           "</simpleEvent>";

        public void Run(RegressionEnvironment env)
        {
            var stmtText = "@Name('s0') select type?,dyn[1]?,nested.nes2?,map('a')? from MyEventWithPrefix";
            env.CompileDeploy(stmtText).AddListener("s0");

            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("type?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("dyn[1]?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor(
                        "nested.nes2?",
                        typeof(XmlNode),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor("map('a')?", typeof(XmlNode), null, false, false, false, false, false)
                },
                env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            var sender = env.EventService.GetEventSender("MyEventWithPrefix");
            var root = SupportXML.SendEvent(sender, SCHEMA_XML);

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(0), theEvent.Get("type?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(4), theEvent.Get("dyn[1]?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(6).ChildNodes.Item(1), theEvent.Get("nested.nes2?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(8), theEvent.Get("map('a')?"));
            SupportEventTypeAssertionUtil.AssertConsistency(theEvent);

            env.UndeployAll();
        }
    }
} // end of namespace