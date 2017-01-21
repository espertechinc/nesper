///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.activator
{
    public class ViewableActivatorTable : ViewableActivator
    {
        private readonly TableMetadata _tableMetadata;
        private readonly ExprEvaluator[] _optionalTableFilters;
    
        public ViewableActivatorTable(TableMetadata tableMetadata, ExprEvaluator[] optionalTableFilters)
        {
            _tableMetadata = tableMetadata;
            _optionalTableFilters = optionalTableFilters;
        }
    
        public ViewableActivationResult Activate(AgentInstanceContext agentInstanceContext, bool isSubselect, bool isRecoveringResilient)
        {
            TableStateInstance state = agentInstanceContext.StatementContext.TableService.GetState(_tableMetadata.TableName, agentInstanceContext.AgentInstanceId);
            return new ViewableActivationResult(new TableStateViewableInternal(_tableMetadata, state, _optionalTableFilters), CollectionUtil.STOP_CALLBACK_NONE, null, null, null, false, false, null);
        }
    }
}
