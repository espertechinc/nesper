///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.arr
{
    /// <summary>Getter for a dynamic mappeds property for maps.</summary>
    public class ObjectArrayMappedPropertyGetter : ObjectArrayEventPropertyGetterAndMapped
    {
        private readonly int _propertyIndex;
        private readonly string _key;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="objectArray">data</param>
        /// <param name="propertyIndex">prop index</param>
        /// <param name="providedKey">key</param>
        /// <exception cref="PropertyAccessException">exception</exception>
        /// <returns>value</returns>
        public static Object GetOAMapValue(Object[] objectArray, int propertyIndex, string providedKey)
        {
            Object value = objectArray[propertyIndex];
            return BaseNestableEventUtil.GetMappedPropertyValue(value, providedKey);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="objectArray">data</param>
        /// <param name="propertyIndex">prop index</param>
        /// <param name="providedKey">key</param>
        /// <exception cref="PropertyAccessException">exception</exception>
        /// <returns>value</returns>
        public static bool GetOAMapExists(Object[] objectArray, int propertyIndex, string providedKey)
        {
            Object value = objectArray[propertyIndex];
            return BaseNestableEventUtil.GetMappedPropertyExists(value, providedKey);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="key">get the element at</param>
        public ObjectArrayMappedPropertyGetter(int propertyIndex, string key)
        {
            this._propertyIndex = propertyIndex;
            this._key = key;
        }

        public Object GetObjectArray(Object[] array)
        {
            return GetOAMapValue(array, _propertyIndex, _key);
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return GetOAMapExists(array, _propertyIndex, _key);
        }

        public Object Get(EventBean eventBean, string mapKey)
        {
            Object[] data = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetOAMapValue(data, _propertyIndex, mapKey);
        }

        public Object Get(EventBean eventBean)
        {
            Object[] data = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetOAMapValue(data, _propertyIndex, _key);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            Object[] data = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetOAMapExists(data, _propertyIndex, _key);
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "GetOAMapValue", underlyingExpression, _propertyIndex, _key);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "GetOAMapExists", underlyingExpression, _propertyIndex, _key);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace