///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    /// Getter for a key property identified by a given key value, using vanilla reflection.
    /// </summary>
    public class KeyedMapMethodPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndMapped
    {
        private readonly MethodInfo _method;
        private readonly object _key;

        public KeyedMapMethodPropertyGetter(
            MethodInfo method,
            object key,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory) : base(
            eventBeanTypedEventFactory,
            beanEventTypeFactory,
            TypeHelper.GetGenericReturnTypeMap(method, false))
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
            var underlying = eventBean.Underlying;
            return GetBeanPropExistsInternal(underlying, _key);
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
                GetBeanPropInternalCodegen(codegenMethodScope, BeanPropType, TargetType, _method, codegenClassScope),
                underlyingExpression,
                Constant(_key));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetBeanPropExistsInternalCodegen(
                    codegenMethodScope,
                    BeanPropType,
                    TargetType,
                    _method,
                    codegenClassScope),
                underlyingExpression,
                Constant(_key));
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
                GetBeanPropInternalCodegen(codegenMethodScope, BeanPropType, TargetType, _method, codegenClassScope),
                CastUnderlying(TargetType, beanExpression),
                key);
        }
        
        
        public object GetBeanPropInternal(
            object @object,
            object key)
        {
            try {
                var result = _method.Invoke(@object, null);
                return GetMapValueChecked(result, key);
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

        public bool GetBeanPropExistsInternal(
            object @object,
            object key)
        {
            try {
                var result = _method.Invoke(@object, null);
                return GetMapKeyExistsChecked(result, key);
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
        
        
        private static CodegenMethod GetBeanPropInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            Type beanPropType,
            Type targetType,
            MethodInfo method,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(beanPropType, typeof(KeyedMapMethodPropertyGetter), codegenClassScope)
                .AddParam(targetType, "@object")
                .AddParam<object>("key")
                .Block
                .DeclareVar(method.ReturnType, "result", ExprDotMethod(Ref("@object"), method.Name))
                .DeclareVar<IDictionary<object, object>>("resultMap",
                    StaticMethod(typeof(CompatExtensions), "AsObjectDictionary", Ref("result")))
                .IfRefNullReturnNull("resultMap")
                .MethodReturn(Cast(beanPropType, ExprDotMethod(Ref("resultMap"), "Get", Ref("key"))));
        }

        private static CodegenMethod GetBeanPropExistsInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            Type beanPropType,
            Type targetType,
            MethodInfo method,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), typeof(KeyedMapMethodPropertyGetter), codegenClassScope)
                .AddParam(targetType, "@object")
                .AddParam<object>("key")
                .Block
                .DeclareVar(method.ReturnType, "result", ExprDotMethod(Ref("@object"), method.Name))
                .DeclareVar<IDictionary<object, object>>(
                    "resultMap",
                    StaticMethod(typeof(CompatExtensions), "AsObjectDictionary", Ref("result")))
                .IfRefNullReturnFalse("resultMap")
                .MethodReturn(ExprDotMethod(Ref("resultMap"), "ContainsKey", Ref("key")));
        }
        
        public override string ToString()
        {
            return $"KeyedMapMethodPropertyGetter  method={_method} key={_key}";
        }
    }
} // end of namespace