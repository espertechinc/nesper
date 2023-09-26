///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    /// Base class for getters for a dynamic property (syntax field.inner?), caches methods to use for classes.
    /// </summary>
    public abstract class DynamicPropertyGetterByMethodOrPropertyBase : BeanEventPropertyGetter
    {
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly BeanEventTypeFactory _beanEventTypeFactory;
        private readonly CopyOnWriteList<DynamicPropertyDescriptorByMethod> _cache;

        private readonly CodegenFieldSharable _sharableCode = new ProxyCodegenFieldSharable() {
            ProcType = () => typeof(CopyOnWriteList<DynamicPropertyDescriptorByMethod>),
            ProcInitCtorScoped = () => NewInstance<CopyOnWriteList<DynamicPropertyDescriptorByMethod>>()
        };

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => _eventBeanTypedEventFactory;

        public BeanEventTypeFactory BeanEventTypeFactory => _beanEventTypeFactory;

        public CopyOnWriteList<DynamicPropertyDescriptorByMethod> Cache => _cache;

        public CodegenFieldSharable SharableCode => _sharableCode;

        /// <summary>
        /// To be implemented to return the method required, or null to indicate an appropriate method could not be found.
        /// </summary>
        /// <param name="clazz">to search for a matching method</param>
        /// <returns>method if found, or null if no matching method exists</returns>
        protected abstract MethodInfo DetermineMethod(Type clazz);

        protected abstract CodegenExpression DetermineMethodCodegen(
            CodegenExpressionRef clazz,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope);

        /// <summary>
        /// Call the getter to obtains the return result object, or null if no such method exists.
        /// </summary>
        /// <param name="descriptor">provides method information for the class</param>
        /// <param name="underlying">is the underlying object to ask for the property value</param>
        /// <returns>underlying</returns>
        protected abstract object Call(
            DynamicPropertyDescriptorByMethod descriptor,
            object underlying);

        protected abstract CodegenExpression CallCodegen(
            CodegenExpressionRef desc,
            CodegenExpressionRef @object,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope);

        public abstract bool IsExistsProperty(EventBean eventBean);

        public abstract CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="cache">cache</param>
        /// <param name="getter">getter</param>
        /// <param name="object">object</param>
        /// <param name="eventBeanTypedEventFactory">event server</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <returns>property</returns>
        public static object CacheAndCall(
            CopyOnWriteList<DynamicPropertyDescriptorByMethod> cache,
            DynamicPropertyGetterByMethodOrPropertyBase getter,
            object @object,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var desc = GetPopulateCache(cache, getter, @object, eventBeanTypedEventFactory);
            if (desc.Method == null) {
                return null;
            }

            return getter.Call(desc, @object);
        }

        private CodegenExpression CacheAndCallCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression memberCache = codegenClassScope.AddOrGetDefaultFieldSharable(_sharableCode);
            var method = parent
                .MakeChild(typeof(object), typeof(DynamicPropertyGetterByMethodOrPropertyBase), codegenClassScope)
                .AddParam<object>("@object");
            method.Block
                .DeclareVar<DynamicPropertyDescriptorByMethod>(
                    "desc",
                    GetPopulateCacheCodegen(memberCache, Ref("@object"), method, codegenClassScope))
                .IfCondition(EqualsNull(ExprDotName(Ref("desc"), "Method")))
                .BlockReturn(ConstantNull())
                .MethodReturn(CallCodegen(Ref("desc"), Ref("@object"), method, codegenClassScope));
            return LocalMethod(method, underlyingExpression);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="cache">cache</param>
        /// <param name="getter">getter</param>
        /// <param name="object">object</param>
        /// <param name="eventBeanTypedEventFactory">event server</param>
        /// <returns>exists-flag</returns>
        public static bool CacheAndExists(
            CopyOnWriteList<DynamicPropertyDescriptorByMethod> cache,
            DynamicPropertyGetterByMethodOrPropertyBase getter,
            object @object,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var desc = GetPopulateCache(cache, getter, @object, eventBeanTypedEventFactory);
            if (desc.Method == null) {
                return false;
            }

            return true;
        }

        protected CodegenExpression CacheAndExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            var memberCache = codegenClassScope.AddOrGetDefaultFieldSharable(_sharableCode);
            var method = parent.MakeChild(
                    typeof(bool),
                    typeof(DynamicPropertyGetterByMethodOrPropertyBase),
                    codegenClassScope)
                .AddParam<object>("@object");
            method.Block
                .DeclareVar<DynamicPropertyDescriptorByMethod>(
                    "desc",
                    GetPopulateCacheCodegen(memberCache, Ref("@object"), method, codegenClassScope))
                .IfCondition(EqualsNull(ExprDotName(Ref("desc"), "Method")))
                .BlockReturn(ConstantFalse())
                .MethodReturn(Constant(true));
            return LocalMethod(method, underlyingExpression);
        }

        public DynamicPropertyGetterByMethodOrPropertyBase(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            _beanEventTypeFactory = beanEventTypeFactory;
            _cache = new CopyOnWriteList<DynamicPropertyDescriptorByMethod>();
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public object GetBeanProp(object @object)
        {
            return CacheAndCall(_cache, this, @object, _eventBeanTypedEventFactory, _beanEventTypeFactory);
        }

        public Type TargetType => typeof(object);

        public bool IsBeanExistsProperty(object @object)
        {
            return CacheAndExists(_cache, this, @object, _eventBeanTypedEventFactory);
        }

        public object Get(EventBean @event)
        {
            return CacheAndCall(_cache, this, @event.Underlying, _eventBeanTypedEventFactory, _beanEventTypeFactory);
        }

        public Type BeanPropType => typeof(object);

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(ExprDotUnderlying(beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(ExprDotUnderlying(beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(ExprDotUnderlying(beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CacheAndCallCodegen(underlyingExpression, codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public object GetFragment(EventBean eventBean)
        {
            var result = Get(eventBean);
            return BaseNativePropertyGetter.GetFragmentDynamic(
                result,
                _eventBeanTypedEventFactory,
                _beanEventTypeFactory);
        }

        internal static DynamicPropertyDescriptorByMethod GetPopulateCache(
            CopyOnWriteList<DynamicPropertyDescriptorByMethod> cache,
            DynamicPropertyGetterByMethodOrPropertyBase dynamicPropertyGetterBase,
            object obj,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var desc = DynamicPropertyCacheCheck(cache, obj);
            if (desc != null) {
                return desc;
            }

            // need to add it
            lock (dynamicPropertyGetterBase) {
                desc = DynamicPropertyCacheCheck(cache, obj);
                if (desc != null) {
                    return desc;
                }

                // Lookup method to use
                var method = dynamicPropertyGetterBase.DetermineMethod(obj.GetType());

                // Cache descriptor and create fast method
                desc = DynamicPropertyCacheAdd(obj.GetType(), method, cache);
                return desc;
            }
        }

        protected CodegenExpression GetPopulateCacheCodegen(
            CodegenExpression memberCache,
            CodegenExpressionRef @object,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            var method = parent
                .MakeChild(
                    typeof(DynamicPropertyDescriptorByMethod),
                    typeof(DynamicPropertyGetterByMethodOrPropertyBase),
                    codegenClassScope)
                .AddParam<CopyOnWriteList<DynamicPropertyDescriptorByMethod>>("cache")
                .AddParam<object>("obj");
            method.Block
                .DeclareVar<DynamicPropertyDescriptorByMethod>("desc",
                    StaticMethod(
                        typeof(DynamicPropertyGetterByMethodOrPropertyBase),
                        "DynamicPropertyCacheCheck",
                        Ref("cache"),
                        Ref("obj")))
                .IfRefNotNull("desc")
                .BlockReturn(Ref("desc"))
                .DeclareVar<Type>("clazz", ExprDotMethod(Ref("obj"), "GetType"))
                .DeclareVar<MethodInfo>("method", DetermineMethodCodegen(Ref("clazz"), method, codegenClassScope))
                .AssignRef(
                    "desc",
                    StaticMethod(
                        typeof(DynamicPropertyGetterByMethodOrPropertyBase),
                        "DynamicPropertyCacheAdd",
                        Ref("clazz"),
                        Ref("method"),
                        Ref("cache")))
                .MethodReturn(Ref("desc"));
            return LocalMethod(method, memberCache, @object);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="obj">target</param>
        /// <param name="cache">cache</param>
        /// <returns>descriptor</returns>
        public static DynamicPropertyDescriptorByMethod DynamicPropertyCacheCheck(
            CopyOnWriteList<DynamicPropertyDescriptorByMethod> cache,
            object obj)
        {
            // Check if the method is already there
            var target = obj.GetType();
            foreach (var desc in cache) {
                if (desc.Clazz == target) {
                    return desc;
                }
            }

            return null;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="method">method</param>
        /// <param name="cache">cache</param>
        /// <returns>descriptor</returns>
        public static DynamicPropertyDescriptorByMethod DynamicPropertyCacheAdd(
            Type clazz,
            MethodInfo method,
            CopyOnWriteList<DynamicPropertyDescriptorByMethod> cache)
        {
            DynamicPropertyDescriptorByMethod propertyDescriptor;
            if (method == null) {
                propertyDescriptor = new DynamicPropertyDescriptorByMethod(clazz, null, false);
            }
            else {
                propertyDescriptor = new DynamicPropertyDescriptorByMethod(
                    clazz,
                    method,
                    method.GetParameters().Length > 0);
            }

            cache.Add(propertyDescriptor);
            return propertyDescriptor;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="descriptor">descriptor</param>
        /// <param name="underlying">underlying</param>
        /// <param name="ex">exception</param>
        /// <returns>exception</returns>
        public static PropertyAccessException HandleException(
            DynamicPropertyDescriptorByMethod descriptor,
            object underlying,
            Exception ex)
        {
            if (ex is InvalidCastException invalidCastException) {
                throw PropertyUtility.GetMismatchException(descriptor.Method, underlying, invalidCastException);
            }

            if (ex is TargetException targetException) {
                throw PropertyUtility.GetTargetException(descriptor.Method, targetException);
            }

            if (ex is TargetInvocationException targetInvocationException) {
                throw PropertyUtility.GetTargetException(descriptor.Method, targetInvocationException);
            }

            if (ex is ArgumentException argumentException) {
                throw PropertyUtility.GetArgumentException(descriptor.Method, argumentException);
            }

            if (ex is MemberAccessException memberAccessException) {
                throw PropertyUtility.GetMemberAccessException(descriptor.Method, memberAccessException);
            }

            throw PropertyUtility.GetGeneralException(descriptor.Method, ex);
        }
    }
} // end of namespace