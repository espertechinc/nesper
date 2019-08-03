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
        private readonly BeanEventTypeFactory _beanEventTypeFactory;
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly Type _fragmentClassType;
        private readonly bool _isArray;
        private readonly bool _isIterable;
        private volatile BeanEventType _fragmentEventType;
        private bool _isFragmentable;

        public BaseNativePropertyGetter(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType,
            Type genericType)
        {
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            _beanEventTypeFactory = beanEventTypeFactory;
            if (returnType.IsArray) {
                _fragmentClassType = returnType.GetElementType();
                _isArray = true;
                _isIterable = false;
            }
            else if (returnType.IsGenericEnumerable()) {
                _fragmentClassType = genericType;
                _isArray = false;
                _isIterable = true;
            }
            else {
                _fragmentClassType = returnType;
                _isArray = false;
                _isIterable = false;
            }

            _isFragmentable = true;
        }

        public abstract Type TargetType { get; }

        public abstract Type BeanPropType { get; }

        public object GetFragment(EventBean eventBean)
        {
            DetermineFragmentable();
            if (!_isFragmentable) {
                return null;
            }

            var @object = Get(eventBean);
            if (@object == null) {
                return null;
            }

            if (_isArray) {
                return ToFragmentArray((object[]) @object, _fragmentEventType, _eventBeanTypedEventFactory);
            }

            if (_isIterable) {
                return ToFragmentIterable(@object, _fragmentEventType, _eventBeanTypedEventFactory);
            }

            return _eventBeanTypedEventFactory.AdapterForTypedBean(@object, _fragmentEventType);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            DetermineFragmentable();
            if (!_isFragmentable) {
                return ConstantNull();
            }

            return UnderlyingFragmentCodegen(
                CastUnderlying(TargetType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            DetermineFragmentable();
            if (!_isFragmentable) {
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
            if (!(@object is IEnumerable<EventBean> enumerable)) {
                return null;
            }

            var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext()) {
                return new EventBean[0];
            }

            var events = new ArrayDeque<EventBean>();
            do {
                object next = enumerator.Current;
                if (next == null) {
                    continue;
                }

                events.Add(eventBeanTypedEventFactory.AdapterForTypedBean(next, fragmentEventType));
            } while (enumerator.MoveNext());

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
                    EventTypeUtility.ResolveTypeCodegen(_fragmentEventType, EPStatementInitServicesConstants.REF)));

            var block = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(TargetType, "underlying")
                .Block
                .DeclareVar(
                    BeanPropType,
                    "@object",
                    UnderlyingGetCodegen(Ref("underlying"), codegenMethodScope, codegenClassScope))
                .IfRefNullReturnNull("@object");

            if (_isArray) {
                return block.MethodReturn(
                    StaticMethod(
                        typeof(BaseNativePropertyGetter),
                        "ToFragmentArray",
                        Cast(typeof(object[]), Ref("@object")),
                        mtype,
                        msvc));
            }

            if (_isIterable) {
                return block.MethodReturn(
                    StaticMethod(typeof(BaseNativePropertyGetter), "ToFragmentIterable", Ref("@object"), mtype, msvc));
            }

            return block.MethodReturn(ExprDotMethod(msvc, "AdapterForTypedObject", Ref("@object"), mtype));
        }

        private void DetermineFragmentable()
        {
            if (_fragmentEventType == null) {
                if (_fragmentClassType.IsFragmentableType()) {
                    _fragmentEventType = _beanEventTypeFactory.GetCreateBeanType(_fragmentClassType);
                }
                else {
                    _isFragmentable = false;
                }
            }
        }
    }
} // end of namespace