///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.mgr;
using com.espertech.esper.util;

namespace com.espertech.esper.core.start
{
    public class EPStatementDestroyCallbackContext : DestroyCallback
    {
        private readonly ContextManagementService _contextManagementService;
        private readonly string _contextName;
        private readonly string _statementName;
        private readonly int _statementId;

        public EPStatementDestroyCallbackContext(
            ContextManagementService contextManagementService,
            string optionalContextName,
            string statementName,
            int statementId)
        {
            _contextManagementService = contextManagementService;
            _contextName = optionalContextName;
            _statementName = statementName;
            _statementId = statementId;
        }
    
        public void Destroy() {
            _contextManagementService.DestroyedStatement(_contextName, _statementName, _statementId);
        }
    }
}
