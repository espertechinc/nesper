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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Getter for a dynamic property (syntax field.inner?), using vanilla reflection.
    /// </summary>
    public class ObjectArrayDynamicPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly string propertyName;

        public ObjectArrayDynamicPropertyGetter(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public object GetObjectArray(object[] array)
        {
            return null;
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return false;
        }

        public object Get(EventBean eventBean)
        {
            return GetOADynamicProp(eventBean, propertyName);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsOADynamicProp(eventBean, propertyName);
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
            return StaticMethod(GetType(), "getOADynamicProp", beanExpression, Constant(propertyName));
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(GetType(), "isExistsOADynamicProp", beanExpression, Constant(propertyName));
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
            return ConstantNull();
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantFalse();
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
        /// <param name="eventBean">bean</param>
        /// <param name="propertyName">props</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException exception</throws>
        public static object GetOADynamicProp(
            EventBean eventBean,
            string propertyName)
        {
            var objectArrayEventType = (ObjectArrayEventType) eventBean.EventType;
            int? index = objectArrayEventType.PropertiesIndexes[propertyName];
            if (index == null) {
                return null;
            }

            var theEvent = (object[]) eventBean.Underlying;
            return theEvent[index.Value];
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="eventBean">bean</param>
        /// <param name="propertyName">name</param>
        /// <returns>flag</returns>
        public static bool IsExistsOADynamicProp(
            EventBean eventBean,
            string propertyName)
        {
            var objectArrayEventType = (ObjectArrayEventType) eventBean.EventType;
            int? index = objectArrayEventType.PropertiesIndexes[propertyName];
            return index != null;
        }
    }
} // end of namespace