///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML; // getDocument
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventSender
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
            execs.Add(new EventXMLSchemaEventSenderCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventSenderPreconfig());
            return execs;
        }

        public class EventXMLSchemaEventSenderPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "EventABC", "BEvent", new RegressionPath());
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EVENTSENDER);
            }
        }

        public class EventXMLSchemaEventSenderCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Public @buseventtype " +
                          "@XMLSchema(RootElementName='a')" +
                          "@XMLSchemaField(Name='element1', XPath='/a/b/c', Type='string')" +
                          "create xml schema MyEventCreateSchemaABC();\n" +
                          "" +
                          "@public @buseventtype " +
                          "@XMLSchema(RootElementName='a', EventSenderValidatesRoot=false)" +
                          "@XMLSchemaField(Name='element2', XPath='//c', Type='string')" +
                          "create xml schema MyEventCreateSchemaB()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchemaABC", "MyEventCreateSchemaB", path);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EVENTSENDER);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeNameABC,
            string eventTypeNameB,
            RegressionPath path)
        {
            var stmtText = "@name('s0') select b.c as type, element1 from " + eventTypeNameABC;
            env.CompileDeploy(stmtText, path).AddListener("s0");

            var doc = GetDocument("<a><b><c>text</c></b></a>");
            var sender = env.EventService.GetEventSender(eventTypeNameABC);
            sender.SendEvent(doc);

            env.AssertEventNew(
                "s0",
                theEvent => {
                    ClassicAssert.AreEqual("text", theEvent.Get("type"));
                    ClassicAssert.AreEqual("text", theEvent.Get("element1"));
                });

            // send wrong event
            try {
                sender.SendEvent(GetDocument("<xxxx><b><c>text</c></b></xxxx>"));
                Assert.Fail();
            }
            catch (EPException ex) {
                ClassicAssert.AreEqual(
                    "Unexpected root element name 'xxxx' encountered, expected a root element name of 'a'",
                    ex.Message);
            }

            try {
                sender.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (EPException ex) {
                ClassicAssert.AreEqual(
                    "Unexpected event object type '" +
                    typeof(SupportBean).FullName +
                    "' encountered, please supply a XmlDocument or XmlElement node",
                    ex.Message);
            }

            env.UndeployModuleContaining("s0");

            // test adding a second type for the same root element
            stmtText = "@name('s0') select element2 from " + eventTypeNameB + "#lastevent";
            env.CompileDeploy(stmtText, path).AddListener("s0");

            // test sender that doesn't care about the root element
            var senderTwo = env.EventService.GetEventSender(eventTypeNameB);
            senderTwo.SendEvent(GetDocument("<xxxx><b><c>text</c></b></xxxx>")); // allowed, not checking

            env.AssertIterator(
                "s0",
                iterator => {
                    var theEvent = iterator.Advance();
                    ClassicAssert.AreEqual("text", theEvent.Get("element2"));
                });

            env.UndeployAll();
        }
    }
} // end of namespace