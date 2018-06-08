///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Descriptor for explicit properties for use with <seealso cref="BaseConfigurableEventType"/>.
    /// </summary>
    public class ExplicitPropertyDescriptor
    {
        /// <summary>Ctor. </summary>
        /// <param name="descriptor">property descriptor</param>
        /// <param name="getter">getter for values</param>
        /// <param name="fragmentArray">true if array fragment</param>
        /// <param name="optionalFragmentTypeName">null if not a fragment, else fragment type name</param>
        public ExplicitPropertyDescriptor(
            EventPropertyDescriptor descriptor, 
            EventPropertyGetterSPI getter, 
            bool fragmentArray, 
            String optionalFragmentTypeName)
        {
            Descriptor = descriptor;
            Getter = getter;
            IsFragmentArray = fragmentArray;
            OptionalFragmentTypeName = optionalFragmentTypeName;
        }

        /// <summary>Returns the property descriptor. </summary>
        /// <value>property descriptor</value>
        public EventPropertyDescriptor Descriptor { get; private set; }

        /// <summary>Returns the getter. </summary>
        /// <value>getter</value>
        public EventPropertyGetterSPI Getter { get; private set; }

        /// <summary>Returns the fragment event type name, or null if none defined. </summary>
        /// <value>fragment type name</value>
        public string OptionalFragmentTypeName { get; private set; }

        /// <summary>
        /// Returns true if an indexed, or false if not indexed.
        /// </summary>
        /// <value>fragment indicator</value>
        public bool IsFragmentArray { get; private set; }

        public override String ToString()
        {
            return Descriptor.PropertyName;
        }
    }
}
