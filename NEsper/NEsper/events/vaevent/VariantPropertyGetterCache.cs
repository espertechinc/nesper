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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// A thread-safe cache for property getters per event type.
    /// <para/>
    /// Since most often getters are used in a row for the same type, keeps a row of last
    /// used getters for fast lookup based on type.
    /// </summary>
    public class VariantPropertyGetterCache
    {
        private volatile EventType[] _knownTypes;
        private volatile VariantPropertyGetterRow _lastUsedGetters;
        private readonly List<String> _properties;
        private IDictionary<EventType, VariantPropertyGetterRow> _allGetters;
        private readonly ILockable _iLock;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lockManager">The lock manager.</param>
        /// <param name="knownTypes">types known at cache construction type, may be an empty list for the ANY type variance.</param>
        public VariantPropertyGetterCache(ILockManager lockManager, EventType[] knownTypes)
        {
            _iLock = lockManager.CreateLock(GetType());
            _knownTypes = knownTypes;
            _allGetters = new Dictionary<EventType, VariantPropertyGetterRow>();
            _properties = new List<String>();
        }
    
        /// <summary>Adds the getters for a property that is identified by a property number which indexes into array of getters per type. </summary>
        /// <param name="assignedPropertyNumber">number of property</param>
        /// <param name="propertyName">to add</param>
        public void AddGetters(int assignedPropertyNumber, String propertyName)
        {
            foreach (EventType type in _knownTypes)
            {
                EventPropertyGetter getter = type.GetGetter(propertyName);
    
                VariantPropertyGetterRow row = _allGetters.Get(type);
                if (row == null)
                {
                    using(_iLock.Acquire())
                    {
                        row = new VariantPropertyGetterRow(type, new EventPropertyGetter[assignedPropertyNumber + 1]);
                        _allGetters.Put(type, row);
                    }
                }
                row.AddGetter(assignedPropertyNumber, getter);
            }
            _properties.Add(propertyName);
        }
    
        /// <summary>Fast lookup of a getter for a property and type. </summary>
        /// <param name="assignedPropertyNumber">number of property to use as index</param>
        /// <param name="eventType">type of underlying event</param>
        /// <returns>getter</returns>
        public EventPropertyGetter GetGetter(int assignedPropertyNumber, EventType eventType)
        {
            VariantPropertyGetterRow lastGetters = _lastUsedGetters;
            if ((lastGetters != null) && (lastGetters.EventType == eventType))
            {
                return lastGetters.GetterPerProp[assignedPropertyNumber];
            }
    
            VariantPropertyGetterRow row = _allGetters.Get(eventType);
    
            // newly seen type (Using ANY type variance or as a subtype of an existing variance type)
            // synchronized add, if added twice then that is ok too
            if (row == null)
            {
                lock(this)
                {
                    row = _allGetters.Get(eventType);
                    if (row == null)
                    {
                        row = AddType(eventType);
                    }
                }            
            }
    
            EventPropertyGetter getter = row.GetterPerProp[assignedPropertyNumber];
            _lastUsedGetters = row;
            return getter;
        }
    
        private VariantPropertyGetterRow AddType(EventType eventType)
        {
            var newKnownTypes = (EventType[]) ResizeArray(_knownTypes, _knownTypes.Length + 1);
            newKnownTypes[newKnownTypes.Length - 1] = eventType;
    
            // create getters
            var getters = new EventPropertyGetter[_properties.Count];
            for (int i = 0; i < _properties.Count; i++)
            {
                getters[i] = eventType.GetGetter(_properties[i]);
            }
    
            var row = new VariantPropertyGetterRow(eventType, getters);

            var newAllGetters = new Dictionary<EventType, VariantPropertyGetterRow>();
            newAllGetters.PutAll(_allGetters);
            newAllGetters.Put(eventType, row);
    
            // overlay volatiles
            _knownTypes = newKnownTypes;
            _allGetters = newAllGetters;
            
            return row;
        }
    
        private static Array ResizeArray(Array oldArray, int newSize)
        {
            var elementType = oldArray.GetType().GetElementType();
            var newArray = Array.CreateInstance(elementType, newSize);
            var oldSize = oldArray.Length;
            
            Array.Copy(
                oldArray,
                newArray,
                Math.Min(oldSize, newSize));

            return newArray;
        }
    
        private class VariantPropertyGetterRow
        {
            public VariantPropertyGetterRow(EventType eventType, EventPropertyGetter[] getterPerProp)
            {
                EventType = eventType;
                GetterPerProp = getterPerProp;
            }

            public EventType EventType { get; private set; }

            public EventPropertyGetter[] GetterPerProp { get; private set; }

            public void AddGetter(int assignedPropertyNumber, EventPropertyGetter getter)
            {
                if (assignedPropertyNumber > (GetterPerProp.Length - 1))
                {
                    GetterPerProp = (EventPropertyGetter[]) ResizeArray(GetterPerProp, GetterPerProp.Length + 10);
                }
                GetterPerProp[assignedPropertyNumber] = getter;
            }
        }
    }
}
