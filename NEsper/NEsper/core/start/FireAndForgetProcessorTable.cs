///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.core.start
{
    public class FireAndForgetProcessorTable : FireAndForgetProcessor
    {
        private readonly TableService _tableService;
        private readonly TableMetadata _tableMetadata;
    
        public FireAndForgetProcessorTable(TableService tableService, TableMetadata tableMetadata)
        {
            this._tableService = tableService;
            this._tableMetadata = tableMetadata;
        }

        public TableMetadata TableMetadata
        {
            get { return _tableMetadata; }
        }

        public override EventType EventTypeResultSetProcessor
        {
            get { return _tableMetadata.InternalEventType; }
        }

        public override EventType EventTypePublic
        {
            get { return _tableMetadata.PublicEventType; }
        }

        public override string ContextName
        {
            get { return _tableMetadata.ContextName; }
        }

        public override FireAndForgetInstance GetProcessorInstanceContextById(int agentInstanceId)
        {
            TableStateInstance instance = _tableService.GetState(_tableMetadata.TableName, agentInstanceId);
            if (instance == null) {
                return null;
            }
            return new FireAndForgetInstanceTable(instance);
        }
    
        public override FireAndForgetInstance GetProcessorInstanceNoContext()
        {
            return GetProcessorInstanceContextById(-1);
        }
    
        public override FireAndForgetInstance GetProcessorInstance(AgentInstanceContext agentInstanceContext)
        {
            return GetProcessorInstanceContextById(agentInstanceContext.AgentInstanceId);
        }
    
        public override ICollection<int> GetProcessorInstancesAll()
        {
            return _tableService.GetAgentInstanceIds(_tableMetadata.TableName);
        }

        public override string NamedWindowOrTableName
        {
            get { return _tableMetadata.TableName; }
        }

        public override bool IsVirtualDataWindow
        {
            get { return false; }
        }

        public override string[][] GetUniqueIndexes(FireAndForgetInstance processorInstance)
        {
            return _tableMetadata.UniqueIndexes;
        }
    }
}
