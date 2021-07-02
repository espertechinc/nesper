///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.hash
{
    public class PropertyHashedFactoryFactoryForge : EventTableFactoryFactoryForgeBase
    {
        private readonly EventType _eventType;
        private readonly CoercionDesc _hashCoercionDesc;
        private readonly string[] _indexedProps;
        private readonly bool _unique;
        private readonly MultiKeyClassRef _multiKeyClassRef;
        private readonly StateMgmtSetting _stateMgmtSettings;
        
        public PropertyHashedFactoryFactoryForge(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string[] indexedProps,
            EventType eventType,
            bool unique,
            CoercionDesc hashCoercionDesc,
            MultiKeyClassRef multiKeyClassRef,
            StateMgmtSetting stateMgmtSettings)
            : base(indexedStreamNum, subqueryNum, isFireAndForget)
        {
            _indexedProps = indexedProps;
            _eventType = eventType;
            _unique = unique;
            _hashCoercionDesc = hashCoercionDesc;
            _multiKeyClassRef = multiKeyClassRef;
            _stateMgmtSettings = stateMgmtSettings;
        }

        public override Type EventTableClass =>
            _unique ? typeof(PropertyHashedEventTableUnique) : typeof(PropertyHashedEventTable);

        protected override Type TypeOf()
        {
            return typeof(PropertyHashedFactoryFactory);
        }

        protected override IList<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            IList<CodegenExpression> @params = new List<CodegenExpression>();
            @params.Add(Constant(_indexedProps));
            @params.Add(Constant(_unique));
            var propertyTypes = EventTypeUtility.GetPropertyTypes(_eventType, _indexedProps);
            var getters = EventTypeUtility.GetGetters(_eventType, _indexedProps);
            var getter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                _eventType,
                getters,
                propertyTypes,
                _hashCoercionDesc.CoercionTypes,
                _multiKeyClassRef,
                method,
                classScope);

            @params.Add(getter);
            @params.Add(ConstantNull()); // no fire-and-forget transform for subqueries
            @params.Add(_multiKeyClassRef.GetExprMKSerde(method, classScope));
            @params.Add(_stateMgmtSettings.ToExpression());

            return @params;
        }

        public override string ToQueryPlan()
        {
            return GetType().Name +
                   (_unique ? " unique" : " non-unique") +
                   " streamNum=" +
                   indexedStreamNum +
                   " propertyNames=" +
                   _indexedProps.RenderAny();
        }
    }
} // end of namespace