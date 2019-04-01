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
using com.espertech.esper.epl.@join.rep;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.exec.@base
{
    public class IndexedTableLookupStrategySingleExpr : JoinExecTableLookupStrategy
    {
        private readonly PropertyIndexedEventTableSingle _index;
        private readonly ExprEvaluator _exprEvaluator;
        private readonly int _streamNum;
        private readonly EventBean[] _eventsPerStream;
        private readonly LookupStrategyDesc _strategyDesc;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="exprNode">The expr node.</param>
        /// <param name="streamNum">The stream num.</param>
        /// <param name="index">index to look up in</param>
        /// <param name="strategyDesc">The strategy desc.</param>
        public IndexedTableLookupStrategySingleExpr(ExprNode exprNode, int streamNum, PropertyIndexedEventTableSingle index, LookupStrategyDesc strategyDesc)
        {
            if (index == null)
            {
                throw new ArgumentException("Unexpected null index received");
            }
            _index = index;
            _streamNum = streamNum;
            _strategyDesc = strategyDesc;
            _eventsPerStream = new EventBean[streamNum + 1];
            _exprEvaluator = exprNode.ExprEvaluator;
        }

        /// <summary>Returns index to look up in. </summary>
        /// <returns>index to use</returns>
        public PropertyIndexedEventTableSingle GetIndex()
        {
            return _index;
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexJoinLookup(this, _index); }

            _eventsPerStream[_streamNum] = theEvent;
            var key = _exprEvaluator.Evaluate(new EvaluateParams(_eventsPerStream, true, exprEvaluatorContext));
            var result = _index.Lookup(key);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexJoinLookup(result, key); }

            return result;
        }

        public override String ToString()
        {
            return "IndexedTableLookupStrategySingleExpr evaluation" +
                    " index=(" + _index + ')';
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return _strategyDesc; }
        }
    }
}
