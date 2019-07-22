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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    /// Plan to perform an indexed table lookup.
    /// </summary>
    public class InKeywordTableLookupPlanMultiIdxForge : TableLookupPlanForge
    {
        private ExprNode keyExpr;

        public InKeywordTableLookupPlanMultiIdxForge(
            int lookupStream,
            int indexedStream,
            bool indexedStreamIsVDW,
            EventType[] typesPerStream,
            TableLookupIndexReqKey[] indexNum,
            ExprNode keyExpr)
            : base(lookupStream, indexedStream, indexedStreamIsVDW, typesPerStream, indexNum)

        {
            this.keyExpr = keyExpr;
        }

        public ExprNode KeyExpr {
            get => keyExpr;
        }

        public override TableLookupKeyDesc KeyDescriptor {
            get {
                return new TableLookupKeyDesc(
                    Collections.GetEmptyList<QueryGraphValueEntryHashKeyedForge>(),
                    Collections.GetEmptyList<QueryGraphValueEntryRangeForge>());
            }
        }

        public override string ToString()
        {
            return this.GetType().Name +
                   " " +
                   base.ToString() +
                   " keyProperties=" +
                   ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(keyExpr);
        }

        public override Type TypeOfPlanFactory()
        {
            return typeof(InKeywordTableLookupPlanMultiIdxFactory);
        }

        public override ICollection<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return Collections.SingletonList<CodegenExpression>(
                CodegenEvaluator(keyExpr.Forge, method, this.GetType(), classScope));
        }
    }
} // end of namespace