///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.epl.variable;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLVariableWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestEPLVariablesTimer()
        {
            using var session = RegressionRunner.Session(Container);
            var configuration = session.Configuration;
            configuration.Runtime.Threading.IsInternalTimerEnabled = true;
            configuration.Common.AddEventType(typeof(SupportBean));
            configuration.Common.AddVariable("var1", typeof(long), "12");
            configuration.Common.AddVariable("var2", typeof(long?), "2");
            configuration.Common.AddVariable("var3", typeof(long?), null);
            RegressionRunner.Run(session, new EPLVariablesTimer());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLVariableEngineConfigXML()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                      "<esper-configuration xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"../esper-configuration-6-0.xsd\">" +
                      "<common><variable name=\"p_1\" type=\"string\" />" +
                      "<variable name=\"p_2\" type=\"bool\" initialization-value=\"true\"/>" +
                      "<variable name=\"p_3\" type=\"long\" initialization-value=\"10\"/>" +
                      "<variable name=\"p_4\" type=\"double\" initialization-value=\"11.1d\"/>" +
                      "</common></esper-configuration>";
            var doc = SupportXML.GetDocument(xml);

            using var session = RegressionRunner.Session(Container);
            var configuration = session.Configuration;
            configuration.Common.AddEventType(typeof(SupportBean));
            configuration.Configure(doc);

            RegressionRunner.Run(session, new EPLVariableEngineConfigXML());
        }

        [Test, RunInApplicationDomain]
        public void TestInvalidConfig()
        {
            var runtimeProvider = new EPRuntimeProvider();

            TryInvalidConfigurationCompiler(
                SupportConfigFactory.GetConfiguration(Container),
                config => config.Common.AddVariable("invalidvar1", typeof(int?), "abc"),
#if NET7_0_OR_GREATER
                $"Failed compiler startup: Error configuring variable 'invalidvar1': Variable 'invalidvar1' of declared type {typeof(int?).CleanName()} cannot be initialized by value 'abc': System.FormatException: The input string 'abc' was not in a correct format."
#else
                $"Failed compiler startup: Error configuring variable 'invalidvar1': Variable 'invalidvar1' of declared type {typeof(int?).CleanName()} cannot be initialized by value 'abc': System.FormatException: Input string was not in a correct format."
#endif
            );
            
            TryInvalidConfigurationRuntime(
                runtimeProvider,
                SupportConfigFactory.GetConfiguration(Container),
                config => config.Common.AddVariable("invalidvar1", typeof(int?), "abc"),
#if NET7_0_OR_GREATER
                $"Failed runtime startup: Error configuring variable 'invalidvar1': Variable 'invalidvar1' of declared type {typeof(int?).CleanName()} cannot be initialized by value 'abc': System.FormatException: The input string 'abc' was not in a correct format."
#else
                $"Failed runtime startup: Error configuring variable 'invalidvar1': Variable 'invalidvar1' of declared type {typeof(int?).CleanName()} cannot be initialized by value 'abc': System.FormatException: Input string was not in a correct format."
#endif
                );

            TryInvalidConfigurationCompiler(
                SupportConfigFactory.GetConfiguration(Container),
                config => config.Common.AddVariable("invalidvar1", typeof(int?), 1.1d),
                "Failed compiler startup: Error configuring variable 'invalidvar1': Variable 'invalidvar1' of declared type " +
                typeof(int?).CleanName() +
                " cannot be initialized by a value of type System.Double");
            TryInvalidConfigurationRuntime(
                runtimeProvider,
                SupportConfigFactory.GetConfiguration(Container),
                config => config.Common.AddVariable("invalidvar1", typeof(int?), 1.1d),
                "Failed runtime startup: Error configuring variable 'invalidvar1': Variable 'invalidvar1' of declared type " +
                typeof(int?).CleanName() +
                " cannot be initialized by a value of type System.Double");
        }
    }
} // end of namespace