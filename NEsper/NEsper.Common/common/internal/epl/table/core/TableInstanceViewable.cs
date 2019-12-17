///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableInstanceViewable : ViewSupport
    {
        private readonly Table tableMetadata;
        private readonly TableInstance tableStateInstance;

        public TableInstanceViewable(
            Table tableMetadata,
            TableInstance tableStateInstance)
        {
            this.tableMetadata = tableMetadata;
            this.tableStateInstance = tableStateInstance;
        }

        public override EventType EventType => tableMetadata.MetaData.PublicEventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // no action required
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            var stateInstance = tableStateInstance;
            var eventToPublic = stateInstance.Table.EventToPublic;
            return stateInstance.EventCollection
                .Select(e => eventToPublic.Convert(e, null, true, stateInstance.AgentInstanceContext))
                .GetEnumerator();
        }
    }
} // end of namespace