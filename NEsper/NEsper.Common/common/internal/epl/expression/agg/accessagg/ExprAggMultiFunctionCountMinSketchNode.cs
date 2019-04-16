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
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.accessagg
{
    /// <summary>
    ///     Represents the Count-min sketch aggregate function.
    /// </summary>
    public class ExprAggMultiFunctionCountMinSketchNode : ExprAggregateNodeBase,
        ExprAggMultiFunctionNode,
        ExprEnumerationEval
    {
        private const double DEFAULT_EPS_OF_TOTAL_COUNT = 0.0001;
        private const double DEFAULT_CONFIDENCE = 0.99;
        private const int DEFAULT_SEED = 1234567;

        private const string MSG_NAME = "Count-min-sketch";
        private const string NAME_EPS_OF_TOTAL_COUNT = "epsOfTotalCount";
        private const string NAME_CONFIDENCE = "confidence";
        private const string NAME_SEED = "seed";
        private const string NAME_TOPK = "topk";
        private const string NAME_AGENT = "agent";

        private static readonly CountMinSketchAgentStringUTF16Forge DEFAULT_AGENT =
            new CountMinSketchAgentStringUTF16Forge();

        public ExprAggMultiFunctionCountMinSketchNode(
            bool distinct,
            CountMinSketchAggType aggType)
            : base(distinct)
        {
            AggType = aggType;
        }

        public override string AggregationFunctionName => AggType.GetFuncName();

        public CountMinSketchAggType AggType { get; }

        internal override bool IsExprTextWildcardWhenNoParams => false;

        public ExprValidationException DeclaredWrongParameterExpr => new ExprValidationException(
            MessagePrefix + " expects either no parameter or a single json parameter object");

        internal override bool IsFilterExpressionAsLastParameter => false;

        private string MessagePrefix => MSG_NAME + " aggregation function '" + AggType.GetFuncName() + "' ";

        public AggregationTableReadDesc ValidateAggregationTableRead(
            ExprValidationContext context,
            TableMetadataColumnAggregation tableAccessColumn,
            TableMetaData table)
        {
            if (AggType == CountMinSketchAggType.STATE || AggType == CountMinSketchAggType.ADD) {
                throw new ExprValidationException(MessagePrefix + "cannot not be used for table access");
            }

            if (!(tableAccessColumn.AggregationPortableValidation is AggregationPortableValidationCountMinSketch)) {
                throw new ExprValidationException(MessagePrefix + "can only be used with count-min-sketch");
            }

            AggregationTableAccessAggReaderForge forge;
            if (AggType == CountMinSketchAggType.FREQ) {
                if (positionalParams.Length == 0 || positionalParams.Length > 1) {
                    throw new ExprValidationException(MessagePrefix + "requires a single parameter expression");
                }

                ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, ChildNodes, context);
                var frequencyEval = ChildNodes[0];
                forge = new AgregationTAAReaderCountMinSketchFreqForge(frequencyEval);
            }
            else {
                if (positionalParams.Length != 0) {
                    throw new ExprValidationException(MessagePrefix + "requires a no parameter expressions");
                }

                forge = new AgregationTAAReaderCountMinSketchTopKForge();
            }

            return new AggregationTableReadDesc(forge, null, null, null);
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public Type ComponentTypeCollection => null;

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

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
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

        internal override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (IsDistinct) {
                throw new ExprValidationException(MessagePrefix + "is not supported with distinct");
            }

            // for declaration, validate the specification and return the state factory
            if (AggType == CountMinSketchAggType.STATE) {
                if (validationContext.StatementRawInfo.StatementType != StatementType.CREATE_TABLE) {
                    throw new ExprValidationException(MessagePrefix + "can only be used in create-table statements");
                }

                var specification = ValidateSpecification(validationContext);
                var stateFactory = new AggregationStateCountMinSketchForge(this, specification);
                return new AggregationForgeFactoryAccessCountMinSketchState(this, stateFactory);
            }

            if (AggType != CountMinSketchAggType.ADD) {
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
            if (AggType == CountMinSketchAggType.ADD || AggType == CountMinSketchAggType.FREQ) {
                addOrFrequencyEvaluator = ChildNodes[0].Forge;
                addOrFrequencyEvaluatorReturnType = addOrFrequencyEvaluator.EvaluationType;
            }

            return new AggregationForgeFactoryAccessCountMinSketchAdd(
                this, addOrFrequencyEvaluator, addOrFrequencyEvaluatorReturnType);
        }

        internal override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return false;
        }

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

            var constantNode = (ExprConstantNode) ChildNodes[0];
            var valueX = constantNode.ConstantValue;
            if (!valueX.GetType().IsGenericStringDictionary()) {
                throw DeclaredWrongParameterExpr;
            }

            // define what to populate
            PopulateFieldWValueDescriptor[] descriptors = {
                new PopulateFieldWValueDescriptor(
                    NAME_EPS_OF_TOTAL_COUNT, typeof(double?), spec.HashesSpec.GetType(),
                    value => {
                        if (value != null) {
                            spec.HashesSpec.EpsOfTotalCount = (double) value;
                        }
                    }, true),
                new PopulateFieldWValueDescriptor(
                    NAME_CONFIDENCE, typeof(double?), spec.HashesSpec.GetType(),
                    value => {
                        if (value != null) {
                            spec.HashesSpec.Confidence = (double) value;
                        }
                    }, true),
                new PopulateFieldWValueDescriptor(
                    NAME_SEED, typeof(int?), spec.HashesSpec.GetType(),
                    value => {
                        if (value != null) {
                            spec.HashesSpec.Seed = value.AsInt();
                        }
                    }, true),
                new PopulateFieldWValueDescriptor(
                    NAME_TOPK, typeof(int?), spec.GetType(),
                    value => {
                        if (value != null) {
                            spec.TopkSpec = value.AsInt();
                        }
                    }, true),
                new PopulateFieldWValueDescriptor(
                    NAME_AGENT, typeof(string), spec.GetType(),
                    value => {
                        if (value != null) {
                            CountMinSketchAgentForge transform;
                            try {
                                var transformClass =
                                    exprValidationContext.ImportService.ResolveClass((string) value, false);
                                transform = TypeHelper.Instantiate<CountMinSketchAgentForge>(transformClass);
                            }
                            catch (Exception e) {
                                throw new ExprValidationException(
                                    "Failed to instantiate agent provider: " + e.Message, e);
                            }

                            spec.Agent = transform;
                        }
                    }, true)
            };

            // populate from json, validates incorrect names, coerces types, instantiates transform
            PopulateUtil.PopulateSpecCheckParameters(
                descriptors, valueX.UnwrapStringDictionary(),
                spec, ExprNodeOrigin.AGGPARAM, exprValidationContext);

            return spec;
        }
    }
} // end of namespace