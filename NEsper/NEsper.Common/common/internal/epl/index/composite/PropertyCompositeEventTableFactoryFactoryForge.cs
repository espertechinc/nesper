///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly EventType _eventType;
        private readonly int _indexedStreamNum;
        private readonly bool _isFireAndForget;
        private readonly string[] _optKeyProps;
        private readonly Type[] _optKeyTypes;
        private readonly MultiKeyClassRef _hashMultikeyClasses;
        private readonly string[] _rangeProps;
        private readonly Type[] _rangeTypes;
        private readonly DataInputOutputSerdeForge[] _rangeSerdes;

        private readonly int? _subqueryNum;

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
            _indexedStreamNum = indexedStreamNum;
            _subqueryNum = subqueryNum;
            _isFireAndForget = isFireAndForget;
            _optKeyProps = optKeyProps;
            _optKeyTypes = optKeyTypes;
            _hashMultikeyClasses = hashMultikeyClasses;
            _rangeProps = rangeProps;
            _rangeTypes = rangeTypes;
            _rangeSerdes = rangeSerdes;
            _eventType = eventType;
        }

        public string ToQueryPlan()
        {
            return GetType().Name +
                   " streamNum=" +
                   _indexedStreamNum +
                   " keys=" +
                   _optKeyProps ==
                   null
                ? "none"
                : _optKeyProps.RenderAny() + " ranges=" + _rangeProps.RenderAny();
        }

        public Type EventTableClass => typeof(PropertyCompositeEventTable);

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PropertyCompositeEventTableFactoryFactory), GetType(), classScope);

            var hashGetter = ConstantNull();
            if (_optKeyProps != null && _optKeyProps.Length > 0) {
                var propertyTypes = EventTypeUtility.GetPropertyTypes(_eventType, _optKeyProps);
                var getters = EventTypeUtility.GetGetters(_eventType, _optKeyProps);
                hashGetter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                    _eventType, getters, propertyTypes, _optKeyTypes, _hashMultikeyClasses, method, classScope);
            }

            method.Block.DeclareVar<EventPropertyValueGetter[]>(
                "rangeGetters",
                NewArrayByLength(typeof(EventPropertyValueGetter), Constant(_rangeProps.Length)));
            for (var i = 0; i < _rangeProps.Length; i++) {
                var propertyType = _eventType.GetPropertyType(_rangeProps[i]);
                var getterSPI = ((EventTypeSPI) _eventType).GetGetterSPI(_rangeProps[i]);
                var getter = EventTypeUtility.CodegenGetterWCoerce(
                    getterSPI,
                    propertyType,
                    _rangeTypes[i],
                    method,
                    GetType(),
                    classScope);
                method.Block.AssignArrayElement(Ref("rangeGetters"), Constant(i), getter);
            }

            IList<CodegenExpression> @params = new List<CodegenExpression>();
            @params.Add(Constant(_indexedStreamNum));
            @params.Add(Constant(_subqueryNum));
            @params.Add(Constant(_isFireAndForget));
            @params.Add(Constant(_optKeyProps));
            @params.Add(Constant(_optKeyTypes));
            @params.Add(hashGetter);
            @params.Add(_hashMultikeyClasses.GetExprMKSerde(method, classScope));
            @params.Add(Constant(_rangeProps));
            @params.Add(Constant(_rangeTypes));
            @params.Add(Ref("rangeGetters"));
            @params.Add(DataInputOutputSerdeForge.CodegenArray(_rangeSerdes, method, classScope, null));

            method.Block.MethodReturn(
                NewInstance<PropertyCompositeEventTableFactoryFactory>(@params.ToArray()));
            return LocalMethod(method);
        }
    }
} // end of namespace