///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.client.deploy;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientDeploy : AbstractTestBase
    {
        public TestSuiteClientDeploy() : base(Configure)
        {
        }

        protected override bool UseDefaultRuntime => true;

        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[]
                     {
                         typeof(SupportBean),
                         typeof(SupportBean_S0)
                     }
                    )
                configuration.Common.AddEventType(clazz);
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployUndeploy
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployUndeploy.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployUndeploy : AbstractTestBase
        {
            public TestClientDeployUndeploy() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalid()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithInvalid());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDependencyChain()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithDependencyChain());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPrecondDepScript()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithPrecondDepScript());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPrecondDepNamedWindow()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithPrecondDepNamedWindow());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPrecondDepVariable()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithPrecondDepVariable());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPrecondDepContext()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithPrecondDepContext());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPrecondDepEventType()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithPrecondDepEventType());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPrecondDepExprDecl()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithPrecondDepExprDecl());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPrecondDepTable()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithPrecondDepTable());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPrecondDepIndex()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithPrecondDepIndex());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPrecondDepClass()
            {
                RegressionRunner.Run(_session, ClientDeployUndeploy.WithPrecondDepClass());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployPreconditionDependency
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployPreconditionDependency.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployPreconditionDependency : AbstractTestBase
        {
            public TestClientDeployPreconditionDependency() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithClass()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithClass());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithScript()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithScript());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithVariablePreconfig()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithVariablePreconfig());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithVariablePath()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithVariablePath());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithEventTypePreconfig()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithEventTypePreconfig());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithEventTypePath()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithEventTypePath());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithNamedWindow()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithNamedWindow());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithTable()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithTable());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithExprDecl()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithExprDecl());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithContext()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithContext());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithNamedWindowOfNamedModule()
            {
                RegressionRunner.Run(_session,
                    ClientDeployPreconditionDependency.WithNamedWindowOfNamedModule());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithIndex()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDependency.WithIndex());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployPreconditionDuplicate
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployPreconditionDuplicate : AbstractTestBase
        {
            public TestClientDeployPreconditionDuplicate() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithNamedWindow()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.WithNamedWindow());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithTable()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.WithTable());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithEventType()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.WithEventType());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithVariable()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.WithVariable());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithExprDecl()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.WithExprDecl());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithScript()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.WithScript());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithContext()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.WithContext());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithIndex()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.WithIndex());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithClass()
            {
                RegressionRunner.Run(_session, ClientDeployPreconditionDuplicate.WithClass());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployUserObject
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployUserObject.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployUserObject : AbstractTestBase
        {
            public TestClientDeployUserObject() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithValues()
            {
                RegressionRunner.Run(_session, ClientDeployUserObject.WithValues());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithResolveContext()
            {
                RegressionRunner.Run(_session, ClientDeployUserObject.WithResolveContext());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployStatementName
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployStatementName.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployStatementName : AbstractTestBase
        {
            public TestClientDeployStatementName() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void Witht()
            {
                RegressionRunner.Run(_session, ClientDeployStatementName.Witht());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployResult
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployResult.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployResult : AbstractTestBase
        {
            public TestClientDeployResult() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithResultSimple()
            {
                RegressionRunner.Run(_session, ClientDeployResult.WithResultSimple());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithStateListener()
            {
                RegressionRunner.Run(_session, ClientDeployResult.WithStateListener());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithGetStmtByDepIdAndName()
            {
                RegressionRunner.Run(_session, ClientDeployResult.WithGetStmtByDepIdAndName());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSameDeploymentId()
            {
                RegressionRunner.Run(_session, ClientDeployResult.WithSameDeploymentId());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployRedefinition
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployRedefinition.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployRedefinition : AbstractTestBase
        {
            public TestClientDeployRedefinition() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithCreateSchemaNamedWindowInsert()
            {
                RegressionRunner.Run(_session,
                    ClientDeployRedefinition.WithCreateSchemaNamedWindowInsert());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithNamedWindow()
            {
                RegressionRunner.Run(_session, ClientDeployRedefinition.WithNamedWindow());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInsertInto()
            {
                RegressionRunner.Run(_session, ClientDeployRedefinition.WithInsertInto());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithVariables()
            {
                RegressionRunner.Run(_session, ClientDeployRedefinition.WithVariables());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployVersion
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployVersion.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployVersion : AbstractTestBase
        {
            public TestClientDeployVersion() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void Withk()
            {
                RegressionRunner.Run(_session, ClientDeployVersion.Withk());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployClassLoaderOption
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployClassLoaderOption.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployClassLoaderOption : AbstractTestBase
        {
            public TestClientDeployClassLoaderOption() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithClassLoaderOptionSimple()
            {
                RegressionRunner.Run(_session, ClientDeployClassLoaderOption.WithClassLoaderOptionSimple());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployRollout
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployRollout.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployRollout : AbstractTestBase
        {
            public TestClientDeployRollout() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithFourInterdepModulesWStmtId()
            {
                RegressionRunner.Run(_session, ClientDeployRollout.WithFourInterdepModulesWStmtId());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithTwoInterdepModules()
            {
                RegressionRunner.Run(_session, ClientDeployRollout.WithTwoInterdepModules());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalid()
            {
                RegressionRunner.Run(_session, ClientDeployRollout.WithInvalid());
            }
        }

        /// <summary>
        ///     Auto-test(s): ClientDeployListDependencies
        ///     <code>
        /// RegressionRunner.Run(_session, ClientDeployListDependencies.Executions());
        /// </code>
        /// </summary>
        public class TestClientDeployListDependencies : AbstractTestBase
        {
            public TestClientDeployListDependencies() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDependenciesObjectTypes()
            {
                RegressionRunner.Run(_session,
                    ClientDeployListDependencies.WithDependenciesObjectTypes());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithiesDependenciesWModuleName()
            {
                RegressionRunner.Run(_session,
                    ClientDeployListDependencies.WithiesDependenciesWModuleName());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDependenciesNoDependencies()
            {
                RegressionRunner.Run(_session,
                    ClientDeployListDependencies.WithDependenciesNoDependencies());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDependencyStar()
            {
                RegressionRunner.Run(_session, ClientDeployListDependencies.WithDependencyStar());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDependenciesInvalid()
            {
                RegressionRunner.Run(_session, ClientDeployListDependencies.WithDependenciesInvalid());
            }
        }
    }
} // end of namespace