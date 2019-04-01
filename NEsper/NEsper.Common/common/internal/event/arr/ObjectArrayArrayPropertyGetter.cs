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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Getter for Map-entries with well-defined fragment type.
    /// </summary>
    public class ObjectArrayArrayPropertyGetter : ObjectArrayEventPropertyGetterAndIndexed
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EventType fragmentType;
        private readonly int index;
        private readonly int propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">array index</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        public ObjectArrayArrayPropertyGetter(
            int propertyIndex, int index, EventBeanTypedEventFactory eventBeanTypedEventFactory, EventType fragmentType)
        {
            this.propertyIndex = propertyIndex;
            this.index = index;
            this.fragmentType = fragmentType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true;
        }

        public object GetObjectArray(object[] array)
        {
            return GetObjectArrayInternal(array, index);
        }

        public object Get(EventBean eventBean, int index)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArrayInternal(array, index);
        }

        public object Get(EventBean obj)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return GetObjectArray(array);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean obj)
        {
            var fragmentUnderlying = Get(obj);
            return BaseNestableEventUtil.GetBNFragmentNonPojo(
                fragmentUnderlying, fragmentType, eventBeanTypedEventFactory);
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(object[]), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(object[]), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(BaseNestableEventUtil), "getBNArrayValueAtIndexWithNullCheck",
                ArrayAtIndex(underlyingExpression, Constant(propertyIndex)), Constant(index));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var mSvc = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var mType = codegenClassScope.AddFieldUnshared(
                true, typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(fragmentType, EPStatementInitServicesConstants.REF));
            return StaticMethod(
                typeof(BaseNestableEventUtil), "getBNFragmentNonPojo",
                UnderlyingGetCodegen(underlyingExpression, codegenMethodScope, codegenClassScope), mType, mSvc);
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression, CodegenExpression key)
        {
            return StaticMethod(
                typeof(BaseNestableEventUtil), "getBNArrayValueAtIndexWithNullCheck",
                ArrayAtIndex(CastUnderlying(typeof(object[]), beanExpression), Constant(propertyIndex)), key);
        }

        private object GetObjectArrayInternal(object[] array, int index)
        {
            var value = array[propertyIndex];
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }
    }
} // end of namespace