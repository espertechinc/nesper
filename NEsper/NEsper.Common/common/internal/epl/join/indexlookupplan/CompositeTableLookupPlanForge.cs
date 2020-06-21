///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan to perform an indexed table lookup.
    /// </summary>
    public class CompositeTableLookupPlanForge : TableLookupPlanForge
    {
        private readonly Type[] _hashCoercionTypes;
        private readonly IList<QueryGraphValueEntryHashKeyedForge> _hashKeys;
        private readonly Type[] _optRangeCoercionTypes;
        private readonly IList<QueryGraphValueEntryRangeForge> _rangeKeyPairs;
        private readonly QueryPlanIndexForge _indexSpecs;
        private readonly MultiKeyClassRef _optionalEPLTableLookupMultiKey;

        public CompositeTableLookupPlanForge(
            int lookupStream,
            int indexedStream,
            bool indexedStreamIsVDW,
            EventType[] typesPerStream,
            TableLookupIndexReqKey indexNum,
            IList<QueryGraphValueEntryHashKeyedForge> hashKeys,
            Type[] hashCoercionTypes,
            IList<QueryGraphValueEntryRangeForge> rangeKeyPairs,
            Type[] optRangeCoercionTypes,
            QueryPlanIndexForge indexSpecs,
            MultiKeyClassRef optionalEPLTableLookupMultiKey)
            : base(lookupStream, indexedStream, indexedStreamIsVDW, typesPerStream, new[] {indexNum})
        {
            _hashKeys = hashKeys;
            _hashCoercionTypes = hashCoercionTypes;
            _rangeKeyPairs = rangeKeyPairs;
            _optRangeCoercionTypes = optRangeCoercionTypes;
            _indexSpecs = indexSpecs;
            _optionalEPLTableLookupMultiKey = optionalEPLTableLookupMultiKey;
        }

        public override TableLookupKeyDesc KeyDescriptor => new TableLookupKeyDesc(_hashKeys, _rangeKeyPairs);

        public override Type TypeOfPlanFactory()
        {
            return typeof(CompositeTableLookupPlanFactory);
        }

        public override ICollection<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var hashGetter = ConstantNull();
            if (!_hashKeys.IsEmpty()) {
                var indexForge = _indexSpecs.Items.Get(IndexNum[0]);
                var forges = QueryGraphValueEntryHashKeyedForge.GetForges(_hashKeys.ToArray());
                if (indexForge != null) {
                    hashGetter = MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(
                        forges,
                        _hashCoercionTypes,
                        indexForge.HashMultiKeyClasses,
                        method,
                        classScope);
                } else {
                    hashGetter = MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(
                        forges,
                        _hashCoercionTypes,
                        _optionalEPLTableLookupMultiKey,
                        method,
                        classScope);
                }
            }

            var rangeGetters = method.MakeChild(typeof(QueryGraphValueEntryRange[]), GetType(), classScope);
            rangeGetters.Block.DeclareVar<QueryGraphValueEntryRange[]>(
                "rangeGetters",
                NewArrayByLength(typeof(QueryGraphValueEntryRange), Constant(_rangeKeyPairs.Count)));
            for (var i = 0; i < _rangeKeyPairs.Count; i++) {
                var optCoercionType = _optRangeCoercionTypes == null ? null : _optRangeCoercionTypes[i];
                rangeGetters.Block.AssignArrayElement(
                    Ref("rangeGetters"),
                    Constant(i),
                    _rangeKeyPairs[i].Make(optCoercionType, rangeGetters, symbols, classScope));
            }

            rangeGetters.Block.MethodReturn(Ref("rangeGetters"));

            return Arrays.AsList(hashGetter, LocalMethod(rangeGetters));
        }

        public override string ToString()
        {
            return "CompositeTableLookupPlan " +
                   base.ToString() +
                   " directKeys=" +
                   QueryGraphValueEntryHashKeyedForge.ToQueryPlan(_hashKeys) +
                   " rangeKeys=" +
                   QueryGraphValueEntryRangeForge.ToQueryPlan(_rangeKeyPairs);
        }
    }
} // end of namespace