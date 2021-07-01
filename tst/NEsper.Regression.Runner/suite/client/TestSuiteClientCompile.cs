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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

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
            _session.Dispose();
            _session = null;
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] {
                typeof(SupportBean),
                typeof(SupportMarketDataBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1)
            }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            configuration.Common.AddVariable("preconfigured_variable", typeof(int), 5, true);

            configuration.Compiler.ByteCode.AttachModuleEPL = true;
            configuration.Common.AddImportType(typeof(SupportBean));
            configuration.Common.AddImportType(typeof(ClientCompileSubstitutionParams.IKey));
            configuration.Common.AddImportType(typeof(ClientCompileSubstitutionParams.MyObjectKeyConcrete));

            configuration.Common.AddEventTypeAutoName("com.espertech.esper.regressionlib.support.autoname.one");
            configuration.Common.AddEventTypeAutoName("com.espertech.esper.regressionlib.support.autoname.two");

            configuration.Compiler.AddPlugInSingleRowFunction("func", typeof(ClientCompileLarge), "Func");
        }

        /// <summary>
        /// Auto-test(s): ClientCompileOutput
        /// <code>
        /// RegressionRunner.Run(session, ClientCompileOutput.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileOutput : AbstractTestBase
        {
            public TestClientCompileOutput() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithManifestSimple() => RegressionRunner.Run(
                _session,
                ClientCompileOutput.WithManifestSimple());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileVisibility
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileVisibility.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileVisibility : AbstractTestBase
        {
            public TestClientCompileVisibility() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithBusRequiresPublic() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithBusRequiresPublic());

            [Test, RunInApplicationDomain]
            public void WithDisambiguateWithUses() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithDisambiguateWithUses());

            [Test, RunInApplicationDomain]
            public void WithAmbiguousTwoPath() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithAmbiguousTwoPath());

            [Test, RunInApplicationDomain]
            public void WithAnnotationInvalid() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithAnnotationInvalid());

            [Test, RunInApplicationDomain]
            public void WithAnnotationBusEventType() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithAnnotationBusEventType());

            [Test, RunInApplicationDomain]
            public void WithModuleNameOption() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithModuleNameOption());

            [Test, RunInApplicationDomain]
            public void WithAnnotationPublic() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithAnnotationPublic());

            [Test, RunInApplicationDomain]
            public void WithAnnotationProtected() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithAnnotationProtected());

            [Test, RunInApplicationDomain]
            public void WithAnnotationPrivate() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithAnnotationPrivate());

            [Test, RunInApplicationDomain]
            public void WithDefaultPrivate() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithDefaultPrivate());

            [Test, RunInApplicationDomain]
            public void WithAmbiguousPathWithPreconfigured() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithAmbiguousPathWithPreconfigured());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowSimple() => RegressionRunner.Run(
                _session,
                ClientCompileVisibility.WithNamedWindowSimple());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileSPI
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileSPI.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileSPI : AbstractTestBase
        {
            public TestClientCompileSPI() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSPIExpression() => RegressionRunner.Run(
                _session,
                ClientCompileSPI.WithSPIExpression());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileUserObject
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileUserObject.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileUserObject : AbstractTestBase
        {
            public TestClientCompileUserObject() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithResolveContextInfo() => RegressionRunner.Run(
                _session,
                ClientCompileUserObject.WithResolveContextInfo());

            [Test, RunInApplicationDomain]
            public void WithDifferentTypes() => RegressionRunner.Run(
                _session,
                ClientCompileUserObject.WithDifferentTypes());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileStatementName
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileStatementName.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileStatementName : AbstractTestBase
        {
            public TestClientCompileStatementName() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void Withe() => RegressionRunner.Run(
                _session,
                ClientCompileStatementName.Withe());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileModule
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileModule.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileModule : AbstractTestBase
        {
            public TestClientCompileModule() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCommentTrailing() => RegressionRunner.Run(
                _session,
                ClientCompileModule.WithCommentTrailing());

            [Test, RunInApplicationDomain]
            public void WithParseFail() => RegressionRunner.Run(
                _session,
                ClientCompileModule.WithParseFail());

            [Test, RunInApplicationDomain]
            public void WithParse() => RegressionRunner.Run(
                _session,
                ClientCompileModule.WithParse());

            [Test, RunInApplicationDomain]
            public void WithTwoModules() => RegressionRunner.Run(
                _session,
                ClientCompileModule.WithTwoModules());

            [Test, RunInApplicationDomain]
            public void WithLineNumberAndComments() => RegressionRunner.Run(
                _session,
                ClientCompileModule.WithLineNumberAndComments());

            [Test, RunInApplicationDomain]
            public void WithWImports() => RegressionRunner.Run(
                _session,
                ClientCompileModule.WithWImports());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileSyntaxValidate
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileSyntaxValidate.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileSyntaxValidate : AbstractTestBase
        {
            public TestClientCompileSyntaxValidate() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSyntaxMgs() => RegressionRunner.Run(
                _session,
                ClientCompileSyntaxValidate.WithSyntaxMgs());

            [Test, RunInApplicationDomain]
            public void WithOptionsValidateOnly() => RegressionRunner.Run(
                _session,
                ClientCompileSyntaxValidate.WithOptionsValidateOnly());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileModuleUses
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileModuleUses.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileModuleUses : AbstractTestBase
        {
            public TestClientCompileModuleUses() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithIgnorableUses() => RegressionRunner.Run(
                _session,
                ClientCompileModuleUses.WithIgnorableUses());

            [Test, RunInApplicationDomain]
            public void WithUnresolvedUses() => RegressionRunner.Run(
                _session,
                ClientCompileModuleUses.WithUnresolvedUses());

            [Test, RunInApplicationDomain]
            public void WithCircular() => RegressionRunner.Run(
                _session,
                ClientCompileModuleUses.WithCircular());

            [Test, RunInApplicationDomain]
            public void WithOrder() => RegressionRunner.Run(
                _session,
                ClientCompileModuleUses.WithOrder());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileStatementObjectModel
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileStatementObjectModel.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileStatementObjectModel : AbstractTestBase
        {
            public TestClientCompileStatementObjectModel() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithPrecedencePatterns() => RegressionRunner.Run(
                _session,
                ClientCompileStatementObjectModel.WithPrecedencePatterns());

            [Test, RunInApplicationDomain]
            public void WithPrecedenceExpressions() => RegressionRunner.Run(
                _session,
                ClientCompileStatementObjectModel.WithPrecedenceExpressions());

            [Test, RunInApplicationDomain]
            public void WithEPLtoOMtoStmt() => RegressionRunner.Run(
                _session,
                ClientCompileStatementObjectModel.WithEPLtoOMtoStmt());

            [Test, RunInApplicationDomain]
            public void WithCreateFromOMComplete() => RegressionRunner.Run(
                _session,
                ClientCompileStatementObjectModel.WithCreateFromOMComplete());

            [Test, RunInApplicationDomain]
            public void WithCreateFromOM() => RegressionRunner.Run(
                _session,
                ClientCompileStatementObjectModel.WithCreateFromOM());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileSubstitutionParams
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileSubstitutionParams.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileSubstitutionParams : AbstractTestBase
        {
            public TestClientCompileSubstitutionParams() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithODAInvalidConstantUseSubsParamsInstead() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithODAInvalidConstantUseSubsParamsInstead());

            [Test, RunInApplicationDomain]
            public void WithSubstParamArray() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamArray());

            [Test, RunInApplicationDomain]
            public void WithSubstParamMultiStmt() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamMultiStmt());

            [Test, RunInApplicationDomain]
            public void WithSubstParamResolverContext() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamResolverContext());

            [Test, RunInApplicationDomain]
            public void WithSubstParamInvalidParametersTyped() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamInvalidParametersTyped());

            [Test, RunInApplicationDomain]
            public void WithSubstParamInvalidParametersUntyped() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamInvalidParametersUntyped());

            [Test, RunInApplicationDomain]
            public void WithSubstParamInvalidInsufficientValues() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamInvalidInsufficientValues());

            [Test, RunInApplicationDomain]
            public void WithSubstParamInvalidNoCallback() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamInvalidNoCallback());

            [Test, RunInApplicationDomain]
            public void WithSubstParamInvalidUse() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamInvalidUse());

            [Test, RunInApplicationDomain]
            public void WithSubstParamSubselect() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamSubselect());

            [Test, RunInApplicationDomain]
            public void WithSubstParamPrimitiveVsBoxed() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamPrimitiveVsBoxed());

            [Test, RunInApplicationDomain]
            public void WithSubstParamSimpleNoParameter() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamSimpleNoParameter());

            [Test, RunInApplicationDomain]
            public void WithSubstParamSimpleTwoParameterWhere() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamSimpleTwoParameterWhere());

            [Test, RunInApplicationDomain]
            public void WithSubstParamSimpleTwoParameterFilter() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamSimpleTwoParameterFilter());

            [Test, RunInApplicationDomain]
            public void WithSubstParamWInheritance() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamWInheritance());

            [Test, RunInApplicationDomain]
            public void WithSubstParamSimpleOneParameterWCast() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamSimpleOneParameterWCast());

            [Test, RunInApplicationDomain]
            public void WithSubstParamPattern() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamPattern());

            [Test, RunInApplicationDomain]
            public void WithSubstParamUnnamedParameterWType() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamUnnamedParameterWType());

            [Test, RunInApplicationDomain]
            public void WithSubstParamMethodInvocation() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamMethodInvocation());

            [Test, RunInApplicationDomain]
            public void WithSubstParamNamedParameter() => RegressionRunner.Run(
                _session,
                ClientCompileSubstitutionParams.WithSubstParamNamedParameter());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileEnginePath
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileEnginePath.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileEnginePath : AbstractTestBase
        {
            public TestClientCompileEnginePath() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithrEnginePathNamedWindowUse() => RegressionRunner.Run(
                _session,
                ClientCompileEnginePath.WithrEnginePathNamedWindowUse());

            [Test, RunInApplicationDomain]
            public void WithEnginePathPreconfiguredEventTypeFromPath() => RegressionRunner.Run(
                _session,
                ClientCompileEnginePath.WithEnginePathPreconfiguredEventTypeFromPath());

            [Test]
            public void WithEnginePathInfraWithIndex() => RegressionRunner.Run(
                _session,
                ClientCompileEnginePath.WithEnginePathInfraWithIndex());

            [Test, RunInApplicationDomain]
            public void WithEnginePathObjectTypes() => RegressionRunner.Run(
                _session,
                ClientCompileEnginePath.WithEnginePathObjectTypes());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileEventTypeAutoName
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileEventTypeAutoName.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileEventTypeAutoName : AbstractTestBase
        {
            public TestClientCompileEventTypeAutoName() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithAmbiguous() => RegressionRunner.Run(
                _session,
                ClientCompileEventTypeAutoName.WithAmbiguous());

            [Test, RunInApplicationDomain]
            public void WithResolve() => RegressionRunner.Run(
                _session,
                ClientCompileEventTypeAutoName.WithResolve());
        }

        /// <summary>
        /// Auto-test(s): ClientCompileLarge
        /// <code>
        /// RegressionRunner.Run(_session, ClientCompileLarge.Executions());
        /// </code>
        /// </summary>

        public class TestClientCompileLarge : AbstractTestBase
        {
            public TestClientCompileLarge() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithLargeConstantPoolDueToMethods() => RegressionRunner.Run(
                _session,
                ClientCompileLarge.WithLargeConstantPoolDueToMethods());
        }
    }
} // end of namespace