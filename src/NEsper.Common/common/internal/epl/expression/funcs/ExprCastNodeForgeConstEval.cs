///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCastNodeForgeConstEval : ExprEvaluator
    {
        private readonly ExprCastNodeForge _forge;
        private readonly object _theConstant;

        public ExprCastNodeForgeConstEval(
            ExprCastNodeForge forge,
            object theConstant)
        {
            this._forge = forge;
            this._theConstant = theConstant;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return _theConstant;
        }

        public static CodegenExpression Codegen(
            ExprCastNodeForge forge,
            CodegenClassScope codegenClassScope)
        {
            if (forge.EvaluationType == null) {
                return ConstantNull();
            }

            var evaluationType = forge.EvaluationType.GetBoxedType();
            var initMethod = codegenClassScope.NamespaceScope.InitMethod.MakeChildWithScope(
                evaluationType,
                typeof(ExprCastNodeForgeConstEval),
                CodegenSymbolProviderEmpty.INSTANCE,
                codegenClassScope);

            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            var compute = initMethod.MakeChildWithScope(
                    evaluationType,
                    typeof(ExprCastNodeForgeConstEval),
                    exprSymbol,
                    codegenClassScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            compute.Block.MethodReturn(
                ExprCastNodeForgeNonConstEval.Codegen(forge, compute, exprSymbol, codegenClassScope));

            initMethod.Block.MethodReturn(LocalMethod(compute, Constant(null), ConstantTrue(), ConstantNull()));

            return codegenClassScope.AddDefaultFieldUnshared(true, evaluationType, LocalMethod(initMethod));
        }
    }
} // end of namespace