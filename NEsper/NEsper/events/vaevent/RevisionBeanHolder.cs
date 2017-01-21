///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>Holds revisions for property groups in an overlay strategy. </summary>
    public class RevisionBeanHolder
    {
        private readonly long version;
        private readonly EventBean eventBean;
        private readonly EventPropertyGetter[] getters;
    
        /// <summary>Ctor. </summary>
        /// <param name="version">the current version</param>
        /// <param name="eventBean">the new event</param>
        /// <param name="getters">the getters</param>
        public RevisionBeanHolder(long version, EventBean eventBean, EventPropertyGetter[] getters)
        {
            this.version = version;
            this.eventBean = eventBean;
            this.getters = getters;
        }

        /// <summary>Returns current version number. </summary>
        /// <returns>version</returns>
        public long Version
        {
            get { return version; }
        }

        /// <summary>Returns the contributing event. </summary>
        /// <returns>event</returns>
        public EventBean EventBean
        {
            get { return eventBean; }
        }

        /// <summary>Returns getters for event property access. </summary>
        /// <returns>getters</returns>
        public EventPropertyGetter[] Getters
        {
            get { return getters; }
        }

        /// <summary>Returns a property value. </summary>
        /// <param name="propertyNumber">number of property</param>
        /// <returns>value</returns>
        public Object GetValueForProperty(int propertyNumber)
        {
            return getters[propertyNumber].Get(eventBean);
        }
    }
}
