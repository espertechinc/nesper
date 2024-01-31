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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     A getter for use with Map-based events simply returns the value for the key.
    /// </summary>
    public class ObjectArrayEntryPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly BeanEventType eventType;
        private readonly int propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">index</param>
        /// <param name="eventType">type of the entry returned</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        public ObjectArrayEntryPropertyGetter(
            int propertyIndex,
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.propertyIndex = propertyIndex;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.eventType = eventType;
        }

        public object GetObjectArray(object[] array)
        {
            return array[propertyIndex];
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object Get(EventBean obj)
        {
            var arr = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return GetObjectArray(arr);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean eventBean)
        {
            if (eventType == null) {
                return null;
            }

            var result = Get(eventBean);
            return BaseNestableEventUtil.GetBNFragmentPono(result, eventType, eventBeanTypedEventFactory);
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
            return ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (eventType == null) {
                return ConstantNull();
            }

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
            return ArrayAtIndex(underlyingExpression, Constant(propertyIndex));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (eventType == null) {
                return ConstantNull();
            }

            var svc = codegenClassScope.NamespaceScope.AddOrGetDefaultFieldSharable(
                EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var type = codegenClassScope.NamespaceScope.AddDefaultFieldUnshared(
                true,
                typeof(BeanEventType),
                Cast(
                    typeof(BeanEventType),
                    EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF)));
            return StaticMethod(
                typeof(BaseNestableEventUtil),
                "GetBNFragmentPono",
                UnderlyingGetCodegen(underlyingExpression, codegenMethodScope, codegenClassScope),
                type,
                svc);
        }
    }
} // end of namespace