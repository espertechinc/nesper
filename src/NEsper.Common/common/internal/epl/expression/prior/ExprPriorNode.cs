///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Xml.XPath;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
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
    ///     Represents the 'prior' prior event function in an expression node tree.
    /// </summary>
    public class ExprPriorNode : ExprNodeBase,
        ExprEvaluator,
        ExprForgeInstrumentable
    {
        private CodegenFieldName _priorStrategyFieldName;
        private Type _resultType;

        public int StreamNumber { get; private set; }

        public int ConstantIndexNumber { get; private set; }

        public ExprForge InnerForge { get; private set; }

        public override ExprForge Forge => this;

        public Type EvaluationType {
            get => _resultType;
            private set => _resultType = value;
        }

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public int RelativeIndex { get; set; } = -1;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public ExprEvaluator ExprEvaluator => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (_resultType.IsNullType()) {
                return ConstantNull();
            }
            
            var method = parent.MakeChild(_resultType, GetType(), codegenClassScope);
            var innerEval = CodegenLegoMethodExpression.CodegenExpression(InnerForge, method, codegenClassScope);
            var eps = exprSymbol.GetAddEPS(method);

            // see ExprPriorEvalStrategyBase
            CodegenExpression future = codegenClassScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                _priorStrategyFieldName,
                typeof(PriorEvalStrategy));

            var innerMethod = LocalMethod(
                innerEval,
                eps,
                exprSymbol.GetAddIsNewData(method),
                exprSymbol.GetAddExprEvalCtx(method));
            method.Block
                .DeclareVar<EventBean>("originalEvent", ArrayAtIndex(eps, Constant(StreamNumber)))
                .DeclareVar<EventBean>(
                    "substituteEvent",
                    ExprDotMethod(
                        future,
                        "GetSubstituteEvent",
                        Ref("originalEvent"),
                        exprSymbol.GetAddIsNewData(method),
                        Constant(ConstantIndexNumber),
                        Constant(RelativeIndex),
                        exprSymbol.GetAddExprEvalCtx(method),
                        Constant(StreamNumber)))
                .AssignArrayElement(eps, Constant(StreamNumber), Ref("substituteEvent"))
                .DeclareVar(EvaluationType, "evalResult", innerMethod)
                .AssignArrayElement(eps, Constant(StreamNumber), Ref("originalEvent"))
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

        public ExprNodeRenderable ExprForgeRenderable => this;

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
            if (constantNodeType.IsNotInt32()) {
                throw new ExprValidationException("Prior function requires an integer index parameter");
            }

            var value = constantNode.Forge.ExprEvaluator.Evaluate(null, false, null);
            ConstantIndexNumber = value.AsInt32();
            InnerForge = ChildNodes[1].Forge;

            // Determine stream number
            if (ChildNodes[1] is ExprIdentNode) {
                var identNode = (ExprIdentNode) ChildNodes[1];
                StreamNumber = identNode.StreamId;
                EvaluationType = InnerForge.EvaluationType.GetBoxedType();
            }
            else if (ChildNodes[1] is ExprStreamUnderlyingNode) {
                var streamNode = (ExprStreamUnderlyingNode) ChildNodes[1];
                StreamNumber = streamNode.StreamId;
                EvaluationType = InnerForge.EvaluationType.GetBoxedType();
            }
            else {
                throw new ExprValidationException("Previous function requires an event property as parameter");
            }

            // add request
            if (validationContext.ViewResourceDelegate == null) {
                throw new ExprValidationException("Prior function cannot be used in this context");
            }

            validationContext.ViewResourceDelegate.AddPriorNodeRequest(this);
            _priorStrategyFieldName = validationContext.MemberNames.PriorStrategy(StreamNumber);
            return null;
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

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprPriorNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace