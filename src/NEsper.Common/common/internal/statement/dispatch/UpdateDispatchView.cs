///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.statement.dispatch
{
    /// <summary>
    /// Update dispatch view to indicate statement results to listeners.
    /// </summary>
    public interface UpdateDispatchView : View
    {
        /// <summary>
        /// Convenience method that accepts a pair of new and old data
        /// as this is the most treated unit.
        /// </summary>
        /// <param name="result">is new data (insert stream) and old data (remove stream)</param>
        void NewResult(UniformPair<EventBean[]> result);
    }
} // end of namespace