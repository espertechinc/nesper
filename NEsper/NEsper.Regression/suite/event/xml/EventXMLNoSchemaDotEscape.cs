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
    public class EventXMLNoSchemaDotEscape : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmt = "@Name('s0') select a\\.b.c\\.d as val from AEvent";
            env.CompileDeploy(stmt).AddListener("s0");

            SendXMLEvent(env, "<myroot><a.b><c.d>value</c.d></a.b></myroot>", "AEvent");
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("value", theEvent.Get("val"));

            env.UndeployAll();
        }
    }
} // end of namespace