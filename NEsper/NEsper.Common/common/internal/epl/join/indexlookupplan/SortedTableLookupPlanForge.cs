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
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan to perform an indexed table lookup.
    /// </summary>
    public class SortedTableLookupPlanForge : TableLookupPlanForge
    {
        private readonly Type optionalCoercionType;

        public SortedTableLookupPlanForge(
            int lookupStream,
            int indexedStream,
            bool indexedStreamIsVDW,
            EventType[] typesPerStream,
            TableLookupIndexReqKey indexNum,
            QueryGraphValueEntryRangeForge rangeKeyPair,
            Type optionalCoercionType)
            :
            base(lookupStream, indexedStream, indexedStreamIsVDW, typesPerStream, new[] {indexNum})
        {
            RangeKeyPair = rangeKeyPair;
            this.optionalCoercionType = optionalCoercionType;
        }

        public QueryGraphValueEntryRangeForge RangeKeyPair { get; }

        public override TableLookupKeyDesc KeyDescriptor => new TableLookupKeyDesc(
            Collections.GetEmptyList<QueryGraphValueEntryHashKeyedForge>(),
            Collections.SingletonList(RangeKeyPair));

        public override string ToString()
        {
            return "SortedTableLookupPlan " +
                   base.ToString() +
                   " keyProperties=" +
                   RangeKeyPair.ToQueryPlan();
        }

        public override Type TypeOfPlanFactory()
        {
            return typeof(SortedTableLookupPlanFactory);
        }

        public override ICollection<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return Collections.SingletonList(RangeKeyPair.Make(optionalCoercionType, method, symbols, classScope));
        }
    }
} // end of namespace