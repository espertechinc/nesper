///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.inkeyword
{
    public class PropertyHashedArrayFactoryFactoryForge : EventTableFactoryFactoryForge
    {
        protected readonly int streamNum;
        protected readonly EventType eventType;
        protected readonly string[] propertyNames;
        protected readonly Type[] propertyTypes;
        protected readonly DataInputOutputSerdeForge[] serdes;
        protected readonly bool unique;
        protected readonly bool isFireAndForget;
        private readonly StateMgmtSetting stateMgmtSettings;

        public PropertyHashedArrayFactoryFactoryForge(
            int streamNum,
            EventType eventType,
            string[] propertyNames,
            Type[] propertyTypes,
            DataInputOutputSerdeForge[] serdes,
            bool unique,
            bool isFireAndForget,
            StateMgmtSetting stateMgmtSettings)
        {
            this.streamNum = streamNum;
            this.eventType = eventType;
            this.propertyNames = propertyNames;
            this.propertyTypes = propertyTypes;
            this.serdes = serdes;
            this.unique = unique;
            this.isFireAndForget = isFireAndForget;
            this.stateMgmtSettings = stateMgmtSettings;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PropertyHashedArrayFactoryFactory), GetType(), classScope);
            method.Block.DeclareVar<EventPropertyValueGetter[]>(
                "getters",
                NewArrayByLength(typeof(EventPropertyValueGetter), Constant(propertyNames.Length)));
            for (var i = 0; i < propertyNames.Length; i++) {
                var getterSPI = ((EventTypeSPI)eventType).GetGetterSPI(propertyNames[i]);
                var getter = EventTypeUtility.CodegenGetterWCoerce(
                    getterSPI,
                    propertyTypes[i],
                    propertyTypes[i],
                    method,
                    GetType(),
                    classScope);
                method.Block.AssignArrayElement(Ref("getters"), Constant(i), getter);
            }

            method.Block.MethodReturn(
                NewInstance(
                    typeof(PropertyHashedArrayFactoryFactory),
                    Constant(streamNum),
                    Constant(propertyNames),
                    Constant(propertyTypes),
                    DataInputOutputSerdeForgeExtensions.CodegenArray(serdes, method, classScope, null),
                    Constant(unique),
                    Ref("getters"),
                    Constant(isFireAndForget),
                    stateMgmtSettings.ToExpression()));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return GetType().Name +
                   (unique ? " unique" : " non-unique") +
                   " streamNum=" +
                   streamNum +
                   " propertyNames=" +
                   propertyNames.RenderAny();
        }

        public Type EventTableClass => typeof(PropertyHashedEventTable);
    }
} // end of namespace