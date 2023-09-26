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
    ///     Getter for an array of event bean using a nested getter.
    /// </summary>
    public class ObjectArrayEventBeanArrayIndexedElementPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int index;
        private readonly EventPropertyGetterSPI nestedGetter;
        private readonly int propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">array index</param>
        /// <param name="nestedGetter">nested getter</param>
        public ObjectArrayEventBeanArrayIndexedElementPropertyGetter(
            int propertyIndex,
            int index,
            EventPropertyGetterSPI nestedGetter)
        {
            this.propertyIndex = propertyIndex;
            this.index = index;
            this.nestedGetter = nestedGetter;
        }

        public object GetObjectArray(object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var wrapper = (EventBean[])array[propertyIndex];
            return BaseNestableEventUtil.GetArrayPropertyValue(wrapper, index, nestedGetter);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object Get(EventBean obj)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean obj)
        {
            var wrapper = (EventBean[])BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj)[propertyIndex];
            return BaseNestableEventUtil.GetArrayPropertyFragment(wrapper, index, nestedGetter);
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
            return LocalMethod(GetObjectArrayCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
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
            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        private CodegenMethod GetObjectArrayCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<object[]>("array")
                .Block
                .DeclareVar<EventBean[]>(
                    "wrapper",
                    Cast(typeof(EventBean[]), ArrayAtIndex(Ref("array"), Constant(propertyIndex))))
                .MethodReturn(
                    LocalMethod(
                        BaseNestableEventUtil.GetArrayPropertyValueCodegen(
                            codegenMethodScope,
                            codegenClassScope,
                            index,
                            nestedGetter),
                        Ref("wrapper")));
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<object[]>("array")
                .Block
                .DeclareVar<EventBean[]>(
                    "wrapper",
                    Cast(typeof(EventBean[]), ArrayAtIndex(Ref("array"), Constant(propertyIndex))))
                .MethodReturn(
                    LocalMethod(
                        BaseNestableEventUtil.GetArrayPropertyFragmentCodegen(
                            codegenMethodScope,
                            codegenClassScope,
                            index,
                            nestedGetter),
                        Ref("wrapper")));
        }
    }
} // end of namespace