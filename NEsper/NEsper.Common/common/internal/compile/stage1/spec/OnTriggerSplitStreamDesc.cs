///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification for the on-select splitstream statement.
    /// </summary>
    public class OnTriggerSplitStreamDesc : OnTriggerDesc
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="onTriggerType">type of trigger</param>
        /// <param name="isFirst">true for use the first-matching where clause, false for all</param>
        /// <param name="splitStreams">streams</param>
        public OnTriggerSplitStreamDesc(
            OnTriggerType onTriggerType,
            bool isFirst,
            IList<OnTriggerSplitStream> splitStreams)
            : base(onTriggerType)
        {
            IsFirst = isFirst;
            SplitStreams = splitStreams;
        }

        /// <summary>
        /// Returns the remaining insert-into and select-clauses in the split-stream clause.
        /// </summary>
        /// <returns>
        /// clauses.
        /// </returns>
        public IList<OnTriggerSplitStream> SplitStreams { get; private set; }

        /// <summary>
        /// Returns indicator whether only the first or all where-clauses are triggering.
        /// </summary>
        /// <returns>
        /// first or all
        /// </returns>
        public bool IsFirst { get; private set; }
    }
}