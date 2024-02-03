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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Property getter for methods using vanilla reflection.
    /// </summary>
    public class ReflectionPropMethodGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter
    {
        private readonly Type _propertyType;
        private readonly PropertyInfo _property;
        private readonly MethodInfo _method;
        private readonly Type _targetType;

        public ReflectionPropMethodGetter(
            PropertyInfo property,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                property.PropertyType)
        {
            _property = property;
            _method = property.GetMethod;
            _propertyType = property.PropertyType;
            _targetType = property.DeclaringType;
        }

        public ReflectionPropMethodGetter(
            MethodInfo method,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                method.ReturnType)
        {
            _property = null;
            _method = method;
            _propertyType = method.ReturnType;
            _targetType = method.DeclaringType;
        }

        public object GetBeanProp(object @object)
        {
            try {
                return _method.Invoke(@object, null);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(_method, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetMemberAccessException(_method, e);
            }
            catch (TargetException e) {
                throw PropertyUtility.GetTargetException(_method, e);
            }
        }

        public bool IsBeanExistsProperty(object @object)
        {
            return true;
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

        //public override Type BeanPropType => _propertyType;

        public override Type TargetType => _targetType;

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
            if (_property != null) {
                return ExprDotName(underlyingExpression, _property.Name);
            }

            return ExprDotMethod(underlyingExpression, _method.Name);
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public override string ToString()
        {
            return
                $"{nameof(_propertyType)}: {_propertyType}, {nameof(_property)}: {_property}, {nameof(_method)}: {_method}, {nameof(_targetType)}: {_targetType}";
        }
    }
} // end of namespace