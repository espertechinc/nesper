///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for an array property identified by a given index, using vanilla reflection.
    /// </summary>
    public class ArrayMethodPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndIndexed
    {
        private readonly int index;
        private readonly MethodInfo method;

        public ArrayMethodPropertyGetter(
            MethodInfo method,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory, method.ReturnType.GetElementType(), null)
        {
            this.index = index;
            this.method = method;

            if (index < 0) {
                throw new ArgumentException("Invalid negative index value");
            }
        }

        public object GetBeanProp(object @object)
        {
            return GetBeanPropInternal(@object, index);
        }

        public bool IsBeanExistsProperty(object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override object Get(EventBean obj)
        {
            var underlying = obj.Underlying;
            return GetBeanProp(underlying);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => method.ReturnType.GetElementType();

        public override Type TargetType => method.DeclaringType;

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(TargetType, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public override CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(TargetType, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetBeanPropInternalCode(codegenMethodScope, method, codegenClassScope), underlyingExpression,
                Constant(index));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        private object GetBeanPropInternal(
            object @object,
            int index)
        {
            try {
                var value = (Array) method.Invoke(@object, null);
                if (value.Length <= index) {
                    return null;
                }

                return value.GetValue(index);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(method, @object, e);
            }
        }

        protected internal static CodegenMethod GetBeanPropInternalCode(
            CodegenMethodScope codegenMethodScope,
            MethodInfo method,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(
                    method.ReturnType.GetElementType().GetBoxedType(),
                    typeof(ArrayMethodPropertyGetter),
                    codegenClassScope)
                .AddParam(method.DeclaringType, "obj")
                .AddParam(typeof(int), "index")
                .Block
                .DeclareVar(method.ReturnType, "array", ExprDotMethod(Ref("obj"), method.Name))
                .IfConditionReturnConst(
                    Relational(
                        ArrayLength(Ref("array")), CodegenExpressionRelational.CodegenRelational.LE, Ref("index")),
                    null)
                .MethodReturn(ArrayAtIndex(Ref("array"), Ref("index")));
        }

        public override string ToString()
        {
            return "ArrayMethodPropertyGetter " +
                   " method=" + method +
                   " index=" + index;
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return LocalMethod(
                GetBeanPropInternalCode(codegenMethodScope, method, codegenClassScope),
                CastUnderlying(TargetType, beanExpression), key);
        }
    }
} // end of namespace