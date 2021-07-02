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
        private readonly MapEventPropertyGetter _mapGetter;

        public ObjectArrayNestedEntryPropertyGetterMap(
            int propertyIndex,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            MapEventPropertyGetter mapGetter)
            : base(propertyIndex, fragmentType, eventBeanTypedEventFactory)
        {
            this._mapGetter = mapGetter;
        }

        public override object HandleNestedValue(object value)
        {
            if (!(value is IDictionary<string, object>)) {
                if (value is EventBean) {
                    return _mapGetter.Get((EventBean) value);
                }

                return null;
            }

            return _mapGetter.GetMap((IDictionary<string, object>) value);
        }

        public override object HandleNestedValueFragment(object value)
        {
            if (!(value is IDictionary<string, object>)) {
                if (value is EventBean) {
                    return _mapGetter.GetFragment((EventBean) value);
                }

                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean = EventBeanTypedEventFactory.AdapterForTypedMap(
                (IDictionary<string, object>) value,
                FragmentType);
            return _mapGetter.GetFragment(eventBean);
        }

        public override bool HandleNestedValueExists(object value)
        {
            if (!(value is IDictionary<string, object>)) {
                if (value is EventBean) {
                    return _mapGetter.IsExistsProperty((EventBean) value);
                }

                return false;
            }

            return _mapGetter.IsMapExistsProperty((IDictionary<string, object>) value);
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
                    _mapGetter,
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
                    _mapGetter,
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
                    _mapGetter,
                    CodegenLegoPropertyBeanOrUnd.AccessType.FRAGMENT,
                    GetType()),
                refName);
        }
    }
} // end of namespace