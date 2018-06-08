///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaNamespaceXPathRelative : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "getQuote";
            desc.DefaultNamespace = "http://services.samples/xsd";
            desc.RootElementNamespace = "http://services.samples/xsd";
            desc.AddNamespacePrefix("m0", "http://services.samples/xsd");
            desc.IsXPathResolvePropertiesAbsolute = false;
            desc.IsXPathPropertyExpr = true;
            configuration.AddEventType("StockQuote", desc);
        }
    
        public override void Run(EPServiceProvider epService) {
    
            var stmt = "select request.symbol as symbol_a, symbol as symbol_b from StockQuote";
            var joinView = epService.EPAdministrator.CreateEPL(stmt);
            var listener = new SupportUpdateListener();
            joinView.Events += listener.Update;
    
            var xml = "<m0:getQuote xmlns:m0=\"http://services.samples/xsd\"><m0:request><m0:symbol>IBM</m0:symbol></m0:request></m0:getQuote>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
    
            epService.EPRuntime.SendEvent(doc);
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("IBM", theEvent.Get("symbol_a"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_b"));
        }
    }
} // end of namespace
