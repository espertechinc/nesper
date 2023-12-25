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
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.streamtype
{
    /// <summary>
    ///     Implementation that provides stream number and property type information.
    /// </summary>
    public class StreamTypeServiceImpl : StreamTypeService
    {
        private bool requireStreamNames;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="isOnDemandStreams">for on-demand stream</param>
        public StreamTypeServiceImpl(bool isOnDemandStreams)
            : this(
                Array.Empty<EventType>(),
                Array.Empty<string>(),
                Array.Empty<bool>(),
                isOnDemandStreams,
                false)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">a single event type for a single stream</param>
        /// <param name="streamName">the stream name of the single stream</param>
        /// <param name="isIStreamOnly">true for no datawindow for stream</param>
        public StreamTypeServiceImpl(
            EventType eventType,
            string streamName,
            bool isIStreamOnly)
            :
            this(new[] { eventType }, new[] { streamName }, new[] { isIStreamOnly }, false, false)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventTypes">array of event types, one for each stream</param>
        /// <param name="streamNames">array of stream names, one for each stream</param>
        /// <param name="isIStreamOnly">true for no datawindow for stream</param>
        /// <param name="isOnDemandStreams">true to indicate that all streams are on-demand pull-based</param>
        /// <param name="optionalStreams">if there are any streams that may not provide events, applicable to outer joins</param>
        public StreamTypeServiceImpl(
            EventType[] eventTypes,
            string[] streamNames,
            bool[] isIStreamOnly,
            bool isOnDemandStreams,
            bool optionalStreams)
        {
            EventTypes = eventTypes;
            StreamNames = streamNames;
            IStreamOnly = isIStreamOnly;
            IsOnDemandStreams = isOnDemandStreams;
            IsOptionalStreams = optionalStreams;

            if (eventTypes.Length != streamNames.Length) {
                throw new ArgumentException("Number of entries for event types and stream names differs");
            }

            HasTableTypes = DetermineHasTableTypes();
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="namesAndTypes">is the ordered list of stream names and event types available (stream zero to N)</param>
        /// <param name="isStreamZeroUnambigous">
        ///     indicates whether when a property is found in stream zero and another stream an exception should bethrown or the
        ///     stream zero should be assumed
        /// </param>
        /// <param name="requireStreamNames">
        ///     is true to indicate that stream names are required for any non-zero streams (for
        ///     subqueries)
        /// </param>
        public StreamTypeServiceImpl(
            LinkedHashMap<string, Pair<EventType, string>> namesAndTypes,
            bool isStreamZeroUnambigous,
            bool requireStreamNames)
        {
            IsStreamZeroUnambigous = isStreamZeroUnambigous;
            this.requireStreamNames = requireStreamNames;
            IStreamOnly = new bool[namesAndTypes.Count];
            EventTypes = new EventType[namesAndTypes.Count];
            StreamNames = new string[namesAndTypes.Count];
            var count = 0;
            foreach (var entry in namesAndTypes) {
                StreamNames[count] = entry.Key;
                EventTypes[count] = entry.Value.First;
                count++;
            }

            HasTableTypes = DetermineHasTableTypes();
            IsOptionalStreams = true;
        }

        public bool IsOptionalStreams { get; }

        public bool IsOnDemandStreams { get; }

        public EventType[] EventTypes { get; }

        public string[] StreamNames { get; }

        public bool[] IStreamOnly { get; }

        public bool OptionalStreams => IsOptionalStreams;

        public bool IsStreamZeroUnambigous { get; set; }

        public bool HasPropertyAgnosticType {
            get {
                foreach (var type in EventTypes) {
                    if (type is EventTypeSPI spi) {
                        if (spi.Metadata.IsPropertyAgnostic) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool HasTableTypes { get; }

        public bool RequireStreamNames {
            get => requireStreamNames;
            set => requireStreamNames = value;
        }


        private bool DetermineHasTableTypes()
        {
            foreach (var type in EventTypes) {
                if (type is EventTypeSPI typeSpi) {
                    if (typeSpi.Metadata.TypeClass == EventTypeTypeClass.TABLE_PUBLIC ||
                        typeSpi.Metadata.TypeClass == EventTypeTypeClass.TABLE_INTERNAL) {
                        return true;
                    }
                }
            }

            return false;
        }

        public int GetStreamNumForStreamName(string streamWildcard)
        {
            for (var i = 0; i < StreamNames.Length; i++) {
                if (streamWildcard.Equals(StreamNames[i])) {
                    return i;
                }
            }

            return -1;
        }

        public PropertyResolutionDescriptor ResolveByPropertyName(
            string propertyName,
            bool obtainFragment)
        {
            if (propertyName == null) {
                throw new ArgumentException("Null property name");
            }

            var desc = FindByPropertyName(propertyName, obtainFragment);
            if (requireStreamNames && desc.StreamNum != 0) {
                throw new PropertyNotFoundException(
                    "Property named '" +
                    propertyName +
                    "' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\"",
                    null);
            }

            return desc;
        }

        public PropertyResolutionDescriptor ResolveByPropertyNameExplicitProps(
            string propertyName,
            bool obtainFragment)
        {
            if (propertyName == null) {
                throw new ArgumentException("Null property name");
            }

            var desc = FindByPropertyNameExplicitProps(propertyName, obtainFragment);
            if (requireStreamNames && desc.StreamNum != 0) {
                throw new PropertyNotFoundException(
                    "Property named '" +
                    propertyName +
                    "' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\"",
                    null);
            }

            return desc;
        }

        public PropertyResolutionDescriptor ResolveByStreamAndPropName(
            string streamName,
            string propertyName,
            bool obtainFragment)
        {
            if (streamName == null) {
                throw new ArgumentException("Null property name");
            }

            if (propertyName == null) {
                throw new ArgumentException("Null property name");
            }

            return FindByStreamName(propertyName, streamName, false, obtainFragment);
        }

        public PropertyResolutionDescriptor ResolveByStreamAndPropNameExplicitProps(
            string streamName,
            string propertyName,
            bool obtainFragment)
        {
            if (streamName == null) {
                throw new ArgumentException("Null property name");
            }

            if (propertyName == null) {
                throw new ArgumentException("Null property name");
            }

            return FindByStreamName(propertyName, streamName, true, obtainFragment);
        }

        public PropertyResolutionDescriptor ResolveByStreamAndPropName(
            string streamAndPropertyName,
            bool obtainFragment)
        {
            if (streamAndPropertyName == null) {
                throw new ArgumentException("Null stream and property name");
            }

            PropertyResolutionDescriptor desc;
            try {
                // first try to resolve as a property name
                desc = FindByPropertyName(streamAndPropertyName, obtainFragment);
            }
            catch (PropertyNotFoundException ex) {
                // Attempt to resolve by extracting a stream name
                var index = StringValue.UnescapedIndexOfDot(streamAndPropertyName);
                if (index == -1) {
                    throw;
                }

                var streamName = streamAndPropertyName.Substring(0, index);
                var propertyName = streamAndPropertyName.Substring(index + 1);
                try {
                    // try to resolve a stream and property name
                    desc = FindByStreamName(propertyName, streamName, false, obtainFragment);
                }
                catch (StreamNotFoundException) {
                    // Consider the runtime URI as a further prefix
                    var propertyNoEnginePair = GetIsStreamQualified(propertyName);
                    if (propertyNoEnginePair == null) {
                        throw ex;
                    }

                    try {
                        return FindByStreamNameOnly(
                            propertyNoEnginePair.First,
                            propertyNoEnginePair.Second,
                            false,
                            obtainFragment);
                    }
                    catch (StreamNotFoundException) {
                        throw ex;
                    }
                }

                return desc;
            }

            return desc;
        }

        private PropertyResolutionDescriptor FindByPropertyName(
            string propertyName,
            bool obtainFragment)
        {
            var index = 0;
            var foundIndex = 0;
            var foundCount = 0;
            EventType streamType = null;

            for (var i = 0; i < EventTypes.Length; i++) {
                if (EventTypes[i] != null) {
                    Type propertyType = null;
                    var found = false;
                    FragmentEventType fragmentEventTypeX = null;

                    if (EventTypes[i].IsProperty(propertyName)) {
                        propertyType = EventTypes[i].GetPropertyType(propertyName);
                        if (obtainFragment) {
                            fragmentEventTypeX = EventTypes[i].GetFragmentType(propertyName);
                        }

                        found = true;
                    }
                    else {
                        // mapped(expression) or array(expression) are not property names but expressions against a property by name "mapped" or "array"
                        var descriptor = EventTypes[i].GetPropertyDescriptor(propertyName);
                        if (descriptor != null) {
                            found = true;
                            propertyType = descriptor.PropertyType;
                            if (descriptor.IsFragment && obtainFragment) {
                                fragmentEventTypeX = EventTypes[i].GetFragmentType(propertyName);
                            }
                        }
                    }

                    if (found) {
                        streamType = EventTypes[i];
                        foundCount++;
                        foundIndex = index;

                        // If the property could be resolved from stream 0 then we don't need to look further
                        if (i == 0 && IsStreamZeroUnambigous) {
                            return new PropertyResolutionDescriptor(
                                StreamNames[0],
                                EventTypes[0],
                                propertyName,
                                0,
                                propertyType,
                                fragmentEventTypeX);
                        }
                    }
                }

                index++;
            }

            HandleFindExceptions(propertyName, foundCount, streamType);

            FragmentEventType fragmentEventType = null;
            if (obtainFragment) {
                fragmentEventType = streamType.GetFragmentType(propertyName);
            }

            return new PropertyResolutionDescriptor(
                StreamNames[foundIndex],
                EventTypes[foundIndex],
                propertyName,
                foundIndex,
                streamType.GetPropertyType(propertyName),
                fragmentEventType);
        }

        private PropertyResolutionDescriptor FindByPropertyNameExplicitProps(
            string propertyName,
            bool obtainFragment)
        {
            var index = 0;
            var foundIndex = 0;
            var foundCount = 0;
            EventType streamType = null;

            for (var i = 0; i < EventTypes.Length; i++) {
                if (EventTypes[i] != null) {
                    var descriptors = EventTypes[i].PropertyDescriptors;
                    Type propertyType = null;
                    var found = false;
                    FragmentEventType fragmentEventTypeX = null;

                    foreach (var desc in descriptors) {
                        if (desc.PropertyName.Equals(propertyName)) {
                            propertyType = desc.PropertyType;
                            found = true;
                            if (obtainFragment && desc.IsFragment) {
                                fragmentEventTypeX = EventTypes[i].GetFragmentType(propertyName);
                            }
                        }
                    }

                    if (found) {
                        streamType = EventTypes[i];
                        foundCount++;
                        foundIndex = index;

                        // If the property could be resolved from stream 0 then we don't need to look further
                        if (i == 0 && IsStreamZeroUnambigous) {
                            return new PropertyResolutionDescriptor(
                                StreamNames[0],
                                EventTypes[0],
                                propertyName,
                                0,
                                propertyType,
                                fragmentEventTypeX);
                        }
                    }
                }

                index++;
            }

            HandleFindExceptions(propertyName, foundCount, streamType);

            FragmentEventType fragmentEventType = null;
            if (obtainFragment) {
                fragmentEventType = streamType.GetFragmentType(propertyName);
            }

            return new PropertyResolutionDescriptor(
                StreamNames[foundIndex],
                EventTypes[foundIndex],
                propertyName,
                foundIndex,
                streamType.GetPropertyType(propertyName),
                fragmentEventType);
        }

        private void HandleFindExceptions(
            string propertyName,
            int foundCount,
            EventType streamType)
        {
            if (foundCount > 1) {
                throw new DuplicatePropertyException(
                    "Property named '" + propertyName + "' is ambiguous as is valid for more then one stream");
            }

            if (streamType == null) {
                var message = "Property named '" + propertyName + "' is not valid in any stream";
                var msgGen = new PropertyNotFoundExceptionSuggestionGenMultiTyped(EventTypes, propertyName);
                throw new PropertyNotFoundException(message, msgGen);
            }
        }

        private PropertyResolutionDescriptor FindByStreamName(
            string propertyName,
            string streamName,
            bool explicitPropertiesOnly,
            bool obtainFragment)
        {
            return FindByStreamNameOnly(propertyName, streamName, explicitPropertiesOnly, obtainFragment);
        }

        private Pair<string, string> GetIsStreamQualified(string propertyName)
        {
            var index = StringValue.UnescapedIndexOfDot(propertyName);
            if (index == -1) {
                return null;
            }

            var streamNameNoEngine = propertyName.Substring(0, index);
            var propertyNameNoEngine = propertyName.Substring(index + 1);
            return new Pair<string, string>(propertyNameNoEngine, streamNameNoEngine);
        }

        private PropertyResolutionDescriptor FindByStreamNameOnly(
            string propertyName,
            string streamName,
            bool explicitPropertiesOnly,
            bool obtainFragment)
        {
            var index = 0;
            EventType streamType = null;

            // Stream name resolution examples:
            // A)  select A1.price from Event.price as A2  => mismatch stream name, cannot resolve
            // B)  select Event1.price from Event2.price   => mismatch event type name, cannot resolve
            for (var i = 0; i < EventTypes.Length; i++) {
                if (EventTypes[i] == null) {
                    index++;
                    continue;
                }

                if (StreamNames[i] != null && StreamNames[i].Equals(streamName)) {
                    streamType = EventTypes[i];
                    break;
                }

                // If the stream name is the event type name, that is also acceptable
                if (EventTypes[i].Name != null && EventTypes[i].Name.Equals(streamName)) {
                    streamType = EventTypes[i];
                    break;
                }

                index++;
            }

            if (streamType == null) {
                var message = "Failed to find a stream named '" + streamName + "'";
                var msgGen = new StreamNotFoundExceptionSuggestionGen(EventTypes, StreamNames, streamName);
                throw new StreamNotFoundException(message, msgGen);
            }

            Type propertyType = null;
            FragmentEventType fragmentEventType = null;

            if (!explicitPropertiesOnly) {
                propertyType = streamType.GetPropertyType(propertyName);
                if (propertyType == null) {
                    var desc = streamType.GetPropertyDescriptor(propertyName);
                    if (desc == null) {
                        throw HandlePropertyNotFound(propertyName, streamName, streamType);
                    }

                    propertyType = desc.PropertyType;
                    if (obtainFragment && desc.IsFragment) {
                        fragmentEventType = streamType.GetFragmentType(propertyName);
                    }
                }
                else {
                    if (obtainFragment) {
                        fragmentEventType = streamType.GetFragmentType(propertyName);
                    }
                }
            }
            else {
                var explicitProps = streamType.PropertyDescriptors;
                var found = false;
                foreach (var prop in explicitProps) {
                    if (prop.PropertyName.Equals(propertyName)) {
                        propertyType = prop.PropertyType;
                        if (obtainFragment && prop.IsFragment) {
                            fragmentEventType = streamType.GetFragmentType(propertyName);
                        }

                        found = true;
                        break;
                    }
                }

                if (!found) {
                    throw HandlePropertyNotFound(propertyName, streamName, streamType);
                }
            }

            return new PropertyResolutionDescriptor(
                streamName,
                streamType,
                propertyName,
                index,
                propertyType,
                fragmentEventType);
        }

        private PropertyNotFoundException HandlePropertyNotFound(
            string propertyName,
            string streamName,
            EventType streamType)
        {
            var message = "Property named '" + propertyName + "' is not valid in stream '" + streamName + "'";
            var msgGen = new PropertyNotFoundExceptionSuggestionGenSingleTyped(streamType, propertyName);
            return new PropertyNotFoundException(message, msgGen);
        }

        internal class PropertyNotFoundExceptionSuggestionGenMultiTyped : StreamTypesExceptionSuggestionGen
        {
            private readonly EventType[] _eventTypes;
            private readonly string _propertyName;

            internal PropertyNotFoundExceptionSuggestionGenMultiTyped(
                EventType[] eventTypes,
                string propertyName)
            {
                this._eventTypes = eventTypes;
                this._propertyName = propertyName;
            }

            public Pair<int, string> Suggestion => StreamTypeServiceUtil.FindLevMatch(_eventTypes, _propertyName);
        }

        internal class PropertyNotFoundExceptionSuggestionGenSingleTyped : StreamTypesExceptionSuggestionGen
        {
            private readonly EventType _eventType;
            private readonly string _propertyName;

            internal PropertyNotFoundExceptionSuggestionGenSingleTyped(
                EventType eventType,
                string propertyName)
            {
                this._eventType = eventType;
                this._propertyName = propertyName;
            }

            public Pair<int, string> Suggestion => StreamTypeServiceUtil.FindLevMatch(_propertyName, _eventType);
        }

        internal class StreamNotFoundExceptionSuggestionGen : StreamTypesExceptionSuggestionGen
        {
            private readonly EventType[] _eventTypes;
            private readonly string _streamName;
            private readonly string[] _streamNames;

            internal StreamNotFoundExceptionSuggestionGen(
                EventType[] eventTypes,
                string[] streamNames,
                string streamName)
            {
                this._eventTypes = eventTypes;
                this._streamNames = streamNames;
                this._streamName = streamName;
            }

            public Pair<int, string> Suggestion {
                get {
                    // find a near match, textually
                    string bestMatch = null;
                    var bestMatchDiff = int.MaxValue;

                    for (var i = 0; i < _eventTypes.Length; i++) {
                        if (_streamNames[i] != null) {
                            var diff = LevenshteinDistance.ComputeLevenshteinDistance(_streamNames[i], _streamName);
                            if (diff < bestMatchDiff) {
                                bestMatchDiff = diff;
                                bestMatch = _streamNames[i];
                            }
                        }

                        if (_eventTypes[i] == null) {
                            continue;
                        }

                        // If the stream name is the event type name, that is also acceptable
                        if (_eventTypes[i].Name != null) {
                            var diff = LevenshteinDistance.ComputeLevenshteinDistance(_eventTypes[i].Name, _streamName);
                            if (diff < bestMatchDiff) {
                                bestMatchDiff = diff;
                                bestMatch = _eventTypes[i].Name;
                            }
                        }
                    }

                    Pair<int, string> suggestion = null;
                    if (bestMatchDiff < int.MaxValue) {
                        suggestion = new Pair<int, string>(bestMatchDiff, bestMatch);
                    }

                    return suggestion;
                }
            }
        }
    }
} // end of namespace