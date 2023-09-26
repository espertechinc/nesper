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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprOrNodeEval : ExprEvaluator
    {
        private readonly ExprEvaluator[] _evaluators;
        private readonly ExprOrNode _parent;

        public ExprOrNodeEval(
            ExprOrNode parent,
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
            bool? result = false;
            // At least one child must evaluate to true
            foreach (var child in _evaluators) {
                var evaluated = child.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                if (evaluated == null) {
                    result = null;
                }
                else {
                    if (true.Equals(evaluated)) {
                        return true;
                    }
                }
            }

            return result;
        }

        public static CodegenExpression Codegen(
            ExprOrNode parent,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(typeof(bool?), typeof(ExprOrNodeEval), codegenClassScope);

            var block = methodNode.Block
                .DeclareVar<bool?>("result", ConstantFalse());

            var count = -1;
            foreach (var child in parent.ChildNodes) {
                count++;
                var childType = child.Forge.EvaluationType;
                if (childType.CanNotBeNull()) {
                    block.IfCondition(
                            child.Forge.EvaluateCodegen(typeof(bool?), methodNode, exprSymbol, codegenClassScope))
                        .BlockReturn(ConstantTrue());
                }
                else {
                    var refname = "r" + count;
                    block.DeclareVar<bool?>(
                            refname,
                            child.Forge.EvaluateCodegen(typeof(bool?), methodNode, exprSymbol, codegenClassScope))
                        .IfCondition(EqualsNull(Ref(refname)))
                        .AssignRef("result", ConstantNull())
                        .IfElse()
                        .IfCondition(ExprDotName(Ref(refname), "Value"))
                        .BlockReturn(ConstantTrue());
                }
            }

            block.MethodReturn(Ref("result"));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace