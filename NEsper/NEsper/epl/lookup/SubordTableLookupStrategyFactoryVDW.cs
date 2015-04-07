///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Strategy for looking up, in some sort of table or index, or a set of events, potentially based on the
    /// events properties, and returning a set of matched events.
    /// </summary>
    public class SubordTableLookupStrategyFactoryVDW : SubordTableLookupStrategyFactory
    {
        private readonly string statementName;
        private readonly string statementId;
        private readonly Attribute[] annotations;
        private readonly EventType[] outerStreams;
        private readonly IList<SubordPropHashKey> hashKeys;
        private readonly CoercionDesc hashKeyCoercionTypes;
        private readonly IList<SubordPropRangeKey> rangeKeys;
        private readonly CoercionDesc rangeKeyCoercionTypes;
        private readonly bool nwOnTrigger;
        private readonly SubordPropPlan joinDesc;
        private readonly bool forceTableScan;
        private readonly SubordinateQueryPlannerIndexPropListPair hashAndRanges;
    
        public SubordTableLookupStrategyFactoryVDW(string statementName, string statementId, Attribute[] annotations, EventType[] outerStreams, IList<SubordPropHashKey> hashKeys, CoercionDesc hashKeyCoercionTypes, IList<SubordPropRangeKey> rangeKeys, CoercionDesc rangeKeyCoercionTypes, bool nwOnTrigger, SubordPropPlan joinDesc, bool forceTableScan, SubordinateQueryPlannerIndexPropListPair hashAndRanges) {
            this.statementName = statementName;
            this.statementId = statementId;
            this.annotations = annotations;
            this.outerStreams = outerStreams;
            this.hashKeys = hashKeys;
            this.hashKeyCoercionTypes = hashKeyCoercionTypes;
            this.rangeKeys = rangeKeys;
            this.rangeKeyCoercionTypes = rangeKeyCoercionTypes;
            this.nwOnTrigger = nwOnTrigger;
            this.joinDesc = joinDesc;
            this.forceTableScan = forceTableScan;
            this.hashAndRanges = hashAndRanges;
        }
    
        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw) {
            Pair<IndexMultiKey,EventTable> tableVW = vdw.GetSubordinateQueryDesc(false, hashAndRanges.HashedProps, hashAndRanges.BtreeProps);
            return vdw.GetSubordinateLookupStrategy(statementName,
                    statementId, annotations,
                    outerStreams, hashKeys, hashKeyCoercionTypes, rangeKeys, rangeKeyCoercionTypes, nwOnTrigger,
                    tableVW.Second, joinDesc, forceTableScan);
        }
    
        public string ToQueryPlan() {
            return this.GetType().Name;
        }
    }
}
