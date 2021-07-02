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
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.fromclausemethod;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLFromClauseMethod
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

        [Test, RunInApplicationDomain]
        public void TestEPLFromClauseMethod()
        {
            RegressionRunner.Run(_session, EPLFromClauseMethod.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLFromClauseMethodNStream()
        {
            RegressionRunner.Run(_session, EPLFromClauseMethodNStream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLFromClauseMethodOuterNStream()
        {
            RegressionRunner.Run(_session, EPLFromClauseMethodOuterNStream.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
                typeof(SupportBean),
                typeof(SupportBeanTwo),
                typeof(SupportBean_A),
                typeof(SupportBean_S0),
                typeof(SupportBeanInt),
                typeof(SupportTradeEventWithSide),
                typeof(SupportEventWithManyArray)
            })
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.Logging.IsEnableQueryPlan = true;

            ConfigurationCommon common = configuration.Common;
            common.AddVariable("var1", typeof(int?), 0);
            common.AddVariable("var2", typeof(int?), 0);
            common.AddVariable("var3", typeof(int?), 0);
            common.AddVariable("var4", typeof(int?), 0);
            common.AddVariable("varN1", typeof(int?), 0);
            common.AddVariable("varN2", typeof(int?), 0);
            common.AddVariable("varN3", typeof(int?), 0);
            common.AddVariable("varN4", typeof(int?), 0);

            configuration.Common.AddImportType(typeof(SupportJoinMethods));
            configuration.Common.AddImportType(typeof(SupportMethodInvocationJoinInvalid));

            ConfigurationCompilerPlugInSingleRowFunction entry = new ConfigurationCompilerPlugInSingleRowFunction();
            entry.Name = "myItemProducerUDF";
            entry.FunctionClassName = typeof(EPLFromClauseMethod).FullName;
            entry.FunctionMethodName = "MyItemProducerUDF";
            entry.EventTypeName = "ItemEvent";
            configuration.Compiler.AddPlugInSingleRowFunction(entry);
        }
    }
} // end of namespace