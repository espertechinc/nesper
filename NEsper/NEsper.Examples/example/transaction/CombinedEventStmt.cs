///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace NEsper.Examples.Transaction
{
    public class CombinedEventStmt
    {
        public static EPStatement Create(EPAdministrator admin)
        {
            // We need to take in events A, B and C and produce a single, combined event
            String stmt = "insert into CombinedEvent(transactionId, customerId, supplierId, latencyAC, latencyBC, latencyAB)" +
                            "select C.TransactionId," +
                                 "CustomerId," +
                                 "SupplierId," +
                                 "C.Timestamp - A.Timestamp," +
                                 "C.Timestamp - B.Timestamp," +
                                 "B.Timestamp - A.Timestamp " +
                            "from TxnEventA.win:time(30 min) A," +
                                 "TxnEventB.win:time(30 min) B," +
                                 "TxnEventC.win:time(30 min) C " +
                            "where A.TransactionId = B.TransactionId and B.TransactionId = C.TransactionId";

            return admin.CreateEPL(stmt);
        }
    }
}
