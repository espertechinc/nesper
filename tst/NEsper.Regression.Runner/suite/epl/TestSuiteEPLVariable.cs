///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.variable;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLVariable
    {
        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Dispose();
            session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestEPLVariablesPerf()
        {
            RegressionRunner.Run(session, new EPLVariablesPerf(), true);
        }
        
        /// <summary>
        /// Auto-test(s): EPLVariablesUse
        /// <code>
        /// RegressionRunner.Run(_session, EPLVariablesUse.Executions());
        /// </code>
        /// </summary>

        public class TestEPLVariablesUse : AbstractTestBase
        {
            public TestEPLVariablesUse() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithWVarargs() => RegressionRunner.Run(_session, EPLVariablesUse.WithWVarargs());

            [Test, RunInApplicationDomain]
            public void WithFilterConstantCustomTypePreconfigured() => RegressionRunner.Run(_session, EPLVariablesUse.WithFilterConstantCustomTypePreconfigured());

            [Test, RunInApplicationDomain]
            public void WithVariableInFilter() => RegressionRunner.Run(_session, EPLVariablesUse.WithVariableInFilter());

            [Test, RunInApplicationDomain]
            public void WithVariableInFilterBoolean() => RegressionRunner.Run(_session, EPLVariablesUse.WithVariableInFilterBoolean());

            [Test, RunInApplicationDomain]
            public void WithConstantVariable() => RegressionRunner.Run(_session, EPLVariablesUse.WithConstantVariable());

            [Test, RunInApplicationDomain]
            public void WithInvokeMethod() => RegressionRunner.Run(_session, EPLVariablesUse.WithInvokeMethod());

            [Test, RunInApplicationDomain]
            public void WithDotSeparateThread() => RegressionRunner.Run(_session, EPLVariablesUse.WithDotSeparateThread());

            [Test, RunInApplicationDomain]
            public void WithEPRuntime() => RegressionRunner.Run(_session, EPLVariablesUse.WithEPRuntime());

            [Test, RunInApplicationDomain]
            public void WithSimpleTwoModules() => RegressionRunner.Run(_session, EPLVariablesUse.WithSimpleTwoModules());

            [Test, RunInApplicationDomain]
            public void WithSimpleSameModule() => RegressionRunner.Run(_session, EPLVariablesUse.WithSimpleSameModule());

            [Test, RunInApplicationDomain]
            public void WithSimplePreconfigured() => RegressionRunner.Run(_session, EPLVariablesUse.WithSimplePreconfigured());
        }
        
        /// <summary>
        /// Auto-test(s): EPLVariablesDestroy
        /// <code>
        /// RegressionRunner.Run(_session, EPLVariablesDestroy.Executions());
        /// </code>
        /// </summary>

        public class TestEPLVariablesDestroy : AbstractTestBase
        {
            public TestEPLVariablesDestroy() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithDestroyReCreateChangeType() => RegressionRunner.Run(_session, EPLVariablesDestroy.WithDestroyReCreateChangeType());

            [Test, RunInApplicationDomain]
            public void WithManageDependency() => RegressionRunner.Run(_session, EPLVariablesDestroy.WithManageDependency());
        }
        
        /// <summary>
        /// Auto-test(s): EPLVariablesOutputRate
        /// <code>
        /// RegressionRunner.Run(_session, EPLVariablesOutputRate.Executions());
        /// </code>
        /// </summary>

        public class TestEPLVariablesOutputRate : AbstractTestBase
        {
            public TestEPLVariablesOutputRate() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithTimeAll() => RegressionRunner.Run(_session, EPLVariablesOutputRate.WithTimeAll());

            [Test, RunInApplicationDomain]
            public void WithEventsAllCompile() => RegressionRunner.Run(_session, EPLVariablesOutputRate.WithEventsAllCompile());

            [Test, RunInApplicationDomain]
            public void WithEventsAllOM() => RegressionRunner.Run(_session, EPLVariablesOutputRate.WithEventsAllOM());

            [Test, RunInApplicationDomain]
            public void WithEventsAll() => RegressionRunner.Run(_session, EPLVariablesOutputRate.WithEventsAll());
        }
        
        /// <summary>
        /// Auto-test(s): EPLVariablesInlinedClass
        /// <code>
        /// RegressionRunner.Run(_session, EPLVariablesInlinedClass.Executions());
        /// </code>
        /// </summary>

        public class TestEPLVariablesInlinedClass : AbstractTestBase
        {
            public TestEPLVariablesInlinedClass() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithGlobal() => RegressionRunner.Run(_session, EPLVariablesInlinedClass.WithGlobal());

            [Test, RunInApplicationDomain]
            public void WithLocal() => RegressionRunner.Run(_session, EPLVariablesInlinedClass.WithLocal());
        }
        
        /// <summary>
        /// Auto-test(s): EPLVariablesEventTyped
        /// <code>
        /// RegressionRunner.Run(_session, EPLVariablesEventTyped.Executions());
        /// </code>
        /// </summary>

        public class TestEPLVariablesEventTyped : AbstractTestBase
        {
            public TestEPLVariablesEventTyped() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLVariablesEventTyped.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithEventTypedSetProp() => RegressionRunner.Run(_session, EPLVariablesEventTyped.WithEventTypedSetProp());

            [Test, RunInApplicationDomain]
            public void WithConfig() => RegressionRunner.Run(_session, EPLVariablesEventTyped.WithConfig());

            [Test, RunInApplicationDomain]
            public void WithEventTypedSceneTwo() => RegressionRunner.Run(_session, EPLVariablesEventTyped.WithEventTypedSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithEventTypedSceneOne() => RegressionRunner.Run(_session, EPLVariablesEventTyped.WithEventTypedSceneOne());
        }
        
        /// <summary>
        /// Auto-test(s): EPLVariablesCreate
        /// <code>
        /// RegressionRunner.Run(_session, EPLVariablesCreate.Executions());
        /// </code>
        /// </summary>

        public class TestEPLVariablesCreate : AbstractTestBase
        {
            public TestEPLVariablesCreate() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithGenericType() => RegressionRunner.Run(_session, EPLVariablesCreate.WithGenericType());

            [Test, RunInApplicationDomain]
            public void WithDimensionAndPrimitive() => RegressionRunner.Run(_session, EPLVariablesCreate.WithDimensionAndPrimitive());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLVariablesCreate.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithDeclarationAndSelect() => RegressionRunner.Run(_session, EPLVariablesCreate.WithDeclarationAndSelect());

            [Test, RunInApplicationDomain]
            public void WithSubscribeAndIterate() => RegressionRunner.Run(_session, EPLVariablesCreate.WithSubscribeAndIterate());

            [Test, RunInApplicationDomain]
            public void WithCompileStartStop() => RegressionRunner.Run(_session, EPLVariablesCreate.WithCompileStartStop());

            [Test, RunInApplicationDomain]
            public void WithOM() => RegressionRunner.Run(_session, EPLVariablesCreate.WithOM());
        }

        /// <summary>
        /// Auto-test(s): EPLVariablesOnSet
        /// <code>
        /// RegressionRunner.Run(_session, EPLVariablesOnSet.Executions());
        /// </code>
        /// </summary>

        public class TestEPLVariablesOnSet : AbstractTestBase
        {
            public TestEPLVariablesOnSet() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithExpression() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithExpression());

            [Test, RunInApplicationDomain]
            public void WithArrayInvalid() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithArrayInvalid());

            [Test]
            public void WithArrayBoxed() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithArrayBoxed());

            [Test, RunInApplicationDomain]
            public void WithArrayAtIndex() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithArrayAtIndex());

            [Test, RunInApplicationDomain]
            public void WithSubqueryMultikeyWArray() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithSubqueryMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithCoercion() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithCoercion());

            [Test, RunInApplicationDomain]
            public void WithRuntimeOrderMultiple() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithRuntimeOrderMultiple());

            [Test, RunInApplicationDomain]
            public void WithAssignmentOrderDup() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithAssignmentOrderDup());

            [Test, RunInApplicationDomain]
            public void WithAssignmentOrderNoDup() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithAssignmentOrderNoDup());

            [Test, RunInApplicationDomain]
            public void WithWDeploy() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithWDeploy());

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithSubquery());

            [Test, RunInApplicationDomain]
            public void WithWithFilter() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithWithFilter());

            [Test, RunInApplicationDomain]
            public void WithObjectModel() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithObjectModel());

            [Test, RunInApplicationDomain]
            public void WithCompile() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithCompile());

            [Test, RunInApplicationDomain]
            public void WithSimpleSceneTwo() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithSimpleSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, EPLVariablesOnSet.WithSimple());
        }


        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportMarketDataBean),
                typeof(EPLVariablesUse.MyVariableCustomEvent),
                typeof(SupportEventWithIntArray),
                typeof(SupportBeanNumeric)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddImportType(typeof(EPLVariablesUse.MySimpleVariableServiceFactory));
            configuration.Common.AddImportType(typeof(EPLVariablesUse.MySimpleVariableService));
            configuration.Common.AddImportType(typeof(EPLVariablesUse.MyVariableCustomType));

            configuration.Common.AddImportType(typeof(SupportEnum));

            ConfigurationCommon common = configuration.Common;
            common.AddVariable("var_simple_preconfig_const", "boolean", true, true);
            common.AddVariable("MYCONST_THREE", "boolean", true, true);
            common.AddVariable("papi_1", typeof(string), "begin");
            common.AddVariable("papi_2", typeof(bool), true);
            common.AddVariable("papi_3", typeof(string), "value");
            common.AddVariable("myRuntimeInitService", typeof(EPLVariablesUse.MySimpleVariableService), null);
            common.AddVariable("MYCONST_TWO", "string", null, true);
            common.AddVariable("varcoll", "String[]", new string[] {"E1", "E2"}, true);
            common.AddVariable("mySimpleVariableService", typeof(EPLVariablesUse.MySimpleVariableService), null);
            common.AddVariable("myInitService", typeof(EPLVariablesUse.MySimpleVariableService), EPLVariablesUse.MySimpleVariableServiceFactory.MakeService());
            common.AddVariable("supportEnum", typeof(SupportEnum), SupportEnum.ENUM_VALUE_1);
            common.AddVariable("enumWithOverride", typeof(EPLVariablesUse.MyEnumWithOverride), EPLVariablesUse.MyEnumWithOverride.LONG);
            common.AddVariable("var1", typeof(int), -1);
            common.AddVariable("var2", typeof(string), "abc");
            common.AddVariable("var1SS", typeof(string), "a");
            common.AddVariable("var2SS", typeof(string), "b");
            common.AddVariable("var1IFB", typeof(string), null);
            common.AddVariable("var2IFB", typeof(string), null);
            common.AddVariable("var1IF", typeof(string), null);
            common.AddVariable("var1OND", typeof(int?), "12");
            common.AddVariable("var2OND", typeof(int?), "2");
            common.AddVariable("var3OND", typeof(int?), null);
            common.AddVariable("var1OD", typeof(int?), 0);
            common.AddVariable("var2OD", typeof(int?), 1);
            common.AddVariable("var3OD", typeof(int?), 2);
            common.AddVariable("var1OM", typeof(double), 10d);
            common.AddVariable("var2OM", typeof(long?), 11L);
            common.AddVariable("var1C", typeof(double), 10d);
            common.AddVariable("var2C", typeof(long?), 11L);
            common.AddVariable("var1RTC", typeof(int?), 10);
            common.AddVariable("var1ROM", typeof(int?), null);
            common.AddVariable("var2ROM", typeof(int?), 1);
            common.AddVariable("var1COE", typeof(float?), null);
            common.AddVariable("var2COE", typeof(double?), null);
            common.AddVariable("var3COE", typeof(long?), null);
            common.AddVariable("var1IS", typeof(string), null);
            common.AddVariable("var2IS", typeof(bool), false);
            common.AddVariable("var3IS", typeof(int), 1);
            common.AddVariable("MyPermanentVar", typeof(string), "thevalue");
            common.AddVariable("vars0_A", "SupportBean_S0", new SupportBean_S0(10));
            common.AddVariable("vars1_A", typeof(SupportBean_S1), new SupportBean_S1(20));
            common.AddVariable("varsobj1", typeof(object), 123, true);
            common.AddVariable("vars2", "SupportBean_S2", new SupportBean_S2(30));
            common.AddVariable("vars3", typeof(SupportBean_S3), new SupportBean_S3(40));
            common.AddVariable("varsobj2", typeof(object), "ABC", true);
            common.AddVariable("var_output_limit", typeof(long), "3");
            common.AddVariable("myNonSerializable", typeof(EPLVariablesEventTyped.NonSerializable), EPLVariablesEventTyped.NON_SERIALIZABLE);
            common.AddVariable("my_variable_custom_typed", typeof(EPLVariablesUse.MyVariableCustomType), EPLVariablesUse.MyVariableCustomType.Of("abc"), true);
            common.AddVariable("varargsTestClient", typeof(EPLVariablesUse.SupportVarargsClient), new EPLVariablesUse.SupportVarargsClientImpl());

            common.AddVariable("my_variable_custom_typed", typeof(EPLVariablesUse.MyVariableCustomType), EPLVariablesUse.MyVariableCustomType.Of("abc"), true);

            configuration.Compiler.ViewResources.IterableUnbound = true;
            configuration.Compiler.ByteCode.AllowSubscriber = true;
        }
    }
} // end of namespace