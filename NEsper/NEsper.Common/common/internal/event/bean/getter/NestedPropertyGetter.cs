///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for one or more levels deep nested properties.
    /// </summary>
    public class NestedPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter
    {
        private readonly BeanEventPropertyGetter[] getterChain;

        public NestedPropertyGetter(
            IList<EventPropertyGetter> getterChain,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            Type finalPropertyType,
            Type finalGenericType,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                eventBeanTypedEventFactory, beanEventTypeFactory, finalPropertyType, finalGenericType)
        {
            this.getterChain = new BeanEventPropertyGetter[getterChain.Count];

            for (var i = 0; i < getterChain.Count; i++) {
                this.getterChain[i] = (BeanEventPropertyGetter) getterChain[i];
            }
        }

        public object GetBeanProp(object value)
        {
            if (value == null) {
                return value;
            }

            for (var i = 0; i < getterChain.Length; i++) {
                value = getterChain[i].GetBeanProp(value);

                if (value == null) {
                    return null;
                }
            }

            return value;
        }

        public bool IsBeanExistsProperty(object value)
        {
            if (value == null) {
                return false;
            }

            var lastElementIndex = getterChain.Length - 1;

            // walk the getter chain up to the previous-to-last element, returning its object value.
            // any null values in between mean the property does not exists
            for (var i = 0; i < getterChain.Length - 1; i++) {
                value = getterChain[i].GetBeanProp(value);

                if (value == null) {
                    return false;
                }
            }

            return getterChain[lastElementIndex].IsBeanExistsProperty(value);
        }

        public override object Get(EventBean eventBean)
        {
            return GetBeanProp(eventBean.Underlying);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return IsBeanExistsProperty(eventBean.Underlying);
        }

        public override Type BeanPropType => getterChain[getterChain.Length - 1].BeanPropType;

        public override Type TargetType => getterChain[0].TargetType;

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
            return LocalMethod(GetBeanPropCodegen(codegenMethodScope, codegenClassScope, false), underlyingExpression);
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetBeanPropCodegen(codegenMethodScope, codegenClassScope, true), underlyingExpression);
        }

        private CodegenMethod GetBeanPropCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            bool exists)
        {
            var block = codegenMethodScope.MakeChild(
                exists ? typeof(bool) : getterChain[getterChain.Length - 1].BeanPropType.GetBoxedType(), GetType(),
                codegenClassScope).AddParam(getterChain[0].TargetType, "value").Block;
            if (!exists) {
                block.IfRefNullReturnNull("value");
            }
            else {
                block.IfRefNullReturnFalse("value");
            }

            var lastName = "value";
            for (var i = 0; i < getterChain.Length - 1; i++) {
                var varName = "l" + i;
                block.DeclareVar(
                    getterChain[i].BeanPropType, varName,
                    getterChain[i].UnderlyingGetCodegen(Ref(lastName), codegenMethodScope, codegenClassScope));
                lastName = varName;
                if (!exists) {
                    block.IfRefNullReturnNull(lastName);
                }
                else {
                    block.IfRefNullReturnFalse(lastName);
                }
            }

            if (!exists) {
                return block.MethodReturn(
                    getterChain[getterChain.Length - 1].UnderlyingGetCodegen(
                        Ref(lastName), codegenMethodScope, codegenClassScope));
            }

            return block.MethodReturn(
                getterChain[getterChain.Length - 1].UnderlyingExistsCodegen(
                    Ref(lastName), codegenMethodScope, codegenClassScope));
        }
    }
} // end of namespace