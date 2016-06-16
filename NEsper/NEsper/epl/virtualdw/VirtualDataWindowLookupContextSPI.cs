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
using com.espertech.esper.client.hook;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.virtualdw
{
    public class VirtualDataWindowLookupContextSPI : VirtualDataWindowLookupContext
    {
        public VirtualDataWindowLookupContextSPI(
            string statementName,
            int statementId,
            Attribute[] statementAnnotations,
            bool fireAndForget,
            String namedWindowName,
            IList<VirtualDataWindowLookupFieldDesc> hashFields,
            IList<VirtualDataWindowLookupFieldDesc> btreeFields,
            SubordPropPlan joinDesc,
            bool forceTableScan,
            EventType[] outerTypePerStream,
            string accessedByStatementName,
            int accessedByStatementSequenceNum)
            : base(statementName,statementId,statementAnnotations,fireAndForget,namedWindowName,hashFields,btreeFields)
        {
            JoinDesc = joinDesc;
            IsForceTableScan = forceTableScan;
            OuterTypePerStream = outerTypePerStream;
            AccessedByStatementName = accessedByStatementName;
            AccessedByStatementSequenceNum = accessedByStatementSequenceNum;
        }

        public SubordPropPlan JoinDesc { get; private set; }

        public bool IsForceTableScan { get; private set; }

        public EventType[] OuterTypePerStream { get; private set; }

        public string AccessedByStatementName { get; private set; }

        public int AccessedByStatementSequenceNum { get; private set; }
    }
}
