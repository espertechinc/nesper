///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableUpdateAndIndex : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionEarlyUniqueIndexViolation(epService);
            RunAssertionLateUniqueIndexViolation(epService);
            RunAssertionFAFUpdate(epService);
        }
    
        private void RunAssertionEarlyUniqueIndexViolation(EPServiceProvider epService) {
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create table MyTableEUIV as (pkey0 string primary key, pkey1 int primary key, thecnt count(*))");
    
            epService.EPAdministrator.CreateEPL("into table MyTableEUIV select count(*) as thecnt from SupportBean group by TheString, IntPrimitive");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
    
            // invalid index being created
            SupportMessageAssertUtil.TryInvalid(epService, "create unique index SecIndex on MyTableEUIV(pkey0)",
                    "Unexpected exception starting statement: Unique index violation, index 'SecIndex' is a unique index and key 'E1' already exists [create unique index SecIndex on MyTableEUIV(pkey0)]");
    
            // try fire-and-forget update of primary key to non-unique value
            try {
                epService.EPRuntime.ExecuteQuery("update MyTableEUIV set pkey1 = 0");
                Assert.Fail();
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error executing statement: Unique index violation, index 'primary-MyTableEUIV' is a unique index and key 'MultiKeyUntyped[E1, 0]' already exists [");
                // assert events are unchanged - no update actually performed
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey0,pkey1".Split(','), new object[][]{new object[] {"E1", 10}, new object[] {"E1", 20}});
            }
    
            // try on-update unique index violation
            epService.EPAdministrator.CreateEPL("@Name('on-update') on SupportBean_S1 update MyTableEUIV set pkey1 = 0");
            try {
                epService.EPRuntime.SendEvent(new SupportBean_S1(0));
                Assert.Fail();
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex.InnerException, "Unexpected exception in statement 'on-update': Unique index violation, index 'primary-MyTableEUIV' is a unique index and key 'MultiKeyUntyped[E1, 0]' already exists");
                // assert events are unchanged - no update actually performed
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey0,pkey1".Split(','), new object[][]{new object[] {"E1", 10}, new object[] {"E1", 20}});
            }
    
            // disallow on-merge unique key updates
            try {
                epService.EPAdministrator.CreateEPL("@Name('on-merge') on SupportBean_S1 merge MyTableEUIV when matched then update set pkey1 = 0");
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex.InnerException, "Validation failed in when-matched (clause 1): On-merge statements may not update unique keys of tables");
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLateUniqueIndexViolation(EPServiceProvider epService) {
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create table MyTableLUIV as (" +
                    "pkey0 string primary key, " +
                    "pkey1 int primary key, " +
                    "col0 int, " +
                    "thecnt count(*))");
    
            epService.EPAdministrator.CreateEPL("into table MyTableLUIV select count(*) as thecnt from SupportBean group by TheString, IntPrimitive");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
    
            // On-merge Exists before creating a unique index
            EPStatement onMerge = epService.EPAdministrator.CreateEPL("@Name('on-merge') on SupportBean_S1 merge MyTableLUIV " +
                    "when matched then update set col0 = 0");
            try {
                epService.EPAdministrator.CreateEPL("create unique index MyUniqueSecondary on MyTableLUIV (col0)");
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate statement 'on-merge' as a recipient of the proposed index: On-merge statements may not update unique keys of tables [");
            }
            onMerge.Dispose();
    
            // on-update Exists before creating a unique index
            EPStatement stmtUpdate = epService.EPAdministrator.CreateEPL("@Name('on-update') on SupportBean_S1 update MyTableLUIV set pkey1 = 0");
            epService.EPAdministrator.CreateEPL("create unique index MyUniqueSecondary on MyTableLUIV (pkey1)");
            try {
                epService.EPRuntime.SendEvent(new SupportBean_S1(0));
                Assert.Fail();
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex.InnerException, "Unexpected exception in statement 'on-update': Unique index violation, index 'MyUniqueSecondary' is a unique index and key '0' already exists");
                // assert events are unchanged - no update actually performed
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey0,pkey1".Split(','), new object[][]{new object[] {"E1", 10}, new object[] {"E2", 20}});
            }
    
            // unregister
            stmtUpdate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFAFUpdate(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create table MyTableFAFU as (pkey0 string primary key, col0 int, col1 int, thecnt count(*))");
            epService.EPAdministrator.CreateEPL("create index MyIndex on MyTableFAFU(col0)");
    
            epService.EPAdministrator.CreateEPL("into table MyTableFAFU select count(*) as thecnt from SupportBean group by TheString");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
    
            epService.EPRuntime.ExecuteQuery("update MyTableFAFU set col0 = 1 where pkey0='E1'");
            epService.EPRuntime.ExecuteQuery("update MyTableFAFU set col0 = 2 where pkey0='E2'");
            AssertFAFOneRowResult(epService, "select pkey0 from MyTableFAFU where col0=1", "pkey0", new object[]{"E1"});
    
            epService.EPRuntime.ExecuteQuery("update MyTableFAFU set col1 = 100 where pkey0='E1'");
            AssertFAFOneRowResult(epService, "select pkey0 from MyTableFAFU where col1=100", "pkey0", new object[]{"E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertFAFOneRowResult(EPServiceProvider epService, string epl, string fields, object[] objects) {
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery(epl);
            Assert.AreEqual(1, result.Array.Length);
            EPAssertionUtil.AssertProps(result.Array[0], fields.Split(','), objects);
        }
    }
} // end of namespace
