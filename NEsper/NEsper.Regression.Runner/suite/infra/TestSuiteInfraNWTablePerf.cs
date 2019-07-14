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
using com.espertech.esper.regressionlib.suite.infra.nwtable;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.infra
{
    [TestFixture]
    public class TestSuiteInfraNWTablePerf
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

        [Test]
        public void TestInfraNWTableFAFIndexPerfWNoQueryPlanLog()
        {
            RegressionRunner.Run(session, InfraNWTableFAFIndexPerfWNoQueryPlanLog.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] { typeof(SupportBean) })
            {
                configuration.Common.AddEventType(clazz);
            }
            configuration.Compiler.AddPlugInSingleRowFunction("justCount", typeof(InfraNWTableFAFIndexPerfWNoQueryPlanLog.InvocationCounter), "justCount");
        }
    }
} // end of namespace