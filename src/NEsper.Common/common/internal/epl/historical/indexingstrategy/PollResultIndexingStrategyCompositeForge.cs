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
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategyCompositeForge : PollResultIndexingStrategyForge
    {
        private readonly EventType _eventType;
        private readonly Type[] _optHashCoercedTypes;
        private readonly string[] _optHashPropertyNames;
        private readonly MultiKeyClassRef _optHashMultiKeyClasses;
        private readonly string[] _rangeProps;
        private readonly Type[] _rangeTypes;
        private readonly int _streamNum;

        public PollResultIndexingStrategyCompositeForge(
            int streamNum,
            EventType eventType,
            string[] optHashPropertyNames,
            Type[] optHashCoercedTypes,
            MultiKeyClassRef optHashMultiKeyClasses,
            string[] rangeProps,
            Type[] rangeTypes)
        {
            this._streamNum = streamNum;
            this._eventType = eventType;
            this._optHashPropertyNames = optHashPropertyNames;
            this._optHashCoercedTypes = optHashCoercedTypes;
            this._optHashMultiKeyClasses = optHashMultiKeyClasses;
            this._rangeProps = rangeProps;
            this._rangeTypes = rangeTypes;
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
            if (_optHashPropertyNames != null) {
                var propertyGetters = EventTypeUtility.GetGetters(_eventType, _optHashPropertyNames);
                var propertyTypes = EventTypeUtility.GetPropertyTypes(_eventType, _optHashPropertyNames);
                hashGetter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                    _eventType,
                    propertyGetters,
                    propertyTypes,
                    _optHashCoercedTypes,
                    _optHashMultiKeyClasses,
                    method,
                    classScope);
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

            method.Block
                .DeclareVar<PollResultIndexingStrategyComposite>(
                    "strat",
                    NewInstance(typeof(PollResultIndexingStrategyComposite)))
                .SetProperty(Ref("strat"), "StreamNum", Constant(_streamNum))
                .SetProperty(Ref("strat"), "OptionalKeyedProps", Constant(_optHashPropertyNames))
                .SetProperty(Ref("strat"), "OptKeyCoercedTypes", Constant(_optHashCoercedTypes))
                .SetProperty(Ref("strat"), "HashGetter", hashGetter)
                .SetProperty(Ref("strat"), "RangeProps", Constant(_rangeProps))
                .SetProperty(Ref("strat"), "OptRangeCoercedTypes", Constant(_rangeTypes))
                .SetProperty(Ref("strat"), "RangeGetters", Ref("rangeGetters"))
                .ExprDotMethod(Ref("strat"), "Init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace