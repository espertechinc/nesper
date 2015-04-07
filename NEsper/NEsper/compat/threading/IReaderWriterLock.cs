///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat.threading
{
    public interface IReaderWriterLock
    {
        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        ILockable ReadLock { get; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        ILockable WriteLock { get; }

        /// <summary>
        /// Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        /// The is writer lock held.
        /// </value>
        bool IsWriterLockHeld { get; }

#if DEBUG
        bool Trace { get; set; }
#endif
    }
}
