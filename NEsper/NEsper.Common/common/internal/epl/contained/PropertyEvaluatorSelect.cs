///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.contained
{
    /// <summary>
    ///     Property evaluator that considers a select-clauses and relies
    ///     on an accumulative property evaluator that presents events for all columns and rows.
    /// </summary>
    public class PropertyEvaluatorSelect : PropertyEvaluator
    {
        private PropertyEvaluatorAccumulative accumulative;
        private SelectExprProcessor selectExprProcessor;

        public EventType ResultEventType {
            set => FragmentEventType = value;
        }

        public SelectExprProcessor SelectExprProcessor {
            set => selectExprProcessor = value;
        }

        public PropertyEvaluatorAccumulative Accumulative {
            set => accumulative = value;
        }

        public EventBean[] GetProperty(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            var rows = accumulative.GetAccumulative(theEvent, exprEvaluatorContext);
            if (rows == null || rows.IsEmpty()) {
                return null;
            }

            var result = new ArrayDeque<EventBean>();
            foreach (var row in rows) {
                var bean = selectExprProcessor.Process(row, true, false, exprEvaluatorContext);
                result.Add(bean);
            }

            return result.ToArray();
        }

        public EventType FragmentEventType { get; private set; }

        public bool CompareTo(PropertyEvaluator otherFilterPropertyEval)
        {
            return false;
        }
    }
} // end of namespace