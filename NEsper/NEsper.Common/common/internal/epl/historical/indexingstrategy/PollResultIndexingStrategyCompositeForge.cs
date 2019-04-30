///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategyCompositeForge : PollResultIndexingStrategyForge
    {
        private readonly EventType eventType;
        private readonly Type[] optHashCoercedTypes;
        private readonly string[] optHashPropertyNames;
        private readonly string[] rangeProps;
        private readonly Type[] rangeTypes;
        private readonly int streamNum;

        public PollResultIndexingStrategyCompositeForge(
            int streamNum,
            EventType eventType,
            string[] optHashPropertyNames,
            Type[] optHashCoercedTypes,
            string[] rangeProps,
            Type[] rangeTypes)
        {
            this.streamNum = streamNum;
            this.eventType = eventType;
            this.optHashPropertyNames = optHashPropertyNames;
            this.optHashCoercedTypes = optHashCoercedTypes;
            this.rangeProps = rangeProps;
            this.rangeTypes = rangeTypes;
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PollResultIndexingStrategyComposite), GetType(), classScope);

            var hashGetter = ConstantNull();
            if (optHashPropertyNames != null) {
                var propertyGetters = EventTypeUtility.GetGetters(eventType, optHashPropertyNames);
                var propertyTypes = EventTypeUtility.GetPropertyTypes(eventType, optHashPropertyNames);
                hashGetter = EventTypeUtility.CodegenGetterMayMultiKeyWCoerce(
                    eventType, propertyGetters, propertyTypes, optHashCoercedTypes, method, GetType(), classScope);
            }

            method.Block.DeclareVar(
                typeof(EventPropertyValueGetter[]), "rangeGetters",
                NewArrayByLength(typeof(EventPropertyValueGetter), Constant(rangeProps.Length)));
            for (var i = 0; i < rangeProps.Length; i++) {
                var propertyType = eventType.GetPropertyType(rangeProps[i]);
                var getterSPI = ((EventTypeSPI) eventType).GetGetterSPI(rangeProps[i]);
                var getter = EventTypeUtility.CodegenGetterWCoerce(
                    getterSPI, propertyType, rangeTypes[i], method, GetType(), classScope);
                method.Block.AssignArrayElement(Ref("rangeGetters"), Constant(i), getter);
            }

            method.Block
                .DeclareVar(
                    typeof(PollResultIndexingStrategyComposite), "strat",
                    NewInstance(typeof(PollResultIndexingStrategyComposite)))
                .SetProperty(Ref("strat"), "StreamNum", Constant(streamNum))
                .SetProperty(Ref("strat"), "OptionalKeyedProps", Constant(optHashPropertyNames))
                .SetProperty(Ref("strat"), "OptKeyCoercedTypes", Constant(optHashCoercedTypes))
                .SetProperty(Ref("strat"), "HashGetter", hashGetter)
                .SetProperty(Ref("strat"), "RangeProps", Constant(rangeProps))
                .SetProperty(Ref("strat"), "OptRangeCoercedTypes", Constant(rangeTypes))
                .SetProperty(Ref("strat"), "RangeGetters", Ref("rangeGetters"))
                .ExprDotMethod(Ref("strat"), "init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace