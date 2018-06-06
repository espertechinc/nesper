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
    public class TxnEventC : TxnEventBase
    {
        private String supplierId;

        public TxnEventC(String transactionId, long timestamp, String supplierId)
            : base(transactionId, timestamp)
        {
            this.supplierId = supplierId;
        }

        public String SupplierId
        {
            get { return supplierId; }
        }

        public override String ToString()
        {
            return base.ToString() + " supplierId=" + supplierId;
        }
    }
}
