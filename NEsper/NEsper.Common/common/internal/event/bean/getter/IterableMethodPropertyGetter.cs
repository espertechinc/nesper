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

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for an iterable property identified by a given index, using vanilla reflection.
    /// </summary>
    public class IterableMethodPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndIndexed
    {
        private readonly int index;
        private readonly MethodInfo method;

        public IterableMethodPropertyGetter(
            MethodInfo method,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
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
            return Constant(true);
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetBeanPropCodegen(codegenMethodScope, BeanPropType, TargetType, method, codegenClassScope),
                underlyingExpression, Constant(index));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return Constant(true);
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        ///     Returns the iterable at a certain index, or null.
        /// </summary>
        /// <param name="value">the iterable</param>
        /// <param name="index">index</param>
        /// <returns>value at index</returns>
        public static object GetBeanEventIterableValue(
            object value,
            int index)
        {
            if (!(value is Iterable)) {
                return null;
            }

            IEnumerator it = ((Iterable) value).GetEnumerator();

            if (index == 0) {
                if (it.MoveNext()) {
                    return it.Current;
                }

                return null;
            }

            var count = 0;
            while (true) {
                if (!it.MoveNext()) {
                    return null;
                }

                if (count < index) {
                    it.Current;
                }
                else {
                    return it.Current;
                }

                count++;
            }
        }

        private object GetBeanPropInternal(
            object @object,
            int index)
        {
            try {
                var value = method.Invoke(@object, null);
                return GetBeanEventIterableValue(value, index);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(method, @object, e);
            }
        }

        protected internal static CodegenMethod GetBeanPropCodegen(
            CodegenMethodScope codegenMethodScope,
            Type beanPropType,
            Type targetType,
            MethodInfo method,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(beanPropType, typeof(IterableMethodPropertyGetter), codegenClassScope)
                .AddParam(targetType, "object").AddParam(typeof(int), "index").Block
                .DeclareVar(typeof(object), "value", ExprDotMethod(Ref("object"), method.Name))
                .MethodReturn(
                    Cast(
                        beanPropType,
                        StaticMethod(
                            typeof(IterableMethodPropertyGetter), "getBeanEventIterableValue", Ref("value"),
                            Ref("index"))));
        }

        public override string ToString()
        {
            return "IterableMethodPropertyGetter " +
                   " method=" + method +
                   " index=" + index;
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return LocalMethod(
                GetBeanPropCodegen(codegenMethodScope, BeanPropType, TargetType, method, codegenClassScope),
                CastUnderlying(TargetType, beanExpression), Constant(index));
        }
    }
} // end of namespace