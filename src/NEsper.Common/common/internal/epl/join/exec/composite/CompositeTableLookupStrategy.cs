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
using com.espertech.esper.common.@internal.epl.index.composite;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    /// <summary>
    /// Lookup on an nested map structure that represents an index for use with at least one range and possibly multiple ranges
    /// and optionally keyed by one or more unique keys.
    /// <para />Use the sorted strategy instead if supporting a single range only and no other unique keys are part of the index.
    /// </summary>
    public class CompositeTableLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly EventType _eventType;
        private readonly PropertyCompositeEventTable _index;
        private readonly CompositeIndexQuery _chain;

        public CompositeTableLookupStrategy(
            EventType eventType,
            int lookupStream,
            ExprEvaluator hashKeys,
            QueryGraphValueEntryRange[] rangeKeyPairs,
            PropertyCompositeEventTable index)
        {
            _eventType = eventType;
            _index = index;
            _chain = CompositeIndexQueryFactory.MakeJoinSingleLookupStream(false, lookupStream, hashKeys, rangeKeyPairs);
        }

        /// <summary>
        /// Returns event type of the lookup event.
        /// </summary>
        /// <returns>event type of the lookup event</returns>
        public EventType EventType {
            get => _eventType;
        }

        /// <summary>
        /// Returns index to look up in.
        /// </summary>
        /// <returns>index to use</returns>
        public PropertyCompositeEventTable Index {
            get => _index;
        }

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            Cursor cursor,
            ExprEvaluatorContext context)
        {
            InstrumentationCommon instrumentationCommon = context.InstrumentationProvider;
            if (instrumentationCommon.Activated()) {
                instrumentationCommon.QIndexJoinLookup(this, _index);
                List<object> keys = new List<object>(2);
                var resultCollectKeys = _chain.GetCollectKeys(theEvent, _index.Index, context, keys, _index.PostProcessor);
                instrumentationCommon.AIndexJoinLookup(resultCollectKeys, keys.Count > 1 ? keys.ToArray() : keys[0]);
                return resultCollectKeys;
            }

            var result = _chain.Get(theEvent, _index.Index, context, _index.PostProcessor);
            if (result != null && result.IsEmpty()) {
                return null;
            }

            return result;
        }

        public LookupStrategyType LookupStrategyType {
            get => LookupStrategyType.COMPOSITE;
        }
    }
} // end of namespace