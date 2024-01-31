///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class PollResultIndexingStrategyHashForge : PollResultIndexingStrategyForge
    {
        private readonly Type[] _coercionTypes;
        private readonly EventType _eventType;
        private readonly string[] _propertyNames;
        private readonly int _streamNum;
        private readonly MultiKeyClassRef _multiKeyClasses;

        public PollResultIndexingStrategyHashForge(
            int streamNum,
            EventType eventType,
            string[] propertyNames,
            Type[] coercionTypes,
            MultiKeyClassRef multiKeyClasses)
        {
            _streamNum = streamNum;
            _eventType = eventType;
            _propertyNames = propertyNames;
            _coercionTypes = coercionTypes;
            _multiKeyClasses = multiKeyClasses;
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

            var propertyGetters = EventTypeUtility.GetGetters(_eventType, _propertyNames);
            var propertyTypes = EventTypeUtility.GetPropertyTypes(_eventType, _propertyNames);
            var valueGetter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                _eventType,
                propertyGetters,
                propertyTypes,
                _coercionTypes,
                _multiKeyClasses,
                method,
                classScope);

            method.Block
                .DeclareVarNewInstance<PollResultIndexingStrategyHash>("strat")
                .SetProperty(Ref("strat"), "StreamNum", Constant(_streamNum))
                .SetProperty(Ref("strat"), "PropertyNames", Constant(_propertyNames))
                .SetProperty(Ref("strat"), "ValueGetter", valueGetter)
                .ExprDotMethod(Ref("strat"), "Init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace