///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalBeanReformatForge : DTLocalForge
    {
        private readonly EventPropertyGetterSPI _getter;
        private readonly Type _getterResultType;
        private readonly DTLocalForge _inner;
        private readonly Type _returnType;

        public DTLocalBeanReformatForge(
            EventPropertyGetterSPI getter,
            Type getterResultType,
            DTLocalForge inner,
            Type returnType)
        {
            _getter = getter;
            _getterResultType = getterResultType;
            _inner = inner;
            _returnType = returnType;
        }

        public DTLocalEvaluator DTEvaluator => new DTLocalBeanReformatEval(_getter, _inner.DTEvaluator);

        public CodegenExpression Codegen(
            CodegenExpression target,
            Type targetType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(_returnType, typeof(DTLocalBeanReformatForge), codegenClassScope)
                .AddParam<EventBean>("target");

            var block = methodNode.Block
                .DeclareVar(
                    _getterResultType,
                    "timestamp",
                    _getter.EventBeanGetCodegen(Ref("target"), methodNode, codegenClassScope));
            if (!_getterResultType.IsPrimitive) {
                block.IfRefNullReturnNull("timestamp");
            }

            var derefTimestamp = Unbox(Ref("timestamp"), _getterResultType);

            block.MethodReturn(
                _inner.Codegen(
                    derefTimestamp, _getterResultType, methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, target);
        }
    }
} // end of namespace