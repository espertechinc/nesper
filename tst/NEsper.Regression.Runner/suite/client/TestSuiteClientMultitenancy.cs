///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.client.multitenancy;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientMultitenancy
    {
        private RegressionSession _session;

        [SetUp]
        public void SetUp()
        {
            _session = RegressionRunner.Session();
            Configure(_session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            _session.Dispose();
            _session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestClientMultitenancyProtected()
        {
            RegressionRunner.Run(_session, ClientMultitenancyProtected.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientMultitenancyInsertInto()
        {
            RegressionRunner.Run(_session, ClientMultitenancyInsertInto.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
        }
    }
} // end of namespace