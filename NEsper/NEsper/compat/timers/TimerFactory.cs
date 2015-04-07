///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.compat.timers;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Creates timers.
    /// </summary>

    public class TimerFactory
    {
        /// <summary>
        /// Gets the default timer factory
        /// </summary>

        public static ITimerFactory DefaultTimerFactory
        {
            get
            {
                lock( factoryLock )
                {
                    if (defaultTimerFactory == null)
                    {
                        // use the system timer factory unless explicitly instructed
                        // to do otherwise.

                        defaultTimerFactory = new SystemTimerFactory();
                    }
                }

                return defaultTimerFactory;
            }
            set
            {
                lock( factoryLock )
                {
                    defaultTimerFactory = value;
                }
            }
        }

        private static ITimerFactory defaultTimerFactory;
        private static readonly Object factoryLock = new Object();
    }
}
