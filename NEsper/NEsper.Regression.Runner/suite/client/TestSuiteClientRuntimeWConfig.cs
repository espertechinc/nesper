///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.client.runtime;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientRuntimeWConfig
    {
        [Test]
        public void TestClientRuntimeRuntimeStateChange()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            new ClientRuntimeRuntimeProvider.ClientRuntimeRuntimeStateChange().Run(configuration);
        }

        [Test]
        public void TestClientRuntimeRuntimeDestroy()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            new ClientRuntimeRuntimeProvider.ClientRuntimeRuntimeDestroy().Run(config);
        }

        [Test]
        public void TestClientRuntimeExHandlerGetContext()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            new ClientRuntimeExceptionHandler.ClientRuntimeExHandlerGetContext().Run(configuration);
        }

        [Test]
        public void TestClientRuntimeExceptionHandlerNoHandler()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            new ClientRuntimeExceptionHandler.ClientRuntimeExceptionHandlerNoHandler().Run(configuration);
        }

        [Test]
        public void TestClientRuntimeInvalidMicroseconds()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            new ClientRuntimeRuntimeProvider.ClientRuntimeMicrosecondInvalid().Run(config);
        }

        [Test]
        public void TestClientRuntimeAnnotationImportInvalid()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.AddAnnotationImportType(typeof(SupportEnum));
            session.Configuration.Common.AddAnnotationImportType(typeof(MyAnnotationValueEnumAttribute));
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            RegressionRunner.Run(session, new ClientRuntimeStatementAnnotation.ClientRuntimeAnnotationImportInvalid());
            session.Destroy();
        }

        [Test]
        public void TestClientRuntimeThreadedConfigInbound()
        {
            RegressionRunner.RunConfigurable(new ClientRuntimeThreadedConfigInbound());
        }

        [Test]
        public void TestClientRuntimeThreadedConfigInboundFastShutdown()
        {
            RegressionRunner.RunConfigurable(new ClientRuntimeThreadedConfigInboundFastShutdown());
        }

        [Test]
        public void TestClientRuntimeThreadedConfigOutbound()
        {
            RegressionRunner.RunConfigurable(new ClientRuntimeThreadedConfigOutbound());
        }

        [Test]
        public void TestClientRuntimeThreadedConfigRoute()
        {
            RegressionRunner.RunConfigurable(new ClientRuntimeThreadedConfigRoute());
        }

        [Test]
        public void TestClientRuntimeThreadedConfigTimer()
        {
            RegressionRunner.RunConfigurable(new ClientRuntimeThreadedConfigTimer());
        }

        [Test]
        public void TestClientRuntimeClockTypeRuntime()
        {
            new ClientRuntimeTimeControlClockType().Run(SupportConfigFactory.GetConfiguration());
        }

        [Test]
        public void TestClientSubscriberDisallowed()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            RegressionRunner.Run(session, new ClientRuntimeSubscriberDisallowed());
            session.Destroy();
        }
    }
} // end of namespace