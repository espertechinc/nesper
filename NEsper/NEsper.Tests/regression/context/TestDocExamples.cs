///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestDocExamples
    {
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestDocSamples()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(BankTxn));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(LoginEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(LogoutEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SecurityEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SensorEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(TrafficEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(TrainEnterEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(TrainLeaveEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(CumulativePrice));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(PassengerScanEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyStartEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyEndEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyInitEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyTermEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            _epService.EPAdministrator.Configuration.AddEventType("StartEventOne", typeof(MyStartEvent));
            _epService.EPAdministrator.Configuration.AddEventType("StartEventTwo", typeof(MyStartEvent));
            _epService.EPAdministrator.Configuration.AddEventType("MyOtherEvent", typeof(MyStartEvent));
            _epService.EPAdministrator.Configuration.AddEventType("EndEventOne", typeof(MyEndEvent));
            _epService.EPAdministrator.Configuration.AddEventType("EndEventTwo", typeof(MyEndEvent));

            Create("create context SegmentedByCustomer partition by custId from BankTxn");
            Create("context SegmentedByCustomer select custId, account, sum(amount) from BankTxn group by account");
            Create("context SegmentedByCustomer\n" +
                    "select * from pattern [\n" +
                    "every a=BankTxn(amount > 400) -> b=BankTxn(amount > 400) where timer:within(10 minutes)\n" +
                    "]");
            _epService.EPAdministrator.DestroyAllStatements();
            Create("create context SegmentedByCustomer partition by\n" +
                    "custId from BankTxn, loginId from LoginEvent, loginId from LogoutEvent");
            _epService.EPAdministrator.DestroyAllStatements();
            Create("create context SegmentedByCustomer partition by\n" +
                    "custId from BankTxn, loginId from LoginEvent(IsFailed=false)");
            _epService.EPAdministrator.DestroyAllStatements();
            Create("create context ByCustomerAndAccount partition by custId and account from BankTxn");
            Create("context ByCustomerAndAccount select custId, account, sum(amount) from BankTxn");
            Create("context ByCustomerAndAccount\n" +
                    "  select context.name, context.id, context.key1, context.key2 from BankTxn");
            _epService.EPAdministrator.DestroyAllStatements();
            Create("create context ByCust partition by custId from BankTxn");
            Create("context ByCust\n" +
                    "select * from BankTxn as t1 unidirectional, BankTxn.win:time(30) t2\n" +
                    "where t1.amount = t2.amount");
            Create("context ByCust\n" +
                    "select * from SecurityEvent as t1 unidirectional, BankTxn.win:time(30) t2\n" +
                    "where t1.customerName = t2.customerName");
            _epService.EPAdministrator.DestroyAllStatements();
            Create("create context CategoryByTemp\n" +
                    "group temp < 65 as cold,\n" +
                    "group temp between 65 and 85 as normal,\n" +
                    "group temp > 85 as large\n" +
                    "from SensorEvent");
            Create("context CategoryByTemp select context.label, count(*) from SensorEvent");
            Create("context CategoryByTemp\n" +
                    "select context.name, context.id, context.label from SensorEvent");
            Create("create context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *)");
            Create("context NineToFive select * from TrafficEvent(speed >= 100)");
            Create("context NineToFive\n" +
                    "select context.name, context.startTime, context.endTime from TrafficEvent(speed >= 100)");
            Create("create context CtxTrainEnter\n" +
                    "initiated by TrainEnterEvent as te\n" +
                    "terminated after 5 minutes");
            Create("context CtxTrainEnter\n" +
                    "select *, context.te.trainId, context.id, context.name from TrainLeaveEvent(trainId = context.te.trainId)");
            Create("context CtxTrainEnter\n" +
                    "select t1 from pattern [\n" +
                    "t1=TrainEnterEvent -> timer:interval(5 min) and not TrainLeaveEvent(trainId = context.te.trainId)]");
            Create("create context CtxEachMinute\n" +
                    "initiated by pattern [every timer:interval(1 minute)]\n" +
                    "terminated after 1 minutes");
            Create("context CtxEachMinute select avg(temp) from SensorEvent");
            Create("context CtxEachMinute\n" +
                    "select context.id, avg(temp) from SensorEvent output snapshot when terminated");
            Create("context CtxEachMinute\n" +
                    "select context.id, avg(temp) from SensorEvent output snapshot every 1 minute and when terminated");
            Create("select venue, ccyPair, side, sum(qty)\n" +
                    "from CumulativePrice\n" +
                    "where side='O'\n" +
                    "group by venue, ccyPair, side");
            Create("create context MyContext partition by venue, ccyPair, side from CumulativePrice(side='O')");
            Create("context MyContext select venue, ccyPair, side, sum(qty) from CumulativePrice");

            Create("create context SegmentedByCustomerHash\n" +
                    "coalesce by consistent_hash_crc32(custId) from BankTxn granularity 16 preallocate");
            Create("context SegmentedByCustomerHash\n" +
                    "select custId, account, sum(amount) from BankTxn group by custId, account");
            Create("create context HashedByCustomer as coalesce\n" +
                    "consistent_hash_crc32(custId) from BankTxn,\n" +
                    "consistent_hash_crc32(loginId) from LoginEvent,\n" +
                    "consistent_hash_crc32(loginId) from LogoutEvent\n" +
                    "granularity 32 preallocate");

            _epService.EPAdministrator.DestroyAllStatements();
            Create("create context HashedByCustomer\n" +
                    "coalesce consistent_hash_crc32(loginId) from LoginEvent(IsFailed = false)\n" +
                    "granularity 1024 preallocate");
            Create("create context ByCustomerHash coalesce consistent_hash_crc32(custId) from BankTxn granularity 1024");
            Create("context ByCustomerHash\n" +
                    "select context.name, context.id from BankTxn");

            Create("create context NineToFiveSegmented\n" +
                    "context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *),\n" +
                    "context SegmentedByCustomer partition by custId from BankTxn");
            Create("context NineToFiveSegmented\n" +
                    "select custId, account, sum(amount) from BankTxn group by account");
            Create("create context CtxNestedTrainEnter\n" +
                    "context InitCtx initiated by TrainEnterEvent as te terminated after 5 minutes,\n" +
                    "context HashCtx coalesce by consistent_hash_crc32(tagId) from PassengerScanEvent\n" +
                    "granularity 16 preallocate");
            Create("context CtxNestedTrainEnter\n" +
                    "select context.InitCtx.te.trainId, context.HashCtx.id,\n" +
                    "tagId, count(*) from PassengerScanEvent group by tagId");
            Create("context NineToFiveSegmented\n" +
                    "select context.NineToFive.startTime, context.SegmentedByCustomer.key1 from BankTxn");
            Create("context NineToFiveSegmented select context.name, context.id from BankTxn");

            Create("create context MyContext start MyStartEvent end MyEndEvent");
            Create("create context MyContext2 initiated MyEvent(level > 0) terminated after 10 seconds");
            Create("create context MyContext3 \n" +
                    "start MyEvent as myevent\n" +
                    "end MyEvent(id=myevent.id)");
            Create("create context MyContext4 \n" +
                    "initiated by MyInitEvent as e1 \n" +
                    "terminated by MyTermEvent(id=e1.id, level <> e1.level)");
            Create("create context MyContext5 start pattern [StartEventOne or StartEventTwo] end after 5 seconds");
            Create("create context MyContext6 initiated by pattern [every MyInitEvent -> MyOtherEvent where timer:within(5)] terminated by MyTermEvent");
            Create("create context MyContext7 \n" +
                    "  start pattern [a=StartEventOne or  b=StartEventTwo]\n" +
                    "  end pattern [EndEventOne(id=a.id) or EndEventTwo(id=b.id)]");
            Create("create context MyContext8 initiated (*, *, *, *, *) terminated after 10 seconds");
            Create("create context NineToFive start after 10 seconds end after 1 minute");
            Create("create context Overlap5SecFor1Min initiated after 5 seconds terminated after 1 minute");
            Create("create context CtxSample\n" +
                    "initiated by MyStartEvent as startevent\n" +
                    "terminated by MyEndEvent(id = startevent.id) as endevent");
            Create("context CtxSample select context.endevent.id, count(*) from MyEvent output snapshot when terminated");

            Create("create context TxnCategoryContext \n" +
                    "  group by amount < 100 as small, \n" +
                    "  group by amount between 100 and 1000 as medium, \n" +
                    "  group by amount > 1000 as large from BankTxn");
            EPStatement stmt = Create("context TxnCategoryContext select * from BankTxn.win:time(1 minute)");
            ContextPartitionSelectorCategory categorySmall = new ProxyContextPartitionSelectorCategory
            {
                ProcLabels = () => "small".AsSingleton()
            };
            stmt.GetEnumerator(categorySmall);
            ContextPartitionSelectorCategory categorySmallMed = new ProxyContextPartitionSelectorCategory
            {
                ProcLabels = () => Collections.List("small", "medium")
            };
            Create("context TxnCategoryContext create window BankTxnWindow.win:time(1 min) as BankTxn");
            _epService.EPRuntime.ExecuteQuery("select count(*) from BankTxnWindow", new ContextPartitionSelector[] { categorySmallMed });

            _epService.EPAdministrator.Configuration.AddEventType<MyTwoKeyInit>();
            _epService.EPAdministrator.Configuration.AddEventType<MyTwoKeyTerm>();
            Create("create context CtxPerKeysAndExternallyControlled\n" +
                    "context PartitionedByKeys " +
                    "  partition by " +
                    "    key1, key2 from MyTwoKeyInit\n," +
                    "    key1, key2 from SensorEvent\n," +
                    "context InitiateAndTerm initiated by MyTwoKeyInit as e1 terminated by MyTwoKeyTerm(key1=e1.key1 and key2=e1.key2)");
            Create("context CtxPerKeysAndExternallyControlled\n" +
                    "select key1, key2, avg(temp) as avgTemp, count(*) as cnt\n" +
                    "from SensorEvent\n" +
                    "output snapshot when terminated\n" +
                    "// note: no group-by needed since \n");
        }

        private EPStatement Create(String epl)
        {
            return _epService.EPAdministrator.CreateEPL(epl);
        }

        public class CumulativePrice
        {
            public string Venue { get; private set; }
            public string CcyPair { get; private set; }
            public string Side { get; private set; }
            public double Qty { get; private set; }
        }

        public class TrainLeaveEvent
        {
            public int TrainId { get; private set; }
        }

        public class TrainEnterEvent
        {
            public int TrainId { get; private set; }
        }

        public class TrafficEvent
        {
            public double Speed { get; private set; }
        }

        public class SensorEvent
        {
            public double Temp { get; private set; }
            public int Key1 { get; set; }
            public int Key2 { get; set; }
        }

        public class LoginEvent
        {
            public string LoginId { get; private set; }
            public bool IsFailed { get; private set; }
        }

        public class LogoutEvent
        {
            public string LoginId { get; private set; }
        }

        public class SecurityEvent
        {
            public string CustomerName { get; private set; }
        }

        public class BankTxn
        {
            public string CustId { get; private set; }
            public string Account { get; private set; }
            public long Amount { get; private set; }
            public string CustomerName { get; private set; }
        }

        public class PassengerScanEvent
        {
            public string TagId { get; private set; }
            public PassengerScanEvent(String tagId)
            {
                TagId = tagId;
            }
        }

        public class MyStartEvent
        {
            public int Id { get; private set; }
            public int Level { get; private set; }
        }

        public class MyEndEvent
        {
            public int Id { get; private set; }
            public int Level { get; private set; }
        }

        public class MyInitEvent
        {
            public int Id { get; private set; }
            public int Level { get; private set; }
        }

        public class MyTermEvent
        {
            public int Id { get; private set; }
            public int Level { get; private set; }
        }

        public class MyEvent
        {
            public int Id { get; private set; }
            public int Level { get; private set; }
        }

        public class MyTwoKeyInit
        {
            public int Key1 { get; private set; }
            public int Key2 { get; private set; }
        }

        public class MyTwoKeyTerm
        {
            public int Key1 { get; private set; }
            public int Key2 { get; private set; }
        }
    }
}
