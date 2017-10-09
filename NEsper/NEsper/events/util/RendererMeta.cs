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
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events.util
{
    /// <summary>
    /// Renderer cache for event type metadata allows fast rendering of a given type of
    /// events.
    /// </summary>
    public class RendererMeta
    {
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        public RendererMeta(EventType eventType, Stack<EventTypePropertyPair> stack, RendererMetaOptions options)
        {
            var gettersSimple = new List<GetterPair>();
            var gettersIndexed = new List<GetterPair>();
            var gettersMapped = new List<GetterPair>();
            var gettersNested = new List<NestedGetterPair>();

            var descriptors = eventType.PropertyDescriptors;
            foreach (EventPropertyDescriptor desc in descriptors)
            {
                String propertyName = desc.PropertyName;

                if ((!desc.IsIndexed) && (!desc.IsMapped) && (!desc.IsFragment))
                {
                    var getter = eventType.GetGetter(propertyName);
                    if (getter == null)
                    {
                        log.Warn("No getter returned for event type '" + eventType.Name + "' and property '" +
                                 propertyName + "'");
                        continue;
                    }
                    gettersSimple.Add(new GetterPair(getter, propertyName,
                                                     OutputValueRendererFactory.GetOutputValueRenderer(
                                                         desc.PropertyType, options)));
                }

                if (desc.IsIndexed && !desc.RequiresIndex && (!desc.IsFragment))
                {
                    var getter = eventType.GetGetter(propertyName);
                    if (getter == null)
                    {
                        log.Warn("No getter returned for event type '" + eventType.Name + "' and property '" +
                                 propertyName + "'");
                        continue;
                    }
                    gettersIndexed.Add(new GetterPair(getter, propertyName,
                                                      OutputValueRendererFactory.GetOutputValueRenderer(
                                                          desc.PropertyType, options)));
                }

                if (desc.IsMapped && !desc.RequiresMapKey && (!desc.IsFragment))
                {
                    var getter = eventType.GetGetter(propertyName);
                    if (getter == null)
                    {
                        log.Warn("No getter returned for event type '" + eventType.Name + "' and property '" +
                                 propertyName + "'");
                        continue;
                    }
                    gettersMapped.Add(new GetterPair(getter, propertyName,
                                                     OutputValueRendererFactory.GetOutputValueRenderer(
                                                         desc.PropertyType, options)));
                }

                if (desc.IsFragment)
                {
                    var getter = eventType.GetGetter(propertyName);
                    var fragmentType = eventType.GetFragmentType(propertyName);
                    if (getter == null)
                    {
                        log.Warn("No getter returned for event type '" + eventType.Name + "' and property '" +
                                 propertyName + "'");
                        continue;
                    }
                    if (fragmentType == null)
                    {
                        log.Warn("No fragment type returned for event type '" + eventType.Name + "' and property '" +
                                 propertyName + "'");
                        continue;
                    }

                    var pair = new EventTypePropertyPair(fragmentType.FragmentType, propertyName);
                    if ((options.PreventLooping && stack.Contains(pair)))
                    {
                        continue; // prevent looping behavior on self-references
                    }

                    stack.Push(pair);
                    var fragmentMetaData = new RendererMeta(fragmentType.FragmentType, stack, options);
                    stack.Pop();

                    gettersNested.Add(new NestedGetterPair(getter, propertyName, fragmentMetaData,
                                                           fragmentType.IsIndexed));
                }
            }

            gettersSimple.Sort((gp1, gp2) => gp1.Name.CompareTo(gp2.Name));
            gettersIndexed.Sort((gp1, gp2) => gp1.Name.CompareTo(gp2.Name));
            gettersMapped.Sort((gp1, gp2) => gp1.Name.CompareTo(gp2.Name));
            gettersNested.Sort((gp1, gp2) => gp1.Name.CompareTo(gp2.Name));

            simpleProperties = gettersSimple.ToArray();
            indexProperties = gettersIndexed.ToArray();
            mappedProperties = gettersMapped.ToArray();
            nestedProperties = gettersNested.ToArray();
        }

        /// <summary>
        /// Returns simple properties.
        /// </summary>
        /// <returns>
        /// properties
        /// </returns>
        public GetterPair[] SimpleProperties
        {
            get { return simpleProperties; }
        }

        /// <summary>
        /// Returns index properties.
        /// </summary>
        /// <returns>
        /// properties
        /// </returns>
        public GetterPair[] IndexProperties
        {
            get { return indexProperties; }
        }

        /// <summary>
        /// Returns nested properties.
        /// </summary>
        /// <returns>
        /// properties
        /// </returns>
        public NestedGetterPair[] NestedProperties
        {
            get { return nestedProperties; }
        }

        /// <summary>
        /// Returns mapped properties.
        /// </summary>
        /// <returns>
        /// mapped props
        /// </returns>
        public GetterPair[] MappedProperties
        {
            get { return mappedProperties; }
        }
    }
}
