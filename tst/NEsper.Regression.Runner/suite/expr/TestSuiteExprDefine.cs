///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprDefine : AbstractTestBase
    {
        public TestSuiteExprDefine() : base(Configure)
        {
        }

        public static void Configure(Configuration configuration)
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

        [Test, RunInApplicationDomain]
        public void TestExprDefineAliasFor()
        {
            RegressionRunner.Run(_session, ExprDefineAliasFor.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDefineEventParameterNonStream()
        {
            RegressionRunner.Run(_session, ExprDefineEventParameterNonStream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDefineLambdaLocReport()
        {
            RegressionRunner.Run(_session, new ExprDefineLambdaLocReport());
        }

        /// <summary>
        /// Auto-test(s): ExprDefineBasic
        /// <code>
        /// RegressionRunner.Run(_session, ExprDefineBasic.Executions());
        /// </code>
        /// </summary>
        public class TestExprDefineBasic : AbstractTestBase
        {
            public TestExprDefineBasic() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSplitStream() => RegressionRunner.Run(_session, ExprDefineBasic.WithSplitStream());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprDefineBasic.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithEventTypeAndSODA() => RegressionRunner.Run(_session, ExprDefineBasic.WithEventTypeAndSODA());

            [Test, RunInApplicationDomain]
            public void WithNestedExpressionMultiSubquery() => RegressionRunner.Run(_session, ExprDefineBasic.WithNestedExpressionMultiSubquery());

            [Test, RunInApplicationDomain]
            public void WithSubqueryNamedWindowCorrelated() => RegressionRunner.Run(_session, ExprDefineBasic.WithSubqueryNamedWindowCorrelated());

            [Test, RunInApplicationDomain]
            public void WithSubqueryNamedWindowUncorrelated() => RegressionRunner.Run(_session, ExprDefineBasic.WithSubqueryNamedWindowUncorrelated());

            [Test, RunInApplicationDomain]
            public void WithSubqueryUncorrelated() => RegressionRunner.Run(_session, ExprDefineBasic.WithSubqueryUncorrelated());

            [Test, RunInApplicationDomain]
            public void WithSubqueryCorrelated() => RegressionRunner.Run(_session, ExprDefineBasic.WithSubqueryCorrelated());

            [Test, RunInApplicationDomain]
            public void WithSubqueryJoinSameField() => RegressionRunner.Run(_session, ExprDefineBasic.WithSubqueryJoinSameField());

            [Test, RunInApplicationDomain]
            public void WithSubqueryCross() => RegressionRunner.Run(_session, ExprDefineBasic.WithSubqueryCross());

            [Test, RunInApplicationDomain]
            public void WithSubqueryMultiresult() => RegressionRunner.Run(_session, ExprDefineBasic.WithSubqueryMultiresult());

            [Test, RunInApplicationDomain]
            public void WithCaseNewMultiReturnNoElse() => RegressionRunner.Run(_session, ExprDefineBasic.WithCaseNewMultiReturnNoElse());

            [Test, RunInApplicationDomain]
            public void WithSequenceAndNested() => RegressionRunner.Run(_session, ExprDefineBasic.WithSequenceAndNested());

            [Test, RunInApplicationDomain]
            public void WithWhereClauseExpression() => RegressionRunner.Run(_session, ExprDefineBasic.WithWhereClauseExpression());

            [Test, RunInApplicationDomain]
            public void WithAnnotationOrder() => RegressionRunner.Run(_session, ExprDefineBasic.WithAnnotationOrder());

            [Test, RunInApplicationDomain]
            public void WithNoParameterVariable() => RegressionRunner.Run(_session, ExprDefineBasic.WithNoParameterVariable());

            [Test, RunInApplicationDomain]
            public void WithOneParameterLambdaReturn() => RegressionRunner.Run(_session, ExprDefineBasic.WithOneParameterLambdaReturn());

            [Test, RunInApplicationDomain]
            public void WithNoParameterArithmetic() => RegressionRunner.Run(_session, ExprDefineBasic.WithNoParameterArithmetic());

            [Test, RunInApplicationDomain]
            public void WithScalarReturn() => RegressionRunner.Run(_session, ExprDefineBasic.WithScalarReturn());

            [Test, RunInApplicationDomain]
            public void WithWildcardAndPattern() => RegressionRunner.Run(_session, ExprDefineBasic.WithWildcardAndPattern());

            [Test, RunInApplicationDomain]
            public void WithAggregationAccess() => RegressionRunner.Run(_session, ExprDefineBasic.WithAggregationAccess());

            [Test, RunInApplicationDomain]
            public void WithAggregatedResult() => RegressionRunner.Run(_session, ExprDefineBasic.WithAggregatedResult());

            [Test, RunInApplicationDomain]
            public void WithAggregationNoAccess() => RegressionRunner.Run(_session, ExprDefineBasic.WithAggregationNoAccess());

            [Test, RunInApplicationDomain]
            public void WithExpressionSimpleTwoModule() => RegressionRunner.Run(_session, ExprDefineBasic.WithExpressionSimpleTwoModule());

            [Test, RunInApplicationDomain]
            public void WithExpressionSimpleSameModule() => RegressionRunner.Run(_session, ExprDefineBasic.WithExpressionSimpleSameModule());

            [Test, RunInApplicationDomain]
            public void WithExpressionSimpleSameStmt() => RegressionRunner.Run(_session, ExprDefineBasic.WithExpressionSimpleSameStmt());
        }
        
        /// <summary>
        /// Auto-test(s): ExprDefineValueParameter
        /// <code>
        /// RegressionRunner.Run(_session, ExprDefineValueParameter.Executions());
        /// </code>
        /// </summary>

        public class TestExprDefineValueParameter : AbstractTestBase
        {
            public TestExprDefineValueParameter() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithSubquery());

            [Test, RunInApplicationDomain]
            public void WithVariable() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithVariable());

            [Test, RunInApplicationDomain]
            public void WithCache() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithCache());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithEVEVE() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithEVEVE());

            [Test, RunInApplicationDomain]
            public void WithEVE() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithEVE());

            [Test, RunInApplicationDomain]
            public void WithVEVE() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithVEVE());

            [Test, RunInApplicationDomain]
            public void WithVEV() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithVEV());

            [Test, RunInApplicationDomain]
            public void WithEV() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithEV());

            [Test, RunInApplicationDomain]
            public void WithVVV() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithVVV());

            [Test, RunInApplicationDomain]
            public void WithVV() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithVV());

            [Test, RunInApplicationDomain]
            public void WithV() => RegressionRunner.Run(_session, ExprDefineValueParameter.WithV());
        }
    }
} // end of namespace