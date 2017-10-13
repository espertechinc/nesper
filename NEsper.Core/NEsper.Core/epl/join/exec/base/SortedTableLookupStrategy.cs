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
using com.espertech.esper.epl.@join.exec.sorted;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.rep;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Lookup on an index that is a sorted index on a single property queried as a range.
    /// <para/>
    /// Use the composite strategy if supporting multiple ranges or if range is in combination with unique key.
    /// </summary>
    public class SortedTableLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly QueryGraphValueEntryRange _rangeKeyPair;
        private readonly PropertySortedEventTable _index;
        private readonly SortedAccessStrategy _strategy;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lookupStream">The lookup stream.</param>
        /// <param name="numStreams">The num streams.</param>
        /// <param name="rangeKeyPair">The range key pair.</param>
        /// <param name="coercionType">Type of the coercion.</param>
        /// <param name="index">index to look up in</param>
        public SortedTableLookupStrategy(int lookupStream, int numStreams, QueryGraphValueEntryRange rangeKeyPair, Type coercionType, PropertySortedEventTable index)
        {
            _rangeKeyPair = rangeKeyPair;
            _index = index;
            _strategy = SortedAccessStrategyFactory.Make(false, lookupStream, numStreams, rangeKeyPair, coercionType);
        }

        /// <summary>Returns index to look up in. </summary>
        /// <value>index to use</value>
        public PropertySortedEventTable Index
        {
            get { return _index; }
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QIndexJoinLookup(this, _index);
                var keys = new List<Object>(2);
                var result = _strategy.LookupCollectKeys(theEvent, _index, exprEvaluatorContext, keys);
                InstrumentationHelper.Get().AIndexJoinLookup(result, keys.Count > 1 ? keys.ToArray() : keys[0]);
                return result;
            }

            return _strategy.Lookup(theEvent, _index, exprEvaluatorContext);
        }

        public override String ToString()
        {
            return "SortedTableLookupStrategy indexProps=" + _rangeKeyPair +
                    " index=(" + _index + ')';
        }

        public LookupStrategyDesc StrategyDesc
        {
            get
            {
                return new LookupStrategyDesc(
                    LookupStrategyType.RANGE, ExprNodeUtility.ToExpressionStringsMinPrecedence(_rangeKeyPair.Expressions));
            }
        }
    }
}
