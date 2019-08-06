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
using com.espertech.esper.compat.collections;

using XLR8.CGLib;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Base class for getters for a dynamic property (syntax field.inner?), caches methods to use for classes.
    /// </summary>
    public abstract class DynamicPropertyGetterBase : BeanEventPropertyGetter
    {
        private readonly BeanEventTypeFactory _beanEventTypeFactory;
        private readonly CopyOnWriteList<DynamicPropertyDescriptor> _cache;
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;

        private readonly CodegenFieldSharable _sharableCode = new ProxyCodegenFieldSharable {
            ProcType = () => typeof(CopyOnWriteList<DynamicPropertyDescriptor>),
            ProcInitCtorScoped = () => NewInstance(typeof(CopyOnWriteList<DynamicPropertyDescriptor>))
        };

        public DynamicPropertyGetterBase(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            _beanEventTypeFactory = beanEventTypeFactory;
            _cache = new CopyOnWriteList<DynamicPropertyDescriptor>();
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

        public bool IsExistsProperty(EventBean eventBean)
        {
            return CacheAndExists(_cache, this, eventBean.Underlying, _eventBeanTypedEventFactory);
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

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CacheAndExistsCodegen(underlyingExpression, codegenMethodScope, codegenClassScope);
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

        /// <summary>
        ///     To be implemented to return the method required, or null to indicate an appropriate method could not be found.
        /// </summary>
        /// <param name="clazz">to search for a matching method</param>
        /// <returns>method if found, or null if no matching method exists</returns>
        internal abstract MethodInfo DetermineMethod(Type clazz);

        internal abstract CodegenExpression DetermineMethodCodegen(
            CodegenExpressionRef clazz,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope);

        /// <summary>
        ///     Call the getter to obtains the return result object, or null if no such method exists.
        /// </summary>
        /// <param name="descriptor">provides method information for the class</param>
        /// <param name="underlying">is the underlying object to ask for the property value</param>
        /// <returns>underlying</returns>
        internal abstract object Call(
            DynamicPropertyDescriptor descriptor,
            object underlying);

        internal abstract CodegenExpression CallCodegen(
            CodegenExpressionRef desc,
            CodegenExpressionRef @object,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope);

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="cache">cache</param>
        /// <param name="getter">getter</param>
        /// <param name="object">object</param>
        /// <param name="eventBeanTypedEventFactory">event server</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <returns>property</returns>
        public static object CacheAndCall(
            CopyOnWriteList<DynamicPropertyDescriptor> cache,
            DynamicPropertyGetterBase getter,
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
            CodegenExpression memberCache = codegenClassScope.AddOrGetFieldSharable(_sharableCode);
            var method = parent
                .MakeChild(typeof(object), typeof(DynamicPropertyGetterBase), codegenClassScope)
                .AddParam(typeof(object), "@object");
            method.Block
                .DeclareVar<DynamicPropertyDescriptor>(
                    "desc",
                    GetPopulateCacheCodegen(memberCache, Ref("@object"), method, codegenClassScope))
                .IfCondition(EqualsNull(ExprDotName(Ref("desc"), "Method")))
                .BlockReturn(ConstantNull())
                .MethodReturn(CallCodegen(Ref("desc"), Ref("@object"), method, codegenClassScope));
            return LocalMethod(method, underlyingExpression);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="cache">cache</param>
        /// <param name="getter">getter</param>
        /// <param name="object">object</param>
        /// <param name="eventBeanTypedEventFactory">event server</param>
        /// <returns>exists-flag</returns>
        public static bool CacheAndExists(
            CopyOnWriteList<DynamicPropertyDescriptor> cache,
            DynamicPropertyGetterBase getter,
            object @object,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var desc = GetPopulateCache(cache, getter, @object, eventBeanTypedEventFactory);
            if (desc.Method == null) {
                return false;
            }

            return true;
        }

        private CodegenExpression CacheAndExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression memberCache = codegenClassScope.AddOrGetFieldSharable(_sharableCode);
            var method = parent.MakeChild(typeof(bool), typeof(DynamicPropertyGetterBase), codegenClassScope)
                .AddParam(typeof(object), "@object");
            method.Block
                .DeclareVar<DynamicPropertyDescriptor>(
                    "desc",
                    GetPopulateCacheCodegen(memberCache, Ref("@object"), method, codegenClassScope))
                .IfCondition(EqualsNull(ExprDotName(Ref("desc"), "Method")))
                .BlockReturn(ConstantFalse())
                .MethodReturn(Constant(true));
            return LocalMethod(method, underlyingExpression);
        }

        private static DynamicPropertyDescriptor GetPopulateCache(
            CopyOnWriteList<DynamicPropertyDescriptor> cache,
            DynamicPropertyGetterBase dynamicPropertyGetterBase,
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

        private CodegenExpression GetPopulateCacheCodegen(
            CodegenExpression memberCache,
            CodegenExpressionRef @object,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            var method = parent
                .MakeChild(typeof(DynamicPropertyDescriptor), typeof(DynamicPropertyGetterBase), codegenClassScope)
                .AddParam(typeof(CopyOnWriteList<DynamicPropertyDescriptor>), "cache")
                .AddParam(typeof(object), "obj");
            method.Block
                .DeclareVar<DynamicPropertyDescriptor>(
                    "desc",
                    StaticMethod(
                        typeof(DynamicPropertyGetterBase),
                        "DynamicPropertyCacheCheck",
                        Ref("cache"),
                        Ref("obj")))
                .IfRefNotNull("desc")
                .BlockReturn(Ref("desc"))
                .DeclareVar<Type>("clazz", ExprDotName(Ref("obj"), "Class"))
                .DeclareVar<MethodInfo>("method", DetermineMethodCodegen(Ref("clazz"), method, codegenClassScope))
                .AssignRef(
                    "desc",
                    StaticMethod(
                        typeof(DynamicPropertyGetterBase),
                        "DynamicPropertyCacheAdd",
                        Ref("clazz"),
                        Ref("method"),
                        Ref("cache")))
                .MethodReturn(Ref("desc"));
            return LocalMethod(method, memberCache, @object);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="obj">target</param>
        /// <param name="cache">cache</param>
        /// <returns>descriptor</returns>
        public static DynamicPropertyDescriptor DynamicPropertyCacheCheck(
            CopyOnWriteList<DynamicPropertyDescriptor> cache,
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
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="method">method</param>
        /// <param name="cache">cache</param>
        /// <returns>descriptor</returns>
        public static DynamicPropertyDescriptor DynamicPropertyCacheAdd(
            Type clazz,
            MethodInfo method,
            CopyOnWriteList<DynamicPropertyDescriptor> cache)
        {
            DynamicPropertyDescriptor propertyDescriptor;
            if (method == null) {
                propertyDescriptor = new DynamicPropertyDescriptor(clazz, null, false);
            }
            else {
                propertyDescriptor = new DynamicPropertyDescriptor(
                    clazz,
                    FastClass.CreateMethod(method),
                    method.GetParameters().Length > 0);
            }

            cache.Add(propertyDescriptor);
            return propertyDescriptor;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="descriptor">descriptor</param>
        /// <param name="underlying">underlying</param>
        /// <param name="ex">throwable</param>
        /// <returns>exception</returns>
        public static PropertyAccessException HandleException(
            DynamicPropertyDescriptor descriptor,
            object underlying,
            Exception ex)
        {
            if (ex is InvalidCastException) {
                throw PropertyUtility.GetMismatchException(
                    descriptor.Method.Target,
                    underlying,
                    (InvalidCastException) ex);
            }

            if (ex is TargetException) {
                throw PropertyUtility.GetTargetException(descriptor.Method.Target, (TargetException) ex);
            }

            if (ex is ArgumentException) {
                throw PropertyUtility.GetArgumentException(descriptor.Method.Target, (ArgumentException) ex);
            }

            if (ex is MemberAccessException) {
                throw PropertyUtility.GetMemberAccessException(descriptor.Method.Target, (MemberAccessException) ex);
            }

            throw PropertyUtility.GetGeneralException(descriptor.Method.Target, ex);
        }
    }
} // end of namespace