///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.events.bean;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>
    /// Getter for one or more levels deep nested properties of maps.
    /// </summary>
    public class MapNestedPropertyGetterMixedType : MapEventPropertyGetter
    {
        private readonly EventPropertyGetter[] _getterChain;
    
        /// <summary>Ctor. </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="eventAdaperService">is a factory for PONO bean event types</param>
        public MapNestedPropertyGetterMixedType(IEnumerable<EventPropertyGetter> getterChain,
                                                EventAdapterService eventAdaperService)
        {
            _getterChain = getterChain.ToArray();
        }
    
        public Object GetMap(IDictionary<String, Object> map)
        {
            Object result = ((MapEventPropertyGetter) _getterChain[0]).GetMap(map);
            return HandleGetterTrailingChain(result);
        }
    
        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            if (!((MapEventPropertyGetter) _getterChain[0]).IsMapExistsProperty(map)) {
                return false;
            }
            Object result = ((MapEventPropertyGetter) _getterChain[0]).GetMap(map);
            return HandleIsExistsTrailingChain(result);
        }
    
        public Object Get(EventBean eventBean)
        {
            Object result = _getterChain[0].Get(eventBean);
            return HandleGetterTrailingChain(result);
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            if (!_getterChain[0].IsExistsProperty(eventBean)) {
                return false;
            }
            Object result = _getterChain[0].Get(eventBean);
            return HandleIsExistsTrailingChain(result);
        }
    
        private bool HandleIsExistsTrailingChain(Object result) {
            for (int i = 1; i < _getterChain.Length; i++)
            {
                if (result == null) {
                    return false;
                }
    
                EventPropertyGetter getter = _getterChain[i];
    
                if (i == _getterChain.Length - 1) {
                    if (getter is BeanEventPropertyGetter) {
                        return ((BeanEventPropertyGetter) getter).IsBeanExistsProperty(result);
                    }
                    else if (result is Map && getter is MapEventPropertyGetter) {
                        return ((MapEventPropertyGetter) getter).IsMapExistsProperty((Map)result);
                    }
                    else if (result is EventBean) {
                        return getter.IsExistsProperty((EventBean) result);
                    }
                    else {
                        return false;
                    }
                }
    
                if (getter is BeanEventPropertyGetter) {
                    result = ((BeanEventPropertyGetter) getter).GetBeanProp(result);
                }
                else if (result is Map && getter is MapEventPropertyGetter) {
                    result = ((MapEventPropertyGetter) getter).GetMap((Map)result);
                }
                else if (result is EventBean) {
                    result = getter.Get((EventBean) result);
                }
                else {
                    return false;
                }
            }
            return false;
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    
    
        private Object HandleGetterTrailingChain(Object result) {
    
            for (int i = 1; i < _getterChain.Length; i++)
            {
                if (result == null) {
                    return null;
                }
                EventPropertyGetter getter = _getterChain[i];
                if (result is EventBean) {
                    result = getter.Get((EventBean) result);
                }
                else if (getter is BeanEventPropertyGetter) {
                    result = ((BeanEventPropertyGetter) getter).GetBeanProp(result);
                }
                else if (result is Map && getter is MapEventPropertyGetter) {
                    result = ((MapEventPropertyGetter) getter).GetMap((Map)result);
                }
                else {
                    return null;
                }
            }
            return result;
        }
    }
}
