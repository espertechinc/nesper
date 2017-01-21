///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace NEsper.Examples.VirtualDW
{
    public class SampleUpdateListener
    {
        public void Update(object sender, UpdateEventArgs updateEventArgs)
        {
            LastEvent = updateEventArgs.NewEvents[0];
        }

        public EventBean LastEvent { get; private set; }
    }
}
