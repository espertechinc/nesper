///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public class QueryGraphValueEntryCustomOperationForge : QueryGraphValueEntryForge
    {
        public IDictionary<int, ExprNode> PositionalExpressions { get; } = new Dictionary<int, ExprNode>();

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryGraphValueEntryCustomOperation), GetType(), classScope);
            method.Block.DeclareVar<IDictionary<int, ExprNode>>(
                "map",
                NewInstance(
                    typeof(Dictionary<int, ExprNode>),
                    Constant(CollectionUtil.CapacityHashMap(PositionalExpressions.Count))));
            foreach (var entry in PositionalExpressions) {
                method.Block.ExprDotMethod(
                    Ref("map"),
                    "put",
                    Constant(entry.Key),
                    ExprNodeUtilityCodegen.CodegenEvaluator(entry.Value.Forge, method, GetType(), classScope));
            }

            method.Block
                .DeclareVar<QueryGraphValueEntryCustomOperation>(
                    "op",
                    NewInstance(typeof(QueryGraphValueEntryCustomOperation)))
                .SetProperty(Ref("op"), "PositionalExpressions", Ref("map"))
                .MethodReturn(Ref("op"));
            return LocalMethod(method);
        }
    }
} // end of namespace