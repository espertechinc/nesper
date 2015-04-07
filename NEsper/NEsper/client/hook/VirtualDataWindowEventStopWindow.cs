///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.hook
{
    /// <summary>This event is raised when a virtual data window is stopped. </summary>
    public class VirtualDataWindowEventStopWindow : VirtualDataWindowEvent {
    
        private readonly String namedWindowName;
    
        /// <summary>Ctor. </summary>
        /// <param name="namedWindowName">named window name</param>
        public VirtualDataWindowEventStopWindow(String namedWindowName) {
            this.namedWindowName = namedWindowName;
        }

        /// <summary>Returns the named window name. </summary>
        /// <value>named window name</value>
        public string NamedWindowName
        {
            get { return namedWindowName; }
        }
    }
}
