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
    public class TxnEventB : TxnEventBase
    {
        public TxnEventB(String transactionId, long timestamp)
            : base(transactionId, timestamp)
        {
        }
    }
}
