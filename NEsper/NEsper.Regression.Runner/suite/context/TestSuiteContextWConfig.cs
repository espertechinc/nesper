///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.context;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.context
{
    [TestFixture]
    public class TestSuiteContextWConfig
    {
        [Test, RunInApplicationDomain]
        public void TestContextKeySegmentedPrioritized()
        {
            RegressionSession session = RegressionRunner.Session();
            ConfigurePrioritized(session.Configuration);
            RegressionRunner.Run(session, new ContextKeySegmentedPrioritized());
            session.Dispose();
        }

        [Test, RunInApplicationDomain]
        public void TestContextKeySegmentedWInitTermPrioritized()
        {
            RegressionSession session = RegressionRunner.Session();
            ConfigurePrioritized(session.Configuration);
            RegressionRunner.Run(session, ContextKeySegmentedWInitTermPrioritized.Executions());
            session.Dispose();
        }

        [Test, RunInApplicationDomain]
        public void TestContextInitTermPrioritized()
        {
            RegressionSession session = RegressionRunner.Session();
            ConfigurePrioritized(session.Configuration);
            RegressionRunner.Run(session, ContextInitTermPrioritized.Executions());
            session.Dispose();
        }

        private static void ConfigurePrioritized(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1), typeof(SupportBean_S2), typeof(ISupportA), typeof(ISupportB),
                typeof(ISupportABCImpl), typeof(ISupportAImpl), typeof(ISupportBImpl), typeof(SupportProductIdEvent)})
            {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            configuration.Runtime.Execution.IsPrioritized = true;
        }
    }
} // end of namespace