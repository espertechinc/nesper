///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.expr.clazz;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprClazz
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
            foreach (var clazz in new[] {typeof(SupportBean)}) {
                configuration.Common.AddEventType(clazz);
            }
        }

        [Test]
        public void TestExprClassClassDependency()
        {
            RegressionRunner.Run(session, ExprClassClassDependency.Executions());
        }

        [Test]
        public void TestExprClassResolution()
        {
            RegressionRunner.Run(session, ExprClassForEPLObjects.Executions());
        }

        [Test]
        public void TestExprClassStaticMethod()
        {
            RegressionRunner.Run(session, ExprClassStaticMethod.Executions());
        }

        [Test]
        public void TestExprClassTypeUse()
        {
            RegressionRunner.Run(session, ExprClassTypeUse.Executions());
        }
    }
} // end of namespace