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
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaXPathBacked
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
            execs.Add(new EventXMLSchemaXPathBackedCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaXPathBackedPreconfig());
            return execs;
        }

        public class EventXMLSchemaXPathBackedPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, true, "XMLSchemaConfigOne", new RegressionPath());
            }
        }

        public class EventXMLSchemaXPathBackedCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var schemaUriSimpleSchema = env.Container.ResourceManager()
                    .ResolveResourceURL("regression/simpleSchema.xsd")
                    .ToString();
                var epl = "@public @buseventtype " +
                          "@XMLSchema(rootElementName='simpleEvent', schemaResource='" +
                          schemaUriSimpleSchema +
                          "', xpathPropertyExpr=true)" +
                          "@XMLSchemaNamespacePrefix(prefix='ss', namespace='samples:schemas:simpleSchema')" +
                          "@XMLSchemaField(name='customProp', xpath='count(/ss:simpleEvent/ss:nested3/ss:nested4)', type='number')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, true, "MyEventCreateSchema", path);
            }
        }

        internal static void RunAssertion(
            RegressionEnvironment env,
            bool xpath,
            string typeName,
            RegressionPath path)
        {
            var stmtSelectWild = "@name('s0') select * from " + typeName;
            env.CompileDeploy(stmtSelectWild, path).AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => {
                    var type = statement.EventType;
                    SupportEventTypeAssertionUtil.AssertConsistency(type);

                    SupportEventPropUtil.AssertPropsEquals(
                        type.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("Nested1", typeof(XmlNode)).WithFragment(!xpath),
                        new SupportEventPropDesc("prop4", typeof(string)),
                        new SupportEventPropDesc("Nested3", typeof(XmlNode)).WithFragment(!xpath),
                        new SupportEventPropDesc("customProp", typeof(double?)));
                });
            env.UndeployModuleContaining("s0");

            var stmt = "@name('s0') select nested1 as nodeProp," +
                       "prop4 as nested1Prop," +
                       "Nested1.prop2 as nested2Prop," +
                       "Nested3.Nested4('a').prop5[1] as complexProp," +
                       "Nested1.Nested2.prop3[2] as indexedProp," +
                       "customProp," +
                       "prop4.attr2 as attrOneProp," +
                       "Nested3.Nested4[2].Id as attrTwoProp" +
                       " from " +
                       typeName +
                       "#length(100)";

            env.CompileDeploy(stmt, path).AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => {
                    var type = statement.EventType;
                    SupportEventTypeAssertionUtil.AssertConsistency(type);
                    SupportEventPropUtil.AssertPropsEquals(
                        type.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("nodeProp", typeof(XmlNode)).WithFragment(!xpath),
                        new SupportEventPropDesc("Nested1Prop", typeof(string)),
                        new SupportEventPropDesc("Nested2Prop", typeof(bool?)),
                        new SupportEventPropDesc("complexProp", typeof(string)),
                        new SupportEventPropDesc("indexedProp", typeof(int?)),
                        new SupportEventPropDesc("customProp", typeof(double?)),
                        new SupportEventPropDesc("attrOneProp", typeof(bool?)),
                        new SupportEventPropDesc("attrTwoProp", typeof(string)));
                });

            var doc = SupportXML.MakeDefaultEvent("test");
            env.SendEventXMLDOM(doc, typeName);

            env.AssertListener(
                "s0",
                listener => {
                    Assert.IsNotNull(listener.LastNewData);
                    var theEvent = listener.LastNewData[0];

                    Assert.AreSame(doc.DocumentElement.ChildNodes.Item(1), theEvent.Get("nodeProp"));
                    Assert.AreEqual("SAMPLE_V6", theEvent.Get("Nested1Prop"));
                    Assert.AreEqual(true, theEvent.Get("Nested2Prop"));
                    Assert.AreEqual("SAMPLE_V8", theEvent.Get("complexProp"));
                    Assert.AreEqual(5, theEvent.Get("indexedProp"));
                    Assert.AreEqual(3.0, theEvent.Get("customProp"));
                    Assert.AreEqual(true, theEvent.Get("attrOneProp"));
                    Assert.AreEqual("c", theEvent.Get("attrTwoProp"));
                });

            /// <summary>
            /// Comment-in for performance testing
            /// long start = System.nanoTime();
            /// {
            /// sendEvent("test");
            /// }
            /// long end = System.nanoTime();
            /// double delta = (end - start) / 1000d / 1000d / 1000d;
            /// Console.WriteLine(delta);
            /// </summary>

            env.UndeployAll();
        }
    }
} // end of namespace