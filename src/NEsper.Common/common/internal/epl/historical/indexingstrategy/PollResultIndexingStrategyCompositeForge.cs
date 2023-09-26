///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategyCompositeForge : PollResultIndexingStrategyForge
    {
        private readonly int streamNum;
        private readonly EventType eventType;
        private readonly string[] optHashPropertyNames;
        private readonly Type[] optHashCoercedTypes;
        private readonly MultiKeyClassRef optHashMultiKeyClasses;
        private readonly string[] rangeProps;
        private readonly Type[] rangeTypes;

        public PollResultIndexingStrategyCompositeForge(
            int streamNum,
            EventType eventType,
            string[] optHashPropertyNames,
            Type[] optHashCoercedTypes,
            MultiKeyClassRef optHashMultiKeyClasses,
            string[] rangeProps,
            Type[] rangeTypes)
        {
            this.streamNum = streamNum;
            this.eventType = eventType;
            this.optHashPropertyNames = optHashPropertyNames;
            this.optHashCoercedTypes = optHashCoercedTypes;
            this.optHashMultiKeyClasses = optHashMultiKeyClasses;
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
                hashGetter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                    eventType,
                    propertyGetters,
                    propertyTypes,
                    optHashCoercedTypes,
                    optHashMultiKeyClasses,
                    method,
                    classScope);
            }

            method.Block.DeclareVar<EventPropertyValueGetter[]>(
                "rangeGetters",
                NewArrayByLength(typeof(EventPropertyValueGetter), Constant(rangeProps.Length)));
            for (var i = 0; i < rangeProps.Length; i++) {
                var propertyType = eventType.GetPropertyType(rangeProps[i]);
                var getterSPI = ((EventTypeSPI)eventType).GetGetterSPI(rangeProps[i]);
                var getter = EventTypeUtility.CodegenGetterWCoerce(
                    getterSPI,
                    propertyType,
                    rangeTypes[i],
                    method,
                    GetType(),
                    classScope);
                method.Block.AssignArrayElement(Ref("rangeGetters"), Constant(i), getter);
            }

            method.Block
                .DeclareVarNewInstance(typeof(PollResultIndexingStrategyComposite), "strat")
                .ExprDotMethod(Ref("strat"), "setStreamNum", Constant(streamNum))
                .ExprDotMethod(Ref("strat"), "setOptionalKeyedProps", Constant(optHashPropertyNames))
                .ExprDotMethod(Ref("strat"), "setOptKeyCoercedTypes", Constant(optHashCoercedTypes))
                .ExprDotMethod(Ref("strat"), "setHashGetter", hashGetter)
                .ExprDotMethod(Ref("strat"), "setRangeProps", Constant(rangeProps))
                .ExprDotMethod(Ref("strat"), "setOptRangeCoercedTypes", Constant(rangeTypes))
                .ExprDotMethod(Ref("strat"), "setRangeGetters", Ref("rangeGetters"))
                .ExprDotMethod(Ref("strat"), "init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace