///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    /// Getter for an array property identified by a given index, using vanilla reflection.
    /// </summary>
    public class ArrayMethodPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndIndexed
    {
        private readonly MethodInfo _method;
        private readonly int index;

        public ArrayMethodPropertyGetter(
            MethodInfo method,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory) : base(
            eventBeanTypedEventFactory,
            beanEventTypeFactory,
            method.ReturnType.GetComponentType())
        {
            this.index = index;
            _method = method;
            if (index < 0) {
                throw new ArgumentException("Invalid negative index value");
            }
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        public object GetBeanProp(object @object)
        {
            return GetBeanPropInternal(@object, index);
        }

        private object GetBeanPropInternal(
            object @object,
            int index)
        {
            try {
                var value = (Array) _method.Invoke(@object, null);
                return CollectionUtil.ArrayValueAtIndex(value, index);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_method, @object, e);
            }
            catch (TargetException e) {
                throw PropertyUtility.GetTargetException(_method, e);
            }
            catch (TargetInvocationException e) {
                throw PropertyUtility.GetTargetException(_method, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetMemberAccessException(_method, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(_method, e);
            }
        }

        private bool GetBeanPropInternalExists(
            object @object,
            int index)
        {
            try {
                var value = _method.Invoke(@object, null);
                return CollectionUtil.ArrayExistsAtIndex((Array) value, index);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_method, @object, e);
            }
            catch (TargetException e) {
                throw PropertyUtility.GetTargetException(_method, e);
            }
            catch (TargetInvocationException e) {
                throw PropertyUtility.GetTargetException(_method, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetMemberAccessException(_method, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(_method, e);
            }
        }

        internal static CodegenMethod GetBeanPropInternalCode(
            CodegenMethodScope codegenMethodScope,
            MethodInfo method,
            CodegenClassScope codegenClassScope)
        {
            var returnType = method.ReturnType;
            var boxedComponent = returnType.GetComponentType().GetBoxedType();
            var declaringClass = method.DeclaringType;
            return codegenMethodScope.MakeChild(boxedComponent, typeof(ArrayMethodPropertyGetter), codegenClassScope)
                .AddParam(declaringClass, "obj")
                .AddParam<int>("index")
                .Block
                .DeclareVar(returnType, "array", ExprDotMethod(Ref("obj"), method.Name))
                .IfRefNullReturnNull("array")
                .IfConditionReturnConst(
                    Relational(
                        ArrayLength(Ref("array")),
                        CodegenExpressionRelational.CodegenRelational.LE,
                        Ref("index")),
                    null)
                .MethodReturn(ArrayAtIndex(Ref("array"), Ref("index")));
        }

        internal static CodegenMethod GetBeanPropInternalExistsCode(
            CodegenMethodScope codegenMethodScope,
            MethodInfo method,
            CodegenClassScope codegenClassScope)
        {
            var returnType = method.ReturnType;
            var declaringClass = method.DeclaringType;
            return codegenMethodScope.MakeChild(typeof(bool), typeof(ArrayMethodPropertyGetter), codegenClassScope)
                .AddParam(declaringClass, "obj")
                .AddParam<int>("index")
                .Block
                .DeclareVar(returnType, "array", ExprDotMethod(Ref("obj"), method.Name))
                .IfRefNullReturnFalse("array")
                .IfConditionReturnConst(
                    Relational(
                        ArrayLength(Ref("array")),
                        CodegenExpressionRelational.CodegenRelational.LE,
                        Ref("index")),
                    false)
                .MethodReturn(ConstantTrue());
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

        public override string ToString()
        {
            return "ArrayMethodPropertyGetter " +
                   " method=" +
                   _method.ToString() +
                   " index=" +
                   index;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            var underlying = eventBean.Underlying;
            return GetBeanPropInternalExists(underlying, index);
        }

        public override Type TargetType => _method.DeclaringType;

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(TargetType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(TargetType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetBeanPropInternalCode(codegenMethodScope, _method, codegenClassScope),
                underlyingExpression,
                Constant(index));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetBeanPropInternalExistsCode(codegenMethodScope, _method, codegenClassScope),
                underlyingExpression,
                Constant(index));
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return LocalMethod(
                GetBeanPropInternalCode(codegenMethodScope, _method, codegenClassScope),
                CastUnderlying(TargetType, beanExpression),
                key);
        }
    }
} // end of namespace