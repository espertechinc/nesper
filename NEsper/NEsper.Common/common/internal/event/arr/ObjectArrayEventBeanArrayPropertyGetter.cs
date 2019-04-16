///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Returns the event bean or the underlying array.
    /// </summary>
    public class ObjectArrayEventBeanArrayPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int propertyIndex;
        private readonly Type underlyingType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">property to get</param>
        /// <param name="underlyingType">type of property</param>
        public ObjectArrayEventBeanArrayPropertyGetter(
            int propertyIndex,
            Type underlyingType)
        {
            this.propertyIndex = propertyIndex;
            this.underlyingType = underlyingType;
        }

        public object GetObjectArray(object[] oa)
        {
            var inner = oa[propertyIndex];
            return BaseNestableEventUtil.GetArrayPropertyAsUnderlyingsArray(underlyingType, (EventBean[]) inner);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object Get(EventBean obj)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return GetObjectArray(array);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean obj)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return array[propertyIndex];
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(object[]), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(object[]), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetObjectArrayCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ArrayAtIndex(underlyingExpression, Constant(propertyIndex));
        }

        private CodegenMethod GetObjectArrayCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "oa").Block
                .DeclareVar(typeof(object), "inner", ArrayAtIndex(Ref("oa"), Constant(propertyIndex)))
                .MethodReturn(
                    LocalMethod(
                        BaseNestableEventUtil.GetArrayPropertyAsUnderlyingsArrayCodegen(
                            underlyingType, codegenMethodScope, codegenClassScope),
                        Cast(typeof(EventBean[]), Ref("inner"))));
        }
    }
} // end of namespace