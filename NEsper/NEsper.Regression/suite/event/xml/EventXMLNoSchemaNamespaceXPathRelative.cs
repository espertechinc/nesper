///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaNamespaceXPathRelative : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmt = "@Name('s0') select request.symbol as symbol_a, symbol as symbol_b from StockQuoteSimpleConfig";
            env.CompileDeploy(stmt).AddListener("s0");

            var xml =
                "<m0:getQuote xmlns:m0=\"http://services.samples/xsd\"><m0:request><m0:symbol>IBM</m0:symbol></m0:request></m0:getQuote>";
            SendXMLEvent(env, xml, "StockQuoteSimpleConfig");

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("IBM", theEvent.Get("symbol_a"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_b"));

            env.UndeployAll();
        }
    }
} // end of namespace