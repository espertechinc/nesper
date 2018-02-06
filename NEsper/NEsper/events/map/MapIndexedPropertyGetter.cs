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

    /// <summary>Getter for a dynamic indexed property for maps.</summary>
    public class MapIndexedPropertyGetter : MapEventPropertyGetter
    {
        private readonly string _fieldName;
        private readonly int _index;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="fieldName">property name</param>
        /// <param name="index">index to get the element at</param>
        public MapIndexedPropertyGetter(string fieldName, int index)
        {
            _index = index;
            _fieldName = fieldName;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return GetMapIndexedValue(map, _fieldName, _index);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return GetMapIndexedExists(map, _fieldName, _index);
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
            return StaticMethodTakingExprAndConst(GetType(), "GetMapIndexedValue", underlyingExpression,
                _fieldName, _index);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "GetMapIndexedExists",
                underlyingExpression, _fieldName, _index);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantNull();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">map</param>
        /// <param name="fieldName">name</param>
        /// <param name="index">index</param>
        /// <exception cref="PropertyAccessException">exception</exception>
        /// <returns>value</returns>
        public static object GetMapIndexedValue(IDictionary<string, object> map, string fieldName, int index)
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
        /// <exception cref="PropertyAccessException">exception</exception>
        /// <returns>value</returns>
        public static bool GetMapIndexedExists(IDictionary<string, object> map, string fieldName, int index)
        {
            var value = map.Get(fieldName);
            return BaseNestableEventUtil.IsExistsIndexedValue(value, index);
        }
    }
} // end of namespace