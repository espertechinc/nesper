///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>Per-event-type descriptor for fast access to getters for key values and changes properties. </summary>
    public class RevisionTypeDesc
    {
        private readonly EventPropertyGetter[] keyPropertyGetters;
        private readonly EventPropertyGetter[] changesetPropertyGetters;
        private readonly PropertyGroupDesc group;
        private readonly int[] changesetPropertyIndex;
    
        /// <summary>Ctor. </summary>
        /// <param name="keyPropertyGetters">key getters</param>
        /// <param name="changesetPropertyGetters">property getters</param>
        /// <param name="group">group this belongs to</param>
        public RevisionTypeDesc(EventPropertyGetter[] keyPropertyGetters, EventPropertyGetter[] changesetPropertyGetters, PropertyGroupDesc group)
        {
            this.keyPropertyGetters = keyPropertyGetters;
            this.changesetPropertyGetters = changesetPropertyGetters;
            this.group = group;
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="keyPropertyGetters">key getters</param>
        /// <param name="changesetPropertyGetters">property getters</param>
        /// <param name="changesetPropertyIndex">indexes of properties contributed</param>
        public RevisionTypeDesc(EventPropertyGetter[] keyPropertyGetters, EventPropertyGetter[] changesetPropertyGetters, int[] changesetPropertyIndex)
        {
            this.keyPropertyGetters = keyPropertyGetters;
            this.changesetPropertyGetters = changesetPropertyGetters;
            this.changesetPropertyIndex = changesetPropertyIndex;
        }

        /// <summary>Returns key getters. </summary>
        /// <returns>getters</returns>
        public EventPropertyGetter[] KeyPropertyGetters
        {
            get { return keyPropertyGetters; }
        }

        /// <summary>Returns property getters. </summary>
        /// <returns>getters</returns>
        public EventPropertyGetter[] ChangesetPropertyGetters
        {
            get { return changesetPropertyGetters; }
        }

        /// <summary>Returns group, or null if not using property groups. </summary>
        /// <returns>group</returns>
        public PropertyGroupDesc Group
        {
            get { return group; }
        }

        /// <summary>Returns indexes of properties contributed, or null if not using indexes. </summary>
        /// <returns>indexes</returns>
        public int[] ChangesetPropertyIndex
        {
            get { return changesetPropertyIndex; }
        }
    }
}
