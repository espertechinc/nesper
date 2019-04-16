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
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan for a full table scan.
    /// </summary>
    public class FullTableScanLookupPlanForge : TableLookupPlanForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to full table scan</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        /// <param name="typesPerStream">types</param>
        /// <param name="indexedStreamIsVDW">vdw indicator</param>
        public FullTableScanLookupPlanForge(
            int lookupStream,
            int indexedStream,
            bool indexedStreamIsVDW,
            EventType[] typesPerStream,
            TableLookupIndexReqKey indexNum)
            : base(lookupStream, indexedStream, indexedStreamIsVDW, typesPerStream, new[] {indexNum})
        {
        }

        public override TableLookupKeyDesc KeyDescriptor => new TableLookupKeyDesc(
            Collections.GetEmptyList<QueryGraphValueEntryHashKeyedForge>(),
            Collections.GetEmptyList<QueryGraphValueEntryRangeForge>());

        public override string ToString()
        {
            return "FullTableScanLookupPlan " +
                   base.ToString();
        }

        public override Type TypeOfPlanFactory()
        {
            return typeof(FullTableScanLookupPlanFactory);
        }

        public override ICollection<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return Collections.GetEmptyList<CodegenExpression>();
        }
    }
} // end of namespace