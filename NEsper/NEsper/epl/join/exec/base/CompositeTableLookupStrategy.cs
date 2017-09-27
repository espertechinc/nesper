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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.exec.composite;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.rep;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Lookup on an nested map structure that represents an index for use with at least one range and possibly multiple ranges
    /// and optionally keyed by one or more unique keys.
    /// <para>
    /// Use the sorted strategy instead if supporting a single range only and no other unique keys are part of the index.
    /// </para>
    /// </summary>
    public class CompositeTableLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly EventType _eventType;
        private readonly PropertyCompositeEventTable _index;
        private readonly CompositeIndexQuery _chain;
        private readonly IList<QueryGraphValueEntryRange> _rangeKeyPairs;
        private readonly LookupStrategyDesc _lookupStrategyDesc;
    
        public CompositeTableLookupStrategy(EventType eventType, int lookupStream, IList<QueryGraphValueEntryHashKeyed> hashKeys, IList<QueryGraphValueEntryRange> rangeKeyPairs, PropertyCompositeEventTable index) {
            _eventType = eventType;
            _index = index;
            _rangeKeyPairs = rangeKeyPairs;
            _chain = CompositeIndexQueryFactory.MakeJoinSingleLookupStream(false, lookupStream, hashKeys, index.OptKeyCoercedTypes, rangeKeyPairs, index.OptRangeCoercedTypes);
    
            var expressionTexts = new ArrayDeque<string>();
            foreach (QueryGraphValueEntryRange pair in rangeKeyPairs) {
                ExprNode[] expressions = pair.Expressions;
                foreach (ExprNode node in expressions) {
                    expressionTexts.Add(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(node));
                }
            }
            _lookupStrategyDesc = new LookupStrategyDesc(LookupStrategyType.COMPOSITE, expressionTexts.ToArray());
        }

        /// <summary>
        /// Returns event type of the lookup event.
        /// </summary>
        /// <value>event type of the lookup event</value>
        public EventType EventType
        {
            get { return _eventType; }
        }

        /// <summary>
        /// Returns index to look up in.
        /// </summary>
        /// <value>index to use</value>
        public PropertyCompositeEventTable Index
        {
            get { return _index; }
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QIndexJoinLookup(this, _index);
                var keys = new List<Object>(2);
                ICollection<EventBean> resultX = _chain.GetCollectKeys(theEvent, _index.MapIndex, context, keys, _index.PostProcessor);
                InstrumentationHelper.Get().AIndexJoinLookup(resultX, keys.Count > 1 ? keys.ToArray() : keys[0]);
                return resultX;
            }

            ICollection<EventBean> result = _chain.Get(theEvent, _index.MapIndex, context, _index.PostProcessor);
            if (result != null && result.IsEmpty()) {
                return null;
            }
            return result;
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return _lookupStrategyDesc; }
        }

        public override String ToString() {
            return string.Format("CompositeTableLookupStrategy indexProps={0} index=({1})", CompatExtensions.Render(_rangeKeyPairs), _index);
        }
    }
} // end of namespace
