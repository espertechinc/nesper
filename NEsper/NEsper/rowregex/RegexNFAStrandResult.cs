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
    /// A result of computing a strand of one or more NFA states that has a list of
    /// start states and a list of all states in the strand.
    /// </summary>
    public class RegexNFAStrandResult
    {
        private IList<RegexNFAState> startStates;
        private IList<RegexNFAStateBase> allStates;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="startStates">NFA start states</param>
        /// <param name="allStates">all states</param>
        public RegexNFAStrandResult(IList<RegexNFAState> startStates, IList<RegexNFAStateBase> allStates)
        {
            this.startStates = startStates;
            this.allStates = allStates;
        }

        /// <summary>
        /// Returns start states.
        /// </summary>
        /// <returns>
        /// start states
        /// </returns>
        public IList<RegexNFAState> StartStates
        {
            get { return startStates; }
        }

        /// <summary>
        /// Returns all states.
        /// </summary>
        /// <returns>
        /// all states
        /// </returns>
        public IList<RegexNFAStateBase> AllStates
        {
            get { return allStates; }
        }
    }
}
