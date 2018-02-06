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
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaNamespaceXPathAbsolute : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var desc = new ConfigurationEventTypeXMLDOM();
            desc.AddXPathProperty("symbol_a", "//m0:symbol", XPathResultType.String);
            desc.AddXPathProperty("symbol_b", "//*[local-name(.) = 'getQuote' and namespace-uri(.) = 'http://services.samples/xsd']", XPathResultType.String);
            desc.AddXPathProperty("symbol_c", "/m0:getQuote/m0:request/m0:symbol", XPathResultType.String);
            desc.RootElementName = "getQuote";
            desc.DefaultNamespace = "http://services.samples/xsd";
            desc.RootElementNamespace = "http://services.samples/xsd";
            desc.AddNamespacePrefix("m0", "http://services.samples/xsd");
            desc.IsXPathResolvePropertiesAbsolute = true;
            desc.IsXPathPropertyExpr = true;
            configuration.AddEventType("StockQuote", desc);
        }
    
        public override void Run(EPServiceProvider epService) {
            var epl = "select symbol_a, symbol_b, symbol_c, request.symbol as symbol_d, symbol as symbol_e from StockQuote";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var xml = "<m0:getQuote xmlns:m0=\"http://services.samples/xsd\">" +
                      "<m0:request>" +
                      "<m0:symbol>IBM</m0:symbol>" +
                      "</m0:request>" +
                      "</m0:getQuote>";
            //string xml = "<getQuote><request><symbol>IBM</symbol></request></getQuote>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // For XPath resolution testing and namespaces...
            /*
            XPathFactory xPathFactory = XPathFactory.NewInstance();
            XPath xPath = xPathFactory.NewXPath();
            var ctx = new XPathNamespaceContext();
            ctx.AddPrefix("m0", "http://services.samples/xsd");
            xPath.NamespaceContext = ctx;
            XPathExpression expression = xPath.Compile("/m0:getQuote/m0:request/m0:symbol");
            xPath.NamespaceContext = ctx;
            Log.Info("result=" + expression.Evaluate(doc,XPathResultType.String));
            */

            epService.EPRuntime.SendEvent(doc);
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("IBM", theEvent.Get("symbol_a"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_b"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_c"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_d"));
            Assert.AreEqual(null, theEvent.Get("symbol_e"));    // should be empty string as we are doing absolute XPath
        }
    }
} // end of namespace
