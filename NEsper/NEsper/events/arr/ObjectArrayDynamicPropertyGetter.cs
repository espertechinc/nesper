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
    /// <summary>
    /// Getter for a dynamic property (syntax field.inner?), using vanilla reflection.
    /// </summary>
    public class ObjectArrayDynamicPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly string _propertyName;

        public ObjectArrayDynamicPropertyGetter(string propertyName)
        {
            _propertyName = propertyName;
        }

        public Object GetObjectArray(Object[] array)
        {
            return null;
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return false;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="eventBean">The event bean.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Object GetOADynamicProp(EventBean eventBean, String propertyName)
        {
            var objectArrayEventType = (ObjectArrayEventType)eventBean.EventType;

            int index;
            if (objectArrayEventType.PropertiesIndexes.TryGetValue(propertyName, out index))
            {
                var theEvent = (Object[])eventBean.Underlying;
                return theEvent[index];
            }

            return null;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        public static bool IsExistsOADynamicProp(EventBean eventBean, String propertyName)
        {
            var objectArrayEventType = (ObjectArrayEventType)eventBean.EventType;
            return objectArrayEventType.PropertiesIndexes.ContainsKey(propertyName);
        }

        public Object Get(EventBean eventBean)
        {
            return GetOADynamicProp(eventBean, _propertyName);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsOADynamicProp(eventBean, _propertyName);
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return StaticMethod(GetType(), "GetOADynamicProp", beanExpression, Constant(_propertyName));
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return StaticMethod(GetType(), "IsExistsOADynamicProp", beanExpression, Constant(_propertyName));
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantFalse();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace
