///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.property
{
    /// <summary>
    /// A property evaluator that considers nested properties and that considers where-clauses but does not consider select-clauses.
    /// </summary>
    public class PropertyEvaluatorNested : PropertyEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly ContainedEventEval[] _containedEventEvals;
        private readonly FragmentEventType[] _fragmentEventType;
        private readonly ExprEvaluator[] _whereClauses;
        private readonly EventBean[] _eventsPerStream;
        private readonly int _lastLevel;
        private readonly IList<String> _expressionTexts;
    
        /// <summary>Ctor. </summary>
        /// <param name="containedEventEvals">property getters or other evaluators</param>
        /// <param name="fragmentEventType">the fragments</param>
        /// <param name="whereClauses">the where clauses</param>
        /// <param name="expressionTexts">the property names that are staggered</param>
        public PropertyEvaluatorNested(ContainedEventEval[] containedEventEvals, FragmentEventType[] fragmentEventType, ExprEvaluator[] whereClauses, IList<String> expressionTexts)
        {
            _fragmentEventType = fragmentEventType;
            _containedEventEvals = containedEventEvals;
            _whereClauses = whereClauses;
            _lastLevel = fragmentEventType.Length - 1;
            _eventsPerStream = new EventBean[fragmentEventType.Length + 1];
            _expressionTexts = expressionTexts;
        }
    
        public EventBean[] GetProperty(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            var resultEvents = new LinkedList<EventBean>();
            _eventsPerStream[0] = theEvent;
            PopulateEvents(theEvent, 0, resultEvents, exprEvaluatorContext);
            if (resultEvents.IsEmpty())
            {
                return null;
            }
            return resultEvents.ToArray();
        }
    
        private void PopulateEvents(EventBean branch, int level, ICollection<EventBean> events, ExprEvaluatorContext exprEvaluatorContext)
        {
            try
            {
                Object result = _containedEventEvals[level].GetFragment(branch, _eventsPerStream, exprEvaluatorContext);
    
                if (_fragmentEventType[level].IsIndexed)
                {
                    var fragments = (EventBean[]) result;
                    if (level == _lastLevel)
                    {
                        if (_whereClauses[level] != null)
                        {
                            foreach (EventBean theEvent in fragments)
                            {
                                _eventsPerStream[level+1] = theEvent;
                                if (ExprNodeUtility.ApplyFilterExpression(_whereClauses[level], _eventsPerStream, exprEvaluatorContext))
                                {
                                    events.Add(theEvent);
                                }
                            }
                        }
                        else
                        {
                            events.AddAll(fragments);
                        }
                    }
                    else
                    {
                        if (_whereClauses[level] != null)
                        {
                            foreach (EventBean next in fragments)
                            {
                                _eventsPerStream[level+1] = next;
                                if (ExprNodeUtility.ApplyFilterExpression(_whereClauses[level], _eventsPerStream, exprEvaluatorContext))
                                {
                                    PopulateEvents(next, level+1, events, exprEvaluatorContext);
                                }
                            }
                        }
                        else
                        {
                            foreach (EventBean next in fragments)
                            {
                                _eventsPerStream[level+1] = next;
                                PopulateEvents(next, level+1, events, exprEvaluatorContext);
                            }
                        }
                    }
                }
                else
                {
                    var fragment = (EventBean) result;
                    if (level == _lastLevel)
                    {
                        if (_whereClauses[level] != null)
                        {
                            _eventsPerStream[level+1] = fragment;
                            if (ExprNodeUtility.ApplyFilterExpression(_whereClauses[level], _eventsPerStream, exprEvaluatorContext))
                            {
                                events.Add(fragment);
                            }
                        }
                        else
                        {
                            events.Add(fragment);
                        }
                    }
                    else
                    {
                        if (_whereClauses[level] != null)
                        {
                            _eventsPerStream[level+1] = fragment;
                            if (ExprNodeUtility.ApplyFilterExpression(_whereClauses[level], _eventsPerStream, exprEvaluatorContext))
                            {
                                PopulateEvents(fragment, level+1, events, exprEvaluatorContext);
                            }
                        }
                        else
                        {
                            _eventsPerStream[level+1] = fragment;
                            PopulateEvents(fragment, level+1, events, exprEvaluatorContext);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error evaluating property expression for event of type '" +
                        branch.EventType.Name +
                        "' and property '" +
                        _expressionTexts[level + 1] + "': " + ex.Message, ex);
            }
        }

        public EventType FragmentEventType
        {
            get { return _fragmentEventType[_lastLevel].FragmentType; }
        }

        public bool CompareTo(PropertyEvaluator otherEval)
        {
            return false;
        }
    }
}
