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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Index lookup strategy for subqueries.
    /// </summary>
    public class SubordIndexedTableLookupStrategyProp : SubordTableLookupStrategy
    {
        /// <summary>Stream numbers to get key values from. </summary>
        private readonly int[] _keyStreamNums;
    
        /// <summary>Getters to use to get key values. </summary>
        private readonly EventPropertyGetter[] _propertyGetters;
    
        /// <summary>MapIndex to look up in. </summary>
        private readonly PropertyIndexedEventTable _index;

        private readonly LookupStrategyDesc _strategyDesc;
    
        public SubordIndexedTableLookupStrategyProp(int[] keyStreamNums, EventPropertyGetter[] propertyGetters, PropertyIndexedEventTable index, LookupStrategyDesc strategyDesc)
        {
            _keyStreamNums = keyStreamNums;
            _propertyGetters = propertyGetters;
            _index = index;
            _strategyDesc = strategyDesc;
        }

        /// <summary>
        /// Returns index to look up in.
        /// </summary>
        /// <value>index to use</value>
        public PropertyIndexedEventTable Index => _index;

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexSubordLookup(this, _index, _keyStreamNums);}
    
            Object[] keys = GetKeys(eventsPerStream);
    
            if (InstrumentationHelper.ENABLED) {
                ISet<EventBean> result = _index.Lookup(keys);
                InstrumentationHelper.Get().AIndexSubordLookup(result, keys);
                return result;
            }
            return _index.Lookup(keys);
        }
    
        /// <summary>Get the index lookup keys. </summary>
        /// <param name="eventsPerStream">is the events for each stream</param>
        /// <returns>key object</returns>
        protected Object[] GetKeys(EventBean[] eventsPerStream)
        {
            Object[] keyValues = new Object[_propertyGetters.Length];
            for (int i = 0; i < _propertyGetters.Length; i++)
            {
                int streamNum = _keyStreamNums[i];
                EventBean theEvent = eventsPerStream[streamNum];
                keyValues[i] = _propertyGetters[i].Get(theEvent);
            }
            return keyValues;
        }
    
        public override String ToString()
        {
            return ToQueryPlan();
        }

        public LookupStrategyDesc StrategyDesc => _strategyDesc;

        public String ToQueryPlan() {
            return GetType().FullName + " keyStreamNums=" + _keyStreamNums.Render();
        }
    }
}
