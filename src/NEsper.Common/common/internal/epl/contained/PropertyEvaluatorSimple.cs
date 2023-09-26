///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.contained
{
    /// <summary>
    ///     Property evaluator that considers only level one and considers a where-clause,
    ///     but does not consider a select clause or N-level.
    /// </summary>
    public class PropertyEvaluatorSimple : PropertyEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ContainedEventEval containedEventEval;
        private bool fragmentIsIndexed;

        public ExprEvaluator Filter { get; set; }

        public string ExpressionText { get; set; }

        public ContainedEventEval ContainedEventEval {
            set => containedEventEval = value;
        }

        public bool FragmentIsIndexed {
            set => fragmentIsIndexed = value;
        }

        public EventType EventType {
            set => FragmentEventType = value;
        }

        public EventBean[] GetProperty(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            try {
                var result = containedEventEval.GetFragment(theEvent, new[] { theEvent }, exprEvaluatorContext);

                EventBean[] rows;
                if (fragmentIsIndexed) {
                    rows = (EventBean[])result;
                }
                else {
                    rows = new[] { (EventBean)result };
                }

                if (Filter == null) {
                    return rows;
                }

                return ExprNodeUtilityEvaluate.ApplyFilterExpression(
                    Filter,
                    theEvent,
                    (EventBean[])result,
                    exprEvaluatorContext);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                Log.Error(
                    "Unexpected error evaluating property expression for event of type '" +
                    theEvent.EventType.Name +
                    "' and property '" +
                    ExpressionText +
                    "': " +
                    ex.Message,
                    ex);
            }

            return null;
        }

        public EventType FragmentEventType { get; private set; }

        public bool CompareTo(PropertyEvaluator otherEval)
        {
            if (!(otherEval is PropertyEvaluatorSimple other)) {
                return false;
            }

            if (!other.ExpressionText.Equals(ExpressionText)) {
                return false;
            }

            if (other.Filter == null && Filter == null) {
                return true;
            }

            return false;
        }
    }
} // end of namespace