///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.core.start
{
    public class EPStatementDestroyCallbackTableIdxRef
    {
        private readonly TableService _tableService;
        private readonly TableMetadata _tableMetadata;
        private readonly string _statementName;

        public static Action New(TableService tableService, TableMetadata tableMetadata, string statementName)
        {
            return new EPStatementDestroyCallbackTableIdxRef(
                tableService,
                tableMetadata,
                statementName).Destroy;
        }

        public EPStatementDestroyCallbackTableIdxRef(TableService tableService, TableMetadata tableMetadata, string statementName) {
            _tableService = tableService;
            _tableMetadata = tableMetadata;
            _statementName = statementName;
        }
    
        public void Destroy() {
            _tableService.RemoveIndexReferencesStmtMayRemoveIndex(_statementName, _tableMetadata);
        }
    }
}
