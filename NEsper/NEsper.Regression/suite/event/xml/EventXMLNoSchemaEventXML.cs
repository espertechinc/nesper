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
    public class EventXMLNoSchemaEventXML
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaEventXMLCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaEventXMLPreconfig());
            return execs;
        }

        public class EventXMLNoSchemaEventXMLPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select event.type as type, event.uid as uid from MyEventWTypeAndUID";
                RunAssertion(env, epl, "MyEventWTypeAndUID");
            }
        }

        public class EventXMLNoSchemaEventXMLCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='event')" +
                          "@XMLSchemaField(Name='event.type', XPath='/event/@type', Type='string')" +
                          "@XMLSchemaField(Name='event.uid', XPath='/event/@uid', Type='string')" +
                          "create xml schema MyEventCreateSchema();\n" +
                          "@Name('s0') select event.type as type, event.uid as uid from MyEventCreateSchema;\n";
                RunAssertion(env, epl, "MyEventCreateSchema");
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string epl,
            string eventTypeName)
        {
            env.CompileDeploy(epl).AddListener("s0");

            SendXMLEvent(env, "<event type=\"a-f-G\" uid=\"terminal.55\" time=\"2007-04-19T13:05:20.22Z\" version=\"2.0\"></event>", eventTypeName);

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("a-f-G", theEvent.Get("type"));
            Assert.AreEqual("terminal.55", theEvent.Get("uid"));

            env.UndeployAll();
        }
    }
} // end of namespace