///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.util
{
    public interface StatementAgentInstanceLock
    {
        void AcquireWriteLock();

        bool AcquireWriteLock(long msecTimeout);

        void ReleaseWriteLock();

        void AcquireReadLock();

        void ReleaseReadLock();
    }

    public class StatementAgentInstanceLockConstants
    {
        /// <summary>
        /// Acquire text.
        /// </summary>
        public const string ACQUIRE_TEXT = "Acquire ";

        /// <summary>
        /// Acquired text.
        /// </summary>
        public const string ACQUIRED_TEXT = "Got     ";

        /// <summary>
        /// Release text.
        /// </summary>
        public const string RELEASE_TEXT = "Release ";

        /// <summary>
        /// Released text.
        /// </summary>
        public const string RELEASED_TEXT = "Freed   ";
    }
} // end of namespace
