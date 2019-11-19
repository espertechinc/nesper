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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Getter for map entry.
    /// </summary>
    public abstract class ObjectArrayPropertyGetterDefaultBase : ObjectArrayEventPropertyGetter
    {
        internal readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        internal readonly EventType fragmentEventType;
        private readonly int propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        public ObjectArrayPropertyGetterDefaultBase(
            int propertyIndex,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.propertyIndex = propertyIndex;
            this.fragmentEventType = fragmentEventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public object GetObjectArray(object[] array)
        {
            return array[propertyIndex];
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return array.Length > propertyIndex;
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

        public object GetFragment(EventBean eventBean)
        {
            var value = Get(eventBean);
            return HandleCreateFragment(value);
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
            if (fragmentEventType == null) {
                return ConstantNull();
            }

            return LocalMethod(
                GetFragmentCodegen(underlyingExpression, codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        internal abstract object HandleCreateFragment(object value);

        internal abstract CodegenExpression HandleCreateFragmentCodegen(
            CodegenExpression value,
            CodegenClassScope codegenClassScope);

        private CodegenMethod GetFragmentCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "oa")
                .Block
                .DeclareVar<object>(
                    "value",
                    UnderlyingGetCodegen(Ref("oa"), codegenMethodScope, codegenClassScope))
                .MethodReturn(HandleCreateFragmentCodegen(Ref("value"), codegenClassScope));
        }
    }
} // end of namespace