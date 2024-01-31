///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace NEsper.Examples.Transaction
{
    public class TxnEventA : TxnEventBase
    {
        private readonly string customerId;

        public TxnEventA(string transactionId, long timestamp, string customerId)
            : base(transactionId, timestamp)
        {
            this.customerId = customerId;
        }

        public string CustomerId => customerId;

        public override string ToString()
        {
            return base.ToString() + " customerId=" + customerId;
        }
    }
}
