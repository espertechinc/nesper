///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.suite.@event.xml.EventXMLSchemaEventObservationDOM;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventObservationXPath : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@Name('s0') select countTags, countTagsInt, idarray, tagArray, tagOne from SensorEventWithXPath",
                path);
            env.CompileDeploy("@Name('e0') insert into TagOneStream select tagOne.* from SensorEventWithXPath", path);
            env.CompileDeploy("@Name('e1') select ID from TagOneStream", path);
            env.CompileDeploy(
                "@Name('e2') insert into TagArrayStream select tagArray as mytags from SensorEventWithXPath",
                path);
            env.CompileDeploy("@Name('e3') select mytags[1].ID from TagArrayStream", path);

            var doc = SupportXML.GetDocument(OBSERVATION_XML);
            env.SendEventXMLDOM(doc, "SensorEventWithXPath");

            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("s0").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e0").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e1").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e2").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("e3").Advance());

            var resultArray = env.GetEnumerator("s0").Advance().Get("idarray");
            EPAssertionUtil.AssertEqualsExactOrder(
                (object[]) resultArray,
                new[] {"urn:epc:1:2.24.400", "urn:epc:1:2.24.401"});
            EPAssertionUtil.AssertProps(
                env.GetEnumerator("s0").Advance(),
                new [] { "countTags","countTagsInt" },
                new object[] {2d, 2});
            Assert.AreEqual("urn:epc:1:2.24.400", env.GetEnumerator("e1").Advance().Get("ID"));
            Assert.AreEqual("urn:epc:1:2.24.401", env.GetEnumerator("e3").Advance().Get("mytags[1].ID"));

            env.UndeployAll();
        }
    }
} // end of namespace