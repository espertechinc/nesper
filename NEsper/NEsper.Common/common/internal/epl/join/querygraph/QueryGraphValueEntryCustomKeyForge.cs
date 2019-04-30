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
    public class QueryGraphValueEntryCustomKeyForge : QueryGraphValueEntryForge
    {
        public QueryGraphValueEntryCustomKeyForge(
            string operationName,
            ExprNode[] exprNodes)
        {
            OperationName = operationName;
            ExprNodes = exprNodes;
        }

        public string OperationName { get; }

        public ExprNode[] ExprNodes { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryGraphValueEntryCustomKey), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(QueryGraphValueEntryCustomKey), "key", NewInstance(typeof(QueryGraphValueEntryCustomKey)))
                .SetProperty(Ref("key"), "OperationName", Constant(OperationName))
                .SetProperty(Ref("key"), "ExprNodes",
                    ExprNodeUtilityCodegen.CodegenEvaluators(ExprNodes, method, GetType(), classScope))
                .SetProperty(Ref("key"), "Expressions",
                    Constant(ExprNodeUtilityPrint.ToExpressionStringsMinPrecedence(ExprNodes)))
                .MethodReturn(Ref("key"));
            return LocalMethod(method);
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (QueryGraphValueEntryCustomKeyForge) o;

            if (!OperationName.Equals(that.OperationName)) {
                return false;
            }

            return ExprNodeUtilityCompare.DeepEquals(ExprNodes, that.ExprNodes, true);
        }

        public override int GetHashCode()
        {
            return OperationName.GetHashCode();
        }
    }
} // end of namespace