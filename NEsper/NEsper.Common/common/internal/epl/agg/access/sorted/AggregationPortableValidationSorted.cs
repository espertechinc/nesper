///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationPortableValidationSorted : AggregationPortableValidation
    {
        public AggregationPortableValidationSorted()
        {
        }

        public AggregationPortableValidationSorted(
            string aggFuncName,
            EventType containedEventType)
        {
            AggFuncName = aggFuncName;
            ContainedEventType = containedEventType;
        }

        public string AggFuncName { get; set; }

        public EventType ContainedEventType { get; set; }

        public void ValidateIntoTableCompatible(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);
            var other = (AggregationPortableValidationSorted) intoTableAgg;
            AggregationValidationUtil.ValidateEventType(ContainedEventType, other.ContainedEventType);
            AggregationValidationUtil.ValidateAggFuncName(AggFuncName, other.AggFuncName);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationPortableValidationSorted), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(AggregationPortableValidationSorted), "v",
                    NewInstance(typeof(AggregationPortableValidationSorted)))
                .SetProperty(Ref("v"), "AggFuncName", Constant(AggFuncName))
                .SetProperty(Ref("v"), "ContainedEventType",
                    EventTypeUtility.ResolveTypeCodegen(ContainedEventType, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("v"));
            return LocalMethod(method);
        }
    }
} // end of namespace