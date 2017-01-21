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
using com.espertech.esper.collection;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Strategy for looking up, in some sort of table or index, or a set of events, potentially based on the
    /// events properties, and returning a set of matched events.
    /// </summary>
    public class SubordTableLookupStrategyFactoryVDW : SubordTableLookupStrategyFactory
    {
        private readonly string _statementName;
        private readonly int _statementId;
        private readonly Attribute[] _annotations;
        private readonly EventType[] _outerStreams;
        private readonly IList<SubordPropHashKey> _hashKeys;
        private readonly CoercionDesc _hashKeyCoercionTypes;
        private readonly IList<SubordPropRangeKey> _rangeKeys;
        private readonly CoercionDesc _rangeKeyCoercionTypes;
        private readonly bool _nwOnTrigger;
        private readonly SubordPropPlan _joinDesc;
        private readonly bool _forceTableScan;
        private readonly SubordinateQueryPlannerIndexPropListPair _hashAndRanges;
    
        public SubordTableLookupStrategyFactoryVDW(string statementName, int statementId, Attribute[] annotations, EventType[] outerStreams, IList<SubordPropHashKey> hashKeys, CoercionDesc hashKeyCoercionTypes, IList<SubordPropRangeKey> rangeKeys, CoercionDesc rangeKeyCoercionTypes, bool nwOnTrigger, SubordPropPlan joinDesc, bool forceTableScan, SubordinateQueryPlannerIndexPropListPair hashAndRanges)
        {
            _statementName = statementName;
            _statementId = statementId;
            _annotations = annotations;
            _outerStreams = outerStreams;
            _hashKeys = hashKeys;
            _hashKeyCoercionTypes = hashKeyCoercionTypes;
            _rangeKeys = rangeKeys;
            _rangeKeyCoercionTypes = rangeKeyCoercionTypes;
            _nwOnTrigger = nwOnTrigger;
            _joinDesc = joinDesc;
            _forceTableScan = forceTableScan;
            _hashAndRanges = hashAndRanges;
        }
    
        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            Pair<IndexMultiKey,EventTable> tableVW = vdw.GetSubordinateQueryDesc(false, _hashAndRanges.HashedProps, _hashAndRanges.BtreeProps);
            return vdw.GetSubordinateLookupStrategy(
                _statementName,
                _statementId,
                _annotations,
                _outerStreams, _hashKeys, _hashKeyCoercionTypes, _rangeKeys, _rangeKeyCoercionTypes, _nwOnTrigger,
                tableVW.Second, _joinDesc, _forceTableScan);
        }
    
        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
}
