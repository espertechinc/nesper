///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client
{
    /// <summary>
    /// Provides an event type for a property of an event.
    /// <para />
    /// A fragment is a property value that is itself an event, or that can be represented
    /// as an event. Thereby a fragment comes with event type metadata and means of querying
    /// the fragment's properties.
    /// <para />
    /// A array or collection of property values that is an array of events or that can be
    /// represented as an array of events has the indexed flag set.
    /// <para />
    /// A map of property values that is an map of events or that can be represented as a map
    /// of events has the mapped flag set.
    /// </summary>
    public class FragmentEventType
    {
        /// <summary>Ctor. </summary>
        /// <param name="fragmentType">the event type for a property value for an event.</param>
        /// <param name="indexed">true to indicate that property value is an array of events</param>
        /// <param name="isNative">true</param>
        public FragmentEventType(EventType fragmentType, bool  indexed, bool  isNative)
        {
            FragmentType = fragmentType;
            IsIndexed = indexed;
            IsNative = isNative;
        }

        /// <summary>
        /// Returns true if the fragment type is an array.
        /// <para/> If a property value is an array and thereby a fragment array, this flag is set to true.
        /// </summary>
        /// <value>indicator if array fragment</value>
        public bool IsIndexed { get; private set; }

        /// <summary>
        /// Returns the type of the fragment.
        /// </summary>
        /// <value>fragment type</value>
        public EventType FragmentType { get; private set; }

        /// <summary>
        /// Returns true if the fragment is a native representation, i.e. a type.
        /// </summary>
        /// <value>indicator whether fragment is a type.</value>
        public bool IsNative { get; private set; }
    }
}
