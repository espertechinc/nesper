///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Implementation that provides stream number and property type information.
    /// </summary>
    [Serializable]
    public class StreamTypeServiceImpl : StreamTypeService
    {
        private readonly EventType[] _eventTypes;
        private readonly String[] _streamNames;
        private readonly bool[] _isIStreamOnly;
        private readonly String _engineURIQualifier;
        private bool _isStreamZeroUnambigous;
        private bool _requireStreamNames;
        private readonly bool _isOnDemandStreams;
        private bool _hasTableTypes;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="isOnDemandStreams"></param>
        public StreamTypeServiceImpl(String engineURI, bool isOnDemandStreams)
            : this(new EventType[0], new String[0], new bool[0], engineURI, isOnDemandStreams)
        {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">a single event type for a single stream</param>
        /// <param name="streamName">the stream name of the single stream</param>
        /// <param name="engineURI">engine URI</param>
        /// <param name="isIStreamOnly">true for no datawindow for stream</param>
        public StreamTypeServiceImpl (EventType eventType, String streamName, bool isIStreamOnly, String engineURI)
            : this(new EventType[] { eventType }, new String[] { streamName }, new bool[] { isIStreamOnly }, engineURI, false)
        {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventTypes">array of event types, one for each stream</param>
        /// <param name="streamNames">array of stream names, one for each stream</param>
        /// <param name="isIStreamOnly">true for no datawindow for stream</param>
        /// <param name="engineURI">engine URI</param>
        /// <param name="isOnDemandStreams">true to indicate that all streams are on-demand pull-based</param>
        public StreamTypeServiceImpl(EventType[] eventTypes, String[] streamNames, bool[] isIStreamOnly, String engineURI, bool isOnDemandStreams)
        {
            _eventTypes = eventTypes;
            _streamNames = streamNames;
            _isIStreamOnly = isIStreamOnly;
            _isOnDemandStreams = isOnDemandStreams;

            if (engineURI == null || EPServiceProviderConstants.DEFAULT_ENGINE_URI.Equals(engineURI))
            {
                _engineURIQualifier = EPServiceProviderConstants.DEFAULT_ENGINE_URI__QUALIFIER;
            }
            else
            {
                _engineURIQualifier = engineURI;
            }
    
            if (eventTypes.Length != streamNames.Length)
            {
                throw new ArgumentException("Number of entries for event types and stream names differs");
            }

            _hasTableTypes = DetermineHasTableTypes();
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="namesAndTypes">is the ordered list of stream names and event types available (stream zero to Count)</param>
        /// <param name="isStreamZeroUnambigous">indicates whether when a property is found in stream zero and another stream an exception should bethrown or the stream zero should be assumed
        /// </param>
        /// <param name="engineURI">uri of the engine</param>
        /// <param name="requireStreamNames">is true to indicate that stream names are required for any non-zero streams (for subqueries)</param>
        public StreamTypeServiceImpl(LinkedHashMap<String, Pair<EventType, String>> namesAndTypes, String engineURI, bool isStreamZeroUnambigous, bool requireStreamNames)
        {
            _isStreamZeroUnambigous = isStreamZeroUnambigous;
            _requireStreamNames = requireStreamNames;
            _engineURIQualifier = engineURI;
            _isIStreamOnly = new bool[namesAndTypes.Count];
            _eventTypes = new EventType[namesAndTypes.Count] ;
            _streamNames = new String[namesAndTypes.Count] ;

            var count = 0;
            foreach (var entry in namesAndTypes)
            {
                _streamNames[count] = entry.Key;
                _eventTypes[count] = entry.Value.First;
                count++;
            }

            _hasTableTypes = DetermineHasTableTypes();
        }

        private bool DetermineHasTableTypes()
        {
            return _eventTypes.OfType<EventTypeSPI>().Any(typeSPI => typeSPI.Metadata.TypeClass == TypeClass.TABLE);
        }

        public bool RequireStreamNames
        {
            get { return _requireStreamNames; }
            set { _requireStreamNames = value; }
        }

        public bool IsOnDemandStreams
        {
            get { return _isOnDemandStreams; }
        }

        public EventType[] EventTypes
        {
            get { return _eventTypes; }
        }

        public string[] StreamNames
        {
            get { return _streamNames; }
        }

        public bool[] IsIStreamOnly
        {
            get { return _isIStreamOnly; }
        }

        public int GetStreamNumForStreamName(String streamWildcard)
        {
            for (var i = 0; i < _streamNames.Length; i++)
            {
                if (streamWildcard.Equals(_streamNames[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    
        public PropertyResolutionDescriptor ResolveByPropertyName(String propertyName, bool obtainFragment)
            
        {
            if (propertyName == null)
            {
                throw new ArgumentException("Null property name");
            }
            var desc = FindByPropertyName(propertyName, obtainFragment);
            if ((_requireStreamNames) && (desc.StreamNum != 0))
            {
                throw new PropertyNotFoundException("Property named '" + propertyName + "' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\"", null);
            }
            return desc;
        }
    
        public PropertyResolutionDescriptor ResolveByPropertyNameExplicitProps(String propertyName, bool obtainFragment) {
            if (propertyName == null)
            {
                throw new ArgumentException("Null property name");
            }
            var desc = FindByPropertyNameExplicitProps(propertyName, obtainFragment);
            if ((_requireStreamNames) && (desc.StreamNum != 0))
            {
                throw new PropertyNotFoundException("Property named '" + propertyName + "' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\"", null);
            }
            return desc;
        }
    
        public PropertyResolutionDescriptor ResolveByStreamAndPropName(String streamName, String propertyName, bool obtainFragment)
            
        {
            if (streamName == null)
            {
                throw new ArgumentException("Null property name");
            }
            if (propertyName == null)
            {
                throw new ArgumentException("Null property name");
            }
            return FindByStreamAndEngineName(propertyName, streamName, false, obtainFragment);
        }
    
        public PropertyResolutionDescriptor ResolveByStreamAndPropNameExplicitProps(String streamName, String propertyName, bool obtainFragment) {
            if (streamName == null)
            {
                throw new ArgumentException("Null property name");
            }
            if (propertyName == null)
            {
                throw new ArgumentException("Null property name");
            }
            return FindByStreamAndEngineName(propertyName, streamName, true, obtainFragment);
        }
    
        public PropertyResolutionDescriptor ResolveByStreamAndPropName(String streamAndPropertyName, bool obtainFragment) 
        {
            if (streamAndPropertyName == null)
            {
                throw new ArgumentException("Null stream and property name");
            }
    
            PropertyResolutionDescriptor desc;
            try
            {
                // first try to resolve as a property name
                desc = FindByPropertyName(streamAndPropertyName, obtainFragment);
            }
            catch (PropertyNotFoundException ex)
            {
                // Attempt to resolve by extracting a stream name
                var index = ASTUtil.UnescapedIndexOfDot(streamAndPropertyName);
                if (index == -1)
                {
                    throw;
                }
                var streamName = streamAndPropertyName.Substring(0, index);
                var propertyName = streamAndPropertyName.Substring(index + 1);
                try
                {
                    // try to resolve a stream and property name
                    desc = FindByStreamAndEngineName(propertyName, streamName, false, obtainFragment);
                }
                catch (StreamNotFoundException)
                {
                    // Consider the engine URI as a further prefix
                    var propertyNoEnginePair = GetIsEngineQualified(propertyName, streamName);
                    if (propertyNoEnginePair == null)
                    {
                        throw ex;
                    }
                    try
                    {
                        return FindByStreamNameOnly(propertyNoEnginePair.First, propertyNoEnginePair.Second, false, obtainFragment);
                    }
                    catch (StreamNotFoundException)
                    {
                        throw ex;
                    }
                }
                return desc;
            }
    
            return desc;
        }
    
        private PropertyResolutionDescriptor FindByPropertyName(String propertyName, bool obtainFragment)
            
        {
            var index = 0;
            var foundIndex = 0;
            var foundCount = 0;
            EventType streamType = null;
    
            for (var i = 0; i < _eventTypes.Length; i++)
            {
                if (_eventTypes[i] != null)
                {
                    Type propertyType = null;
                    var found = false;
                    FragmentEventType fragmentEventTypeX = null;
                    
                    if (_eventTypes[i].IsProperty(propertyName)) {
                        propertyType = _eventTypes[i].GetPropertyType(propertyName);
                        if (obtainFragment) {
                            fragmentEventTypeX = _eventTypes[i].GetFragmentType(propertyName);
                        }
                        found = true;
                    }
                    else {
                        // mapped(expression) or array(expression) are not property names but expressions against a property by name "mapped" or "array"
                        var descriptor = _eventTypes[i].GetPropertyDescriptor(propertyName);
                        if (descriptor != null) {
                            found = true;
                            propertyType = descriptor.PropertyType;
                            if (descriptor.IsFragment && obtainFragment) {
                                fragmentEventTypeX =  _eventTypes[i].GetFragmentType(propertyName);
                            }
                        }
                    }
    
                    if (found) {
                        streamType = _eventTypes[i];
                        foundCount++;
                        foundIndex = index;
    
                        // If the property could be resolved from stream 0 then we don't need to look further
                        if ((i == 0) && _isStreamZeroUnambigous)
                        {
                            return new PropertyResolutionDescriptor(_streamNames[0], _eventTypes[0], propertyName, 0, propertyType, fragmentEventTypeX);
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
    
            return new PropertyResolutionDescriptor(_streamNames[foundIndex], _eventTypes[foundIndex], propertyName, foundIndex, streamType.GetPropertyType(propertyName), fragmentEventType);
        }
    
        private PropertyResolutionDescriptor FindByPropertyNameExplicitProps(String propertyName, bool obtainFragment)
        {
            var index = 0;
            var foundIndex = 0;
            var foundCount = 0;
            EventType streamType = null;
    
            for (var i = 0; i < _eventTypes.Length; i++)
            {
                if (_eventTypes[i] != null)
                {
                    var descriptors  = _eventTypes[i].PropertyDescriptors;
                    Type propertyType = null;
                    var found = false;
                    FragmentEventType fragmentEventTypeX = null;
    
                    foreach (var desc in descriptors) {
                        if (desc.PropertyName.Equals(propertyName)) {
                            propertyType = desc.PropertyType;
                            found = true;
                            if (obtainFragment && desc.IsFragment) {
                                fragmentEventTypeX = _eventTypes[i].GetFragmentType(propertyName);
                            }
                        }
                    }
    
                    if (found) {
                        streamType = _eventTypes[i];
                        foundCount++;
                        foundIndex = index;
    
                        // If the property could be resolved from stream 0 then we don't need to look further
                        if ((i == 0) && _isStreamZeroUnambigous)
                        {
                            return new PropertyResolutionDescriptor(_streamNames[0], _eventTypes[0], propertyName, 0, propertyType, fragmentEventTypeX);
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
    
            return new PropertyResolutionDescriptor(_streamNames[foundIndex], _eventTypes[foundIndex], propertyName, foundIndex, streamType.GetPropertyType(propertyName), fragmentEventType);
        }
    
        private void HandleFindExceptions(String propertyName, int foundCount, EventType streamType) {
            if (foundCount > 1)
            {
                throw new DuplicatePropertyException("Property named '" + propertyName + "' is ambiguous as is valid for more then one stream");
            }
    
            if (streamType == null)
            {
                var message = "Property named '" + propertyName + "' is not valid in any stream";
                var msgGen = new PropertyNotFoundExceptionSuggestionGenMultiTyped(_eventTypes, propertyName);
                throw new PropertyNotFoundException(message, msgGen.GetSuggestion);
            }
        }

        private PropertyResolutionDescriptor FindByStreamAndEngineName(String propertyName, String streamName, bool explicitPropertiesOnly, bool obtainFragment)
            
        {
            PropertyResolutionDescriptor desc;
            try
            {
                desc = FindByStreamNameOnly(propertyName, streamName, explicitPropertiesOnly, obtainFragment);
            }
            catch (PropertyNotFoundException)
            {
                var propertyNoEnginePair = GetIsEngineQualified(propertyName, streamName);
                if (propertyNoEnginePair == null)
                {
                    throw;
                }
                return FindByStreamNameOnly(propertyNoEnginePair.First, propertyNoEnginePair.Second, explicitPropertiesOnly, obtainFragment);
            }
            catch (StreamNotFoundException)
            {
                var propertyNoEnginePair = GetIsEngineQualified(propertyName, streamName);
                if (propertyNoEnginePair == null)
                {
                    throw;
                }
                return FindByStreamNameOnly(propertyNoEnginePair.First, propertyNoEnginePair.Second, explicitPropertiesOnly, obtainFragment);
            }
            return desc;
        }
    
        private Pair<String, String> GetIsEngineQualified(String propertyName, String streamName) {
    
            // If still not found, test for the stream name to contain the engine URI
            if (!streamName.Equals(_engineURIQualifier))
            {
                return null;
            }
    
            var index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                return null;
            }
    
            var streamNameNoEngine = propertyName.Substring(0, index);
            var propertyNameNoEngine = propertyName.Substring(index + 1);
            return new Pair<String, String>(propertyNameNoEngine, streamNameNoEngine);
        }
    
        private PropertyResolutionDescriptor FindByStreamNameOnly(String propertyName, String streamName, bool explicitPropertiesOnly, bool obtainFragment)
            
        {
            var index = 0;
            EventType streamType = null;
    
            // Stream name resultion examples:
            // A)  select A1.price from Event.price as A2  => mismatch stream name, cannot resolve
            // B)  select Event1.price from Event2.price   => mismatch event type name, cannot resolve
            // C)  select default.Event2.price from Event2.price   => possible prefix of engine name
            for (var i = 0; i < _eventTypes.Length; i++)
            {
                if (_eventTypes[i] == null)
                {
                    index++;
                    continue;
                }
                if ((_streamNames[i] != null) && (_streamNames[i].Equals(streamName)))
                {
                    streamType = _eventTypes[i];
                    break;
                }
    
                // If the stream name is the event type name, that is also acceptable
                if ((_eventTypes[i].Name != null) && (_eventTypes[i].Name.Equals(streamName)))
                {
                    streamType = _eventTypes[i];
                    break;
                }
    
                index++;
            }
            
            if (streamType == null)
            {
                var message = "Failed to find a stream named '" + streamName + "'";
                var msgGen = new StreamNotFoundExceptionSuggestionGen(_eventTypes, _streamNames, streamName);
                throw new StreamNotFoundException(message, msgGen.GetSuggestion);
            }
    
            Type propertyType = null;
            FragmentEventType fragmentEventType = null;
    
            if (!explicitPropertiesOnly) {
                propertyType = streamType.GetPropertyType(propertyName);
                if (propertyType == null)
                {
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
    
            return new PropertyResolutionDescriptor(streamName, streamType, propertyName, index, propertyType, fragmentEventType);
        }
    
        private PropertyNotFoundException HandlePropertyNotFound(String propertyName, String streamName, EventType streamType)
        {
            var message = "Property named '" + propertyName + "' is not valid in stream '" + streamName + "'";
            var msgGen = new PropertyNotFoundExceptionSuggestionGenSingleTyped(streamType, propertyName);
            return new PropertyNotFoundException(message, msgGen.GetSuggestion);
        }

        public string EngineURIQualifier
        {
            get { return _engineURIQualifier; }
        }

        public bool HasPropertyAgnosticType
        {
            get
            {
                return _eventTypes.OfType<EventTypeSPI>().Any(spi => spi.Metadata.IsPropertyAgnostic);
            }
        }

        public bool IsStreamZeroUnambigous
        {
            get { return _isStreamZeroUnambigous; }
            set { _isStreamZeroUnambigous = value; }
        }

        public bool HasTableTypes
        {
            get { return _hasTableTypes; }
        }

        internal class PropertyNotFoundExceptionSuggestionGenMultiTyped
        {
            private readonly EventType[] _eventTypes;
            private readonly String _propertyName;

            internal PropertyNotFoundExceptionSuggestionGenMultiTyped(EventType[] eventTypes, String propertyName) {
                _eventTypes = eventTypes;
                _propertyName = propertyName;
            }

            public Pair<int, string> GetSuggestion() {
                return StreamTypeServiceUtil.FindLevMatch(_eventTypes, _propertyName);
            }
        }

        internal class PropertyNotFoundExceptionSuggestionGenSingleTyped {
            private readonly EventType _eventType;
            private readonly String _propertyName;

            internal PropertyNotFoundExceptionSuggestionGenSingleTyped(EventType eventType, String propertyName) {
                _eventType = eventType;
                _propertyName = propertyName;
            }

            public Pair<int, string> GetSuggestion() {
                return StreamTypeServiceUtil.FindLevMatch(_propertyName, _eventType);
            }
        }

        internal class StreamNotFoundExceptionSuggestionGen {
            private readonly EventType[] _eventTypes;
            private readonly String[] _streamNames;
            private readonly String _streamName;

            public StreamNotFoundExceptionSuggestionGen(EventType[] eventTypes, String[] streamNames, String streamName) {
                _eventTypes = eventTypes;
                _streamNames = streamNames;
                _streamName = streamName;
            }

            public Pair<int, string> GetSuggestion() {
                // find a near match, textually
                String bestMatch = null;
                var bestMatchDiff = int.MaxValue;

                for (var i = 0; i < _eventTypes.Length; i++)
                {
                    if (_streamNames[i] != null)
                    {
                        var diff = LevenshteinDistance.ComputeLevenshteinDistance(_streamNames[i], _streamName);
                        if (diff < bestMatchDiff)
                        {
                            bestMatchDiff = diff;
                            bestMatch = _streamNames[i];
                        }
                    }

                    if (_eventTypes[i] == null)
                    {
                        continue;
                    }

                    // If the stream name is the event type name, that is also acceptable
                    if (_eventTypes[i].Name != null)
                    {
                        var diff = LevenshteinDistance.ComputeLevenshteinDistance(_eventTypes[i].Name, _streamName);
                        if (diff < bestMatchDiff)
                        {
                            bestMatchDiff = diff;
                            bestMatch = _eventTypes[i].Name;
                        }
                    }
                }

                Pair<int, string> suggestion = null;
                if (bestMatchDiff < int.MaxValue)
                {
                    suggestion = new Pair<int, string>(bestMatchDiff, bestMatch);
                }
                return suggestion;
            }
        }
    }
}
