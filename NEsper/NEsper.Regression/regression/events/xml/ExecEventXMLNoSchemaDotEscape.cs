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

using static com.espertech.esper.regression.events.xml.ExecEventXMLNoSchemaNestedXMLDOMGetter;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaDotEscape : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myroot";
            configuration.AddEventType("AEvent", xmlDOMEventTypeDesc);
        }
    
        public override void Run(EPServiceProvider epService) {
            var updateListener = new SupportUpdateListener();
    
            string stmt = "select a\\.b.c\\.d as val from AEvent";
            EPStatement joinView = epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += updateListener.Update;
    
            SendXMLEvent(epService, "<myroot><a.b><c.d>value</c.d></a.b></myroot>");
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("value", theEvent.Get("val"));
        }
    }
} // end of namespace
