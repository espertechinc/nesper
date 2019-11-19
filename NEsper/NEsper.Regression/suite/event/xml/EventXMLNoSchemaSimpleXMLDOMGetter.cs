///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaSimpleXMLDOMGetter : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmt = "@Name('s0') select " +
                       "element1, " +
                       "invalidelement, " +
                       "element4.element41 as nestedElement," +
                       "element2.element21('e21_2') as mappedElement," +
                       "element2.element21[1] as indexedElement," +
                       "element3.myattribute as invalidattribute " +
                       "from TestXMLNoSchemaType#length(100)";
            env.CompileDeploy(stmt).AddListener("s0");

            // Generate document with the specified in element1 to confirm we have independent events
            EventXMLNoSchemaSimpleXMLXPathProperties.SendEvent(env, "EventA", "TestXMLNoSchemaType");
            AssertDataGetter(env, "EventA", false);

            EventXMLNoSchemaSimpleXMLXPathProperties.SendEvent(env, "EventB", "TestXMLNoSchemaType");
            AssertDataGetter(env, "EventB", false);

            env.UndeployAll();
        }

        internal static void AssertDataGetter(
            RegressionEnvironment env,
            string element1,
            bool isInvalidReturnsEmptyString)
        {
            Assert.IsNotNull(env.Listener("s0").LastNewData);
            var theEvent = env.Listener("s0").LastNewData[0];

            Assert.AreEqual(element1, theEvent.Get("element1"));
            Assert.AreEqual("VAL4-1", theEvent.Get("nestedElement"));
            Assert.AreEqual("VAL21-2", theEvent.Get("mappedElement"));
            Assert.AreEqual("VAL21-2", theEvent.Get("indexedElement"));

#if true
            Assert.AreEqual(null, theEvent.Get("invalidelement"));
            Assert.AreEqual(null, theEvent.Get("invalidattribute"));
#else
            if (isInvalidReturnsEmptyString) {
                Assert.AreEqual("", theEvent.Get("invalidelement"));
                Assert.AreEqual("", theEvent.Get("invalidattribute"));
            }
            else {
                Assert.AreEqual(null, theEvent.Get("invalidelement"));
                Assert.AreEqual(null, theEvent.Get("invalidattribute"));
            }
#endif
        }
    }
} // end of namespace