///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.fromclausemethod;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLFromClauseMethodWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestEPLFromClauseMethodCacheExpiry()
        {
            using RegressionSession session = RegressionRunner.Session(Container);
            var methodConfig = new ConfigurationCommonMethodRef();
            methodConfig.SetExpiryTimeCache(1, 10);
            session.Configuration.Common.AddMethodRef(typeof(SupportStaticMethodInvocations), methodConfig);
            session.Configuration.Common.AddImportNamespace(typeof(SupportStaticMethodInvocations));
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            RegressionRunner.Run(session, new EPLFromClauseMethodCacheExpiry());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLFromClauseMethodCacheLRU()
        {
            using RegressionSession session = RegressionRunner.Session(Container);
            var methodConfig = new ConfigurationCommonMethodRef();
            methodConfig.LRUCache = (3);
            session.Configuration.Common.AddMethodRef(typeof(SupportStaticMethodInvocations), methodConfig);
            session.Configuration.Common.AddImportNamespace(typeof(SupportStaticMethodInvocations));
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            RegressionRunner.Run(session, new EPLFromClauseMethodCacheLRU());
        }

        /// <summary>
        /// Auto-test(s): EPLFromClauseMethodJoinPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLFromClauseMethodJoinPerformance.Executions());
        /// </code>
        /// </summary>
        public class TestEPLFromClauseMethodJoinPerformance : AbstractTestBase {
            public TestEPLFromClauseMethodJoinPerformance() : base(Configure)
            {
            }

            static void Configure(Configuration configuration)
            {
                var configMethod = new ConfigurationCommonMethodRef();
                configMethod.LRUCache = (10);
                configuration.Common.AddMethodRef(typeof(SupportJoinMethods), configMethod);
                configuration.Common.AddEventType(typeof(SupportBeanInt));
                configuration.Common.AddImportType(typeof(SupportJoinMethods));
            }

            [Test, RunInApplicationDomain]
            public void With1Stream2HistInnerJoinPerformance() => RegressionRunner.Run(_session,
                EPLFromClauseMethodJoinPerformance.With1Stream2HistInnerJoinPerformance());

            [Test, RunInApplicationDomain]
            public void With1Stream2HistOuterJoinPerformance() => RegressionRunner.Run(_session,
                EPLFromClauseMethodJoinPerformance.With1Stream2HistOuterJoinPerformance());

            [Test, RunInApplicationDomain]
            public void With2Stream1HistTwoSidedEntryIdenticalIndex() => RegressionRunner.Run(_session,
                EPLFromClauseMethodJoinPerformance.With2Stream1HistTwoSidedEntryIdenticalIndex());

            [Test, RunInApplicationDomain]
            public void With2Stream1HistTwoSidedEntryMixedIndex() => RegressionRunner.Run(_session,
                EPLFromClauseMethodJoinPerformance.With2Stream1HistTwoSidedEntryMixedIndex());
        }

        /// <summary>
        /// Auto-test(s): EPLFromClauseMethodVariable
        /// <code>
        /// RegressionRunner.Run(_session, EPLFromClauseMethodVariable.Executions());
        /// </code>
        /// </summary>
        public class TestEPLFromClauseMethodVariable : AbstractTestBase {
            public TestEPLFromClauseMethodVariable() : base(Configure)
            {
            }

            static void Configure(Configuration configuration)
            {
                configuration.Common.AddMethodRef(typeof(EPLFromClauseMethodVariable.MyStaticService),
                    new ConfigurationCommonMethodRef());
                configuration.Common.AddImportType(typeof(EPLFromClauseMethodVariable.MyStaticService));
                configuration.Common.AddImportType(typeof(EPLFromClauseMethodVariable.MyNonConstantServiceVariableFactory));
                configuration.Common.AddImportType(typeof(EPLFromClauseMethodVariable.MyNonConstantServiceVariable));
                ConfigurationCommon common = configuration.Common;
                common.AddVariable("MyConstantServiceVariable",
                    typeof(EPLFromClauseMethodVariable.MyConstantServiceVariable),
                    new EPLFromClauseMethodVariable.MyConstantServiceVariable());
                common.AddVariable("MyNonConstantServiceVariable",
                    typeof(EPLFromClauseMethodVariable.MyNonConstantServiceVariable),
                    new EPLFromClauseMethodVariable.MyNonConstantServiceVariable("postfix"));
                common.AddVariable("MyNullMap", typeof(EPLFromClauseMethodVariable.MyMethodHandlerMap), null);
                common.AddVariable("MyMethodHandlerMap", typeof(EPLFromClauseMethodVariable.MyMethodHandlerMap),
                    new EPLFromClauseMethodVariable.MyMethodHandlerMap("a", "b"));
                common.AddVariable("MyMethodHandlerOA", typeof(EPLFromClauseMethodVariable.MyMethodHandlerOA),
                    new EPLFromClauseMethodVariable.MyMethodHandlerOA("a", "b"));
                configuration.Common.Logging.IsEnableQueryPlan = true;
                configuration.Common.AddEventType(typeof(SupportBean));
                configuration.Common.AddEventType(typeof(SupportBean_S0));
                configuration.Common.AddEventType(typeof(SupportBean_S1));
                configuration.Common.AddEventType(typeof(SupportBean_S2));
            }

            [Test, RunInApplicationDomain]
            public void WithConstantVariable() =>
                RegressionRunner.Run(_session, EPLFromClauseMethodVariable.WithConstantVariable());

            [Test, RunInApplicationDomain]
            public void WithNonConstantVariable() =>
                RegressionRunner.Run(_session, EPLFromClauseMethodVariable.WithNonConstantVariable());

            [Test, RunInApplicationDomain]
            public void WithContextVariable() =>
                RegressionRunner.Run(_session, EPLFromClauseMethodVariable.WithContextVariable());

            [Test, RunInApplicationDomain]
            public void WithVariableMapAndOA() =>
                RegressionRunner.Run(_session, EPLFromClauseMethodVariable.WithVariableMapAndOA());

            [Test, RunInApplicationDomain]
            public void WithVariableInvalid() =>
                RegressionRunner.Run(_session, EPLFromClauseMethodVariable.WithVariableInvalid());
        }

        /// <summary>
        /// Auto-test(s): EPLFromClauseMethodMultikeyWArray
        /// <code>
        /// RegressionRunner.Run(_session, EPLFromClauseMethodMultikeyWArray.Executions());
        /// </code>
        /// </summary>
        public class TestEPLFromClauseMethodMultikeyWArray : AbstractTestBase {
            public TestEPLFromClauseMethodMultikeyWArray() : base(Configure)
            {
            }

            static void Configure(Configuration configuration)
            {
                var methodConfig = new ConfigurationCommonMethodRef();
                methodConfig.SetExpiryTimeCache(1, 10);
                configuration.Common.AddMethodRef(typeof(EPLFromClauseMethodMultikeyWArray.SupportJoinResultIsArray), methodConfig);
                configuration.Common.AddEventType<SupportEventWithManyArray>();
                configuration.Common.Logging.IsEnableQueryPlan = true;
            }

            [Test, RunInApplicationDomain]
            public void WithJoinArray() =>
                RegressionRunner.Run(_session, EPLFromClauseMethodMultikeyWArray.WithJoinArray());

            [Test, RunInApplicationDomain]
            public void WithJoinTwoField() =>
                RegressionRunner.Run(_session, EPLFromClauseMethodMultikeyWArray.WithJoinTwoField());

            [Test, RunInApplicationDomain]
            public void WithJoinComposite() =>
                RegressionRunner.Run(_session, EPLFromClauseMethodMultikeyWArray.WithJoinComposite());

            [Test, RunInApplicationDomain]
            public void WithParameterizedByArray() => RegressionRunner.Run(_session,
                EPLFromClauseMethodMultikeyWArray.WithParameterizedByArray());

            [Test, RunInApplicationDomain]
            public void WithParameterizedByTwoField() => RegressionRunner.Run(_session,
                EPLFromClauseMethodMultikeyWArray.WithParameterizedByTwoField());
        }
    }
} // end of namespace
