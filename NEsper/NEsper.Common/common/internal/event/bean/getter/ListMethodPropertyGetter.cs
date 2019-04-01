///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for a list property identified by a given index, using vanilla reflection.
    /// </summary>
    public class ListMethodPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndIndexed
    {
        private readonly int index;
        private readonly MethodInfo method;

        public ListMethodPropertyGetter(
            MethodInfo method, int index, EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory) : base(
            eventBeanTypedEventFactory, beanEventTypeFactory, TypeHelper.GetGenericReturnType(method, false), null)
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

        public override Type BeanPropType => TypeHelper.GetGenericReturnType(method, false);

        public override Type TargetType => method.DeclaringType;

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
                GetBeanPropInternalCodegen(codegenMethodScope, BeanPropType, TargetType, method, codegenClassScope),
                underlyingExpression, Constant(index));
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
                var value = method.Invoke(@object, null);
                if (!(value is IList<object>)) {
                    return null;
                }

                var valueList = (IList<object>) value;
                if (valueList.Count <= index) {
                    return null;
                }

                return valueList[index];
            }
            catch (ClassCastException e) {
                throw PropertyUtility.GetMismatchException(method, @object, e);
            }
        }

        private static CodegenMethod GetBeanPropInternalCodegen(
            CodegenMethodScope codegenMethodScope, Type beanPropType, Type targetType, MethodInfo method,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(beanPropType, typeof(ListMethodPropertyGetter), codegenClassScope)
                .AddParam(targetType, "object").AddParam(typeof(int), "index").Block
                .DeclareVar(typeof(object), "value", ExprDotMethod(Ref("object"), method.Name))
                .IfRefNotTypeReturnConst("value", typeof(IList<object>), null)
                .DeclareVar(typeof(IList<object>), "l", Cast(typeof(IList<object>), Ref("value")))
                .IfConditionReturnConst(Relational(ExprDotMethod(Ref("l"), "size"), LE, Ref("index")), null)
                .MethodReturn(Cast(beanPropType, ExprDotMethod(Ref("l"), "get", Ref("index"))));
        }

        public override string ToString()
        {
            return "ListMethodPropertyGetter " +
                   " method=" + method +
                   " index=" + index;
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression, CodegenExpression key)
        {
            return LocalMethod(
                GetBeanPropInternalCodegen(codegenMethodScope, BeanPropType, TargetType, method, codegenClassScope),
                CastUnderlying(TargetType, beanExpression), key);
        }
    }
} // end of namespace