///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    [Serializable]
    public class QueryGraphValueEntryInKeywordMultiIdxForge : QueryGraphValueEntryForge
    {
        internal QueryGraphValueEntryInKeywordMultiIdxForge(ExprNode keyExpr)
        {
            KeyExpr = keyExpr;
        }

        public ExprNode KeyExpr { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            throw new UnsupportedOperationException("Fire-and-forget queries don't support in-clause multi-indexes");
        }

        public string ToQueryPlan()
        {
            return "in-keyword multi-indexed single keyed lookup " +
                   ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(KeyExpr);
        }
    }
} // end of namespace