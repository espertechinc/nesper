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
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.metrics.instrumentation;

namespace com.espertech.esper.common.@internal.epl.join.exec.inkeyword
{
    /// <summary>
    /// Lookup on an index using a set of expression results as key values.
    /// </summary>
    public class InKeywordMultiTableLookupStrategyExpr : JoinExecTableLookupStrategy
    {
        private readonly InKeywordTableLookupPlanMultiIdxFactory _factory;
        private readonly PropertyHashedEventTable[] _indexes;
        private readonly EventBean[] _eventsPerStream;

        public InKeywordMultiTableLookupStrategyExpr(
            InKeywordTableLookupPlanMultiIdxFactory factory,
            PropertyHashedEventTable[] indexes)
        {
            _factory = factory;
            _indexes = indexes;
            _eventsPerStream = new EventBean[factory.LookupStream + 1];
        }

        public PropertyHashedEventTable[] Index => _indexes;

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            Cursor cursor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QIndexJoinLookup(this, _indexes[0]);

            _eventsPerStream[_factory.LookupStream] = theEvent;
            var result = InKeywordTableLookupUtil.MultiIndexLookup(
                _factory.KeyExpr,
                _eventsPerStream,
                exprEvaluatorContext,
                _indexes);

            instrumentationCommon.AIndexJoinLookup(result, null);

            return result;
        }

        public LookupStrategyType LookupStrategyType => LookupStrategyType.INKEYWORDMULTIIDX;
    }
} // end of namespace