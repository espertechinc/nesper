///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private readonly string[] indexedProps;
        private readonly EventType eventType;
        private readonly bool unique;
        private readonly CoercionDesc hashCoercionDesc;
        private readonly MultiKeyClassRef multiKeyClassRef;
        private readonly StateMgmtSetting stateMgmtSettings;

        public PropertyHashedFactoryFactoryForge(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string[] indexedProps,
            EventType eventType,
            bool unique,
            CoercionDesc hashCoercionDesc,
            MultiKeyClassRef multiKeyClassRef,
            StateMgmtSetting stateMgmtSettings) : base(indexedStreamNum, subqueryNum, isFireAndForget)
        {
            this.indexedProps = indexedProps;
            this.eventType = eventType;
            this.unique = unique;
            this.hashCoercionDesc = hashCoercionDesc;
            this.multiKeyClassRef = multiKeyClassRef;
            this.stateMgmtSettings = stateMgmtSettings;
        }

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
            @params.Add(Constant(indexedProps));
            @params.Add(Constant(unique));
            var propertyTypes = EventTypeUtility.GetPropertyTypes(eventType, indexedProps);
            var getters = EventTypeUtility.GetGetters(eventType, indexedProps);

            var getter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                eventType,
                getters,
                propertyTypes,
                hashCoercionDesc.CoercionTypes,
                multiKeyClassRef,
                method,
                classScope);
            @params.Add(getter);
            @params.Add(ConstantNull()); // no fire-and-forget transform for subqueries
            @params.Add(multiKeyClassRef.GetExprMKSerde(method, classScope));
            @params.Add(stateMgmtSettings.ToExpression());
            return @params;
        }

        public override string ToQueryPlan()
        {
            return GetType().Name +
                   (unique ? " unique" : " non-unique") +
                   " streamNum=" +
                   indexedStreamNum +
                   " propertyNames=" +
                   indexedProps.RenderAny();
        }

        public override Type EventTableClass =>
            unique ? typeof(PropertyHashedEventTableUnique) : typeof(PropertyHashedEventTable);
    }
} // end of namespace