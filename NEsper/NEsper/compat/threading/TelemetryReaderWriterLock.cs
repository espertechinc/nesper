///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.threading
{
    public class TelemetryReaderWriterLock : IReaderWriterLock
    {
        /// <summary>
        /// Common identifier for the reader-writer
        /// </summary>
        private readonly String _id;

        /// <summary>
        /// Lock that holds the real lock implementation.
        /// </summary>
        private readonly IReaderWriterLock _subLock;

        /// <summary>
        /// Occurs when the lock is released.
        /// </summary>
        public event EventHandler<TelemetryEventArgs> ReadLockReleased;

        /// <summary>
        /// Occurs when the lock is released.
        /// </summary>
        public event EventHandler<TelemetryEventArgs> WriteLockReleased;

        /// <summary>
        /// Raises the <see cref="E:ReadLockReleased"/> event.
        /// </summary>
        /// <param name="e">The <see cref="com.espertech.esper.compat.threading.TelemetryEventArgs"/> instance containing the event data.</param>
        protected void OnReadLockReleased(TelemetryEventArgs e)
        {
            if (ReadLockReleased != null)
            {
                ReadLockReleased(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:WriteLockReleased"/> event.
        /// </summary>
        /// <param name="e">The <see cref="com.espertech.esper.compat.threading.TelemetryEventArgs"/> instance containing the event data.</param>
        protected void OnWriteLockReleased(TelemetryEventArgs e)
        {
            if (WriteLockReleased != null)
            {
                WriteLockReleased(this, e);
            }
        }

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        /// <value></value>
        public ILockable ReadLock { get; set; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        /// <value></value>
        public ILockable WriteLock { get; set; }

        /// <summary>
        /// Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        /// The is writer lock held.
        /// </value>
        public bool IsWriterLockHeld {
            get { return _subLock.IsWriterLockHeld; }
        }

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TelemetryReaderWriterLock"/> is trace.
        /// </summary>
        /// <value><c>true</c> if trace; otherwise, <c>false</c>.</value>
        public bool Trace { get; set; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryReaderWriterLock"/> class.
        /// </summary>
        /// <param name="subLock">The sub lock.</param>
        public TelemetryReaderWriterLock(IReaderWriterLock subLock)
        {
            _id = Guid.NewGuid().ToString();
            _subLock = subLock;

            do {
                var telemetryLock = new TelemetryLock(_id, _subLock.ReadLock);
                telemetryLock.LockReleased += (sender, e) => OnReadLockReleased(e);
                ReadLock = telemetryLock;
            } while (false);

            do {
                var telemetryLock = new TelemetryLock(_id, _subLock.WriteLock);
                telemetryLock.LockReleased += (sender, e) => OnWriteLockReleased(e);
                WriteLock = telemetryLock;
            } while (false);
        }
    }
}
