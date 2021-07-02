///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategyInKeywordMultiForge : PollResultIndexingStrategyForge
    {
        private readonly EventType _eventType;
        private readonly string[] _propertyNames;
        private readonly int _streamNum;

        public PollResultIndexingStrategyInKeywordMultiForge(
            int streamNum,
            EventType eventType,
            string[] propertyNames)
        {
            this._streamNum = streamNum;
            this._eventType = eventType;
            this._propertyNames = propertyNames;
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
            var method = parent.MakeChild(typeof(PollResultIndexingStrategyInKeywordMulti), GetType(), classScope);

            method.Block.DeclareVar<EventPropertyValueGetter[]>(
                "getters",
                NewArrayByLength(typeof(EventPropertyValueGetter), Constant(_propertyNames.Length)));
            for (var i = 0; i < _propertyNames.Length; i++) {
                var getter = ((EventTypeSPI) _eventType).GetGetterSPI(_propertyNames[i]);
                var getterType = _eventType.GetPropertyType(_propertyNames[i]);
                var eval = EventTypeUtility.CodegenGetterWCoerce(
                    getter,
                    getterType,
                    getterType,
                    method,
                    GetType(),
                    classScope);
                method.Block.AssignArrayElement(Ref("getters"), Constant(i), eval);
            }

            method.Block
                .DeclareVar<PollResultIndexingStrategyInKeywordMulti>(
                    "strat",
                    NewInstance(typeof(PollResultIndexingStrategyInKeywordMulti)))
                .SetProperty(Ref("strat"), "StreamNum", Constant(_streamNum))
                .SetProperty(Ref("strat"), "PropertyNames", Constant(_propertyNames))
                .SetProperty(Ref("strat"), "ValueGetters", Ref("getters"))
                .ExprDotMethod(Ref("strat"), "Init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace