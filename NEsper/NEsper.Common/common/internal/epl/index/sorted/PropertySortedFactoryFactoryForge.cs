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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.sorted
{
    public class PropertySortedFactoryFactoryForge : EventTableFactoryFactoryForgeBase
    {
        private readonly CoercionDesc coercionDesc;
        private readonly EventType eventType;

        private readonly string indexedProp;

        public PropertySortedFactoryFactoryForge(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string indexedProp,
            EventType eventType,
            CoercionDesc coercionDesc)
            : base(indexedStreamNum, subqueryNum, isFireAndForget)
        {
            this.indexedProp = indexedProp;
            this.eventType = eventType;
            this.coercionDesc = coercionDesc;
        }

        public override Type EventTableClass => typeof(PropertySortedEventTable);

        protected override Type TypeOf()
        {
            return typeof(PropertySortedFactoryFactory);
        }

        protected override IList<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            IList<CodegenExpression> @params = new List<CodegenExpression>();
            @params.Add(Constant(indexedProp));
            @params.Add(Constant(coercionDesc.CoercionTypes[0]));
            var propertyType = eventType.GetPropertyType(indexedProp);
            var getterSPI = ((EventTypeSPI) eventType).GetGetterSPI(indexedProp);
            var getter = EventTypeUtility.CodegenGetterWCoerce(
                getterSPI, propertyType, coercionDesc.CoercionTypes[0], method, GetType(), classScope);
            @params.Add(getter);
            return @params;
        }

        public override string ToQueryPlan()
        {
            return GetType().Name +
                   " streamNum=" + indexedStreamNum +
                   " propertyName=" + indexedProp;
        }
    }
} // end of namespace