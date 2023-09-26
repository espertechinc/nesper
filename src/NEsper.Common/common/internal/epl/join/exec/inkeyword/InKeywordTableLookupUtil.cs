///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.inkeyword
{
    public class InKeywordTableLookupUtil
    {
        public static ISet<EventBean> MultiIndexLookup(
            ExprEvaluator evaluator,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            EventTable[] indexes)
        {
            var key = evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
            var first = true;
            ISet<EventBean> result = null;

            foreach (var table in indexes) {
                var found = ((PropertyHashedEventTable)table).Lookup(key);
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

        public static ISet<EventBean> SingleIndexLookup(
            ExprEvaluator[] evaluators,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            PropertyHashedEventTable index)
        {
            var first = true;
            ISet<EventBean> result = null;

            foreach (var evaluator in evaluators) {
                var key = evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                var found = index.Lookup(key);
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
} // end of namespace