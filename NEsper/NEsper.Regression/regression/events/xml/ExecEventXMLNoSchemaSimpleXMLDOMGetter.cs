///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.events.xml.ExecEventXMLNoSchemaSimpleXMLXPathProperties;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaSimpleXMLDOMGetter : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            xmlDOMEventTypeDesc.IsXPathPropertyExpr = false;    // <== DOM getter
            configuration.AddEventType("TestXMLNoSchemaType", xmlDOMEventTypeDesc);
        }
    
        public override void Run(EPServiceProvider epService) {
            string stmt =
                    "select element1, invalidelement, " +
                            "element4.element41 as nestedElement," +
                            "element2.element21('e21_2') as mappedElement," +
                            "element2.element21[1] as indexedElement," +
                            "element3.myattribute as invalidattribute " +
                            "from TestXMLNoSchemaType#length(100)";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(stmt);
            var updateListener = new SupportUpdateListener();
            joinView.Events += updateListener.Update;
    
            // Generate document with the specified in element1 to confirm we have independent events
            SendEvent(epService, "EventA");
            AssertDataGetter(updateListener, "EventA", false);
    
            SendEvent(epService, "EventB");
            AssertDataGetter(updateListener, "EventB", false);
        }
    
        internal static void AssertDataGetter(SupportUpdateListener updateListener, string element1, bool isInvalidReturnsEmptyString) {
            Assert.IsNotNull(updateListener.LastNewData);
            EventBean theEvent = updateListener.LastNewData[0];
    
            Assert.AreEqual(element1, theEvent.Get("element1"));
            Assert.AreEqual("VAL4-1", theEvent.Get("nestedElement"));
            Assert.AreEqual("VAL21-2", theEvent.Get("mappedElement"));
            Assert.AreEqual("VAL21-2", theEvent.Get("indexedElement"));
    
            if (isInvalidReturnsEmptyString) {
                Assert.AreEqual("", theEvent.Get("invalidelement"));
                Assert.AreEqual("", theEvent.Get("invalidattribute"));
            } else {
                Assert.AreEqual(null, theEvent.Get("invalidelement"));
                Assert.AreEqual(null, theEvent.Get("invalidattribute"));
            }
        }
    }
    
    
    
} // end of namespace
