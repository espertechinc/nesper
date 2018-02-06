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
        protected readonly int StreamNum;
        protected readonly IList<String> OptionalKeyedProps;
        protected readonly IList<String> RangeProps;
        protected readonly CompositeIndexEnterRemove Chain;
        protected readonly IList<Type> OptKeyCoercedTypes;
        protected readonly IList<Type> OptRangeCoercedTypes;

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
            StreamNum = streamNum;
            RangeProps = rangeProps;
            OptionalKeyedProps = optionalKeyedProps;
            OptKeyCoercedTypes = optKeyCoercedTypes;
            OptRangeCoercedTypes = optRangeCoercedTypes;

            // construct chain
            var enterRemoves = new List<CompositeIndexEnterRemove>();
            if (optionalKeyedProps != null && optionalKeyedProps.Count > 0)
            {
                enterRemoves.Add(new CompositeIndexEnterRemoveKeyed(eventType, optionalKeyedProps, optKeyCoercedTypes));
            }
            int count = 0;
            foreach (String rangeProp in rangeProps)
            {
                var coercionType = optRangeCoercedTypes == null ? null : optRangeCoercedTypes[count];
                enterRemoves.Add(new CompositeIndexEnterRemoveRange(eventType, rangeProp, coercionType));
                count++;
            }

            // Hook up as chain for remove
            CompositeIndexEnterRemove last = null;
            foreach (CompositeIndexEnterRemove action in enterRemoves)
            {
                if (last != null)
                {
                    last.SetNext(action);
                }
                last = action;
            }
            Chain = enterRemoves[0];
        }

        public EventTable[] MakeEventTables(EventTableFactoryTableIdent tableIdent, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventTableOrganization organization = Organization;
            return new EventTable[] { new PropertyCompositeEventTableImpl(
                OptKeyCoercedTypes, OptRangeCoercedTypes, organization, (OptionalKeyedProps != null && OptionalKeyedProps.Count > 0), Chain) };
        }

        protected EventTableOrganization Organization
        {
            get
            {
                return new EventTableOrganization(
                    null, false, OptKeyCoercedTypes != null || OptRangeCoercedTypes != null, StreamNum,
                    CombinedPropertyLists(OptionalKeyedProps, RangeProps), EventTableOrganizationType.COMPOSITE);
            }
        }

        public Type EventTableType
        {
            get { return typeof(PropertyCompositeEventTable); }
        }

        public String ToQueryPlan()
        {
            return GetType().FullName +
                    " streamNum=" + StreamNum +
                    " keys=" + OptionalKeyedProps.Render() +
                    " ranges=" + RangeProps.Render();
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
