///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.prev
{
    /// <summary>
    ///     Represents the 'prev' previous event function in match-recognize "define" item.
    /// </summary>
    public class ExprPreviousMatchRecognizeNode : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        private int assignedIndex;
        private int? constantIndexNumber;
        private CodegenFieldName previousStrategyFieldName;
        private int streamNumber;

        public override ExprForge Forge => this;

        public ExprNode ForgeRenderable => this;

        /// <summary>
        ///     Returns the index number.
        /// </summary>
        /// <value>index number</value>
        public int ConstantIndexNumber {
            get {
                if (constantIndexNumber == null) {
                    var constantNode = ChildNodes[1];
                    var value = constantNode.Forge.ExprEvaluator.Evaluate(null, false, null);
                    constantIndexNumber = value.AsInt();
                }

                return constantIndexNumber.Value;
            }
        }

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        /// <summary>
        ///     Sets the index to use when accessing via getter
        /// </summary>
        /// <value>index</value>
        public int AssignedIndex {
            set => assignedIndex = value;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType { get; private set; }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(EvaluationType, GetType(), classScope);
            var eps = symbols.GetAddEPS(method);

            var strategy = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                previousStrategyFieldName,
                typeof(RowRecogPreviousStrategy));

            var innerEval = CodegenLegoMethodExpression.CodegenExpression(ChildNodes[0].Forge, method, classScope);

            method.Block
                .DeclareVar<RowRecogStateRandomAccess>(
                    "access",
                    ExprDotMethod(strategy, "GetAccess", symbols.GetAddExprEvalCtx(method)))
                .DeclareVar<EventBean>(
                    "substituteEvent",
                    ExprDotMethod(Ref("access"), "GetPreviousEvent", Constant(assignedIndex)))
                .IfRefNullReturnNull("substituteEvent")
                .DeclareVar<EventBean>("originalEvent", ArrayAtIndex(eps, Constant(streamNumber)))
                .AssignArrayElement(eps, Constant(streamNumber), Ref("substituteEvent"))
                .DeclareVar(
                    EvaluationType,
                    "evalResult",
                    LocalMethod(innerEval, eps, symbols.GetAddIsNewData(method), symbols.GetAddExprEvalCtx(method)))
                .AssignArrayElement(eps, Constant(streamNumber), Ref("originalEvent"))
                .MethodReturn(Ref("evalResult"));

            return LocalMethod(method);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 2) {
                throw new ExprValidationException("Match-Recognize Previous expression must have 2 parameters");
            }

            if (!(ChildNodes[0] is ExprIdentNode)) {
                throw new ExprValidationException(
                    "Match-Recognize Previous expression requires an property identifier as the first parameter");
            }

            if (!ChildNodes[1].Forge.ForgeConstantType.IsCompileTimeConstant ||
                !ChildNodes[1].Forge.EvaluationType.IsNumericNonFP()) {
                throw new ExprValidationException(
                    "Match-Recognize Previous expression requires an integer index parameter or expression as the second parameter");
            }

            var constantNode = ChildNodes[1];
            var value = constantNode.Forge.ExprEvaluator.Evaluate(null, false, null);

            if (!value.IsNumber()) {
                throw new ExprValidationException(
                    "Match-Recognize Previous expression requires an integer index parameter or expression as the second parameter");
            }

            constantIndexNumber = value.AsInt();

            // Determine stream number
            var identNode = (ExprIdentNode) ChildNodes[0];
            streamNumber = identNode.StreamId;
            var forge = ChildNodes[0].Forge;
            EvaluationType = forge.EvaluationType;
            previousStrategyFieldName = validationContext.MemberNames.PreviousMatchrecognizeStrategy();

            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("prev(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(',');
            ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(')');
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprPreviousMatchRecognizeNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace