///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// A Statement-lock implementation that doesn't lock.
    /// </summary>
    public class StatementAgentInstanceLockNoLockImpl :
        VoidReaderWriterLock,
        StatementAgentInstanceLock
    {
        private readonly string name;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">of lock</param>
        public StatementAgentInstanceLockNoLockImpl(string name)
        {
            this.name = name;
        }
    }
} // end of namespace