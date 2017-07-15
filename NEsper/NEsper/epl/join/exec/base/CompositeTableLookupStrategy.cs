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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.exec.composite;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.epl.join.table;
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
    public class CompositeTableLookupStrategy : JoinExecTableLookupStrategy {
        private readonly EventType eventType;
        private readonly PropertyCompositeEventTable index;
        private readonly CompositeIndexQuery chain;
        private readonly List<QueryGraphValueEntryRange> rangeKeyPairs;
        private readonly LookupStrategyDesc lookupStrategyDesc;
    
        public CompositeTableLookupStrategy(EventType eventType, int lookupStream, List<QueryGraphValueEntryHashKeyed> hashKeys, List<QueryGraphValueEntryRange> rangeKeyPairs, PropertyCompositeEventTable index) {
            this.eventType = eventType;
            this.index = index;
            this.rangeKeyPairs = rangeKeyPairs;
            chain = CompositeIndexQueryFactory.MakeJoinSingleLookupStream(false, lookupStream, hashKeys, index.OptKeyCoercedTypes, rangeKeyPairs, index.OptRangeCoercedTypes);
    
            var expressionTexts = new ArrayDeque<string>();
            foreach (QueryGraphValueEntryRange pair in rangeKeyPairs) {
                ExprNode[] expressions = pair.Expressions;
                foreach (ExprNode node in expressions) {
                    expressionTexts.Add(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(node));
                }
            }
            lookupStrategyDesc = new LookupStrategyDesc(LookupStrategyType.COMPOSITE, expressionTexts.ToArray());
        }

        /// <summary>
        /// Returns event type of the lookup event.
        /// </summary>
        /// <value>event type of the lookup event</value>
        public EventType EventType
        {
            get { return eventType; }
        }

        /// <summary>
        /// Returns index to look up in.
        /// </summary>
        /// <value>index to use</value>
        public PropertyCompositeEventTable Index
        {
            get { return index; }
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QIndexJoinLookup(this, index);
                var keys = new List<Object>(2);
                ICollection<EventBean> resultX = chain.GetCollectKeys(theEvent, index.Index, context, keys, index.PostProcessor);
                InstrumentationHelper.Get().AIndexJoinLookup(resultX, keys.Count > 1 ? keys.ToArray() : keys[0]);
                return resultX;
            }
    
            ISet<EventBean> result = chain.Get(theEvent, index.Index, context, index.PostProcessor);
            if (result != null && result.IsEmpty()) {
                return null;
            }
            return result;
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return lookupStrategyDesc; }
        }

        public override String ToString() {
            return string.Format("CompositeTableLookupStrategy indexProps={0} index=({1})", CompatExtensions.Render(rangeKeyPairs), index);
        }
    }
} // end of namespace
