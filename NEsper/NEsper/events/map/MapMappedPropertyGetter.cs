///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>Getter for a dynamic mappeds property for maps.</summary>
    public class MapMappedPropertyGetter : MapEventPropertyGetter
        , MapEventPropertyGetterAndMapped
    {
        private readonly string _fieldName;
        private readonly string _key;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="fieldName">property name</param>
        /// <param name="key">get the element at</param>
        public MapMappedPropertyGetter(string fieldName, string key)
        {
            _key = key;
            _fieldName = fieldName;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return GetMapMappedValue(map, _fieldName, _key);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return IsMapExistsProperty(map, _fieldName, _key);
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

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "GetMapMappedValue", underlyingExpression,
                _fieldName, _key);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "IsMapExistsProperty",
                underlyingExpression, _fieldName, _key);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantNull();
        }

        public object Get(EventBean eventBean, string mapKey)
        {
            var data = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMapMappedValue(data, _fieldName, mapKey);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">map</param>
        /// <param name="fieldName">field</param>
        /// <param name="providedKey">key</param>
        /// <exception cref="PropertyAccessException">exception</exception>
        /// <returns>value</returns>
        public static object GetMapMappedValue(IDictionary<string, object> map, string fieldName, string providedKey)
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
        /// <exception cref="PropertyAccessException">exception</exception>
        /// <returns>value</returns>
        public static bool IsMapExistsProperty(IDictionary<string, object> map, string fieldName, string key)
        {
            var value = map.Get(fieldName);
            return BaseNestableEventUtil.GetMappedPropertyExists(value, key);
        }
    }
} // end of namespace