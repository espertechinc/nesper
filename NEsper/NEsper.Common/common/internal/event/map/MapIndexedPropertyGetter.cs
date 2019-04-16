///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Getter for a dynamic indexed property for maps.
    /// </summary>
    public class MapIndexedPropertyGetter : MapEventPropertyGetter
    {
        private readonly string fieldName;
        private readonly int index;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="fieldName">property name</param>
        /// <param name="index">index to get the element at</param>
        public MapIndexedPropertyGetter(
            string fieldName,
            int index)
        {
            this.index = index;
            this.fieldName = fieldName;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return GetMapIndexedValue(map, fieldName, index);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return GetMapIndexedExists(map, fieldName, index);
        }

        public object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsMapExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
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
                CastUnderlying(typeof(IDictionary<object, object>), beanExpression), codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(IDictionary<object, object>), beanExpression), codegenMethodScope,
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
                GetType(), "getMapIndexedValue", underlyingExpression, Constant(fieldName), Constant(index));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                GetType(), "getMapIndexedExists", underlyingExpression, Constant(fieldName), Constant(index));
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
        /// <param name="map">map</param>
        /// <param name="fieldName">name</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException exception</throws>
        public static object GetMapIndexedValue(
            IDictionary<string, object> map,
            string fieldName,
            int index)
        {
            var value = map.Get(fieldName);
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">map</param>
        /// <param name="fieldName">name</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException exception</throws>
        public static bool GetMapIndexedExists(
            IDictionary<string, object> map,
            string fieldName,
            int index)
        {
            var value = map.Get(fieldName);
            return BaseNestableEventUtil.IsExistsIndexedValue(value, index);
        }
    }
} // end of namespace