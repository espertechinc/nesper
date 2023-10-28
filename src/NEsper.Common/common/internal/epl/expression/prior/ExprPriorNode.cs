///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.prior
{
    /// <summary>
    /// Represents the 'prior' prior event function in an expression node tree.
    /// </summary>
    public class ExprPriorNode : ExprNodeBase,
        ExprEvaluator,
        ExprForgeInstrumentable
    {
        private Type resultType;
        private int streamNumber;
        private int constantIndexNumber;
        private ExprForge innerForge;
        private int relativeIndex = -1;
        private CodegenFieldName priorStrategyFieldName;

        public ExprEvaluator ExprEvaluator => this;

        public int StreamNumber => streamNumber;

        public int ConstantIndexNumber => constantIndexNumber;

        public ExprForge InnerForge => innerForge;

        public override ExprForge Forge => this;

        public Type EvaluationType => resultType;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 2) {
                throw new ExprValidationException("Prior node must have 2 parameters");
            }

            if (!ChildNodes[0].Forge.ForgeConstantType.IsCompileTimeConstant) {
                throw new ExprValidationException(
                    "Prior function requires a constant-value integer-typed index expression as the first parameter");
            }

            // Child identifier nodes receive optional event
            ExprNodeUtilityMake.SetChildIdentNodesOptionalEvent(this);

            var constantNode = ChildNodes[0];
            var constantNodeType = constantNode.Forge.EvaluationType;
            if (!constantNodeType.IsTypeInteger()) {
                throw new ExprValidationException("Prior function requires an integer index parameter");
            }

            var value = constantNode.Forge.ExprEvaluator.Evaluate(null, false, null);
            constantIndexNumber = value.AsInt32();
            innerForge = ChildNodes[1].Forge;

            // Determine stream number
            if (ChildNodes[1] is ExprIdentNode) {
                var identNode = (ExprIdentNode)ChildNodes[1];
                streamNumber = identNode.StreamId;
                resultType = innerForge.EvaluationType.GetBoxedType();
            }
            else if (ChildNodes[1] is ExprStreamUnderlyingNode) {
                var streamNode = (ExprStreamUnderlyingNode)ChildNodes[1];
                streamNumber = streamNode.StreamId;
                resultType = innerForge.EvaluationType.GetBoxedType();
            }
            else {
                throw new ExprValidationException("Previous function requires an event property as parameter");
            }

            // add request
            if (validationContext.ViewResourceDelegate == null) {
                throw new ExprValidationException("Prior function cannot be used in this context");
            }

            validationContext.ViewResourceDelegate.AddPriorNodeRequest(this);
            priorStrategyFieldName = validationContext.MemberNames.PriorStrategy(streamNumber);
            return null;
        }

        public bool IsConstantResult => false;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (resultType == null) {
                return ConstantNull();
            }

            var method = parent.MakeChild(resultType, GetType(), codegenClassScope);

            var innerEval =
                CodegenLegoMethodExpression.CodegenExpression(innerForge, method, codegenClassScope);
            var eps = exprSymbol.GetAddEPS(method);

            // see ExprPriorEvalStrategyBase
            var future = codegenClassScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                priorStrategyFieldName,
                typeof(PriorEvalStrategy));
            method.Block
                .DeclareVar<EventBean>("originalEvent", ArrayAtIndex(eps, Constant(streamNumber)))
                .DeclareVar<EventBean>("substituteEvent",
                    ExprDotMethod(
                        future,
                        "GetSubstituteEvent",
                        Ref("originalEvent"),
                        exprSymbol.GetAddIsNewData(method),
                        Constant(constantIndexNumber),
                        Constant(relativeIndex),
                        exprSymbol.GetAddExprEvalCtx(method),
                        Constant(streamNumber)))
                .AssignArrayElement(eps, Constant(streamNumber), Ref("substituteEvent"))
                .DeclareVar(
                    resultType,
                    "evalResult",
                    LocalMethod(
                        innerEval,
                        eps,
                        exprSymbol.GetAddIsNewData(method),
                        exprSymbol.GetAddExprEvalCtx(method)))
                .AssignArrayElement(eps, Constant(streamNumber), Ref("originalEvent"))
                .MethodReturn(Ref("evalResult"));

            return LocalMethod(method);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprPrior",
                requiredType,
                parent,
                exprSymbol,
                codegenClassScope).Build();
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("prior(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            writer.Write(',');
            ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public ExprNode ForgeRenderable => this;

        public ExprNodeRenderable ExprForgeRenderable => ForgeRenderable;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprPriorNode)) {
                return false;
            }

            return true;
        }

        public int RelativeIndex {
            set => relativeIndex = value;
        }
    }
} // end of namespace