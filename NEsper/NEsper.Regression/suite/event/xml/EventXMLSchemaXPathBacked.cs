///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
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
            execs.Add(new EventXMLSchemaXPathBackedPreconfig());
            execs.Add(new EventXMLSchemaXPathBackedCreateSchema());
            return execs;
        }

        public class EventXMLSchemaXPathBackedPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, true, "XMLSchemaConfigOne", new RegressionPath());
            }
        }

        public class EventXMLSchemaXPathBackedCreateSchema : RegressionExecution {
            public void Run (RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSimpleSchema = resourceManager.GetResourceAsStream("regression/simpleSchema.xsd").ConsumeStream();
                var epl = "@public @buseventtype " +
                             "@XMLSchema(rootElementName='simpleEvent', schemaResource='" + schemaUriSimpleSchema + "', xpathPropertyExpr=true)" +
                             "@XMLSchemaNamespacePrefix(prefix='ss', namespace='samples:schemas:simpleSchema')" +
                             "@XMLSchemaField(name='customProp', xpath='count(/ss:simpleEvent/ss:nested3/ss:nested4)', type='number')" +
                             "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath ();
                env.CompileDeploy (epl, path);
                RunAssertion (env, true, "MyEventCreateSchema", path);
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

            CollectionAssert.AreEquivalent(
                new EventPropertyDescriptor[] {
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, !xpath),
                    new EventPropertyDescriptor("prop4", typeof(string), typeof(char), false, false, false, false, false),
                    new EventPropertyDescriptor("nested3", typeof(XmlNode), null, false, false, false, false, !xpath),
                    new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false)
                },
                type.PropertyDescriptors);
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
            CollectionAssert.AreEquivalent(
                new EventPropertyDescriptor[] {
                    new EventPropertyDescriptor("nodeProp", typeof(XmlNode), null, false, false, false, false, !xpath),
                    new EventPropertyDescriptor("nested1Prop", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("nested2Prop", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("complexProp", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("indexedProp", typeof(int?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attrOneProp", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attrTwoProp", typeof(string), typeof(char), false, false, true, false, false)
                },
                type.PropertyDescriptors);

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