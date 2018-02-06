///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


/*
 * Created on Apr 22, 2006
 *
 */

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

/** Generates events for a continuous stream of transactions.
 * Rules for generating events are coded in <see cref="createNextTransaction"/>.
 * 
 * @author Hans Gilde
 *
 */

namespace NEsper.Examples.Transaction.sim
{
    public class TransactionEventSource : EventSource
    {
        protected String currentTransactionID;
        protected Random random = RandomUtil.GetNewInstance();
        protected List<TxnEventBase> transactionEvents;
        protected IEnumerator<TxnEventBase> transactionEnum;

        protected int maxTrans;
        protected int numTrans;

        protected FieldGenerator fieldGenerator = new FieldGenerator();

        /**
         * @param howManyTransactions How many transactions should events be generated for?
         */
        public TransactionEventSource(int howManyTransactions)
        {
            maxTrans = howManyTransactions;
        }

        protected List<TxnEventBase> CreateNextTransaction()
        {
            List<TxnEventBase> t = new List<TxnEventBase>();

            long beginningStamp = DateTimeHelper.CurrentTimeMillis;
            //skip event 1 with probability 1 in 5000
            if (random.Next(5000) < 4998)
            {
                TxnEventA txnEventA = new TxnEventA(null, beginningStamp, fieldGenerator.GetRandomCustomer());
                t.Add(txnEventA);
            }

            long e2Stamp = fieldGenerator.randomLatency(beginningStamp);
            //skip event 2 with probability 1 in 1000
            if (random.Next(1000) < 9998)
            {
                TxnEventB txnEventB = new TxnEventB(null, e2Stamp);
                t.Add(txnEventB);
            }

            long e3Stamp = fieldGenerator.randomLatency(e2Stamp);
            //skip event 3 with probability 1 in 10000
            if (random.Next(10000) < 9998)
            {
                TxnEventC txnEventC = new TxnEventC(null, e3Stamp, fieldGenerator.GetRandomSupplier());
                t.Add(txnEventC);
            }
            else
            {
                Log.Debug(".createNextTransaction generated missing event");
            }

            return t;
        }

        /**
         * @return Returns the maxTrans.
         */
        public int MaxTrans
        {
            get { return maxTrans; }
        }
        
        public override IEnumerator<TxnEventBase> GetEnumerator()
        {
        	while( numTrans < maxTrans )
        	{
        		while ((transactionEnum == null) ||
        		       (transactionEnum.MoveNext() == false))
        		{
			        //create a new transaction, with ID.
			        int id = random.Next();
			        currentTransactionID = Convert.ToString(id);
			        transactionEvents = CreateNextTransaction();
			        transactionEnum = transactionEvents.GetEnumerator();
        		}
        		
		        numTrans++;

		        TxnEventBase m = transactionEnum.Current;
		        m.TransactionId = currentTransactionID;
		        yield return m;
        	}
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
