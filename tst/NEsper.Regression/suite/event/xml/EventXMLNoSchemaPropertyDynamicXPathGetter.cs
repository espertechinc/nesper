///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Xml;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaPropertyDynamicXPathGetter
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
            execs.Add(new EventXMLNoSchemaPropertyDynamicXPathGetterCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaPropertyDynamicXPathGetterPreconfig());
            return execs;
        }

        public class EventXMLNoSchemaPropertyDynamicXPathGetterPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "MyEventWXPathExprTrue", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaPropertyDynamicXPathGetterCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='simpleEvent', XPathPropertyExpr=true)" +
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
            var stmtText = "@name('s0') select type?,dyn[1]?,nested.nes2?,map('a')?,other? from " + eventTypeName;
            env.CompileDeploy(stmtText, path).AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("type?", typeof(XmlNode)),
                        new SupportEventPropDesc("dyn[1]?", typeof(XmlNode)),
                        new SupportEventPropDesc("nested.nes2?", typeof(XmlNode)),
                        new SupportEventPropDesc("map('a')?", typeof(XmlNode)),
                        new SupportEventPropDesc("other?", typeof(XmlNode)));
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            var root = SupportXML.SendXMLEvent(
                env,
                EventXMLNoSchemaPropertyDynamicDOMGetter.NOSCHEMA_XML,
                eventTypeName);
            env.AssertEventNew(
                "s0",
                theEvent => {
                    ClassicAssert.AreEqual(root.DocumentElement.ChildNodes.Item(0), theEvent.Get("type?"));
                    ClassicAssert.AreEqual(root.DocumentElement.ChildNodes.Item(2), theEvent.Get("dyn[1]?"));
                    ClassicAssert.AreEqual(
                        root.DocumentElement.ChildNodes.Item(3).ChildNodes.Item(0),
                        theEvent.Get("nested.nes2?"));
                    ClassicAssert.AreEqual(root.DocumentElement.ChildNodes.Item(4), theEvent.Get("map('a')?"));
                    ClassicAssert.IsNull(theEvent.Get("other?"));
                    SupportEventTypeAssertionUtil.AssertConsistency(theEvent);
                });

            env.UndeployAll();
        }
    }
} // end of namespace