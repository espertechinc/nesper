///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.expr.datetime;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprDateTimeWConfig : AbstractTestContainer
    {
        private readonly EPRuntimeProvider runtimeProvider = new EPRuntimeProvider();
        
        private void TryInvalidConfig(
            Type beanEventClass,
            ConfigurationCommonEventTypeBean configBean,
            string expected)
        {
            TryInvalidConfigurationCompileAndRuntime(
                runtimeProvider,
                SupportConfigFactory.GetConfiguration(Container),
                config => config.Common.AddEventType(beanEventClass.Name, beanEventClass.FullName, configBean),
                expected);
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTMicrosecondResolution()
        {
            using var session = RegressionRunner.Session(Container);
            session.Configuration.Common.AddEventType(typeof(SupportDateTime));
            session.Configuration.Common.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;
            TestSuiteExprDateTime.AddIdStsEtsEvent(session.Configuration);
            RegressionRunner.Run(session, ExprDTResolution.Executions(true));
        }

        [Test, RunInApplicationDomain]
        public void TestInvalidConfigure()
        {
            var configBean = new ConfigurationCommonEventTypeBean();

            configBean.StartTimestampPropertyName = null;
            configBean.EndTimestampPropertyName = "DateTimeEx";
            TryInvalidConfig(typeof(SupportDateTime), configBean, "Declared end timestamp property requires that a start timestamp property is also declared");

            configBean.StartTimestampPropertyName = "xyz";
            configBean.EndTimestampPropertyName = null;
            TryInvalidConfig(typeof(SupportBean), configBean, "Declared start timestamp property name 'xyz' was not found");

            configBean.StartTimestampPropertyName = "LongPrimitive";
            configBean.EndTimestampPropertyName = "xyz";
            TryInvalidConfig(typeof(SupportBean), configBean, "Declared end timestamp property name 'xyz' was not found");

            configBean.EndTimestampPropertyName = null;
            configBean.StartTimestampPropertyName = "TheString";
            TryInvalidConfig(
                typeof(SupportBean),
                configBean,
                "Declared start timestamp property 'TheString' is expected to return a DateTimeEx, DateTime, DateTimeOffset or long-typed value but returns 'System.String'");

            configBean.StartTimestampPropertyName = "LongPrimitive";
            configBean.EndTimestampPropertyName = "TheString";
            TryInvalidConfig(
                typeof(SupportBean),
                configBean,
                "Declared end timestamp property 'TheString' is expected to return a DateTimeEx, DateTime, DateTimeOffset or long-typed value but returns 'System.String'");

            configBean.StartTimestampPropertyName = "LongDate";
            configBean.EndTimestampPropertyName = "DateTimeEx";
            TryInvalidConfig(
                typeof(SupportDateTime),
                configBean,
                "Declared end timestamp property 'DateTimeEx' is expected to have the same property type as the start-timestamp property 'LongDate'");
        }
    }
} // end of namespace