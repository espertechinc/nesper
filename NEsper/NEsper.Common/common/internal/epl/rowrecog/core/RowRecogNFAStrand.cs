///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.rowrecog.nfa;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    ///     A strand of one or more NFA states that has a list of start states, end states and a list of all states in the
    ///     strand.
    /// </summary>
    public class RowRecogNFAStrand
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="startStates">start states</param>
        /// <param name="endStates">end states</param>
        /// <param name="allStates">all states</param>
        /// <param name="passthrough">true if this strand passes through (zero-or-more multiplicity for all NFA in strand)</param>
        public RowRecogNFAStrand(
            IList<RowRecogNFAStateForgeBase> startStates,
            IList<RowRecogNFAStateForgeBase> endStates,
            IList<RowRecogNFAStateForgeBase> allStates,
            bool passthrough)
        {
            StartStates = startStates;
            EndStates = endStates;
            AllStates = allStates;
            IsPassthrough = passthrough;
        }

        /// <summary>
        ///     Returns the start states.
        /// </summary>
        /// <value>start states</value>
        public IList<RowRecogNFAStateForgeBase> StartStates { get; }

        /// <summary>
        ///     Returns the end states.
        /// </summary>
        /// <value>end states</value>
        public IList<RowRecogNFAStateForgeBase> EndStates { get; }

        /// <summary>
        ///     Returns all states.
        /// </summary>
        /// <value>all states</value>
        public IList<RowRecogNFAStateForgeBase> AllStates { get; }

        /// <summary>
        ///     Returns indicator if passing-through (zero-or-more multiplicity for all NFA states in strand).
        /// </summary>
        /// <value>pass-through</value>
        public bool IsPassthrough { get; }
    }
} // end of namespace