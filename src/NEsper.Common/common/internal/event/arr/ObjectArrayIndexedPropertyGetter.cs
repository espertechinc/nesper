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
    ///     Getter for a dynamic indexed property for maps.
    /// </summary>
    public class ObjectArrayIndexedPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _index;
        private readonly int _propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">index to get the element at</param>
        public ObjectArrayIndexedPropertyGetter(
            int propertyIndex,
            int index)
        {
            this._propertyIndex = propertyIndex;
            this._index = index;
        }

        public object GetObjectArray(object[] array)
        {
            return GetObjectArrayIndexValue(array, _propertyIndex, _index);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return IsObjectArrayExistsProperty(array, _propertyIndex, _index);
        }

        public object Get(EventBean eventBean)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsObjectArrayExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean));
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
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
            return ConstantNull();
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                GetType(),
                "GetObjectArrayIndexValue",
                underlyingExpression,
                Constant(_propertyIndex),
                Constant(_index));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                GetType(),
                "IsObjectArrayExistsProperty",
                underlyingExpression,
                Constant(_propertyIndex),
                Constant(_index));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="array">array</param>
        /// <param name="propertyIndex">prop index</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException exception</throws>
        public static object GetObjectArrayIndexValue(
            object[] array,
            int propertyIndex,
            int index)
        {
            var value = array[propertyIndex];
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="array">array</param>
        /// <param name="propertyIndex">prop index</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException exception</throws>
        public static bool IsObjectArrayExistsProperty(
            object[] array,
            int propertyIndex,
            int index)
        {
            var value = array[propertyIndex];
            return BaseNestableEventUtil.IsExistsIndexedValue(value, index);
        }
    }
} // end of namespace