///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.core
{
	/// <summary> Encapsulates the result of resolving a property and optional stream name against a supplied list of streams
	/// <see cref="com.espertech.esper.epl.core.StreamTypeService"/>.
	/// </summary>
	public class PropertyResolutionDescriptor
	{
	    /// <summary> Returns stream name.</summary>
	    /// <returns> stream name
	    /// </returns>
	    public String StreamName { get; private set; }

	    /// <summary> Returns event type of the stream that the property was found in.</summary>
	    /// <returns> stream's event type
	    /// </returns>
	    public EventType StreamEventType { get; private set; }

	    /// <summary> Returns resolved property name of the property as it exists in a stream.</summary>
	    /// <returns> property name as resolved in a stream
	    /// </returns>
	    public String PropertyName { get; private set; }

	    /// <summary> Returns the number of the stream the property was found in.</summary>
	    /// <returns> stream offset number Starting at zero to N-1 where N is the number of streams
	    /// </returns>
	    public int StreamNum { get; private set; }

	    /// <summary> Returns the property type of the resolved property.</summary>
	    /// <returns> class of property
	    /// </returns>
	    public Type PropertyType { get; private set; }

        /// <summary>
        /// Gets or sets the type of the fragment event.
        /// </summary>
        /// <value>The type of the fragment event.</value>
        public FragmentEventType FragmentEventType { get; private set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamName">is the stream name</param>
        /// <param name="streamEventType">is the event type of the stream where the property was found</param>
        /// <param name="propertyName">is the regular name of property</param>
        /// <param name="streamNum">is the number offset of the stream</param>
        /// <param name="propertyType">is the type of the property</param>
        /// <param name="fragmentEventType">Type of the fragment event.</param>
        public PropertyResolutionDescriptor(String streamName, EventType streamEventType, String propertyName, int streamNum, Type propertyType, FragmentEventType fragmentEventType)
		{
            StreamNum = streamNum;
            StreamName = streamName;
			StreamEventType = streamEventType;
			PropertyName = propertyName;
			PropertyType = propertyType;
            FragmentEventType = fragmentEventType;
		}
	}
}
