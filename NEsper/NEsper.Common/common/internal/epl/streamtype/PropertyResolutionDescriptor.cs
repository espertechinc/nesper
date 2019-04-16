///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.streamtype
{
    /// <summary>
    ///     Encapsulates the result of resolving a property and optional stream name against a supplied list of streams
    ///     <seealso cref="StreamTypeService" />.
    /// </summary>
    public class PropertyResolutionDescriptor
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamName">is the stream name</param>
        /// <param name="streamEventType">is the event type of the stream where the property was found</param>
        /// <param name="propertyName">is the regular name of property</param>
        /// <param name="streamNum">is the number offset of the stream</param>
        /// <param name="propertyType">is the type of the property</param>
        /// <param name="fragmentEventType">fragment type</param>
        public PropertyResolutionDescriptor(
            string streamName,
            EventType streamEventType,
            string propertyName,
            int streamNum,
            Type propertyType,
            FragmentEventType fragmentEventType)
        {
            StreamName = streamName;
            StreamEventType = streamEventType;
            PropertyName = propertyName;
            StreamNum = streamNum;
            PropertyType = propertyType;
            FragmentEventType = fragmentEventType;
        }

        /// <summary>
        ///     Returns stream name.
        /// </summary>
        /// <returns>stream name</returns>
        public string StreamName { get; }

        /// <summary>
        ///     Returns event type of the stream that the property was found in.
        /// </summary>
        /// <returns>stream's event type</returns>
        public EventType StreamEventType { get; }

        /// <summary>
        ///     Returns resolved property name of the property as it exists in a stream.
        /// </summary>
        /// <returns>property name as resolved in a stream</returns>
        public string PropertyName { get; }

        /// <summary>
        ///     Returns the number of the stream the property was found in.
        /// </summary>
        /// <returns>stream offset number starting at zero to N-1 where N is the number of streams</returns>
        public int StreamNum { get; }

        /// <summary>
        ///     Returns the property type of the resolved property.
        /// </summary>
        /// <returns>class of property</returns>
        public Type PropertyType { get; }

        public FragmentEventType FragmentEventType { get; }
    }
} // end of namespace