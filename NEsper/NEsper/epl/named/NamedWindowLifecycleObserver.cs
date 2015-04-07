///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.named
{
    /// <summary>Observer named window events. </summary>
    public interface NamedWindowLifecycleObserver
    {
        /// <summary>Observer named window changes. </summary>
        /// <param name="theEvent">indicates named window action</param>
        void Observe(NamedWindowLifecycleEvent theEvent);
    }
}