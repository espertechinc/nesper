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
    /// Returns the event bean or the underlying array.
    /// </summary>
    public class ObjectArrayEventBeanArrayPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly Type _underlyingType;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property to get</param>
        /// <param name="underlyingType">type of property</param>
        public ObjectArrayEventBeanArrayPropertyGetter(int propertyIndex, Type underlyingType)
        {
            _propertyIndex = propertyIndex;
            _underlyingType = underlyingType;
        }

        public Object GetObjectArray(Object[] oa)
        {
            Object inner = oa[_propertyIndex];
            return BaseNestableEventUtil.GetArrayPropertyAsUnderlyingsArray(_underlyingType, (EventBean[]) inner);
        }

        private String GetObjectArrayCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(Object[]), "oa", GetType())
                .DeclareVar(typeof(Object), "inner", ArrayAtIndex(Ref("oa"), Constant(_propertyIndex)))
                .MethodReturn(LocalMethod(
                    BaseNestableEventUtil.GetArrayPropertyAsUnderlyingsArrayCodegen(_underlyingType, context),
                    Cast(typeof(EventBean[]),
                    Ref("inner"))));
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public Object Get(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public Object GetFragment(EventBean obj)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return array[_propertyIndex];
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
}
