///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.access.countminsketch;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.accessagg
{
    /// <summary>
    /// Represents the Count-min sketch aggregate function.
    /// </summary>
    public class ExprAggMultiFunctionCountMinSketchNode : ExprAggregateNodeBase,
        ExprAggMultiFunctionNode,
        ExprEnumerationEval
    {
        private const double DEFAULT_EPS_OF_TOTAL_COUNT = 0.0001;
        private const double DEFAULT_CONFIDENCE = 0.99;
        private const int DEFAULT_SEED = 1234567;

        private static readonly CountMinSketchAgentStringUTF16Forge DEFAULT_AGENT =
            new CountMinSketchAgentStringUTF16Forge();

        public const string MSG_NAME = "Count-min-sketch";
        private const string NAME_EPS_OF_TOTAL_COUNT = "epsOfTotalCount";
        private const string NAME_CONFIDENCE = "confidence";
        private const string NAME_SEED = "seed";
        private const string NAME_TOPK = "topk";
        private const string NAME_AGENT = "agent";
        private readonly CountMinSketchAggType aggType;
        private AggregationForgeFactory forgeFactory;

        public ExprAggMultiFunctionCountMinSketchNode(
            bool distinct,
            CountMinSketchAggType aggType) : base(distinct)
        {
            this.aggType = aggType;
        }

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (IsDistinct) {
                throw new ExprValidationException(MessagePrefix + "is not supported with distinct");
            }

            // for declaration, validate the specification and return the state factory
            if (aggType == CountMinSketchAggType.STATE) {
                if (validationContext.StatementRawInfo.StatementType != StatementType.CREATE_TABLE) {
                    throw new ExprValidationException(MessagePrefix + "can only be used in create-table statements");
                }

                var specification = ValidateSpecification(validationContext);
                var stateFactory = new AggregationStateCountMinSketchForge(this, specification);
                forgeFactory = new AggregationForgeFactoryAccessCountMinSketchState(this, stateFactory);
                return forgeFactory;
            }

            if (aggType != CountMinSketchAggType.ADD) {
                // other methods are only used with table-access expressions
                throw new ExprValidationException(MessagePrefix + "requires the use of a table-access expression");
            }

            if (validationContext.StatementRawInfo.IntoTableName == null) {
                throw new ExprValidationException(MessagePrefix + "can only be used with into-table");
            }

            if (positionalParams.Length == 0 || positionalParams.Length > 1) {
                throw new ExprValidationException(MessagePrefix + "requires a single parameter expression");
            }

            ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, ChildNodes, validationContext);
            // obtain evaluator
            ExprForge addOrFrequencyEvaluator = null;
            Type addOrFrequencyEvaluatorReturnType = null;
            if (aggType == CountMinSketchAggType.ADD) {
                addOrFrequencyEvaluator = ChildNodes[0].Forge;
                addOrFrequencyEvaluatorReturnType = addOrFrequencyEvaluator.EvaluationType;
                if (addOrFrequencyEvaluatorReturnType == null) {
                    throw new ExprValidationException("Invalid null-type parameter");
                }
            }

            forgeFactory = new AggregationForgeFactoryAccessCountMinSketchAdd(
                this,
                addOrFrequencyEvaluator,
                addOrFrequencyEvaluatorReturnType);
            return forgeFactory;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;
        public override string AggregationFunctionName => aggType.GetFuncName();

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return false;
        }

        public CountMinSketchAggType AggType => aggType;

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public Type ComponentTypeCollection => null;

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return null;
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        protected override bool IsExprTextWildcardWhenNoParams => false;

        private CountMinSketchSpecForge ValidateSpecification(ExprValidationContext exprValidationContext)
        {
            // default specification
            var hashes = new CountMinSketchSpecHashes(DEFAULT_EPS_OF_TOTAL_COUNT, DEFAULT_CONFIDENCE, DEFAULT_SEED);
            var spec = new CountMinSketchSpecForge(hashes, null, DEFAULT_AGENT);
            // no parameters
            if (ChildNodes.Length == 0) {
                return spec;
            }

            // check expected parameter type: a json object
            if (ChildNodes.Length > 1 || !(ChildNodes[0] is ExprConstantNode)) {
                throw DeclaredWrongParameterExpr;
            }

            var constantNode = (ExprConstantNode)ChildNodes[0];
            var value = constantNode.ConstantValue;
            if (!(value is IDictionary<string, object>)) {
                throw DeclaredWrongParameterExpr;
            }

            // define what to populate
            var descriptors = new PopulateFieldWValueDescriptor[] {
                new PopulateFieldWValueDescriptor(
                    NAME_EPS_OF_TOTAL_COUNT,
                    typeof(double?),
                    typeof(CountMinSketchSpecHashes),
                    value => {
                        if (value != null) {
                            spec.HashesSpec.EpsOfTotalCount = value.AsDouble();
                        }
                    },
                    true),
                new PopulateFieldWValueDescriptor(
                    NAME_CONFIDENCE,
                    typeof(double?),
                    typeof(CountMinSketchSpecHashes),
                    value => {
                        if (value != null) {
                            spec.HashesSpec.Confidence = value.AsDouble();
                        }
                    },
                    true),
                new PopulateFieldWValueDescriptor(
                    NAME_SEED,
                    typeof(int?),
                    typeof(CountMinSketchSpecHashes),
                    value => {
                        if (value != null) {
                            spec.HashesSpec.Seed = value.AsInt32();
                        }
                    },
                    true),
                new PopulateFieldWValueDescriptor(
                    NAME_TOPK,
                    typeof(int?),
                    typeof(CountMinSketchSpecForge),
                    value => {
                        if (value != null) {
                            spec.TopkSpec = value.AsInt32();
                        }
                    },
                    true),
                new PopulateFieldWValueDescriptor(
                    NAME_AGENT,
                    typeof(string),
                    typeof(CountMinSketchSpecForge),
                    value => {
                        if (value != null) {
                            CountMinSketchAgentForge transform;
                            try {
                                var transformClass = exprValidationContext.ImportService.ResolveType(
                                    (string)value,
                                    false,
                                    ExtensionClassEmpty.INSTANCE);
                                transform = TypeHelper.Instantiate<CountMinSketchAgentForge>(transformClass);
                            }
                            catch (Exception e) {
                                throw new ExprValidationException(
                                    "Failed to instantiate agent provider: " + e.Message,
                                    e);
                            }

                            spec.Agent = transform;
                        }
                    },
                    true)
            };
            // populate from json, validates incorrect names, coerces types, instantiates transform
            PopulateUtil.PopulateSpecCheckParameters(
                descriptors,
                (IDictionary<string, object>)value,
                spec,
                ExprNodeOrigin.AGGPARAM,
                exprValidationContext);
            return spec;
        }

        public override bool IsFilterExpressionAsLastParameter => false;

        public ExprValidationException DeclaredWrongParameterExpr => new ExprValidationException(
            MessagePrefix + " expects either no parameter or a single json parameter object");

        public AggregationForgeFactory AggregationForgeFactory => forgeFactory;

        public string MessagePrefix => MSG_NAME + " aggregation function '" + aggType.GetFuncName() + "' ";
    }
} // end of namespace