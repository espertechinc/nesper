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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.plugin
{
    public class AggregationPortableValidationPluginMultiFunc : AggregationPortableValidation
    {
        private string aggregationFunctionName;

        public void ValidateIntoTableCompatible(string tableExpression, AggregationPortableValidation intoTableAgg, string intoExpression, AggregationForgeFactory factory)
        {
            AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);
        }

        public CodegenExpression Make(CodegenMethodScope parent, ModuleTableInitializeSymbol symbols, CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(AggregationPortableValidationPluginMultiFunc), this.GetType(), classScope);
            method.Block
                    .DeclareVar(typeof(AggregationPortableValidationPluginMultiFunc), "portable", NewInstance(typeof(AggregationPortableValidationPluginMultiFunc)))
                    .ExprDotMethod(@Ref("portable"), "setAggregationFunctionName", Constant(aggregationFunctionName))
                    .MethodReturn(@Ref("portable"));
            return LocalMethod(method);
        }

        public string AggregationFunctionName
        {
            get => aggregationFunctionName;
        }

        public void SetAggregationFunctionName(string aggregationFunctionName)
        {
            this.aggregationFunctionName = aggregationFunctionName;
        }
    }
} // end of namespace