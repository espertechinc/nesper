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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.variant
{
    /// <summary>
    ///     A thread-safe cache for property getters per event type.
    ///     <para />
    ///     Since most often getters are used in a row for the same type, keeps a row of last used getters for
    ///     fast lookup based on type.
    /// </summary>
    public class VariantPropertyGetterCache
    {
        private readonly IList<string> _properties;
        private IDictionary<EventType, VariantPropertyGetterRow> _allGetters;
        private volatile EventType[] _knownTypes;
        private volatile VariantPropertyGetterRow _lastUsedGetters;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="knownTypes">types known at cache construction type, may be an empty list for the ANY type variance.</param>
        public VariantPropertyGetterCache(EventType[] knownTypes)
        {
            this._knownTypes = knownTypes;
            _allGetters = new Dictionary<EventType, VariantPropertyGetterRow>();
            _properties = new List<string>();
        }

        /// <summary>
        ///     Adds the getters for a property that is identified by a property number which indexes into array of getters per
        ///     type.
        /// </summary>
        /// <param name="propertyName">to add</param>
        public void AddGetters(string propertyName)
        {
            foreach (var type in _knownTypes) {
                var getter = type.GetGetter(propertyName);

                var row = _allGetters.Get(type);
                if (row == null) {
                    lock (this) {
                        row = new VariantPropertyGetterRow(type, new Dictionary<string, EventPropertyGetter>());
                        _allGetters.Put(type, row);
                    }
                }

                row.AddGetter(propertyName, getter);
            }

            _properties.Add(propertyName);
        }

        /// <summary>
        ///     Fast lookup of a getter for a property and type.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="eventType">type of underlying event</param>
        /// <returns>getter</returns>
        public EventPropertyGetter GetGetter(
            string propertyName,
            EventType eventType)
        {
            var lastGetters = _lastUsedGetters;
            if (lastGetters != null && lastGetters.EventType == eventType) {
                var getterInner = lastGetters.GetterPerProp.Get(propertyName);
                if (getterInner == null) {
                    getterInner = eventType.GetGetter(propertyName);
                    lastGetters.AddGetter(propertyName, getterInner);
                }

                return getterInner;
            }

            var row = _allGetters.Get(eventType);

            // newly seen type (Using ANY type variance or as a subtype of an existing variance type)
            // synchronized add, if added twice then that is ok too
            if (row == null) {
                lock (this) {
                    row = _allGetters.Get(eventType);
                    if (row == null) {
                        row = AddType(eventType);
                    }
                }
            }

            var getter = row.GetterPerProp.Get(propertyName);
            _lastUsedGetters = row;

            if (getter == null) {
                getter = eventType.GetGetter(propertyName);
                row.AddGetter(propertyName, getter);
            }

            return getter;
        }

        private VariantPropertyGetterRow AddType(EventType eventType)
        {
            var newKnownTypes = (EventType[]) ResizeArray(_knownTypes, _knownTypes.Length + 1);
            newKnownTypes[newKnownTypes.Length - 1] = eventType;

            // create getters
            IDictionary<string, EventPropertyGetter> getters = new Dictionary<string, EventPropertyGetter>();
            for (var i = 0; i < _properties.Count; i++) {
                var propertyName = _properties[i];
                var getter = eventType.GetGetter(propertyName);
                getters.Put(propertyName, getter);
            }

            var row = new VariantPropertyGetterRow(eventType, getters);

            IDictionary<EventType, VariantPropertyGetterRow> newAllGetters =
                new Dictionary<EventType, VariantPropertyGetterRow>();
            newAllGetters.PutAll(_allGetters);
            newAllGetters.Put(eventType, row);

            // overlay volatiles
            _knownTypes = newKnownTypes;
            _allGetters = newAllGetters;

            return row;
        }

        private static Array ResizeArray(
            Array oldArray,
            int newSize)
        {
            var oldSize = oldArray.Length;
            var elementType = oldArray.GetType().GetElementType();
            var newArray = Arrays.CreateInstanceChecked(elementType, newSize);
            var preserveLength = Math.Min(oldSize, newSize);
            if (preserveLength > 0) {
                Array.Copy(oldArray, 0, newArray, 0, preserveLength);
            }

            return newArray;
        }

        internal class VariantPropertyGetterRow
        {
            internal VariantPropertyGetterRow(
                EventType eventType,
                IDictionary<string, EventPropertyGetter> getterPerProp)
            {
                EventType = eventType;
                GetterPerProp = getterPerProp;
            }

            public EventType EventType { get; }

            public IDictionary<string, EventPropertyGetter> GetterPerProp { get; }

            public void AddGetter(
                string propertyName,
                EventPropertyGetter getter)
            {
                GetterPerProp.Put(propertyName, getter);
            }
        }
    }
} // end of namespace