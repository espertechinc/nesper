///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaXPathArray : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string xml = "<Event IsTriggering=\"True\">\n" +
                    "<Field Name=\"A\" Value=\"987654321\"/>\n" +
                    "<Field Name=\"B\" Value=\"2196958725202\"/>\n" +
                    "<Field Name=\"C\" Value=\"1232363702\"/>\n" +
                    "<Participants>\n" +
                    "<Participant>\n" +
                    "<Field Name=\"A\" Value=\"9876543210\"/>\n" +
                    "<Field Name=\"B\" Value=\"966607340\"/>\n" +
                    "<Field Name=\"D\" Value=\"353263010930650\"/>\n" +
                    "</Participant>\n" +
                    "</Participants>\n" +
                    "</Event>";
    
            var desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "Event";
            desc.AddXPathProperty("A", "//Field[@Name='A']/@Value", XPathResultType.NodeSet, "string[]");
            epService.EPAdministrator.Configuration.AddEventType("Event", desc);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from Event");
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            XmlDocument doc = SupportXML.GetDocument(xml);
            epService.EPRuntime.SendEvent(doc);
    
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, "A".Split(','), new object[]{new object[]{"987654321", "9876543210"}});
        }
    }
} // end of namespace
