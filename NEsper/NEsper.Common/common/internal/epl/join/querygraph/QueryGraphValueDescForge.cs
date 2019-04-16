///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public class QueryGraphValueDescForge
    {
        public QueryGraphValueDescForge(
            ExprNode[] indexExprs,
            QueryGraphValueEntryForge entry)
        {
            IndexExprs = indexExprs;
            Entry = entry;
        }

        public ExprNode[] IndexExprs { get; }

        public QueryGraphValueEntryForge Entry { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var indexes = new string[IndexExprs.Length];
            for (var i = 0; i < indexes.Length; i++) {
                indexes[i] = GetSingleIdentNodeProp(IndexExprs[i]);
            }

            return NewInstance(typeof(QueryGraphValueDesc), Constant(indexes), Entry.Make(parent, symbols, classScope));
        }

        private string GetSingleIdentNodeProp(ExprNode indexExpr)
        {
            var identNode = (ExprIdentNode) indexExpr;
            return identNode.ResolvedPropertyName;
        }
    }
} // end of namespace