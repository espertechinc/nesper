///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.rep;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Lookup on an index using a set of expression results as key values.
    /// </summary>
    public class InKeywordSingleTableLookupStrategyExpr : JoinExecTableLookupStrategy
    {
        private readonly PropertyIndexedEventTableSingle _index;
        private readonly int _streamNum;
        private readonly EventBean[] _eventsPerStream;
        private readonly ExprEvaluator[] _evaluators;
        private readonly LookupStrategyDesc _lookupStrategyDesc;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">The evaluators.</param>
        /// <param name="streamNum">The stream num.</param>
        /// <param name="index">index to look up in</param>
        /// <param name="lookupStrategyDesc">The lookup strategy desc.</param>
        public InKeywordSingleTableLookupStrategyExpr(ExprEvaluator[] evaluators, int streamNum, PropertyIndexedEventTableSingle index, LookupStrategyDesc lookupStrategyDesc)
        {
            if (index == null)
            {
                throw new ArgumentException("Unexpected null index received");
            }
            _index = index;
            _streamNum = streamNum;
            _eventsPerStream = new EventBean[streamNum + 1];
            _evaluators = evaluators;
            _lookupStrategyDesc = lookupStrategyDesc;
        }

        /// <summary>Returns index to look up in. </summary>
        /// <value>index to use</value>
        public PropertyIndexedEventTableSingle Index
        {
            get { return _index; }
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexJoinLookup(this, _index); }

            _eventsPerStream[_streamNum] = theEvent;
            var result = InKeywordTableLookupUtil.SingleIndexLookup(
                _evaluators, _eventsPerStream, exprEvaluatorContext, _index);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexJoinLookup(result, null); }
            return result;
        }

        public override String ToString()
        {
            return "IndexedTableLookupStrategyExpr expressions" +
                    " index=(" + _index + ')';
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return _lookupStrategyDesc; }
        }
    }
}
