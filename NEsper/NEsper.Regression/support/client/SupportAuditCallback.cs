///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.metrics.audit;

namespace com.espertech.esper.regressionlib.support.client
{
    public class SupportAuditCallback
    {
        public IList<AuditContext> Audits { get; } = new List<AuditContext>();

        public void Audit(AuditContext auditContext)
        {
            Audits.Add(auditContext);
        }
    }
} // end of namespace