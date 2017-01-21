///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.adapter
{
    /// <summary>
    /// An InputAdapter takes some external data, converts it into events, and sends it into the runtime engine.
    /// </summary>
    public interface InputAdapter : Adapter
    {
    }

    public class InputAdapterConstants
    {
        /// <summary>
        /// Use for MapMessage events to indicate the event type name.
        /// </summary>
        public static readonly string ESPERIO_MAP_EVENT_TYPE = typeof(InputAdapter).FullName + "_maptype";
    }
}
