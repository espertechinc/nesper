///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaWithRestriction : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var text = "@Name('s0') select order_amount from OrderEvent";
            env.CompileDeploy(text).AddListener("s0");

            SupportXML.SendXMLEvent(
                env,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<order>\n" +
                "<order_amount>202.1</order_amount>" +
                "</order>",
                "OrderEvent");
            var theEvent = env.Listener("s0").LastNewData[0];
            Assert.AreEqual(typeof(double), theEvent.Get("order_amount").GetType());
            Assert.AreEqual(202.1d, theEvent.Get("order_amount"));
            env.Listener("s0").Reset();

            env.UndeployAll();
        }
    }
} // end of namespace