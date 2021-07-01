///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     Guard instances inspect a matched events and makes a determination on whether to let it pass or not.
    /// </summary>
    public interface Guard
    {
        /// <summary>
        ///     Start the guard operation.
        /// </summary>
        void StartGuard();

        /// <summary>
        ///     Called when sub-expression quits, or when the pattern stopped.
        /// </summary>
        void StopGuard();

        /// <summary>
        ///     Returns true if inspection shows that the match events can pass, or false to not pass.
        /// </summary>
        /// <param name="matchEvent">is the map of matching events</param>
        /// <returns>true to pass, false to not pass</returns>
        bool Inspect(MatchedEventMap matchEvent);

        void Accept(EventGuardVisitor visitor);
    }
} // end of namespace