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
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>Lookup on an index using a set of expression results as key values. </summary>
    public class InKeywordMultiTableLookupStrategyExpr : JoinExecTableLookupStrategy
    {
        private readonly PropertyIndexedEventTableSingle[] _indexes;
        private readonly int _streamNum;
        private readonly EventBean[] _eventsPerStream;
        private readonly ExprEvaluator _evaluator;
        private readonly LookupStrategyDesc _lookupStrategyDesc;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluator">The evaluator.</param>
        /// <param name="streamNum">The stream num.</param>
        /// <param name="indexes">The indexes.</param>
        /// <param name="lookupStrategyDesc">The lookup strategy desc.</param>
        public InKeywordMultiTableLookupStrategyExpr(ExprEvaluator evaluator, int streamNum, PropertyIndexedEventTableSingle[] indexes, LookupStrategyDesc lookupStrategyDesc)
        {
            if (indexes == null) {
                throw new ArgumentException("Unexpected null index received");
            }
            this._indexes = indexes;
            this._streamNum = streamNum;
            this._eventsPerStream = new EventBean[streamNum + 1];
            this._evaluator = evaluator;
            this._lookupStrategyDesc = lookupStrategyDesc;
        }
    
        /// <summary>Returns index to look up in. </summary>
        /// <returns>index to use</returns>
        public PropertyIndexedEventTableSingle[] GetIndex()
        {
            return _indexes;
        }
    
        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventsPerStream[_streamNum] = theEvent;
            return InKeywordTableLookupUtil.MultiIndexLookup(_evaluator, _eventsPerStream, exprEvaluatorContext, _indexes);
        }
    
        public override String ToString()
        {
            return this.GetType().Name + " " + _lookupStrategyDesc;
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return _lookupStrategyDesc; }
        }
    }
}
