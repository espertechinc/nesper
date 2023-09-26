///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.ops.ExprLikeNodeForgeConstEval;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprLikeNodeFormNonconstEval : ExprEvaluator
    {
        private readonly ExprLikeNodeForgeNonconst _form;
        private readonly ExprEvaluator _lhsEval;
        private readonly ExprEvaluator _optionalEscapeEval;
        private readonly ExprEvaluator _patternEval;

        public ExprLikeNodeFormNonconstEval(
            ExprLikeNodeForgeNonconst forge,
            ExprEvaluator lhsEval,
            ExprEvaluator patternEval,
            ExprEvaluator optionalEscapeEval)
        {
            _form = forge;
            _lhsEval = lhsEval;
            _patternEval = patternEval;
            _optionalEscapeEval = optionalEscapeEval;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var pattern = (string)_patternEval.Evaluate(eventsPerStream, isNewData, context);
            if (pattern == null) {
                return null;
            }

            var es = '\\';
            if (_optionalEscapeEval != null) {
                var escapeString = (string)_optionalEscapeEval.Evaluate(eventsPerStream, isNewData, context);
                if (!string.IsNullOrEmpty(escapeString)) {
                    es = escapeString[0];
                }
            }

            var likeUtil = new LikeUtil(pattern, es, false);

            var value = _lhsEval.Evaluate(eventsPerStream, isNewData, context);
            if (value == null) {
                return null;
            }

            if (_form.IsNumericValue) {
                value = value.ToString();
            }

            var result = _form.ForgeRenderable.IsNot ^ likeUtil.CompareTo((string)value);

            return result;
        }

        public static CodegenMethod Codegen(
            ExprLikeNodeForgeNonconst forge,
            ExprNode lhs,
            ExprNode pattern,
            ExprNode optionalEscape,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprLikeNodeFormNonconstEval),
                codegenClassScope);
            var blockMethod = methodNode.Block
                .DeclareVar<string>(
                    "pattern",
                    pattern.Forge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("pattern");

            // initial like-setup
            blockMethod.DeclareVar<char>("es", Constant('\\'));
            if (optionalEscape != null) {
                blockMethod.DeclareVar<string>(
                    "escapeString",
                    optionalEscape.Forge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope));
                blockMethod.IfCondition(
                        And(
                            NotEqualsNull(Ref("escapeString")),
                            Not(
                                StaticMethod<string>("IsNullOrEmpty", Ref("escapeString")))))
                    .AssignRef("es", ArrayAtIndex(Ref("escapeString"), Constant(0)));
            }

            blockMethod.DeclareVar<LikeUtil>(
                "likeUtil",
                NewInstance<LikeUtil>(Ref("pattern"), Ref("es"), Constant(false)));

            if (!forge.IsNumericValue) {
                blockMethod.DeclareVar<string>(
                        "value",
                        lhs.Forge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetLikeCode(forge, Ref("likeUtil"), Ref("value")));
            }
            else {
                blockMethod.DeclareVar<object>(
                        "value",
                        lhs.Forge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetLikeCode(forge, Ref("likeUtil"), ExprDotMethod(Ref("value"), "ToString")));
            }

            return methodNode;
        }
    }
} // end of namespace