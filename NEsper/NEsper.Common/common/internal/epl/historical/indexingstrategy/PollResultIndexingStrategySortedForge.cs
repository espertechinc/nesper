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
        private readonly EventType eventType;
        private readonly string propertyName;
        private readonly int streamNum;
        private readonly Type valueType;

        public PollResultIndexingStrategySortedForge(
            int streamNum,
            EventType eventType,
            string propertyName,
            Type valueType)
        {
            this.streamNum = streamNum;
            this.eventType = eventType;
            this.propertyName = propertyName;
            this.valueType = valueType;
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

            var propertyGetter = ((EventTypeSPI) eventType).GetGetterSPI(propertyName);
            var propertyType = eventType.GetPropertyType(propertyName);
            var valueGetter = EventTypeUtility.CodegenGetterWCoerce(
                propertyGetter, propertyType, valueType, method, GetType(), classScope);

            method.Block
                .DeclareVar(
                    typeof(PollResultIndexingStrategySorted), "strat",
                    NewInstance(typeof(PollResultIndexingStrategySorted)))
                .SetProperty(Ref("strat"), "StreamNum", Constant(streamNum))
                .SetProperty(Ref("strat"), "PropertyName", Constant(propertyName))
                .SetProperty(Ref("strat"), "ValueGetter", valueGetter)
                .SetProperty(Ref("strat"), "ValueType", Constant(valueType))
                .ExprDotMethod(Ref("strat"), "init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace