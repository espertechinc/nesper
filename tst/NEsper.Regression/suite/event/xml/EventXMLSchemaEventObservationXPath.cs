///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.regressionlib.suite.@event.xml.EventXMLSchemaEventObservationDOM;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventObservationXPath
    {
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
            execs.Add(new EventXMLSchemaEventObservationXPathCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventObservationXPathPreconfig());
            return execs;
        }

        public class EventXMLSchemaEventObservationXPathPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "SensorEventWithXPath", new RegressionPath());
            }
        }

        public class EventXMLSchemaEventObservationXPathCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSensorEvent = resourceManager.ResolveResourceURL("regression/sensorSchema.xsd");
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='Sensor', SchemaResource='" +
                          schemaUriSensorEvent +
                          "')" +
                          "@XMLSchemaNamespacePrefix(Prefix='ss', Namespace='SensorSchema')" +
                          "@XMLSchemaField(Name='countTags', XPath='count(/ss:Sensor/ss:Observation/ss:Tag)', Type='number')" +
                          "@XMLSchemaField(Name='countTagsInt', XPath='count(/ss:Sensor/ss:Observation/ss:Tag)', Type='number', CastToType='int')" +
                          "@XMLSchemaField(Name='idarray', XPath='//ss:Tag/ss:ID', Type='NODESET', CastToType='String[]')" +
                          "@XMLSchemaField(Name='tagArray', XPath='//ss:Tag', Type='NODESET', EventTypeName='TagEvent')" +
                          "@XMLSchemaField(Name='tagOne', XPath='//ss:Tag[position() = 1]', Type='any', EventTypeName='TagEvent')" +
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
                "@name('s0') select countTags, countTagsInt, idarray, tagArray, tagOne from " + eventTypeName,
                path);
            env.CompileDeploy(
                "@name('e0') @public insert into TagOneStream select tagOne.* from " + eventTypeName,
                path);
            env.CompileDeploy("@name('e1') select ID from TagOneStream", path);
            env.CompileDeploy(
                "@name('e2') @public insert into TagArrayStream select tagArray as mytags from " + eventTypeName,
                path);
            env.CompileDeploy("@name('e3') select mytags[1].ID from TagArrayStream", path);

            var doc = SupportXML.GetDocument(OBSERVATION_XML);
            env.SendEventXMLDOM(doc, eventTypeName);

            env.Milestone(0);

            env.AssertIterator("s0", en => SupportEventTypeAssertionUtil.AssertConsistency(en.Advance()));
            env.AssertIterator("e0", en => SupportEventTypeAssertionUtil.AssertConsistency(en.Advance()));
            env.AssertIterator("e1", en => SupportEventTypeAssertionUtil.AssertConsistency(en.Advance()));
            env.AssertIterator("e2", en => SupportEventTypeAssertionUtil.AssertConsistency(en.Advance()));
            env.AssertIterator("e3", en => SupportEventTypeAssertionUtil.AssertConsistency(en.Advance()));

            env.AssertIterator(
                "s0",
                en => {
                    Assert.That(en, Is.Not.Null);
                    Assert.That(en.MoveNext(), Is.True);
                    Assert.That(en.Current, Is.Not.Null);
                });

            env.AssertIterator(
                "s0",
                en => {
                    var resultArray = en.Advance().Get("idarray");
                    EPAssertionUtil.AssertEqualsExactOrder(
                        (object[])resultArray,
                        new[] { "urn:epc:1:2.24.400", "urn:epc:1:2.24.401" });
                    EPAssertionUtil.AssertProps(
                        env.GetEnumerator("s0").Advance(),
                        new[] { "countTags", "countTagsInt" },
                        new object[] { 2d, 2 });
                    ClassicAssert.AreEqual("urn:epc:1:2.24.400", env.GetEnumerator("e1").Advance().Get("ID"));
                    ClassicAssert.AreEqual("urn:epc:1:2.24.401", env.GetEnumerator("e3").Advance().Get("mytags[1].ID"));
                });

            env.UndeployAll();
        }
    }
} // end of namespace