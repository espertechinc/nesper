///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml.XPath;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventXMLWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestInvalidConfig()
        {
            var runtimeProvider = new EPRuntimeProvider();
            
            ConfigurationCommonEventTypeXMLDOM desc = new ConfigurationCommonEventTypeXMLDOM();
            desc.RootElementName = "ABC";
            desc.StartTimestampPropertyName = "mystarttimestamp";
            desc.EndTimestampPropertyName = "myendtimestamp";
            desc.AddXPathProperty("mystarttimestamp", "/test/prop", XPathResultType.Number);

            TryInvalidConfigurationCompileAndRuntime(
                runtimeProvider,
                SupportConfigFactory.GetConfiguration(Container),
                config => config.Common.AddEventType("TypeXML", desc),
                "Declared start timestamp property 'mystarttimestamp' is expected to return a DateTimeEx, DateTime, DateTimeOffset or long-typed value but returns 'System.Nullable<System.Double>'");
        }
    }
} // end of namespace