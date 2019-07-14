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
    public class EventXMLNoSchemaNestedXMLDOMGetter : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmt = "@Name('s0') select b.c as type, element1, result1 from AEventWithXPath";
            env.CompileDeploy(stmt).AddListener("s0");

            SendXMLEvent(env, "<a><b><c></c></b></a>", "AEventWithXPath");
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("", theEvent.Get("type"));
            Assert.AreEqual("", theEvent.Get("element1"));

            SendXMLEvent(env, "<a><b></b></a>", "AEventWithXPath");
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("type"));
            Assert.AreEqual("", theEvent.Get("element1"));

            SendXMLEvent(env, "<a><b><c>text</c></b></a>", "AEventWithXPath");
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("text", theEvent.Get("type"));
            Assert.AreEqual("text", theEvent.Get("element1"));

            env.UndeployAll();
        }
    }
} // end of namespace