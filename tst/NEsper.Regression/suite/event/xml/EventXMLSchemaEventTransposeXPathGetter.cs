///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeXPathGetter
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
            execs.Add(new EventXMLSchemaEventTransposeXPathGetterCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeXPathGetterPreconfig());
            return execs;
        }

        public class EventXMLSchemaEventTransposeXPathGetterPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "TestXMLSchemaTypeWithSS", new RegressionPath());
            }
        }

        public class EventXMLSchemaEventTransposeXPathGetterCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSimpleSchema = resourceManager.ResolveResourceURL("regression/simpleSchema.xsd");
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='simpleEvent', SchemaResource='" +
                          schemaUriSimpleSchema +
                          "', XPathPropertyExpr=true)" +
                          "@XMLSchemaNamespacePrefix(Prefix='ss', Namespace='samples:schemas:simpleSchema')" +
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
            // Note that XPath Node results when transposed must be queried by XPath that is also absolute.
            // For example: "nested1" -> "/n0:simpleEvent/n0:nested1" results in a Node.
            // That result Node's "prop1" ->  "/n0:simpleEvent/n0:nested1/n0:prop1" and "/n0:nested1/n0:prop1" does NOT result in a value.
            // Therefore property transposal is disabled for Property-XPath expressions.

            // note class not a fragment
            env.CompileDeploy(
                "@name('s0') insert into MyNestedStream select nested1 from " + eventTypeName + "#lastevent",
                path);
            env.AssertStatement(
                "s0",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("nested1", typeof(XmlNode)));
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            env.AssertThat(
                () => {
                    var type = env.Runtime.EventTypeService.GetEventTypePreconfigured(eventTypeName);
                    SupportEventTypeAssertionUtil.AssertConsistency(type);
                    Assert.IsNull(type.GetFragmentType("nested1"));
                    Assert.IsNull(type.GetFragmentType("nested1.Nested2"));
                });

            var doc = SupportXML.MakeDefaultEvent("ABC");
            env.SendEventXMLDOM(doc, eventTypeName);
            env.AssertIterator("s0", en => SupportEventTypeAssertionUtil.AssertConsistency(en.Advance()));

            env.UndeployAll();
        }
    }
} // end of namespace