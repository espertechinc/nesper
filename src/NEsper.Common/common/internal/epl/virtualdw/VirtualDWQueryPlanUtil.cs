///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class VirtualDWQueryPlanUtil
    {
        private static readonly EventTableOrganization TABLE_ORGANIZATION =
            new EventTableOrganization(null, false, false, 0, null, EventTableOrganizationType.VDW);

        public static Pair<IndexMultiKey, VirtualDWEventTable> GetSubordinateQueryDesc(
            bool unique,
            IndexedPropDesc[] hashedProps,
            IndexedPropDesc[] btreeProps)
        {
            IList<VirtualDataWindowLookupFieldDesc> hashFields = new List<VirtualDataWindowLookupFieldDesc>();
            foreach (var hashprop in hashedProps) {
                hashFields.Add(
                    new VirtualDataWindowLookupFieldDesc(
                        hashprop.IndexPropName,
                        VirtualDataWindowLookupOp.EQUALS,
                        hashprop.CoercionType));
            }

            IList<VirtualDataWindowLookupFieldDesc> btreeFields = new List<VirtualDataWindowLookupFieldDesc>();
            foreach (var btreeprop in btreeProps) {
                btreeFields.Add(
                    new VirtualDataWindowLookupFieldDesc(btreeprop.IndexPropName, null, btreeprop.CoercionType));
            }

            var eventTable = new VirtualDWEventTable(unique, hashFields, btreeFields, TABLE_ORGANIZATION);
            var imk = new IndexMultiKey(unique, hashedProps, btreeProps, null);
            return new Pair<IndexMultiKey, VirtualDWEventTable>(imk, eventTable);
        }

        public static EventTable GetJoinIndexTable(QueryPlanIndexItem queryPlanIndexItem)
        {
            IList<VirtualDataWindowLookupFieldDesc> hashFields = new List<VirtualDataWindowLookupFieldDesc>();
            var count = 0;
            if (queryPlanIndexItem.HashProps != null) {
                foreach (var indexProp in queryPlanIndexItem.HashProps) {
                    var coercionType = queryPlanIndexItem.HashPropTypes == null
                        ? null
                        : queryPlanIndexItem.HashPropTypes[count];
                    hashFields.Add(
                        new VirtualDataWindowLookupFieldDesc(
                            indexProp,
                            VirtualDataWindowLookupOp.EQUALS,
                            coercionType));
                    count++;
                }
            }

            IList<VirtualDataWindowLookupFieldDesc> btreeFields = new List<VirtualDataWindowLookupFieldDesc>();
            count = 0;
            if (queryPlanIndexItem.RangeProps != null) {
                foreach (var btreeprop in queryPlanIndexItem.RangeProps) {
                    var coercionType = queryPlanIndexItem.RangePropTypes == null
                        ? null
                        : queryPlanIndexItem.RangePropTypes[count];
                    btreeFields.Add(new VirtualDataWindowLookupFieldDesc(btreeprop, null, coercionType));
                    count++;
                }
            }

            return new VirtualDWEventTable(false, hashFields, btreeFields, TABLE_ORGANIZATION);
        }

        public static Pair<IndexMultiKey, EventTable> GetFireAndForgetDesc(
            EventType eventType,
            ISet<string> keysAvailable,
            ISet<string> rangesAvailable)
        {
            IList<VirtualDataWindowLookupFieldDesc> hashFields = new List<VirtualDataWindowLookupFieldDesc>();
            IList<IndexedPropDesc> hashIndexedFields = new List<IndexedPropDesc>();
            foreach (var hashprop in keysAvailable) {
                hashFields.Add(new VirtualDataWindowLookupFieldDesc(hashprop, VirtualDataWindowLookupOp.EQUALS, null));
                hashIndexedFields.Add(new IndexedPropDesc(hashprop, eventType.GetPropertyType(hashprop)));
            }

            IList<VirtualDataWindowLookupFieldDesc> btreeFields = new List<VirtualDataWindowLookupFieldDesc>();
            IList<IndexedPropDesc> btreeIndexedFields = new List<IndexedPropDesc>();
            foreach (var btreeprop in rangesAvailable) {
                btreeFields.Add(new VirtualDataWindowLookupFieldDesc(btreeprop, null, null));
                btreeIndexedFields.Add(new IndexedPropDesc(btreeprop, eventType.GetPropertyType(btreeprop)));
            }

            var noopTable = new VirtualDWEventTable(false, hashFields, btreeFields, TABLE_ORGANIZATION);
            var imk = new IndexMultiKey(false, hashIndexedFields, btreeIndexedFields, null);

            return new Pair<IndexMultiKey, EventTable>(imk, noopTable);
        }
    }
} // end of namespace