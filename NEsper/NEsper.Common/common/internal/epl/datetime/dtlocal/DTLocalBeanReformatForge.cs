///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalBeanReformatForge : DTLocalForge
    {
        private readonly EventPropertyGetterSPI getter;
        private readonly Type getterResultType;
        private readonly DTLocalForge inner;
        private readonly Type returnType;

        public DTLocalBeanReformatForge(
            EventPropertyGetterSPI getter,
            Type getterResultType,
            DTLocalForge inner,
            Type returnType)
        {
            this.getter = getter;
            this.getterResultType = getterResultType;
            this.inner = inner;
            this.returnType = returnType;
        }

        public DTLocalEvaluator DTEvaluator {
            get => new DTLocalBeanReformatEval(getter, inner.DTEvaluator);
        }

        public CodegenExpression Codegen(
            CodegenExpression target,
            Type targetType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope
                .MakeChild(returnType, typeof(DTLocalBeanReformatForge), codegenClassScope)
                .AddParam(typeof(EventBean), "target");

            CodegenBlock block = methodNode.Block
                .DeclareVar(
                    getterResultType,
                    "timestamp",
                    getter.EventBeanGetCodegen(@Ref("target"), methodNode, codegenClassScope));
            if (!getterResultType.IsPrimitive) {
                block.IfRefNullReturnNull("timestamp");
            }

            CodegenExpression derefTimestamp = Unbox(Ref("timestamp"), getterResultType);

            block.MethodReturn(
                inner.Codegen(derefTimestamp, getterResultType, methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, target);
        }
    }
} // end of namespace