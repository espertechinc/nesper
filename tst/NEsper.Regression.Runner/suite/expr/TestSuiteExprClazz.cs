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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

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
            session.Dispose();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {typeof(SupportBean)}) {
                configuration.Common.AddEventType(clazz);
            }
        }

        /// <summary>
        /// Auto-test(s): ExprClassStaticMethod
        /// <code>
        /// RegressionRunner.Run(_session, ExprClassStaticMethod.Executions());
        /// </code>
        /// </summary>

        public class TestExprClassStaticMethod : AbstractTestBase
        {
            public TestExprClassStaticMethod() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDocSamples() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithDocSamples());

            [Test, RunInApplicationDomain]
            public void WithInvalidCompile() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithInvalidCompile());

            [Test, RunInApplicationDomain]
            public void WithStaticMethodLocalAndCreateClassTogether() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithStaticMethodLocalAndCreateClassTogether());

            [Test, RunInApplicationDomain]
            public void WithStaticMethodCreateClassWithPackageName() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithStaticMethodCreateClassWithPackageName());

            [Test, RunInApplicationDomain]
            public void WithStaticMethodLocalWithPackageName() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithStaticMethodLocalWithPackageName());

            [Test, RunInApplicationDomain]
            public void WithStaticMethodCreateFAFQuery() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithStaticMethodCreateFAFQuery());

            [Test, RunInApplicationDomain]
            public void WithStaticMethodLocalFAFQuery() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithStaticMethodLocalFAFQuery());

            [Test, RunInApplicationDomain]
            public void WithStaticMethodCreateCompileVsRuntime() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithStaticMethodCreateCompileVsRuntime());

            [Test, RunInApplicationDomain]
            public void WithStaticMethodCreate() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithStaticMethodCreate());

            [Test, RunInApplicationDomain]
            public void WithStaticMethodLocal() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithStaticMethodLocal());
            
            [Test, RunInApplicationDomain]
            public void WithCompilerInlinedClassInspectionOption() => RegressionRunner.Run(_session, ExprClassStaticMethod.WithCompilerInlinedClassInspectionOption());
        }
        
        /// <summary>
        /// Auto-test(s): ExprClassClassDependency
        /// <code>
        /// RegressionRunner.Run(_session, ExprClassClassDependency.Executions());
        /// </code>
        /// </summary>

        public class TestExprClassClassDependency : AbstractTestBase
        {
            public TestExprClassClassDependency() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithClasspath() => RegressionRunner.Run(_session, ExprClassClassDependency.WithClasspath());

            [Test]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprClassClassDependency.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithAllLocal() => RegressionRunner.Run(_session, ExprClassClassDependency.WithAllLocal());
        }
        
        /// <summary>
        /// Auto-test(s): ExprClassForEPLObjects
        /// <code>
        /// RegressionRunner.Run(_session, ExprClassForEPLObjects.Executions());
        /// </code>
        /// </summary>

        public class TestExprClassForEPLObjects : AbstractTestBase
        {
            public TestExprClassForEPLObjects() : base(Configure) { }

            [Test]
            public void WithScript() => RegressionRunner.Run(_session, ExprClassForEPLObjects.WithScript());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprClassForEPLObjects.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithOutputColType() => RegressionRunner.Run(_session, ExprClassForEPLObjects.WithOutputColType());

            [Test, RunInApplicationDomain]
            public void WithFromClauseMethod() => RegressionRunner.Run(_session, ExprClassForEPLObjects.WithFromClauseMethod());
        }
        
        /// <summary>
        /// Auto-test(s): ExprClassTypeUse
        /// <code>
        /// RegressionRunner.Run(_session, ExprClassTypeUse.Executions());
        /// </code>
        /// </summary>

        public class TestExprClassTypeUse : AbstractTestBase
        {
            public TestExprClassTypeUse() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithNewKeyword() => RegressionRunner.Run(_session, ExprClassTypeUse.WithNewKeyword());

            [Test, RunInApplicationDomain]
            public void WithInnerClass() => RegressionRunner.Run(_session, ExprClassTypeUse.WithInnerClass());

            [Test, RunInApplicationDomain]
            public void WithConst() => RegressionRunner.Run(_session, ExprClassTypeUse.WithConst());

            [Test, RunInApplicationDomain]
            public void WithUseEnum() => RegressionRunner.Run(_session, ExprClassTypeUse.WithUseEnum());
        }
    }
} // end of namespace