///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.expr.clazz;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprClazzWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestEPLScriptExpressionDisable()
        {
            using var session = RegressionRunner.Session(Container);
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Compiler.ByteCode.IsAllowInlinedClass = false;
            RegressionRunner.Run(session, new ExprClassDisable());
        }
    }
} // end of namespace