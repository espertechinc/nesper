///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.suite.@event.map;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventMapWConfig
    {
        [Test, RunInApplicationDomain]
        public void TestEventMapNestedConfigRuntime()
        {
            RegressionSession session = RegressionRunner.Session();
            RegressionRunner.Run(session, new EventMapNestedConfigRuntime());
            session.Destroy();
        }

        [Test, RunInApplicationDomain]
        public void TestEventMapInheritanceRuntime()
        {
            RegressionSession session = RegressionRunner.Session();
            RegressionRunner.Run(session, new EventMapInheritanceRuntime());
            session.Destroy();
        }

        [Test, RunInApplicationDomain]
        public void TestInvalidConfig()
        {
            // supertype not found
            TryInvalidConfigure(
                config => {
                    ConfigurationCommonEventTypeMap map = new ConfigurationCommonEventTypeMap();
                    map.SuperTypes = Collections.SingletonSet("NONE");
                    config.Common.AddEventType("InvalidMap", Collections.EmptyDataMap, map);
                },
                "Supertype by name 'NONE' could not be found");

            // invalid property
            TryInvalidConfigure(
                config => { config.Common.AddEventType("InvalidMap", Collections.SingletonDataMap("key", "XXX")); },
                "Nestable type configuration encountered an unexpected property type name 'XXX' for property 'key', expected Type or Dictionary or the name of a previously-declared event type");

            // invalid key
#if NOT_VALID
            IDictionary<string, object> invalid = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {new int?(5), null}
                });
            TryInvalidConfigure(
                config => { config.Common.AddEventType("InvalidMap", invalid); },
                GetCastMessage(typeof(int?), typeof(string)));
#endif

            IDictionary<string, object> invalidTwo = EventMapCore.MakeMap(
                new object[][] {
                    new object[] {"abc", new SupportBean()}
                });
            TryInvalidConfigure(
                config => { config.Common.AddEventType("InvalidMap", invalidTwo); },
                "Nestable type configuration encountered an unexpected property type name 'SupportBean(null, 0)' for property 'abc', expected Type or Dictionary or the name of a previously-declared event type");
        }

        private void TryInvalidConfigure(
            Consumer<Configuration> configurer,
            string expected)
        {
            SupportMessageAssertUtil.TryInvalidConfigurationCompileAndRuntime(
                SupportConfigFactory.GetConfiguration(),
                configurer,
                expected);
        }

        public static string GetCastMessage(
            Type from,
            Type to)
        {
            return from.CleanName() + " cannot be cast to " + to.CleanName();
        }
    }
} // end of namespace