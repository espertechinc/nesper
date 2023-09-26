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
    /// Iteration result for row regex.
    /// </summary>
    public class RowRecogIteratorResult
    {
        private IList<RowRecogNFAStateEntry> endStates;
        private int eventSequenceNum;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "endStates">end states</param>
        /// <param name = "eventSequenceNum">seq num of event</param>
        public RowRecogIteratorResult(
            IList<RowRecogNFAStateEntry> endStates,
            int eventSequenceNum)
        {
            this.endStates = endStates;
            this.eventSequenceNum = eventSequenceNum;
        }

        /// <summary>
        /// Returns the event seq num.
        /// </summary>
        /// <returns>seq num</returns>
        public int EventSequenceNum => eventSequenceNum;

        public IList<RowRecogNFAStateEntry> EndStates => endStates;
    }
} // end of namespace