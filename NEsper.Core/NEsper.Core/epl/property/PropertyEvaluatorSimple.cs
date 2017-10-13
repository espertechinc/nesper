///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.property
{
    /// <summary>
    ///     Property evaluator that considers only level one and considers a where-clause,
    ///     but does not consider a select clause or N-level.
    /// </summary>
    public class PropertyEvaluatorSimple : PropertyEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ContainedEventEval _containedEventEval;
        private readonly string _expressionText;
        private readonly ExprEvaluator _filter;
        private readonly FragmentEventType _fragmentEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="containedEventEval">property getter or other evaluator</param>
        /// <param name="fragmentEventType">property event type</param>
        /// <param name="filter">optional where-clause expression</param>
        /// <param name="expressionText">the property name</param>
        public PropertyEvaluatorSimple(
            ContainedEventEval containedEventEval,
            FragmentEventType fragmentEventType,
            ExprEvaluator filter,
            string expressionText)
        {
            _fragmentEventType = fragmentEventType;
            _containedEventEval = containedEventEval;
            _filter = filter;
            _expressionText = expressionText;
        }

        /// <summary>
        ///     Returns the property name.
        /// </summary>
        /// <value>property name</value>
        public string ExpressionText
        {
            get { return _expressionText; }
        }

        /// <summary>
        ///     Returns the filter.
        /// </summary>
        /// <value>filter</value>
        public ExprEvaluator Filter
        {
            get { return _filter; }
        }

        public EventBean[] GetProperty(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            try
            {
                Object result = _containedEventEval.GetFragment(
                    theEvent, new EventBean[] { theEvent }, exprEvaluatorContext);

                EventBean[] rows;
                if (_fragmentEventType.IsIndexed)
                {
                    rows = (EventBean[]) result;
                }
                else
                {
                    rows = new EventBean[] { (EventBean) result };
                }

                if (_filter == null)
                {
                    return rows;
                }
                return ExprNodeUtility.ApplyFilterExpression(
                    _filter, theEvent, (EventBean[]) result, exprEvaluatorContext);
            }
            catch (Exception ex)
            {
                Log.Error(
                    "Unexpected error evaluating property expression for event of type '" +
                    theEvent.EventType.Name +
                    "' and property '" +
                    _expressionText + "': " + ex.Message, ex);
            }
            return null;
        }

        public EventType FragmentEventType
        {
            get { return _fragmentEventType.FragmentType; }
        }

        public bool CompareTo(PropertyEvaluator otherEval)
        {
            if (!(otherEval is PropertyEvaluatorSimple))
            {
                return false;
            }
            var other = (PropertyEvaluatorSimple) otherEval;
            if (!other.ExpressionText.Equals(ExpressionText))
            {
                return false;
            }
            if ((other.Filter == null) && (Filter == null))
            {
                return true;
            }
            return false;
        }
    }
} // end of namespace