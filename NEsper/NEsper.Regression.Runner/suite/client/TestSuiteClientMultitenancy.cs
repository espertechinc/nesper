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
        public void TestClientMultitenancyProtected()
        {
            RegressionRunner.Run(session, ClientMultitenancyProtected.Executions());
        }

        [Test]
        public void TestClientMultitenancyInsertInto()
        {
            RegressionRunner.Run(session, ClientMultitenancyInsertInto.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
        }
    }
} // end of namespace