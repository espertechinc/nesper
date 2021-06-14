///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
            Create(env, path, "create context SegmentedByCustomer partition by CustId from BankTxn");
            Create(
                env,
                path,
                "context SegmentedByCustomer select CustId, Account, sum(Amount) from BankTxn group by Account");
            Create(
                env,
                path,
                "context SegmentedByCustomer\n" +
                "select * from pattern [\n" +
                "every a=BankTxn(Amount > 400) -> b=BankTxn(Amount > 400) where timer:within(10 minutes)\n" +
                "]");
            UndeployClearPath(env, path);
            Create(
                env,
                path,
                "create context SegmentedByCustomer partition by\n" +
                "CustId from BankTxn, LoginId from LoginEvent, LoginId from LogoutEvent");
            UndeployClearPath(env, path);
            Create(
                env,
                path,
                "create context SegmentedByCustomer partition by\n" +
                "CustId from BankTxn, LoginId from LoginEvent(IsFailed=false)");
            UndeployClearPath(env, path);
            Create(env, path, "create context ByCustomerAndAccount partition by CustId and Account from BankTxn");
            Create(env, path, "context ByCustomerAndAccount select CustId, Account, sum(Amount) from BankTxn");
            Create(
                env,
                path,
                "context ByCustomerAndAccount\n" +
                "  select context.name, context.id, context.key1, context.key2 from BankTxn");
            UndeployClearPath(env, path);
            Create(env, path, "create context ByCust partition by CustId from BankTxn");
            Create(
                env,
                path,
                "context ByCust\n" +
                "select * from BankTxn as t1 unidirectional, BankTxn#time(30) t2\n" +
                "where t1.Amount = t2.Amount");
            Create(
                env,
                path,
                "context ByCust\n" +
                "select * from SecurityEvent as t1 unidirectional, BankTxn#time(30) t2\n" +
                "where t1.CustomerName = t2.CustomerName");
            UndeployClearPath(env, path);
            Create(
                env,
                path,
                "create context CategoryByTemp\n" +
                "group Temp < 65 as cold,\n" +
                "group Temp between 65 and 85 as normal,\n" +
                "group Temp > 85 as large\n" +
                "from SensorEvent");
            Create(env, path, "context CategoryByTemp select context.label, count(*) from SensorEvent");
            Create(
                env,
                path,
                "context CategoryByTemp\n" +
                "select context.name, context.id, context.label from SensorEvent");
            Create(env, path, "create context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *)");
            Create(env, path, "context NineToFive select * from TrafficEvent(Speed >= 100)");
            Create(
                env,
                path,
                "context NineToFive\n" +
                "select context.name, context.startTime, context.endTime from TrafficEvent(Speed >= 100)");
            Create(
                env,
                path,
                "create context CtxTrainEnter\n" +
                "initiated by TrainEnterEvent as te\n" +
                "terminated after 5 minutes");
            Create(
                env,
                path,
                "context CtxTrainEnter\n" +
                "select *, context.te.TrainId, context.id, context.name from TrainLeaveEvent(TrainId = context.te.TrainId)");
            Create(
                env,
                path,
                "context CtxTrainEnter\n" +
                "select t1 from pattern [\n" +
                "t1=TrainEnterEvent -> timer:interval(5 min) and not TrainLeaveEvent(TrainId = context.te.TrainId)]");
            Create(
                env,
                path,
                "create context CtxEachMinute\n" +
                "initiated by pattern [every timer:interval(1 minute)]\n" +
                "terminated after 1 minutes");
            Create(env, path, "context CtxEachMinute select avg(Temp) from SensorEvent");
            Create(
                env,
                path,
                "context CtxEachMinute\n" +
                "select context.id, avg(Temp) from SensorEvent output snapshot when terminated");
            Create(
                env,
                path,
                "context CtxEachMinute\n" +
                "select context.id, avg(Temp) from SensorEvent output snapshot every 1 minute and when terminated");
            Create(
                env,
                path,
                "select Venue, CcyPair, Side, sum(Qty)\n" +
                "from CumulativePrice\n" +
                "where Side='O'\n" +
                "group by Venue, CcyPair, Side");
            Create(
                env,
                path,
                "create context MyContext partition by Venue, CcyPair, Side from CumulativePrice(Side='O')");
            Create(env, path, "context MyContext select Venue, CcyPair, Side, sum(Qty) from CumulativePrice");

            Create(
                env,
                path,
                "create context SegmentedByCustomerHash\n" +
                "coalesce by consistent_hash_crc32(CustId) from BankTxn granularity 16 preallocate");
            Create(
                env,
                path,
                "context SegmentedByCustomerHash\n" +
                "select CustId, Account, sum(Amount) from BankTxn group by CustId, Account");
            Create(
                env,
                path,
                "create context HashedByCustomer as coalesce\n" +
                "consistent_hash_crc32(CustId) from BankTxn,\n" +
                "consistent_hash_crc32(LoginId) from LoginEvent,\n" +
                "consistent_hash_crc32(LoginId) from LogoutEvent\n" +
                "granularity 32 preallocate");

            UndeployClearPath(env, path);
            Create(
                env,
                path,
                "create context HashedByCustomer\n" +
                "coalesce consistent_hash_crc32(LoginId) from LoginEvent(IsFailed = false)\n" +
                "granularity 1024 preallocate");
            Create(
                env,
                path,
                "create context ByCustomerHash coalesce consistent_hash_crc32(CustId) from BankTxn granularity 1024");
            Create(
                env,
                path,
                "context ByCustomerHash\n" +
                "select context.name, context.id from BankTxn");

            Create(
                env,
                path,
                "create context NineToFiveSegmented\n" +
                "context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *),\n" +
                "context SegmentedByCustomer partition by CustId from BankTxn");
            Create(
                env,
                path,
                "context NineToFiveSegmented\n" +
                "select CustId, Account, sum(Amount) from BankTxn group by Account");
            Create(
                env,
                path,
                "create context CtxNestedTrainEnter\n" +
                "context InitCtx initiated by TrainEnterEvent as te terminated after 5 minutes,\n" +
                "context HashCtx coalesce by consistent_hash_crc32(TagId) from PassengerScanEvent\n" +
                "granularity 16 preallocate");
            Create(
                env,
                path,
                "context CtxNestedTrainEnter\n" +
                "select context.InitCtx.te.TrainId, context.HashCtx.id,\n" +
                "TagId, count(*) from PassengerScanEvent group by TagId");
            Create(
                env,
                path,
                "context NineToFiveSegmented\n" +
                "select context.NineToFive.startTime, context.SegmentedByCustomer.key1 from BankTxn");
            Create(env, path, "context NineToFiveSegmented select context.name, context.id from BankTxn");

            Create(env, path, "create context MyContext start MyStartEvent end MyEndEvent");
            Create(env, path, "create context MyContext2 initiated MyEvent(Level > 0) terminated after 10 seconds");
            Create(
                env,
                path,
                "create context MyContext3 \n" +
                "start MyEvent as myevent\n" +
                "end MyEvent(Id=myevent.Id)");
            Create(
                env,
                path,
                "create context MyContext4 \n" +
                "initiated by MyInitEvent as e1 \n" +
                "terminated by MyTermEvent(Id=e1.Id, Level <> e1.Level)");
            Create(
                env,
                path,
                "create context MyContext5 start pattern [StartEventOne or StartEventTwo] end after 5 seconds");
            Create(
                env,
                path,
                "create context MyContext6 initiated by pattern [every MyInitEvent -> MyOtherEvent where timer:within(5)] terminated by MyTermEvent");
            Create(
                env,
                path,
                "create context MyContext7 \n" +
                "  start pattern [a=StartEventOne or  b=StartEventTwo]\n" +
                "  end pattern [EndEventOne(Id=a.Id) or EndEventTwo(Id=b.Id)]");
            Create(env, path, "create context MyContext8 initiated (*, *, *, *, *) terminated after 10 seconds");
            Create(env, path, "create context NineToFive start after 10 seconds end after 1 minute");
            Create(env, path, "create context Overlap5SecFor1Min initiated after 5 seconds terminated after 1 minute");
            Create(
                env,
                path,
                "create context CtxSample\n" +
                "initiated by MyStartEvent as startevent\n" +
                "terminated by MyEndEvent(Id = startevent.Id) as endevent");
            Create(
                env,
                path,
                "context CtxSample select context.endevent.Id, count(*) from MyEvent output snapshot when terminated");

            Create(
                env,
                path,
                "create context TxnCategoryContext \n" +
                "  group by Amount < 100 as small, \n" +
                "  group by Amount between 100 and 1000 as medium, \n" +
                "  group by Amount > 1000 as large from BankTxn");
            Create(env, path, "@Name('s0') context TxnCategoryContext select * from BankTxn#time(1 minute)");
            ContextPartitionSelectorCategory categorySmall = new ProxyContextPartitionSelectorCategory {
                ProcLabels = () => Collections.SingletonList("small")
            };
            env.Statement("s0").GetEnumerator(categorySmall);
            ContextPartitionSelectorCategory categorySmallMed = new ProxyContextPartitionSelectorCategory {
                ProcLabels = () => new HashSet<string>(Arrays.AsList("small", "medium"))
            };
            Create(env, path, "context TxnCategoryContext create window BankTxnWindow#time(1 min) as BankTxn");
            var faf = env.CompileFAF("select count(*) from BankTxnWindow", path);
            env.Runtime.FireAndForgetService.ExecuteQuery(faf, new ContextPartitionSelector[] {categorySmallMed});

            Create(
                env,
                path,
                "create context CtxPerKeysAndExternallyControlled\n" +
                "context PartitionedByKeys " +
                "  partition by " +
                "    Key1, Key2 from MyTwoKeyInit\n," +
                "    Key1, Key2 from SensorEvent\n," +
                "context InitiateAndTerm initiated by MyTwoKeyInit as e1 terminated by MyTwoKeyTerm(Key1=e1.Key1 and Key2=e1.Key2)");
            Create(
                env,
                path,
                "context CtxPerKeysAndExternallyControlled\n" +
                "select Key1, Key2, avg(Temp) as avgTemp, count(*) as cnt\n" +
                "from SensorEvent\n" +
                "output snapshot when terminated\n" +
                "// note: no group-by needed since \n");

            Create(
                env,
                path,
                "create context PerCustId_TriggeredByLargeAmount\n" +
                "  partition by CustId from BankTxn \n" +
                "  initiated by BankTxn(Amount>100) as largeTxn");
            Create(
                env,
                path,
                "context PerCustId_TriggeredByLargeAmount select context.largeTxn, CustId, sum(Amount) from BankTxn");
            Create(
                env,
                path,
                "create context PerCustId_UntilExpired\n" +
                "  partition by CustId from BankTxn \n" +
                "  terminated by BankTxn(IsExpired=true)");
            Create(
                env,
                path,
                "context PerCustId_UntilExpired select CustId, sum(Amount) from BankTxn output last when terminated");
            Create(
                env,
                path,
                "create context PerCustId_TriggeredByLargeAmount_UntilExpired\n" +
                "  partition by CustId from BankTxn \n" +
                "  initiated by BankTxn(Amount>100) as txn\n" +
                "  terminated by BankTxn(IsExpired=true and User=txn.User)");
            Create(
                env,
                path,
                "create context PerCust_AmountGreater100\n" +
                "  partition by CustId from BankTxn(Amount>100)\n" +
                "  initiated by BankTxn");
            Create(env, path, "context PerCust_AmountGreater100 select CustId, sum(Amount) from BankTxn");
            Create(
                env,
                path,
                "create context PerCust_TriggeredByLargeTxn\n" +
                "  partition by CustId from BankTxn\n" +
                "  initiated by BankTxn(Amount>100)");
            Create(env, path, "context PerCust_TriggeredByLargeTxn select CustId, sum(Amount) from BankTxn");

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

        public class CumulativePrice
        {
            public string Venue { get; set; }

            public string CcyPair { get; set; }

            public string Side { get; set; }

            public double Qty { get; set; }
        }

        public class TrainLeaveEvent
        {
            public int TrainId { get; set; }
        }

        public class TrainEnterEvent
        {
            public int TrainId { get; set; }
        }

        public class TrafficEvent
        {
            public double Speed { get; set; }
        }

        public class SensorEvent
        {
            public double Temp { get; set; }

            public int Key1 { get; set; }

            public int Key2 { get; set; }
        }

        public class LoginEvent
        {
            public string LoginId { get; set; }

            public bool IsFailed { get; set; }
        }

        public class LogoutEvent
        {
            public string LoginId { get; set; }
        }

        public class SecurityEvent
        {
            public string CustomerName { get; set; }
        }

        public class BankTxn
        {
            public string CustId { get; set; }

            public string Account { get; set; }

            public long Amount { get; set; }

            public string CustomerName { get; set; }

            public bool IsExpired { get; set; }

            public string User { get; set; }
        }

        public class PassengerScanEvent
        {
            public PassengerScanEvent(string tagId)
            {
                TagId = tagId;
            }

            public string TagId { get; set; }
        }

        public class MyStartEvent
        {
            public int Level { get; set; }

            public int Id { get; set; }
        }

        public class MyEndEvent
        {
            public int Level { get; set; }

            public int Id { get; set; }
        }

        public class MyInitEvent
        {
            public int Level { get; set; }

            public int Id { get; set; }
        }

        public class MyTermEvent
        {
            public int Level { get; set; }

            public int Id { get; set; }
        }

        public class MyEvent
        {
            public int Level { get; set; }

            public int Id { get; set; }
        }

        public class MyTwoKeyInit
        {
            public int Key1 { get; set; }

            public int Key2 { get; set; }
        }

        public class MyTwoKeyTerm
        {
            public int Key1 { get; set; }

            public int Key2 { get; set; }
        }
    }
} // end of namespace