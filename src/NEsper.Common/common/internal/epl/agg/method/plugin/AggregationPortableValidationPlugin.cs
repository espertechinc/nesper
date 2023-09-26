///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.plugin
{
    public class AggregationPortableValidationPlugin : AggregationPortableValidationBase
    {
        private string _functionName;

        public AggregationPortableValidationPlugin(
            bool distinct,
            string functionName)
            : base(distinct)

        {
            _functionName = functionName;
        }

        public AggregationPortableValidationPlugin()
        {
        }

        protected override Type TypeOf()
        {
            return typeof(AggregationPortableValidationPlugin);
        }

        protected override void CodegenInlineSet(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.SetProperty(@ref, "FunctionName", Constant(_functionName));
        }

        protected override void ValidateIntoTable(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            var that = (AggregationPortableValidationPlugin)intoTableAgg;
            if (!_functionName.Equals(that._functionName)) {
                throw new ExprValidationException(
                    "The aggregation declares '" + _functionName + "' and provided is '" + that._functionName + "'");
            }
        }

        public string FunctionName {
            get => _functionName;
            set => _functionName = value;
        }
    }
} // end of namespace