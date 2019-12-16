///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.composite;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.composite
{
    /// <summary>
    ///     For use when the index comprises of either two or more ranges or a unique key in combination with a range.
    ///     Organizes into a TreeMap&lt;key, TreeMap&lt;key2, Set&lt;EventBean&gt;&gt;, for short. The top level can also be
    ///     just Map&lt;HashableMultiKey, TreeMap...&gt;.
    ///     Expected at least either (A) one key and one range or (B) zero keys and 2 ranges.
    ///     <para />
    ///     An alternative implementatation could have been based on "TreeMap&lt;ComparableMultiKey, Set&lt;EventBean&gt;&gt;
    ///     &gt;", however the following implication arrive
    ///     - not applicable for range-only lookups (since there the key can be the value itself
    ///     - not applicable for multiple nested range as ordering not nested
    ///     - each add/remove and lookup would also need to construct a key object.
    /// </summary>
    public class PropertyCompositeEventTableFactory : EventTableFactory
    {
        internal readonly EventPropertyValueGetter HashGetter;
        internal readonly string[] OptionalKeyedProps;
        internal readonly Type[] OptKeyCoercedTypes;
        internal readonly Type[] OptRangeCoercedTypes;
        internal readonly EventPropertyValueGetter[] RangeGetters;
        internal readonly string[] RangeProps;
        internal readonly int StreamNum;

        public PropertyCompositeEventTableFactory(
            int streamNum,
            string[] optionalKeyedProps,
            Type[] optKeyCoercedTypes,
            EventPropertyValueGetter hashGetter,
            string[] rangeProps,
            Type[] optRangeCoercedTypes,
            EventPropertyValueGetter[] rangeGetters)
        {
            this.StreamNum = streamNum;
            this.OptionalKeyedProps = optionalKeyedProps;
            this.OptKeyCoercedTypes = optKeyCoercedTypes;
            this.HashGetter = hashGetter;
            this.RangeProps = rangeProps;
            this.OptRangeCoercedTypes = optRangeCoercedTypes;
            this.RangeGetters = rangeGetters;

            // construct chain
            IList<CompositeIndexEnterRemove> enterRemoves = new List<CompositeIndexEnterRemove>();
            if (optionalKeyedProps != null && optionalKeyedProps.Length > 0) {
                enterRemoves.Add(new CompositeIndexEnterRemoveKeyed(hashGetter));
            }

            foreach (var rangeGetter in rangeGetters) {
                enterRemoves.Add(new CompositeIndexEnterRemoveRange(rangeGetter));
            }

            // Hook up as chain for remove
            CompositeIndexEnterRemove last = null;
            foreach (var action in enterRemoves) {
                if (last != null) {
                    last.Next = action;
                }

                last = action;
            }

            Chain = enterRemoves[0];
        }

        public CompositeIndexEnterRemove Chain { get; }

        internal EventTableOrganization Organization => new EventTableOrganization(
            null,
            false,
            OptKeyCoercedTypes != null || OptRangeCoercedTypes != null,
            StreamNum,
            CombinedPropertyLists(OptionalKeyedProps, RangeProps),
            EventTableOrganizationType.COMPOSITE);

        public EventTable[] MakeEventTables(
            AgentInstanceContext agentInstanceContext,
            int? subqueryNumber)
        {
            return new EventTable[] {new PropertyCompositeEventTableImpl(this)};
        }

        public Type EventTableClass => typeof(PropertyCompositeEventTable);

        public string ToQueryPlan()
        {
            return GetType().Name +
                   " streamNum=" +
                   StreamNum +
                   " keys=" +
                   CompatExtensions.RenderAny(OptionalKeyedProps) +
                   " ranges=" +
                   CompatExtensions.RenderAny(RangeProps);
        }

        private string[] CombinedPropertyLists(
            string[] optionalKeyedProps,
            string[] rangeProps)
        {
            if (optionalKeyedProps == null) {
                return rangeProps;
            }

            if (rangeProps == null) {
                return optionalKeyedProps;
            }

            return (string[]) CollectionUtil.AddArrays(optionalKeyedProps, rangeProps);
        }
    }
} // end of namespace