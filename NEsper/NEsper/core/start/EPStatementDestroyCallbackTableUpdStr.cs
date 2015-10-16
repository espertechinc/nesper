///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.core.start
{
    public class EPStatementDestroyCallbackTableUpdStr
    {
        private readonly TableService _tableService;
        private readonly TableMetadata _tableMetadata;
        private readonly string _statementName;
    
        public EPStatementDestroyCallbackTableUpdStr(TableService tableService, TableMetadata tableMetadata, string statementName) {
            _tableService = tableService;
            _tableMetadata = tableMetadata;
            _statementName = statementName;
        }
    
        public void Destroy() {
            _tableService.RemoveTableUpdateStrategyReceivers(_tableMetadata, _statementName);
        }
    }
}
