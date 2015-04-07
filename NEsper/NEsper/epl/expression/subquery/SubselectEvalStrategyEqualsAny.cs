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
    /// Strategy for subselects with "=/!=/&lt;&gt; ANY".
    /// </summary>
    public class SubselectEvalStrategyEqualsAny : SubselectEvalStrategy
    {
        private readonly Coercer _coercer;
        private readonly ExprEvaluator _filterExpr;
        private readonly bool _isNot;
        private readonly bool _mustCoerce;
        private readonly ExprEvaluator _selectClauseExpr;
        private readonly ExprEvaluator _valueExpr;

        /// <summary>Ctor. </summary>
        /// <param name="notIn">false for =, true for !=</param>
        /// <param name="mustCoerce">coercion required</param>
        /// <param name="coercionType">type to coerce to</param>
        /// <param name="valueExpr">LHS</param>
        /// <param name="selectClauseExpr">select clause or null</param>
        /// <param name="filterExpr">filter or null</param>
        public SubselectEvalStrategyEqualsAny(bool notIn,
                                              bool mustCoerce,
                                              Type coercionType,
                                              ExprEvaluator valueExpr,
                                              ExprEvaluator selectClauseExpr,
                                              ExprEvaluator filterExpr)
        {
            _isNot = notIn;
            _mustCoerce = mustCoerce;
            _coercer = mustCoerce ? CoercerFactory.GetCoercer(null, coercionType) : null;
            _valueExpr = valueExpr;
            _filterExpr = filterExpr;
            _selectClauseExpr = selectClauseExpr;
        }

        #region SubselectEvalStrategy Members

        public Object Evaluate(EventBean[] eventsPerStream,
                               bool isNewData,
                               ICollection<EventBean> matchingEvents,
                               ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((matchingEvents == null) || (matchingEvents.Count == 0))
            {
                return false;
            }

            // Evaluate the child expression
            Object leftResult = _valueExpr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));

            // Evaluation event-per-stream
            var events = new EventBean[eventsPerStream.Length + 1];
            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);

            if (_isNot)
            {
                // Evaluate each select until we have a match
                bool hasNonNullRow = false;
                bool hasNullRow = false;
                foreach (EventBean theEvent in matchingEvents)
                {
                    events[0] = theEvent;

                    Object rightResult;
                    if (_selectClauseExpr != null)
                    {
                        rightResult = _selectClauseExpr.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext));
                    }
                    else
                    {
                        rightResult = events[0].Underlying;
                    }

                    // Eval filter expression
                    if (_filterExpr != null)
                    {
                        var pass = (bool?) _filterExpr.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext));
                        if (!pass.GetValueOrDefault(false))
                        {
                            continue;
                        }
                    }
                    if (leftResult == null)
                    {
                        return null;
                    }

                    if (rightResult != null)
                    {
                        hasNonNullRow = true;
                        if (!_mustCoerce)
                        {
                            if (!leftResult.Equals(rightResult))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            object left = _coercer.Invoke(leftResult);
                            object right = _coercer.Invoke(rightResult);
                            if (!left.Equals(right))
                            {
                                return true;
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

                return false;
            }
            else
            {
                // Evaluate each select until we have a match
                bool hasNonNullRow = false;
                bool hasNullRow = false;
                foreach (EventBean theEvent in matchingEvents)
                {
                    events[0] = theEvent;

                    Object rightResult;
                    if (_selectClauseExpr != null)
                    {
                        rightResult = _selectClauseExpr.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext));
                    }
                    else
                    {
                        rightResult = events[0].Underlying;
                    }

                    // Eval filter expression
                    if (_filterExpr != null)
                    {
                        var pass = (bool?) _filterExpr.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext));
                        if (!pass.GetValueOrDefault(false))
                        {
                            continue;
                        }
                    }
                    if (leftResult == null)
                    {
                        return null;
                    }

                    if (rightResult != null)
                    {
                        hasNonNullRow = true;
                        if (!_mustCoerce)
                        {
                            if (leftResult.Equals(rightResult))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            object left = _coercer.Invoke(leftResult);
                            object right = _coercer.Invoke(rightResult);
                            if (left.Equals(right))
                            {
                                return true;
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

                return false;
            }
        }

        #endregion
    }
}