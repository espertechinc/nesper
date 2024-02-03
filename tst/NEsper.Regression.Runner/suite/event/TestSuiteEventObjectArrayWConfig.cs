///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.@event.objectarray;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventObjectArrayWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestEventObjectArrayConfiguredStatic()
        {
            using var session = RegressionRunner.Session(Container);
            session.Configuration.Common.EventMeta.DefaultEventRepresentation = EventUnderlyingType.OBJECTARRAY;
            session.Configuration.Common.AddEventType(
                "MyOAType",
                new[] {"bean", "TheString", "map"},
                new object[] {typeof(SupportBean), "string", "map"});
            RegressionRunner.Run(session, new EventObjectArrayConfiguredStatic());
        }

        [Test, RunInApplicationDomain]
        public void TestEventObjectArrayInheritanceConfigRuntime()
        {
            using var session = RegressionRunner.Session(Container);
            RegressionRunner.Run(session, new EventObjectArrayInheritanceConfigRuntime());
        }

        [Test, RunInApplicationDomain]
        public void TestInvalidConfig()
        {
            // invalid multiple supertypes
            var invalidOAConfig = new ConfigurationCommonEventTypeObjectArray();
            invalidOAConfig.SuperTypes = Collections.Set("A", "B");
            string[] invalidOANames = {"P00"};
            object[] invalidOATypes = {typeof(int)};
            try {
                var configuration = SupportConfigFactory.GetConfiguration(Container);
                configuration.Common.AddEventType("MyInvalidEventTwo", invalidOANames, invalidOATypes, invalidOAConfig);
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                ClassicAssert.AreEqual("Object-array event types only allow a single supertype", ex.Message);
            }

            // mismatched property number
            try {
                var configuration = SupportConfigFactory.GetConfiguration(Container);
                configuration.Common.AddEventType(
                    "MyInvalidEvent",
                    new[] {"P00"},
                    new object[] {typeof(int), typeof(string)});
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                ClassicAssert.AreEqual(
                    "Number of property names and property types do not match, found 1 property names and 2 property types",
                    ex.Message);
            }
        }
    }
} // end of namespace