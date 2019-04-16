///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for a key property identified by a given key value, using vanilla reflection.
    /// </summary>
    public class KeyedMapMethodPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndMapped
    {
        private readonly object key;
        private readonly MethodInfo method;

        public KeyedMapMethodPropertyGetter(
            MethodInfo method,
            object key,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                eventBeanTypedEventFactory, beanEventTypeFactory, TypeHelper.GetGenericReturnTypeMap(method, false), null)
        {
            this.key = key;
            this.method = method;
        }

        public object GetBeanProp(object @object)
        {
            return GetBeanPropInternal(@object, key);
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

        public override Type BeanPropType => TypeHelper.GetGenericReturnTypeMap(method, false);

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
            return ConstantTrue();
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetBeanPropInternalCodegen(codegenMethodScope, BeanPropType, TargetType, method, codegenClassScope),
                underlyingExpression, Constant(key));
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
                GetBeanPropInternalCodegen(codegenMethodScope, BeanPropType, TargetType, method, codegenClassScope),
                CastUnderlying(TargetType, beanExpression), key);
        }

        public object GetBeanPropInternal(
            object @object,
            object key)
        {
            try {
                var result = method.Invoke(@object, null);
                if (!(result is IDictionary<object, object>)) {
                    return null;
                }

                var resultMap = (IDictionary<object, object>) result;
                return resultMap.Get(key);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(method, @object, e);
            }
        }

        private static CodegenMethod GetBeanPropInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            Type beanPropType,
            Type targetType,
            MethodInfo method,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(beanPropType, typeof(KeyedMapMethodPropertyGetter), codegenClassScope)
                .AddParam(targetType, "object").AddParam(typeof(object), "key").Block
                .DeclareVar(method.ReturnType, "result", ExprDotMethod(Ref("object"), method.Name))
                .IfRefNotTypeReturnConst("result", typeof(IDictionary<object, object>), null)
                .MethodReturn(
                    Cast(
                        beanPropType,
                        ExprDotMethod(Cast(typeof(IDictionary<object, object>), Ref("result")), "get", Ref("key"))));
        }

        public override string ToString()
        {
            return "KeyedMapMethodPropertyGetter " +
                   " method=" + method +
                   " key=" + key;
        }
    }
} // end of namespace