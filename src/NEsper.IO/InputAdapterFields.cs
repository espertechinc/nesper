///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esperio
{
    public abstract class InputAdapterFields
    {
        /// <summary>
        /// Use for MapMessage events to indicate the event type name.
        /// </summary>
        
        public readonly string ESPERIO_MAP_EVENT_TYPE = typeof(InputAdapter).FullName + "_maptype";
    }
}