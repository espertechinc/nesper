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
        private RegressionSession _session;

        [SetUp]
        public void SetUp()
        {
            _session = RegressionRunner.Session();
            Configuration configuration = _session.Configuration;
            Configure(configuration);
        }

        [TearDown]
        public void TearDown()
        {
            _session.Destroy();
            _session = null;
        }

        [Test]
        public void TestClientDeployUndeploy()
        {
            RegressionRunner.Run(_session, ClientDeployUndeploy.Executions());
        }

        [Test]
        public void TestClientDeployPreconditionDependency()
        {
            RegressionRunner.Run(_session, ClientDeployPreconditionDependency.Executions());
        }

        [Test]
        public void TestClientDeployPreconditionDuplicate()
        {
            RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientDeployUserObject()
        {
            RegressionRunner.Run(_session, ClientDeployUserObject.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientDeployStatementName()
        {
            RegressionRunner.Run(_session, ClientDeployStatementName.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientClientDeployResult()
        {
            RegressionRunner.Run(_session, ClientDeployResult.Executions());
        }

        [Test]
        public void TestClientDeployRedefinition()
        {
            RegressionRunner.Run(_session, ClientDeployRedefinition.Executions());
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