///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.expr.define;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.lrreport;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprDefine
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

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean), 
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_ST0),
                typeof(SupportBean_ST1), 
                typeof(SupportBean_ST0_Container), 
                typeof(SupportCollection),
                typeof(SupportBeanObject), 
                typeof(LocationReport)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddImportType(typeof(LRUtil));
            configuration.Common.AddImportType(typeof(ExprDefineValueParameter));
            configuration.Common.AddImportType(typeof(ExprDefineValueParameter.ExprDefineLocalService));
        }

        [Test]
        public void TestExprDefineAliasFor()
        {
            RegressionRunner.Run(session, ExprDefineAliasFor.Executions());
        }

        [Test]
        public void TestExprDefineBasic()
        {
            RegressionRunner.Run(session, ExprDefineBasic.Executions());
        }

        [Test]
        public void TestExprDefineEventParameterNonStream()
        {
            RegressionRunner.Run(session, ExprDefineEventParameterNonStream.Executions());
        }

        [Test]
        public void TestExprDefineLambdaLocReport()
        {
            RegressionRunner.Run(session, new ExprDefineLambdaLocReport());
        }

        [Test]
        public void TestExprDefineValueParameter()
        {
            RegressionRunner.Run(session, ExprDefineValueParameter.Executions());
        }
    }
} // end of namespace