///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.util;

namespace com.espertech.esper.core.start
{
    public class EPStatementDestroyCallbackContext
    {
        private readonly ContextManagementService _contextManagementService;
        private readonly string _contextName;
        private readonly string _statementName;
        private readonly string _statementId;

        public static Action New(
            ContextManagementService contextManagementService,
            string optionalContextName,
            string statementName,
            string statementId)
        {
            return new EPStatementDestroyCallbackContext(
                contextManagementService,
                optionalContextName,
                statementName,
                statementId).Destroy;
        }
    
        public EPStatementDestroyCallbackContext(ContextManagementService contextManagementService, string optionalContextName, string statementName, string statementId)
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
