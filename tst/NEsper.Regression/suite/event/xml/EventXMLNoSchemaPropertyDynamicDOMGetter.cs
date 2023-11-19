///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaPropertyDynamicDOMGetter
    {
        public const string NOSCHEMA_XML = "<simpleEvent>\n" +
                                           "\t<type>abc</type>\n" +
                                           "\t<dyn>1</dyn>\n" +
                                           "\t<dyn>2</dyn>\n" +
                                           "\t<nested>\n" +
                                           "\t\t<nes2>3</nes2>\n" +
                                           "\t</nested>\n" +
                                           "\t<map Id='a'>4</map>\n" +
                                           "</simpleEvent>";

        public static IList<RegressionExecution> Executions()
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
            execs.Add(new EventXMLNoSchemaPropertyDynamicDOMGetterCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaPropertyDynamicDOMGetterPreconfig());
            return execs;
        }

        public class EventXMLNoSchemaPropertyDynamicDOMGetterPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "MyEventSimpleEvent", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaPropertyDynamicDOMGetterCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='simpleEvent')" +
                          " create xml schema MyEventCreateSchema as ()";
                var path = new RegressionPath();
                env.EplToModelCompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeName,
            RegressionPath path)
        {
            var stmtText = "@name('s0') select type?,dyn[1]?,nested.nes2?,map('a')? from " + eventTypeName;
            env.CompileDeploy(stmtText, path).AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("type?", typeof(XmlNode)),
                        new SupportEventPropDesc("dyn[1]?", typeof(XmlNode)),
                        new SupportEventPropDesc("Nested.nes2?", typeof(XmlNode)),
                        new SupportEventPropDesc("map('a')?", typeof(XmlNode)));
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            var root = SupportXML.SendXMLEvent(env, NOSCHEMA_XML, eventTypeName);
            env.AssertEventNew(
                "s0",
                theEvent => {
                    Assert.AreSame(root.DocumentElement.ChildNodes.Item(0), theEvent.Get("type?"));
                    Assert.AreSame(root.DocumentElement.ChildNodes.Item(2), theEvent.Get("dyn[1]?"));
                    Assert.AreSame(
                        root.DocumentElement.ChildNodes.Item(3).ChildNodes.Item(0),
                        theEvent.Get("Nested.nes2?"));
                    Assert.AreSame(root.DocumentElement.ChildNodes.Item(4), theEvent.Get("map('a')?"));
                    SupportEventTypeAssertionUtil.AssertConsistency(theEvent);
                });

            env.UndeployAll();
        }
    }
} // end of namespace