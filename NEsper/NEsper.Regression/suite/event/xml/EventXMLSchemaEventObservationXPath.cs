///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.suite.@event.xml.EventXMLSchemaEventObservationDOM;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventObservationXPath
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventObservationXPathPreconfig());
            execs.Add(new EventXMLSchemaEventObservationXPathCreateSchema());
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
                var schemaUriSensorEvent = resourceManager.GetResourceAsStream("regression/sensorSchema.xsd").ConsumeStream();
                var epl = "@public @buseventtype " +
                          "@XMLSchema(rootElementName='Sensor', schemaResource='" +
                          schemaUriSensorEvent +
                          "')" +
                          "@XMLSchemaNamespacePrefix(prefix='ss', namespace='SensorSchema')" +
                          "@XMLSchemaField(name='countTags', xpath='count(/ss:Sensor/ss:Observation/ss:Tag)', type='number')" +
                          "@XMLSchemaField(name='countTagsInt', xpath='count(/ss:Sensor/ss:Observation/ss:Tag)', type='number', castToType='int')" +
                          "@XMLSchemaField(name='idarray', xpath='//ss:Tag/ss:ID', type='NODESET', castToType='String[]')" +
                          "@XMLSchemaField(name='tagArray', xpath='//ss:Tag', type='NODESET', eventTypeName='TagEvent')" +
                          "@XMLSchemaField(name='tagOne', xpath='//ss:Tag[position() = 1]', type='node', eventTypeName='TagEvent')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(RegressionEnvironment env, string eventTypeName, RegressionPath path)
        {
            env.CompileDeploy("@name('s0') select countTags, countTagsInt, idarray, tagArray, tagOne from " + eventTypeName, path);
            env.CompileDeploy("@name('e0') insert into TagOneStream select tagOne.* from " + eventTypeName, path);
            env.CompileDeploy("@name('e1') select ID from TagOneStream", path);
            env.CompileDeploy("@name('e2') insert into TagArrayStream select tagArray as mytags from " + eventTypeName, path);
            env.CompileDeploy("@name('e3') select mytags[1].ID from TagArrayStream", path);

            var doc = SupportXML.GetDocument(OBSERVATION_XML);
            env.SendEventXMLDOM(doc, eventTypeName);

            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("s0").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e0").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e1").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e2").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e3").Advance());

            var resultEnumerator = env.GetEnumerator("s0");
            Assert.That(resultEnumerator, Is.Not.Null);
            Assert.That(resultEnumerator.MoveNext(), Is.True);
            Assert.That(resultEnumerator.Current, Is.Not.Null);

            var resultArray = resultEnumerator.Current.Get("idarray");
            EPAssertionUtil.AssertEqualsExactOrder(
                (object[]) resultArray,
                new[] {"urn:epc:1:2.24.400", "urn:epc:1:2.24.401"});
            EPAssertionUtil.AssertProps(
                env.GetEnumerator("s0").Advance(),
                new [] { "countTags","countTagsInt" },
                new object[] { 2d, 2 });
            Assert.AreEqual("urn:epc:1:2.24.400", env.GetEnumerator("e1").Advance().Get("ID"));
            Assert.AreEqual("urn:epc:1:2.24.401", env.GetEnumerator("e3").Advance().Get("mytags[1].ID"));

            env.UndeployAll();
        }
    }
} // end of namespace