///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventVariantWConfig
    {
        [Test]
        public void TestInvalidConfig()
        {
            ConfigurationCommonVariantStream config = new ConfigurationCommonVariantStream();
            TryInvalidVarstream(config, "Failed compiler startup: Invalid variant stream configuration, no event type name has been added and default type variance requires at least one type, for name 'ABC'");

            config.AddEventTypeName("dummy");
            TryInvalidVarstream(config, "Failed compiler startup: Event type by name 'dummy' could not be found for use in variant stream configuration by name 'ABC'");
        }

        private void TryInvalidVarstream(ConfigurationCommonVariantStream config, string expected)
        {
            TryInvalidConfigurationCompiler(SupportConfigFactory.GetConfiguration(), configuration => configuration.Common.AddVariantStream("ABC", config), expected);
        }
    }
} // end of namespace