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
        private readonly int streamNum;
        private readonly EventType eventType;
        private readonly string[] propertyNames;
        private readonly Type[] propertyTypes;
        private readonly DataInputOutputSerdeForge[] serdes;
        private readonly bool unique;
        private readonly bool isFireAndForget;

        public PropertyHashedArrayFactoryFactoryForge(
            int streamNum,
            EventType eventType,
            string[] propertyNames,
            Type[] propertyTypes,
            DataInputOutputSerdeForge[] serdes,
            bool unique,
            bool isFireAndForget)
        {
            this.streamNum = streamNum;
            this.eventType = eventType;
            this.propertyNames = propertyNames;
            this.unique = unique;
            this.isFireAndForget = isFireAndForget;
            this.propertyTypes = propertyTypes;
            this.serdes = serdes;
        }

        public Type EventTableClass {
            get => typeof(PropertyHashedEventTable);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(PropertyHashedArrayFactoryFactory),
                this.GetType(),
                classScope);

            method.Block.DeclareVar<EventPropertyValueGetter[]>(
                "getters",
                NewArrayByLength(typeof(EventPropertyValueGetter), Constant(propertyNames.Length)));
            for (int i = 0; i < propertyNames.Length; i++) {
                EventPropertyGetterSPI getterSPI = ((EventTypeSPI) eventType).GetGetterSPI(propertyNames[i]);
                CodegenExpression getter = EventTypeUtility.CodegenGetterWCoerce(
                    getterSPI,
                    propertyTypes[i],
                    propertyTypes[i],
                    method,
                    this.GetType(),
                    classScope);
                method.Block.AssignArrayElement(Ref("getters"), Constant(i), getter);
            }

            method.Block.MethodReturn(
                NewInstance<PropertyHashedArrayFactoryFactory>(
                    Constant(streamNum),
                    Constant(propertyNames),
                    Constant(propertyTypes),
                    DataInputOutputSerdeForgeExtensions.CodegenArray(serdes, method, classScope, null),
                    Constant(unique),
                    Ref("getters"),
                    Constant(isFireAndForget)));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return this.GetType().Name +
                   (unique ? " unique" : " non-unique") +
                   " streamNum=" +
                   streamNum +
                   " propertyNames=" +
                   CompatExtensions.RenderAny(propertyNames);
        }
    }
} // end of namespace