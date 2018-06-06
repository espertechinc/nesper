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
    public class FindMissingEventStmt
    {
        public const int TIME_WINDOW_TXNC_IN_SEC = 60 * 60;

        //
        // We need to detect a transaction that did not make it through all three events.
        // In other words, a transaction with events A or B, but not C.
        // Note that, in this case, what we care about is event C.
        // The lack of events A or B could indicate a failure in the event transport and should be ignored.
        // Although the lack of an event C could also be a transport failure, it merits looking into.
        //
        public static EPStatement Create(EPAdministrator admin)
        {
            // The inner table to both A and B is C.
            //
            // The listener will consider old events generated when either A or B leave the window, with
            // a window size for A and B of 30 minutes.
            //
            // The window of C is declared large to ensure the C events don't leave the window before A and B
            // thus generating false alerts, making these obvious via timestamp. Lets keep 1 hour of data for C.
            String stmt = "select * from " +
                            "TxnEventA.win:time(30 min) A " +
                              "full outer join " +
                            "TxnEventC.win:time(1 hour) C on A.TransactionId = C.TransactionId " +
                              "full outer join " +
                            "TxnEventB.win:time(30 min) B on B.TransactionId = C.TransactionId " +
                          "where C.TransactionId is null";

            return admin.CreateEPL(stmt);
        }
    }
}
