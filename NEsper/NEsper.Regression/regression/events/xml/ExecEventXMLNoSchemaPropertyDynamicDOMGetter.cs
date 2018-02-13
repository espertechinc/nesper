///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util.support;

// using static org.junit.Assert.assertSame;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaPropertyDynamicDOMGetter : RegressionExecution {
        internal static readonly string NOSCHEMA_XML = "<simpleEvent>\n" +
                "\t<type>abc</type>\n" +
                "\t<dyn>1</dyn>\n" +
                "\t<dyn>2</dyn>\n" +
                "\t<nested>\n" +
                "\t\t<nes2>3</nes2>\n" +
                "\t</nested>\n" +
                "\t<map id='a'>4</map>\n" +
                "</simpleEvent>";
    
        public override void Configure(Configuration configuration) {
            var desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "simpleEvent";
            configuration.AddEventType("MyEvent", desc);
        }
    
        public override void Run(EPServiceProvider epService) {
    
            string stmtText = "select type?,dyn[1]?,nested.nes2?,Map('a')? from MyEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[]{
                    new EventPropertyDescriptor("type?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("dyn[1]?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("nested.nes2?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("Map('a')?", typeof(XmlNode), null, false, false, false, false, false),
            }, stmt.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmt.EventType);
    
            XmlDocument root = SupportXML.SendEvent(epService.EPRuntime, NOSCHEMA_XML);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(1), theEvent.Get("type?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(5), theEvent.Get("dyn[1]?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(7).ChildNodes.Item(1), theEvent.Get("nested.nes2?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(9), theEvent.Get("Map('a')?"));
            SupportEventTypeAssertionUtil.AssertConsistency(theEvent);
        }
    }
} // end of namespace
