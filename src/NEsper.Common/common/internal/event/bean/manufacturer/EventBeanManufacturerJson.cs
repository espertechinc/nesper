///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    ///     Factory for Json-underlying events.
    /// </summary>
    public class EventBeanManufacturerJson : EventBeanManufacturer
    {
        private readonly EventBeanTypedEventFactory _eventAdapterService;
        private readonly JsonEventType _jsonEventType;
        private readonly int[] _nativeNums;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="jsonEventType">type to create</param>
        /// <param name="eventAdapterService">event factory</param>
        /// <param name="nativeNums">native field numbers</param>
        public EventBeanManufacturerJson(
            JsonEventType jsonEventType,
            EventBeanTypedEventFactory eventAdapterService,
            int[] nativeNums)
        {
            _eventAdapterService = eventAdapterService;
            _jsonEventType = jsonEventType;
            _nativeNums = nativeNums;
        }

        public EventBean Make(object[] properties)
        {
            var values = MakeUnderlying(properties);
            return _eventAdapterService.AdapterForTypedJson(values, _jsonEventType);
        }

        public object MakeUnderlying(object[] properties)
        {
            var @delegate = _jsonEventType.Delegate;
            var underlying = @delegate.Allocate();
            for (var i = 0; i < properties.Length; i++) {
                @delegate.TrySetProperty(_nativeNums[i], properties[i], underlying);
            }

            return underlying;
        }

        internal static int[] FindPropertyIndexes(
            JsonEventType jsonEventType,
            WriteablePropertyDescriptor[] writables)
        {
            var nativeNums = new int[writables.Length];
            for (var i = 0; i < writables.Length; i++) {
                nativeNums[i] = FindPropertyIndex(jsonEventType, writables[i].PropertyName);
            }

            return nativeNums;
        }

        internal static int FindPropertyIndex(
            JsonEventType jsonEventType,
            string propertyName)
        {
            var types = jsonEventType.Types;
            var index = 0;
            foreach (var entry in types) {
                if (entry.Key.Equals(propertyName)) {
                    return index;
                }

                index++;
            }

            throw new IllegalStateException("Failed to find writable property '" + propertyName + "'");
        }
    }
} // end of namespace