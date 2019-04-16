///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class ExprTableEvalStrategyFactoryForge
    {
        private readonly TableMetaData tableMeta;
        private readonly ExprForge[] optionalGroupKeys;
        private ExprTableEvalStrategyEnum strategyEnum;
        private int aggColumnNum = -1;
        private int propertyIndex = -1;
        private ExprEnumerationGivenEventForge optionalEnumEval;
        private AggregationTableAccessAggReaderForge accessAggStrategy;

        public ExprTableEvalStrategyFactoryForge(
            TableMetaData tableMeta,
            ExprForge[] optionalGroupKeys)
        {
            this.tableMeta = tableMeta;
            this.optionalGroupKeys = optionalGroupKeys;
        }

        public ExprTableEvalStrategyEnum StrategyEnum {
            set { this.strategyEnum = value; }
        }

        public int PropertyIndex {
            set { this.propertyIndex = value; }
        }

        public ExprEnumerationGivenEventForge OptionalEnumEval {
            set { this.optionalEnumEval = value; }
        }

        public int AggColumnNum {
            set { this.aggColumnNum = value; }
        }

        public AggregationTableAccessAggReaderForge AccessAggStrategy {
            set { this.accessAggStrategy = value; }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(ExprTableEvalStrategyFactory), this.GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(ExprTableEvalStrategyFactory), "factory", NewInstance(typeof(ExprTableEvalStrategyFactory)))
                .ExprDotMethod(@Ref("factory"), "setStrategyEnum", Constant(strategyEnum))
                .ExprDotMethod(
                    @Ref("factory"), "setTable",
                    TableDeployTimeResolver.MakeResolveTable(tableMeta, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(
                    @Ref("factory"), "setGroupKeyEval",
                    optionalGroupKeys == null || optionalGroupKeys.Length == 0
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                            optionalGroupKeys, tableMeta.KeyTypes, method, this.GetType(), classScope))
                .ExprDotMethod(@Ref("factory"), "setAggColumnNum", Constant(aggColumnNum))
                .ExprDotMethod(@Ref("factory"), "setPropertyIndex", Constant(propertyIndex))
                .ExprDotMethod(
                    @Ref("factory"), "setOptionalEnumEval",
                    optionalEnumEval == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenExprEnumEval(
                            optionalEnumEval, method, symbols, classScope, this.GetType()))
                .ExprDotMethod(
                    @Ref("factory"), "setAccessAggReader",
                    accessAggStrategy == null
                        ? ConstantNull()
                        : accessAggStrategy.CodegenCreateReader(method, symbols, classScope))
                .MethodReturn(@Ref("factory"));
            return LocalMethod(method);
        }
    }
} // end of namespace