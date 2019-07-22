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
    ///     Getter for a dynamic mappeds property for maps.
    /// </summary>
    public class MapMappedPropertyGetter : MapEventPropertyGetter,
        MapEventPropertyGetterAndMapped
    {
        private readonly string fieldName;
        private readonly string key;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="fieldName">property name</param>
        /// <param name="key">get the element at</param>
        public MapMappedPropertyGetter(
            string fieldName,
            string key)
        {
            this.key = key;
            this.fieldName = fieldName;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return GetMapMappedValue(map, fieldName, key);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return IsMapExistsProperty(map, fieldName, key);
        }

        public object Get(EventBean eventBean)
        {
            var data = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMap(data);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var data = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return IsMapExistsProperty(data);
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
                CastUnderlying(typeof(IDictionary<object, object>), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(IDictionary<object, object>), beanExpression),
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
                "getMapMappedValue",
                underlyingExpression,
                Constant(fieldName),
                Constant(key));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                GetType(),
                "isMapExistsProperty",
                underlyingExpression,
                Constant(fieldName),
                Constant(key));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public object Get(
            EventBean eventBean,
            string mapKey)
        {
            var data = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMapMappedValue(data, fieldName, mapKey);
        }

        public CodegenExpression EventBeanGetMappedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return StaticMethod(
                GetType(),
                "getMapMappedValue",
                CastUnderlying(typeof(IDictionary<object, object>), beanExpression),
                Constant(fieldName),
                key);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">map</param>
        /// <param name="fieldName">field</param>
        /// <param name="providedKey">key</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException exception</throws>
        public static object GetMapMappedValue(
            IDictionary<string, object> map,
            string fieldName,
            string providedKey)
        {
            var value = map.Get(fieldName);
            return BaseNestableEventUtil.GetMappedPropertyValue(value, providedKey);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">map</param>
        /// <param name="fieldName">field</param>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException exception</throws>
        public static bool IsMapExistsProperty(
            IDictionary<string, object> map,
            string fieldName,
            string key)
        {
            var value = map.Get(fieldName);
            return BaseNestableEventUtil.GetMappedPropertyExists(value, key);
        }
    }
} // end of namespace