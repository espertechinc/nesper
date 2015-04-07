///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableUpdateAndIndex  {
    
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp() {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestEarlyUniqueIndexViolation() {
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create table MyTable as (pkey0 string primary key, pkey1 int primary key, thecnt count(*))");
    
            epService.EPAdministrator.CreateEPL("into table MyTable select count(*) as thecnt from SupportBean group by TheString, IntPrimitive");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
    
            // invalid index being created
            SupportMessageAssertUtil.TryInvalid(epService, "create unique index SecIndex on MyTable(pkey0)",
                    "Unexpected exception starting statement: Unique index violation, index 'SecIndex' is a unique index and key 'E1' already exists [create unique index SecIndex on MyTable(pkey0)]");
    
            // try fire-and-forget update of primary key to non-unique value
            try {
                epService.EPRuntime.ExecuteQuery("update MyTable set pkey1 = 0");
                Assert.Fail();
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error executing statement: Unique index violation, index 'primary-MyTable' is a unique index and key 'MultiKeyUntyped[E1, 0]' already exists [");
                // assert events are unchanged - no update actually performed
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey0,pkey1".Split(','), new object[][] { new object[] { "E1", 10 }, new object[] { "E1", 20 } });
            }
    
            // try on-update unique index violation
            epService.EPAdministrator.CreateEPL("@Name('on-update') on SupportBean_S1 update MyTable set pkey1 = 0");
            try {
                epService.EPRuntime.SendEvent(new SupportBean_S1(0));
                Assert.Fail();
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex.InnerException, "Unexpected exception in statement 'on-update': Unique index violation, index 'primary-MyTable' is a unique index and key 'MultiKeyUntyped[E1, 0]' already exists");
                // assert events are unchanged - no update actually performed
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey0,pkey1".Split(','), new object[][] { new object[] { "E1", 10 }, new object[] { "E1", 20 } });
            }
    
            // disallow on-merge unique key updates
            try {
                epService.EPAdministrator.CreateEPL("@Name('on-merge') on SupportBean_S1 merge MyTable when matched then update set pkey1 = 0");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex.InnerException, "Validation failed in when-matched (clause 1): On-merge statements may not update unique keys of tables");
            }
        }
    
        [Test]
        public void TestLateUniqueIndexViolation() {
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create table MyTable as (" +
                    "pkey0 string primary key, " +
                    "pkey1 int primary key, " +
                    "col0 int, " +
                    "thecnt count(*))");
    
            epService.EPAdministrator.CreateEPL("into table MyTable select count(*) as thecnt from SupportBean group by TheString, IntPrimitive");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
    
            // On-merge exists before creating a unique index
            EPStatement onMerge = epService.EPAdministrator.CreateEPL("@Name('on-merge') on SupportBean_S1 merge MyTable " +
                    "when matched then update set col0 = 0");
            try {
                epService.EPAdministrator.CreateEPL("create unique index MyUniqueSecondary on MyTable (col0)");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate statement 'on-merge' as a recipient of the proposed index: On-merge statements may not update unique keys of tables [");
            }
            onMerge.Dispose();
    
            // on-update exists before creating a unique index
            EPStatement stmtUpdate = epService.EPAdministrator.CreateEPL("@Name('on-update') on SupportBean_S1 update MyTable set pkey1 = 0");
            epService.EPAdministrator.CreateEPL("create unique index MyUniqueSecondary on MyTable (pkey1)");
            try {
                epService.EPRuntime.SendEvent(new SupportBean_S1(0));
                Assert.Fail();
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex.InnerException, "Unexpected exception in statement 'on-update': Unique index violation, index 'MyUniqueSecondary' is a unique index and key '0' already exists");
                // assert events are unchanged - no update actually performed
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey0,pkey1".Split(','), new object[][] { new object[] { "E1", 10 }, new object[] { "E2", 20 } });
            }
    
            // unregister
            stmtUpdate.Dispose();
        }
    
        [Test]
        public void TestFAFUpdate() {
            epService.EPAdministrator.CreateEPL("create table MyTable as (pkey0 string primary key, col0 int, col1 int, thecnt count(*))");
            epService.EPAdministrator.CreateEPL("create index MyIndex on MyTable(col0)");
    
            epService.EPAdministrator.CreateEPL("into table MyTable select count(*) as thecnt from SupportBean group by TheString");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
    
            epService.EPRuntime.ExecuteQuery("update MyTable set col0 = 1 where pkey0='E1'");
            epService.EPRuntime.ExecuteQuery("update MyTable set col0 = 2 where pkey0='E2'");
            AssertFAFOneRowResult("select pkey0 from MyTable where col0=1", "pkey0", new object[]{"E1"});
    
            epService.EPRuntime.ExecuteQuery("update MyTable set col1 = 100 where pkey0='E1'");
            AssertFAFOneRowResult("select pkey0 from MyTable where col1=100", "pkey0", new object[]{"E1"});
        }
    
        private void AssertFAFOneRowResult(string epl, string fields, object[] objects) {
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery(epl);
            Assert.AreEqual(1, result.Array.Length);
            EPAssertionUtil.AssertProps(result.Array[0], fields.Split(','), objects);
        }
    }
}
