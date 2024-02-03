///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Interface for a statement-level service for coordinating the insert/remove stream generation,
    /// native deliver to subscribers and the presence/absence of listener or subscribers to a statement.
    /// </summary>
    public interface StatementResultService
    {
        string StatementName { get; }

        IThreadLocal<StatementDispatchTLEntry> DispatchTL { get; }

        void Execute(StatementDispatchTLEntry dispatchTLEntry);

        void Indicate(
            UniformPair<EventBean[]> results,
            StatementDispatchTLEntry dispatchTLEntry);

        bool IsMakeSynthetic { get; }

        bool IsMakeNatural { get; }

        void ClearDeliveriesRemoveStream(EventBean[] removedEvents);
    }
} // end of namespace