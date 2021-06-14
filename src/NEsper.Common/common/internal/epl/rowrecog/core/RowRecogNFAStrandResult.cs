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
    ///     A result of computing a strand of one or more NFA states that has a list of start states and a list of all states
    ///     in the strand.
    /// </summary>
    public class RowRecogNFAStrandResult
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="startStates">NFA start states</param>
        /// <param name="allStates">all states</param>
        public RowRecogNFAStrandResult(
            IList<RowRecogNFAStateForge> startStates,
            IList<RowRecogNFAStateForgeBase> allStates)
        {
            StartStates = startStates;
            AllStates = allStates;
        }

        /// <summary>
        ///     Returns start states.
        /// </summary>
        /// <value>start states</value>
        public IList<RowRecogNFAStateForge> StartStates { get; }

        /// <summary>
        ///     Returns all states.
        /// </summary>
        /// <value>all states</value>
        public IList<RowRecogNFAStateForgeBase> AllStates { get; }
    }
} // end of namespace