///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public interface TableInstance
    {
        ICollection<EventBean> EventCollection { get; }

        Table Table { get; }

        AgentInstanceContext AgentInstanceContext { get; }

        EventTableIndexRepository IndexRepository { get; }

        IEnumerable<EventBean> IterableTableScan { get; }

        IReaderWriterLock TableLevelRWLock { get; }

        void AddEventUnadorned(EventBean @event);

        void AddEvent(EventBean @event);

        void ClearInstance();

        void Destroy();

        void HandleRowUpdated(ObjectArrayBackedEventBean updatedEvent);

        void DeleteEvent(EventBean matchingEvent);

        void AddExplicitIndex(
            string indexName,
            string indexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            bool isRecoveringResilient);

        void RemoveExplicitIndex(string indexName);

        EventTable GetIndex(string indexName);

        void HandleRowUpdateKeyBeforeUpdate(ObjectArrayBackedEventBean updatedEvent);

        void HandleRowUpdateKeyAfterUpdate(ObjectArrayBackedEventBean updatedEvent);
    }
} // end of namespace