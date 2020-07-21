///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaNamespaceXPathRelative
    {
        private ConfigurationCommonEventTypeXMLDOM stockQuoteSimpleConfig = new ConfigurationCommonEventTypeXMLDOM();

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaNamespaceXPathRelativePreconfig());
            execs.Add(new EventXMLNoSchemaNamespaceXPathRelativeCreateSchema());
            return execs;
        }

        public class EventXMLNoSchemaNamespaceXPathRelativePreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "StockQuoteSimpleConfig", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaNamespaceXPathRelativeCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype " +
                    "@XMLSchema(rootElementName='getQuote', defaultNamespace='http://services.samples/xsd', rootElementNamespace='http://services.samples/xsd', xpathResolvePropertiesAbsolute=false," +
                    "  xpathPropertyExpr=true)" +
                    "@XMLSchemaNamespacePrefix(prefix='m0', namespace='http://services.samples/xsd')" +
                    "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            String eventTypeName,
            RegressionPath path)
        {
            var stmt = "@name('s0') select request.symbol as symbol_a, symbol as symbol_b from " + eventTypeName;
            env.CompileDeploy(stmt, path).AddListener("s0");

            var xml =
                "<m0:getQuote xmlns:m0=\"http://services.samples/xsd\"><m0:request><m0:symbol>IBM</m0:symbol></m0:request></m0:getQuote>";
            SendXMLEvent(env, xml, eventTypeName);

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("IBM", theEvent.Get("symbol_a"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_b"));

            env.UndeployAll();
        }
    }
} // end of namespace