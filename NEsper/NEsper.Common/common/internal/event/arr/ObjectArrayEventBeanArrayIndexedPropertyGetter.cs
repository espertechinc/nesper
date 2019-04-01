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
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Getter for array events.
    /// </summary>
    public class ObjectArrayEventBeanArrayIndexedPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int index;
        private readonly int propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">array index</param>
        public ObjectArrayEventBeanArrayIndexedPropertyGetter(int propertyIndex, int index)
        {
            this.propertyIndex = propertyIndex;
            this.index = index;
        }

        public object GetObjectArray(object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var wrapper = (EventBean[]) array[propertyIndex];
            return BaseNestableEventUtil.GetBNArrayPropertyUnderlying(wrapper, index);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true;
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
            var wrapper = (EventBean[]) array[propertyIndex];
            return BaseNestableEventUtil.GetBNArrayPropertyBean(wrapper, index);
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(object[]), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(object[]), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetObjectArrayCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        private CodegenMethod GetObjectArrayCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "array").Block
                .DeclareVar(
                    typeof(EventBean[]), "wrapper",
                    Cast(typeof(EventBean[]), ArrayAtIndex(Ref("array"), Constant(propertyIndex))))
                .MethodReturn(
                    StaticMethod(
                        typeof(BaseNestableEventUtil), "getBNArrayPropertyUnderlying", Ref("wrapper"),
                        Constant(index)));
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "array").Block
                .DeclareVar(
                    typeof(EventBean[]), "wrapper",
                    Cast(typeof(EventBean[]), ArrayAtIndex(Ref("array"), Constant(propertyIndex))))
                .MethodReturn(
                    StaticMethod(
                        typeof(BaseNestableEventUtil), "getBNArrayPropertyBean", Ref("wrapper"), Constant(index)));
        }
    }
} // end of namespace