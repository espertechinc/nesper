///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.context;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraExecuteQuery : IndexBackingTableInfo
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
    
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            config.AddEventType<SupportBean>("SupportBean");
            config.AddEventType<SupportBean_A>("SupportBean_A");
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            SupportQueryPlanIndexHook.Reset();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestInsert() {
            RunAssertionInsert(true);
            RunAssertionInsert(false);
        }
    
        [Test]
        public void TestParameterizedQuery() {
            RunAssertionParameterizedQuery(true);
            RunAssertionParameterizedQuery(false);
        }
    
        [Test]
        public void TestUpdate() {
            RunAssertionUpdate(true);
            RunAssertionUpdate(false);
        }
    
        [Test]
        public void TestDelete() {
            RunAssertionDelete(true);
            RunAssertionDelete(false);
        }
    
        [Test]
        public void TestDeleteContextPartitioned() {
            RunAssertionDeleteContextPartitioned(true);
            RunAssertionDeleteContextPartitioned(false);
        }
    
        [Test]
        public void TestSelectWildcard() {
            RunAssertionSelectWildcard(true);
            RunAssertionSelectWildcard(false);
        }
        
        [Test]
        public void TestAssertionSelectCountStar() {
            RunAssertionSelectCountStar(true);
            RunAssertionSelectCountStar(false);
        }
    
        [Test]
        public void TestAggUngroupedRowForAll() {
            RunAssertionAggUngroupedRowForAll(true);
            RunAssertionAggUngroupedRowForAll(false);
        }
    
        [Test]
        public void TestInClause() {
            RunAssertionInClause(true);
            RunAssertionInClause(false);
        }
    
        [Test]
        public void TestAggUngroupedRowForGroup() {
            RunAssertionAggUngroupedRowForGroup(true);
            RunAssertionAggUngroupedRowForGroup(false);
        }
    
        [Test]
        public void TestJoin() {
            RunAssertionJoin(true, true);
            RunAssertionJoin(false, false);
            RunAssertionJoin(true, false);
            RunAssertionJoin(false, true);
        }
    
        [Test]
        public void TestAggUngroupedRowForEvent() {
            RunAssertionAggUngroupedRowForEvent(true);
            RunAssertionAggUngroupedRowForEvent(false);
        }
    
        [Test]
        public void TestJoinWhere() {
            RunAssertionJoinWhere(true);
            RunAssertionJoinWhere(false);
        }
    
        [Test]
        public void Test3StreamInnerJoin() {
            RunAssertion3StreamInnerJoin(EventRepresentationEnum.OBJECTARRAY, true);
            RunAssertion3StreamInnerJoin(EventRepresentationEnum.MAP, true);
            RunAssertion3StreamInnerJoin(EventRepresentationEnum.DEFAULT, true);
            RunAssertion3StreamInnerJoin(EventRepresentationEnum.OBJECTARRAY, false);
        }
    
        [Test]
        public void TestInvalid() {
            RunAssertionInvalid(true);
            RunAssertionInvalid(false);
        }
    
        [Test]
        public void TestExecuteFilter()
        {
            RunAssertionExecuteFilter(true);
            RunAssertionExecuteFilter(false);
        }
    
        private void RunAssertionExecuteFilter(bool isNamedWindow) 
        {
            SetupInfra(isNamedWindow);
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
    
            var query = "select * from MyInfra(IntPrimitive > 1, IntPrimitive < 10)";
            RunAssertionFilter(query);
    
            query = "select * from MyInfra(IntPrimitive > 1) where IntPrimitive < 10";
            RunAssertionFilter(query);
    
            query = "select * from MyInfra where IntPrimitive < 10 and IntPrimitive > 1";
            RunAssertionFilter(query);
    
            DestroyInfra();
        }
    
        private void RunAssertionFilter(string query)
        {
            var fields = "TheString,IntPrimitive".Split(',');
            var result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { "E3", 5 } });
    
            var prepared = _epService.EPRuntime.PrepareQuery(query);
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new object[][] { new object[] { "E3", 5 } });
        }
    
        private void RunAssertionInvalid(bool isNamedWindow)
        {
            SetupInfra(isNamedWindow);
            string epl;
    
            epl = "insert into MyInfra select 1";
            TryInvalid(epl, "Error executing statement: Column '1' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into MyInfra select 1]");
    
            epl = "selectoo man";
            TryInvalidSyntax(epl, "Incorrect syntax near 'selectoo' [selectoo man]");
    
            epl = "select (select * from MyInfra) from MyInfra";
            TryInvalid(epl, "Subqueries are not a supported feature of on-demand queries [select (select * from MyInfra) from MyInfra]");
    
            epl = "select * from MyInfra output every 10 seconds";
            TryInvalid(epl, "Error executing statement: Output rate limiting is not a supported feature of on-demand queries [select * from MyInfra output every 10 seconds]");
    
            epl = "select prev(1, TheString) from MyInfra";
            TryInvalid(epl, "Error executing statement: Failed to validate select-clause expression 'prev(1,TheString)': Previous function cannot be used in this context [select prev(1, TheString) from MyInfra]");
    
            epl = "insert into MyInfra(IntPrimitive) select 'a'";
            if (isNamedWindow) {
                TryInvalid(epl, "Error executing statement: Invalid assignment of column 'IntPrimitive' of type 'System.String' to event property 'IntPrimitive' typed as 'System.Int32', column and parameter types mismatch [insert into MyInfra(IntPrimitive) select 'a']");
            }
            else {
                TryInvalid(epl, "Error executing statement: Invalid assignment of column 'IntPrimitive' of type 'System.String' to event property 'IntPrimitive' typed as 'System.Int32', column and parameter types mismatch [insert into MyInfra(IntPrimitive) select 'a']");
            }
    
            epl = "insert into MyInfra(IntPrimitive, TheString) select 1";
            TryInvalid(epl, "Error executing statement: Number of supplied values in the select or values clause does not match insert-into clause [insert into MyInfra(IntPrimitive, TheString) select 1]");
    
            epl = "insert into MyInfra select 1 as IntPrimitive from MyInfra";
            TryInvalid(epl, "Error executing statement: Insert-into fire-and-forget query can only consist of an insert-into clause and a select-clause [insert into MyInfra select 1 as IntPrimitive from MyInfra]");
    
            epl = "insert into MyInfra(IntPrimitive, TheString) values (1, 'a', 1)";
            TryInvalid(epl, "Error executing statement: Number of supplied values in the select or values clause does not match insert-into clause [insert into MyInfra(IntPrimitive, TheString) values (1, 'a', 1)]");
    
            if (isNamedWindow) {
                epl = "select * from pattern [every MyInfra]";
                TryInvalid(epl, "Error executing statement: On-demand queries require tables or named windows and do not allow event streams or patterns [select * from pattern [every MyInfra]]");
    
                epl = "select * from MyInfra.stat:uni(IntPrimitive)";
                TryInvalid(epl, "Error executing statement: Views are not a supported feature of on-demand queries [select * from MyInfra.stat:uni(IntPrimitive)]");
            }
    
            DestroyInfra();
        }
    
        private void TryInvalid(string epl, string message)
        {
            try
            {
                _epService.EPRuntime.ExecuteQuery(epl);
                Assert.Fail();
            }
            catch(EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void TryInvalidSyntax(string epl, string message)
        {
            try
            {
                _epService.EPRuntime.ExecuteQuery(epl);
                Assert.Fail();
            }
            catch(EPStatementSyntaxException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void RunAssertion3StreamInnerJoin(EventRepresentationEnum eventRepresentationEnum, bool isNamedWindow)
        {
            var eplEvents = eventRepresentationEnum.GetAnnotationText() + " create schema Product (productId string, categoryId string);" +
                    eventRepresentationEnum.GetAnnotationText() + " create schema Category (categoryId string, owner string);" +
                    eventRepresentationEnum.GetAnnotationText() + " create schema ProductOwnerDetails (productId string, owner string);";
            string epl;
            if (isNamedWindow) {
                epl = eplEvents +
                        eventRepresentationEnum.GetAnnotationText() + " create window WinProduct.win:keepall() as select * from Product;" +
                        eventRepresentationEnum.GetAnnotationText() + " create window WinCategory.win:keepall() as select * from Category;" +
                        eventRepresentationEnum.GetAnnotationText() + " create window WinProductOwnerDetails.win:keepall() as select * from ProductOwnerDetails;" +
                        "insert into WinProduct select * from Product;" +
                        "insert into WinCategory select * from Category;" +
                        "insert into WinProductOwnerDetails select * from ProductOwnerDetails;";
            }
            else {
                epl = eplEvents +
                      "create table WinProduct (productId string primary key, categoryId string primary key);" +
                      "create table WinCategory (categoryId string primary key, owner string primary key);" +
                      "create table WinProductOwnerDetails (productId string primary key, owner string);" +
                      "on Product t1 merge WinProduct t2 where t1.productId = t2.productId and t1.categoryId = t2.categoryId when not matched then insert select productId, categoryId;" +
                      "on Category t1 merge WinCategory t2 where t1.categoryId = t2.categoryId when not matched then insert select categoryId, owner;" +
                      "on ProductOwnerDetails t1 merge WinProductOwnerDetails t2 where t1.productId = t2.productId when not matched then insert select productId, owner;";
            }
            var dAdmin = _epService.EPAdministrator.DeploymentAdmin;
            dAdmin.Deploy(dAdmin.Parse(epl), new DeploymentOptions());
    
            SendEvent(eventRepresentationEnum, _epService, "Product", new string[] {"productId=Product1", "categoryId=Category1"});
            SendEvent(eventRepresentationEnum, _epService, "Product", new string[] {"productId=Product2", "categoryId=Category1"});
            SendEvent(eventRepresentationEnum, _epService, "Product", new string[] {"productId=Product3", "categoryId=Category1"});
            SendEvent(eventRepresentationEnum, _epService, "Category", new string[] {"categoryId=Category1", "owner=Petar"});
            SendEvent(eventRepresentationEnum, _epService, "ProductOwnerDetails", new string[] {"productId=Product1", "owner=Petar"});
    
            var fields = "WinProduct.productId".Split(',');
            EventBean[] queryResults;
            queryResults = _epService.EPRuntime.ExecuteQuery("" +
                    "select WinProduct.productId " +
                    " from WinProduct" +
                    " inner join WinCategory on WinProduct.categoryId=WinCategory.categoryId" +
                    " inner join WinProductOwnerDetails on WinProduct.productId=WinProductOwnerDetails.productId"
            ).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new object[][] { new object[] { "Product1" } });
    
            queryResults = _epService.EPRuntime.ExecuteQuery("" +
                    "select WinProduct.productId " +
                    " from WinProduct" +
                    " inner join WinCategory on WinProduct.categoryId=WinCategory.categoryId" +
                    " inner join WinProductOwnerDetails on WinProduct.productId=WinProductOwnerDetails.productId" +
                    " where WinCategory.owner=WinProductOwnerDetails.owner"
            ).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new object[][] { new object[] { "Product1" } });
    
            queryResults = _epService.EPRuntime.ExecuteQuery("" +
                    "select WinProduct.productId " +
                    " from WinProduct, WinCategory, WinProductOwnerDetails" +
                    " where WinCategory.owner=WinProductOwnerDetails.owner" +
                    " and WinProduct.categoryId=WinCategory.categoryId" +
                    " and WinProduct.productId=WinProductOwnerDetails.productId"
            ).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new object[][] { new object[] { "Product1" } });
    
            var eplQuery = "select WinProduct.productId " +
                    " from WinProduct" +
                    " inner join WinCategory on WinProduct.categoryId=WinCategory.categoryId" +
                    " inner join WinProductOwnerDetails on WinProduct.productId=WinProductOwnerDetails.productId" +
                    " having WinCategory.owner=WinProductOwnerDetails.owner";
            queryResults = _epService.EPRuntime.ExecuteQuery(eplQuery).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new object[][] { new object[] { "Product1" } });
    
            var model = _epService.EPAdministrator.CompileEPL(eplQuery);
            queryResults = _epService.EPRuntime.ExecuteQuery(model).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new object[][] { new object[] { "Product1" } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("Product", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("Category", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("ProductOwnerDetails", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("WinProduct", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("WinCategory", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("WinProductOwnerDetails", false);
        }
    
        private void RunAssertionJoinWhere(bool isNamedWindow) 
        {
            var eplCreateOne = isNamedWindow ?
                    (EventRepresentationEnum.MAP.GetAnnotationText() + " create window Infra1.win:keepall() (key String, keyJoin String)") :
                    "create table Infra1 (key String primary key, keyJoin String)";
            var eplCreateTwo = isNamedWindow ?
                    (EventRepresentationEnum.MAP.GetAnnotationText() + " create window Infra2.win:keepall() (keyJoin String, value double)") :
                    "create table Infra2 (keyJoin String primary key, value double primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreateOne);
            _epService.EPAdministrator.CreateEPL(eplCreateTwo);
    
            var queryAgg = "select w1.key, sum(value) from Infra1 w1, Infra2 w2 WHERE w1.keyJoin = w2.keyJoin GROUP BY w1.key order by w1.key";
            var fieldsAgg = "w1.key,sum(value)".Split(',');
            var queryNoagg = "select w1.key, w2.value from Infra1 w1, Infra2 w2 where w1.keyJoin = w2.keyJoin and value = 1 order by w1.key";
            var fieldsNoagg = "w1.key,w2.value".Split(',');
    
            var result = _epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            Assert.AreEqual(0, result.Length);
            result = _epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            Assert.IsNull(result);
    
            InsertInfra1Event("key1", "keyJoin1");
    
            result = _epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            Assert.AreEqual(0, result.Length);
            result = _epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            Assert.IsNull(result);
    
            InsertInfra2Event("keyJoin1", 1d);
    
            result = _epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsAgg, new object[][] { new object[] { "key1", 1d } });
            result = _epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsNoagg, new object[][] { new object[] { "key1", 1d } });
    
            InsertInfra2Event("keyJoin2", 2d);
    
            result = _epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsAgg, new object[][] { new object[] { "key1", 1d } });
            result = _epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsNoagg, new object[][] { new object[] { "key1", 1d } });
    
            InsertInfra1Event("key2", "keyJoin2");
    
            result = _epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsAgg, new object[][] { new object[] { "key1", 1d }, new object[] { "key2", 2d } });
            result = _epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsNoagg, new object[][] { new object[] { "key1", 1d } });
    
            InsertInfra2Event("keyJoin2", 1d);
    
            result = _epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsAgg, new object[][] { new object[] { "key1", 1d }, new object[] { "key2", 3d } });
            result = _epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsNoagg, new object[][] { new object[] { "key1", 1d }, new object[] { "key2", 1d } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("Infra1", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("Infra2", false);
        }
    
        private void InsertInfra1Event(string key, string keyJoin) {
            _epService.EPRuntime.ExecuteQuery(string.Format("insert into Infra1 values ('{0}', '{1}')", key, keyJoin));
        }
    
        private void InsertInfra2Event(string keyJoin, double value) {
            _epService.EPRuntime.ExecuteQuery(string.Format("insert into Infra2 values ('{0}', {1}d)", keyJoin, value));
        }
    
        public void RunAssertionAggUngroupedRowForEvent(bool isNamedWindow) 
        {
            SetupInfra(isNamedWindow);
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            var fields = new string[] {"TheString", "total"};
    
            var query = "select TheString, sum(IntPrimitive) as total from MyInfra";
            var result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.GetEnumerator(), fields, new object[][] { new object[] { "E1", 16 }, new object[] { "E2", 16 }, new object[] { "E3", 16 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", -2));
            result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.GetEnumerator(), fields, new object[][] { new object[] { "E1", 14 }, new object[] { "E2", 14 }, new object[] { "E3", 14 }, new object[] { "E4", 14 } });
            
            DestroyInfra();
        }
    
        private void RunAssertionJoin(bool isFirstNW, bool isSecondNW) 
        {
           SetupInfra(isFirstNW);
            
            var eplSecondCreate = isSecondNW ?
                    "create window MySecondInfra.win:keepall() as select * from SupportBean_A" :
                    "create table MySecondInfra as (id string primary key)";
            _epService.EPAdministrator.CreateEPL(eplSecondCreate);
            var eplSecondFill = isSecondNW ?
                    "insert into MySecondInfra select * from SupportBean_A " :
                    "on SupportBean_A sba merge MySecondInfra msi where msi.id = sba.id when not matched then insert select id";
            _epService.EPAdministrator.CreateEPL(eplSecondFill);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            var fields = new string[] {"TheString", "IntPrimitive", "id"};
    
            var query = "select TheString, IntPrimitive, id from MyInfra nw1, " +
                    "MySecondInfra nw2 where nw1.TheString = nw2.id";
            var result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { "E2", 11, "E2" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            _epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
    
            result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.GetEnumerator(), fields, new object[][] { new object[] { "E2", 11, "E2" }, new object[] { "E3", 1, "E3" }, new object[] { "E3", 2, "E3" } });
            
            DestroyInfra();
            _epService.EPAdministrator.Configuration.RemoveEventType("MySecondInfra", false);
        }
    
        private void RunAssertionAggUngroupedRowForGroup(bool isNamedWindow) 
        {
            SetupInfra(isNamedWindow);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            var fields = new string[] {"TheString", "total"};
    
            var query = "select TheString, sum(IntPrimitive) as total from MyInfra group by TheString order by TheString asc";
            var result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { "E1", 6 }, new object[] { "E2", 11 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", -2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { "E1", 6 }, new object[] { "E2", 9 }, new object[] { "E3", 3 } });
    
            DestroyInfra();
        }
    
        private void RunAssertionInClause(bool isNamedWindow)
        {
            SetupInfra(isNamedWindow);
            
            _epService.EPRuntime.SendEvent(MakeBean("E1", 10, 100L));
            _epService.EPRuntime.SendEvent(MakeBean("E2", 20, 200L));
            _epService.EPRuntime.SendEvent(MakeBean("E3", 30, 300L));
            _epService.EPRuntime.SendEvent(MakeBean("E4", 40, 400L));
    
            // try no index
            RunAssertionIn();
    
            // try suitable index
            var stmtIdx1 = _epService.EPAdministrator.CreateEPL("create index Idx1 on MyInfra(TheString, IntPrimitive)");
            RunAssertionIn();
            stmtIdx1.Dispose();
    
            // backwards index
            var stmtIdx2 = _epService.EPAdministrator.CreateEPL("create index Idx2 on MyInfra(IntPrimitive, TheString)");
            RunAssertionIn();
            stmtIdx2.Dispose();
    
            // partial index
            var stmtIdx3 = _epService.EPAdministrator.CreateEPL("create index Idx3 on MyInfra(IntPrimitive)");
            RunAssertionIn();
            stmtIdx3.Dispose();
    
            DestroyInfra();
        }
    
        private void RunAssertionIn()
        {
            TryAssertionIn("TheString in ('E2', 'E3') and IntPrimitive in (10, 20)", new long?[]{200L});
            TryAssertionIn("IntPrimitive in (30, 20) and TheString in ('E4', 'E1')", new long?[]{});
            TryAssertionIn("IntPrimitive in (30, 20) and TheString in ('E2', 'E1')", new long?[]{200L});
            TryAssertionIn("TheString in ('E2', 'E3') and IntPrimitive in (20, 30)", new long?[]{200L, 300L});
            TryAssertionIn("TheString in ('E2', 'E3') and IntPrimitive in (30, 20)", new long?[]{200L, 300L});
            TryAssertionIn("TheString in ('E1', 'E2', 'E3', 'E4') and IntPrimitive in (10, 20, 30)", new long?[]{100L, 200L, 300L});
        }
    
        private void TryAssertionIn(string filter, long?[] expected)
        {
            var result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where " + filter);
            Assert.AreEqual(result.Array.Length, expected.Length);
            IList<long?> values = new List<long?>();
            foreach (var @event in result.Array) {
                values.Add((long?) @event.Get("LongPrimitive"));
            }
            EPAssertionUtil.AssertEqualsAnyOrder(expected, values.ToArray());
        }
    
        private void RunAssertionAggUngroupedRowForAll(bool isNamedWindow) 
        {
            SetupInfra(isNamedWindow);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            var fields = new string[] {"total"};
    
            var query = "select sum(IntPrimitive) as total from MyInfra";
            var result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { 16 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", -2));
            result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { 14 } });
    
            DestroyInfra();
        }
    
        private void RunAssertionSelectCountStar(bool isNamedWindow) 
        {
            SetupInfra(isNamedWindow);
            
            var fields = new string[] {"cnt"};
            var query = "select count(*) as cnt from MyInfra";
            var prepared = _epService.EPRuntime.PrepareQuery(query);
    
            var result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { 0L } });
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new object[][] { new object[] { 0L } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { 1L } });
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new object[][] { new object[] { 1L } });
            result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { 1L } });
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new object[][] { new object[] { 1L } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { 2L } });
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new object[][] { new object[] { 2L } });
    
            var model = _epService.EPAdministrator.CompileEPL(query);
            result = _epService.EPRuntime.ExecuteQuery(model);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { 2L } });
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new object[][] { new object[] { 2L } });
    
            var preparedFromModel = _epService.EPRuntime.PrepareQuery(model);
            EPAssertionUtil.AssertPropsPerRow(preparedFromModel.Execute().GetEnumerator(), fields, new object[][] { new object[] { 2L } });
    
            DestroyInfra();
        }
    
        private void RunAssertionSelectWildcard(bool isNamedWindow) 
        {
            SetupInfra(isNamedWindow);
    
            var query = "select * from MyInfra";
            var prepared = _epService.EPRuntime.PrepareQuery(query);
            var result = _epService.EPRuntime.ExecuteQuery(query);
            var fields = new string[] {"TheString", "IntPrimitive"};
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            result = _epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(prepared.Execute().GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
    
            DestroyInfra();
        }
    
        private void RunAssertionDeleteContextPartitioned(bool isNamedWindow)
        {
            // test hash-segmented context
            var eplCtx = "create context MyCtx coalesce consistent_hash_crc32(TheString) from SupportBean granularity 4 preallocate";
            _epService.EPAdministrator.CreateEPL(eplCtx);
    
            var eplCreate = isNamedWindow ?
                    "context MyCtx create window CtxInfra.win:keepall() as SupportBean" :
                    "context MyCtx create table CtxInfra (TheString string primary key, IntPrimitive int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            var eplPopulate = isNamedWindow ?
                    "context MyCtx insert into CtxInfra select * from SupportBean" :
                    "context MyCtx on SupportBean sb merge CtxInfra ci where sb.TheString = ci.TheString and sb.IntPrimitive = ci.IntPrimitive when not matched then insert select TheString, IntPrimitive";
            _epService.EPAdministrator.CreateEPL(eplPopulate);
    
            var codeFunc = new SupportHashCodeFuncGranularCRC32(4);
            var codes = new int[5];
            for (var i = 0; i < 5; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
                codes[i] = codeFunc.CodeFor("E" + i);
            }
            EPAssertionUtil.AssertEqualsExactOrder(new int[] {3, 1, 3, 1, 2}, codes);   // just to make sure CRC32 didn't change
    
            // assert counts individually per context partition
            AssertCtxInfraCountPerCode(new long[] {0, 2, 1, 2});
    
            // delete per context partition (E0 ended up in '3')
            _epService.EPRuntime.ExecuteQuery("delete from CtxInfra where TheString = 'E0'", new ContextPartitionSelector[] {new SupportSelectorByHashCode(1)});
            AssertCtxInfraCountPerCode(new long[] {0, 2, 1, 2});
    
            var result = _epService.EPRuntime.ExecuteQuery("delete from CtxInfra where TheString = 'E0'", new ContextPartitionSelector[] {new SupportSelectorByHashCode(3)});
            AssertCtxInfraCountPerCode(new long[] {0, 2, 1, 1});
            if (isNamedWindow) {
                EPAssertionUtil.AssertPropsPerRow(result.Array, "TheString".Split(','), new object[][] { new object[] { "E0" } });
            }
    
            // delete per context partition (E1 ended up in '1')
            _epService.EPRuntime.ExecuteQuery("delete from CtxInfra where TheString = 'E1'", new ContextPartitionSelector[] {new SupportSelectorByHashCode(0)});
            AssertCtxInfraCountPerCode(new long[] {0, 2, 1, 1});
    
            _epService.EPRuntime.ExecuteQuery("delete from CtxInfra where TheString = 'E1'", new ContextPartitionSelector[] {new SupportSelectorByHashCode(1)});
            AssertCtxInfraCountPerCode(new long[] {0, 1, 1, 1});
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test category-segmented context
            var eplCtxCategory = "create context MyCtxCat group by IntPrimitive < 0 as negative, group by IntPrimitive > 0 as positive from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplCtxCategory);
            _epService.EPAdministrator.CreateEPL("context MyCtxCat create window CtxInfraCat.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("context MyCtxCat insert into CtxInfraCat select * from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -2));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", -3));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            Assert.AreEqual(2L, GetCtxInfraCatCount("positive"));
            Assert.AreEqual(2L, GetCtxInfraCatCount("negative"));
    
            result = _epService.EPRuntime.ExecuteQuery("context MyCtxCat delete from CtxInfraCat where context.label = 'negative'");
            Assert.AreEqual(2L, GetCtxInfraCatCount("positive"));
            Assert.AreEqual(0L, GetCtxInfraCatCount("negative"));
            EPAssertionUtil.AssertPropsPerRow(result.Array, "TheString".Split(','), new object[][] { new object[] { "E1" }, new object[] { "E3" } });
    
            DestroyInfra();
            _epService.EPAdministrator.Configuration.RemoveEventType("CtxInfra", false);
        }
    
        private void RunAssertionDelete(bool isNamedWindow)
        {
            SetupInfra(isNamedWindow);
    
            // test delete-all
            for (var i = 0; i < 10; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            Assert.AreEqual(10L, MyInfraCount);
            var result = _epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            Assert.AreEqual(0L, MyInfraCount);
            if (isNamedWindow) {
                Assert.AreEqual(_epService.EPAdministrator.Configuration.GetEventType("MyInfra"), result.EventType);
                Assert.AreEqual(10, result.Array.Length);
                Assert.AreEqual("E0", result.Array[0].Get("TheString"));
            }
            else {
                Assert.AreEqual(0, result.Array.Length);
            }
    
            // test SODA + where-clause
            for (var i = 0; i < 10; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            Assert.AreEqual(10L, MyInfraCount);
            var eplWithWhere = "delete from MyInfra where TheString=\"E1\"";
            var modelWithWhere = _epService.EPAdministrator.CompileEPL(eplWithWhere);
            Assert.AreEqual(eplWithWhere, modelWithWhere.ToEPL());
            result = _epService.EPRuntime.ExecuteQuery(modelWithWhere);
            Assert.AreEqual(9L, MyInfraCount);
            if (isNamedWindow) {
                Assert.AreEqual(_epService.EPAdministrator.Configuration.GetEventType("MyInfra"), result.EventType);
                EPAssertionUtil.AssertPropsPerRow(result.Array, "TheString".Split(','), new object[][] { new object[] { "E1" } });
            }
    
            // test SODA delete-all
            var eplDelete = "delete from MyInfra";
            var modelDeleteOnly = _epService.EPAdministrator.CompileEPL(eplDelete);
            Assert.AreEqual(eplDelete, modelDeleteOnly.ToEPL());
            _epService.EPRuntime.ExecuteQuery(modelDeleteOnly);
            Assert.AreEqual(0L, MyInfraCount);
    
            // test with index
            if (isNamedWindow) {
                _epService.EPAdministrator.CreateEPL("create unique index Idx1 on MyInfra (TheString)");
            }
            for (var i = 0; i < 5; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            RunQueryAssertCount(INDEX_CALLBACK_HOOK + "delete from MyInfra where TheString = 'E1' and IntPrimitive = 0", 5, isNamedWindow ? "Idx1" : "primary-MyInfra", isNamedWindow ? BACKING_SINGLE_UNIQUE : BACKING_MULTI_UNIQUE);
            RunQueryAssertCount(INDEX_CALLBACK_HOOK + "delete from MyInfra where TheString = 'E1' and IntPrimitive = 1", 4, isNamedWindow ? "Idx1" : "primary-MyInfra", isNamedWindow ? BACKING_SINGLE_UNIQUE : BACKING_MULTI_UNIQUE);
            RunQueryAssertCount(INDEX_CALLBACK_HOOK + "delete from MyInfra where TheString = 'E2'", 3, isNamedWindow ? "Idx1" : null, isNamedWindow ? BACKING_SINGLE_UNIQUE : null);
            RunQueryAssertCount(INDEX_CALLBACK_HOOK + "delete from MyInfra where IntPrimitive = 4", 2, null, null);
    
            // test with alias
            RunQueryAssertCount(INDEX_CALLBACK_HOOK + "delete from MyInfra as w1 where w1.TheString = 'E3'", 1, isNamedWindow ? "Idx1" : null, isNamedWindow ? BACKING_SINGLE_UNIQUE : null);
    
            // test consumption
            var stmt = _epService.EPAdministrator.CreateEPL("select rstream * from MyInfra");
            stmt.Events += _listener.Update;
            _epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            var fields = new string[] {"TheString", "IntPrimitive"};
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E0", 0});
            }
            else {
                Assert.IsFalse(_listener.IsInvoked);
            }
    
            DestroyInfra();
        }
    
        private void RunQueryAssertCount(string epl, int count, string indexName, string backingClass) {
            _epService.EPRuntime.ExecuteQuery(epl);
            Assert.AreEqual(count, MyInfraCount);
            SupportQueryPlanIndexHook.AssertFAFAndReset(indexName, backingClass);
        }

        private long MyInfraCount
        {
            get { return _epService.EPRuntime.ExecuteQuery("select count(*) as c0 from MyInfra").Array[0].Get("c0").AsLong(); }
        }

        private void RunAssertionUpdate(bool isNamedWindow)
        {
            SetupInfra(isNamedWindow);
            var fields = new string[] {"TheString", "IntPrimitive"};
    
            // test update-all
            for (var i = 0; i < 2; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            var result = _epService.EPRuntime.ExecuteQuery("update MyInfra set TheString = 'ABC'");
            var resultInOrder = _epService.EPAdministrator.GetStatement("TheInfra")
                .OrderBy(e => e.Get("IntPrimitive"))
                .GetEnumerator();
            EPAssertionUtil.AssertPropsPerRow(resultInOrder, fields, new object[][] { new object[] { "ABC", 0 }, new object[] { "ABC", 1 } });
            if (isNamedWindow) {
                EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "ABC", 0 }, new object[] { "ABC", 1 } });
            }
    
            // test update with where-clause
            _epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            for (var i = 0; i < 3; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            result = _epService.EPRuntime.ExecuteQuery("update MyInfra set TheString = 'X', IntPrimitive=-1 where TheString = 'E1' and IntPrimitive = 1");
            if (isNamedWindow) {
                EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "X", -1 } });
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_epService.EPAdministrator.GetStatement("TheInfra").GetEnumerator(), fields, new object[][] { new object[] { "E0", 0 }, new object[] { "E2", 2 }, new object[] { "X", -1 } });
    
            // test update with SODA
            var epl = "update MyInfra set IntPrimitive=IntPrimitive+10 where TheString=\"E2\"";
            var model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            result = _epService.EPRuntime.ExecuteQuery(model);
            if (isNamedWindow) {
                EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E2", 12 } });
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_epService.EPAdministrator.GetStatement("TheInfra").GetEnumerator(), fields, new object[][] { new object[] { "E0", 0 }, new object[] { "X", -1 }, new object[] { "E2", 12 } });
    
            // test update with initial value
            result = _epService.EPRuntime.ExecuteQuery("update MyInfra set IntPrimitive=5, TheString='x', TheString = initial.TheString || 'y', IntPrimitive=initial.IntPrimitive+100 where TheString = 'E0'");
            if (isNamedWindow) {
                EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E0y", 100 } });
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_epService.EPAdministrator.GetStatement("TheInfra").GetEnumerator(), fields, new object[][] { new object[] { "X", -1 }, new object[] { "E2", 12 }, new object[] { "E0y", 100 } });
            _epService.EPRuntime.ExecuteQuery("delete from MyInfra");
    
            // test with index
            if (isNamedWindow) {
                _epService.EPAdministrator.CreateEPL("create unique index Idx1 on MyInfra (TheString)");
            }
            for (var i = 0; i < 5; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            RunQueryAssertCountNonNegative(INDEX_CALLBACK_HOOK + "update MyInfra set IntPrimitive=-1 where TheString = 'E1' and IntPrimitive = 0", 5, isNamedWindow ? "Idx1" : "primary-MyInfra", isNamedWindow ? BACKING_SINGLE_UNIQUE : BACKING_MULTI_UNIQUE);
            RunQueryAssertCountNonNegative(INDEX_CALLBACK_HOOK + "update MyInfra set IntPrimitive=-1 where TheString = 'E1' and IntPrimitive = 1", 4, isNamedWindow ? "Idx1" : "primary-MyInfra", isNamedWindow ? BACKING_SINGLE_UNIQUE : BACKING_MULTI_UNIQUE);
            RunQueryAssertCountNonNegative(INDEX_CALLBACK_HOOK + "update MyInfra set IntPrimitive=-1 where TheString = 'E2'", 3, isNamedWindow ? "Idx1" : null, isNamedWindow ? BACKING_SINGLE_UNIQUE : null);
            RunQueryAssertCountNonNegative(INDEX_CALLBACK_HOOK + "update MyInfra set IntPrimitive=-1 where IntPrimitive = 4", 2, null, null);
    
            // test with alias
            RunQueryAssertCountNonNegative(INDEX_CALLBACK_HOOK + "update MyInfra as w1 set IntPrimitive=-1 where w1.TheString = 'E3'", 1, isNamedWindow ? "Idx1" : null, isNamedWindow ? BACKING_SINGLE_UNIQUE : null);
    
            // test consumption
            var stmt = _epService.EPAdministrator.CreateEPL("select irstream * from MyInfra");
            stmt.Events += _listener.Update ;
            _epService.EPRuntime.ExecuteQuery("update MyInfra set IntPrimitive=1000 where TheString = 'E0'");
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new object[] {"E0", 1000}, new object[] {"E0", 0});
            }
    
            // test update via UDF and setter
            if (isNamedWindow) {
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("doubleInt", this.GetType().FullName, "DoubleInt");
                _epService.EPRuntime.ExecuteQuery("delete from MyInfra");
                _epService.EPRuntime.SendEvent(new SupportBean("A", 10));
                _epService.EPRuntime.ExecuteQuery("update MyInfra mw set mw.set_TheString('XYZ'), doubleInt(mw)");
                EPAssertionUtil.AssertPropsPerRow(_epService.EPAdministrator.GetStatement("TheInfra").GetEnumerator(),
                        "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "XYZ", 20 } });
            }
    
            DestroyInfra();
        }
    
        private void RunQueryAssertCountNonNegative(string epl, int count, string indexName, string backingClass)
        {
            _epService.EPRuntime.ExecuteQuery(epl);
            var actual = _epService.EPRuntime.ExecuteQuery("select count(*) as c0 from MyInfra where IntPrimitive >= 0").Array[0].Get("c0").AsLong();
            Assert.AreEqual(count, actual);
            SupportQueryPlanIndexHook.AssertFAFAndReset(indexName, backingClass);
        }
    
        private void RunAssertionParameterizedQuery(bool isNamedWindow)
        {
            SetupInfra(isNamedWindow);
    
            for (var i = 0; i < 10; i++) {
                _epService.EPRuntime.SendEvent(MakeBean("E" + i, i, i*1000));
            }
    
            // test one parameter
            var eplOneParam = "select * from MyInfra where IntPrimitive = ?";
            var pqOneParam = _epService.EPRuntime.PrepareQueryWithParameters(eplOneParam);
            for (var i = 0; i < 10; i++) {
                RunParameterizedQuery(pqOneParam, new object[] {i}, new string[] {"E" + i});
            }
            RunParameterizedQuery(pqOneParam, new object[] {-1}, null); // not found
    
            // test two parameter
            var eplTwoParam = "select * from MyInfra where IntPrimitive = ? and LongPrimitive = ?";
            var pqTwoParam = _epService.EPRuntime.PrepareQueryWithParameters(eplTwoParam);
            for (var i = 0; i < 10; i++) {
                RunParameterizedQuery(pqTwoParam, new object[] {i, (long) i*1000}, new string[] {"E" + i});
            }
            RunParameterizedQuery(pqTwoParam, new object[] {-1, 1000}, null); // not found
    
            // test in-clause with string objects
            var eplInSimple = "select * from MyInfra where TheString in (?, ?, ?)";
            var pqInSimple = _epService.EPRuntime.PrepareQueryWithParameters(eplInSimple);
            RunParameterizedQuery(pqInSimple, new object[] {"A", "A", "A"}, null); // not found
            RunParameterizedQuery(pqInSimple, new object[] {"A", "E3", "A"}, new string[] {"E3"});
    
            // test in-clause with string array
            var eplInArray = "select * from MyInfra where TheString in (?)";
            var pqInArray = _epService.EPRuntime.PrepareQueryWithParameters(eplInArray);
            RunParameterizedQuery(pqInArray, new object[] { new string[] {"E3", "E6", "E8"}}, new string[] {"E3", "E6", "E8"});
    
            // various combinations
            RunParameterizedQuery(_epService.EPRuntime.PrepareQueryWithParameters("select * from MyInfra where TheString in (?) and LongPrimitive = 4000"),
                    new object[] { new string[] {"E3", "E4", "E8"}}, new string[] {"E4"});
            RunParameterizedQuery(_epService.EPRuntime.PrepareQueryWithParameters("select * from MyInfra where LongPrimitive > 8000"),
                    new object[] {}, new string[] {"E9"});
            RunParameterizedQuery(_epService.EPRuntime.PrepareQueryWithParameters("select * from MyInfra where LongPrimitive < ?"),
                    new object[] { 2000}, new string[] {"E0", "E1"});
            RunParameterizedQuery(_epService.EPRuntime.PrepareQueryWithParameters("select * from MyInfra where LongPrimitive between ? and ?"),
                    new object[] { 2000, 4000}, new string[] {"E2", "E3", "E4"});
    
            DestroyInfra();
        }
    
        private void RunParameterizedQuery(EPOnDemandPreparedQueryParameterized parameterizedQuery, object[] parameters, string[] expected)
        {
            for (var i = 0; i < parameters.Length; i++) {
                parameterizedQuery.SetObject(i+1, parameters[i]);
            }
            var result = _epService.EPRuntime.ExecuteQuery(parameterizedQuery);
            if (expected == null) {
                Assert.AreEqual(0, result.Array.Length);
            }
            else {
                Assert.AreEqual(expected.Length, result.Array.Length);
                var resultStrings = new string[result.Array.Length];
                for (var i = 0; i < resultStrings.Length; i++) {
                    resultStrings[i] = (string) result.Array[i].Get("TheString");
                }
                EPAssertionUtil.AssertEqualsAnyOrder(expected, resultStrings);
            }
        }
    
        public void RunAssertionInsert(bool isNamedWindow)
        {
            SetupInfra(isNamedWindow);
    
            var stmt = _epService.EPAdministrator.GetStatement("TheInfra");
            var propertyNames = "TheString,IntPrimitive".Split(',');
    
            // try column name provided with insert-into
            var eplSelect = "insert into MyInfra (TheString, IntPrimitive) select 'a', 1";
            var resultOne = _epService.EPRuntime.ExecuteQuery(eplSelect);
            AssertFAFInsertResult(resultOne, new object[]{"a", 1}, propertyNames, stmt, isNamedWindow);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), propertyNames, new object[][] { new object[] { "a", 1 } });
    
            // try SODA and column name not provided with insert-into
            var eplTwo = "insert into MyInfra select \"b\" as TheString, 2 as IntPrimitive";
            var modelWSelect = _epService.EPAdministrator.CompileEPL(eplTwo);
            Assert.AreEqual(eplTwo, modelWSelect.ToEPL());
            var resultTwo = _epService.EPRuntime.ExecuteQuery(modelWSelect);
            AssertFAFInsertResult(resultTwo, new object[]{"b", 2}, propertyNames, stmt, isNamedWindow);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), propertyNames, new object[][] { new object[] { "a", 1 }, new object[] { "b", 2 } });
    
            // create unique index, insert duplicate row
            _epService.EPAdministrator.CreateEPL("create unique index I1 on MyInfra (TheString)");
            try {
                var eplThree = "insert into MyInfra (TheString) select 'a' as TheString";
                _epService.EPRuntime.ExecuteQuery(eplThree);
            }
            catch (EPException ex) {
                Assert.AreEqual("Error executing statement: Unique index violation, index 'I1' is a unique index and key 'a' already exists [insert into MyInfra (TheString) select 'a' as TheString]", ex.Message);
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), propertyNames, new object[][] { new object[] { "a", 1 }, new object[] { "b", 2 } });
    
            // try second no-column-provided version
            var eplMyInfraThree = isNamedWindow ?
                    "create window MyInfraThree.win:keepall() as (p0 string, p1 int)" :
                    "create table MyInfraThree as (p0 string, p1 int)";
            var stmtMyInfraThree = _epService.EPAdministrator.CreateEPL(eplMyInfraThree);
            _epService.EPRuntime.ExecuteQuery("insert into MyInfraThree select 'a' as p0, 1 as p1");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtMyInfraThree.GetEnumerator(), "p0,p1".Split(','), new object[][] { new object[] { "a", 1 } });
    
            // try enum-value insert
            _epService.EPAdministrator.CreateEPL("create schema MyMode (mode " + typeof(SupportEnum).FullName + ")");
            var eplInfraMode = isNamedWindow ?
                    "create window MyInfraTwo.std:unique(mode) as MyMode" :
                    "create table MyInfraTwo as (mode " + typeof(SupportEnum).FullName + ")";
            var stmtEnumWin = _epService.EPAdministrator.CreateEPL(eplInfraMode);
            _epService.EPRuntime.ExecuteQuery("insert into MyInfraTwo select " + typeof(SupportEnum).FullName + "." + SupportEnum.ENUM_VALUE_2.GetName() + " as mode");
            EPAssertionUtil.AssertProps(stmtEnumWin.First(), "mode".Split(','), new object[] {SupportEnum.ENUM_VALUE_2});
    
            // try insert-into with values-keyword and explicit column names
            _epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            var eplValuesKW = "insert into MyInfra(TheString, IntPrimitive) values (\"a\", 1)";
            var resultValuesKW = _epService.EPRuntime.ExecuteQuery(eplValuesKW);
            AssertFAFInsertResult(resultValuesKW, new object[]{"a", 1}, propertyNames, stmt, isNamedWindow);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), propertyNames, new object[][] { new object[] { "a", 1 } });
    
            // try insert-into with values model
            _epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            var modelWValuesKW = _epService.EPAdministrator.CompileEPL(eplValuesKW);
            Assert.AreEqual(eplValuesKW, modelWValuesKW.ToEPL());
            _epService.EPRuntime.ExecuteQuery(modelWValuesKW);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), propertyNames, new object[][] { new object[] { "a", 1 } });
    
            // try insert-into with values-keyword and as-names
            _epService.EPRuntime.ExecuteQuery("delete from MyInfraThree");
            var eplValuesWithoutCols = "insert into MyInfraThree values ('b', 2)";
            _epService.EPRuntime.ExecuteQuery(eplValuesWithoutCols);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtMyInfraThree.GetEnumerator(), "p0,p1".Split(','), new object[][] { new object[] { "b", 2 } });
    
            DestroyInfra();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfraTwo", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfraThree", false);
        }
    
        private void AssertFAFInsertResult(EPOnDemandQueryResult resultOne, object[] objects, string[] propertyNames, EPStatement stmt, bool isNamedWindow)
        {
            Assert.AreSame(resultOne.EventType, stmt.EventType);
            if (isNamedWindow) {
                Assert.AreEqual(1, resultOne.Array.Length);
                EPAssertionUtil.AssertPropsPerRow(resultOne.Array, propertyNames, new object[][] {objects});
            }
            else {
                Assert.AreEqual(0, resultOne.Array.Length);
            }
        }
    
        private void SetupInfra(bool isNamedWindow)
        {
            var eplCreate = isNamedWindow ?
                    "@Name('TheInfra') create window MyInfra.win:keepall() as select * from SupportBean" :
                    "@Name('TheInfra') create table MyInfra as (TheString string primary key, IntPrimitive int primary key, LongPrimitive long)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            var eplInsert = isNamedWindow ?
                    "@Name('Insert') insert into MyInfra select * from SupportBean" :
                    "@Name('Insert') on SupportBean sb merge MyInfra mi where mi.TheString = sb.TheString and mi.IntPrimitive=sb.IntPrimitive" +
                            " when not matched then insert select TheString, IntPrimitive, LongPrimitive";
            _epService.EPAdministrator.CreateEPL(eplInsert);
        }
    
        private void DestroyInfra()
        {
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private SupportBean MakeBean(string theString, int intPrimitive, long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        public static void DoubleInt(SupportBean bean)
        {
            bean.IntPrimitive = bean.IntPrimitive * 2;
        }
    
        private void AssertCtxInfraCountPerCode(long[] expectedCountPerCode)
        {
            for (var i = 0; i < expectedCountPerCode.Length; i++) {
                Assert.AreEqual(expectedCountPerCode[i], GetCtxInfraCount(i), "for code " + i);
            }
        }
    
        private void SendEvent(EventRepresentationEnum eventRepresentationEnum, EPServiceProvider epService, string eventName, string[] attributes)
        {
            var eventMap = new Dictionary<string, object>();
            var eventObjectArray = new List<object>();
            foreach (var attribute in attributes) {
                var key = attribute.Split('=')[0];
                var value = attribute.Split('=')[1];
                eventMap.Put(key, value);
                eventObjectArray.Add(value);
            }
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(eventObjectArray.ToArray(), eventName);
            }
            else {
                epService.EPRuntime.SendEvent(eventMap, eventName);
            }
        }
    
        private long GetCtxInfraCount(int hashCode)
        {
            var result = _epService.EPRuntime.ExecuteQuery("select count(*) as c0 from CtxInfra", new ContextPartitionSelector[] {new SupportSelectorByHashCode(hashCode)});
            return result.Array[0].Get("c0").AsLong();
        }
    
        private long GetCtxInfraCatCount(string categoryName)
        {
            var result = _epService.EPRuntime.ExecuteQuery("select count(*) as c0 from CtxInfraCat", new ContextPartitionSelector[] {new SupportSelectorCategory(categoryName)});
            return result.Array[0].Get("c0").AsLong();
        }
    }
}
