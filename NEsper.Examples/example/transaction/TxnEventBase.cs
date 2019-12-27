///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace NEsper.Examples.Transaction
{
    public class TxnEventBase
    {
        private string _transactionId;
        private readonly long _timestamp;

        protected TxnEventBase(string transactionId, long timestamp)
        {
            _transactionId = transactionId;
            _timestamp = timestamp;
        }

        public string TransactionId
        {
            get => _transactionId;
            set => _transactionId = value;
        }

        public long Timestamp => _timestamp;

        public override string ToString()
        {
            return "transactionId=" + _transactionId +
                   " timestamp=" + _timestamp;
        }
    }
}
