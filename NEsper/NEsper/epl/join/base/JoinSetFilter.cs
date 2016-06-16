///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Processes join tuple set by filtering out tuples.
    /// </summary>
    public class JoinSetFilter : JoinSetProcessor
    {
        private readonly ExprEvaluator _filterExprNode;
    
        /// <summary>Ctor. </summary>
        /// <param name="filterExprNode">filter tree</param>
        public JoinSetFilter(ExprEvaluator filterExprNode)
        {
            _filterExprNode = filterExprNode;
        }
    
        public void Process(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            // Filter
            if (_filterExprNode != null)
            {
                if (InstrumentationHelper.ENABLED) {
                    if ((newEvents != null && newEvents.Count > 0) || (oldEvents != null && oldEvents.Count > 0)) {
                        InstrumentationHelper.Get().QJoinExecFilter();
                        Filter(_filterExprNode, newEvents, true, exprEvaluatorContext);
                        if (oldEvents != null) {
                            Filter(_filterExprNode, oldEvents, false, exprEvaluatorContext);
                        }
                        InstrumentationHelper.Get().AJoinExecFilter(newEvents, oldEvents);
                    }
                    return;
                }
    
                Filter(_filterExprNode, newEvents, true, exprEvaluatorContext);
                if (oldEvents != null) {
                    Filter(_filterExprNode, oldEvents, false, exprEvaluatorContext);
                }
            }
        }

        /// <summary>
        /// Filter event by applying the filter nodes evaluation method.
        /// </summary>
        /// <param name="filterExprNode">top node of the filter expression tree.</param>
        /// <param name="events">set of tuples of events</param>
        /// <param name="isNewData">true to indicate filter new data (istream) and not old data (rstream)</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        public static void Filter(ExprEvaluator filterExprNode, ICollection<MultiKey<EventBean>> events, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var keyList = new List<MultiKey<EventBean>>();

            foreach (MultiKey<EventBean> key in events)
            {
                var eventArr = key.Array;
                var evaluateParams = new EvaluateParams(eventArr, isNewData, exprEvaluatorContext);
                var matched = filterExprNode.Evaluate(evaluateParams);
                if ((matched == null) || (false.Equals(matched)))
                {
                    keyList.Add(key);
                }
            }

            keyList.ForEach(ev => events.Remove(ev));
        }
    }
}
