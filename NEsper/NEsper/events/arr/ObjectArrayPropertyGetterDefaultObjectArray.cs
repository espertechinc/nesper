///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.arr
{
    /// <summary>Getter for map entry. </summary>
    public class ObjectArrayPropertyGetterDefaultObjectArray : ObjectArrayPropertyGetterDefaultBase
    {
        public ObjectArrayPropertyGetterDefaultObjectArray(int propertyIndex, EventType fragmentEventType, EventAdapterService eventAdapterService)
            : base(propertyIndex, fragmentEventType, eventAdapterService)
        {
        }

        protected override Object HandleCreateFragment(Object value)
        {
            if (FragmentEventType == null)
            {
                return null;
            }
            return BaseNestableEventUtil.HandleCreateFragmentObjectArray(value, FragmentEventType, EventAdapterService);
        }
    }
}
