///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// A getter that works on events residing within a Map as an event property.
    /// </summary>
    public class RevisionNestedPropertyGetter : EventPropertyGetter
    {
        private readonly EventPropertyGetter revisionGetter;
        private readonly EventPropertyGetter nestedGetter;
        private readonly EventAdapterService eventAdapterService;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="revisionGetter">getter for revision value</param>
        /// <param name="nestedGetter">getter to apply to revision value</param>
        /// <param name="eventAdapterService">for handling object types</param>
        public RevisionNestedPropertyGetter(EventPropertyGetter revisionGetter, EventPropertyGetter nestedGetter, EventAdapterService eventAdapterService) {
            this.revisionGetter = revisionGetter;
            this.eventAdapterService = eventAdapterService;
            this.nestedGetter = nestedGetter;
        }
    
        public Object Get(EventBean eventBean)
        {
            Object result = revisionGetter.Get(eventBean);
            if (result == null)
            {
                return result;
            }
    
            // Object within the map
            return nestedGetter.Get(eventAdapterService.AdapterForObject(result));
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null; // no fragments provided by revision events
        }
    }
}
