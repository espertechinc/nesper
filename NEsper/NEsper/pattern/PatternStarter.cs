///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Interface for observing when an event expression needs to start (by adding the first listener).
    /// The publishing event expression supplies the callback used for indicating matches. The implementation 
    /// supplies as a return value the callback to use to stop the event expression.
    /// </summary>
    public interface PatternStarter
    {
        /// <summary>
        /// An event expression was started and supplies the callback to use when matching events appear. Returns the callback to use to stop the event expression.
        /// </summary>
        /// <param name="matchCallback">must be supplied to indicate what to call when the expression turns true</param>
        /// <param name="context">is the context for handles to services required for evaluation.</param>
        /// <param name="isRecoveringResilient">if set to <c>true</c> [is recovering resilient].</param>
        /// <returns>a callback to stop the expression again</returns>
        PatternStopCallback Start(PatternMatchCallback matchCallback,
                                  PatternContext context,
                                  bool isRecoveringResilient);
    }
}
