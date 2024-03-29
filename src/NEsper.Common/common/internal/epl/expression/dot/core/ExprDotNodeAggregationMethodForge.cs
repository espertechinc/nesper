///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public abstract class ExprDotNodeAggregationMethodForge : ExprDotNodeForge
    {
        private readonly ExprDotNodeImpl _parent;
        private readonly string _aggregationMethodName;
        private readonly ExprNode[] _parameters;
        private readonly AggregationPortableValidation _validation;
        private AggregationMultiFunctionMethodDesc _methodDesc;

        protected abstract CodegenExpression EvaluateCodegen(
            string readerMethodName,
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope);

        protected abstract void ToEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags);

        protected abstract string TableName { get; }
        protected abstract string TableColumnName { get; }

        public ExprDotNodeAggregationMethodForge(
            ExprDotNodeImpl parent,
            string aggregationMethodName,
            ExprNode[] parameters,
            AggregationPortableValidation validation)
        {
            _parent = parent;
            _aggregationMethodName = aggregationMethodName;
            _parameters = parameters;
            _validation = validation;
        }

        public void Validate(ExprValidationContext validationContext)
        {
            _methodDesc = _validation.ValidateAggregationMethod(validationContext, _aggregationMethodName, _parameters);
        }

        public override Type EvaluationType => _methodDesc.Reader.ResultType;

        public override bool IsReturnsConstantResult => false;

        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector => null;

        public override int? StreamNumReferenced => null;

        public override string RootPropertyName => null;

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return EvaluateCodegen("GetValue", requiredType, parent, symbols, classScope);
        }

        public override ExprEvaluator ExprEvaluator =>
            throw ExprDotNodeAggregationMethodRootNode.NotAvailableCompileTime();

        public override ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable(
                    (
                        writer,
                        parentPrecedence,
                        flags) => ToPrecedenceFreeEPL(writer, flags));
            }
        }

        protected CodegenExpressionInstanceField GetReader(CodegenClassScope classScope)
        {
            return classScope.AddOrGetDefaultFieldSharable(
                new AggregationMethodCodegenField(_methodDesc.Reader, classScope, GetType()));
        }

        public EventType EventTypeCollection => _methodDesc.EventTypeCollection;

        public EventType EventTypeSingle => _methodDesc.EventTypeSingle;

        public Type ComponentTypeCollection => _methodDesc.ComponentTypeCollection;

        public override CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprTableSubpropAccessor",
                    requiredType,
                    parent,
                    symbols,
                    classScope)
                .Qparam(Constant(TableName)) // table name
                .Qparam(Constant(TableColumnName)) // subprop name
                .Qparam(Constant(_aggregationMethodName)) // agg expression
                .Build();
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return EvaluateCodegen(
                "GetValueCollectionEvents",
                typeof(ICollection<EventBean>),
                parent,
                symbols,
                classScope);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return EvaluateCodegen(
                "GetValueCollectionScalar",
                typeof(ICollection<object>),
                parent,
                symbols,
                classScope);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return EvaluateCodegen("GetValueEventBean", typeof(EventBean), parent, symbols, classScope);
        }

        public void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ToEPL(writer, flags);
            writer.Write(".");
            writer.Write(_aggregationMethodName);
            writer.Write("(");
            ExprNodeUtilityPrint.ToExpressionStringParameterList(_parameters, writer);
            writer.Write(")");
        }

        public void Accept(ExprNodeVisitor visitor)
        {
            foreach (var parameter in _parameters) {
                parameter.Accept(visitor);
            }
        }

        public void Accept(ExprNodeVisitorWithParent visitor)
        {
            foreach (var parameter in _parameters) {
                parameter.Accept(visitor);
            }
        }

        public void AcceptChildnodes(ExprNodeVisitorWithParent visitor)
        {
            foreach (var parameter in _parameters) {
                parameter.AcceptChildnodes(visitor, _parent);
            }
        }
    }
} // end of namespace