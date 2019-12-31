///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace NEsper.Examples.Transaction
{
    public class TxnEventC : TxnEventBase
    {
        private readonly string supplierId;

        public TxnEventC(string transactionId, long timestamp, string supplierId)
            : base(transactionId, timestamp)
        {
            this.supplierId = supplierId;
        }

        public string SupplierId => supplierId;

        public override string ToString()
        {
            return base.ToString() + " supplierId=" + supplierId;
        }
    }
}
