///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>Index lookup strategy for subqueries.</summary>
    public class SubordIndexedTableLookupStrategyPropFactory : SubordTableLookupStrategyFactory
    {
        private readonly string[] _properties;
    
        /// <summary>Stream numbers to get key values from.</summary>
        private readonly int[] _keyStreamNums;
    
        /// <summary>Getters to use to get key values.</summary>
        private readonly EventPropertyGetter[] _propertyGetters;
    
        private readonly LookupStrategyDesc _strategyDesc;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventTypes">is the event types per stream</param>
        /// <param name="keyStreamNumbers">is the stream number per property</param>
        /// <param name="properties">is the key properties</param>
        /// <param name="isNWOnTrigger">indicator whether named window trigger</param>
        public SubordIndexedTableLookupStrategyPropFactory(bool isNWOnTrigger, EventType[] eventTypes, int[] keyStreamNumbers, string[] properties)
        {
            _keyStreamNums = keyStreamNumbers;
            _properties = properties;
            _strategyDesc = new LookupStrategyDesc(LookupStrategyType.MULTIPROP, properties);
    
            _propertyGetters = new EventPropertyGetter[properties.Length];
            for (int i = 0; i < keyStreamNumbers.Length; i++) {
                int streamNumber = keyStreamNumbers[i];
                string property = properties[i];
                EventType eventType = eventTypes[streamNumber];
                _propertyGetters[i] = eventType.GetGetter(property);
    
                if (_propertyGetters[i] == null) {
                    throw new ArgumentException("Property named '" + properties[i] + "' is invalid for type " + eventType);
                }
            }
    
            for (int i = 0; i < _keyStreamNums.Length; i++) {
                _keyStreamNums[i] += isNWOnTrigger ? 1 : 0; // for on-trigger the key will be provided in a {1,2,...} stream and not {0,...}
            }
        }
    
        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw) {
            return new SubordIndexedTableLookupStrategyProp(_keyStreamNums, _propertyGetters, (PropertyIndexedEventTable) eventTable[0], _strategyDesc);
        }

        /// <summary>
        /// Returns properties to use from lookup event to look up in index.
        /// </summary>
        /// <value>properties to use from lookup event</value>
        public string[] Properties => _properties;

        public string ToQueryPlan() {
            return GetType().Name +
                    " indexProps=" + CompatExtensions.Render(_properties) +
                    " keyStreamNums=" + CompatExtensions.Render(_keyStreamNums);
        }
    }
} // end of namespace
