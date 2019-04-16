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
    public class KeyedMapFieldPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndMapped
    {
        private readonly FieldInfo field;
        private readonly object key;

        public KeyedMapFieldPropertyGetter(
            FieldInfo field,
            object key,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                eventBeanTypedEventFactory, beanEventTypeFactory, TypeHelper.GetGenericFieldTypeMap(field, false), null)
        {
            this.key = key;
            this.field = field;
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

        public override Type BeanPropType => TypeHelper.GetGenericFieldTypeMap(field, false);

        public override Type TargetType => field.DeclaringType;

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
                GetBeanPropInternalCodegen(codegenMethodScope, codegenClassScope), underlyingExpression, Constant(key));
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
                GetBeanPropInternalCodegen(codegenMethodScope, codegenClassScope),
                CastUnderlying(TargetType, beanExpression), key);
        }

        public object GetBeanPropInternal(
            object @object,
            object key)
        {
            try {
                object result = field.Get(@object);
                if (!(result is IDictionary<object, object>)) {
                    return null;
                }

                var resultMap = (IDictionary<object, object>) result;
                return resultMap.Get(key);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(field, @object, e);
            }
        }

        private CodegenMethod GetBeanPropInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(BeanPropType, GetType(), codegenClassScope)
                .AddParam(TargetType, "object").AddParam(typeof(object), "key").Block
                .DeclareVar(typeof(object), "result", ExprDotName(Ref("object"), field.Name))
                .IfRefNotTypeReturnConst("result", typeof(IDictionary<object, object>), null)
                .DeclareVarWCast(typeof(IDictionary<object, object>), "map", "result")
                .MethodReturn(Cast(BeanPropType, ExprDotMethod(Ref("map"), "get", Ref("key"))));
        }

        public override string ToString()
        {
            return "KeyedMapFieldPropertyGetter " +
                   " field=" + field +
                   " key=" + key;
        }
    }
} // end of namespace