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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotMethodForgeNoDuckEvalWrapArray : ExprDotMethodForgeNoDuckEvalPlain
    {
        public ExprDotMethodForgeNoDuckEvalWrapArray(
            ExprDotMethodForgeNoDuck forge,
            ExprEvaluator[] parameters)
            : base(forge, parameters)
        {
        }

        public override EPType TypeInfo => EPTypeHelper.CollectionOfSingleValue(forge.Method.ReturnType.GetElementType());

        public override object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = base.Evaluate(target, eventsPerStream, isNewData, exprEvaluatorContext);
            if (result == null || !result.GetType().IsArray) {
                return null;
            }

            return CollectionUtil.ArrayToCollectionAllowNull<object>(result);
        }

        public static CodegenExpression CodegenWrapArray(
            ExprDotMethodForgeNoDuck forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    typeof(ICollection<object>), typeof(ExprDotMethodForgeNoDuckEvalWrapArray), codegenClassScope)
                .AddParam(innerType, "target");

            var returnType = forge.Method.ReturnType;
            methodNode.Block
                .DeclareVar(
                    returnType.GetBoxedType(), "array",
                    CodegenPlain(forge, Ref("target"), innerType, methodNode, exprSymbol, codegenClassScope))
                .MethodReturn(
                    CollectionUtil.ArrayToCollectionAllowNullCodegen(
                        methodNode, returnType, Ref("array"), codegenClassScope));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace