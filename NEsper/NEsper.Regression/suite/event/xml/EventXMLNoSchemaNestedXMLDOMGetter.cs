///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaNestedXMLDOMGetter
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaNestedXMLDOMGetterPreconfig());
            execs.Add(new EventXMLNoSchemaNestedXMLDOMGetterCreateSchema());
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
                          "@XMLSchema(rootElementName='a')" +
                          "@XMLSchemaField(name='element1', xpath='/a/b/c', type='string')" +
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
            var stmt = "@name('s0') select b.c as type, element1, result1 from " + eventTypeName;
            env.CompileDeploy(stmt, path).AddListener("s0");

            SendXMLEvent(env, "<a><b><c></c></b></a>", eventTypeName);
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("", theEvent.Get("type"));
            Assert.AreEqual("", theEvent.Get("element1"));

            SendXMLEvent(env, "<a><b></b></a>", eventTypeName);
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.IsNull(theEvent.Get("type"));
            Assert.IsNull(theEvent.Get("element1"));

            SendXMLEvent(env, "<a><b><c>text</c></b></a>", eventTypeName);
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("text", theEvent.Get("type"));
            Assert.AreEqual("text", theEvent.Get("element1"));

            env.UndeployAll();
        }
    }
} // end of namespace