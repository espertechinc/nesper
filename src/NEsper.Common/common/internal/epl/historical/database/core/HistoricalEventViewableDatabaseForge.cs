///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    public class HistoricalEventViewableDatabaseForge : HistoricalEventViewableForgeBase
    {
        private readonly string _databaseName;
        private readonly string[] _inputParameters;
        private readonly string _preparedStatementText;
        private readonly IDictionary<string, DBOutputTypeDesc> _outputTypes;

        public HistoricalEventViewableDatabaseForge(
            int streamNum,
            EventType eventType,
            string databaseName,
            string[] inputParameters,
            string preparedStatementText,
            IDictionary<string, DBOutputTypeDesc> outputTypes) : base(streamNum, eventType)
        {
            _databaseName = databaseName;
            _inputParameters = inputParameters;
            _preparedStatementText = preparedStatementText;
            _outputTypes = outputTypes;
        }

        public override IList<StmtClassForgeableFactory> Validate(
            StreamTypeService typeService,
            IDictionary<int, IList<ExprNode>> sqlParameters,
            StatementRawInfo rawInfo,
            StatementCompileTimeServices services)
        {
            var count = 0;
            var validationContext = new ExprValidationContextBuilder(typeService, rawInfo, services)
                .WithAllowBindingConsumption(true)
                .Build();
            var inputParamNodes = new ExprNode[_inputParameters.Length];
            foreach (var inputParam in _inputParameters) {
                var raw = FindSQLExpressionNode(StreamNum, count, sqlParameters);
                if (raw == null) {
                    throw new ExprValidationException(
                        "Internal error find expression for historical stream parameter " +
                        count +
                        " stream " +
                        StreamNum);
                }

                var evaluator = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.DATABASEPOLL,
                    raw,
                    validationContext);
                inputParamNodes[count++] = evaluator;

                var visitor = new ExprNodeIdentifierCollectVisitor();
                visitor.Visit(evaluator);
                foreach (var identNode in visitor.ExprProperties) {
                    if (identNode.StreamId == StreamNum) {
                        throw new ExprValidationException(
                            "Invalid expression '" + inputParam + "' resolves to the historical data itself");
                    }

                    SubordinateStreams.Add(identNode.StreamId);
                }
            }

            InputParamEvaluators = ExprNodeUtilityQuery.GetForges(inputParamNodes);

            // plan multikey
            var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(
                InputParamEvaluators,
                false,
                rawInfo,
                services.SerdeResolver);
            MultiKeyClassRef = multiKeyPlan.ClassRef;

            return multiKeyPlan.MultiKeyForgeables;
        }

        public override Type TypeOfImplementation()
        {
            return typeof(HistoricalEventViewableDatabaseFactory);
        }

        public override void CodegenSetter(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(@ref, "DatabaseName", Constant(_databaseName))
                .SetProperty(@ref, "InputParameters", Constant(_inputParameters))
                .SetProperty(@ref, "PreparedStatementText", Constant(_preparedStatementText))
                .SetProperty(@ref, "OutputTypes", MakeOutputTypes(method, symbols, classScope));
        }

        private CodegenExpression MakeOutputTypes(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(IDictionary<string, DBOutputTypeDesc>), GetType(), classScope);
            method.Block.DeclareVar<IDictionary<string, DBOutputTypeDesc>>(
                "types",
                NewInstance(
                    typeof(Dictionary<string, DBOutputTypeDesc>),
                    Constant(CollectionUtil.CapacityHashMap(_outputTypes.Count))));
            foreach (var entry in _outputTypes) {
                method.Block.ExprDotMethod(Ref("types"), "Put", Constant(entry.Key), entry.Value.Make());
            }

            method.Block.MethodReturn(Ref("types"));
            return LocalMethod(method);
        }

        private static ExprNode FindSQLExpressionNode(
            int myStreamNumber,
            int count,
            IDictionary<int, IList<ExprNode>> sqlParameters)
        {
            if (sqlParameters == null || sqlParameters.IsEmpty()) {
                return null;
            }

            var parameters = sqlParameters.Get(myStreamNumber);
            if (parameters == null || parameters.IsEmpty() || parameters.Count < count + 1) {
                return null;
            }

            return parameters[count];
        }
    }
} // end of namespace