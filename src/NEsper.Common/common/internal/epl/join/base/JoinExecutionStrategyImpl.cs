///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.join.@base.JoinSetComposerUtil;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    /// <summary>
    ///     Join execution strategy based on a 3-step getSelectListEvents of composing a join set, filtering the join set and
    ///     indicating.
    /// </summary>
    public class JoinExecutionStrategyImpl : JoinExecutionStrategy
    {
        private readonly JoinSetComposer composer;
        private readonly JoinSetProcessor indicator;
        private readonly ExprEvaluator optionalFilter;
        private readonly ExprEvaluatorContext staticExprEvaluatorContext;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="composer">determines join tuple set</param>
        /// <param name="optionalFilter">for filtering among tuples</param>
        /// <param name="indicator">for presenting the info to a view</param>
        /// <param name="staticExprEvaluatorContext">expression evaluation context for static evaluation (not for runtime eval)</param>
        public JoinExecutionStrategyImpl(
            JoinSetComposer composer,
            ExprEvaluator optionalFilter,
            JoinSetProcessor indicator,
            ExprEvaluatorContext staticExprEvaluatorContext)
        {
            this.composer = composer;
            this.optionalFilter = optionalFilter;
            this.indicator = indicator;
            this.staticExprEvaluatorContext = staticExprEvaluatorContext;
        }

        public void Join(
            EventBean[][] newDataPerStream,
            EventBean[][] oldDataPerStream)
        {
            var instrumentationCommon = staticExprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QJoinExecStrategy();

            var joinSet = composer.Join(newDataPerStream, oldDataPerStream, staticExprEvaluatorContext);

            instrumentationCommon.AJoinExecStrategy(joinSet);

            if (optionalFilter != null) {
                instrumentationCommon.QJoinExecFilter();
                ProcessFilter(joinSet.First, joinSet.Second, staticExprEvaluatorContext);
                instrumentationCommon.AJoinExecFilter(joinSet.First, joinSet.Second);
            }

            if (!joinSet.First.IsEmpty() || !joinSet.Second.IsEmpty()) {
                instrumentationCommon.QJoinExecProcess(joinSet);
                indicator.Process(joinSet.First, joinSet.Second, staticExprEvaluatorContext);
                instrumentationCommon.AJoinExecProcess();
            }
        }

        public ISet<MultiKeyArrayOfKeys<EventBean>> StaticJoin()
        {
            var joinSet = composer.StaticJoin();
            if (optionalFilter != null) {
                ProcessFilter(joinSet, null, staticExprEvaluatorContext);
            }

            return joinSet;
        }

        private void ProcessFilter(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            Filter(optionalFilter, newEvents, true, exprEvaluatorContext);
            if (oldEvents != null) {
                Filter(optionalFilter, oldEvents, false, exprEvaluatorContext);
            }
        }
    }
} // end of namespace