///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayNestedEntryPropertyGetterMap : ObjectArrayNestedEntryPropertyGetterBase
    {
        private readonly MapEventPropertyGetter mapGetter;

        public ObjectArrayNestedEntryPropertyGetterMap(
            int propertyIndex,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            MapEventPropertyGetter mapGetter)
            : base(propertyIndex, fragmentType, eventBeanTypedEventFactory)
        {
            this.mapGetter = mapGetter;
        }

        public override object HandleNestedValue(object value)
        {
            if (!(value is IDictionary<string, object>)) {
                if (value is EventBean) {
                    return mapGetter.Get((EventBean) value);
                }

                return null;
            }

            return mapGetter.GetMap((IDictionary<string, object>) value);
        }

        public override object HandleNestedValueFragment(object value)
        {
            if (!(value is IDictionary<string, object>)) {
                if (value is EventBean) {
                    return mapGetter.GetFragment((EventBean) value);
                }

                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean = eventBeanTypedEventFactory.AdapterForTypedMap(
                (IDictionary<string, object>) value,
                fragmentType);
            return mapGetter.GetFragment(eventBean);
        }

        public override bool HandleNestedValueExists(object value)
        {
            if (!(value is IDictionary<string, object>)) {
                if (value is EventBean) {
                    return mapGetter.IsExistsProperty((EventBean) value);
                }

                return false;
            }

            return mapGetter.IsMapExistsProperty((IDictionary<string, object>) value);
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                CodegenLegoPropertyBeanOrUnd.From(
                    codegenMethodScope,
                    codegenClassScope,
                    typeof(IDictionary<string, object>),
                    mapGetter,
                    CodegenLegoPropertyBeanOrUnd.AccessType.GET,
                    GetType()),
                refName);
        }

        public override CodegenExpression HandleNestedValueExistsCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                CodegenLegoPropertyBeanOrUnd.From(
                    codegenMethodScope,
                    codegenClassScope,
                    typeof(IDictionary<object, object>),
                    mapGetter,
                    CodegenLegoPropertyBeanOrUnd.AccessType.EXISTS,
                    GetType()),
                refName);
        }

        public override CodegenExpression HandleNestedValueFragmentCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                CodegenLegoPropertyBeanOrUnd.From(
                    codegenMethodScope,
                    codegenClassScope,
                    typeof(IDictionary<object, object>),
                    mapGetter,
                    CodegenLegoPropertyBeanOrUnd.AccessType.FRAGMENT,
                    GetType()),
                refName);
        }
    }
} // end of namespace