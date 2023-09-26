///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    /// Plan to perform an indexed table lookup.
    /// </summary>
    public class IndexedTableLookupPlanHashedOnlyForge : TableLookupPlanForge
    {
        private readonly QueryGraphValueEntryHashKeyedForge[] hashKeys;
        private readonly QueryPlanIndexForge indexSpecs;
        private readonly Type[] optionalCoercionTypes;
        private readonly MultiKeyClassRef optionalEPLTableLookupMultiKey;

        public IndexedTableLookupPlanHashedOnlyForge(
            int lookupStream,
            int indexedStream,
            bool indexedStreamIsVDW,
            EventType[] typesPerStream,
            TableLookupIndexReqKey indexNum,
            QueryGraphValueEntryHashKeyedForge[] hashKeys,
            QueryPlanIndexForge indexSpecs,
            Type[] optionalCoercionTypes,
            MultiKeyClassRef optionalEPLTableLookupMultiKey) : base(
            lookupStream,
            indexedStream,
            indexedStreamIsVDW,
            typesPerStream,
            new TableLookupIndexReqKey[] { indexNum })
        {
            this.hashKeys = hashKeys;
            this.indexSpecs = indexSpecs;
            this.optionalCoercionTypes = optionalCoercionTypes;
            this.optionalEPLTableLookupMultiKey = optionalEPLTableLookupMultiKey;
        }

        public override string ToString()
        {
            return "IndexedTableLookupPlan " + base.ToString() + " keyProperty=" + KeyDescriptor;
        }

        public override Type TypeOfPlanFactory()
        {
            return typeof(IndexedTableLookupPlanHashedOnlyFactory);
        }

        public override ICollection<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var forges = QueryGraphValueEntryHashKeyedForge.GetForges(hashKeys);
            var types = ExprNodeUtilityQuery.GetExprResultTypes(forges);
            // we take coercion types from the index plan as the index plan is always accurate but not always available (for tables it is not)
            Type[] coercionTypes;
            var indexForge = indexSpecs.Items.Get(IndexNum[0]);
            if (indexForge != null) {
                coercionTypes = indexForge.HashTypes;
            }
            else {
                coercionTypes = optionalCoercionTypes;
            }

            CodegenExpression getter;
            var eventType = typesPerStream[LookupStream];
            var getterSPIS = QueryGraphValueEntryHashKeyedForge.GetGettersIfPropsOnly(hashKeys);
            if (indexForge != null) {
                if (getterSPIS != null) {
                    getter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                        eventType,
                        getterSPIS,
                        types,
                        coercionTypes,
                        indexForge.HashMultiKeyClasses,
                        method,
                        classScope);
                }
                else {
                    getter = MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(
                        forges,
                        coercionTypes,
                        indexForge.HashMultiKeyClasses,
                        method,
                        classScope);
                }
            }
            else {
                if (getterSPIS != null) {
                    getter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                        eventType,
                        getterSPIS,
                        types,
                        coercionTypes,
                        optionalEPLTableLookupMultiKey,
                        method,
                        classScope);
                }
                else {
                    getter = MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(
                        forges,
                        coercionTypes,
                        optionalEPLTableLookupMultiKey,
                        method,
                        classScope);
                }
            }

            return Collections.SingletonList(getter);
        }

        public override TableLookupKeyDesc KeyDescriptor => new TableLookupKeyDesc(
            Arrays.AsList(hashKeys),
            EmptyList<QueryGraphValueEntryRangeForge>.Instance);

        public QueryGraphValueEntryHashKeyedForge[] HashKeys => hashKeys;
    }
} // end of namespace