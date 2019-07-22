///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.access.plugin;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.rettype;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.accessagg
{
    /// <summary>
    ///     Represents a custom aggregation function in an expresson tree.
    /// </summary>
    public class ExprPlugInMultiFunctionAggNode : ExprAggregateNodeBase,
        ExprEnumerationEval,
        ExprAggMultiFunctionNode,
        ExprPlugInAggNodeMarker
    {
        private readonly AggregationMultiFunctionForge aggregationMultiFunctionForge;
        private readonly ConfigurationCompilerPlugInAggregationMultiFunction config;
        private readonly string functionName;
        private AggregationForgeFactoryAccessPlugin factory;

        public ExprPlugInMultiFunctionAggNode(
            bool distinct,
            ConfigurationCompilerPlugInAggregationMultiFunction config,
            AggregationMultiFunctionForge aggregationMultiFunctionForge,
            string functionName)
            : base(distinct)
        {
            this.aggregationMultiFunctionForge = aggregationMultiFunctionForge;
            this.functionName = functionName;
            this.config = config;
        }

        public override string AggregationFunctionName => functionName;

        public override bool IsFilterExpressionAsLastParameter => false;

        public AggregationTableReadDesc ValidateAggregationTableRead(
            ExprValidationContext validationContext,
            TableMetadataColumnAggregation tableAccessColumn,
            TableMetaData table)
        {
            // child node validation
            ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, ChildNodes, validationContext);

            // portable validation
            var validation = tableAccessColumn.AggregationPortableValidation;
            if (!(validation is AggregationPortableValidationPluginMultiFunc)) {
                throw new ExprValidationException("Invalid aggregation column type");
            }

            // obtain handler
            var ctx = new AggregationMultiFunctionValidationContext(
                functionName,
                validationContext.StreamTypeService.EventTypes,
                positionalParams,
                validationContext.StatementName,
                validationContext,
                config,
                null,
                ChildNodes,
                optionalFilter);
            var handler = aggregationMultiFunctionForge.ValidateGetHandler(ctx);

            // set of reader
            var epType = handler.ReturnType;
            Type returnType = EPTypeHelper.GetNormalizedClass(epType);
            var forge = new AggregationTableAccessAggReaderForgePlugIn(
                returnType,
                (AggregationMultiFunctionTableReaderModeManaged) handler.TableReaderMode);
            EventType eventTypeCollection = EPTypeHelper.OptionalIsEventTypeColl(epType);
            EventType eventTypeSingle = EPTypeHelper.OptionalIsEventTypeSingle(epType);
            Type componentTypeCollection = EPTypeHelper.OptionalIsComponentTypeColl(epType);
            return new AggregationTableReadDesc(forge, eventTypeCollection, componentTypeCollection, eventTypeSingle);
        }

        public Type ComponentTypeCollection => factory.ComponentTypeCollection;

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return factory.EventTypeCollection;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return factory.EventTypeSingle;
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var future = GetAggFuture(codegenClassScope);
            return ExprDotMethod(
                future,
                "getCollectionOfEvents",
                Constant(column),
                exprSymbol.GetAddEPS(parent),
                exprSymbol.GetAddIsNewData(parent),
                exprSymbol.GetAddExprEvalCtx(parent));
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var future = GetAggFuture(codegenClassScope);
            return ExprDotMethod(
                future,
                "getCollectionScalar",
                Constant(column),
                exprSymbol.GetAddEPS(parent),
                exprSymbol.GetAddIsNewData(parent),
                exprSymbol.GetAddExprEvalCtx(parent));
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var future = GetAggFuture(codegenClassScope);
            return ExprDotMethod(
                future,
                "getEventBean",
                Constant(column),
                exprSymbol.GetAddEPS(parent),
                exprSymbol.GetAddIsNewData(parent),
                exprSymbol.GetAddExprEvalCtx(parent));
        }

        public ExprNodeRenderable EnumForgeRenderable => this;
        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            ValidatePositionals(validationContext);
            // validate using the context provided by the 'outside' streams to determine parameters
            // at this time 'inside' expressions like 'window(IntPrimitive)' are not handled
            ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, ChildNodes, validationContext);
            var ctx = new AggregationMultiFunctionValidationContext(
                functionName,
                validationContext.StreamTypeService.EventTypes,
                positionalParams,
                validationContext.StatementName,
                validationContext,
                config,
                null,
                ChildNodes,
                optionalFilter);
            var handlerPlugin = aggregationMultiFunctionForge.ValidateGetHandler(ctx);
            factory = new AggregationForgeFactoryAccessPlugin(this, handlerPlugin);
            return factory;
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return false;
        }
    }
} // end of namespace