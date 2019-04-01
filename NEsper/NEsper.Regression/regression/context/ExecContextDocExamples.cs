///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.context
{
    public class ExecContextDocExamples : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(BankTxn));
            epService.EPAdministrator.Configuration.AddEventType(typeof(LoginEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(LogoutEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(SecurityEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(SensorEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(TrafficEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(TrainEnterEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(TrainLeaveEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(CumulativePrice));
            epService.EPAdministrator.Configuration.AddEventType(typeof(PassengerScanEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyStartEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEndEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyInitEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyTermEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            epService.EPAdministrator.Configuration.AddEventType("StartEventOne", typeof(MyStartEvent));
            epService.EPAdministrator.Configuration.AddEventType("StartEventTwo", typeof(MyStartEvent));
            epService.EPAdministrator.Configuration.AddEventType("MyOtherEvent", typeof(MyStartEvent));
            epService.EPAdministrator.Configuration.AddEventType("EndEventOne", typeof(MyEndEvent));
            epService.EPAdministrator.Configuration.AddEventType("EndEventTwo", typeof(MyEndEvent));

            Create(epService, "create context SegmentedByCustomer partition by custId from BankTxn");
            Create(
                epService,
                "context SegmentedByCustomer select custId, account, sum(amount) from BankTxn group by account");
            Create(
                epService, "context SegmentedByCustomer\n" +
                           "select * from pattern [\n" +
                           "every a=BankTxn(amount > 400) -> b=BankTxn(amount > 400) where timer:within(10 minutes)\n" +
                           "]");
            epService.EPAdministrator.DestroyAllStatements();
            Create(
                epService, "create context SegmentedByCustomer partition by\n" +
                           "custId from BankTxn, loginId from LoginEvent, loginId from LogoutEvent");
            epService.EPAdministrator.DestroyAllStatements();
            Create(
                epService, "create context SegmentedByCustomer partition by\n" +
                           "custId from BankTxn, loginId from LoginEvent(IsFailed=false)");
            epService.EPAdministrator.DestroyAllStatements();
            Create(epService, "create context ByCustomerAndAccount partition by custId and account from BankTxn");
            Create(epService, "context ByCustomerAndAccount select custId, account, sum(amount) from BankTxn");
            Create(
                epService, "context ByCustomerAndAccount\n" +
                           "  select context.name, context.id, context.key1, context.key2 from BankTxn");
            epService.EPAdministrator.DestroyAllStatements();
            Create(epService, "create context ByCust partition by custId from BankTxn");
            Create(
                epService, "context ByCust\n" +
                           "select * from BankTxn as t1 unidirectional, BankTxn#time(30) t2\n" +
                           "where t1.amount = t2.amount");
            Create(
                epService, "context ByCust\n" +
                           "select * from SecurityEvent as t1 unidirectional, BankTxn#time(30) t2\n" +
                           "where t1.customerName = t2.customerName");
            epService.EPAdministrator.DestroyAllStatements();
            Create(
                epService, "create context CategoryByTemp\n" +
                           "group temp < 65 as cold,\n" +
                           "group temp between 65 and 85 as normal,\n" +
                           "group temp > 85 as large\n" +
                           "from SensorEvent");
            Create(epService, "context CategoryByTemp select context.label, count(*) from SensorEvent");
            Create(
                epService, "context CategoryByTemp\n" +
                           "select context.name, context.id, context.label from SensorEvent");
            Create(epService, "create context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *)");
            Create(epService, "context NineToFive select * from TrafficEvent(speed >= 100)");
            Create(
                epService, "context NineToFive\n" +
                           "select context.name, context.startTime, context.endTime from TrafficEvent(speed >= 100)");
            Create(
                epService, "create context CtxTrainEnter\n" +
                           "initiated by TrainEnterEvent as te\n" +
                           "terminated after 5 minutes");
            Create(
                epService, "context CtxTrainEnter\n" +
                           "select *, context.te.trainId, context.id, context.name from TrainLeaveEvent(trainId = context.te.trainId)");
            Create(
                epService, "context CtxTrainEnter\n" +
                           "select t1 from pattern [\n" +
                           "t1=TrainEnterEvent -> timer:interval(5 min) and not TrainLeaveEvent(trainId = context.te.trainId)]");
            Create(
                epService, "create context CtxEachMinute\n" +
                           "initiated by pattern [every timer:interval(1 minute)]\n" +
                           "terminated after 1 minutes");
            Create(epService, "context CtxEachMinute select avg(temp) from SensorEvent");
            Create(
                epService, "context CtxEachMinute\n" +
                           "select context.id, avg(temp) from SensorEvent output snapshot when terminated");
            Create(
                epService, "context CtxEachMinute\n" +
                           "select context.id, avg(temp) from SensorEvent output snapshot every 1 minute and when terminated");
            Create(
                epService, "select venue, ccyPair, side, sum(qty)\n" +
                           "from CumulativePrice\n" +
                           "where side='O'\n" +
                           "group by venue, ccyPair, side");
            Create(
                epService, "create context MyContext partition by venue, ccyPair, side from CumulativePrice(side='O')");
            Create(epService, "context MyContext select venue, ccyPair, side, sum(qty) from CumulativePrice");

            Create(
                epService, "create context SegmentedByCustomerHash\n" +
                           "coalesce by consistent_hash_crc32(custId) from BankTxn granularity 16 preallocate");
            Create(
                epService, "context SegmentedByCustomerHash\n" +
                           "select custId, account, sum(amount) from BankTxn group by custId, account");
            Create(
                epService, "create context HashedByCustomer as coalesce\n" +
                           "consistent_hash_crc32(custId) from BankTxn,\n" +
                           "consistent_hash_crc32(loginId) from LoginEvent,\n" +
                           "consistent_hash_crc32(loginId) from LogoutEvent\n" +
                           "granularity 32 preallocate");

            epService.EPAdministrator.DestroyAllStatements();
            Create(
                epService, "create context HashedByCustomer\n" +
                           "coalesce consistent_hash_crc32(loginId) from LoginEvent(IsFailed = false)\n" +
                           "granularity 1024 preallocate");
            Create(
                epService,
                "create context ByCustomerHash coalesce consistent_hash_crc32(custId) from BankTxn granularity 1024");
            Create(
                epService, "context ByCustomerHash\n" +
                           "select context.name, context.id from BankTxn");

            Create(
                epService, "create context NineToFiveSegmented\n" +
                           "context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *),\n" +
                           "context SegmentedByCustomer partition by custId from BankTxn");
            Create(
                epService, "context NineToFiveSegmented\n" +
                           "select custId, account, sum(amount) from BankTxn group by account");
            Create(
                epService, "create context CtxNestedTrainEnter\n" +
                           "context InitCtx initiated by TrainEnterEvent as te terminated after 5 minutes,\n" +
                           "context HashCtx coalesce by consistent_hash_crc32(tagId) from PassengerScanEvent\n" +
                           "granularity 16 preallocate");
            Create(
                epService, "context CtxNestedTrainEnter\n" +
                           "select context.InitCtx.te.trainId, context.HashCtx.id,\n" +
                           "tagId, count(*) from PassengerScanEvent group by tagId");
            Create(
                epService, "context NineToFiveSegmented\n" +
                           "select context.NineToFive.startTime, context.SegmentedByCustomer.key1 from BankTxn");
            Create(epService, "context NineToFiveSegmented select context.name, context.id from BankTxn");

            Create(epService, "create context MyContext start MyStartEvent end MyEndEvent");
            Create(epService, "create context MyContext2 initiated MyEvent(level > 0) terminated after 10 seconds");
            Create(
                epService, "create context MyContext3 \n" +
                           "start MyEvent as myevent\n" +
                           "end MyEvent(id=myevent.id)");
            Create(
                epService, "create context MyContext4 \n" +
                           "initiated by MyInitEvent as e1 \n" +
                           "terminated by MyTermEvent(id=e1.id, level <> e1.level)");
            Create(
                epService,
                "create context MyContext5 start pattern [StartEventOne or StartEventTwo] end after 5 seconds");
            Create(
                epService,
                "create context MyContext6 initiated by pattern [every MyInitEvent -> MyOtherEvent where timer:within(5)] terminated by MyTermEvent");
            Create(
                epService, "create context MyContext7 \n" +
                           "  start pattern [a=StartEventOne or  b=StartEventTwo]\n" +
                           "  end pattern [EndEventOne(id=a.id) or EndEventTwo(id=b.id)]");
            Create(epService, "create context MyContext8 initiated (*, *, *, *, *) terminated after 10 seconds");
            Create(epService, "create context NineToFive start after 10 seconds end after 1 minute");
            Create(epService, "create context Overlap5SecFor1Min initiated after 5 seconds terminated after 1 minute");
            Create(
                epService, "create context CtxSample\n" +
                           "initiated by MyStartEvent as startevent\n" +
                           "terminated by MyEndEvent(id = startevent.id) as endevent");
            Create(
                epService,
                "context CtxSample select context.endevent.id, count(*) from MyEvent output snapshot when terminated");

            Create(
                epService, "create context TxnCategoryContext \n" +
                           "  group by amount < 100 as small, \n" +
                           "  group by amount between 100 and 1000 as medium, \n" +
                           "  group by amount > 1000 as large from BankTxn");
            var stmt = Create(epService, "context TxnCategoryContext select * from BankTxn#time(1 minute)");
            var categorySmall = new ProxyContextPartitionSelectorCategory(() => Collections.SingletonList("small"));

            stmt.GetEnumerator(categorySmall);

            var categorySmallMed = new ProxyContextPartitionSelectorCategory(
                () => new HashSet<string>(Collections.List("small", "medium")));

            Create(epService, "context TxnCategoryContext create window BankTxnWindow#time(1 min) as BankTxn");
            epService.EPRuntime.ExecuteQuery(
                "select count(*) from BankTxnWindow", new ContextPartitionSelector[] {categorySmallMed});

            epService.EPAdministrator.Configuration.AddEventType(typeof(MyTwoKeyInit));
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyTwoKeyTerm));
            Create(
                epService, "create context CtxPerKeysAndExternallyControlled\n" +
                           "context PartitionedByKeys " +
                           "  partition by " +
                           "    key1, key2 from MyTwoKeyInit\n," +
                           "    key1, key2 from SensorEvent\n," +
                           "context InitiateAndTerm initiated by MyTwoKeyInit as e1 terminated by MyTwoKeyTerm(key1=e1.key1 and key2=e1.key2)");
            Create(
                epService, "context CtxPerKeysAndExternallyControlled\n" +
                           "select key1, key2, avg(temp) as avgTemp, count(*) as cnt\n" +
                           "from SensorEvent\n" +
                           "output snapshot when terminated\n" +
                           "// note: no group-by needed since \n");
        }

        private EPStatement Create(EPServiceProvider epService, string epl)
        {
            return epService.EPAdministrator.CreateEPL(epl);
        }

        public class CumulativePrice
        {
            public string Venue { get; }

            public string CcyPair { get; }

            public string Side { get; }

            public double Qty { get; }
        }

        public class TrainLeaveEvent
        {
            public int TrainId { get; }
        }

        public class TrainEnterEvent
        {
            public int TrainId { get; }
        }

        public class TrafficEvent
        {
            public double Speed { get; }
        }

        public class SensorEvent
        {
            public double Temp { get; }

            public int Key1 { get; set; }

            public int Key2 { get; set; }
        }

        public class LoginEvent
        {
            public string LoginId { get; }

            public bool IsFailed { get; }
        }

        public class LogoutEvent
        {
            public string LoginId { get; }
        }

        public class SecurityEvent
        {
            public string CustomerName { get; }
        }

        public class BankTxn
        {
            public string CustId { get; }

            public string Account { get; }

            public long Amount { get; }

            public string CustomerName { get; }
        }

        public class PassengerScanEvent
        {
            public PassengerScanEvent(string tagId)
            {
                TagId = tagId;
            }

            public string TagId { get; }
        }

        public class MyStartEvent
        {
            public int Id { get; }

            public int Level { get; }
        }

        public class MyEndEvent
        {
            public int Id { get; }

            public int Level { get; }
        }

        public class MyInitEvent
        {
            public int Id { get; }

            public int Level { get; }
        }

        public class MyTermEvent
        {
            public int Id { get; }

            public int Level { get; }
        }

        public class MyEvent
        {
            public int Id { get; }

            public int Level { get; }
        }

        public class MyTwoKeyInit
        {
            public int Key1 { get; }

            public int Key2 { get; }
        }

        public class MyTwoKeyTerm
        {
            public int Key1 { get; }

            public int Key2 { get; }
        }
    }
} // end of namespace