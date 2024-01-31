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
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    public abstract class ObjectArrayNestedEntryPropertyGetterBase : ObjectArrayEventPropertyGetter
    {
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly EventType _fragmentType;
        private readonly int _propertyIndex;

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => _eventBeanTypedEventFactory;

        public EventType FragmentType => _fragmentType;

        public int PropertyIndex => _propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">the property to look at</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        public ObjectArrayNestedEntryPropertyGetterBase(
            int propertyIndex,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            _propertyIndex = propertyIndex;
            _fragmentType = fragmentType;
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public object GetObjectArray(object[] array)
        {
            var value = array[_propertyIndex];
            if (value == null) {
                return null;
            }

            return HandleNestedValue(value);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            var value = array[_propertyIndex];
            if (value == null) {
                return false;
            }

            return HandleNestedValueExists(value);
        }

        public object Get(EventBean obj)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            var value = array[_propertyIndex];
            if (value == null) {
                return false;
            }

            return HandleNestedValueExists(value);
        }

        public object GetFragment(EventBean obj)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            var value = array[_propertyIndex];
            if (value == null) {
                return null;
            }

            return HandleNestedValueFragment(value);
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(object[]), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(object[]), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(object[]), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(IsExistsPropertyCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public abstract object HandleNestedValue(object value);

        public abstract bool HandleNestedValueExists(object value);

        public abstract object HandleNestedValueFragment(object value);

        public abstract CodegenExpression HandleNestedValueCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public abstract CodegenExpression HandleNestedValueExistsCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public abstract CodegenExpression HandleNestedValueFragmentCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        private CodegenMethod GetCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<object[]>("array")
                .Block
                .DeclareVar<object>("value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnNull("value")
                .MethodReturn(HandleNestedValueCodegen(Ref("value"), codegenMethodScope, codegenClassScope));
        }

        private CodegenMethod IsExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam<object[]>("array")
                .Block
                .DeclareVar<object>("value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnFalse("value")
                .MethodReturn(HandleNestedValueExistsCodegen(Ref("value"), codegenMethodScope, codegenClassScope));
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<object[]>("array")
                .Block
                .DeclareVar<object>("value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnFalse("value")
                .MethodReturn(HandleNestedValueFragmentCodegen(Ref("value"), codegenMethodScope, codegenClassScope));
        }
    }
} // end of namespace