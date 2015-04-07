///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern.guard
{
    /// <summary>Receiver for quit events for use by guards. </summary>
    public interface Quitable
    {
        /// <summary>Indicate guard quitted. </summary>
        void GuardQuit();

        /// <summary>
        /// Retains the pattern context with relevant pattern and statement-level services.
        /// <para/> 
        /// The pattern context is the same context as provided to the guard factory and is 
        /// provided by the quitable so the guard instance does not need to retain the pattern 
        /// context.
        /// </summary>
        /// <value>pattern context</value>
        PatternAgentInstanceContext Context { get; }
    }
}
