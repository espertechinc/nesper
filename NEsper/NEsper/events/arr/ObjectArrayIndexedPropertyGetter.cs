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
    /// <summary>Getter for a dynamic indexed property for maps.</summary>
    public class ObjectArrayIndexedPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly int _index;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="array">array</param>
        /// <param name="propertyIndex">prop index</param>
        /// <param name="index">index</param>
        /// <exception cref="PropertyAccessException">exception</exception>
        /// <returns>value</returns>
        public static Object GetObjectArrayIndexValue(Object[] array, int propertyIndex, int index)
        {
            Object value = array[propertyIndex];
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="array">array</param>
        /// <param name="propertyIndex">prop index</param>
        /// <param name="index">index</param>
        /// <exception cref="PropertyAccessException">exception</exception>
        /// <returns>value</returns>
        public static bool IsObjectArrayExistsProperty(Object[] array, int propertyIndex, int index)
        {
            Object value = array[propertyIndex];
            return BaseNestableEventUtil.IsExistsIndexedValue(value, index);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">index to get the element at</param>
        public ObjectArrayIndexedPropertyGetter(int propertyIndex, int index)
        {
            this._propertyIndex = propertyIndex;
            this._index = index;
        }

        public Object GetObjectArray(Object[] array)
        {
            return GetObjectArrayIndexValue(array, _propertyIndex, _index);
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return IsObjectArrayExistsProperty(array, _propertyIndex, _index);
        }

        public Object Get(EventBean eventBean)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsObjectArrayExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean));
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
            return StaticMethodTakingExprAndConst(GetType(), "GetObjectArrayIndexValue", underlyingExpression, _propertyIndex, _index);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "IsObjectArrayExistsProperty", underlyingExpression, _propertyIndex, _index);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace