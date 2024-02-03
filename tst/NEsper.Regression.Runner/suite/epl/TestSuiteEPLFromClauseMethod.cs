///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLFromClauseMethod : AbstractTestBase
    {
        public TestSuiteEPLFromClauseMethod() : base(Configure)
        {
        }

        public static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] {
                         typeof(SupportBean),
                         typeof(SupportBeanTwo),
                         typeof(SupportBean_A),
                         typeof(SupportBean_S0),
                         typeof(SupportBeanInt),
                         typeof(SupportTradeEventWithSide),
                         typeof(SupportEventWithManyArray)
                     }
                    ) {
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

        /// <summary>
        /// Auto-test(s): EPLFromClauseMethod
        /// <code>
        /// RegressionRunner.Run(_session, EPLFromClauseMethod.Executions());
        /// </code>
        /// </summary>
        public class TestEPLFromClauseMethod : AbstractTestBase
        {
            public TestEPLFromClauseMethod() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With2JoinHistoricalIndependentOuter() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.With2JoinHistoricalIndependentOuter());

            [Test, RunInApplicationDomain]
            public void With2JoinHistoricalSubordinateOuterMultiField() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.With2JoinHistoricalSubordinateOuterMultiField());

            [Test, RunInApplicationDomain]
            public void With2JoinHistoricalSubordinateOuter() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.With2JoinHistoricalSubordinateOuter());

            [Test, RunInApplicationDomain]
            public void With2JoinHistoricalOnlyDependent() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.With2JoinHistoricalOnlyDependent());

            [Test, RunInApplicationDomain]
            public void With2JoinHistoricalOnlyIndependent() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.With2JoinHistoricalOnlyIndependent());

            [Test, RunInApplicationDomain]
            public void WithNoJoinIterateVariables() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.WithNoJoinIterateVariables());

            [Test, RunInApplicationDomain]
            public void WithOverloaded() => RegressionRunner.Run(_session, EPLFromClauseMethod.WithOverloaded());

            [Test, RunInApplicationDomain]
            public void With2StreamMaxAggregation() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.With2StreamMaxAggregation());

            [Test, RunInApplicationDomain]
            public void WithDifferentReturnTypes() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.WithDifferentReturnTypes());

            [Test, RunInApplicationDomain]
            public void WithArrayNoArg() => RegressionRunner.Run(_session, EPLFromClauseMethod.WithArrayNoArg());

            [Test, RunInApplicationDomain]
            public void WithArrayWithArg() => RegressionRunner.Run(_session, EPLFromClauseMethod.WithArrayWithArg());

            [Test, RunInApplicationDomain]
            public void WithObjectNoArg() => RegressionRunner.Run(_session, EPLFromClauseMethod.WithObjectNoArg());

            [Test, RunInApplicationDomain]
            public void WithObjectWithArg() => RegressionRunner.Run(_session, EPLFromClauseMethod.WithObjectWithArg());

            [Test, RunInApplicationDomain]
            public void WithInvocationTargetEx() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.WithInvocationTargetEx());

            [Test, RunInApplicationDomain]
            public void WithStreamNameWContext() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.WithStreamNameWContext());

            [Test, RunInApplicationDomain]
            public void WithWithMethodResultParam() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.WithWithMethodResultParam());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLFromClauseMethod.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithEventBeanArray() =>
                RegressionRunner.Run(_session, EPLFromClauseMethod.WithEventBeanArray());

            [Test, RunInApplicationDomain]
            public void WithUDFAndScriptReturningEvents() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.WithUDFAndScriptReturningEvents());

            [Test, RunInApplicationDomain]
            public void With2JoinEventItselfProvidesMethod() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethod.With2JoinEventItselfProvidesMethod());
        }

        /// <summary>
        /// Auto-test(s): EPLFromClauseMethodNStream
        /// <code>
        /// RegressionRunner.Run(_session, EPLFromClauseMethodNStream.Executions());
        /// </code>
        /// </summary>
        public class TestEPLFromClauseMethodNStream : AbstractTestBase
        {
            public TestEPLFromClauseMethodNStream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With1Stream2HistStarSubordinateCartesianLast() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With1Stream2HistStarSubordinateCartesianLast());

            [Test, RunInApplicationDomain]
            public void With1Stream2HistStarSubordinateJoinedKeepall() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With1Stream2HistStarSubordinateJoinedKeepall());

            [Test, RunInApplicationDomain]
            public void With1Stream2HistForwardSubordinate() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With1Stream2HistForwardSubordinate());

            [Test, RunInApplicationDomain]
            public void With1Stream3HistStarSubordinateCartesianLast() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With1Stream3HistStarSubordinateCartesianLast());

            [Test, RunInApplicationDomain]
            public void With1Stream3HistForwardSubordinate() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With1Stream3HistForwardSubordinate());

            [Test, RunInApplicationDomain]
            public void With1Stream3HistChainSubordinate() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With1Stream3HistChainSubordinate());

            [Test, RunInApplicationDomain]
            public void With2Stream2HistStarSubordinate() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With2Stream2HistStarSubordinate());

            [Test, RunInApplicationDomain]
            public void With3Stream1HistSubordinate() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With3Stream1HistSubordinate());

            [Test, RunInApplicationDomain]
            public void With3HistPureNoSubordinate() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With3HistPureNoSubordinate());

            [Test, RunInApplicationDomain]
            public void With3Hist1Subordinate() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With3Hist1Subordinate());

            [Test, RunInApplicationDomain]
            public void With3Hist2SubordinateChain() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With3Hist2SubordinateChain());

            [Test, RunInApplicationDomain]
            public void With3Stream1HistStreamNWTwice() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodNStream.With3Stream1HistStreamNWTwice());
        }

        /// <summary>
        /// Auto-test(s): EPLFromClauseMethodOuterNStream
        /// <code>
        /// RegressionRunner.Run(_session, EPLFromClauseMethodOuterNStream.Executions());
        /// </code>
        /// </summary>
        public class TestEPLFromClauseMethodOuterNStream : AbstractTestBase
        {
            public TestEPLFromClauseMethodOuterNStream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With1Stream2HistStarSubordinateLeftRight() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodOuterNStream.With1Stream2HistStarSubordinateLeftRight());

            [Test, RunInApplicationDomain]
            public void With1Stream2HistStarSubordinateInner() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodOuterNStream.With1Stream2HistStarSubordinateInner());

            [Test, RunInApplicationDomain]
            public void With1Stream2HistForwardSubordinate() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodOuterNStream.With1Stream2HistForwardSubordinate());

            [Test, RunInApplicationDomain]
            public void With1Stream3HistForwardSubordinate() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodOuterNStream.With1Stream3HistForwardSubordinate());

            [Test, RunInApplicationDomain]
            public void With1Stream3HistForwardSubordinateChain() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodOuterNStream.With1Stream3HistForwardSubordinateChain());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLFromClauseMethodOuterNStream.WithInvalid());

            [Test, RunInApplicationDomain]
            public void With2Stream1HistStarSubordinateLeftRight() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodOuterNStream.With2Stream1HistStarSubordinateLeftRight());

            [Test, RunInApplicationDomain]
            public void With1Stream2HistStarNoSubordinateLeftRight() => RegressionRunner.Run(
                _session,
                EPLFromClauseMethodOuterNStream.With1Stream2HistStarNoSubordinateLeftRight());
        }
    }
} // end of namespace