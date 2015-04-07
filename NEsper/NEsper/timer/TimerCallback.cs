///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.timer
{
    /// <summary>
    /// Invoked by the internal clocking service at regular intervals.
    /// </summary>

    public delegate void TimerCallback();

    public interface ITimerCallback
    {
        void TimerCallback();
    }
}
