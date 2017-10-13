///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.property
{
    /// <summary>
    /// Property evaluator that considers a select-clauses and relies on an accumulative 
    /// property evaluator that presents events for all columns and rows.
    /// </summary>
    public class PropertyEvaluatorSelect : PropertyEvaluator
    {
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly PropertyEvaluatorAccumulative _accumulative;
    
        /// <summary>Ctor. </summary>
        /// <param name="selectExprProcessor">evaluates the select clause</param>
        /// <param name="accumulative">provides property events for input events</param>
        public PropertyEvaluatorSelect(SelectExprProcessor selectExprProcessor, PropertyEvaluatorAccumulative accumulative)
        {
            _selectExprProcessor = selectExprProcessor;
            _accumulative = accumulative;
        }
    
        public EventBean[] GetProperty(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            var rows = _accumulative.GetAccumulative(theEvent, exprEvaluatorContext);
            if ((rows == null) || (rows.IsEmpty()))
            {
                return null;
            }
            var result = new LinkedList<EventBean>();
            foreach (EventBean[] row in rows)
            {
                EventBean bean = _selectExprProcessor.Process(row, true, false, exprEvaluatorContext);
                result.AddLast(bean);
            }
            return result.ToArray();
        }

        public EventType FragmentEventType
        {
            get { return _selectExprProcessor.ResultEventType; }
        }

        public bool CompareTo(PropertyEvaluator otherFilterPropertyEval)
        {
            return false;
        }
    }
}
