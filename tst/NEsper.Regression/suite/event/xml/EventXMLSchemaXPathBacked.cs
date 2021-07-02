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

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaXPathBacked
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaXPathBackedCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
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
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSimpleSchema = resourceManager.ResolveResourceURL("regression/simpleSchema.xsd");
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='simpleEvent', SchemaResource='" +
                          schemaUriSimpleSchema +
                          "', XPathPropertyExpr=true)" +
                          "@XMLSchemaNamespacePrefix(Prefix='ss', Namespace='samples:schemas:simpleSchema')" +
                          "@XMLSchemaField(Name='customProp', XPath='count(/ss:simpleEvent/ss:nested3/ss:nested4)', Type='number')" +
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
            var stmtSelectWild = "@Name('s0') select * from " + typeName;
            env.CompileDeploy(stmtSelectWild, path).AddListener("s0");
            var type = env.Statement("s0").EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(type);

            SupportEventPropUtil.AssertPropsEquals(
                type.PropertyDescriptors.ToArray(),
                new SupportEventPropDesc("nested1", typeof(XmlNode)).WithFragment(!xpath),
                new SupportEventPropDesc("prop4", typeof(string)).WithIndexed(false).WithComponentType(typeof(char)),
                new SupportEventPropDesc("nested3", typeof(XmlNode)).WithFragment(!xpath),
                new SupportEventPropDesc("customProp", typeof(double?)));
            env.UndeployModuleContaining("s0");

            var stmt = "@Name('s0') select nested1 as nodeProp," +
                       "prop4 as nested1Prop," +
                       "nested1.prop2 as nested2Prop," +
                       "nested3.nested4('a').prop5[1] as complexProp," +
                       "nested1.nested2.prop3[2] as indexedProp," +
                       "customProp," +
                       "prop4.attr2 as attrOneProp," +
                       "nested3.nested4[2].id as attrTwoProp" +
                       " from " +
                       typeName +
                       "#length(100)";

            env.CompileDeploy(stmt, path).AddListener("s0");
            type = env.Statement("s0").EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(type);

            SupportEventPropUtil.AssertPropsEquals(
                type.PropertyDescriptors.ToArray(),
                new SupportEventPropDesc("nodeProp", typeof(XmlNode)).WithFragment(!xpath),
                new SupportEventPropDesc("nested1Prop", typeof(string)).WithComponentType(typeof(char)),
                new SupportEventPropDesc("nested2Prop", typeof(bool?)),
                new SupportEventPropDesc("complexProp", typeof(string)).WithComponentType(typeof(char)),
                new SupportEventPropDesc("indexedProp", typeof(int?)),
                new SupportEventPropDesc("customProp", typeof(double?)),
                new SupportEventPropDesc("attrOneProp", typeof(bool?)),
                new SupportEventPropDesc("attrTwoProp", typeof(string)).WithComponentType(typeof(char))
                );

            var eventDoc = SupportXML.SendDefaultEvent(env.EventService, "test", typeName);

            Assert.IsNotNull(env.Listener("s0").LastNewData);
            var theEvent = env.Listener("s0").LastNewData[0];

            Assert.AreSame(eventDoc.DocumentElement.ChildNodes.Item(0), theEvent.Get("nodeProp"));
            Assert.AreEqual("SAMPLE_V6", theEvent.Get("nested1Prop"));
            Assert.AreEqual(true, theEvent.Get("nested2Prop"));
            Assert.AreEqual("SAMPLE_V8", theEvent.Get("complexProp"));
            Assert.AreEqual(5, theEvent.Get("indexedProp"));
            Assert.AreEqual(3.0, theEvent.Get("customProp"));
            Assert.AreEqual(true, theEvent.Get("attrOneProp"));
            Assert.AreEqual("c", theEvent.Get("attrTwoProp"));

            env.UndeployAll();
        }
    }
} // end of namespace