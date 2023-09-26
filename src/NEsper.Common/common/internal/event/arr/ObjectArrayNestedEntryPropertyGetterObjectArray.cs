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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayNestedEntryPropertyGetterObjectArray : ObjectArrayNestedEntryPropertyGetterBase
    {
        private readonly ObjectArrayEventPropertyGetter arrayGetter;

        public ObjectArrayNestedEntryPropertyGetterObjectArray(
            int propertyIndex,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ObjectArrayEventPropertyGetter arrayGetter)
            : base(propertyIndex, fragmentType, eventBeanTypedEventFactory)
        {
            this.arrayGetter = arrayGetter;
        }

        public override object HandleNestedValue(object value)
        {
            if (!(value is object[] objects)) {
                if (value is EventBean bean) {
                    return arrayGetter.Get(bean);
                }

                return null;
            }

            return arrayGetter.GetObjectArray(objects);
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
            EventBean eventBean = EventBeanTypedEventFactory.AdapterForTypedObjectArray(objects, FragmentType);
            return arrayGetter.GetFragment(eventBean);
        }

        public override bool HandleNestedValueExists(object value)
        {
            if (!(value is object[] objects)) {
                if (value is EventBean bean) {
                    return arrayGetter.IsExistsProperty(bean);
                }

                return false;
            }

            return arrayGetter.IsObjectArrayExistsProperty(objects);
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GenerateMethod(codegenMethodScope, codegenClassScope, CodegenLegoPropertyBeanOrUnd.AccessType.GET),
                name);
        }

        public override CodegenExpression HandleNestedValueExistsCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GenerateMethod(codegenMethodScope, codegenClassScope, CodegenLegoPropertyBeanOrUnd.AccessType.EXISTS),
                refName);
        }

        public override CodegenExpression HandleNestedValueFragmentCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GenerateMethod(codegenMethodScope, codegenClassScope, CodegenLegoPropertyBeanOrUnd.AccessType.FRAGMENT),
                refName);
        }

        private CodegenMethod GenerateMethod(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenLegoPropertyBeanOrUnd.AccessType accessType)
        {
            return CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope,
                codegenClassScope,
                typeof(object[]),
                arrayGetter,
                accessType,
                GetType());
        }
    }
} // end of namespace