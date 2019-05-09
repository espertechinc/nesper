///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public abstract class ExprPlugInSingleRowNodeForge : ExprForgeInstrumentable,
        EventPropertyValueGetterForge
    {
        private readonly ExprPlugInSingleRowNode parent;

        protected ExprPlugInSingleRowNodeForge(
            ExprPlugInSingleRowNode parent,
            bool isReturnsConstantResult)
        {
            this.parent = parent;
            IsReturnsConstantResult = isReturnsConstantResult;
        }

        public abstract MethodInfo Method { get; }

        public bool IsReturnsConstantResult { get; }

        public abstract ExprEvaluator ExprEvaluator { get; }

        public abstract CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        public abstract Type EvaluationType { get; }
        public abstract ExprForgeConstantType ForgeConstantType { get; }

        public abstract CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        public abstract CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        protected CodegenExpression[] MethodAsParams {
            get {
                var method = Method;
                var parameterTypes = method.GetParameterTypes().Select(type => type.FullName).ToArray();
                return new[] {
                    Constant(method.DeclaringType.FullName),
                    Constant(method.Name), Constant(method.ReturnType.GetSimpleName()), Constant(parameterTypes)
                };
            }
        }

        public ExprNodeRenderable ExprForgeRenderable => parent;

        public bool HasMethodInvocationContextParam()
        {
            foreach (var param in Method.GetParameterTypes()) {
                if (param == typeof(EPLMethodInvocationContext)) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace