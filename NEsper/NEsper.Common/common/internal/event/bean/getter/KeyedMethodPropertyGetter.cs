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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for a key property identified by a given key value, using vanilla reflection.
    /// </summary>
    public class KeyedMethodPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndMapped,
        EventPropertyGetterAndIndexed
    {
        private readonly object _key;
        private readonly MethodInfo _method;

        public KeyedMethodPropertyGetter(
            MethodInfo method,
            object key,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                method.ReturnType,
                null)
        {
            _key = key;
            _method = method;
        }

        public object GetBeanProp(object @object)
        {
            return GetBeanPropInternal(@object, _key);
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

        public override Type BeanPropType => _method.ReturnType;

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
            return ConstantTrue();
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetBeanPropInternalCodegen(codegenMethodScope, TargetType, _method, codegenClassScope),
                underlyingExpression,
                Constant(_key));
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

        public object Get(
            EventBean eventBean,
            string mapKey)
        {
            return GetBeanPropInternal(eventBean.Underlying, mapKey);
        }

        public CodegenExpression EventBeanGetMappedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return LocalMethod(
                GetBeanPropInternalCodegen(codegenMethodScope, TargetType, _method, codegenClassScope),
                CastUnderlying(TargetType, beanExpression),
                key);
        }

        private object GetBeanPropInternal(
            object @object,
            object key)
        {
            try {
                return _method.Invoke(@object, new[] {key});
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_method, @object, e);
            }
            catch (TargetInvocationException e) {
                throw PropertyUtility.GetTargetException(_method, e);
            }
            catch (TargetException e) {
                throw PropertyUtility.GetTargetException(_method, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetMemberAccessException(_method, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(_method, e);
            }
        }

        protected internal static CodegenMethod GetBeanPropInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            Type targetType,
            MethodInfo method,
            CodegenClassScope codegenClassScope)
        {
            var parameterTypes = method.GetParameterTypes();
            var returnType = method.ReturnType;
            return codegenMethodScope.MakeChild(returnType, typeof(KeyedMethodPropertyGetter), codegenClassScope)
                .AddParam(targetType, "@object")
                .AddParam(parameterTypes[0], "key")
                .Block
                .DebugStack()
                .MethodReturn(ExprDotMethod(Ref("@object"), method.Name, Ref("key")));
        }

        public override string ToString()
        {
            return "KeyedMethodPropertyGetter " +
                   " method=" +
                   _method +
                   " key=" +
                   _key;
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return LocalMethod(
                GetBeanPropInternalCodegen(codegenMethodScope, TargetType, _method, codegenClassScope),
                CastUnderlying(TargetType, beanExpression),
                key);
        }
    }
} // end of namespace