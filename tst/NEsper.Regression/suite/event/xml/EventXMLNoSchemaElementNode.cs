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
    public class EventXMLNoSchemaElementNode
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaElementNodeCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaElementNodePreconfig());
            return execs;
        }

        public class EventXMLNoSchemaElementNodePreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "MyEvent", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaElementNodeCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Public @buseventtype " +
                          "@XMLSchema(RootElementName='batch-event')" +
                          "@XMLSchemaField(Name='event.type', XPath='//event/@type', Type='string')" +
                          "@XMLSchemaField(Name='event.uid', XPath='//event/@uid', Type='string')" +
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
            var stmt = "@name('s0') select event.type as type, event.uid as uid from " + eventTypeName;
            env.CompileDeploy(stmt, path).AddListener("s0");

            var xml = "<batch-event>" +
                      "<event type=\"a-f-G\" uid=\"terminal.55\" time=\"2007-04-19T13:05:20.22Z\" version=\"2.0\"/>" +
                      "</batch-event>";
            SendXMLEvent(env, xml, eventTypeName);

            env.AssertEventNew(
                "s0",
                theEvent => {
                    Assert.AreEqual("a-f-G", theEvent.Get("type"));
                    Assert.AreEqual("terminal.55", theEvent.Get("uid"));
                });

            env.UndeployAll();
        }
    }
} // end of namespace