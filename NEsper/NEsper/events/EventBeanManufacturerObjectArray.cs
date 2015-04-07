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
using com.espertech.esper.compat;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Factory for ObjectArray-underlying events.
    /// </summary>
    public class EventBeanManufacturerObjectArray : EventBeanManufacturer
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly ObjectArrayEventType _eventType;
        private readonly int[] _indexPerWritable;
        private readonly bool _oneToOne;

        /// <summary>Ctor. </summary>
        /// <param name="eventType">type to create</param>
        /// <param name="eventAdapterService">event factory</param>
        /// <param name="properties">written properties</param>
        public EventBeanManufacturerObjectArray(ObjectArrayEventType eventType,
                                                EventAdapterService eventAdapterService,
                                                IList<WriteablePropertyDescriptor> properties)
        {
            _eventAdapterService = eventAdapterService;
            _eventType = eventType;

            IDictionary<String, int> indexes = eventType.PropertiesIndexes;
            _indexPerWritable = new int[properties.Count];
            bool oneToOneMapping = true;
            for (int i = 0; i < properties.Count; i++)
            {
                String propertyName = properties[i].PropertyName;
                int index;
                if (!indexes.TryGetValue(propertyName, out index))
                {
                    throw new IllegalStateException("Failed to find property '" + propertyName +
                                                    "' among the array indexes");
                }
                _indexPerWritable[i] = index;
                if (index != i)
                {
                    oneToOneMapping = false;
                }
            }
            _oneToOne = oneToOneMapping && properties.Count == eventType.PropertyNames.Length;
        }

        #region EventBeanManufacturer Members

        public EventBean Make(Object[] properties)
        {
            var cols = MakeUnderlying(properties) as object[];
            return _eventAdapterService.AdapterForTypedObjectArray(cols, _eventType);
        }

        #endregion

        public object MakeUnderlying(Object[] properties)
        {
            if (_oneToOne)
            {
                return properties;
            }
            var cols = new object[_eventType.PropertyNames.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                int indexToWrite = _indexPerWritable[i];
                cols[indexToWrite] = properties[i];
            }
            return cols;
        }
    }
}