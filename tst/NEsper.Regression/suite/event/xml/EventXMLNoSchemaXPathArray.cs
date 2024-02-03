///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaXPathArray
    {
        public static List<RegressionExecution> Executions()
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
            execs.Add(new EventXMLNoSchemaXPathArrayCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaXPathArrayPreconfig());
            return execs;
        }

        public class EventXMLNoSchemaXPathArrayPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "Event", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaXPathArrayCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='Event')" +
                          "@XMLSchemaField(Name='A', XPath='//Field[@Name=\"A\"]/@Value', Type='nodeset', CastToType='string[]')" +
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

            env.CompileDeploy("@name('s0') select * from " + eventTypeName, path).AddListener("s0");

            SendXMLEvent(env, xml, eventTypeName);

            env.AssertPropsNew(
                "s0",
                new[] { "A" },
                new object[] { new object[] { "987654321", "9876543210" } });

            env.UndeployAll();
        }
    }
} // end of namespace