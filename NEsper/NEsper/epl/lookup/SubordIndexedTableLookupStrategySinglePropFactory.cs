///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Index lookup strategy for subqueries.
    /// </summary>
    public class SubordIndexedTableLookupStrategySinglePropFactory : SubordTableLookupStrategyFactory
    {
        private readonly String _property;
    
        /// <summary>Stream numbers to get key values from. </summary>
        private readonly int _keyStreamNum;
    
        /// <summary>Getters to use to get key values. </summary>
        private readonly EventPropertyGetter _propertyGetter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isNWOnTrigger">if set to <c>true</c> [is NW on trigger].</param>
        /// <param name="eventTypes">is the event types per stream</param>
        /// <param name="keyStreamNum">is the stream number per property</param>
        /// <param name="property">is the key properties</param>
        public SubordIndexedTableLookupStrategySinglePropFactory(bool isNWOnTrigger, EventType[] eventTypes, int keyStreamNum, String property)
        {
            _keyStreamNum = keyStreamNum + (isNWOnTrigger ? 1 : 0); // for on-trigger the key will be provided in a {1,2,...} stream and not {0,...}
            _property = property;
            _propertyGetter = EventBeanUtility.GetAssertPropertyGetter(eventTypes, keyStreamNum, property);
        }
    
        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            if (eventTable[0] is PropertyIndexedEventTableSingleUnique) {
                return new SubordIndexedTableLookupStrategySinglePropUnique(_keyStreamNum, _propertyGetter, (PropertyIndexedEventTableSingleUnique) eventTable[0],
                        new LookupStrategyDesc(LookupStrategyType.SINGLEPROPUNIQUE, new String[] {_property}));
            }
            LookupStrategyDesc desc = new LookupStrategyDesc(LookupStrategyType.SINGLEPROPNONUNIQUE, new String[] {_property});
            return new SubordIndexedTableLookupStrategySingleProp(_keyStreamNum, _propertyGetter, (PropertyIndexedEventTableSingle) eventTable[0], desc);
        }
    
        public override String ToString()
        {
            return ToQueryPlan();
        }
    
        public String ToQueryPlan() {
            return GetType().Name + " property=" + _property + " stream=" + _keyStreamNum;
        }
    }
}
