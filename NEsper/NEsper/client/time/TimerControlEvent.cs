///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.time
{
    /// <summary>
    /// Event for controlling clocking, i.e. to enable and disable external clocking.
    /// </summary>

    [Serializable]
    public sealed class TimerControlEvent : TimerEvent
    {
        /// <summary>
        /// Enumeration that describes what type of clock we are using.
        /// </summary>

        public enum ClockTypeEnum
        {
            /// <summary> For external clocking.</summary>
            CLOCK_EXTERNAL,
            /// <summary> For internal clocking.</summary>
            CLOCK_INTERNAL
        };

        /// <summary> Constructor takes a clocking type as parameter.</summary>
        /// <param name="clockType">for internal or external clocking
        /// </param>
        public TimerControlEvent(ClockTypeEnum clockType)
        {
            ClockType = clockType;
        }

        /// <summary> Returns clocking type.</summary>
        /// <returns> clocking type
        /// </returns>
        public ClockTypeEnum ClockType { get; private set; }
    }
}
