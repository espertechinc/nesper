///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprAndNodeEval : ExprEvaluator
    {
        private readonly ExprEvaluator[] _evaluators;
        private readonly ExprAndNodeImpl _parent;

        public ExprAndNodeEval(
            ExprAndNodeImpl parent,
            ExprEvaluator[] evaluators)
        {
            _parent = parent;
            _evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            bool? result = true;
            foreach (var child in _evaluators) {
                var evaluated = child.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                if (evaluated == null) {
                    result = null;
                }
                else if (false.Equals(evaluated)) {
                    return false;
                }
            }

            return result;
        }

        public static CodegenExpression Codegen(
            ExprAndNodeImpl parent,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(typeof(bool?), typeof(ExprAndNodeEval), codegenClassScope);
            var block = methodNode.Block
                .DeclareVar<bool?>("result", ConstantTrue());

            var count = -1;
            foreach (var child in parent.ChildNodes) {
                count++;
                var childType = child.Forge.EvaluationType;
                if (childType.CanNotBeNull()) {
                    block.IfCondition(
                            Not(child.Forge.EvaluateCodegen(typeof(bool?), methodNode, exprSymbol, codegenClassScope)))
                        .BlockReturn(ConstantFalse());
                }
                else {
                    var refname = "r" + count;
                    block.DeclareVar<bool?>(
                            refname,
                            child.Forge.EvaluateCodegen(typeof(bool?), methodNode, exprSymbol, codegenClassScope))
                        .IfCondition(EqualsNull(Ref(refname)))
                        .AssignRef("result", ConstantNull())
                        .IfElse()
                        .IfCondition(Not(ExprDotName(Ref(refname), "Value")))
                        .BlockReturn(ConstantFalse());
                }
            }

            block.MethodReturn(Ref("result"));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace