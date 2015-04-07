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
using com.espertech.esper.type;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>Strategy for subselects with "&gt;/&lt;/&lt;=/&gt;= ANY". </summary>
    public class SubselectEvalStrategyRelOpAny : SubselectEvalStrategy
    {
        private readonly RelationalOpEnumExtensions.Computer _computer;
        private readonly ExprEvaluator _filterExpr;
        private readonly ExprEvaluator _selectClauseExpr;
        private readonly ExprEvaluator _valueExpr;

        /// <summary>Ctor. </summary>
        /// <param name="computer">operator</param>
        /// <param name="valueExpr">LHS</param>
        /// <param name="selectClause">select or null</param>
        /// <param name="filterExpr">filter or null</param>
        public SubselectEvalStrategyRelOpAny(RelationalOpEnumExtensions.Computer computer,
                                             ExprEvaluator valueExpr,
                                             ExprEvaluator selectClause,
                                             ExprEvaluator filterExpr)
        {
            _computer = computer;
            _valueExpr = valueExpr;
            _selectClauseExpr = selectClause;
            _filterExpr = filterExpr;
        }

        #region SubselectEvalStrategy Members

        public Object Evaluate(EventBean[] eventsPerStream,
                               bool isNewData,
                               ICollection<EventBean> matchingEvents,
                               ExprEvaluatorContext exprEvaluatorContext)
        {
            // Evaluate the value expression
            Object valueLeft = _valueExpr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));

            if (matchingEvents == null)
            {
                return false;
            }
            if (matchingEvents.Count == 0)
            {
                return false;
            }

            // Evaluation event-per-stream
            var events = new EventBean[eventsPerStream.Length + 1];
            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);

            // Filter and check each row.
            bool hasNonNullRow = false;
            bool hasRows = false;
            foreach (EventBean subselectEvent in matchingEvents)
            {
                // Prepare filter expression event list
                events[0] = subselectEvent;

                // Eval filter expression
                if (_filterExpr != null)
                {
                    var pass = (bool?) _filterExpr.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext));
                    if (!pass.GetValueOrDefault(false))
                    {
                        continue;
                    }
                }
                hasRows = true;

                Object valueRight;
                if (_selectClauseExpr != null)
                {
                    valueRight = _selectClauseExpr.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext));
                }
                else
                {
                    valueRight = events[0].Underlying;
                }

                if (valueRight != null)
                {
                    hasNonNullRow = true;
                }

                if ((valueLeft != null) && (valueRight != null))
                {
                    if (_computer.Invoke(valueLeft, valueRight))
                    {
                        return true;
                    }
                }
            }

            if (!hasRows)
            {
                return false;
            }
            if ((!hasNonNullRow) || (valueLeft == null))
            {
                return null;
            }
            return false;
        }

        #endregion
    }
}