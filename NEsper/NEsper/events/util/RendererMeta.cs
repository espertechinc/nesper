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
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events.util
{
    /// <summary>
    /// Renderer cache for event type metadata allows fast rendering of a given type of events.
    /// </summary>
    public class RendererMeta {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly GetterPair[] simpleProperties;
        private readonly GetterPair[] indexProperties;
        private readonly GetterPair[] mappedProperties;
        private readonly NestedGetterPair[] nestedProperties;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">to render</param>
        /// <param name="stack">the stack of properties to avoid looping</param>
        /// <param name="options">rendering options</param>
        public RendererMeta(EventType eventType, Stack<EventTypePropertyPair> stack, RendererMetaOptions options) {
            var gettersSimple = new List<GetterPair>();
            var gettersIndexed = new List<GetterPair>();
            var gettersMapped = new List<GetterPair>();
            var gettersNested = new List<NestedGetterPair>();
    
            EventPropertyDescriptor[] descriptors = eventType.PropertyDescriptors;
            foreach (EventPropertyDescriptor desc in descriptors) {
                string propertyName = desc.PropertyName;
    
                if ((!desc.IsIndexed) && (!desc.IsMapped) && (!desc.IsFragment)) {
                    EventPropertyGetter getter = eventType.GetGetter(propertyName);
                    if (getter == null) {
                        Log.Warn("No getter returned for event type '" + eventType.Name + "' and property '" + propertyName + "'");
                        continue;
                    }
                    gettersSimple.Add(new GetterPair(getter, propertyName, OutputValueRendererFactory.GetOutputValueRenderer(desc.PropertyType, options)));
                }
    
                if (desc.IsIndexed && !desc.IsRequiresIndex && (!desc.IsFragment)) {
                    EventPropertyGetter getter = eventType.GetGetter(propertyName);
                    if (getter == null) {
                        Log.Warn("No getter returned for event type '" + eventType.Name + "' and property '" + propertyName + "'");
                        continue;
                    }
                    gettersIndexed.Add(new GetterPair(getter, propertyName, OutputValueRendererFactory.GetOutputValueRenderer(desc.PropertyType, options)));
                }
    
                if (desc.IsMapped && !desc.IsRequiresMapkey && (!desc.IsFragment)) {
                    EventPropertyGetter getter = eventType.GetGetter(propertyName);
                    if (getter == null) {
                        Log.Warn("No getter returned for event type '" + eventType.Name + "' and property '" + propertyName + "'");
                        continue;
                    }
                    gettersMapped.Add(new GetterPair(getter, propertyName, OutputValueRendererFactory.GetOutputValueRenderer(desc.PropertyType, options)));
                }
    
                if (desc.IsFragment) {
                    EventPropertyGetter getter = eventType.GetGetter(propertyName);
                    FragmentEventType fragmentType = eventType.GetFragmentType(propertyName);
                    if (getter == null) {
                        Log.Warn("No getter returned for event type '" + eventType.Name + "' and property '" + propertyName + "'");
                        continue;
                    }
                    if (fragmentType == null) {
                        Log.Warn("No fragment type returned for event type '" + eventType.Name + "' and property '" + propertyName + "'");
                        continue;
                    }
    
                    var pair = new EventTypePropertyPair(fragmentType.FragmentType, propertyName);
                    if (options.IsPreventLooping && stack.Contains(pair)) {
                        continue;   // prevent looping behavior on self-references
                    }
    
                    stack.Push(pair);
                    var fragmentMetaData = new RendererMeta(fragmentType.FragmentType, stack, options);
                    stack.Pop();
    
                    gettersNested.Add(new NestedGetterPair(getter, propertyName, fragmentMetaData, fragmentType.IsIndexed));
                }
            }
    
            simpleProperties = gettersSimple.ToArray(new GetterPair[gettersSimple.Count]);
            indexProperties = gettersIndexed.ToArray(new GetterPair[gettersIndexed.Count]);
            mappedProperties = gettersMapped.ToArray(new GetterPair[gettersMapped.Count]);
            nestedProperties = gettersNested.ToArray(new NestedGetterPair[gettersNested.Count]);
        }
    
        /// <summary>
        /// Returns simple properties.
        /// </summary>
        /// <returns>properties</returns>
        public GetterPair[] GetSimpleProperties() {
            return simpleProperties;
        }
    
        /// <summary>
        /// Returns index properties.
        /// </summary>
        /// <returns>properties</returns>
        public GetterPair[] GetIndexProperties() {
            return indexProperties;
        }
    
        /// <summary>
        /// Returns nested properties.
        /// </summary>
        /// <returns>properties</returns>
        public NestedGetterPair[] GetNestedProperties() {
            return nestedProperties;
        }
    
        /// <summary>
        /// Returns mapped properties.
        /// </summary>
        /// <returns>mapped props</returns>
        public GetterPair[] GetMappedProperties() {
            return mappedProperties;
        }
    }
} // end of namespace
