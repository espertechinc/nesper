///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Iteration result for row regex.
    /// </summary>
    internal class EventRowRegexIteratorResult
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="endStates">end states</param>
        /// <param name="eventSequenceNum">seq num of event</param>
        internal EventRowRegexIteratorResult(IList<RegexNFAStateEntry> endStates, int eventSequenceNum)
        {
            EndStates = endStates;
            EventSequenceNum = eventSequenceNum;
        }

        /// <summary>
        /// Returns the end states
        /// </summary>
        /// <returns>
        /// end states
        /// </returns>
        internal IList<RegexNFAStateEntry> EndStates { get; private set; }

        /// <summary>
        /// Returns the event seq num.
        /// </summary>
        /// <returns>
        /// seq num
        /// </returns>
        internal int EventSequenceNum { get; private set; }
    }
}
