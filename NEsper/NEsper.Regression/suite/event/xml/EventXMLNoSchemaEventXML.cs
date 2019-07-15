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
    public class EventXMLNoSchemaEventXML : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmt = "@Name('s0') select event.type as type, event.uId as uId from MyEventWTypeAndUID";
            env.CompileDeploy(stmt).AddListener("s0");

            SendXMLEvent(
                env,
                "<event type=\"a-f-G\" uId=\"terminal.55\" time=\"2007-04-19T13:05:20.22Z\" version=\"2.0\"></event>",
                "MyEventWTypeAndUID");
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("a-f-G", theEvent.Get("type"));
            Assert.AreEqual("terminal.55", theEvent.Get("uId"));

            env.UndeployAll();
        }
    }
} // end of namespace