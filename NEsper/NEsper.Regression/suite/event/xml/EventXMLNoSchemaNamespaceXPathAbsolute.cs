///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaNamespaceXPathAbsolute
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaNamespaceXPathAbsolutePreconfig());
            execs.Add(new EventXMLNoSchemaNamespaceXPathAbsoluteCreateSchema());
            return execs;
        }

        public class EventXMLNoSchemaNamespaceXPathAbsolutePreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "StockQuote", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaNamespaceXPathAbsoluteCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='getQuote', DefaultNamespace='http://services.samples/xsd', RootElementNamespace='http://services.samples/xsd'," +
                          "  XPathResolvePropertiesAbsolute=true, XPathPropertyExpr=true)" +
                          "@XMLSchemaNamespacePrefix(Prefix='m0', Namespace='http://services.samples/xsd')" +
                          "@XMLSchemaField(Name='symbol_a', XPath='//m0:symbol', Type='string')" +
                          "@XMLSchemaField(Name='symbol_b', XPath='//*[local-name(.) = \"getQuote\" and namespace-uri(.) = \"http://services.samples/xsd\"]', Type='string')" +
                          "@XMLSchemaField(Name='symbol_c', XPath='/m0:getQuote/m0:request/m0:symbol', Type='string')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeName,
            RegressionPath path)
        {
            var epl = "@Name('s0') select symbol_a, symbol_b, symbol_c, request.symbol as symbol_d, symbol as symbol_e from " + eventTypeName;
            env.CompileDeploy(epl, path).AddListener("s0");

            var xml = "<m0:getQuote xmlns:m0=\"http://services.samples/xsd\"><m0:request><m0:symbol>IBM</m0:symbol></m0:request></m0:getQuote>";
            SendXMLEvent(env, xml, eventTypeName);

            // For XPath resolution testing and namespaces...

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("IBM", theEvent.Get("symbol_a"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_b"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_c"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_d"));
            Assert.IsNull(theEvent.Get("symbol_e")); // should be empty as we are doing absolute XPath

            env.UndeployAll();
        }
    }
} // end of namespace