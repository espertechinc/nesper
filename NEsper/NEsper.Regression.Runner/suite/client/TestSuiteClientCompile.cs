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
using com.espertech.esper.regressionlib.suite.client.compile;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientCompile
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
        public void TestClientCompileOutput()
        {
            RegressionRunner.Run(session, ClientCompileOutput.Executions());
        }

        [Test]
        public void TestClientCompileVisibility()
        {
            RegressionRunner.Run(session, ClientCompileVisibility.Executions());
        }

        [Test]
        public void TestClientCompileSPI()
        {
            RegressionRunner.Run(session, ClientCompileSPI.Executions());
        }

        [Test]
        public void TestClientCompileUserObject()
        {
            RegressionRunner.Run(session, ClientCompileUserObject.Executions());
        }

        [Test]
        public void TestClientCompileStatementName()
        {
            RegressionRunner.Run(session, ClientCompileStatementName.Executions());
        }

        [Test]
        public void TestClientCompileModule()
        {
            RegressionRunner.Run(session, ClientCompileModule.Executions());
        }

        [Test]
        public void TestClientCompileSyntaxValidate()
        {
            RegressionRunner.Run(session, ClientCompileSyntaxValidate.Executions());
        }

        [Test]
        public void TestClientCompileModuleUses()
        {
            RegressionRunner.Run(session, ClientCompileModuleUses.Executions());
        }

        [Test]
        public void TestClientCompileStatementObjectModel()
        {
            RegressionRunner.Run(session, ClientCompileStatementObjectModel.Executions());
        }

        [Test]
        public void TestClientCompileSubstitutionParams()
        {
            RegressionRunner.Run(session, ClientCompileSubstitutionParams.Executions());
        }

        [Test]
        public void TestClientCompileEnginePath()
        {
            RegressionRunner.Run(session, ClientCompileEnginePath.Executions());
        }

        [Test]
        public void TestClientCompileEventTypeAutoName()
        {
            RegressionRunner.Run(session, ClientCompileEventTypeAutoName.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] { typeof(SupportBean), typeof(SupportMarketDataBean), typeof(SupportBean_S0), typeof(SupportBean_S1) })
            {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            configuration.Common.AddVariable("preconfigured_variable", typeof(int), 5, true);

            configuration.Compiler.ByteCode.AttachModuleEPL = true;
            configuration.Common.AddImportType(typeof(SupportBean));
            configuration.Common.AddImportType(typeof(ClientCompileSubstitutionParams.IKey));
            configuration.Common.AddImportType(typeof(ClientCompileSubstitutionParams.MyObjectKeyConcrete));

            configuration.Common.AddEventTypeAutoName("com.espertech.esper.regressionlib.support.autoname.one");
            configuration.Common.AddEventTypeAutoName("com.espertech.esper.regressionlib.support.autoname.two");
        }
    }
} // end of namespace