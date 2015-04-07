///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.property
{
    /// <summary>
    /// Property evaluator that considers only level one and considers a where-clause, but does not consider a select clause or N-level.
    /// </summary>
    public class PropertyEvaluatorSimple : PropertyEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ContainedEventEval _containedEventEval;
        private readonly FragmentEventType _fragmentEventType;
        private readonly ExprEvaluator _filter;
        private readonly String _expressionText;
    
        /// <summary>Ctor. </summary>
        /// <param name="containedEventEval">property getter or other evaluator</param>
        /// <param name="fragmentEventType">property event type</param>
        /// <param name="filter">optional where-clause expression</param>
        /// <param name="expressionText">the property name</param>
        public PropertyEvaluatorSimple(ContainedEventEval containedEventEval, FragmentEventType fragmentEventType, ExprEvaluator filter, String expressionText)
        {
            _fragmentEventType = fragmentEventType;
            _containedEventEval = containedEventEval;
            _filter = filter;
            _expressionText = expressionText;
        }
    
        public EventBean[] GetProperty(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            try
            {
                Object result = _containedEventEval.GetFragment(theEvent, new[] {theEvent}, exprEvaluatorContext);
    
                EventBean[] rows;
                if (_fragmentEventType.IsIndexed)
                {
                    rows = (EventBean[]) result;
                }
                else
                {
                    rows = new[] {(EventBean) result};
                }
    
                if (_filter == null)
                {
                    return rows;
                }
                return ExprNodeUtility.ApplyFilterExpression(_filter, theEvent, (EventBean[]) result, exprEvaluatorContext);
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error evaluating property expression for event of type '" +
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

        /// <summary>Returns the property name. </summary>
        /// <value>property name</value>
        public string ExpressionText
        {
            get { return _expressionText; }
        }

        /// <summary>Returns the filter. </summary>
        /// <value>filter</value>
        public ExprEvaluator Filter
        {
            get { return _filter; }
        }


        public bool Equals(PropertyEvaluatorSimple other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._expressionText, _expressionText) && Equals(other._filter, _filter);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PropertyEvaluatorSimple)) return false;
            return Equals((PropertyEvaluatorSimple) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_expressionText != null ? _expressionText.GetHashCode() : 0)*397) ^ (_filter != null ? _filter.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Compares the object to another evaluator.
        /// </summary>
        /// <param name="otherEval">The other eval.</param>
        /// <returns></returns>
        public bool CompareTo(PropertyEvaluator otherEval)
        {
            return Equals(otherEval);
        }
    }
}
