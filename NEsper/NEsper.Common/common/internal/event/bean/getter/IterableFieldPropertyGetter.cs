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
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.@event.bean.getter.IterableMethodPropertyGetter;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for an iterable property backed by a field, identified by a given index, using vanilla reflection.
    /// </summary>
    public class IterableFieldPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndIndexed
    {
        private readonly FieldInfo field;
        private readonly int index;

        public IterableFieldPropertyGetter(
            FieldInfo field, int index, EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory) : base(
            eventBeanTypedEventFactory, beanEventTypeFactory, TypeHelper.GetGenericFieldType(field, false), null)
        {
            this.index = index;
            this.field = field;

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

        public override Type BeanPropType => TypeHelper.GetGenericFieldType(field, false);

        public override Type TargetType => field.DeclaringType;

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(TargetType, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public override CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetBeanPropInternalCodegen(codegenMethodScope, codegenClassScope), underlyingExpression,
                Constant(index));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        public object GetBeanPropInternal(object @object, int index)
        {
            try {
                object value = field.Get(@object);
                return GetBeanEventIterableValue(value, index);
            }
            catch (ClassCastException e) {
                throw PropertyUtility.GetMismatchException(field, @object, e);
            }
        }

        private CodegenMethod GetBeanPropInternalCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(BeanPropType, GetType(), codegenClassScope)
                .AddParam(TargetType, "object").AddParam(typeof(int), "index").Block
                .DeclareVar(typeof(object), "value", ExprDotName(Ref("object"), field.Name))
                .MethodReturn(
                    Cast(
                        BeanPropType,
                        StaticMethod(
                            typeof(IterableMethodPropertyGetter), "getBeanEventIterableValue", Ref("value"),
                            Ref("index"))));
        }

        public override string ToString()
        {
            return "IterableFieldPropertyGetter " +
                   " field=" + field +
                   " index=" + index;
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression, CodegenExpression key)
        {
            return LocalMethod(
                GetBeanPropInternalCodegen(codegenMethodScope, codegenClassScope),
                CastUnderlying(TargetType, beanExpression), key);
        }
    }
} // end of namespace