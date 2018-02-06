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
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Index lookup strategy for subqueries.
    /// </summary>
    public class SubordIndexedTableLookupStrategySinglePropUnique : SubordTableLookupStrategy
    {
        /// <summary>Stream numbers to get key values from. </summary>
        private readonly int _keyStreamNum;
    
        /// <summary>Getters to use to get key values. </summary>
        private readonly EventPropertyGetter _propertyGetter;
    
        /// <summary>MapIndex to look up in. </summary>
        private readonly PropertyIndexedEventTableSingleUnique _index;
    
        private readonly LookupStrategyDesc _strategyDesc;
    
        public SubordIndexedTableLookupStrategySinglePropUnique(int keyStreamNum, EventPropertyGetter propertyGetter, PropertyIndexedEventTableSingleUnique index, LookupStrategyDesc strategyDesc)
        {
            _keyStreamNum = keyStreamNum;
            _propertyGetter = propertyGetter;
            _index = index;
            _strategyDesc = strategyDesc;
        }

        /// <summary>Returns index to look up in. </summary>
        /// <value>index to use</value>
        public PropertyIndexedEventTableSingleUnique Index => _index;

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexSubordLookup(this, _index, new int[]{_keyStreamNum});}
    
            Object key = GetKey(eventsPerStream);
    
            if (InstrumentationHelper.ENABLED) {
                ISet<EventBean> result = _index.Lookup(key);
                InstrumentationHelper.Get().AIndexSubordLookup(result, key);
                return result;
            }
            return _index.Lookup(key);
        }

        public LookupStrategyDesc StrategyDesc => _strategyDesc;

        /// <summary>Get the index lookup keys. </summary>
        /// <param name="eventsPerStream">is the events for each stream</param>
        /// <returns>key object</returns>
        protected Object GetKey(EventBean[] eventsPerStream)
        {
            EventBean theEvent = eventsPerStream[_keyStreamNum];
            return _propertyGetter.Get(theEvent);
        }
    
        public override String ToString()
        {
            return ToQueryPlan();
        }
    
        public String ToQueryPlan() {
            return GetType().FullName + " stream=" + _keyStreamNum;
        }
    }
}
