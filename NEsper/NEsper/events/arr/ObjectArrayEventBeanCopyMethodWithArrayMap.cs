///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// Copy method for Map-underlying events.
    /// </summary>
    public class ObjectArrayEventBeanCopyMethodWithArrayMap : EventBeanCopyMethod
    {
        private readonly ICollection<int> _arrayIndexesToCopy;
        private readonly EventAdapterService _eventAdapterService;
        private readonly ObjectArrayEventType _eventType;
        private readonly ICollection<int> _mapIndexesToCopy;

        public ObjectArrayEventBeanCopyMethodWithArrayMap(ObjectArrayEventType eventType,
                                                          EventAdapterService eventAdapterService,
                                                          ICollection<String> mapPropertiesToCopy,
                                                          ICollection<String> arrayPropertiesToCopy,
                                                          IDictionary<String, int> propertiesIndexes)
        {
            _eventType = eventType;
            _eventAdapterService = eventAdapterService;

            _mapIndexesToCopy = new HashSet<int>();
            foreach (String prop in mapPropertiesToCopy)
            {
                int index;

                if (propertiesIndexes.TryGetValue(prop, out index))
                {
                    _mapIndexesToCopy.Add(index);
                }
            }

            _arrayIndexesToCopy = new HashSet<int>();
            foreach (String prop in arrayPropertiesToCopy)
            {
                int? index = propertiesIndexes.Get(prop);
                if (index != null)
                {
                    _arrayIndexesToCopy.Add(index.Value);
                }
            }
        }

        #region EventBeanCopyMethod Members

        public EventBean Copy(EventBean theEvent)
        {
            var arrayBacked = (ObjectArrayBackedEventBean) theEvent;
            object[] props = arrayBacked.Properties;
            var shallowCopy = new Object[props.Length];
            Array.Copy(props, 0, shallowCopy, 0, props.Length);

            foreach (int index in _mapIndexesToCopy)
            {
                var innerMap = (IDictionary<String, Object>) shallowCopy[index];
                if (innerMap != null)
                {
                    var copy = new Dictionary<String, Object>(innerMap);
                    shallowCopy[index] = copy;
                }
            }

            foreach (int index in _arrayIndexesToCopy)
            {
                var array = shallowCopy[index] as Array;
                if (array != null && array.Length != 0)
                {
                    Array copied = Array.CreateInstance(array.GetType().GetElementType(), array.Length);
                    Array.Copy(array, 0, copied, 0, array.Length);
                    shallowCopy[index] = copied;
                }
            }
            return _eventAdapterService.AdapterForTypedObjectArray(shallowCopy, _eventType);
        }

        #endregion
    }
}