///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    /// <summary>
    /// Strategy for use with <seealso cref="StatementResultService" /> to dispatch to
    /// a statement's subscriber via method invocations.
    /// </summary>
    public interface ResultDeliveryStrategy
    {
        /// <summary>Execute the dispatch. </summary>
        /// <param name="result">is the insert and remove stream to indicate</param>
        void Execute(UniformPair<EventBean[]> result);
    }
}