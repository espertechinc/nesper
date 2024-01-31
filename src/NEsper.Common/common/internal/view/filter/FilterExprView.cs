///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.filter
{
    /// <summary>
    /// Simple filter view filtering events using a filter expression tree.
    /// </summary>
    public class FilterExprView : ViewSupport
    {
        private readonly ExprEvaluator exprEvaluator;
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        private readonly string whereClauseEvaluatorTextForAudit;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="exprEvaluator">Filter expression evaluation impl</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <param name="whereClauseEvaluatorTextForAudit">text or null if no-audit</param>
        public FilterExprView(
            ExprEvaluator exprEvaluator,
            ExprEvaluatorContext exprEvaluatorContext,
            string whereClauseEvaluatorTextForAudit)
        {
            this.exprEvaluator = exprEvaluator;
            this.exprEvaluatorContext = exprEvaluatorContext;
            this.whereClauseEvaluatorTextForAudit = whereClauseEvaluatorTextForAudit;
        }

        public override EventType EventType => Parent.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return FilterExprViewIterator.For(Parent.GetEnumerator(), exprEvaluator, exprEvaluatorContext);
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            var instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QWhereClauseFilter(whereClauseEvaluatorTextForAudit, newData, oldData);

            var filteredNewData = FilterEvents(exprEvaluator, newData, true, exprEvaluatorContext);
            var filteredOldData = FilterEvents(exprEvaluator, oldData, false, exprEvaluatorContext);

            instrumentationCommon.AWhereClauseFilter(filteredNewData, filteredOldData);

            if (filteredNewData != null || filteredOldData != null) {
                instrumentationCommon.QWhereClauseIR(filteredNewData, filteredOldData);
                Child.Update(filteredNewData, filteredOldData);
                instrumentationCommon.AWhereClauseIR();
            }
        }

        /// <summary>
        /// Filters events using the supplied evaluator.
        /// </summary>
        /// <param name="exprEvaluator">evaluator to use</param>
        /// <param name="events">events to filter</param>
        /// <param name="isNewData">true to indicate filter new data (istream) and not old data (rstream)</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>filtered events, or null if no events got through the filter</returns>
        private EventBean[] FilterEvents(
            ExprEvaluator exprEvaluator,
            EventBean[] events,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return null;
            }

            var instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;

            var evalEventArr = new EventBean[1];
            var passResult = new bool[events.Length];
            var passCount = 0;

            for (var i = 0; i < events.Length; i++) {
                evalEventArr[0] = events[i];
                instrumentationCommon.QWhereClauseFilterEval(i, events[i], isNewData);
                var pass = exprEvaluator.Evaluate(evalEventArr, isNewData, exprEvaluatorContext).AsBoxedBoolean();
                instrumentationCommon.AWhereClauseFilterEval(pass);
                if (pass != null && true.Equals(pass)) {
                    passResult[i] = true;
                    passCount++;
                }
            }

            if (passCount == 0) {
                return null;
            }

            if (passCount == events.Length) {
                return events;
            }

            var resultArray = new EventBean[passCount];
            var count = 0;
            for (var i = 0; i < passResult.Length; i++) {
                if (passResult[i]) {
                    resultArray[count] = events[i];
                    count++;
                }
            }

            return resultArray;
        }
    }
} // end of namespace