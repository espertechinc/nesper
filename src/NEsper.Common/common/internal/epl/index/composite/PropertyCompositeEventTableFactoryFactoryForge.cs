///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.composite
{
    public class PropertyCompositeEventTableFactoryFactoryForge : EventTableFactoryFactoryForge
    {
        private readonly int indexedStreamNum;
        private readonly int? subqueryNum;
        private readonly bool isFireAndForget;
        private readonly string[] optKeyProps;
        private readonly Type[] optKeyTypes;
        private readonly MultiKeyClassRef hashMultikeyClasses;
        private readonly string[] rangeProps;
        private readonly Type[] rangeTypes;
        private readonly DataInputOutputSerdeForge[] rangeSerdes;
        private readonly EventType eventType;

        public PropertyCompositeEventTableFactoryFactoryForge(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string[] optKeyProps,
            Type[] optKeyTypes,
            MultiKeyClassRef hashMultikeyClasses,
            string[] rangeProps,
            Type[] rangeTypes,
            DataInputOutputSerdeForge[] rangeSerdes,
            EventType eventType)
        {
            this.indexedStreamNum = indexedStreamNum;
            this.subqueryNum = subqueryNum;
            this.isFireAndForget = isFireAndForget;
            this.optKeyProps = optKeyProps;
            this.optKeyTypes = optKeyTypes;
            this.hashMultikeyClasses = hashMultikeyClasses;
            this.rangeProps = rangeProps;
            this.rangeTypes = rangeTypes;
            this.rangeSerdes = rangeSerdes;
            this.eventType = eventType;
        }

        public string ToQueryPlan()
        {
            return GetType().Name +
                   " streamNum=" +
                   indexedStreamNum +
                   " keys=" +
                   (optKeyProps == null ? "none" : optKeyProps.RenderAny()) +
                   " ranges=" +
                   rangeProps.RenderAny();
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PropertyCompositeEventTableFactoryFactory), GetType(), classScope);
            var hashGetter = ConstantNull();
            if (optKeyProps != null && optKeyProps.Length > 0) {
                var propertyTypes = EventTypeUtility.GetPropertyTypes(eventType, optKeyProps);
                var getters = EventTypeUtility.GetGetters(eventType, optKeyProps);
                hashGetter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                    eventType,
                    getters,
                    propertyTypes,
                    optKeyTypes,
                    hashMultikeyClasses,
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

            IList<CodegenExpression> @params = new List<CodegenExpression>();
            @params.Add(Constant(indexedStreamNum));
            @params.Add(Constant(subqueryNum));
            @params.Add(Constant(isFireAndForget));
            @params.Add(Constant(optKeyProps));
            @params.Add(Constant(optKeyTypes));
            @params.Add(hashGetter);
            @params.Add(hashMultikeyClasses.GetExprMKSerde(method, classScope));
            @params.Add(Constant(rangeProps));
            @params.Add(Constant(rangeTypes));
            @params.Add(Ref("rangeGetters"));
            @params.Add(DataInputOutputSerdeForgeExtensions.CodegenArray(rangeSerdes, method, classScope, null));
            method.Block.MethodReturn(
                NewInstance(typeof(PropertyCompositeEventTableFactoryFactory), @params.ToArray()));
            return LocalMethod(method);
        }

        public Type EventTableClass => typeof(PropertyCompositeEventTable);
    }
} // end of namespace