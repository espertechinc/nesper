///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Copy method for Map-underlying events.
    /// </summary>
    public class ObjectArrayEventBeanCopyMethodWithArrayMap : EventBeanCopyMethod
    {
        private readonly int[] _arrayIndexesToCopy;
        private readonly EventBeanTypedEventFactory _eventAdapterService;
        private readonly ObjectArrayEventType _eventType;
        private readonly int[] _mapIndexesToCopy;

        public ObjectArrayEventBeanCopyMethodWithArrayMap(
            ObjectArrayEventType eventType,
            EventBeanTypedEventFactory eventAdapterService,
            int[] mapIndexesToCopy,
            int[] arrayIndexesToCopy)
        {
            this._eventType = eventType;
            this._eventAdapterService = eventAdapterService;
            this._mapIndexesToCopy = mapIndexesToCopy;
            this._arrayIndexesToCopy = arrayIndexesToCopy;
        }

        public EventBean Copy(EventBean theEvent)
        {
            var arrayBacked = (ObjectArrayBackedEventBean) theEvent;
            var props = arrayBacked.Properties;
            var shallowCopy = new object[props.Length];
            Array.Copy(props, 0, shallowCopy, 0, props.Length);

            foreach (var index in _mapIndexesToCopy) {
                var innerMap = (IDictionary<string, object>) shallowCopy[index];
                if (innerMap != null) {
                    var copy = new Dictionary<string, object>(innerMap);
                    shallowCopy[index] = copy;
                }
            }

            foreach (var index in _arrayIndexesToCopy) {
                var array = shallowCopy[index] as Array;
                if (array != null && array.Length != 0) {
                    var elementType = array.GetType().GetElementType();
                    var copied = Arrays.CreateInstanceChecked(elementType, array.Length);
                    array.CopyTo(copied, 0);
                    shallowCopy[index] = copied;
                }
            }

            return _eventAdapterService.AdapterForTypedObjectArray(shallowCopy, _eventType);
        }
    }
} // end of namespace