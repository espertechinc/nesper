///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly int _streamNum;
        private readonly EventType _eventType;
        private readonly string[] _propertyNames;
        private readonly Type[] _propertyTypes;
        private readonly DataInputOutputSerdeForge[] _serdes;
        private readonly bool _unique;
        private readonly bool _isFireAndForget;
        private readonly StateMgmtSetting _stateMgmtSettings;

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
            _streamNum = streamNum;
            _eventType = eventType;
            _propertyNames = propertyNames;
            _unique = unique;
            _isFireAndForget = isFireAndForget;
            _propertyTypes = propertyTypes;
            _serdes = serdes;
            _stateMgmtSettings = stateMgmtSettings;
        }

        public Type EventTableClass {
            get => typeof(PropertyHashedEventTable);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(PropertyHashedArrayFactoryFactory),
                GetType(),
                classScope);

            method.Block.DeclareVar<EventPropertyValueGetter[]>(
                "getters",
                NewArrayByLength(typeof(EventPropertyValueGetter), Constant(_propertyNames.Length)));
            for (var i = 0; i < _propertyNames.Length; i++) {
                var getterSPI = ((EventTypeSPI) _eventType).GetGetterSPI(_propertyNames[i]);
                var getter = EventTypeUtility.CodegenGetterWCoerce(
                    getterSPI,
                    _propertyTypes[i],
                    _propertyTypes[i],
                    method,
                    GetType(),
                    classScope);
                method.Block.AssignArrayElement(Ref("getters"), Constant(i), getter);
            }

            method.Block.MethodReturn(
                NewInstance<PropertyHashedArrayFactoryFactory>(
                    Constant(_streamNum),
                    Constant(_propertyNames),
                    Constant(_propertyTypes),
                    DataInputOutputSerdeForgeExtensions.CodegenArray(_serdes, method, classScope, null),
                    Constant(_unique),
                    Ref("getters"),
                    Constant(_isFireAndForget),
                    _stateMgmtSettings.ToExpression()));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return GetType().Name +
                   (_unique ? " unique" : " non-unique") +
                   " streamNum=" +
                   _streamNum +
                   " propertyNames=" +
                   CompatExtensions.RenderAny(_propertyNames);
        }
    }
} // end of namespace