///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Simple filter view filtering events using a filter expression tree.
    /// </summary>
    public class FilterExprView : ViewSupport
    {
        private readonly ExprNode _exprNode;
        private readonly ExprEvaluator _exprEvaluator;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="exprNode">The expr node.</param>
        /// <param name="exprEvaluator">Filter expression evaluation impl</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        public FilterExprView(ExprNode exprNode, ExprEvaluator exprEvaluator, ExprEvaluatorContext exprEvaluatorContext)
        {
            _exprNode = exprNode;
            _exprEvaluator = exprEvaluator;
            _exprEvaluatorContext = exprEvaluatorContext;
        }

        public override EventType EventType => Parent.EventType;

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <returns></returns>

        private static IEnumerator<EventBean> GetEnumerator(IEnumerator<EventBean> source, ExprEvaluator filter, ExprEvaluatorContext exprEvaluatorContext)
        {
            var evalEventArr = new EventBean[1];

            while (source.MoveNext())
            {
                var candidate = source.Current;
                evalEventArr[0] = candidate;

                var pass = (bool?)filter.Evaluate(new EvaluateParams(evalEventArr, true, exprEvaluatorContext));
                if (pass ?? false)
                {
                    yield return candidate;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return GetEnumerator(Parent.GetEnumerator(), _exprEvaluator, _exprEvaluatorContext);
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QWhereClauseFilter(_exprNode, newData, oldData); }

            var filteredNewData = FilterEvents(_exprEvaluator, newData, true, _exprEvaluatorContext);
            var filteredOldData = FilterEvents(_exprEvaluator, oldData, false, _exprEvaluatorContext);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AWhereClauseFilter(filteredNewData, filteredOldData); }

            if ((filteredNewData != null) || (filteredOldData != null))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QWhereClauseIR(filteredNewData, filteredOldData); }
                UpdateChildren(filteredNewData, filteredOldData);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AWhereClauseIR(); }
            }
        }

        /// <summary>Filters events using the supplied evaluator. </summary>
        /// <param name="exprEvaluator">evaluator to use</param>
        /// <param name="events">events to filter</param>
        /// <param name="isNewData">true to indicate filter new data (istream) and not old data (rstream)</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>filtered events, or null if no events got through the filter</returns>
        private EventBean[] FilterEvents(ExprEvaluator exprEvaluator, EventBean[] events, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null)
            {
                return null;
            }

            var evalEventArr = new EventBean[1];
            var passResult = new bool[events.Length];
            var passCount = 0;

            for (var i = 0; i < events.Length; i++)
            {
                evalEventArr[0] = events[i];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QWhereClauseFilterEval(i, events[i], isNewData); }
                var pass = (bool?)exprEvaluator.Evaluate(new EvaluateParams(evalEventArr, isNewData, exprEvaluatorContext));
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AWhereClauseFilterEval(pass); }
                if ((pass != null) && (pass.Value))
                {
                    passResult[i] = true;
                    passCount++;
                }
            }

            if (passCount == 0)
            {
                return null;
            }
            if (passCount == events.Length)
            {
                return events;
            }

            var resultArray = new EventBean[passCount];
            var count = 0;
            for (var i = 0; i < passResult.Length; i++)
            {
                if (passResult[i])
                {
                    resultArray[count] = events[i];
                    count++;
                }
            }
            return resultArray;
        }
    }
}
