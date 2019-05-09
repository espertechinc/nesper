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
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents a NOT expression in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprNotNode : ExprNodeBase,
        ExprEvaluator,
        ExprForgeInstrumentable
    {
        [NonSerialized] private ExprEvaluator evaluator;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have a single child node
            if (this.ChildNodes.Length != 1)
            {
                throw new ExprValidationException("The NOT node requires exactly 1 child node");
            }

            ExprForge forge = this.ChildNodes[0].Forge;
            Type childType = forge.EvaluationType;
            if (!TypeHelper.IsBoolean(childType))
            {
                throw new ExprValidationException("Incorrect use of NOT clause, sub-expressions do not return boolean");
            }

            evaluator = forge.ExprEvaluator;
            return null;
        }

        public ExprEvaluator ExprEvaluator
        {
            get => this;
        }

        public override ExprForge Forge
        {
            get => this;
        }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprNode ForgeRenderable
        {
            get => this;
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            ExprForge child = this.ChildNodes[0].Forge;
            if (child.EvaluationType == typeof(bool))
            {
                Not(child.EvaluateCodegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope));
            }

            CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(bool?), typeof(ExprNotNode), codegenClassScope);
            methodNode.Block
                .DeclareVar(typeof(bool?), "b", child.EvaluateCodegen(typeof(bool?), methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("b")
                .MethodReturn(Not(@Ref("b")));
            return LocalMethod(methodNode);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(this.GetType(), this, "ExprNot", requiredType, codegenMethodScope, exprSymbol, codegenClassScope)
                .Build();
        }

        public Type EvaluationType
        {
            get => typeof(bool?);
        }

        public bool IsConstantResult
        {
            get => false;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluated = evaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (evaluated == null)
            {
                return null;
            }

            return false.Equals(evaluated);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("not ");
            this.ChildNodes[0].ToEPL(writer, Precedence);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get => ExprPrecedenceEnum.NEGATED;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprNotNode))
            {
                return false;
            }

            return true;
        }

        public ExprForgeConstantType ForgeConstantType
        {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace