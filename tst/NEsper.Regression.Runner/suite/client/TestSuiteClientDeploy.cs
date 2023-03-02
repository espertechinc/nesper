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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientDeploy : AbstractTestBase
    {
        public TestSuiteClientDeploy() : base(Configure)
        {
        }

        protected override bool UseDefaultRuntime => true;

        [Test, RunInApplicationDomain]
        public void TestClientDeployUndeploy()
        {
            RegressionRunner.Run(_session, ClientDeployUndeploy.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientDeployPreconditionDependency()
        {
            RegressionRunner.Run(_session, ClientDeployPreconditionDependency.Executions());
        }

        [Test, RunInApplicationDomain]
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

        [Test, RunInApplicationDomain]
        public void TestClientDeployRedefinition()
        {
            RegressionRunner.Run(_session, ClientDeployRedefinition.Executions());
        }
        
        [Test, RunInApplicationDomain]
        public void TestClientDeployVersion() {
            RegressionRunner.Run(_session, ClientDeployVersion.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientDeployClassLoaderOption() {
            RegressionRunner.Run(_session, ClientDeployClassLoaderOption.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientDeployRollout() {
            RegressionRunner.Run(_session, ClientDeployRollout.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientDeployListDependencies() {
            RegressionRunner.Run(_session, ClientDeployListDependencies.Executions());
        }

        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new Type[] { typeof(SupportBean), typeof(SupportBean_S0) })
            {
                configuration.Common.AddEventType(clazz);
            }
        }
    }
} // end of namespace