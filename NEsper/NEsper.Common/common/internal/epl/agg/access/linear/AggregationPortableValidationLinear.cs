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

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationPortableValidationLinear : AggregationPortableValidation
    {
        public AggregationPortableValidationLinear()
        {
        }

        public AggregationPortableValidationLinear(EventType containedEventType)
        {
            ContainedEventType = containedEventType;
        }

        public EventType ContainedEventType { get; private set; }

        public void SetContainedEventType(EventType containedEventType)
        {
            ContainedEventType = containedEventType;
        }

        public void ValidateIntoTableCompatible(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);
            var other = (AggregationPortableValidationLinear) intoTableAgg;
            AggregationValidationUtil.ValidateEventType(ContainedEventType, other.ContainedEventType);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationPortableValidationLinear), GetType(), classScope);
            method.Block
                .DeclareVar<AggregationPortableValidationLinear>(
                    "v",
                    NewInstance(typeof(AggregationPortableValidationLinear)))
                .SetProperty(
                    Ref("v"),
                    "ContainedEventType",
                    EventTypeUtility.ResolveTypeCodegen(ContainedEventType, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("v"));
            return LocalMethod(method);
        }
    }
} // end of namespace