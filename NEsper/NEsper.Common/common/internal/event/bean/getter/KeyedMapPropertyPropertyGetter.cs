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
    public class KeyedMapPropertyPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndMapped
    {
        private readonly PropertyInfo _property;
        private readonly object _key;

        public KeyedMapPropertyPropertyGetter(
            PropertyInfo property,
            object key,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                TypeHelper.GetGenericPropertyTypeMap(property, false),
                null)
        {
            _key = key;
            _property = property;
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

        public override Type BeanPropType => TypeHelper.GetGenericPropertyTypeMap(_property, false);

        public override Type TargetType => _property.DeclaringType;

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
                GetBeanPropInternalCodegen(codegenMethodScope, codegenClassScope),
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
                CastUnderlying(TargetType, beanExpression),
                key);
        }

        public object GetBeanPropInternal(
            object @object,
            object key)
        {
            try {
                var result = _property.GetValue(@object);
                var resultMap = result.AsObjectDictionary();
                return resultMap?.Get(key);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_property, @object, e);
            }
        }

        private CodegenMethod GetBeanPropInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(BeanPropType, GetType(), codegenClassScope)
                .AddParam(TargetType, "@object")
                .AddParam(typeof(object), "key")
                .Block
                .DeclareVar<object>("result", ExprDotName(Ref("@object"), _property.Name))
                .DeclareVar<IDictionary<object, object>>("resultMap", 
                    StaticMethod(typeof(CompatExtensions), "AsObjectDictionary", Ref("result")))
                .IfRefNullReturnNull("resultMap")
                .MethodReturn(Cast(BeanPropType, ExprDotMethod(Ref("resultMap"), "Get", Ref("key"))));
        }

        public override string ToString()
        {
            return $"KeyedMapPropertyPropertyGetter: property={_property} key={_key}";
        }
    }
} // end of namespace