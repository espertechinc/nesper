///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>
    /// Represents a in-subselect evaluation strategy.
    /// </summary>
    public class SubselectEvalStrategyEqualsIn : SubselectEvalStrategy
    {
        private readonly bool _isNotIn;
        private readonly bool _mustCoerce;
        private readonly Coercer _coercer;
        private readonly ExprEvaluator _valueExpr;
        private readonly ExprEvaluator _filterExpr;
        private readonly ExprEvaluator _selectClauseExpr;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="notIn">false for =, true for !=</param>
        /// <param name="mustCoerce">coercion required</param>
        /// <param name="coercionType">type to coerce to</param>
        /// <param name="valueExpr">LHS</param>
        /// <param name="selectClauseExpr">select clause or null</param>
        /// <param name="filterExpr">filter or null</param>
        public SubselectEvalStrategyEqualsIn(bool notIn,
                                             bool mustCoerce,
                                             Type coercionType,
                                             ExprEvaluator valueExpr,
                                             ExprEvaluator selectClauseExpr,
                                             ExprEvaluator filterExpr)
        {
            _isNotIn = notIn;
            _mustCoerce = mustCoerce;
            _coercer = mustCoerce ? CoercerFactory.GetCoercer(null, coercionType) : null;
            _valueExpr = valueExpr;
            _filterExpr = filterExpr;
            _selectClauseExpr = selectClauseExpr;
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (matchingEvents == null)
            {
                return _isNotIn;
            }
            if (matchingEvents.Count == 0)
            {
                return _isNotIn;
            }
    
            // Evaluate the child expression
            Object leftResult = _valueExpr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
    
            // Evaluation event-per-stream
            var events = new EventBean[eventsPerStream.Length + 1];
            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);
    
            if (_filterExpr == null)
            {
                if (leftResult == null)
                {
                    return null;
                }
    
                // Evaluate each select until we have a match
                bool hasNonNullRow = false;
                bool hasNullRow = false;
                foreach (EventBean eventBean in matchingEvents)
                {
                    events[0] = eventBean;
    
                    Object rightResult =
                        _selectClauseExpr != null
                            ? _selectClauseExpr.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext))
                            : events[0].Underlying;
    
                    if (rightResult != null)
                    {
                        hasNonNullRow = true;
                        if (!_mustCoerce)
                        {
                            if (leftResult.Equals(rightResult))
                            {
                                return !_isNotIn;
                            }
                        }
                        else
                        {
                            var left = _coercer.Invoke(leftResult);
                            var right = _coercer.Invoke(rightResult);
                            if (Equals(left, right))
                            {
                                return !_isNotIn;
                            }
                        }
                    }
                    else
                    {
                        hasNullRow = true;
                    }
                }
    
                if ((!hasNonNullRow) || (hasNullRow))
                {
                    return null;
                }
                return _isNotIn;
            }
    
            // Filter and check each row.
            bool hasNullRowOuter = false;
            foreach (EventBean subselectEvent in matchingEvents)
            {
                // Prepare filter expression event list
                events[0] = subselectEvent;
    
                // Eval filter expression
                var pass = (bool?)_filterExpr.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext));
                if ((pass == null) || (!pass.Value))
                {
                    continue;
                }
                if (leftResult == null)
                {
                    return null;
                }

                object rightResult =
                    _selectClauseExpr != null
                        ? _selectClauseExpr.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext))
                        : events[0].Underlying;
    
                if (rightResult == null)
                {
                    hasNullRowOuter = true;
                }
                else
                {
                    if (!_mustCoerce)
                    {
                        if (leftResult.Equals(rightResult))
                        {
                            return !_isNotIn;
                        }
                    }
                    else
                    {
                        var left = _coercer.Invoke(leftResult);
                        var right = _coercer.Invoke(rightResult);
                        if (Equals(left, right))
                        {
                            return !_isNotIn;
                        }
                    }
                }
            }
    
            if (hasNullRowOuter)
            {
                return null;
            }
    
            return _isNotIn;
        }
    }
}
