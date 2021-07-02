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
    public class PollResultIndexingStrategySortedForge : PollResultIndexingStrategyForge
    {
        private readonly EventType _eventType;
        private readonly string _propertyName;
        private readonly int _streamNum;
        private readonly Type _valueType;

        public PollResultIndexingStrategySortedForge(
            int streamNum,
            EventType eventType,
            string propertyName,
            Type valueType)
        {
            this._streamNum = streamNum;
            this._eventType = eventType;
            this._propertyName = propertyName;
            this._valueType = valueType;
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
            var method = parent.MakeChild(typeof(PollResultIndexingStrategySorted), GetType(), classScope);

            var propertyGetter = ((EventTypeSPI) _eventType).GetGetterSPI(_propertyName);
            var propertyType = _eventType.GetPropertyType(_propertyName);
            var valueGetter = EventTypeUtility.CodegenGetterWCoerce(
                propertyGetter,
                propertyType,
                _valueType,
                method,
                GetType(),
                classScope);

            method.Block
                .DeclareVar<PollResultIndexingStrategySorted>(
                    "strat",
                    NewInstance(typeof(PollResultIndexingStrategySorted)))
                .SetProperty(Ref("strat"), "StreamNum", Constant(_streamNum))
                .SetProperty(Ref("strat"), "PropertyName", Constant(_propertyName))
                .SetProperty(Ref("strat"), "ValueGetter", valueGetter)
                .SetProperty(Ref("strat"), "ValueType", Constant(_valueType))
                .ExprDotMethod(Ref("strat"), "Init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace