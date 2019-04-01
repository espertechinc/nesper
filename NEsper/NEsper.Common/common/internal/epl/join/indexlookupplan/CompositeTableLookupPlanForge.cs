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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan to perform an indexed table lookup.
    /// </summary>
    public class CompositeTableLookupPlanForge : TableLookupPlanForge
    {
        private readonly Type[] hashCoercionTypes;
        private readonly IList<QueryGraphValueEntryHashKeyedForge> hashKeys;
        private readonly Type[] optRangeCoercionTypes;
        private readonly IList<QueryGraphValueEntryRangeForge> rangeKeyPairs;

        public CompositeTableLookupPlanForge(
            int lookupStream, int indexedStream, bool indexedStreamIsVDW, EventType[] typesPerStream,
            TableLookupIndexReqKey indexNum, IList<QueryGraphValueEntryHashKeyedForge> hashKeys,
            Type[] hashCoercionTypes, IList<QueryGraphValueEntryRangeForge> rangeKeyPairs, Type[] optRangeCoercionTypes)
            : base(lookupStream, indexedStream, indexedStreamIsVDW, typesPerStream, new[] {indexNum})
        {
            this.hashKeys = hashKeys;
            this.hashCoercionTypes = hashCoercionTypes;
            this.rangeKeyPairs = rangeKeyPairs;
            this.optRangeCoercionTypes = optRangeCoercionTypes;
        }

        public override TableLookupKeyDesc KeyDescriptor => new TableLookupKeyDesc(hashKeys, rangeKeyPairs);

        public override Type TypeOfPlanFactory()
        {
            return typeof(CompositeTableLookupPlanFactory);
        }

        public override ICollection<CodegenExpression> AdditionalParams(
            CodegenMethod method, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var hashGetter = ConstantNull();
            if (!hashKeys.IsEmpty()) {
                var forges = QueryGraphValueEntryHashKeyedForge.GetForges(hashKeys.ToArray());
                hashGetter = ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                    forges, hashCoercionTypes, method, GetType(), classScope);
            }

            var rangeGetters = method.MakeChild(typeof(QueryGraphValueEntryRange[]), GetType(), classScope);
            rangeGetters.Block.DeclareVar(
                typeof(QueryGraphValueEntryRange[]), "rangeGetters",
                NewArrayByLength(typeof(QueryGraphValueEntryRange), Constant(rangeKeyPairs.Count)));
            for (var i = 0; i < rangeKeyPairs.Count; i++) {
                var optCoercionType = optRangeCoercionTypes == null ? null : optRangeCoercionTypes[i];
                rangeGetters.Block.AssignArrayElement(
                    Ref("rangeGetters"), Constant(i),
                    rangeKeyPairs[i].Make(optCoercionType, rangeGetters, symbols, classScope));
            }

            rangeGetters.Block.MethodReturn(Ref("rangeGetters"));

            return Arrays.AsList(hashGetter, LocalMethod(rangeGetters));
        }

        public override string ToString()
        {
            return "CompositeTableLookupPlan " +
                   base.ToString() +
                   " directKeys=" + QueryGraphValueEntryHashKeyedForge.ToQueryPlan(hashKeys) +
                   " rangeKeys=" + QueryGraphValueEntryRangeForge.ToQueryPlan(rangeKeyPairs);
        }
    }
} // end of namespace