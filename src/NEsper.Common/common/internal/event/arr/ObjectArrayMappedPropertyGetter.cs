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
    /// <summary>
    ///     Getter for a dynamic mappeds property for maps.
    /// </summary>
    public class ObjectArrayMappedPropertyGetter : ObjectArrayEventPropertyGetterAndMapped
    {
        private readonly string key;
        private readonly int propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="key">get the element at</param>
        public ObjectArrayMappedPropertyGetter(
            int propertyIndex,
            string key)
        {
            this.propertyIndex = propertyIndex;
            this.key = key;
        }

        public object GetObjectArray(object[] array)
        {
            return GetOAMapValue(array, propertyIndex, key);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return GetOAMapExists(array, propertyIndex, key);
        }

        public object Get(
            EventBean eventBean,
            string mapKey)
        {
            var data = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetOAMapValue(data, propertyIndex, mapKey);
        }

        public object Get(EventBean eventBean)
        {
            var data = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetOAMapValue(data, propertyIndex, key);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var data = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetOAMapExists(data, propertyIndex, key);
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
                "GetOAMapValue",
                underlyingExpression,
                Constant(propertyIndex),
                Constant(key));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                GetType(),
                "GetOAMapExists",
                underlyingExpression,
                Constant(propertyIndex),
                Constant(key));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EventBeanGetMappedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return StaticMethod(
                GetType(),
                "GetOAMapValue",
                CastUnderlying(typeof(object[]), beanExpression),
                Constant(propertyIndex),
                key);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="objectArray">data</param>
        /// <param name="propertyIndex">prop index</param>
        /// <param name="providedKey">key</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException exception</throws>
        public static object GetOAMapValue(
            object[] objectArray,
            int propertyIndex,
            string providedKey)
        {
            var value = objectArray[propertyIndex];
            return BaseNestableEventUtil.GetMappedPropertyValue(value, providedKey);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="objectArray">data</param>
        /// <param name="propertyIndex">prop index</param>
        /// <param name="providedKey">key</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException exception</throws>
        public static bool GetOAMapExists(
            object[] objectArray,
            int propertyIndex,
            string providedKey)
        {
            var value = objectArray[propertyIndex];
            return BaseNestableEventUtil.GetMappedPropertyExists(value, providedKey);
        }
    }
} // end of namespace