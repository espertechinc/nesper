///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class MapNestedEntryPropertyGetterObjectArray : MapNestedEntryPropertyGetterBase
    {
        private readonly ObjectArrayEventPropertyGetter arrayGetter;

        public MapNestedEntryPropertyGetterObjectArray(
            string propertyMap,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ObjectArrayEventPropertyGetter arrayGetter)
            : base(propertyMap, fragmentType, eventBeanTypedEventFactory)
        {
            this.arrayGetter = arrayGetter;
        }

        public override object HandleNestedValue(object value)
        {
            if (value is object[] arrayValue) {
                return arrayGetter.GetObjectArray(arrayValue);
            }

            if (value is EventBean eventBean) {
                return arrayGetter.Get(eventBean);
            }

            return null;
        }


        public override bool HandleNestedValueExists(object value)
        {
            if (value is object[] arrayValue) {
                return arrayGetter.IsObjectArrayExistsProperty(arrayValue);
            }

            if (value is ObjectArrayBackedEventBean eventBean) {
                return arrayGetter.IsObjectArrayExistsProperty(eventBean.Properties);
            }

            return false;
        }

        public override object HandleNestedValueFragment(object value)
        {
            if (!(value is object[] objects)) {
                if (value is EventBean bean) {
                    return arrayGetter.GetFragment(bean);
                }

                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean = eventBeanTypedEventFactory.AdapterForTypedObjectArray(objects, fragmentType);
            return arrayGetter.GetFragment(eventBean);
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope,
                codegenClassScope,
                typeof(object[]),
                arrayGetter,
                CodegenLegoPropertyBeanOrUnd.AccessType.GET,
                GetType());
            return LocalMethod(method, name);
        }


        public override CodegenExpression HandleNestedValueExistsCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope,
                codegenClassScope,
                typeof(object[]),
                arrayGetter,
                CodegenLegoPropertyBeanOrUnd.AccessType.EXISTS,
                GetType());
            return LocalMethod(method, name);
        }

        public override CodegenExpression HandleNestedValueFragmentCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope,
                codegenClassScope,
                typeof(object[]),
                arrayGetter,
                CodegenLegoPropertyBeanOrUnd.AccessType.FRAGMENT,
                GetType());
            return LocalMethod(method, name);
        }
    }
} // end of namespace