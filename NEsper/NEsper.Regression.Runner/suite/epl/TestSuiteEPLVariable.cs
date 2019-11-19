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
using com.espertech.esper.regressionrun.Runner;

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
            session.Destroy();
            session = null;
        }

        [Test]
        public void TestEPLVariables()
        {
            RegressionRunner.Run(session, EPLVariables.Executions());
        }

        [Test]
        public void TestEPLVariablesCreate()
        {
            RegressionRunner.Run(session, EPLVariablesCreate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLVariablesDestroy()
        {
            RegressionRunner.Run(session, EPLVariablesDestroy.Executions());
        }

        [Test]
        public void TestEPLVariablesEventTyped()
        {
            RegressionRunner.Run(session, EPLVariablesEventTyped.Executions());
        }

        [Test]
        public void TestEPLVariablesPerf()
        {
            RegressionRunner.Run(session, new EPLVariablesPerf());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLVariablesOutputRate()
        {
            RegressionRunner.Run(session, EPLVariablesOutputRate.Executions());
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportMarketDataBean),
                typeof(EPLVariables.MyVariableCustomEvent)})
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddImportType(typeof(EPLVariables.MySimpleVariableServiceFactory));
            configuration.Common.AddImportType(typeof(EPLVariables.MySimpleVariableService));
            configuration.Common.AddImportType(typeof(EPLVariables.MyVariableCustomType));

            configuration.Common.AddImportType(typeof(SupportEnum));

            ConfigurationCommon common = configuration.Common;
            common.AddVariable("var_simple_preconfig_const", "boolean", true, true);
            common.AddVariable("MYCONST_THREE", "boolean", true, true);
            common.AddVariable("papi_1", typeof(string), "begin");
            common.AddVariable("papi_2", typeof(bool), true);
            common.AddVariable("papi_3", typeof(string), "value");
            common.AddVariable("myRuntimeInitService", typeof(EPLVariables.MySimpleVariableService), null);
            common.AddVariable("MYCONST_TWO", "string", null, true);
            common.AddVariable("varcoll", "String[]", new string[] { "E1", "E2" }, true);
            common.AddVariable("mySimpleVariableService", typeof(EPLVariables.MySimpleVariableService), null);
            common.AddVariable("myInitService", typeof(EPLVariables.MySimpleVariableService), EPLVariables.MySimpleVariableServiceFactory.MakeService());
            common.AddVariable("supportEnum", typeof(SupportEnum), SupportEnum.ENUM_VALUE_1);
            common.AddVariable("enumWithOverride", typeof(EPLVariables.MyEnumWithOverride), EPLVariables.MyEnumWithOverride.LONG);
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
            common.AddVariable("my_variable_custom_typed", typeof(EPLVariables.MyVariableCustomType), EPLVariables.MyVariableCustomType.Of("abc"), true);

            configuration.Compiler.ViewResources.IterableUnbound = true;
            configuration.Compiler.ByteCode.AllowSubscriber = true;
        }
    }
} // end of namespace