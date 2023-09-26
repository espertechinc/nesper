///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.expr.exprcore;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprCoreWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestExprCoreBigNumberSupportMathContext()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Common.AddEventType(typeof(SupportBean));
                session.Configuration.Compiler.Expression.MathContext = MathContext.DECIMAL32;
                session.Configuration.Compiler.ByteCode.IsAllowSubscriber =true;
                RegressionRunner.Run(session, ExprCoreBigNumberSupportMathContext.Executions());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreConcatThreadingProfileLarge()
        {
            using (var session = RegressionRunner.Session(Container)) {
                var configuration = session.Configuration;
                configuration.Common.Execution.ThreadingProfile = ThreadingProfile.LARGE;
                configuration.Common.AddEventType(typeof(SupportBean_S0));
                RegressionRunner.Run(session, new ExprCoreConcat());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreDotExpressionDuckTyping()
        {
            using (var session = RegressionRunner.Session(Container)) {
                var configuration = session.Configuration;
                configuration.Compiler.Expression.DuckTyping = true;
                configuration.Common.AddEventType(typeof(SupportBeanDuckType));
                configuration.Common.AddEventType(typeof(SupportBeanDuckTypeOne));
                configuration.Common.AddEventType(typeof(SupportBeanDuckTypeTwo));
                RegressionRunner.Run(session, new ExprCoreDotExpressionDuckTyping());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreMathDivisionRules()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Compiler.Expression.IntegerDivision = true;
                session.Configuration.Compiler.Expression.DivisionByZeroReturnsNull = true;
                session.Configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
                RegressionRunner.Run(session, ExprCoreMathDivisionRules.Executions());
            }
        }
    }
} // end of namespace