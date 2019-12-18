///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.regressionlib.suite.client.compile;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean = com.espertech.esper.common.@internal.support.SupportBean;
using SupportBean_S0 = com.espertech.esper.common.@internal.support.SupportBean_S0;
using SupportBean_S1 = com.espertech.esper.common.@internal.support.SupportBean_S1;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientCompile
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
            _session.Destroy();
            _session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileOutput()
        {
            RegressionRunner.Run(_session, ClientCompileOutput.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileVisibility()
        {
            RegressionRunner.Run(_session, ClientCompileVisibility.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileSPI()
        {
            RegressionRunner.Run(_session, ClientCompileSPI.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileUserObject()
        {
            RegressionRunner.Run(_session, ClientCompileUserObject.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileStatementName()
        {
            RegressionRunner.Run(_session, ClientCompileStatementName.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileModule()
        {
            RegressionRunner.Run(_session, ClientCompileModule.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileSyntaxValidate()
        {
            RegressionRunner.Run(_session, ClientCompileSyntaxValidate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileModuleUses()
        {
            RegressionRunner.Run(_session, ClientCompileModuleUses.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileStatementObjectModel()
        {
            RegressionRunner.Run(_session, ClientCompileStatementObjectModel.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileSubstitutionParams()
        {
            RegressionRunner.Run(_session, ClientCompileSubstitutionParams.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileEnginePath()
        {
            RegressionRunner.Run(_session, ClientCompileEnginePath.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileEventTypeAutoName()
        {
            RegressionRunner.Run(_session, ClientCompileEventTypeAutoName.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] {
                typeof(SupportBean),
                typeof(SupportMarketDataBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1)
            })
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