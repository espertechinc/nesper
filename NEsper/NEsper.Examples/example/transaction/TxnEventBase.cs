///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace NEsper.Examples.Transaction
{
    public class TxnEventBase
    {
        private String _transactionId;
        private readonly long _timestamp;

        public TxnEventBase(String transactionId, long timestamp)
        {
            _transactionId = transactionId;
            _timestamp = timestamp;
        }

        public String TransactionId
        {
            get { return _transactionId; }
            set { _transactionId = value; }
        }

        public long Timestamp
        {
            get { return _timestamp; }
        }

        public override String ToString()
        {
            return "transactionId=" + _transactionId +
                   " timestamp=" + _timestamp;
        }
    }
}
