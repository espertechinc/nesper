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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    ///     Represents the EXISTS(property) function in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprPropertyExistsNode : ExprNodeBase,
        ExprEvaluator,
        ExprForgeInstrumentable
    {
        private ExprIdentNode identNode;

        public override ExprForge Forge => this;

        public ExprNode ForgeRenderable => this;

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return identNode.ExprEvaluatorIdent.EvaluatePropertyExists(eventsPerStream, isNewData);
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => typeof(bool?);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(typeof(bool?), GetType(), codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(identNode.StreamId)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(
                    identNode.ExprEvaluatorIdent.Getter.EventBeanExistsCodegen(
                        Ref("@event"),
                        methodNode,
                        codegenClassScope));
            return LocalMethod(methodNode);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprPropExists",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Build();
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 1) {
                throw new ExprValidationException("Exists function node must have exactly 1 child node");
            }

            if (!(ChildNodes[0] is ExprIdentNode)) {
                throw new ExprValidationException(
                    "Exists function expects an property value expression as the child node");
            }

            identNode = (ExprIdentNode) ChildNodes[0];
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("exists(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            writer.Write(')');
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return node is ExprPropertyExistsNode;
        }
    }
} // end of namespace