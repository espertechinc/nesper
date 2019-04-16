///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Base getter for native fragments.
    /// </summary>
    public abstract class BaseNativePropertyGetter : EventPropertyGetterSPI
    {
        private readonly BeanEventTypeFactory beanEventTypeFactory;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly Type fragmentClassType;
        private readonly bool isArray;
        private readonly bool isIterable;
        private volatile BeanEventType fragmentEventType;
        private bool isFragmentable;

        public BaseNativePropertyGetter(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType,
            Type genericType)
        {
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.beanEventTypeFactory = beanEventTypeFactory;
            if (returnType.IsArray) {
                fragmentClassType = returnType.GetElementType();
                isArray = true;
                isIterable = false;
            }
            else if (returnType.IsImplementsInterface(typeof(Iterable))) {
                fragmentClassType = genericType;
                isArray = false;
                isIterable = true;
            }
            else {
                fragmentClassType = returnType;
                isArray = false;
                isIterable = false;
            }

            isFragmentable = true;
        }

        public abstract Type TargetType { get; }

        public abstract Type BeanPropType { get; }

        public object GetFragment(EventBean eventBean)
        {
            DetermineFragmentable();
            if (!isFragmentable) {
                return null;
            }

            var @object = Get(eventBean);
            if (@object == null) {
                return null;
            }

            if (isArray) {
                return ToFragmentArray((object[]) @object, fragmentEventType, eventBeanTypedEventFactory);
            }

            if (isIterable) {
                return ToFragmentIterable(@object, fragmentEventType, eventBeanTypedEventFactory);
            }

            return eventBeanTypedEventFactory.AdapterForTypedBean(@object, fragmentEventType);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            DetermineFragmentable();
            if (!isFragmentable) {
                return ConstantNull();
            }

            return UnderlyingFragmentCodegen(
                CastUnderlying(TargetType, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            DetermineFragmentable();
            if (!isFragmentable) {
                return ConstantNull();
            }

            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public abstract object Get(EventBean eventBean);
        public abstract bool IsExistsProperty(EventBean eventBean);

        public abstract CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public abstract CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public abstract CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public abstract CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="objectArray">array</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventBeanTypedEventFactory">event adapters</param>
        /// <returns>array</returns>
        public static object ToFragmentArray(
            object[] objectArray,
            BeanEventType fragmentEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var events = new EventBean[objectArray.Length];
            var countFilled = 0;

            for (var i = 0; i < objectArray.Length; i++) {
                var element = objectArray[i];
                if (element == null) {
                    continue;
                }

                events[countFilled] = eventBeanTypedEventFactory.AdapterForTypedBean(element, fragmentEventType);
                countFilled++;
            }

            if (countFilled == objectArray.Length) {
                return events;
            }

            if (countFilled == 0) {
                return new EventBean[0];
            }

            var returnVal = new EventBean[countFilled];
            Array.Copy(events, 0, returnVal, 0, countFilled);
            return returnVal;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        ///     Returns the fragment for dynamic properties.
        /// </summary>
        /// <param name="object">to inspect</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <returns>fragment</returns>
        public static object GetFragmentDynamic(
            object @object,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            if (@object == null) {
                return null;
            }

            BeanEventType fragmentEventType = null;
            var isArray = false;
            var objectType = @object.GetType();
            if (objectType.IsArray) {
                if (objectType.GetElementType().IsFragmentableType()) {
                    isArray = true;
                    fragmentEventType = beanEventTypeFactory.GetCreateBeanType(objectType.GetElementType());
                }
            }
            else {
                if (objectType.IsFragmentableType()) {
                    fragmentEventType = beanEventTypeFactory.GetCreateBeanType(objectType);
                }
            }

            if (fragmentEventType == null) {
                return null;
            }

            if (isArray) {
                var objectArray = (Array) @object;
                var len = objectArray.Length;
                var events = new EventBean[len];
                var countFilled = 0;

                for (var i = 0; i < len; i++) {
                    var element = objectArray.GetValue(i);
                    if (element == null) {
                        continue;
                    }

                    events[countFilled] = eventBeanTypedEventFactory.AdapterForTypedBean(element, fragmentEventType);
                    countFilled++;
                }

                if (countFilled == len) {
                    return events;
                }

                if (countFilled == 0) {
                    return new EventBean[0];
                }

                var returnVal = new EventBean[countFilled];
                Array.Copy(events, 0, returnVal, 0, countFilled);
                return returnVal;
            }

            return eventBeanTypedEventFactory.AdapterForTypedBean(@object, fragmentEventType);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        ///     Returns the fragment for dynamic properties.
        /// </summary>
        /// <param name="object">to inspect</param>
        /// <param name="fragmentEventType">type</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <returns>fragment</returns>
        public static object ToFragmentIterable(
            object @object,
            BeanEventType fragmentEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            if (!(@object is Iterable)) {
                return null;
            }

            IEnumerator<EventBean> iterator = ((IEnumerator<EventBean>) @object).GetEnumerator();
            if (!iterator.HasNext) {
                return new EventBean[0];
            }

            var events = new ArrayDeque<EventBean>();
            while (iterator.HasNext) {
                object next = iterator.Next();
                if (next == null) {
                    continue;
                }

                events.Add(eventBeanTypedEventFactory.AdapterForTypedBean(next, fragmentEventType));
            }

            return events.ToArray();
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var msvc = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var mtype = codegenClassScope.AddFieldUnshared<BeanEventType>(
                false,
                Cast(
                    typeof(BeanEventType),
                    EventTypeUtility.ResolveTypeCodegen(fragmentEventType, EPStatementInitServicesConstants.REF)));

            var block = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(TargetType, "underlying").Block
                .DeclareVar(
                    BeanPropType, "object",
                    UnderlyingGetCodegen(Ref("underlying"), codegenMethodScope, codegenClassScope))
                .IfRefNullReturnNull("object");

            if (isArray) {
                return block.MethodReturn(
                    StaticMethod(
                        typeof(BaseNativePropertyGetter), "toFragmentArray", Cast(typeof(object[]), Ref("object")),
                        mtype, msvc));
            }

            if (isIterable) {
                return block.MethodReturn(
                    StaticMethod(typeof(BaseNativePropertyGetter), "toFragmentIterable", Ref("object"), mtype, msvc));
            }

            return block.MethodReturn(ExprDotMethod(msvc, "adapterForTypedBean", Ref("object"), mtype));
        }

        private void DetermineFragmentable()
        {
            if (fragmentEventType == null) {
                if (fragmentClassType.IsFragmentableType()) {
                    fragmentEventType = beanEventTypeFactory.GetCreateBeanType(fragmentClassType);
                }
                else {
                    isFragmentable = false;
                }
            }
        }
    }
} // end of namespace