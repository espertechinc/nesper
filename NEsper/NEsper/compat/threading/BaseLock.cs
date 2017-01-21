///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.compat.threading
{
    /// <summary>
    /// Base class for disposable lock pattern.
    /// </summary>
    abstract public class BaseLock : IDisposable
    {
        /// <summary>Acquire text.</summary>
        internal const string ACQUIRE_TEXT = "Acquire ";

        /// <summary>Acquired text.</summary>
        internal const string ACQUIRED_TEXT = "Got     ";

        /// <summary>Release text.</summary>
        internal const string RELEASE_TEXT = "Release ";

        /// <summary>Released text.</summary>
        internal const string RELEASED_TEXT = "Freed   ";

        public static int RLockTimeout;
        public static int WLockTimeout;
        public static int MLockTimeout;

        static BaseLock()
        {
            RLockTimeout = CompatSettings.Default.ReaderLockTimeout;
            WLockTimeout = CompatSettings.Default.WriterLockTimeout;
            MLockTimeout = CompatSettings.Default.MonitorLockTimeout;
        }



        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();
    }
}
