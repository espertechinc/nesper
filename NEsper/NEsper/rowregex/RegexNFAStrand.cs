///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// A strand of one or more NFA states that has a list of start states, end states
    /// and a list of all states in the strand.
    /// </summary>
    public class RegexNFAStrand
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="startStates">start states</param>
        /// <param name="endStates">end states</param>
        /// <param name="allStates">all states</param>
        /// <param name="passthrough">true if this strand passes through (zero-or-more multiplicity for all NFA in strand)</param>
        public RegexNFAStrand(IList<RegexNFAStateBase> startStates, IList<RegexNFAStateBase> endStates,
                              IList<RegexNFAStateBase> allStates, bool passthrough)
        {
            StartStates = startStates;
            EndStates = endStates;
            AllStates = allStates;
            IsPassthrough = passthrough;
        }

        /// <summary>
        /// Returns the start states.
        /// </summary>
        /// <returns>
        /// start states
        /// </returns>
        public IList<RegexNFAStateBase> StartStates { get; private set; }

        /// <summary>
        /// Returns the end states.
        /// </summary>
        /// <returns>
        /// end states
        /// </returns>
        public IList<RegexNFAStateBase> EndStates { get; private set; }

        /// <summary>
        /// Returns all states.
        /// </summary>
        /// <returns>
        /// all states
        /// </returns>
        public IList<RegexNFAStateBase> AllStates { get; private set; }

        /// <summary>
        /// Returns indicator if passing-through (zero-or-more multiplicity for all NFA
        /// states in strand).
        /// </summary>
        /// <returns>
        /// pass-through
        /// </returns>
        public bool IsPassthrough { get; private set; }
    }
}
