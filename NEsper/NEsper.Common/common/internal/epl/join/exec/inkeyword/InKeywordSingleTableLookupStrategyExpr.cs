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
    public class InKeywordSingleTableLookupStrategyExpr : JoinExecTableLookupStrategy
    {
        private readonly InKeywordTableLookupPlanSingleIdxFactory _factory;
        private readonly PropertyHashedEventTable _index;
        private readonly EventBean[] _eventsPerStream;

        public InKeywordSingleTableLookupStrategyExpr(
            InKeywordTableLookupPlanSingleIdxFactory factory,
            PropertyHashedEventTable index)
        {
            this._factory = factory;
            this._index = index;
            this._eventsPerStream = new EventBean[factory.LookupStream + 1];
        }

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            Cursor cursor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            InstrumentationCommon instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QIndexJoinLookup(this, _index);

            _eventsPerStream[_factory.LookupStream] = theEvent;
            ISet<EventBean> result = InKeywordTableLookupUtil.SingleIndexLookup(
                _factory.Expressions,
                _eventsPerStream,
                exprEvaluatorContext,
                _index);

            instrumentationCommon.AIndexJoinLookup(result, null);
            return result;
        }

        public override string ToString()
        {
            return "IndexedTableLookupStrategyExpr expressions" +
                   " index=(" +
                   _index +
                   ')';
        }

        public LookupStrategyType LookupStrategyType {
            get => LookupStrategyType.INKEYWORDSINGLEIDX;
        }
    }
} // end of namespace