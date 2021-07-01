///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.vdw;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class VirtualDataWindowLookupContextSPI : VirtualDataWindowLookupContext
    {
        public VirtualDataWindowLookupContextSPI(
            string deploymentId,
            string statementName,
            int statementId,
            Attribute[] statementAnnotations,
            bool isFireAndForget,
            string namedWindowName,
            IList<VirtualDataWindowLookupFieldDesc> hashFields,
            IList<VirtualDataWindowLookupFieldDesc> btreeFields,
            int accessedByStatementSequenceNum)
            : base(
                deploymentId,
                statementName,
                statementId,
                statementAnnotations,
                isFireAndForget,
                namedWindowName,
                hashFields,
                btreeFields)
        {
            AccessedByStatementSequenceNum = accessedByStatementSequenceNum;
        }

        public int AccessedByStatementSequenceNum { get; }
    }
} // end of namespace