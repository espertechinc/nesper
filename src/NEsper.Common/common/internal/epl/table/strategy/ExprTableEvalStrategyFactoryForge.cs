///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
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
        private readonly TableMetaData _tableMeta;
        private readonly ExprForge[] _optionalGroupKeys;
        private ExprTableEvalStrategyEnum _strategyEnum;
        private int _aggColumnNum = -1;
        private int _propertyIndex = -1;
        private ExprEnumerationGivenEventForge _optionalEnumEval;
        private AggregationMethodForge _aggregationMethod;

        public ExprTableEvalStrategyFactoryForge(
            TableMetaData tableMeta,
            ExprForge[] optionalGroupKeys)
        {
            _tableMeta = tableMeta;
            _optionalGroupKeys = optionalGroupKeys;
        }

        public ExprTableEvalStrategyEnum StrategyEnum {
            set => _strategyEnum = value;
        }

        public int PropertyIndex {
            set => _propertyIndex = value;
        }

        public ExprEnumerationGivenEventForge OptionalEnumEval {
            set => _optionalEnumEval = value;
        }

        public int AggColumnNum {
            set => _aggColumnNum = value;
        }

        public AggregationMethodForge AggregationMethodForge {
            set => _aggregationMethod = value;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ExprTableEvalStrategyFactory), GetType(), classScope);
            var groupKeyEval = ConstantNull();
            if (_optionalGroupKeys != null && _optionalGroupKeys.Length > 0) {
                groupKeyEval = MultiKeyCodegen.CodegenEvaluatorReturnObjectOrArrayWCoerce(
                    _optionalGroupKeys,
                    _tableMeta.KeyTypes,
                    true,
                    method,
                    GetType(),
                    classScope);
            }

            var optionalEnumEval = ConstantNull();
            if (_optionalEnumEval != null) {
                optionalEnumEval = ExprNodeUtilityCodegen.CodegenExprEnumEval(
                    _optionalEnumEval,
                    method,
                    symbols,
                    classScope,
                    GetType());
            }

            var aggregationMethodEval = ConstantNull();
            if (_aggregationMethod != null) {
                aggregationMethodEval = _aggregationMethod.CodegenCreateReader(method, symbols, classScope);
            }

            method.Block
                .DeclareVar<ExprTableEvalStrategyFactory>("factory", NewInstance(typeof(ExprTableEvalStrategyFactory)))
                .SetProperty(Ref("factory"), "StrategyEnum", Constant(_strategyEnum))
                .SetProperty(
                    Ref("factory"),
                    "Table",
                    TableDeployTimeResolver.MakeResolveTable(_tableMeta, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("factory"), "GroupKeyEval", groupKeyEval)
                .SetProperty(Ref("factory"), "AggColumnNum", Constant(_aggColumnNum))
                .SetProperty(Ref("factory"), "PropertyIndex", Constant(_propertyIndex))
                .SetProperty(Ref("factory"), "OptionalEnumEval", optionalEnumEval)
                .SetProperty(Ref("factory"), "AggregationMethod", aggregationMethodEval)
                .MethodReturn(Ref("factory"));

            return LocalMethod(method);
        }
    }
} // end of namespace