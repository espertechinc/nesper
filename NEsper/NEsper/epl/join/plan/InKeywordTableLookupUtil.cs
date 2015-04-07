///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.plan
{
    public class InKeywordTableLookupUtil
    {
        public static ICollection<EventBean> MultiIndexLookup(ExprEvaluator evaluator, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, EventTable[] indexes)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            var key = evaluator.Evaluate(evaluateParams);
            var first = true;
            ICollection<EventBean> result = null;
    
            foreach (var table in indexes) {
    
                ICollection<EventBean> found = ((PropertyIndexedEventTableSingle) table).Lookup(key);
                if (found != null && !found.IsEmpty()) {
                    if (result == null) {
                        result = found;
                    }
                    else if (first) {
                        var copy = new LinkedHashSet<EventBean>();
                        copy.AddAll(result);
                        copy.AddAll(found);
                        result = copy;
                        first = false;
                    }
                    else {
                        result.AddAll(found);
                    }
                }
            }
    
            return result;
        }
    
        public static ICollection<EventBean> SingleIndexLookup(ExprEvaluator[] evaluators, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, PropertyIndexedEventTableSingle index)
        {
            var first = true;
            ICollection<EventBean> result = null;
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
    
            foreach (var evaluator in evaluators) {
                var key = evaluator.Evaluate(evaluateParams);
                ICollection<EventBean> found = index.Lookup(key);
                if (found != null && !found.IsEmpty()) {
                    if (result == null) {
                        result = found;
                    }
                    else if (first) {
                        var copy = new LinkedHashSet<EventBean>();
                        copy.AddAll(result);
                        copy.AddAll(found);
                        result = copy;
                        first = false;
                    }
                    else {
                        result.AddAll(found);
                    }
                }
            }
    
            return result;
        }
    }
}
