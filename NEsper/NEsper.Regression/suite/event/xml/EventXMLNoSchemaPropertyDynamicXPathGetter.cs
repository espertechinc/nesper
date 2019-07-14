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
    public class EventXMLNoSchemaPropertyDynamicXPathGetter : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmtText = "@Name('s0') select type?,dyn[1]?,nested.nes2?,map('a')?,other? from MyEventWXPathExprTrue";
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
                    new EventPropertyDescriptor("map('a')?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("other?", typeof(XmlNode), null, false, false, false, false, false)
                },
                env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            var root = SupportXML.SendXMLEvent(
                env,
                EventXMLNoSchemaPropertyDynamicDOMGetter.NOSCHEMA_XML,
                "MyEventWXPathExprTrue");
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(1), theEvent.Get("type?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(5), theEvent.Get("dyn[1]?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(7).ChildNodes.Item(1), theEvent.Get("nested.nes2?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(9), theEvent.Get("map('a')?"));
            Assert.IsNull(theEvent.Get("other?"));
            SupportEventTypeAssertionUtil.AssertConsistency(theEvent);

            env.UndeployAll();
        }
    }
} // end of namespace