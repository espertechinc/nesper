///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.expr.exprcore;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprCoreWConfig
    {
        [Test, RunInApplicationDomain]
        public void TestExprCoreDotExpressionDuckTyping()
        {
            RegressionSession session = RegressionRunner.Session();
            Configuration configuration = session.Configuration;
            configuration.Compiler.Expression.DuckTyping = true;
            configuration.Common.AddEventType(typeof(SupportBeanDuckType));
            configuration.Common.AddEventType(typeof(SupportBeanDuckTypeOne));
            configuration.Common.AddEventType(typeof(SupportBeanDuckTypeTwo));
            RegressionRunner.Run(session, new ExprCoreDotExpressionDuckTyping());
            session.Destroy();
        }

        [Test]
        public void TestExprCoreBigNumberSupportMathContext()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Compiler.Expression.MathContext = MathContext.DECIMAL32;
            session.Configuration.Compiler.ByteCode.AllowSubscriber = true;
            RegressionRunner.Run(session, ExprCoreBigNumberSupportMathContext.Executions());
            session.Destroy();
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreMathDivisionRules()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.AddImportType(typeof(BigIntegerHelper));
            session.Configuration.Compiler.Expression.IntegerDivision = true;
            session.Configuration.Compiler.Expression.DivisionByZeroReturnsNull = true;
            session.Configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
            RegressionRunner.Run(session, ExprCoreMathDivisionRules.Executions());
            session.Destroy();
        }
    }
} // end of namespace