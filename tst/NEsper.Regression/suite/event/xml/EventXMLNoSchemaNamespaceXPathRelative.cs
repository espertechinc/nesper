///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaNamespaceXPathRelative
    {
        private ConfigurationCommonEventTypeXMLDOM stockQuoteSimpleConfig = new ConfigurationCommonEventTypeXMLDOM();

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithPreconfig(execs);
            With(CreateSchema)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaNamespaceXPathRelativeCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaNamespaceXPathRelativePreconfig());
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
                    "@XMLSchema(RootElementName='getQuote', DefaultNamespace='http://services.samples/xsd', RootElementNamespace='http://services.samples/xsd', XPathResolvePropertiesAbsolute=false," +
                    "  XPathPropertyExpr=true)" +
                    "@XMLSchemaNamespacePrefix(Prefix='m0', Namespace='http://services.samples/xsd')" +
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
            var stmt = "@name('s0') select request.Symbol as symbol_a, Symbol as symbol_b from " + eventTypeName;
            env.CompileDeploy(stmt, path).AddListener("s0");

            var xml =
                "<m0:getQuote xmlns:m0=\"http://services.samples/xsd\"><m0:request><m0:Symbol>IBM</m0:Symbol></m0:request></m0:getQuote>";
            SendXMLEvent(env, xml, eventTypeName);

            env.AssertEventNew(
                "s0",
                theEvent => {
                    ClassicAssert.AreEqual("IBM", theEvent.Get("symbol_a"));
                    ClassicAssert.AreEqual("IBM", theEvent.Get("symbol_b"));
                });

            env.UndeployAll();
        }
    }
} // end of namespace