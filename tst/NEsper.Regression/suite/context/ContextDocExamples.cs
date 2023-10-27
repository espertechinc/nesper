///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;


namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextDocExamples : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            Create(env, path, "@Public create context SegmentedByCustomer partition by custId from BankTxn");
            Create(
                env,
                path,
                "context SegmentedByCustomer select custId, account, sum(Amount) from BankTxn group by account");
            Create(
                env,
                path,
                "context SegmentedByCustomer\n" +
                "select * from pattern [\n" +
                "every a=BankTxn(amount > 400) -> b=BankTxn(Amount > 400) where timer:within(10 minutes)\n" +
                "]");
            UndeployClearPath(env, path);
            Create(
                env,
                path,
                "@Public create context SegmentedByCustomer partition by\n" +
                "custId from BankTxn, loginId from LoginEvent, loginId from LogoutEvent");
            UndeployClearPath(env, path);
            Create(
                env,
                path,
                "@Public create context SegmentedByCustomer partition by\n" +
                "custId from BankTxn, loginId from LoginEvent(failed=false)");
            UndeployClearPath(env, path);
            Create(
                env,
                path,
                "@Public create context ByCustomerAndAccount partition by custId and account from BankTxn");
            Create(env, path, "context ByCustomerAndAccount select custId, account, sum(Amount) from BankTxn");
            Create(
                env,
                path,
                "context ByCustomerAndAccount\n" +
                "  select context.name, context.Id, context.key1, context.key2 from BankTxn");
            UndeployClearPath(env, path);
            Create(env, path, "@Public create context ByCust partition by custId from BankTxn");
            Create(
                env,
                path,
                "context ByCust\n" +
                "select * from BankTxn as t1 unidirectional, BankTxn#time(30) t2\n" +
                "where t1.amount = t2.Amount");
            Create(
                env,
                path,
                "context ByCust\n" +
                "select * from SecurityEvent as t1 unidirectional, BankTxn#time(30) t2\n" +
                "where t1.customerName = t2.customerName");
            UndeployClearPath(env, path);
            Create(
                env,
                path,
                "@Public create context CategoryByTemp\n" +
                "group temp < 65 as cold,\n" +
                "group temp between 65 and 85 as normal,\n" +
                "group temp > 85 as large\n" +
                "from SensorEvent");
            Create(env, path, "context CategoryByTemp select context.label, count(*) from SensorEvent");
            Create(
                env,
                path,
                "context CategoryByTemp\n" +
                "select context.name, context.Id, context.label from SensorEvent");
            Create(env, path, "@Public create context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *)");
            Create(env, path, "context NineToFive select * from TrafficEvent(speed >= 100)");
            Create(
                env,
                path,
                "context NineToFive\n" +
                "select context.name, context.startTime, context.endTime from TrafficEvent(speed >= 100)");
            Create(
                env,
                path,
                "@Public create context CtxTrainEnter\n" +
                "initiated by TrainEnterEvent as te\n" +
                "terminated after 5 minutes");
            Create(
                env,
                path,
                "context CtxTrainEnter\n" +
                "select *, context.te.trainId, context.Id, context.name from TrainLeaveEvent(trainId = context.te.trainId)");
            Create(
                env,
                path,
                "context CtxTrainEnter\n" +
                "select t1 from pattern [\n" +
                "t1=TrainEnterEvent -> timer:interval(5 min) and not TrainLeaveEvent(trainId = context.te.trainId)]");
            Create(
                env,
                path,
                "@Public create context CtxEachMinute\n" +
                "initiated by pattern [every timer:interval(1 minute)]\n" +
                "terminated after 1 minutes");
            Create(env, path, "context CtxEachMinute select avg(temp) from SensorEvent");
            Create(
                env,
                path,
                "context CtxEachMinute\n" +
                "select context.Id, avg(temp) from SensorEvent output snapshot when terminated");
            Create(
                env,
                path,
                "context CtxEachMinute\n" +
                "select context.Id, avg(temp) from SensorEvent output snapshot every 1 minute and when terminated");
            Create(
                env,
                path,
                "select venue, ccyPair, side, sum(qty)\n" +
                "from CumulativePrice\n" +
                "where side='O'\n" +
                "group by venue, ccyPair, side");
            Create(
                env,
                path,
                "@Public create context MyContext partition by venue, ccyPair, side from CumulativePrice(side='O')");
            Create(env, path, "context MyContext select venue, ccyPair, side, sum(qty) from CumulativePrice");

            Create(
                env,
                path,
                "@Public create context SegmentedByCustomerHash\n" +
                "coalesce by consistent_hash_crc32(custId) from BankTxn granularity 16 preallocate");
            Create(
                env,
                path,
                "context SegmentedByCustomerHash\n" +
                "select custId, account, sum(Amount) from BankTxn group by custId, account");
            Create(
                env,
                path,
                "@Public create context HashedByCustomer as coalesce\n" +
                "consistent_hash_crc32(custId) from BankTxn,\n" +
                "consistent_hash_crc32(loginId) from LoginEvent,\n" +
                "consistent_hash_crc32(loginId) from LogoutEvent\n" +
                "granularity 32 preallocate");

            UndeployClearPath(env, path);
            Create(
                env,
                path,
                "@Public create context HashedByCustomer\n" +
                "coalesce consistent_hash_crc32(loginId) from LoginEvent(failed = false)\n" +
                "granularity 1024 preallocate");
            Create(
                env,
                path,
                "@Public create context ByCustomerHash coalesce consistent_hash_crc32(custId) from BankTxn granularity 1024");
            Create(
                env,
                path,
                "context ByCustomerHash\n" +
                "select context.name, context.Id from BankTxn");

            Create(
                env,
                path,
                "@Public create context NineToFiveSegmented\n" +
                "context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *),\n" +
                "context SegmentedByCustomer partition by custId from BankTxn");
            Create(
                env,
                path,
                "context NineToFiveSegmented\n" +
                "select custId, account, sum(Amount) from BankTxn group by account");
            Create(
                env,
                path,
                "@Public create context CtxNestedTrainEnter\n" +
                "context InitCtx initiated by TrainEnterEvent as te terminated after 5 minutes,\n" +
                "context HashCtx coalesce by consistent_hash_crc32(tagId) from PassengerScanEvent\n" +
                "granularity 16 preallocate");
            Create(
                env,
                path,
                "context CtxNestedTrainEnter\n" +
                "select context.InitCtx.te.trainId, context.HashCtx.Id,\n" +
                "tagId, count(*) from PassengerScanEvent group by tagId");
            Create(
                env,
                path,
                "context NineToFiveSegmented\n" +
                "select context.NineToFive.startTime, context.SegmentedByCustomer.key1 from BankTxn");
            Create(env, path, "context NineToFiveSegmented select context.name, context.Id from BankTxn");

            Create(env, path, "@Public create context MyContext start MyStartEvent end MyEndEvent");
            Create(
                env,
                path,
                "@Public create context MyContext2 initiated MyEvent(level > 0) terminated after 10 seconds");
            Create(
                env,
                path,
                "@Public create context MyContext3 \n" +
                "start MyEvent as myevent\n" +
                "end MyEvent(Id=myevent.Id)");
            Create(
                env,
                path,
                "@Public create context MyContext4 \n" +
                "initiated by MyInitEvent as e1 \n" +
                "terminated by MyTermEvent(Id=e1.Id, level <> e1.level)");
            Create(
                env,
                path,
                "@Public create context MyContext5 start pattern [StartEventOne or StartEventTwo] end after 5 seconds");
            Create(
                env,
                path,
                "@Public create context MyContext6 initiated by pattern [every MyInitEvent -> MyOtherEvent where timer:within(5)] terminated by MyTermEvent");
            Create(
                env,
                path,
                "@Public create context MyContext7 \n" +
                "  start pattern [a=StartEventOne or  b=StartEventTwo]\n" +
                "  end pattern [EndEventOne(id=a.Id) or EndEventTwo(Id=b.Id)]");
            Create(
                env,
                path,
                "@Public create context MyContext8 initiated (*, *, *, *, *) terminated after 10 seconds");
            Create(env, path, "@Public create context NineToFive start after 10 seconds end after 1 minute");
            Create(
                env,
                path,
                "@Public create context Overlap5SecFor1Min initiated after 5 seconds terminated after 1 minute");
            Create(
                env,
                path,
                "@Public create context CtxSample\n" +
                "initiated by MyStartEvent as startevent\n" +
                "terminated by MyEndEvent(Id = startevent.Id) as endevent");
            Create(
                env,
                path,
                "context CtxSample select context.endevent.Id, count(*) from MyEvent output snapshot when terminated");

            Create(
                env,
                path,
                "@Public create context TxnCategoryContext \n" +
                "  group by Amount < 100 as small, \n" +
                "  group by Amount between 100 and 1000 as medium, \n" +
                "  group by Amount > 1000 as large from BankTxn");
            Create(env, path, "@name('s0') context TxnCategoryContext select * from BankTxn#time(1 minute)");
            ContextPartitionSelectorCategory categorySmall =
                new ProxyContextPartitionSelectorCategory(() => Collections.SingletonSet("small"));
            env.AssertStatement("s0", statement => statement.GetEnumerator(categorySmall));
            ContextPartitionSelectorCategory categorySmallMed = new ProxyContextPartitionSelectorCategory(
                () => new HashSet<string>(Arrays.AsList("small", "medium")));

            Create(env, path, "@Public context TxnCategoryContext create window BankTxnWindow#time(1 min) as BankTxn");
            env.AssertThat(
                () => {
                    var faf = env.CompileFAF("select count(*) from BankTxnWindow", path);
                    env.Runtime.FireAndForgetService.ExecuteQuery(
                        faf,
                        new ContextPartitionSelector[] { categorySmallMed });
                });

            Create(
                env,
                path,
                "@Public create context CtxPerKeysAndExternallyControlled\n" +
                "context PartitionedByKeys " +
                "  partition by " +
                "    key1, key2 from MyTwoKeyInit\n," +
                "    key1, key2 from SensorEvent\n," +
                "context InitiateAndTerm initiated by MyTwoKeyInit as e1 terminated by MyTwoKeyTerm(key1=e1.key1 and key2=e1.key2)");
            Create(
                env,
                path,
                "context CtxPerKeysAndExternallyControlled\n" +
                "select key1, key2, avg(temp) as avgTemp, count(*) as cnt\n" +
                "from SensorEvent\n" +
                "output snapshot when terminated\n" +
                "// note: no group-by needed since \n");

            Create(
                env,
                path,
                "@Public create context PerCustId_TriggeredByLargeAmount\n" +
                "  partition by custId from BankTxn \n" +
                "  initiated by BankTxn(Amount>100) as largeTxn");
            Create(
                env,
                path,
                "context PerCustId_TriggeredByLargeAmount select context.largeTxn, custId, sum(Amount) from BankTxn");
            Create(
                env,
                path,
                "@Public create context PerCustId_UntilExpired\n" +
                "  partition by custId from BankTxn \n" +
                "  terminated by BankTxn(expired=true)");
            Create(
                env,
                path,
                "context PerCustId_UntilExpired select custId, sum(Amount) from BankTxn output last when terminated");
            Create(
                env,
                path,
                "@Public create context PerCustId_TriggeredByLargeAmount_UntilExpired\n" +
                "  partition by custId from BankTxn \n" +
                "  initiated by BankTxn(Amount>100) as txn\n" +
                "  terminated by BankTxn(expired=true and user=txn.user)");
            Create(
                env,
                path,
                "@Public create context PerCust_AmountGreater100\n" +
                "  partition by custId from BankTxn(Amount>100)\n" +
                "  initiated by BankTxn");
            Create(env, path, "context PerCust_AmountGreater100 select custId, sum(Amount) from BankTxn");
            Create(
                env,
                path,
                "@Public create context PerCust_TriggeredByLargeTxn\n" +
                "  partition by custId from BankTxn\n" +
                "  initiated by BankTxn(Amount>100)");
            Create(env, path, "context PerCust_TriggeredByLargeTxn select custId, sum(Amount) from BankTxn");

            env.UndeployAll();
        }

        private void UndeployClearPath(
            RegressionEnvironment env,
            RegressionPath path)
        {
            env.UndeployAll();
            path.Clear();
        }

        private void Create(
            RegressionEnvironment env,
            RegressionPath path,
            string epl)
        {
            env.CompileDeploy(epl, path);
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class CumulativePrice
        {
            private string venue;
            private string ccyPair;
            private string side;
            private double qty;

            public string Venue => venue;

            public string CcyPair => ccyPair;

            public string Side => side;

            public double Qty => qty;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class TrainLeaveEvent
        {
            private int trainId;
            public int TrainId => trainId;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class TrainEnterEvent
        {
            private int trainId;

            public int TrainId => trainId;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class TrafficEvent
        {
            private double speed;

            public double Speed => speed;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class SensorEvent
        {
            private double temp;
            private int key1;
            private int key2;

            public double Temp => temp;

            public int Key1 {
                get => key1;
                set => key1 = value;
            }

            public int Key2 {
                get => key2;
                set => key2 = value;
            }
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class LoginEvent
        {
            private string loginId;
            private bool failed;

            public string LoginId => loginId;

            public bool Failed => failed;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class LogoutEvent
        {
            private string loginId;

            public string LoginId => loginId;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class SecurityEvent
        {
            private string customerName;

            public string CustomerName => customerName;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class BankTxn
        {
            private string custId;
            private string account;
            private long amount;
            private string customerName;
            private bool expired;
            private string user;


            public string CustId => custId;

            public string Account => account;

            public long Amount => amount;

            public string CustomerName => customerName;

            public bool IsExpired => expired;

            public string User => user;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class PassengerScanEvent
        {
            private readonly string tagId;

            public PassengerScanEvent(string tagId)
            {
                this.tagId = tagId;
            }

            public string TagId => tagId;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class MyStartEvent
        {
            private int id;
            private int level;

            public int Id => id;

            public int Level => level;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class MyEndEvent
        {
            private int id;
            private int level;

            public int Id => id;

            public int Level => level;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class MyInitEvent
        {
            private int id;
            private int level;

            public int Id => id;

            public int Level => level;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class MyTermEvent
        {
            private int id;
            private int level;

            public int Id => id;

            public int Level => level;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class MyEvent
        {
            private int id;
            private int level;

            public int Id => id;

            public int Level => level;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class MyTwoKeyInit
        {
            private int key1;
            private int key2;

            public int Key1 => key1;

            public int Key2 => key2;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        [Serializable]
        public class MyTwoKeyTerm
        {
            private int key1;
            private int key2;

            public int Key1 => key1;

            public int Key2 => key2;
        }
    }
} // end of namespace