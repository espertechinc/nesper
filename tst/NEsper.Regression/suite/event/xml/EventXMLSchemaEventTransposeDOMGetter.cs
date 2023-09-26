///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeDOMGetter
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
            execs.Add(new EventXMLSchemaEventTransposeDOMGetterCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeDOMGetterPreconfig());
            return execs;
        }

        public class EventXMLSchemaEventTransposeDOMGetterPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "SimpleEventWSchema", new RegressionPath());
            }
        }

        public class EventXMLSchemaEventTransposeDOMGetterCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var schemaUriSimpleSchema = env.Container.ResourceManager()
                    .ResolveResourceURL("regression/simpleSchema.xsd")
                    .ToString();
                var epl = "@Public @buseventtype " +
                          "@XMLSchema(rootElementName='simpleEvent', schemaResource='" +
                          schemaUriSimpleSchema +
                          "')" +
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
            env.CompileDeploy(
                "@name('s0') @public insert into MyNestedStream select nested1 from " + eventTypeName + "#lastevent",
                path);
            env.AssertStatement(
                "s0",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("nested1", typeof(XmlNode)).WithFragment());
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            env.CompileDeploy(
                "@name('s1') select nested1.attr1 as attr1, nested1.prop1 as prop1, nested1.prop2 as prop2, nested1.nested2.prop3 as prop3, nested1.nested2.prop3[0] as prop3_0, nested1.nested2 as nested2 from MyNestedStream#lastevent",
                path);
            env.AssertStatement(
                "s1",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("prop1", typeof(string)),
                        new SupportEventPropDesc("prop2", typeof(bool?)),
                        new SupportEventPropDesc("attr1", typeof(string)),
                        new SupportEventPropDesc("prop3", typeof(int?[])).WithIndexed(),
                        new SupportEventPropDesc("prop3_0", typeof(int?)),
                        new SupportEventPropDesc("nested2", typeof(XmlNode)).WithFragment());
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            env.CompileDeploy("@name('sw') select * from MyNestedStream", path);
            env.AssertStatement(
                "sw",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("nested1", typeof(XmlNode)).WithFragment());
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            env.CompileDeploy(
                "@name('iw') insert into MyNestedStreamTwo select nested1.* from " + eventTypeName + "#lastevent",
                path);
            env.AssertStatement(
                "iw",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("prop1", typeof(string)),
                        new SupportEventPropDesc("prop2", typeof(bool?)),
                        new SupportEventPropDesc("attr1", typeof(string)),
                        new SupportEventPropDesc("nested2", typeof(XmlNode)).WithFragment());
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            var doc = SupportXML.MakeDefaultEvent("test");
            env.SendEventXMLDOM(doc, eventTypeName);

            env.AssertIterator(
                "iw",
                iterator => {
                    var stmtInsertWildcardBean = iterator.Advance();
                    SupportEventTypeAssertionUtil.AssertConsistency(stmtInsertWildcardBean);
                    EPAssertionUtil.AssertProps(
                        stmtInsertWildcardBean,
                        "prop1,prop2,attr1".SplitCsv(),
                        new object[] { "SAMPLE_V1", true, "SAMPLE_ATTR1" });

                    var fragmentNested2 = (EventBean)stmtInsertWildcardBean.GetFragment("nested2");
                    Assert.AreEqual(4, fragmentNested2.Get("prop3[1]"));
                    Assert.AreEqual(eventTypeName + ".nested1.nested2", fragmentNested2.EventType.Name);
                });

            env.AssertIterator(
                "s0",
                iterator => {
                    var stmtInsertBean = iterator.Advance();
                    SupportEventTypeAssertionUtil.AssertConsistency(stmtInsertBean);
                    var fragmentNested1 = (EventBean)stmtInsertBean.GetFragment("nested1");
                    Assert.AreEqual(5, fragmentNested1.Get("nested2.prop3[2]"));
                    Assert.AreEqual(eventTypeName + ".nested1", fragmentNested1.EventType.Name);
                });
            env.AssertIterator("sw", iterator => SupportEventTypeAssertionUtil.AssertConsistency(iterator.Advance()));

            env.UndeployAll();
        }
    }
} // end of namespace