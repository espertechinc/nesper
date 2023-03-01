///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.suite.expr.define;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprDefineWConfig : AbstractTestContainer
    {
        private void Run(
            int? configuredCacheSize,
            ExprDefineConfigurations exec)
        {
            using var session = RegressionRunner.Session(Container);

            var configuration = session.Configuration;
            if (configuredCacheSize != null) {
                configuration.Runtime.Execution.DeclaredExprValueCacheSize = configuredCacheSize.Value;
            }

            foreach (var clazz in new[] {typeof(SupportBean_ST0), typeof(SupportBean_ST1)}) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Compiler.AddPlugInSingleRowFunction("alwaysTrue", typeof(SupportStaticMethodLib), "AlwaysTrue");

            RegressionRunner.Run(session, exec);
        }

        [Test, RunInApplicationDomain]
        public void TestExprDefineConfigurations()
        {
            Run(null, new ExprDefineConfigurations(4));
            Run(0, new ExprDefineConfigurations(4));
            Run(1, new ExprDefineConfigurations(4));
            Run(2, new ExprDefineConfigurations(2));
        }
    }
} // end of namespace