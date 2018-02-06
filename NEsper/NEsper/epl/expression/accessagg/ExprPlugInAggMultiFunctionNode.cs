///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.plugin;

namespace com.espertech.esper.epl.expression.accessagg
{
    /// <summary>
    /// Represents a custom aggregation function in an expresson tree.
    /// </summary>
    [Serializable]
    public class ExprPlugInAggMultiFunctionNode
        : ExprAggregateNodeBase
        , ExprEvaluatorEnumeration
        , ExprAggregateAccessMultiValueNode
        , ExprAggregationPlugInNodeMarker
    {
        private readonly PlugInAggregationMultiFunctionFactory _pluginAggregationMultiFunctionFactory;
        private readonly string _functionName;
        private readonly ConfigurationPlugInAggregationMultiFunction _config;
        [NonSerialized]
        private ExprPlugInAggMultiFunctionNodeFactory _factory;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        /// <param name="config">The configuration.</param>
        /// <param name="pluginAggregationMultiFunctionFactory">the factory</param>
        /// <param name="functionName">is the aggregation function name</param>
        public ExprPlugInAggMultiFunctionNode(bool distinct, ConfigurationPlugInAggregationMultiFunction config, PlugInAggregationMultiFunctionFactory pluginAggregationMultiFunctionFactory, string functionName)
            : base(distinct)
        {
            _pluginAggregationMultiFunctionFactory = pluginAggregationMultiFunctionFactory;
            _functionName = functionName;
            _config = config;
        }

        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            ValidatePositionals();
            return ValidateAggregationParamsWBinding(validationContext, null);
        }

        public AggregationMethodFactory ValidateAggregationParamsWBinding(ExprValidationContext validationContext, TableMetadataColumnAggregation tableAccessColumn)
        {
            // validate using the context provided by the 'outside' streams to determine parameters
            // at this time 'inside' expressions like 'window(intPrimitive)' are not handled
            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, ChildNodes, validationContext);
            return ValidateAggregationInternal(validationContext, tableAccessColumn);
        }

        private AggregationMethodFactory ValidateAggregationInternal(ExprValidationContext validationContext, TableMetadataColumnAggregation optionalTableColumn)
        {
            var ctx = new PlugInAggregationMultiFunctionValidationContext(
                _functionName,
                validationContext.StreamTypeService.EventTypes, PositionalParams,
                validationContext.StreamTypeService.EngineURIQualifier,
                validationContext.StatementName,
                validationContext, _config, optionalTableColumn, ChildNodes);

            var handlerPlugin = _pluginAggregationMultiFunctionFactory.ValidateGetHandler(ctx);
            _factory = new ExprPlugInAggMultiFunctionNodeFactory(this, handlerPlugin, validationContext.EngineImportService.AggregationFactoryFactory, validationContext.StatementExtensionSvcContext);
            return _factory;
        }

        public override string AggregationFunctionName
        {
            get { return _functionName; }
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            return AggregationResultFuture.GetCollectionOfEvents(Column, evaluateParams);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            var result = AggregationResultFuture.GetValue(Column, evaluateParams.ExprEvaluatorContext.AgentInstanceId, evaluateParams);
            if (result == null)
            {
                return null;
            }

            return result.Unwrap<object>();
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return _factory.EventTypeCollection;
        }

        public Type ComponentTypeCollection
        {
            get { return _factory.ComponentTypeCollection; }
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return _factory.EventTypeSingle;
        }

        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
            return AggregationResultFuture.GetEventBean(Column, evaluateParams);
        }

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return false;
        }

        protected override bool IsFilterExpressionAsLastParameter => false;

    }
} // end of namespace
