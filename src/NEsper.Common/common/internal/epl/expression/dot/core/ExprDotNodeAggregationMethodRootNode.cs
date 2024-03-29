///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeAggregationMethodRootNode : ExprNodeBase,
        ExprEnumerationForge,
        ExprForge
    {
        private readonly ExprDotNodeAggregationMethodForge _forge;

        public ExprDotNodeAggregationMethodRootNode(ExprDotNodeAggregationMethodForge forge)
        {
            _forge = forge;
        }

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable(
                    (
                        writer,
                        parentPrecedence,
                        flags) => _forge.ToPrecedenceFreeEPL(writer, flags));
            }
        }

        public ExprNodeRenderable EnumForgeRenderable {
            get {
                return new ProxyExprNodeRenderable(
                    (
                        writer,
                        parentPrecedence,
                        flags) => _forge.ToPrecedenceFreeEPL(writer, flags));
            }
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return _forge.EventTypeCollection;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return _forge.EventTypeSingle;
        }

        public Type ComponentTypeCollection => _forge.ComponentTypeCollection;

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _forge.EvaluateGetROCollectionEventsCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _forge.EvaluateGetEventBeanCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _forge.EvaluateGetROCollectionScalarCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => throw NotAvailableCompileTime();

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _forge.EvaluateCodegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type EvaluationType => _forge.EvaluationType;

        public ExprEvaluator ExprEvaluator =>
            throw new UnsupportedOperationException("Evaluator not available at compile-time");

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // validation already done
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            _forge.ToPrecedenceFreeEPL(writer, flags);
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return false;
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            _forge.Accept(visitor);
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            base.Accept(visitor);
            _forge.Accept(visitor);
        }

        public override void AcceptChildnodes(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            _forge.AcceptChildnodes(visitor);
        }

        public static UnsupportedOperationException NotAvailableCompileTime()
        {
            return new UnsupportedOperationException("Evaluator not available at compile-time");
        }
    }
} // end of namespace