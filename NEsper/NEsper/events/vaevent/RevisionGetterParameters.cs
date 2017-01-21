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
    /// <summary>
    /// Getter parameters for revision events.
    /// </summary>
    public class RevisionGetterParameters
    {
        private readonly String propertyName;
        private readonly int propertyNumber;
        private readonly EventPropertyGetter baseGetter;
        private readonly int[] propertyGroups;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyName">the property this gets</param>
        /// <param name="propertyNumber">the property number</param>
        /// <param name="fullGetter">the getter of the base event to use, if any</param>
        /// <param name="authoritySets">is the group numbers that the getter may access to obtain a property value</param>
        public RevisionGetterParameters(String propertyName, int propertyNumber, EventPropertyGetter fullGetter, int[] authoritySets)
        {
            this.propertyName = propertyName;
            this.propertyNumber = propertyNumber;
            this.baseGetter = fullGetter;
            this.propertyGroups = authoritySets;
        }

        /// <summary>Returns the group numbers to look for updated properties comparing version numbers. </summary>
        /// <returns>groups</returns>
        public int[] PropertyGroups
        {
            get { return propertyGroups; }
        }

        /// <summary>Returns the property number. </summary>
        /// <returns>property number</returns>
        public int PropertyNumber
        {
            get { return propertyNumber; }
        }

        /// <summary>Returns the getter for the base event type. </summary>
        /// <returns>base getter</returns>
        public EventPropertyGetter BaseGetter
        {
            get { return baseGetter; }
        }
    }
}
