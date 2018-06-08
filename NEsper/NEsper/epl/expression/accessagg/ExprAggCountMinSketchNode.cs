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
using com.espertech.esper.client.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.approx;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.accessagg
{
    /// <summary>
    /// Represents the Count-min sketch aggregate function.
    /// </summary>
    [Serializable]
    public class ExprAggCountMinSketchNode
        : ExprAggregateNodeBase
        , ExprAggregateAccessMultiValueNode
    {
        private const double DEFAULT_EPS_OF_TOTAL_COUNT = 0.0001;
        private const double DEFAULT_CONFIDENCE = 0.99;
        private const int DEFAULT_SEED = 1234567;

        private static readonly CountMinSketchAgentStringUTF16 DEFAULT_AGENT = new CountMinSketchAgentStringUTF16();

        private const string MSG_NAME = "Count-min-sketch";
        private const string NAME_EPS_OF_TOTAL_COUNT = "epsOfTotalCount";
        private const string NAME_CONFIDENCE = "confidence";
        private const string NAME_SEED = "seed";
        private const string NAME_TOPK = "topk";
        private const string NAME_AGENT = "agent";

        private readonly CountMinSketchAggType _aggType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        /// <param name="aggType">Type of the aggregate.</param>
        public ExprAggCountMinSketchNode(bool distinct, CountMinSketchAggType aggType)
            : base(distinct)
        {
            _aggType = aggType;
        }

        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            return ValidateAggregationInternal(validationContext, null);
        }

        public AggregationMethodFactory ValidateAggregationParamsWBinding(ExprValidationContext context, TableMetadataColumnAggregation tableAccessColumn)
        {
            return ValidateAggregationInternal(context, tableAccessColumn);
        }

        public override string AggregationFunctionName => _aggType.GetFuncName();

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return false;
        }

        public CountMinSketchAggType AggType => _aggType;

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return null;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            return null;
        }

        public Type ComponentTypeCollection => null;

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            return null;
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
            return null;
        }

        protected override bool IsExprTextWildcardWhenNoParams => false;

        private AggregationMethodFactory ValidateAggregationInternal(ExprValidationContext context, TableMetadataColumnAggregation tableAccessColumn)
        {
            if (IsDistinct)
            {
                throw new ExprValidationException(MessagePrefix + "is not supported with distinct");
            }

            // for declaration, validate the specification and return the state factory
            if (_aggType == CountMinSketchAggType.STATE)
            {
                if (context.ExprEvaluatorContext.StatementType != StatementType.CREATE_TABLE)
                {
                    throw new ExprValidationException(MessagePrefix + "can only be used in create-table statements");
                }
                var specification = ValidateSpecification(context);
                var stateFactory = context.EngineImportService.AggregationFactoryFactory.MakeCountMinSketch(context.StatementExtensionSvcContext, this, specification);
                return new ExprAggCountMinSketchNodeFactoryState(stateFactory);
            }

            var positionalParams = PositionalParams;

            // validate number of parameters
            if (_aggType == CountMinSketchAggType.ADD || _aggType == CountMinSketchAggType.FREQ)
            {
                if (positionalParams.Length == 0 || positionalParams.Length > 1)
                {
                    throw new ExprValidationException(MessagePrefix + "requires a single parameter expression");
                }
            }
            else
            {
                if (positionalParams.Length != 0)
                {
                    throw new ExprValidationException(MessagePrefix + "requires a no parameter expressions");
                }
            }

            // validate into-table and table-access
            if (_aggType == CountMinSketchAggType.ADD)
            {
                if (context.IntoTableName == null)
                {
                    throw new ExprValidationException(MessagePrefix + "can only be used with into-table");
                }
            }
            else
            {
                if (tableAccessColumn == null)
                {
                    throw new ExprValidationException(MessagePrefix + "requires the use of a table-access expression");
                }
                ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, ChildNodes, context);
            }

            // obtain evaluator
            ExprEvaluator addOrFrequencyEvaluator = null;
            if (_aggType == CountMinSketchAggType.ADD || _aggType == CountMinSketchAggType.FREQ)
            {
                addOrFrequencyEvaluator = ChildNodes[0].ExprEvaluator;
            }

            return new ExprAggCountMinSketchNodeFactoryUse(this, addOrFrequencyEvaluator);
        }

        private CountMinSketchSpec ValidateSpecification(ExprValidationContext exprValidationContext)
        {
            // default specification
            var spec = new CountMinSketchSpec(new CountMinSketchSpecHashes(DEFAULT_EPS_OF_TOTAL_COUNT, DEFAULT_CONFIDENCE, DEFAULT_SEED), null, DEFAULT_AGENT);

            // no parameters
            if (ChildNodes.Count == 0)
            {
                return spec;
            }

            // check expected parameter type: a json object
            if (ChildNodes.Count > 1 || !(ChildNodes[0] is ExprConstantNode))
            {
                throw DeclaredWrongParameterExpr;
            }
            var constantNode = (ExprConstantNode)ChildNodes[0];
            var value = constantNode.GetConstantValue(exprValidationContext.ExprEvaluatorContext);
            if (!(value is IDictionary<string, object>))
            {
                throw DeclaredWrongParameterExpr;
            }

            // define what to populate
            var descriptors = new PopulateFieldWValueDescriptor[] {
                new PopulateFieldWValueDescriptor(NAME_EPS_OF_TOTAL_COUNT, typeof(double), spec.HashesSpec.GetType(), vv => {
                        if (vv != null) {spec.HashesSpec.EpsOfTotalCount = (double) vv;}
                    }, true),
                new PopulateFieldWValueDescriptor(NAME_CONFIDENCE, typeof(double), spec.HashesSpec.GetType(), vv => {
                        if (vv != null) {spec.HashesSpec.Confidence = (double) vv;}
                    }, true),
                new PopulateFieldWValueDescriptor(NAME_SEED, typeof(int), spec.HashesSpec.GetType(), vv =>  {
                        if (vv != null) {spec.HashesSpec.Seed = (int) vv;}
                    }, true),
                new PopulateFieldWValueDescriptor(NAME_TOPK, typeof(int), spec.GetType(), vv => {
                        if (vv != null) {spec.TopkSpec = (int) vv;}
                    }, true),
                new PopulateFieldWValueDescriptor(NAME_AGENT, typeof(string), spec.GetType(), vv =>  {
                        if (vv != null) {
                            CountMinSketchAgent transform;
                            try {
                                var transformClass = exprValidationContext.EngineImportService.ResolveType((string)vv, false);
                                transform = TypeHelper.Instantiate<CountMinSketchAgent>(transformClass);
                            }
                            catch (Exception e) {
                                throw new ExprValidationException("Failed to instantiate agent provider: " + e.Message, e);
                            }
                            spec.Agent = transform;
                        }
                    }, true)
            };

            // populate from json, validates incorrect names, coerces types, instantiates transform
            PopulateUtil.PopulateSpecCheckParameters(descriptors, (IDictionary<String, object>)value, spec, ExprNodeOrigin.AGGPARAM, exprValidationContext);

            return spec;
        }

        public ExprValidationException DeclaredWrongParameterExpr => new ExprValidationException(
            MessagePrefix + " expects either no parameter or a single json parameter object");

        protected override bool IsFilterExpressionAsLastParameter => false;

        private string MessagePrefix => MSG_NAME + " aggregation function '" + _aggType.GetFuncName() + "' ";
    }
}
