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
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan to perform an indexed table lookup.
    /// </summary>
    public class InKeywordTableLookupPlanSingleIdxForge : TableLookupPlanForge
    {
        public InKeywordTableLookupPlanSingleIdxForge(
            int lookupStream,
            int indexedStream,
            bool indexedStreamIsVDW,
            EventType[] typesPerStream,
            TableLookupIndexReqKey indexNum,
            ExprNode[] expressions)
            : base(lookupStream, indexedStream, indexedStreamIsVDW, typesPerStream, new[] {indexNum})

        {
            Expressions = expressions;
        }

        public ExprNode[] Expressions { get; }

        public override TableLookupKeyDesc KeyDescriptor => new TableLookupKeyDesc(
            Collections.GetEmptyList<QueryGraphValueEntryHashKeyedForge>(),
            Collections.GetEmptyList<QueryGraphValueEntryRangeForge>());

        public override string ToString()
        {
            return GetType().Name +
                   " " +
                   base.ToString() +
                   " keyProperties=" +
                   ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceAsList(Expressions);
        }

        public override Type TypeOfPlanFactory()
        {
            return typeof(InKeywordTableLookupPlanSingleIdxFactory);
        }

        public override ICollection<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return Collections.SingletonList(
                ExprNodeUtilityCodegen.CodegenEvaluators(
                    Expressions,
                    method,
                    GetType(),
                    classScope));
        }
    }
} // end of namespace