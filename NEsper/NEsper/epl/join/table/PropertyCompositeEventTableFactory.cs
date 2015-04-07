///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.@join.exec.composite;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// For use when the index comprises of either two or more ranges or a unique key in 
    /// combination with a range. Expected at least either (A) one key and one range or
    /// (B) zero keys and 2 ranges.
    /// <para/>
    /// - not applicable for range-only lookups (since there the key can be the value itself
    /// - not applicable for multiple nested range as ordering not nested 
    /// - each add/remove and lookup would also need to construct a key object.
    /// </summary>
    public class PropertyCompositeEventTableFactory : EventTableFactory
    {
        private readonly int _streamNum;
        private readonly IList<String> _optionalKeyedProps;
        private readonly IList<String> _rangeProps;
        private readonly CompositeIndexEnterRemove _chain;
        private readonly IList<Type> _optKeyCoercedTypes;
        private readonly IList<Type> _optRangeCoercedTypes;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">the stream number that is indexed</param>
        /// <param name="eventType">types of events indexed</param>
        /// <param name="optionalKeyedProps">The optional keyed props.</param>
        /// <param name="optKeyCoercedTypes">The opt key coerced types.</param>
        /// <param name="rangeProps">The range props.</param>
        /// <param name="optRangeCoercedTypes">property types</param>
        public PropertyCompositeEventTableFactory(int streamNum, EventType eventType, IList<String> optionalKeyedProps, IList<Type> optKeyCoercedTypes, IList<String> rangeProps, IList<Type> optRangeCoercedTypes)
        {
            _streamNum = streamNum;
            _rangeProps = rangeProps;
            _optionalKeyedProps = optionalKeyedProps;
            _optKeyCoercedTypes = optKeyCoercedTypes;
            _optRangeCoercedTypes = optRangeCoercedTypes;
    
            // construct chain
            var enterRemoves = new List<CompositeIndexEnterRemove>();
            if (optionalKeyedProps != null && optionalKeyedProps.Count > 0) {
                enterRemoves.Add(new CompositeIndexEnterRemoveKeyed(eventType, optionalKeyedProps, optKeyCoercedTypes));
            }
            int count = 0;
            foreach (String rangeProp in rangeProps) {
                var coercionType = optRangeCoercedTypes == null ? null : optRangeCoercedTypes[count];
                enterRemoves.Add(new CompositeIndexEnterRemoveRange(eventType, rangeProp, coercionType));
                count++;
            }
    
            // Hook up as chain for remove
            CompositeIndexEnterRemove last = null;
            foreach (CompositeIndexEnterRemove action in enterRemoves)
            {
                if (last != null) {
                    last.SetNext(action);
                }
                last = action;
            }
            _chain = enterRemoves[0];
        }
    
        public EventTable[] MakeEventTables()
        {
            var organization = new EventTableOrganization(null, false, _optKeyCoercedTypes != null || _optRangeCoercedTypes != null, _streamNum, CombinedPropertyLists(_optionalKeyedProps, _rangeProps), EventTableOrganization.EventTableOrganizationType.COMPOSITE);
            return new EventTable[]
            {
                new PropertyCompositeEventTable(
                    (_optionalKeyedProps != null && _optionalKeyedProps.Count > 0), _chain, _optKeyCoercedTypes,
                    _optRangeCoercedTypes, organization)
            };
        }

        public Type EventTableType
        {
            get { return typeof (PropertyCompositeEventTable); }
        }

        public String ToQueryPlan()
        {
            return GetType().FullName +
                    " streamNum=" + _streamNum +
                    " keys=" + _optionalKeyedProps.Render() +
                    " ranges=" + _rangeProps.Render();
        }
    
        private IList<String> CombinedPropertyLists(IList<string> optionalKeyedProps, IList<string> rangeProps)
        {
            if (optionalKeyedProps == null)
            {
                return rangeProps;
            }
            if (rangeProps == null)
            {
                return optionalKeyedProps;
            }

            return Enumerable.Concat(optionalKeyedProps, rangeProps).ToList();
            //return (IList<String>) CollectionUtil.AddArrays(optionalKeyedProps, rangeProps);
        }
    }
}
