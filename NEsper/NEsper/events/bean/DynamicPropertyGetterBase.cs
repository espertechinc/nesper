///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat.collections;

using XLR8.CGLib;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Base class for getters for a dynamic property (syntax field.inner?), caches methods to use for classes.
    /// </summary>
    public abstract class DynamicPropertyGetterBase : BeanEventPropertyGetter
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly CopyOnWriteList<DynamicPropertyDescriptor> _cache;
        private ICodegenMember _codegenCache;
        private ICodegenMember _codegenThis;
        private ICodegenMember _codegenEventAdapterService;

        /// <summary>
        /// To be implemented to return the method required, or null to indicate an appropriate method could not be found.
        /// </summary>
        /// <param name="clazz">to search for a matching method</param>
        /// <returns>method if found, or null if no matching method exists</returns>
        protected abstract MethodInfo DetermineMethod(Type clazz);

        /// <summary>
        /// Call the getter to obtains the return result object, or null if no such method exists.
        /// </summary>
        /// <param name="descriptor">provides method information for the class</param>
        /// <param name="underlying">is the underlying object to ask for the property value</param>
        /// <returns>underlying</returns>
        protected abstract Object Call(DynamicPropertyDescriptor descriptor, Object underlying);

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="cache">cache</param>
        /// <param name="getter">getter</param>
        /// <param name="object">object</param>
        /// <param name="eventAdapterService">event server</param>
        /// <returns>property</returns>
        public static Object CacheAndCall(CopyOnWriteList<DynamicPropertyDescriptor> cache, DynamicPropertyGetterBase getter, Object @object, EventAdapterService eventAdapterService)
        {
            var desc = GetPopulateCache(cache, getter, @object, eventAdapterService);
            if (desc.Method == null)
            {
                return null;
            }
            return getter.Call(desc, @object);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="cache">cache</param>
        /// <param name="getter">getter</param>
        /// <param name="object">object</param>
        /// <param name="eventAdapterService">event server</param>
        /// <returns>exists-flag</returns>
        public static bool CacheAndExists(CopyOnWriteList<DynamicPropertyDescriptor> cache, DynamicPropertyGetterBase getter, Object @object, EventAdapterService eventAdapterService)
        {
            var desc = GetPopulateCache(cache, getter, @object, eventAdapterService);
            if (desc.Method == null)
            {
                return false;
            }
            return true;
        }

        public DynamicPropertyGetterBase(EventAdapterService eventAdapterService)
        {
            _cache = new CopyOnWriteList<DynamicPropertyDescriptor>();
            _eventAdapterService = eventAdapterService;
        }

        public Object GetBeanProp(Object @object)
        {
            return CacheAndCall(_cache, this, @object, _eventAdapterService);
        }

        public Type TargetType => typeof(Object);

        public bool IsBeanExistsProperty(Object @object)
        {
            return CacheAndExists(_cache, this, @object, _eventAdapterService);
        }

        public Object Get(EventBean @event)
        {
            return CacheAndCall(_cache, this, @event.Underlying, _eventAdapterService);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return CacheAndExists(_cache, this, eventBean.Underlying, _eventAdapterService);
        }

        public Type BeanPropType => typeof(Object);

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(ExprDotUnderlying(beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(ExprDotUnderlying(beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(ExprDotUnderlying(beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            CodegenMembers(context);
            return StaticMethod(this.GetType(), "CacheAndCall",
                Ref(_codegenCache.MemberName),
                Ref(_codegenThis.MemberName), underlyingExpression,
                Ref(_codegenEventAdapterService.MemberName));
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            CodegenMembers(context);
            return StaticMethod(this.GetType(), "CacheAndExists",
                Ref(_codegenCache.MemberName),
                Ref(_codegenThis.MemberName), underlyingExpression,
                Ref(_codegenEventAdapterService.MemberName));
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            CodegenMembers(context);
            return StaticMethod(typeof(BaseNativePropertyGetter), "GetFragmentDynamic", 
                CodegenUnderlyingGet(underlyingExpression, context),
                Ref(_codegenEventAdapterService.MemberName));
        }

        public Object GetFragment(EventBean eventBean)
        {
            Object result = Get(eventBean);
            return BaseNativePropertyGetter.GetFragmentDynamic(result, _eventAdapterService);
        }

        private static DynamicPropertyDescriptor GetPopulateCache(CopyOnWriteList<DynamicPropertyDescriptor> cache, DynamicPropertyGetterBase dynamicPropertyGetterBase, Object obj, EventAdapterService eventAdapterService)
        {
            // Check if the method is already there
            var target = obj.GetType();
            foreach (DynamicPropertyDescriptor desc in cache)
            {
                if (desc.Clazz == target)
                {
                    return desc;
                }
            }

            // need to add it
            lock (dynamicPropertyGetterBase)
            {
                foreach (DynamicPropertyDescriptor desc in cache)
                {
                    if (desc.Clazz == target)
                    {
                        return desc;
                    }
                }

                // Lookup method to use
                var method = dynamicPropertyGetterBase.DetermineMethod(target);

                // Cache descriptor and create fast method
                DynamicPropertyDescriptor propertyDescriptor;
                if (method == null)
                {
                    propertyDescriptor = new DynamicPropertyDescriptor(target, null, false);
                }
                else
                {
                    var fastClass = FastClass.Create(target);
                    var fastMethod = fastClass.GetMethod(method);
                    propertyDescriptor = new DynamicPropertyDescriptor(target, fastMethod, fastMethod.ParameterCount > 0);
                }
                cache.Add(propertyDescriptor);
                return propertyDescriptor;
            }
        }

        private void CodegenMembers(ICodegenContext context)
        {
            if (_codegenCache == null)
            {
                _codegenCache = context.MakeMember(typeof(CopyOnWriteList<DynamicPropertyDescriptor>), typeof(DynamicPropertyDescriptor), _cache);
                _codegenThis = context.MakeMember(typeof(DynamicPropertyGetterBase), this);
                _codegenEventAdapterService = context.MakeMember(typeof(EventAdapterService), _eventAdapterService);
            }
            context.AddMember(_codegenCache);
            context.AddMember(_codegenThis);
            context.AddMember(_codegenEventAdapterService);
        }
    }
} // end of namespace
