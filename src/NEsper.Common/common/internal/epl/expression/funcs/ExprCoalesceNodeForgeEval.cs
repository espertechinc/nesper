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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCoalesceNodeForgeEval : ExprEvaluator
    {
        private readonly ExprCoalesceNodeForge _forge;
        private readonly ExprEvaluator[] _evaluators;

        internal ExprCoalesceNodeForgeEval(
            ExprCoalesceNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            this._forge = forge;
            this._evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object value;

            // Look for the first non-null return value
            for (var i = 0; i < _evaluators.Length; i++) {
                value = _evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (value != null) {
                    // Check if we need to coerce
                    if (_forge.IsNumericCoercion[i]) {
                        value = TypeHelper.CoerceBoxed(value, _forge.EvaluationType);
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
            if (forge.EvaluationType == null || forge.EvaluationType == null) {
                return ConstantNull();
            }

            var evaluationClass = forge.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprCoalesceNodeForgeEval),
                codegenClassScope);

            var block = methodNode.Block;
            var num = 0;
            var doneWithReturn = false;
            foreach (var node in forge.ForgeRenderable.ChildNodes) {
                var evaltype = node.Forge.EvaluationType;
                if (evaltype != null) {
                    var classtype = evaltype;
                    var refname = "r" + num;
                    block.DeclareVar(
                        classtype,
                        refname,
                        node.Forge.EvaluateCodegen(classtype, methodNode, exprSymbol, codegenClassScope));

                    if (classtype.IsPrimitive) {
                        if (!forge.IsNumericCoercion[num]) {
                            block.MethodReturn(Ref(refname));
                            doneWithReturn = true;
                        }
                        else {
                            var coercer = SimpleNumberCoercerFactory.GetCoercer(classtype, evaluationClass);
                            block.MethodReturn(coercer.CoerceCodegen(Ref(refname), classtype));
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
                            TypeHelper.CoerceNumberBoxedToBoxedCodegen(
                                Ref(refname),
                                classtype,
                                forge.EvaluationType));
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