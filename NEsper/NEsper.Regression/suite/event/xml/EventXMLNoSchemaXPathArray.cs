///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaXPathArray : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var xml = "<Event IsTriggering=\"True\">\n" +
                      "<Field Name=\"A\" Value=\"987654321\"/>\n" +
                      "<Field Name=\"B\" Value=\"2196958725202\"/>\n" +
                      "<Field Name=\"C\" Value=\"1232363702\"/>\n" +
                      "<Participants>\n" +
                      "<Participant>\n" +
                      "<Field Name=\"A\" Value=\"9876543210\"/>\n" +
                      "<Field Name=\"B\" Value=\"966607340\"/>\n" +
                      "<Field Name=\"D\" Value=\"353263010930650\"/>\n" +
                      "</Participant>\n" +
                      "</Participants>\n" +
                      "</Event>";

            env.CompileDeploy("@Name('s0') select * from Event").AddListener("s0");

            SendXMLEvent(env, xml, "Event");

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                theEvent,
                "A".SplitCsv(),
                new object[] {new object[] {"987654321", "9876543210"}});

            env.UndeployAll();
        }
    }
} // end of namespace