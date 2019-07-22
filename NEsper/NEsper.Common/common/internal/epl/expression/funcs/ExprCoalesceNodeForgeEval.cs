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
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCoalesceNodeForgeEval : ExprEvaluator
    {
        private readonly ExprEvaluator[] evaluators;
        private readonly ExprCoalesceNodeForge forge;

        internal ExprCoalesceNodeForgeEval(
            ExprCoalesceNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            this.forge = forge;
            this.evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object value;

            // Look for the first non-null return value
            for (var i = 0; i < evaluators.Length; i++) {
                value = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (value != null) {
                    // Check if we need to coerce
                    if (forge.IsNumericCoercion[i]) {
                        value = TypeHelper.CoerceBoxed(value, forge.EvaluationType);
                    }

                    return value;
                }
            }

            return null;
        }

        public static CodegenExpression Codegen(
            ExprCoalesceNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (forge.EvaluationType == null) {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprCoalesceNodeForgeEval),
                codegenClassScope);

            var block = methodNode.Block;
            var num = 0;
            var doneWithReturn = false;
            foreach (var node in forge.ForgeRenderable.ChildNodes) {
                var reftype = node.Forge.EvaluationType;
                if (reftype != null) {
                    var refname = "r" + num;
                    block.DeclareVar(
                        reftype,
                        refname,
                        node.Forge.EvaluateCodegen(reftype, methodNode, exprSymbol, codegenClassScope));

                    if (reftype.IsPrimitive) {
                        if (!forge.IsNumericCoercion[num]) {
                            block.MethodReturn(Ref(refname));
                            doneWithReturn = true;
                        }
                        else {
                            var coercer = SimpleNumberCoercerFactory.GetCoercer(reftype, forge.EvaluationType);
                            block.MethodReturn(coercer.CoerceCodegen(Ref(refname), reftype));
                            doneWithReturn = true;
                        }

                        break;
                    }

                    var blockIf = block.IfCondition(NotEqualsNull(Ref(refname)));
                    if (!forge.IsNumericCoercion[num]) {
                        blockIf.BlockReturn(Ref(refname));
                    }
                    else {
                        blockIf.BlockReturn(
                            TypeHelper.CoerceNumberBoxedToBoxedCodegen(Ref(refname), reftype, forge.EvaluationType));
                    }
                }

                num++;
            }

            if (!doneWithReturn) {
                block.MethodReturn(ConstantNull());
            }

            return LocalMethod(methodNode);
        }
    }
} // end of namespace