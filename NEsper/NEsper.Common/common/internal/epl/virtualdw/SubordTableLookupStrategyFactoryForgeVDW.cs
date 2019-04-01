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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    /// <summary>
    ///     Strategy for looking up, in some sort of table or index, or a set of events, potentially based on the
    ///     events properties, and returning a set of matched events.
    /// </summary>
    public class SubordTableLookupStrategyFactoryForgeVDW : SubordTableLookupStrategyFactoryForge
    {
        private readonly Attribute[] _annotations;
        private readonly bool _forceTableScan;
        private readonly SubordinateQueryPlannerIndexPropListPair _hashAndRanges;
        private readonly CoercionDesc _hashKeyCoercionTypes;
        private readonly IList<SubordPropHashKeyForge> _hashKeys;
        private readonly SubordPropPlan _joinDesc;
        private readonly bool _nwOnTrigger;
        private readonly EventType[] _outerStreams;
        private readonly CoercionDesc _rangeKeyCoercionTypes;
        private readonly IList<SubordPropRangeKeyForge> _rangeKeys;
        private readonly string _statementName;

        public SubordTableLookupStrategyFactoryForgeVDW(
            string statementName, Attribute[] annotations, EventType[] outerStreams,
            IList<SubordPropHashKeyForge> hashKeys, CoercionDesc hashKeyCoercionTypes,
            IList<SubordPropRangeKeyForge> rangeKeys, CoercionDesc rangeKeyCoercionTypes, bool nwOnTrigger,
            SubordPropPlan joinDesc, bool forceTableScan, SubordinateQueryPlannerIndexPropListPair hashAndRanges)
        {
            _statementName = statementName;
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

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var hashes = new ExprNode[_hashKeys.Count];
            var hashTypes = new Type[_hashKeys.Count];
            for (var i = 0; i < _hashKeys.Count; i++) {
                hashes[i] = _hashKeys[i].HashKey.KeyExpr;
                hashTypes[i] = _hashKeyCoercionTypes.CoercionTypes[i];
            }

            var ranges = new QueryGraphValueEntryRangeForge[_rangeKeys.Count];
            var rangesTypes = new Type[_rangeKeys.Count];
            for (var i = 0; i < _rangeKeys.Count; i++) {
                ranges[i] = _rangeKeys[i].RangeInfo;
                rangesTypes[i] = _rangeKeyCoercionTypes.CoercionTypes[i];
            }

            var builder = new SAIFFInitializeBuilder(
                typeof(SubordTableLookupStrategyFactoryVDW), GetType(), "lookup", parent, symbols, classScope);
            builder
                .Expression("indexHashedProps", IndexedPropDesc.MakeArray(_hashAndRanges.HashedProps))
                .Expression("indexBtreeProps", IndexedPropDesc.MakeArray(_hashAndRanges.BtreeProps))
                .Constant("nwOnTrigger", _nwOnTrigger)
                .Constant("numOuterStreams", _outerStreams.Length)
                .Expression(
                    "hashEvals",
                    ExprNodeUtilityCodegen.CodegenEvaluators(hashes, builder.Method, GetType(), classScope))
                .Constant("hashCoercionTypes", hashTypes)
                .Expression(
                    "rangeEvals", QueryGraphValueEntryRangeForge.MakeArray(ranges, builder.Method, symbols, classScope))
                .Constant("rangeCoercionTypes", rangesTypes);
            return builder.Build();
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace