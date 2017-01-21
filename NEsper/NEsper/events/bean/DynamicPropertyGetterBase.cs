///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Base class for getters for a dynamic property (syntax field.inner?), caches
    /// methods to use for classes.
    /// </summary>
    public abstract class DynamicPropertyGetterBase : BeanEventPropertyGetter
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly CopyOnWriteList<DynamicPropertyDescriptor> _cache;
        private readonly ILockable _iLock;

        /// <summary>
        /// To be implemented to return the method required, or null to indicate an
        /// appropriate method could not be found.
        /// </summary>
        /// <param name="type">to search for a matching method</param>
        /// <returns>
        /// method if found, or null if no matching method exists
        /// </returns>
        protected abstract MethodInfo DetermineMethod(Type type);

        /// <summary>
        /// Call the getter to obtains the return result object, or null if no such method
        /// exists.
        /// </summary>
        /// <param name="descriptor">provides method information for the class</param>
        /// <param name="underlying">is the underlying object to ask for the property value</param>
        /// <returns>
        /// underlying
        /// </returns>
        protected abstract Object Call(DynamicPropertyDescriptor descriptor, Object underlying);

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        protected DynamicPropertyGetterBase(EventAdapterService eventAdapterService)
        {
            this._iLock = LockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            this._cache = new CopyOnWriteList<DynamicPropertyDescriptor>();
            this._eventAdapterService = eventAdapterService;
        }

        public Object GetBeanProp(Object @object)
        {
            DynamicPropertyDescriptor desc = GetPopulateCache(@object);
            if (desc.GetMethod() == null)
            {
                return null;
            }
            return Call(desc, @object);
        }

        public bool IsBeanExistsProperty(Object @object)
        {
            DynamicPropertyDescriptor desc = GetPopulateCache(@object);
            if (desc.GetMethod() == null)
            {
                return false;
            }
            return true;
        }

        public Object Get(EventBean eventBean)
        {
            DynamicPropertyDescriptor desc = GetPopulateCache(eventBean.Underlying);
            if (desc.GetMethod() == null)
            {
                return null;
            }
            return Call(desc, eventBean.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            DynamicPropertyDescriptor desc = GetPopulateCache(eventBean.Underlying);
            if (desc.GetMethod() == null)
            {
                return false;
            }
            return true;
        }

        private DynamicPropertyDescriptor GetPopulateCache(Object obj)
        {
            // Check if the method is already there
            Type target = obj.GetType();
            foreach (DynamicPropertyDescriptor desc in _cache)
            {
                if (desc.GetClazz() == target)
                {
                    return desc;
                }
            }

            // need to add it
            using (_iLock.Acquire())
            {
                foreach (DynamicPropertyDescriptor desc in _cache)
                {
                    if (desc.GetClazz() == target)
                    {
                        return desc;
                    }
                }

                // Lookup method to use
                MethodInfo method = DetermineMethod(target);

                // Cache descriptor and create fast method
                DynamicPropertyDescriptor propertyDescriptor;
                if (method == null)
                {
                    propertyDescriptor = new DynamicPropertyDescriptor(target, null, false);
                }
                else
                {
                    FastClass fastClass = FastClass.Create(target);
                    FastMethod fastMethod = fastClass.GetMethod(method);
                    propertyDescriptor = new DynamicPropertyDescriptor(target, fastMethod, fastMethod.ParameterCount > 0);
                }
                _cache.Add(propertyDescriptor);
                return propertyDescriptor;
            }
        }

        public Object GetFragment(EventBean eventBean)
        {
            Object result = Get(eventBean);
            return BaseNativePropertyGetter.GetFragmentDynamic(result, _eventAdapterService);
        }
    }
}
