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
    /// A getter for use with Map-based events simply returns the value for the key.
    /// </summary>
    public class ObjectArrayEventBeanPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">property to get</param>
        public ObjectArrayEventBeanPropertyGetter(int propertyIndex)
        {
            this._propertyIndex = propertyIndex;
        }

        public Object GetObjectArray(Object[] array)
        {
            var eventBean = array[_propertyIndex];
            if (eventBean == null)
            {
                return null;
            }

            var theEvent = (EventBean)eventBean;
            return theEvent.Underlying;
        }

        private string GetObjectArrayCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(Object[]), "array", this.GetType())
                .DeclareVar(typeof(Object), "eventBean", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnNull("eventBean")
                .MethodReturn(ExprDotUnderlying(Cast(typeof(EventBean), Ref("eventBean"))));
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object Get(EventBean obj)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object GetFragment(EventBean obj)
        {
            return BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj)[_propertyIndex];
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetObjectArrayCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ArrayAtIndex(underlyingExpression, Constant(_propertyIndex));
        }
    }
} // end of namespace