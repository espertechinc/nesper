///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.service.resource;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Statement-level extension services.
    /// </summary>
    public interface StatementExtensionSvcContext
    {
        StatementResourceService StmtResources { get; }
    }

    public class ProxyStatementExtensionSvcContext : StatementExtensionSvcContext
    {
        public Func<StatementResourceService> ProcStmtResources { get; set; }

        public StatementResourceService StmtResources
        {
            get { return ProcStmtResources.Invoke(); }
        }
    }
}
