///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML; // sendXMLEvent
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaNestedXMLDOMGetter
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaNestedXMLDOMGetterCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaNestedXMLDOMGetterPreconfig());
            return execs;
        }

        public class EventXMLNoSchemaNestedXMLDOMGetterPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "AEventWithXPath", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaNestedXMLDOMGetterCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='a')" +
                          "@XMLSchemaField(Name='element1', XPath='/a/b/c', Type='string')" +
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
            var stmt = "@name('s0') select b.c as type, element1, result1 from " + eventTypeName;
            env.CompileDeploy(stmt, path).AddListener("s0");

            SendXMLEvent(env, "<a><b><c></c></b></a>", eventTypeName);
            env.AssertEventNew(
                "s0",
                theEvent => {
                    ClassicAssert.AreEqual("", theEvent.Get("type"));
                    ClassicAssert.AreEqual("", theEvent.Get("element1"));
                });

            SendXMLEvent(env, "<a><b></b></a>", eventTypeName);
            env.AssertEventNew(
                "s0",
                theEvent => {
                    ClassicAssert.IsNull(theEvent.Get("type"));
                    ClassicAssert.IsNull(theEvent.Get("element1"));
                });

            SendXMLEvent(env, "<a><b><c>text</c></b></a>", eventTypeName);
            env.AssertEventNew(
                "s0",
                theEvent => {
                    ClassicAssert.AreEqual("text", theEvent.Get("type"));
                    ClassicAssert.AreEqual("text", theEvent.Get("element1"));
                });

            env.UndeployAll();
        }
    }
} // end of namespace