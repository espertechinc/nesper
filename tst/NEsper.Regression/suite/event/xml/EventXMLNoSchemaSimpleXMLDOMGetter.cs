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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaSimpleXMLDOMGetter
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
            execs.Add(new EventXMLNoSchemaSimpleXMLDOMGetterCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaSimpleXMLDOMGetterPreconfig());
            return execs;
        }

        public class EventXMLNoSchemaSimpleXMLDOMGetterPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "TestXMLNoSchemaType", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaSimpleXMLDOMGetterCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='myevent', XPathPropertyExpr=false)" +
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
            var stmt = "@name('s0') select " +
                       "element1, " +
                       "invalidelement, " +
                       "element4.element41 as nestedElement," +
                       "element2.element21('e21_2') as mappedElement," +
                       "element2.element21[1] as indexedElement," +
                       "element3.myattribute as invalidattribute " +
                       "from " +
                       eventTypeName +
                       "#length(100)";
            env.CompileDeploy(stmt, path).AddListener("s0");

            // Generate document with the specified in element1 to confirm we have independent events
            EventXMLNoSchemaSimpleXMLXPathProperties.SendEvent(env, "EventA", eventTypeName);
            AssertDataGetter(env, "EventA", false);

            EventXMLNoSchemaSimpleXMLXPathProperties.SendEvent(env, "EventB", eventTypeName);
            AssertDataGetter(env, "EventB", false);

            env.UndeployAll();
        }

        internal static void AssertDataGetter(
            RegressionEnvironment env,
            string element1,
            bool isInvalidReturnsEmptyString)
        {
            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.IsNotNull(listener.LastNewData);
                    var theEvent = listener.LastNewData[0];

                    ClassicAssert.AreEqual(element1, theEvent.Get("element1"));
                    ClassicAssert.AreEqual("VAL4-1", theEvent.Get("nestedElement"));
                    ClassicAssert.AreEqual("VAL21-2", theEvent.Get("mappedElement"));
                    ClassicAssert.AreEqual("VAL21-2", theEvent.Get("indexedElement"));

#if true
                    ClassicAssert.AreEqual(null, theEvent.Get("invalidelement"));
                    ClassicAssert.AreEqual(null, theEvent.Get("invalidattribute"));
#else
                    if (isInvalidReturnsEmptyString) {
                        ClassicAssert.AreEqual("", theEvent.Get("invalidelement"));
                        ClassicAssert.AreEqual("", theEvent.Get("invalidattribute"));
                    }
                    else {
                        ClassicAssert.AreEqual(null, theEvent.Get("invalidelement"));
                        ClassicAssert.AreEqual(null, theEvent.Get("invalidattribute"));
                    }
#endif
                });
        }
    }
} // end of namespace