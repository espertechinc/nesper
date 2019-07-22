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
    public class PollResultIndexingStrategyHashForge : PollResultIndexingStrategyForge
    {
        private readonly Type[] coercionTypes;
        private readonly EventType eventType;
        private readonly string[] propertyNames;
        private readonly int streamNum;

        public PollResultIndexingStrategyHashForge(
            int streamNum,
            EventType eventType,
            string[] propertyNames,
            Type[] coercionTypes)
        {
            this.streamNum = streamNum;
            this.eventType = eventType;
            this.propertyNames = propertyNames;
            this.coercionTypes = coercionTypes;
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
            var method = parent.MakeChild(typeof(PollResultIndexingStrategyHash), GetType(), classScope);

            var propertyGetters = EventTypeUtility.GetGetters(eventType, propertyNames);
            var propertyTypes = EventTypeUtility.GetPropertyTypes(eventType, propertyNames);
            var valueGetter = EventTypeUtility.CodegenGetterMayMultiKeyWCoerce(
                eventType,
                propertyGetters,
                propertyTypes,
                coercionTypes,
                method,
                GetType(),
                classScope);

            method.Block
                .DeclareVar<PollResultIndexingStrategyHash>(
                    "strat",
                    NewInstance(typeof(PollResultIndexingStrategyHash)))
                .SetProperty(Ref("strat"), "StreamNum", Constant(streamNum))
                .SetProperty(Ref("strat"), "PropertyNames", Constant(propertyNames))
                .SetProperty(Ref("strat"), "ValueGetter", valueGetter)
                .ExprDotMethod(Ref("strat"), "init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace