///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.events.map
{
    /// <summary>Getter for map entry. </summary>
    public class MapPropertyGetterDefaultNoFragment : MapPropertyGetterDefaultBase
    {
        public MapPropertyGetterDefaultNoFragment(String propertyName, EventAdapterService eventAdapterService)
            : base(propertyName, null, eventAdapterService)
        {
        }

        protected override Object HandleCreateFragment(Object value)
        {
            return null;
        }
    }
}
