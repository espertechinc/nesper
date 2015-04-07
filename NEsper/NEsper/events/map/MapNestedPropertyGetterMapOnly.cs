///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>
    /// Getter for one or more levels deep nested properties of maps.
    /// </summary>
    public class MapNestedPropertyGetterMapOnly : MapEventPropertyGetter
    {
        private readonly MapEventPropertyGetter[] _mapGetterChain;
    
        /// <summary>Ctor. </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="eventAdaperService">is a factory for PONO bean event types</param>
        public MapNestedPropertyGetterMapOnly(IList<EventPropertyGetter> getterChain, EventAdapterService eventAdaperService)
        {
            _mapGetterChain = new MapEventPropertyGetter[getterChain.Count];
            for (int i = 0; i < getterChain.Count; i++)
            {
                _mapGetterChain[i] = (MapEventPropertyGetter) getterChain[i];
            }
        }
    
        public Object GetMap(IDictionary<String, Object> map)
        {
            Object result = _mapGetterChain[0].GetMap(map);
            return HandleGetterTrailingChain(result);
        }
    
        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            if (!_mapGetterChain[0].IsMapExistsProperty(map)) {
                return false;
            }
            Object result = _mapGetterChain[0].GetMap(map);
            return HandleIsExistsTrailingChain(result);
        }
    
        public Object Get(EventBean eventBean)
        {
            Object result = _mapGetterChain[0].Get(eventBean);
            return HandleGetterTrailingChain(result);
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            if (!_mapGetterChain[0].IsExistsProperty(eventBean)) {
                return false;
            }
            Object result = _mapGetterChain[0].Get(eventBean);
            return HandleIsExistsTrailingChain(result);
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    
        private bool HandleIsExistsTrailingChain(Object result) {
            for (int i = 1; i < _mapGetterChain.Length; i++)
            {
                if (result == null) {
                    return false;
                }
    
                MapEventPropertyGetter getter = _mapGetterChain[i];
    
                if (i == _mapGetterChain.Length - 1) {
                    if (!(result is Map)) {
                        if (result is EventBean) {
                            return getter.IsExistsProperty((EventBean) result);
                        }
                        return false;
                    }
                    else {
                        return getter.IsMapExistsProperty((IDictionary<String, Object>) result);
                    }
                }
    
                if (!(result is Map)) {
                    if (result is EventBean) {
                        result = getter.Get((EventBean) result);
                    }
                    else {
                        return false;
                    }
                }
                else {
                    result = getter.GetMap((IDictionary<String, Object>) result);
                }
            }
            return true;
        }
    
        private Object HandleGetterTrailingChain(Object result) {
            for (int i = 1; i < _mapGetterChain.Length; i++)
            {
                if (result == null) {
                    return null;
                }
    
                MapEventPropertyGetter getter = _mapGetterChain[i];
                if (!(result is Map)) {
                    if (result is EventBean) {
                        result = getter.Get((EventBean) result);
                    }
                    else {
                        return null;
                    }
                }
                else {
                    result = getter.GetMap((IDictionary<String, Object>) result);
                }
            }
            return result;
        }
    }
}
