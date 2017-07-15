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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>Index lookup strategy for subqueries.</summary>
    public class SubordIndexedTableLookupStrategyPropFactory : SubordTableLookupStrategyFactory {
        private readonly string[] properties;
    
        /// <summary>Stream numbers to get key values from.</summary>
        private readonly int[] keyStreamNums;
    
        /// <summary>Getters to use to get key values.</summary>
        private readonly EventPropertyGetter[] propertyGetters;
    
        private readonly LookupStrategyDesc strategyDesc;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventTypes">is the event types per stream</param>
        /// <param name="keyStreamNumbers">is the stream number per property</param>
        /// <param name="properties">is the key properties</param>
        /// <param name="isNWOnTrigger">indicator whether named window trigger</param>
        public SubordIndexedTableLookupStrategyPropFactory(bool isNWOnTrigger, EventType[] eventTypes, int[] keyStreamNumbers, string[] properties) {
            this.keyStreamNums = keyStreamNumbers;
            this.properties = properties;
            this.strategyDesc = new LookupStrategyDesc(LookupStrategyType.MULTIPROP, properties);
    
            propertyGetters = new EventPropertyGetter[properties.Length];
            for (int i = 0; i < keyStreamNumbers.Length; i++) {
                int streamNumber = keyStreamNumbers[i];
                string property = properties[i];
                EventType eventType = eventTypes[streamNumber];
                propertyGetters[i] = eventType.GetGetter(property);
    
                if (propertyGetters[i] == null) {
                    throw new ArgumentException("Property named '" + properties[i] + "' is invalid for type " + eventType);
                }
            }
    
            for (int i = 0; i < keyStreamNums.Length; i++) {
                keyStreamNums[i] += isNWOnTrigger ? 1 : 0; // for on-trigger the key will be provided in a {1,2,...} stream and not {0,...}
            }
        }
    
        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw) {
            return new SubordIndexedTableLookupStrategyProp(keyStreamNums, propertyGetters, (PropertyIndexedEventTable) eventTable[0], strategyDesc);
        }
    
        /// <summary>
        /// Returns properties to use from lookup event to look up in index.
        /// </summary>
        /// <returns>properties to use from lookup event</returns>
        public string[] GetProperties() {
            return properties;
        }
    
        public string ToQueryPlan() {
            return this.GetType().Name +
                    " indexProps=" + Arrays.ToString(properties) +
                    " keyStreamNums=" + Arrays.ToString(keyStreamNums);
        }
    }
} // end of namespace
