///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     Receiver for quit events for use by guards.
    /// </summary>
    public interface Quitable
    {
        /// <summary>
        ///     Retains the pattern context with relevant pattern and statement-level services.
        ///     <para />
        ///     The pattern context is the same context as provided to the guard factory and
        ///     is provided by the quitable so the guard instance does not need to retain the pattern context.
        /// </summary>
        /// <returns>pattern context</returns>
        PatternAgentInstanceContext Context { get; }

        /// <summary>
        ///     Indicate guard quitted.
        /// </summary>
        void GuardQuit();
    }
} // end of namespace