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
using com.espertech.esper.regressionlib.suite.client.deploy;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientDeploy
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configuration configuration = session.Configuration;
            Configure(configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test]
        public void TestClientDeployUndeploy()
        {
            RegressionRunner.Run(session, ClientDeployUndeploy.Executions());
        }

        [Test]
        public void TestClientDeployPreconditionDependency()
        {
            RegressionRunner.Run(session, ClientDeployPreconditionDependency.Executions());
        }

        [Test]
        public void TestClientDeployPreconditionDuplicate()
        {
            RegressionRunner.Run(session, ClientDeployPreconditionDuplicate.Executions());
        }

        [Test]
        public void TestClientDeployUserObject()
        {
            RegressionRunner.Run(session, ClientDeployUserObject.Executions());
        }

        [Test]
        public void TestClientDeployStatementName()
        {
            RegressionRunner.Run(session, ClientDeployStatementName.Executions());
        }

        [Test]
        public void TestClientClientDeployResult()
        {
            RegressionRunner.Run(session, ClientDeployResult.Executions());
        }

        [Test]
        public void TestClientDeployRedefinition()
        {
            RegressionRunner.Run(session, ClientDeployRedefinition.Executions());
        }

        private void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] { typeof(SupportBean), typeof(SupportBean_S0) })
            {
                configuration.Common.AddEventType(clazz);
            }
        }
    }
} // end of namespace