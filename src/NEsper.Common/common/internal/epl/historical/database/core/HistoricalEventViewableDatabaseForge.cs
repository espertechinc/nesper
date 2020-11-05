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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    public class HistoricalEventViewableDatabaseForge : HistoricalEventViewableForgeBase
    {
        private readonly string databaseName;
        private readonly string[] inputParameters;
        private readonly string preparedStatementText;
        private readonly IDictionary<string, DBOutputTypeDesc> outputTypes;

        public HistoricalEventViewableDatabaseForge(
            int streamNum,
            EventType eventType,
            string databaseName,
            string[] inputParameters,
            string preparedStatementText,
            IDictionary<string, DBOutputTypeDesc> outputTypes)
            : base(streamNum, eventType)
        {
            this.databaseName = databaseName;
            this.inputParameters = inputParameters;
            this.preparedStatementText = preparedStatementText;
            this.outputTypes = outputTypes;
        }

        public override IList<StmtClassForgeableFactory> Validate(
            StreamTypeService typeService,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            int count = 0;
            ExprValidationContext validationContext =
                new ExprValidationContextBuilder(typeService, @base.StatementRawInfo, services)
                    .WithAllowBindingConsumption(true)
                    .Build();
            ExprNode[] inputParamNodes = new ExprNode[inputParameters.Length];
            foreach (string inputParam in inputParameters) {
                ExprNode raw = FindSQLExpressionNode(StreamNum, count, @base.StatementSpec.Raw.SqlParameters);
                if (raw == null) {
                    throw new ExprValidationException(
                        "Internal error find expression for historical stream parameter " +
                        count +
                        " stream " +
                        StreamNum);
                }

                ExprNode evaluator = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.DATABASEPOLL,
                    raw,
                    validationContext);
                inputParamNodes[count++] = evaluator;

                ExprNodeIdentifierCollectVisitor visitor = new ExprNodeIdentifierCollectVisitor();
                visitor.Visit(evaluator);
                foreach (ExprIdentNode identNode in visitor.ExprProperties) {
                    if (identNode.StreamId == StreamNum) {
                        throw new ExprValidationException(
                            "Invalid expression '" + inputParam + "' resolves to the historical data itself");
                    }

                    SubordinateStreams.Add(identNode.StreamId);
                }
            }

            InputParamEvaluators = ExprNodeUtilityQuery.GetForges(inputParamNodes);
            
            
            // plan multikey
            MultiKeyPlan multiKeyPlan = MultiKeyPlanner.PlanMultiKey(InputParamEvaluators, false, @base.StatementRawInfo, services.SerdeResolver);
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
                .SetProperty(@ref, "DatabaseName", Constant(databaseName))
                .SetProperty(@ref, "InputParameters", Constant(inputParameters))
                .SetProperty(@ref, "PreparedStatementText", Constant(preparedStatementText))
                .SetProperty(@ref, "OutputTypes", MakeOutputTypes(method, symbols, classScope));
        }

        private CodegenExpression MakeOutputTypes(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(IDictionary<object, object>), this.GetType(), classScope);
            method.Block.DeclareVar<IDictionary<object, object>>(
                "types",
                NewInstance(typeof(Dictionary<object, object>)));
            foreach (KeyValuePair<string, DBOutputTypeDesc> entry in outputTypes) {
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
            if ((sqlParameters == null) || (sqlParameters.IsEmpty())) {
                return null;
            }

            IList<ExprNode> parameters = sqlParameters.Get(myStreamNumber);
            if ((parameters == null) || (parameters.IsEmpty()) || (parameters.Count < (count + 1))) {
                return null;
            }

            return parameters[count];
        }
    }
} // end of namespace