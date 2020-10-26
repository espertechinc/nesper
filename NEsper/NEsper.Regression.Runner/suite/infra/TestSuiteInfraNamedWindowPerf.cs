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
using com.espertech.esper.regressionlib.suite.infra.namedwindow;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.infra
{
    // see INFRA suite for additional Named Window tests
    [TestFixture]
    public class TestSuiteInfraNamedWindowPerf
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowPerformance()
        {
            RegressionRunner.Run(session, InfraNamedWindowPerformance.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWIndowFAFQueryJoinPerformance()
        {
            RegressionRunner.Run(session, new InfraNamedWIndowFAFQueryJoinPerformance());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBeanRange),
                typeof(SupportBean_A),
                typeof(SupportMarketDataBean),
                typeof(SupportSimpleBeanTwo),
                typeof(SupportSimpleBeanOne)})
            {
                configuration.Common.AddEventType(clazz);
            }
        }
    }
} // end of namespace