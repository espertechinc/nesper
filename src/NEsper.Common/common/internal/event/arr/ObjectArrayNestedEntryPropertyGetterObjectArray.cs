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
        private readonly ObjectArrayEventPropertyGetter _arrayGetter;

        public ObjectArrayNestedEntryPropertyGetterObjectArray(
            int propertyIndex,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ObjectArrayEventPropertyGetter arrayGetter)
            : base(propertyIndex, fragmentType, eventBeanTypedEventFactory)
        {
            this._arrayGetter = arrayGetter;
        }

        public override object HandleNestedValue(object value)
        {
            if (!(value is object[])) {
                if (value is EventBean) {
                    return _arrayGetter.Get((EventBean) value);
                }

                return null;
            }

            return _arrayGetter.GetObjectArray((object[]) value);
        }

        public override object HandleNestedValueFragment(object value)
        {
            if (!(value is object[])) {
                if (value is EventBean) {
                    return _arrayGetter.GetFragment((EventBean) value);
                }

                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean = EventBeanTypedEventFactory.AdapterForTypedObjectArray((object[]) value, FragmentType);
            return _arrayGetter.GetFragment(eventBean);
        }

        public override bool HandleNestedValueExists(object value)
        {
            if (!(value is object[])) {
                if (value is EventBean) {
                    return _arrayGetter.IsExistsProperty((EventBean) value);
                }

                return false;
            }

            return _arrayGetter.IsObjectArrayExistsProperty((object[]) value);
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
                _arrayGetter,
                accessType,
                GetType());
        }
    }
} // end of namespace