///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.util;

namespace com.espertech.esper.supportregression.client
{
    public class SupportAuditCallback
    {
        private readonly IList<AuditContext> _audits = new List<AuditContext>();

        public void Audit(AuditContext auditContext)
        {
            _audits.Add(auditContext);
        }

        public IList<AuditContext> Audits
        {
            get { return _audits; }
        }
    }
}
