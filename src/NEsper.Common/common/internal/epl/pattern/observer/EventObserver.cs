///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     Observers observe and indicate other external events such as timing events.
    /// </summary>
    public interface EventObserver
    {
        MatchedEventMap BeginState { get; }

        /// <summary>
        ///     Start observing.
        /// </summary>
        void StartObserve();

        /// <summary>
        ///     Stop observing.
        /// </summary>
        void StopObserve();

        void Accept(EventObserverVisitor visitor);
    }
} // end of namespace