///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.context;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.ContextStateCacheHook;

namespace com.espertech.esper.regression.context
{
    public class ExecContextAdminPartitionSPI : RegressionExecution
    {
        private static readonly SupportHashCodeFuncGranularCRC32 CODE_FUNC_MOD64 =
            new SupportHashCodeFuncGranularCRC32(64);
        private static readonly string[] FIELDS = "c0,c1".Split(',');
        private static readonly string[] FIELDSCP = "c0,c1,c2".Split(',');
        private static readonly int HASH_MOD_E1_STRING_BY_64 = 5;

        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionDestroyCtxPartitions(epService);
            RunAssertionInvalid(epService);
            RunAssertionStopStartNestedCtxPartitions(epService);
            RunAssertionGetContextStatementNames(epService);
            RunAssertionAcrossURIExtractImport(epService);
            RunAssertionSameURIExtractStopImportStart(epService);
        }
    
        private void RunAssertionDestroyCtxPartitions(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecContextAdminPartitionSPI))) {
                return;
            }
    
            AssertExtractDestroyPartitionedById(epService);
            AssertDestroyCategory(epService);
            AssertDestroyHashSegmented(epService);
            AssertDestroyPartitioned(epService);
            AssertDestroyInitTerm(epService);
            AssertDestroyNested(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecContextAdminPartitionSPI))) {
                return;
            }
    
            // context not found
            try {
                GetSpi(epService).GetContextNestingLevel("undefined");
                Assert.Fail();
            } catch (ArgumentException ex) {
                Assert.AreEqual("Context by name 'undefined' could not be found", ex.Message);
            }
    
            // invalid selector for context
            epService.EPAdministrator.CreateEPL("create context SomeContext partition by TheString from SupportBean");
            try {
                GetSpi(epService).DestroyContextPartitions("SomeContext", new SupportSelectorCategory("abc"));
                Assert.Fail();
            } catch (InvalidContextPartitionSelector) {
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStopStartNestedCtxPartitions(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecContextAdminPartitionSPI))) {
                return;
            }
    
            string contextName = "CategoryContext";
            string createCtx = CONTEXT_CACHE_HOOK + "create context CategoryContext as " +
                    "group by IntPrimitive < 0 as negative, group by IntPrimitive > 0 as positive from SupportBean";
            epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context CategoryContext " +
                    "select TheString as c0, sum(IntPrimitive) as c1, context.id as c2 from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", -5));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 20));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E1", 10, 1}, new object[] {"E2", -5, 0}, new object[] {"E3", 30, 1}});
    
            // stop category "negative"
            SupportContextStateCacheImpl.Reset();
            ContextPartitionCollection collStop = GetSpi(epService).StopContextPartitions("CategoryContext", new SupportSelectorCategory("negative"));
            AssertPathInfo(collStop.Descriptors, new[] {new object[] {0, MakeIdentCat("negative"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[] {new object[] {0, MakeIdentCat("negative"), "-"}, new object[] {1, MakeIdentCat("positive"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, false));
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", -6));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 30));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E5", 60, 1}});
    
            // start category "negative"
            ContextPartitionCollection collStart = GetSpi(epService).StartContextPartitions("CategoryContext", new SupportSelectorCategory("negative"));
            AssertPathInfo(collStart.Descriptors, new[] {new object[] {0, MakeIdentCat("negative"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[] {new object[] {0, MakeIdentCat("negative"), "+"}, new object[] {1, MakeIdentCat("positive"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", -7));
            epService.EPRuntime.SendEvent(new SupportBean("E7", 40));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E6", -7, 0}, new object[] {"E7", 100, 1}});
    
            // stop category "positive"
            SupportContextStateCacheImpl.Reset();
            GetSpi(epService).StopContextPartition("CategoryContext", 1);
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[] {new object[] {0, MakeIdentCat("negative"), "+"}, new object[] {1, MakeIdentCat("positive"), "-"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 2, 1, 1, false));
    
            epService.EPRuntime.SendEvent(new SupportBean("E8", -8));
            epService.EPRuntime.SendEvent(new SupportBean("E9", 50));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E8", -15, 0}});
    
            // start category "positive"
            GetSpi(epService).StartContextPartition("CategoryContext", 1);
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[] {new object[] {0, MakeIdentCat("negative"), "+"}, new object[] {1, MakeIdentCat("positive"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 2, 1, 1, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("E10", -9));
            epService.EPRuntime.SendEvent(new SupportBean("E11", 60));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E10", -24, 0}, new object[] {"E11", 60, 1}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionGetContextStatementNames(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecContextAdminPartitionSPI))) {
                return;
            }
    
            epService.EPAdministrator.CreateEPL("create context CtxA partition by TheString from SupportBean");
            EPStatement stmtA = epService.EPAdministrator.CreateEPL("@Name('A') context CtxA select count(*) from SupportBean");
            EPStatement stmtB = epService.EPAdministrator.CreateEPL("@Name('B') context CtxA select sum(IntPrimitive) from SupportBean");
    
            EPAssertionUtil.AssertEqualsAnyOrder(GetSpi(epService).GetContextStatementNames("CtxA"), "A,B".Split(','));
    
            stmtA.Dispose();
            EPAssertionUtil.AssertEqualsAnyOrder(GetSpi(epService).GetContextStatementNames("CtxA"), "B".Split(','));
    
            stmtB.Dispose();
            EPAssertionUtil.AssertEqualsAnyOrder(GetSpi(epService).GetContextStatementNames("CtxA"), new string[0]);
    
            Assert.AreEqual(null, GetSpi(epService).GetContextStatementNames("undefined"));
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAcrossURIExtractImport(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecContextAdminPartitionSPI))) {
                return;
            }
    
            AssertHashSegmentedImport(epService);
            AssertPartitionedImport(epService);
            AssertCategoryImport(epService);
            AssertInitTermImport(epService);
            AssertNestedContextImport(epService);
        }
    
        private void RunAssertionSameURIExtractStopImportStart(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecContextAdminPartitionSPI))) {
                return;
            }
    
            AssertHashSegmentedIndividualSelector(epService, new MySelectorHashById(Collections.SingletonSet(HASH_MOD_E1_STRING_BY_64)));
            AssertHashSegmentedIndividualSelector(epService, new MySelectorHashFiltered(HASH_MOD_E1_STRING_BY_64));
            AssertHashSegmentedIndividualSelector(epService, new SupportSelectorById(Collections.SingletonSet(0)));
            AssertHashSegmentedAllSelector(epService);
    
            AssertCategoryIndividualSelector(epService, new SupportSelectorCategory(Collections.SingletonSet("G2")));
            AssertCategoryIndividualSelector(epService, new MySelectorCategoryFiltered("G2"));
            AssertCategoryIndividualSelector(epService, new SupportSelectorById(Collections.SingletonSet(1)));
            AssertCategoryAllSelector(epService);
    
            AssertPartitionedIndividualSelector(epService, new SupportSelectorById(Collections.SingletonSet(0)));
            AssertPartitionedIndividualSelector(epService, new SupportSelectorPartitioned(Collections.SingletonList(new object[]{"E1"})));
            AssertPartitionedIndividualSelector(epService, new MySelectorPartitionFiltered(new object[]{"E1"}));
            AssertPartitionedAllSelector(epService);
    
            AssertInitTermIndividualSelector(epService, new MySelectorInitTermFiltered("E1"));
            AssertInitTermIndividualSelector(epService, new SupportSelectorById(Collections.SingletonSet(0)));
            AssertInitTermAllSelector(epService);
    
            AssertNestedContextIndividualSelector(epService, new SupportSelectorNested(
                    new MySelectorPartitionFiltered(new object[]{"E2"}), new MySelectorCategoryFiltered("positive")));
        }
    
        private void AssertDestroyNested(EPServiceProvider epService) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextNested(epService, listener);
            Assert.AreEqual(2, epService.EPAdministrator.ContextPartitionAdmin.GetContextNestingLevel(contextName));
            string[] fieldsnested = "c0,c1,c2,c3".Split(',');
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 11));
            epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 12));
            epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 13));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, fieldsnested,
                    new[] {new object[] {"E1", 1, 10L, 1}, new object[] {"E2", 1, 11L, 3}, new object[] {"E1", -1, 12L, 0}, new object[] {"E2", -1, 13L, 2}});
    
            // destroy hash for "S0_2"
            ContextPartitionCollection collDestroy = GetSpi(epService).DestroyContextPartitions(contextName, new SupportSelectorNested(new SupportSelectorPartitioned("E2"), new SupportSelectorCategory("negative")));
            AssertPathInfo(collDestroy.Descriptors, new[] {new object[] {2, MakeIdentNested(MakeIdentPart("E2"), MakeIdentCat("negative")), "+"}});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, true), new[]
            {
                new object[]{0, MakeIdentNested(MakeIdentPart("E1"), MakeIdentCat("negative")), "+"},
                new object[]{1, MakeIdentNested(MakeIdentPart("E1"), MakeIdentCat("positive")), "+"},
                new object[]{3, MakeIdentNested(MakeIdentPart("E2"), MakeIdentCat("positive")), "+"}
            });
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(2, 2, 1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 20));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 21));
            epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 22));
            epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 23));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, fieldsnested,
                    new[] {new object[] {"E1", 1, 30L, 1}, new object[] {"E2", 1, 32L, 3}, new object[] {"E1", -1, 34L, 0}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertDestroyInitTerm(EPServiceProvider epService) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextInitTerm(epService, listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S0_3"));
            epService.EPRuntime.SendEvent(new SupportBean("S0_1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("S0_2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("S0_3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"S0_1", 1, 0}, new object[] {"S0_2", 2, 1}, new object[] {"S0_3", 3, 2}});
    
            // destroy hash for "S0_2"
            ContextPartitionCollection collDestroy = GetSpi(epService).DestroyContextPartitions(contextName, new SupportSelectorById(1));
            AssertPathInfo(collDestroy.Descriptors, new[] {new object[] {1, null, "+"}});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[] {new object[] {0, null, "+"}, new object[] {2, null, "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, true), new ContextState(1, 0, 3, 2, null, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 2));
    
            epService.EPRuntime.SendEvent(new SupportBean("S0_1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("S0_2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("S0_3", 30));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"S0_1", 11, 0}, new object[] {"S0_3", 33, 2}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertDestroyHashSegmented(EPServiceProvider epService) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextHashSegmented(epService, listener);
            int hashCodeE1 = CODE_FUNC_MOD64.CodeFor("E1");
            int hashCodeE2 = CODE_FUNC_MOD64.CodeFor("E2");
            int hashCodeE3 = CODE_FUNC_MOD64.CodeFor("E3");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E1", 1, 0}, new object[] {"E2", 2, 1}, new object[] {"E3", 3, 2}});
    
            // destroy hash for "E2"
            ContextPartitionCollection collDestroy = GetSpi(epService).DestroyContextPartitions(contextName, new SupportSelectorByHashCode(hashCodeE2));
            AssertPathInfo(collDestroy.Descriptors, new[] {new object[] {1, MakeIdentHash(hashCodeE2), "+"}});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[] {new object[] {0, MakeIdentHash(hashCodeE1), "+"}, new object[] {2, MakeIdentHash(hashCodeE3), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true), new ContextState(1, 0, 3, 2, hashCodeE3, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 2));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E1", 11, 0}, new object[] {"E2", 20, 3}, new object[] {"E3", 33, 2}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertDestroyPartitioned(EPServiceProvider epService) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextPartitioned(epService, listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E1", 1, 0}, new object[] {"E2", 2, 1}, new object[] {"E3", 3, 2}});
    
            // destroy hash for "E2"
            ContextPartitionCollection collDestroy = GetSpi(epService).DestroyContextPartitions(contextName, new SupportSelectorPartitioned("E2"));
            AssertPathInfo(collDestroy.Descriptors, new[] {new object[] {1, MakeIdentPart("E2"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[] {new object[] {0, MakeIdentPart("E1"), "+"}, new object[] {2, MakeIdentPart("E3"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new object[]{"E1"}, true), new ContextState(1, 0, 3, 2, new object[]{"E3"}, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 2));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E1", 11, 0}, new object[] {"E2", 20, 3}, new object[] {"E3", 33, 2}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertExtractDestroyPartitionedById(EPServiceProvider epService) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextPartitioned(epService, listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E1", 1, 0}, new object[] {"E2", 2, 1}, new object[] {"E3", 3, 2}});
    
            // destroy hash for "E2"
            ContextPartitionDescriptor destroyedOne = GetSpi(epService).DestroyContextPartition(contextName, 1);
            AssertPathInfo("destroyed", destroyedOne, new object[]{1, MakeIdentPart("E2"), "+"});
    
            // destroy hash for "E3"
            EPContextPartitionExtract collDestroy = GetSpi(epService).ExtractDestroyPaths(contextName, new SupportSelectorById(0));
            AssertPathInfo(collDestroy.Collection.Descriptors, new[] {new object[] {0, MakeIdentPart("E1"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[] {new object[] {2, MakeIdentPart("E3"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 3, 2, new object[]{"E3"}, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 1), new ContextStatePathKey(1, 0, 2));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E1", 10, 3}, new object[] {"E2", 20, 4}, new object[] {"E3", 33, 2}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertDestroyCategory(EPServiceProvider epService) {
            SupportContextStateCacheImpl.Reset();
            string contextName = "CategoryContext";
            string createCtx = CONTEXT_CACHE_HOOK + "create context CategoryContext as " +
                    "group by IntPrimitive < 0 as negative, " +
                    "group by IntPrimitive = 0 as zero," +
                    "group by IntPrimitive > 0 as positive from SupportBean";
            epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context CategoryContext " +
                    "select TheString as c0, count(*) as c1, context.id as c2 from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E1", 1L, 0}, new object[] {"E2", 1L, 1}, new object[] {"E3", 1L, 2}});
    
            // destroy category "negative"
            ContextPartitionCollection collDestroy = GetSpi(epService).DestroyContextPartitions(contextName, new SupportSelectorCategory("zero"));
            AssertPathInfo(collDestroy.Descriptors, new[] {new object[] {1, MakeIdentCat("zero"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[] {new object[] {0, MakeIdentCat("negative"), "+"}, new object[] {2, MakeIdentCat("positive"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 3, 2, 2, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 2));
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", -1));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP, new[] {new object[] {"E4", 2L, 0}, new object[] {"E6", 2L, 2}});
    
            // destroy again, should return empty
            ContextPartitionCollection collDestroyTwo = GetSpi(epService).DestroyContextPartitions(contextName, new SupportSelectorCategory("zero"));
            Assert.IsTrue(collDestroyTwo.Descriptors.IsEmpty());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertCategoryAllSelector(EPServiceProvider epService) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextCategory(epService, listener);
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, true), new ContextState(1, 0, 3, 2, 2, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDS, new[] {new object[] {"G1", 1}, new object[] {"G2", 10}, new object[] {"G3", 100}});
    
            // deactivate all categories
            EPContextPartitionExtract extract = GetSpi(epService).ExtractStopPaths(contextName, ContextPartitionSelectorAll.INSTANCE);
            AssertPathInfo(extract.Collection.Descriptors, new[]
            {
                new object[]{0, MakeIdentCat("G1"), "+"},
                new object[]{1, MakeIdentCat("G2"), "+"},
                new object[]{2, MakeIdentCat("G3"), "+"}
            });
            Assert.AreEqual(1, extract.NumNestingLevels);
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, false), new ContextState(1, 0, 2, 1, 1, false), new ContextState(1, 0, 3, 2, 2, false));
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 12));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 102));
            Assert.IsFalse(listener.IsInvoked);
            AssertCreateStmtNotActive(epService, "context CategoryContext select * from SupportBean", new SupportBean("G1", -1));
            AssertCreateStmtNotActive(epService, "context CategoryContext select * from SupportBean", new SupportBean("G2", -1));
            AssertCreateStmtNotActive(epService, "context CategoryContext select * from SupportBean", new SupportBean("G3", -1));
    
            // activate categories
            GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, true), new ContextState(1, 0, 3, 2, 2, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 13));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 103));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDS, new[] {new object[] {"G1", 3}, new object[] {"G2", 13}, new object[] {"G3", 103}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertCategoryIndividualSelector(EPServiceProvider epService, ContextPartitionSelector selectorCategoryG2) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextCategory(epService, listener);
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, true), new ContextState(1, 0, 3, 2, 2, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDS, new[] {new object[] {"G1", 1}, new object[] {"G2", 10}, new object[] {"G3", 100}});
    
            // deactivate category G2
            EPContextPartitionExtract extract = GetSpi(epService).ExtractStopPaths(contextName, selectorCategoryG2);
            AssertPathInfo(extract.Collection.Descriptors, new[] {new object[] {1, MakeIdentCat("G2"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, false), new ContextState(1, 0, 3, 2, 2, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 12));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 102));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDS, new[] {new object[] {"G1", 3}, new object[] {"G3", 202}});
            AssertCreateStmtNotActive(epService, "context CategoryContext select * from SupportBean", new SupportBean("G2", -1));
    
            // activate category G2
            GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, true), new ContextState(1, 0, 3, 2, 2, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 13));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 103));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDS, new[] {new object[] {"G1", 6}, new object[] {"G2", 13}, new object[] {"G3", 305}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertHashSegmentedImport(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextHashSegmented(epService, listener);
    
            // context partition 0 = code for E2
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"E2", 20, 0});
    
            // context partition 1 = code for E1
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"E1", 10, 1});
    
            EPContextPartitionExtract extract = GetSpi(epService).ExtractPaths(contextName, new ContextPartitionSelectorAll());
    
            epService.EPAdministrator.DestroyAllStatements();
            SetUpContextHashSegmented(epService, listener);
    
            // context partition 0 = code for E3
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"E3", 30, 0});
    
            // context partition 1 = code for E4
            epService.EPRuntime.SendEvent(new SupportBean("E4", 40));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"E4", 40, 1});
    
            // context partition 2 = code for E1
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"E1", 11, 2});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[]
            {
                new object[]{0, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E3")), "+"},
                new object[]{1, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E4")), "+"},
                new object[]{2, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E1")), "+"}});
    
            EPContextPartitionImportResult importResult = GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new[] { new[] { 2, 1}}); // mapping 1 --> 2  (agent instance id 1 to 2)
            AssertImportsCPids(importResult.AllocatedToImported, new[] { new[] { 3, 0}}); // mapping 0 --> 3 (agent instance id 0 to 3)
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[]
            {
                new object[]{0, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E3")), "+"},
                new object[]{1, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E4")), "+"},
                new object[]{2, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E1")), "+"},
                new object[]{3, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E2")), "+"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 41));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));  // was reset
            epService.EPRuntime.SendEvent(new SupportBean("E2", 22));  // was created
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP,
                    new[] {new object[] {"E3", 61, 0}, new object[] {"E4", 81, 1}, new object[] {"E1", 12, 2}, new object[] {"E2", 22, 3}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertPartitionedImport(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextPartitioned(epService, listener);
            Assert.AreEqual(1, epService.EPAdministrator.ContextPartitionAdmin.GetContextNestingLevel(contextName));
    
            // context partition 0 = E2
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"E2", 20, 0});
    
            // context partition 1 = E1
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"E1", 10, 1});
    
            EPContextPartitionExtract extract = GetSpi(epService).ExtractPaths(contextName, new ContextPartitionSelectorAll());
            AssertPathInfo(extract.Collection.Descriptors, new[] {new object[] {0, MakeIdentPart("E2"), "+"}, new object[] {1, MakeIdentPart("E1"), "+"}});
    
            epService.EPAdministrator.DestroyAllStatements();
            SetUpContextPartitioned(epService, listener);
    
            // context partition 0 = E1
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"E1", 11, 0});
    
            // context partition 1 = E3
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"E3", 30, 1});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[]
            {
                new object[]{0, MakeIdentPart("E1"), "+"},
                new object[]{1, MakeIdentPart("E3"), "+"}});
    
            EPContextPartitionImportResult importResult = GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new[] { new[] { 0, 1}}); // mapping 1 --> 0  (agent instance id 1 to 2)
            AssertImportsCPids(importResult.AllocatedToImported, new[] { new[] { 2, 0}}); // mapping 0 --> 2 (agent instance id 0 to 3)
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[]
            {
                new object[]{0, MakeIdentPart("E1"), "+"},
                new object[]{1, MakeIdentPart("E3"), "+"},
                new object[]{2, MakeIdentPart("E2"), "+"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));  // was reset
            epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 22));  // was created
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP,
                    new[] {new object[] {"E1", 12, 0}, new object[] {"E3", 61, 1}, new object[] {"E2", 22, 2}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertCategoryImport(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextCategory(epService, listener);
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[]
            {
                new object[]{0, MakeIdentCat("G1"), "+"},
                new object[]{1, MakeIdentCat("G2"), "+"},
                new object[]{2, MakeIdentCat("G3"), "+"}
            });
    
            // context partition 0 = G1
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"G1", 10, 0});
    
            EPContextPartitionExtract extract = GetSpi(epService).ExtractPaths(contextName, new ContextPartitionSelectorAll());
    
            epService.EPAdministrator.DestroyAllStatements();
            SetUpContextCategory(epService, listener);
    
            // context partition 1 = G2
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"G2", 20, 1});
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[]
            {
                new object[]{0, MakeIdentCat("G1"), "+"},
                new object[]{1, MakeIdentCat("G2"), "+"},
                new object[]{2, MakeIdentCat("G3"), "+"}
            });
    
            EPContextPartitionImportResult importResult = GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new[] { new[] { 0, 0}, new[] { 1, 1}, new[] { 2, 2}}); // mapping 1 --> 0  (agent instance id 1 to 2)
            AssertImportsCPids(importResult.AllocatedToImported, new int[0][]); // no new ones allocated
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[]
            {
                new object[]{0, MakeIdentCat("G1"), "+"},
                new object[]{1, MakeIdentCat("G2"), "+"},
                new object[]{2, MakeIdentCat("G3"), "+"}
            });
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));  // was reset
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            epService.EPRuntime.SendEvent(new SupportBean("G3", 31));  // was created
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, FIELDSCP,
                    new[] {new object[] {"G1", 11, 0}, new object[] {"G2", 21, 1}, new object[] {"G3", 31, 2}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertInitTermImport(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextInitTerm(epService, listener);
    
            // context partition 0
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean("S0_1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"S0_1", 10, 0});
    
            EPContextPartitionExtract extract = GetSpi(epService).ExtractPaths(contextName, new ContextPartitionSelectorAll());
    
            epService.EPAdministrator.DestroyAllStatements();
            SetUpContextInitTerm(epService, listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "S0_2"));
            epService.EPRuntime.SendEvent(new SupportBean("S0_2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"S0_2", 20, 0});
    
            EPContextPartitionImportResult importResult = GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new int[0][]); // no existing found
            AssertImportsCPids(importResult.AllocatedToImported, new[] { new[] { 1, 0}}); // new one created is 1
    
            epService.EPRuntime.SendEvent(new SupportBean("S0_2", 21));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"S0_2", 41, 0});
    
            epService.EPRuntime.SendEvent(new SupportBean("S0_1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDSCP, new object[]{"S0_1", 11, 1});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertNestedContextImport(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextNested(epService, listener);
            string[] fieldsnested = "c0,c1,c2,c3".Split(',');
    
            // context partition subpath 0=G1+negative, 1=G1+positive
            epService.EPRuntime.SendEvent(MakeEvent("G1", 10, 1000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsnested, new object[]{"G1", 10, 1000L, 1});
    
            // context partition subpath 2=G2+negative, 2=G2+positive
            epService.EPRuntime.SendEvent(MakeEvent("G2", -20, 2000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsnested, new object[]{"G2", -20, 2000L, 2});
    
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, true), new[]
            {
                new object[]{0, MakeIdentNested(MakeIdentPart("G1"), MakeIdentCat("negative")), "+"},
                new object[]{1, MakeIdentNested(MakeIdentPart("G1"), MakeIdentCat("positive")), "+"},
                new object[]{2, MakeIdentNested(MakeIdentPart("G2"), MakeIdentCat("negative")), "+"},
                new object[]{3, MakeIdentNested(MakeIdentPart("G2"), MakeIdentCat("positive")), "+"}});
    
            var nestedSelector = new SupportSelectorNested(new ContextPartitionSelectorAll(), new ContextPartitionSelectorAll());
            EPContextPartitionExtract extract = GetSpi(epService).ExtractPaths(contextName, nestedSelector);
            Assert.AreEqual(2, extract.NumNestingLevels);
    
            epService.EPAdministrator.DestroyAllStatements();
            SetUpContextNested(epService, listener);
    
            // context partition subpath 0=G3+negative, 1=G3+positive
            epService.EPRuntime.SendEvent(MakeEvent("G3", 30, 3000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsnested, new object[]{"G3", 30, 3000L, 1});
    
            // context partition subpath 2=G1+negative, 3=G1+positive
            epService.EPRuntime.SendEvent(MakeEvent("G1", 11, 1001));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsnested, new object[]{"G1", 11, 1001L, 3});
    
            EPContextPartitionImportResult importResult = GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new[] { new[] { 2, 0}, new[] { 3, 1}}); // mapping 0 --> 2, 1 --> 3  (agent instance id 1 to 2)
            AssertImportsCPids(importResult.AllocatedToImported, new[] { new[] { 4, 2}, new[] { 5, 3}});  // allocated ones
    
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, true), new[]
            {
                new object[]{0, MakeIdentNested(MakeIdentPart("G3"), MakeIdentCat("negative")), "+"},
                new object[]{1, MakeIdentNested(MakeIdentPart("G3"), MakeIdentCat("positive")), "+"},
                new object[]{2, MakeIdentNested(MakeIdentPart("G1"), MakeIdentCat("negative")), "+"},
                new object[]{3, MakeIdentNested(MakeIdentPart("G1"), MakeIdentCat("positive")), "+"},
                new object[]{4, MakeIdentNested(MakeIdentPart("G2"), MakeIdentCat("negative")), "+"},
                new object[]{5, MakeIdentNested(MakeIdentPart("G2"), MakeIdentCat("positive")), "+"}});
    
            epService.EPRuntime.SendEvent(MakeEvent("G3", 31, 3001));
            epService.EPRuntime.SendEvent(MakeEvent("G1", 12, 1002));  // reset
            epService.EPRuntime.SendEvent(MakeEvent("G2", 21, 2001));  // new
            epService.EPRuntime.SendEvent(MakeEvent("G2", -22, 2002));  // new
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, fieldsnested,
                    new[] {new object[] {"G3", 31, 6001L, 1}, new object[] {"G1", 12, 1002L, 3}, new object[] {"G2", 21, 2001L, 5}, new object[] {"G2", -22, 2002L, 4}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertHashSegmentedAllSelector(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextHashSegmented(epService, listener);
            int hashCodeE1 = CODE_FUNC_MOD64.CodeFor("E1");
            int hashCodeE2 = CODE_FUNC_MOD64.CodeFor("E2");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 10});
            EPAssertionUtil.AssertEqualsAnyOrder(new[]{0}, GetAllCPIds(epService, "HashSegByString", false));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 20});
            EPAssertionUtil.AssertEqualsAnyOrder(new[]{0, 1}, GetAllCPIds(epService, "HashSegByString", false));
    
            // deactivate all partitions
            EPContextPartitionExtract extract = GetSpi(epService).ExtractStopPaths(contextName, ContextPartitionSelectorAll.INSTANCE);
            AssertPathInfo(GetAllCPDescriptors(epService, contextName, false), new[]
            {
                new object[]{0, MakeIdentHash(hashCodeE1), "-"},
                new object[]{1, MakeIdentHash(hashCodeE2), "-"}
            });
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, false), new ContextState(1, 0, 2, 1, hashCodeE2, false));
    
            // assert E1 and E2 inactive
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            Assert.IsFalse(listener.IsInvoked);
            AssertCreateStmtNotActive(epService, "context HashSegByString select * from SupportBean", new SupportBean("E1", -1));
            AssertCreateStmtNotActive(epService, "context HashSegByString select * from SupportBean", new SupportBean("E2", -1));
    
            // activate context partitions
            GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true), new ContextState(1, 0, 2, 1, hashCodeE2, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 12});
            epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 22});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertHashSegmentedIndividualSelector(EPServiceProvider epService, ContextPartitionSelector selector) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextHashSegmented(epService, listener);
            int hashCodeE1 = CODE_FUNC_MOD64.CodeFor("E1");
            int hashCodeE2 = CODE_FUNC_MOD64.CodeFor("E2");
            Assert.IsTrue(hashCodeE1 != hashCodeE2);
            Assert.AreEqual(HASH_MOD_E1_STRING_BY_64, hashCodeE1);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 10});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true));
            AssertPathInfo("failed at code E1", GetSpi(epService).GetDescriptor(contextName, 0), new object[]{0, MakeIdentHash(hashCodeE1), "+"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 20});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true), new ContextState(1, 0, 2, 1, hashCodeE2, true));
    
            // deactive partition for "E1" code
            EPContextPartitionExtract extract = GetSpi(epService).ExtractStopPaths(contextName, selector);
            AssertPathInfo(extract.Collection.Descriptors, new[] {new object[] {0, MakeIdentHash(hashCodeE1), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, false), new ContextState(1, 0, 2, 1, hashCodeE2, true));
    
            // assert E1 inactive
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            Assert.IsFalse(listener.IsInvoked);
            AssertCreateStmtNotActive(epService, "context HashSegByString select * from SupportBean", new SupportBean("E1", -1));
    
            // assert E2 still active
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 41});
    
            // activate context partition for "E1"
            GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true), new ContextState(1, 0, 2, 1, hashCodeE2, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 12});
            epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 63});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertCreateStmtNotActive(EPServiceProvider epService, string epl, SupportBean testevent) {
            var local = new SupportUpdateListener();
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += local.Update;
    
            epService.EPRuntime.SendEvent(testevent);
            Assert.IsFalse(local.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void AssertPartitionedIndividualSelector(EPServiceProvider epService, ContextPartitionSelector selector) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextPartitioned(epService, listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 10});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new object[]{"E1"}, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 20});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new object[]{"E1"}, true), new ContextState(1, 0, 2, 1, new object[]{"E2"}, true));
    
            // deactive partition for "E1" code
            EPContextPartitionExtract extract = GetSpi(epService).ExtractStopPaths(contextName, selector);
            AssertPathInfo(extract.Collection.Descriptors, new[] {new object[] {0, MakeIdentPart("E1"), "+"}});
            AssertCreateStmtNotActive(epService, "context PartitionByString select * from SupportBean", new SupportBean("E1", -1));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new object[]{"E1"}, false), new ContextState(1, 0, 2, 1, new object[]{"E2"}, true));
    
            // assert E1 inactive
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 41});
    
            // activate context partition for "E1"
            GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new object[]{"E1"}, true), new ContextState(1, 0, 2, 1, new object[]{"E2"}, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 12});
            epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 63});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertPartitionedAllSelector(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextPartitioned(epService, listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 20});
    
            // deactive partition for all
            EPContextPartitionExtract extract = GetSpi(epService).ExtractStopPaths(contextName, ContextPartitionSelectorAll.INSTANCE);
            AssertPathInfo(extract.Collection.Descriptors, new[] {new object[] {0, MakeIdentPart("E1"), "+"}, new object[] {1, MakeIdentPart("E2"), "+"}});
            AssertCreateStmtNotActive(epService, "context PartitionByString select * from SupportBean", new SupportBean("E1", -1));
            AssertCreateStmtNotActive(epService, "context PartitionByString select * from SupportBean", new SupportBean("E2", -1));
    
            // assert E1 inactive
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            Assert.IsFalse(listener.IsInvoked);
    
            // activate context partition for all
            GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 12});
            epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 22});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertInitTermIndividualSelector(EPServiceProvider epService, ContextPartitionSelector selector) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextInitTerm(epService, listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, true));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, true), new ContextState(1, 0, 2, 1, null, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 20});
    
            // deactive partition for "E1" code
            EPContextPartitionExtract extract = GetSpi(epService).ExtractStopPaths(contextName, selector);
            AssertPathInfo(extract.Collection.Descriptors, new[] {new object[] {0, null, "+"}});
            AssertCreateStmtNotActive(epService, "context InitAndTermCtx select * from SupportBean(TheString = context.sbs0.p00)", new SupportBean("E1", -1));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, false), new ContextState(1, 0, 2, 1, null, true));
    
            // assert E1 inactive
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 41});
    
            // activate context partition for "E1"
            GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, true), new ContextState(1, 0, 2, 1, null, true));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 12});
            epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 63});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertInitTermAllSelector(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextInitTerm(epService, listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 20});
    
            // deactive partitions (all)
            EPContextPartitionExtract extract = GetSpi(epService).ExtractStopPaths(contextName, ContextPartitionSelectorAll.INSTANCE);
            AssertPathInfo(extract.Collection.Descriptors, new[] {new object[] {0, null, "+"}, new object[] {0, null, "+"}});
            AssertCreateStmtNotActive(epService, "context InitAndTermCtx select * from SupportBean(TheString = context.sbs0.p00)", new SupportBean("E1", -1));
            AssertCreateStmtNotActive(epService, "context InitAndTermCtx select * from SupportBean(TheString = context.sbs0.p00)", new SupportBean("E2", -1));
    
            // assert all inactive
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            Assert.IsFalse(listener.IsInvoked);
    
            // activate context partition (all)
            GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E1", 12});
            epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{"E2", 22});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertNestedContextIndividualSelector(EPServiceProvider epService, ContextPartitionSelector selector) {
            SupportContextStateCacheImpl.Reset();
            var listener = new SupportUpdateListener();
            string contextName = SetUpContextNested(epService, listener);
            string[] fields = "c0,c1,c2".Split(',');
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 1, null, true),
                    new ContextState(2, 1, 1, 0, null, true), new ContextState(2, 1, 2, 1, null, true));
            EPAssertionUtil.AssertEqualsAnyOrder(new[]{0, 1}, GetAllCPIds(epService, "NestedContext", false));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 11));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 12));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 1, null, true),
                    new ContextState(2, 1, 1, 0, null, true), new ContextState(2, 1, 2, 1, null, true),
                    new ContextState(1, 0, 2, 2, null, true),
                    new ContextState(2, 2, 1, 2, null, true), new ContextState(2, 2, 2, 3, null, true));
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 13));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, fields,
                    new[] {new object[] {"E1", 1, 10L}, new object[] {"E1", -1, 11L}, new object[] {"E2", 1, 12L}, new object[] {"E2", -1, 13L}});
    
            // deactive partition for E2/positive code
            EPContextPartitionExtract extract = GetSpi(epService).ExtractStopPaths(contextName, selector);
            AssertPathInfo(extract.Collection.Descriptors, new[]
            {new object[] {
                    3, MakeIdentNested(MakeIdentPart("E2"), MakeIdentCat("positive")), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 1, null, true),
                    new ContextState(2, 1, 1, 0, null, true), new ContextState(2, 1, 2, 1, null, true),
                    new ContextState(1, 0, 2, 2, null, true),
                    new ContextState(2, 2, 1, 2, null, true), new ContextState(2, 2, 2, 3, null, false));
    
            // assert E2/G2(1) inactive
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 20));
            epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 21));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 22)); // not used
            epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 23));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, fields,
                    new[] {new object[] {"E1", 1, 30L}, new object[] {"E1", -1, 32L}, new object[] {"E2", -1, 36L}});
            AssertCreateStmtNotActive(epService, "context NestedContext select * from SupportBean", new SupportBean("E2", 10000));
    
            // activate context partition for E2/positive
            GetSpi(epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30));
            epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 31));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 32));
            epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 33));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened().First, fields,
                    new[] {new object[] {"E1", 1, 60L}, new object[] {"E1", -1, 63L}, new object[] {"E2", 1, 32L}, new object[] {"E2", -1, 33L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private string SetUpContextNested(EPServiceProvider epService, SupportUpdateListener listener) {
    
            string createCtx = CONTEXT_CACHE_HOOK + "create context NestedContext as " +
                    "context ACtx partition by TheString from SupportBean, " +
                    "context BCtx " +
                    "  group by IntPrimitive < 0 as negative," +
                    "  group by IntPrimitive > 0 as positive from SupportBean";
            epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context NestedContext " +
                    "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2, context.id as c3 from SupportBean");
            stmt.Events += listener.Update;
    
            return "NestedContext";
        }
    
        private string SetUpContextHashSegmented(EPServiceProvider epService, SupportUpdateListener listener) {
    
            string createCtx = CONTEXT_CACHE_HOOK + "create context HashSegByString as coalesce by Consistent_hash_crc32(TheString) from SupportBean granularity 64";
            epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context HashSegByString " +
                    "select TheString as c0, sum(IntPrimitive) as c1, context.id as c2 " +
                    "from SupportBean group by TheString");
            stmt.Events += listener.Update;
    
            return "HashSegByString";
        }
    
        private string SetUpContextPartitioned(EPServiceProvider epService, SupportUpdateListener listener) {
    
            string createCtx = CONTEXT_CACHE_HOOK + "create context PartitionByString as partition by TheString from SupportBean";
            epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context PartitionByString " +
                    "select TheString as c0, sum(IntPrimitive) as c1, context.id as c2 " +
                    "from SupportBean");
            stmt.Events += listener.Update;
    
            return "PartitionByString";
        }
    
        private string SetUpContextCategory(EPServiceProvider epService, SupportUpdateListener listener) {
    
            string createCtx = CONTEXT_CACHE_HOOK + "create context CategoryContext as " +
                    "group by TheString = 'G1' as G1," +
                    "group by TheString = 'G2' as G2," +
                    "group by TheString = 'G3' as G3 from SupportBean";
            epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context CategoryContext " +
                    "select TheString as c0, sum(IntPrimitive) as c1, context.id as c2 " +
                    "from SupportBean");
            stmt.Events += listener.Update;
    
            return "CategoryContext";
        }
    
        private object MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        private string SetUpContextInitTerm(EPServiceProvider epService, SupportUpdateListener listener) {
    
            string createCtx = CONTEXT_CACHE_HOOK + "create context InitAndTermCtx as " +
                    "initiated by SupportBean_S0 sbs0 " +
                    "terminated after 24 hours";
            epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context InitAndTermCtx " +
                    "select TheString as c0, sum(IntPrimitive) as c1, context.id as c2 " +
                    "from SupportBean(TheString = context.sbs0.p00)");
            stmt.Events += listener.Update;
    
            return "InitAndTermCtx";
        }
    
        public class MySelectorHashById : ContextPartitionSelectorHash {
    
            private readonly ISet<int> _hashes;
    
            public MySelectorHashById(ISet<int> hashes) {
                _hashes = hashes;
            }

            public ICollection<int> Hashes => _hashes;
        }
    
        public class MySelectorHashFiltered : ContextPartitionSelectorFiltered {
            private readonly int _hashCode;
    
            public MySelectorHashFiltered(int hashCode) {
                _hashCode = hashCode;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                ContextPartitionIdentifierHash hash = (ContextPartitionIdentifierHash) contextPartitionIdentifier;
                return hash.Hash == _hashCode;
            }
        }
    
        public class MySelectorCategoryFiltered : ContextPartitionSelectorFiltered {
            private readonly string _label;
    
            public MySelectorCategoryFiltered(string label) {
                _label = label;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                ContextPartitionIdentifierCategory cat = (ContextPartitionIdentifierCategory) contextPartitionIdentifier;
                return cat.Label.Equals(_label);
            }
        }
    
        public class MySelectorPartitionFiltered : ContextPartitionSelectorFiltered {
            private readonly object[] _keys;
    
            public MySelectorPartitionFiltered(object[] keys) {
                _keys = keys;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                ContextPartitionIdentifierPartitioned part = (ContextPartitionIdentifierPartitioned) contextPartitionIdentifier;
                return Collections.AreEqual(part.Keys, _keys);
            }
        }
    
        public class MySelectorInitTermFiltered : ContextPartitionSelectorFiltered {
            private readonly string _p00PropertyValue;
    
            public MySelectorInitTermFiltered(string p00PropertyValue) {
                _p00PropertyValue = p00PropertyValue;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                ContextPartitionIdentifierInitiatedTerminated id = (ContextPartitionIdentifierInitiatedTerminated) contextPartitionIdentifier;
                EventBean @event = (EventBean) id.Properties.Get("sbs0");
                return _p00PropertyValue.Equals(@event.Get("p00"));
            }
        }
    
        public static void AssertImportsCPids(IDictionary<int, int> received, int[][] expected) {
            if (expected == null) {
                if (received == null) {
                    return;
                }
            } else {
                ScopeTestHelper.AssertNotNull(received);
            }
    
            if (expected != null) {
                for (int j = 0; j < expected.Length; j++) {
                    int key = expected[j][0];
                    int value = expected[j][1];
                    int receivevalue = received.Get(key);
                    ScopeTestHelper.AssertEquals("Error asserting key '" + key + "'", value, receivevalue);
                }
            }
        }
    
        private static void AssertPathInfo(IDictionary<int, ContextPartitionDescriptor> cpinfo,
                                           object[][] expected) {
    
            Assert.AreEqual(expected.Length, cpinfo.Count);
    
            for (int i = 0; i < expected.Length; i++) {
                object[] expectedRow = expected[i];
                string message = "failed assertion for item " + i;
                int expectedId = expectedRow[0].AsInt();
                ContextPartitionDescriptor desc = cpinfo.Get(expectedId);
                AssertPathInfo(message, desc, expectedRow);
            }
        }
    
        private static void AssertPathInfo(string message,
                                           ContextPartitionDescriptor desc,
                                           object[] expectedRow) {
            int expectedId = expectedRow[0].AsInt();
            ContextPartitionIdentifier expectedIdent = (ContextPartitionIdentifier) expectedRow[1];
            string expectedState = (string) expectedRow[2];
    
            Assert.AreEqual(desc.AgentInstanceId, expectedId, message);
            if (expectedIdent != null) {
                Assert.IsTrue(expectedIdent.CompareTo(desc.Identifier), message);
            } else {
                Assert.IsTrue(desc.Identifier is ContextPartitionIdentifierInitiatedTerminated, message);
            }
    
            ContextPartitionState stateEnum;
            if (expectedState.Equals("+")) {
                stateEnum = ContextPartitionState.STARTED;
            } else if (expectedState.Equals("-")) {
                stateEnum = ContextPartitionState.STOPPED;
            } else {
                throw new IllegalStateException("Failed to parse expected state '" + expectedState + "' as {+,-}");
            }
            Assert.AreEqual(stateEnum, desc.State, message);
        }
    
        private static IDictionary<int, ContextPartitionDescriptor> GetAllCPDescriptors(EPServiceProvider epService, string contextName, bool nested) {
            ContextPartitionSelector selector = ContextPartitionSelectorAll.INSTANCE;
            if (nested) {
                selector = new SupportSelectorNested(ContextPartitionSelectorAll.INSTANCE, ContextPartitionSelectorAll.INSTANCE);
            }
            return GetSpi(epService).GetContextPartitions(contextName, selector).Descriptors;
        }
    
        private static ICollection<int> GetAllCPIds(EPServiceProvider epService, string contextName, bool nested) {
            ContextPartitionSelector selector = ContextPartitionSelectorAll.INSTANCE;
            if (nested) {
                selector = new SupportSelectorNested(ContextPartitionSelectorAll.INSTANCE, ContextPartitionSelectorAll.INSTANCE);
            }
            return GetSpi(epService).GetContextPartitionIds(contextName, selector);
        }
    
        private static EPContextPartitionAdminSPI GetSpi(EPServiceProvider epService) {
            return (EPContextPartitionAdminSPI) epService.EPAdministrator.ContextPartitionAdmin;
        }
    
        private static ContextPartitionIdentifier MakeIdentCat(string label) {
            return new ContextPartitionIdentifierCategory(label);
        }
    
        private static ContextPartitionIdentifier MakeIdentHash(int code) {
            return new ContextPartitionIdentifierHash(code);
        }
    
        private static ContextPartitionIdentifier MakeIdentPart(object singleKey) {
            return new ContextPartitionIdentifierPartitioned(new[]{singleKey});
        }
    
        private static ContextPartitionIdentifier MakeIdentNested(params ContextPartitionIdentifier[] identifiers) {
            return new ContextPartitionIdentifierNested(identifiers);
        }
    
        private class AgentInstanceSelectorAll : AgentInstanceSelector {
            public bool Select(AgentInstance agentInstance) {
                return true;
            }
        }
    }
} // end of namespace
