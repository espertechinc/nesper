///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestAdminContextPartitionSPI : ContextStateCacheHook
    {
        private static readonly SupportHashCodeFuncGranularCRC32 CODE_FUNC_MOD64 =
            new SupportHashCodeFuncGranularCRC32(64);
        private static readonly String[] FIELDS = "c0,c1".Split(',');
        private static readonly String[] FIELDSCP = "c0,c1,c2".Split(',');
        private static readonly int HASH_MOD_E1_STRING_BY_64 = 5;
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType<SupportBean_S1>();
            configuration.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _epService = null;
        }
    
        [Test]
        public void TestDestroyCtxPartitions() {
            AssertExtractDestroyPartitionedById();
            AssertDestroyCategory();
            AssertDestroyHashSegmented();
            AssertDestroyPartitioned();
            AssertDestroyInitTerm();
            AssertDestroyNested();
        }
    
        [Test]
        public void TestInvalid() {
            // context not found
            try {
                GetSpi(_epService).GetContextNestingLevel("undefined");
                Assert.Fail();
            }
            catch (ArgumentException ex) {
                Assert.AreEqual("Context by name 'undefined' could not be found", ex.Message);
            }
    
            // invalid selector for context
            _epService.EPAdministrator.CreateEPL("create context SomeContext partition by TheString from SupportBean");
            try {
                GetSpi(_epService).DestroyContextPartitions("SomeContext", new SupportSelectorCategory("abc"));
                Assert.Fail();
            }
            catch (InvalidContextPartitionSelector) {
            }
        }
    
        [Test]
        public void TestStopStartNestedCtxPartitions() {
            String contextName = "CategoryContext";
            String createCtx = CONTEXT_CACHE_HOOK + "create context CategoryContext as " +
                    "group by IntPrimitive < 0 as negative, group by IntPrimitive > 0 as positive from SupportBean";
            _epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("context CategoryContext " +
                    "select TheString as c0, Sum(IntPrimitive) as c1, context.id as c2 from SupportBean");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", -5));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 20));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][]{new Object[] {"E1", 10, 1}, new Object[] {"E2", -5, 0}, new Object[] {"E3", 30, 1}});
    
            // stop category "negative"
            SupportContextStateCacheImpl.Reset();
            ContextPartitionCollection collStop = GetSpi(_epService).StopContextPartitions("CategoryContext", new SupportSelectorCategory("negative"));
            AssertPathInfo(collStop.Descriptors, new Object[][] { new Object[] {0, MakeIdentCat("negative"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] { new Object[] {0, MakeIdentCat("negative"), "-"}, new Object[] {1, MakeIdentCat("positive"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, false));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", -6));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 30));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][] {new Object[] {"E5", 60, 1}});
    
            // start category "negative"
            ContextPartitionCollection collStart = GetSpi(_epService).StartContextPartitions("CategoryContext", new SupportSelectorCategory("negative"));
            AssertPathInfo(collStart.Descriptors, new Object[][] { new Object[] {0, MakeIdentCat("negative"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] { new Object[] {0, MakeIdentCat("negative"), "+"}, new Object[] {1, MakeIdentCat("positive"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", -7));
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 40));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][] {new Object[] {"E6", -7, 0}, new Object[] {"E7", 100, 1}});
    
            // stop category "positive"
            SupportContextStateCacheImpl.Reset();
            GetSpi(_epService).StopContextPartition("CategoryContext", 1);
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] { new Object[] {0, MakeIdentCat("negative"), "+"}, new Object[] {1, MakeIdentCat("positive"), "-"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 2, 1, 1, false));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E8", -8));
            _epService.EPRuntime.SendEvent(new SupportBean("E9", 50));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][] {new Object[] {"E8", -15, 0}});
    
            // start category "positive"
            GetSpi(_epService).StartContextPartition("CategoryContext", 1);
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] { new Object[] {0, MakeIdentCat("negative"), "+"}, new Object[] {1, MakeIdentCat("positive"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 2, 1, 1, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E10", -9));
            _epService.EPRuntime.SendEvent(new SupportBean("E11", 60));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][] {new Object[] {"E10", -24, 0}, new Object[] {"E11", 60, 1}});
        }
    
        [Test]
        public void TestGetContextStatementNames() {
            _epService.EPAdministrator.CreateEPL("create context CtxA partition by TheString from SupportBean");
            EPStatement stmtA = _epService.EPAdministrator.CreateEPL("@Name('A') context CtxA select count(*) from SupportBean");
            EPStatement stmtB = _epService.EPAdministrator.CreateEPL("@Name('B') context CtxA select sum(IntPrimitive) from SupportBean");
    
            EPAssertionUtil.AssertEqualsAnyOrder(GetSpi(_epService).GetContextStatementNames("CtxA"), "A,B".Split(','));
    
            stmtA.Dispose();
            EPAssertionUtil.AssertEqualsAnyOrder(GetSpi(_epService).GetContextStatementNames("CtxA"), "B".Split(','));
    
            stmtB.Dispose();
            EPAssertionUtil.AssertEqualsAnyOrder(GetSpi(_epService).GetContextStatementNames("CtxA"), new String[0]);
    
            Assert.AreEqual(null, GetSpi(_epService).GetContextStatementNames("undefined"));
        }
    
        [Test]
        public void TestAcrossURIExtractImport()
        {
            AssertHashSegmentedImport();
            AssertPartitionedImport();
            AssertCategoryImport();
            AssertInitTermImport();
            AssertNestedContextImport();
        }
    
        [Test]
        public void TestSameURIExtractStopImportStart()
        {
            AssertHashSegmentedIndividualSelector(new MySelectorHashById(Collections.SingletonList(HASH_MOD_E1_STRING_BY_64)));
            AssertHashSegmentedIndividualSelector(new MySelectorHashFiltered(HASH_MOD_E1_STRING_BY_64));
            AssertHashSegmentedIndividualSelector(new SupportSelectorById(Collections.SingletonList(0)));
            AssertHashSegmentedAllSelector();
    
            AssertCategoryIndividualSelector(new SupportSelectorCategory(Collections.SingletonList("G2")));
            AssertCategoryIndividualSelector(new MySelectorCategoryFiltered("G2"));
            AssertCategoryIndividualSelector(new SupportSelectorById(Collections.SingletonList(1)));
            AssertCategoryAllSelector();
    
            AssertPartitionedIndividualSelector(new SupportSelectorById(Collections.SingletonList(0)));
            AssertPartitionedIndividualSelector(new SupportSelectorPartitioned(Collections.SingletonList(new Object[]{"E1"})));
            AssertPartitionedIndividualSelector(new MySelectorPartitionFiltered(new Object[]{"E1"}));
            AssertPartitionedAllSelector();
    
            AssertInitTermIndividualSelector(new MySelectorInitTermFiltered("E1"));
            AssertInitTermIndividualSelector(new SupportSelectorById(Collections.SingletonList(0)));
            AssertInitTermAllSelector();
    
            AssertNestedContextIndividualSelector(new SupportSelectorNested(
                    new MySelectorPartitionFiltered(new Object[]{"E2"}), new MySelectorCategoryFiltered("positive")));
        }
    
        private void AssertDestroyNested() {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextNested();
            Assert.AreEqual(2, _epService.EPAdministrator.ContextPartitionAdmin.GetContextNestingLevel(contextName));
            String[] fieldsnested = "c0,c1,c2,c3".Split(',');
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 11));
            _epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 12));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 13));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, fieldsnested,
                    new Object[][]{new Object[] {"E1", 1, 10L, 1}, new Object[] {"E2", 1, 11L, 3}, new Object[] {"E1", -1, 12L, 0}, new Object[] {"E2", -1, 13L, 2}});
    
            // destroy hash for "S0_2"
            ContextPartitionCollection collDestroy = GetSpi(_epService).DestroyContextPartitions(contextName, new SupportSelectorNested(new SupportSelectorPartitioned("E2"), new SupportSelectorCategory("negative")));
            AssertPathInfo(collDestroy.Descriptors, new Object[][] { new Object[] {2, MakeIdentNested(MakeIdentPart("E2"), MakeIdentCat("negative")), "+"}});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, true), new Object[][] {
                    new Object[] {0, MakeIdentNested(MakeIdentPart("E1"), MakeIdentCat("negative")), "+"},
                    new Object[] {1, MakeIdentNested(MakeIdentPart("E1"), MakeIdentCat("positive")), "+"},
                    new Object[] {3, MakeIdentNested(MakeIdentPart("E2"), MakeIdentCat("positive")), "+"}});
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(2, 2, 1));
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 20));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 21));
            _epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 22));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 23));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, fieldsnested,
                    new Object[][] {new Object[] {"E1", 1, 30L, 1}, new Object[] {"E2", 1, 32L, 3}, new Object[] {"E1", -1, 34L, 0}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertDestroyInitTerm() {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextInitTerm();
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S0_3"));
            _epService.EPRuntime.SendEvent(new SupportBean("S0_1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("S0_2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("S0_3", 3));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][]{new Object[] {"S0_1", 1, 0}, new Object[] {"S0_2", 2, 1}, new Object[] {"S0_3", 3, 2}});
    
            // destroy hash for "S0_2"
            ContextPartitionCollection collDestroy = GetSpi(_epService).DestroyContextPartitions(contextName, new SupportSelectorById(1));
            AssertPathInfo(collDestroy.Descriptors, new Object[][] { new Object[] {1, null, "+"}});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {new Object[] {0, null, "+"}, new Object[] {2, null, "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, true), new ContextState(1, 0, 3, 2, null, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 2));
    
            _epService.EPRuntime.SendEvent(new SupportBean("S0_1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("S0_2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("S0_3", 30));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][] {new Object[] {"S0_1", 11, 0}, new Object[] {"S0_3", 33, 2}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertDestroyHashSegmented() {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextHashSegmented();
            int hashCodeE1 = CODE_FUNC_MOD64.CodeFor("E1");
            int hashCodeE2 = CODE_FUNC_MOD64.CodeFor("E2");
            int hashCodeE3 = CODE_FUNC_MOD64.CodeFor("E3");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][]{new Object[] {"E1", 1, 0}, new Object[] {"E2", 2, 1}, new Object[] {"E3", 3, 2}});
    
            // destroy hash for "E2"
            ContextPartitionCollection collDestroy = GetSpi(_epService).DestroyContextPartitions(contextName, new SupportSelectorByHashCode(hashCodeE2));
            AssertPathInfo(collDestroy.Descriptors, new Object[][] { new Object[] {1, MakeIdentHash(hashCodeE2), "+"}});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {new Object[] {0, MakeIdentHash(hashCodeE1), "+"}, new Object[] {2, MakeIdentHash(hashCodeE3), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true), new ContextState(1, 0, 3, 2, hashCodeE3, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 2));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][] {new Object[] {"E1", 11, 0}, new Object[] {"E2", 20, 3}, new Object[] {"E3", 33, 2}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertDestroyPartitioned() {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextPartitioned();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][]{new Object[] {"E1", 1, 0}, new Object[] {"E2", 2, 1}, new Object[] {"E3", 3, 2}});
    
            // destroy hash for "E2"
            ContextPartitionCollection collDestroy = GetSpi(_epService).DestroyContextPartitions(contextName, new SupportSelectorPartitioned("E2"));
            AssertPathInfo(collDestroy.Descriptors, new Object[][] { new Object[] {1, MakeIdentPart("E2"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {new Object[] {0, MakeIdentPart("E1"), "+"}, new Object[] {2, MakeIdentPart("E3"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new Object[] {"E1"}, true), new ContextState(1, 0, 3, 2, new Object[] {"E3"}, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 2));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][] {new Object[] {"E1", 11, 0}, new Object[] {"E2", 20, 3}, new Object[] {"E3", 33, 2}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertExtractDestroyPartitionedById() {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextPartitioned();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][]{new Object[] {"E1", 1, 0}, new Object[] {"E2", 2, 1}, new Object[] {"E3", 3, 2}});
    
            // destroy hash for "E2"
            ContextPartitionDescriptor destroyedOne = GetSpi(_epService).DestroyContextPartition(contextName, 1);
            AssertPathInfo("destroyed", destroyedOne, new Object[] {1, MakeIdentPart("E2"), "+"});
    
            // destroy hash for "E3"
            EPContextPartitionExtract collDestroy = GetSpi(_epService).ExtractDestroyPaths(contextName, new SupportSelectorById(0));
            AssertPathInfo(collDestroy.Collection.Descriptors, new Object[][] { new Object[] {0, MakeIdentPart("E1"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {new Object[] {2, MakeIdentPart("E3"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 3, 2, new Object[] {"E3"}, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 1), new ContextStatePathKey(1, 0, 2));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][] {new Object[] {"E1", 10, 3}, new Object[] {"E2", 20, 4}, new Object[] {"E3", 33, 2}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertDestroyCategory() {
            SupportContextStateCacheImpl.Reset();
            String contextName = "CategoryContext";
            String createCtx = CONTEXT_CACHE_HOOK + "create context CategoryContext as " +
                    "group by IntPrimitive < 0 as negative, " +
                    "group by IntPrimitive = 0 as zero," +
                    "group by IntPrimitive > 0 as positive from SupportBean";
            _epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("context CategoryContext " +
                    "select TheString as c0, Count(*) as c1, context.id as c2 from SupportBean");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][]{new Object[] {"E1", 1L, 0}, new Object[] {"E2", 1L, 1}, new Object[] {"E3", 1L, 2}});
    
            // destroy category "negative"
            ContextPartitionCollection collDestroy = GetSpi(_epService).DestroyContextPartitions(contextName, new SupportSelectorCategory("zero"));
            AssertPathInfo(collDestroy.Descriptors, new Object[][] { new Object[] {1, MakeIdentCat("zero"), "+"}});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {new Object[] {0, MakeIdentCat("negative"), "+"}, new Object[] {2, MakeIdentCat("positive"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 3, 2, 2, true));
            SupportContextStateCacheImpl.AssertRemovedState(new ContextStatePathKey(1, 0, 2));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", -1));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP, new Object[][] {new Object[] {"E4", 2L, 0}, new Object[] {"E6", 2L, 2}});
    
            // destroy again, should return empty
            ContextPartitionCollection collDestroyTwo = GetSpi(_epService).DestroyContextPartitions(contextName, new SupportSelectorCategory("zero"));
            Assert.IsTrue(collDestroyTwo.Descriptors.IsEmpty());
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertCategoryAllSelector() {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextCategory();
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, true), new ContextState(1, 0, 3, 2, 2, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 100));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDS, new Object[][]{new Object[] {"G1", 1}, new Object[] {"G2", 10}, new Object[] {"G3", 100}});
    
            // deactivate all categories
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractStopPaths(contextName, ContextPartitionSelectorAll.INSTANCE);
            AssertPathInfo(extract.Collection.Descriptors, new Object[][] {new Object[] {0, MakeIdentCat("G1"), "+"}, new Object[] {1, MakeIdentCat("G2"), "+"}, new Object[] {2, MakeIdentCat("G3"), "+"}});
            Assert.AreEqual(1, extract.NumNestingLevels);
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, false), new ContextState(1, 0, 2, 1, 1, false), new ContextState(1, 0, 3, 2, 2, false));
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 12));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 102));
            Assert.IsFalse(_listener.IsInvoked);
            AssertCreateStmtNotActive("context CategoryContext select * from SupportBean", new SupportBean("G1", -1));
            AssertCreateStmtNotActive("context CategoryContext select * from SupportBean", new SupportBean("G2", -1));
            AssertCreateStmtNotActive("context CategoryContext select * from SupportBean", new SupportBean("G3", -1));
    
            // activate categories
            GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, true), new ContextState(1, 0, 3, 2, 2, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 13));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 103));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDS, new Object[][] {new Object[] {"G1", 3}, new Object[] {"G2", 13}, new Object[] {"G3", 103}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertCategoryIndividualSelector(ContextPartitionSelector selectorCategoryG2) {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextCategory();
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, true), new ContextState(1, 0, 3, 2, 2, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 100));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDS, new Object[][]{new Object[] {"G1", 1}, new Object[] {"G2", 10}, new Object[] {"G3", 100}});
    
            // deactivate category G2
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractStopPaths(contextName, selectorCategoryG2);
            AssertPathInfo(extract.Collection.Descriptors, new Object[][]{new Object[] {1, MakeIdentCat("G2"), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, false), new ContextState(1, 0, 3, 2, 2, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 12));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 102));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDS, new Object[][]{new Object[] {"G1", 3}, new Object[] {"G3", 202}});
            AssertCreateStmtNotActive("context CategoryContext select * from SupportBean", new SupportBean("G2", -1));
    
            // activate category G2
            GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, 0, true), new ContextState(1, 0, 2, 1, 1, true), new ContextState(1, 0, 3, 2, 2, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 13));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 103));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDS, new Object[][]{new Object[] {"G1", 6}, new Object[] {"G2", 13}, new Object[] {"G3", 305}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertHashSegmentedImport() {
            String contextName = SetUpContextHashSegmented();
    
            // context partition 0 = code for E2
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"E2", 20, 0});
    
            // context partition 1 = code for E1
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"E1", 10, 1});
    
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractPaths(contextName, new ContextPartitionSelectorAll());
    
            _epService.Initialize();
            SetUpContextHashSegmented();
    
            // context partition 0 = code for E3
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"E3", 30, 0});
    
            // context partition 1 = code for E4
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 40));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"E4", 40, 1});
    
            // context partition 2 = code for E1
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"E1", 11, 2});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {
                    new Object[] {0, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E3")), "+"},
                    new Object[] {1, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E4")), "+"},
                    new Object[] {2, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E1")), "+"}});
    
            EPContextPartitionImportResult importResult = GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new int[][] {new int[] {2, 1}}); // mapping 1 --> 2  (agent instance id 1 to 2)
            AssertImportsCPids(importResult.AllocatedToImported, new int[][] {new int[] {3, 0}}); // mapping 0 --> 3 (agent instance id 0 to 3)
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {
                    new Object[] {0, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E3")), "+"},
                    new Object[] {1, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E4")), "+"},
                    new Object[] {2, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E1")), "+"},
                    new Object[] {3, MakeIdentHash(CODE_FUNC_MOD64.CodeFor("E2")), "+"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 41));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));  // was reset
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));  // was created
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP,
                    new Object[][] {new Object[] {"E3", 61, 0}, new Object[] {"E4", 81, 1}, new Object[] {"E1", 12, 2}, new Object[] {"E2", 22, 3}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertPartitionedImport() {
            String contextName = SetUpContextPartitioned();
            Assert.AreEqual(1, _epService.EPAdministrator.ContextPartitionAdmin.GetContextNestingLevel(contextName));
    
            // context partition 0 = E2
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"E2", 20, 0});
    
            // context partition 1 = E1
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"E1", 10, 1});
    
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractPaths(contextName, new ContextPartitionSelectorAll());
            AssertPathInfo(extract.Collection.Descriptors, new Object[][] { new Object[] {0, MakeIdentPart("E2"), "+"}, new Object[] {1, MakeIdentPart("E1"), "+"}});
    
            _epService.Initialize();
            SetUpContextPartitioned();
    
            // context partition 0 = E1
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[] {"E1", 11, 0});
    
            // context partition 1 = E3
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[] {"E3", 30, 1});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {
                    new Object[] {0, MakeIdentPart("E1"), "+"}, new Object[] {1, MakeIdentPart("E3"), "+"}});
    
            EPContextPartitionImportResult importResult = GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new int[][] { new int[] { 0, 1 } }); // mapping 1 --> 0  (agent instance id 1 to 2)
            AssertImportsCPids(importResult.AllocatedToImported, new int[][] { new int[] { 2, 0 } }); // mapping 0 --> 2 (agent instance id 0 to 3)
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {
                    new Object[] {0, MakeIdentPart("E1"), "+"}, new Object[] {1, MakeIdentPart("E3"), "+"}, new Object[] {2, MakeIdentPart("E2"), "+"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));  // was reset
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));  // was created
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP,
                    new Object[][] {new Object[] {"E1", 12, 0}, new Object[] {"E3", 61, 1}, new Object[] {"E2", 22, 2}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertCategoryImport() {
            String contextName = SetUpContextCategory();
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {
                    new Object[] {0, MakeIdentCat("G1"), "+"}, new Object[] {1, MakeIdentCat("G2"), "+"}, new Object[] {2, MakeIdentCat("G3"), "+"}});
    
            // context partition 0 = G1
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[] {"G1", 10, 0});
    
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractPaths(contextName, new ContextPartitionSelectorAll());
    
            _epService.Initialize();
            SetUpContextCategory();
    
            // context partition 1 = G2
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[] {"G2", 20, 1});
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {
                    new Object[] {0, MakeIdentCat("G1"), "+"}, new Object[] {1, MakeIdentCat("G2"), "+"}, new Object[] {2, MakeIdentCat("G3"), "+"}});
    
            EPContextPartitionImportResult importResult = GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new int[][] { new int[] { 0, 0 }, new int[] { 1, 1 }, new int[] { 2, 2 } }); // mapping 1 --> 0  (agent instance id 1 to 2)
            AssertImportsCPids(importResult.AllocatedToImported, new int[0][]); // no new ones allocated
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {
                    new Object[] {0, MakeIdentCat("G1"), "+"}, new Object[] {1, MakeIdentCat("G2"), "+"}, new Object[] {2, MakeIdentCat("G3"), "+"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));  // was reset
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 31));  // was created
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, FIELDSCP,
                    new Object[][] {new Object[] {"G1", 11, 0}, new Object[] {"G2", 21, 1}, new Object[] {"G3", 31, 2}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertInitTermImport() {
            String contextName = SetUpContextInitTerm();
    
            // context partition 0
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("S0_1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"S0_1", 10, 0});
    
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractPaths(contextName, new ContextPartitionSelectorAll());
    
            _epService.Initialize();
            SetUpContextInitTerm();
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "S0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean("S0_2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"S0_2", 20, 0});
    
            EPContextPartitionImportResult importResult = GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new int[0][]); // no existing found
            AssertImportsCPids(importResult.AllocatedToImported, new int[][] {new int[] {1, 0}}); // new one created is 1
    
            _epService.EPRuntime.SendEvent(new SupportBean("S0_2", 21));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"S0_2", 41, 0});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S0_1", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDSCP, new Object[]{"S0_1", 11, 1});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertNestedContextImport() {
            String contextName = SetUpContextNested();
            String[] fieldsnested = "c0,c1,c2,c3".Split(',');
    
            // context partition subpath 0=G1+negative, 1=G1+positive
            _epService.EPRuntime.SendEvent(MakeEvent("G1", 10, 1000));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsnested, new Object[]{"G1", 10, 1000L, 1});
    
            // context partition subpath 2=G2+negative, 2=G2+positive
            _epService.EPRuntime.SendEvent(MakeEvent("G2", -20, 2000));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsnested, new Object[]{"G2", -20, 2000L, 2});
    
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, true), new Object[][] {
                    new Object[] {0, MakeIdentNested(MakeIdentPart("G1"), MakeIdentCat("negative")), "+"},
                    new Object[] {1, MakeIdentNested(MakeIdentPart("G1"), MakeIdentCat("positive")), "+"},
                    new Object[] {2, MakeIdentNested(MakeIdentPart("G2"), MakeIdentCat("negative")), "+"},
                    new Object[] {3, MakeIdentNested(MakeIdentPart("G2"), MakeIdentCat("positive")), "+"}});
    
            SupportSelectorNested nestedSelector = new SupportSelectorNested(new ContextPartitionSelectorAll(), new ContextPartitionSelectorAll());
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractPaths(contextName, nestedSelector);
            Assert.AreEqual(2, extract.NumNestingLevels);
    
            _epService.Initialize();
            SetUpContextNested();
    
            // context partition subpath 0=G3+negative, 1=G3+positive
            _epService.EPRuntime.SendEvent(MakeEvent("G3", 30, 3000));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsnested, new Object[] {"G3", 30, 3000L, 1});
    
            // context partition subpath 2=G1+negative, 3=G1+positive
            _epService.EPRuntime.SendEvent(MakeEvent("G1", 11, 1001));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsnested, new Object[] {"G1", 11, 1001L, 3});
    
            EPContextPartitionImportResult importResult = GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            AssertImportsCPids(importResult.ExistingToImported, new int[][] { new int[] { 2, 0 }, new int[] { 3, 1 } }); // mapping 0 --> 2, 1 --> 3  (agent instance id 1 to 2)
            AssertImportsCPids(importResult.AllocatedToImported, new int[][] { new int[] { 4, 2 }, new int[] { 5, 3 } });  // allocated ones
    
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, true), new Object[][] {
                    new Object[] {0, MakeIdentNested(MakeIdentPart("G3"), MakeIdentCat("negative")), "+"},
                    new Object[] {1, MakeIdentNested(MakeIdentPart("G3"), MakeIdentCat("positive")), "+"},
                    new Object[] {2, MakeIdentNested(MakeIdentPart("G1"), MakeIdentCat("negative")), "+"},
                    new Object[] {3, MakeIdentNested(MakeIdentPart("G1"), MakeIdentCat("positive")), "+"},
                    new Object[] {4, MakeIdentNested(MakeIdentPart("G2"), MakeIdentCat("negative")), "+"},
                    new Object[] {5, MakeIdentNested(MakeIdentPart("G2"), MakeIdentCat("positive")), "+"}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("G3", 31, 3001));
            _epService.EPRuntime.SendEvent(MakeEvent("G1", 12, 1002));  // reset
            _epService.EPRuntime.SendEvent(MakeEvent("G2", 21, 2001));  // new
            _epService.EPRuntime.SendEvent(MakeEvent("G2", -22, 2002));  // new
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, fieldsnested,
                    new Object[][] {new Object[] {"G3", 31, 6001L, 1}, new Object[] {"G1", 12, 1002L, 3}, new Object[] {"G2", 21, 2001L, 5}, new Object[] {"G2", -22, 2002L, 4}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertHashSegmentedAllSelector()
        {
            String contextName = SetUpContextHashSegmented();
            int hashCodeE1 = CODE_FUNC_MOD64.CodeFor("E1");
            int hashCodeE2 = CODE_FUNC_MOD64.CodeFor("E2");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[]{"E1", 10});
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 0 }, GetAllCPIds(_epService, "HashSegByString", false));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[]{"E2", 20});
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 0, 1 }, GetAllCPIds(_epService, "HashSegByString", false));
    
            // deactivate all partitions
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractStopPaths(contextName, ContextPartitionSelectorAll.INSTANCE);
            AssertPathInfo(GetAllCPDescriptors(_epService, contextName, false), new Object[][] {
                    new Object[] {0, MakeIdentHash(hashCodeE1), "-"}, new Object[] {1, MakeIdentHash(hashCodeE2), "-"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, false), new ContextState(1, 0, 2, 1, hashCodeE2, false));
    
            // assert E1 and E2 inactive
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            Assert.IsFalse(_listener.IsInvoked);
            AssertCreateStmtNotActive("context HashSegByString select * from SupportBean", new SupportBean("E1", -1));
            AssertCreateStmtNotActive("context HashSegByString select * from SupportBean", new SupportBean("E2", -1));
    
            // activate context partitions
            GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true), new ContextState(1, 0, 2, 1, hashCodeE2, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E1", 12});
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 22});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertHashSegmentedIndividualSelector(ContextPartitionSelector selector) {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextHashSegmented();
            int hashCodeE1 = CODE_FUNC_MOD64.CodeFor("E1");
            int hashCodeE2 = CODE_FUNC_MOD64.CodeFor("E2");
            Assert.IsTrue(hashCodeE1 != hashCodeE2);
            Assert.AreEqual(HASH_MOD_E1_STRING_BY_64, hashCodeE1);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[]{"E1", 10});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true));
            AssertPathInfo("failed at code E1", GetSpi(_epService).GetDescriptor(contextName, 0), new Object[] {0, MakeIdentHash(hashCodeE1), "+"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[]{"E2", 20});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true), new ContextState(1, 0, 2, 1, hashCodeE2, true));
    
            // deactive partition for "E1" code
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractStopPaths(contextName, selector);
            AssertPathInfo(extract.Collection.Descriptors, new Object[][] { new Object[] {0, MakeIdentHash(hashCodeE1), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, false), new ContextState(1, 0, 2, 1, hashCodeE2, true));
    
            // assert E1 inactive
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            Assert.IsFalse(_listener.IsInvoked);
            AssertCreateStmtNotActive("context HashSegByString select * from SupportBean", new SupportBean("E1", -1));
    
            // assert E2 still active
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 41});
    
            // activate context partition for "E1"
            GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, hashCodeE1, true), new ContextState(1, 0, 2, 1, hashCodeE2, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E1", 12});
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 63});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertCreateStmtNotActive(String epl, SupportBean testevent) {
            SupportUpdateListener local = new SupportUpdateListener();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += local.Update;
    
            _epService.EPRuntime.SendEvent(testevent);
            Assert.IsFalse(local.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void AssertPartitionedIndividualSelector(ContextPartitionSelector selector) {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextPartitioned();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E1", 10});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new Object[]{"E1"}, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[]{"E2", 20});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new Object[]{"E1"}, true), new ContextState(1, 0, 2, 1, new Object[]{"E2"}, true));
    
            // deactive partition for "E1" code
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractStopPaths(contextName, selector);
            AssertPathInfo(extract.Collection.Descriptors, new Object[][] { new Object[] {0, MakeIdentPart("E1"), "+"}});
            AssertCreateStmtNotActive("context PartitionByString select * from SupportBean", new SupportBean("E1", -1));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new Object[] {"E1"}, false), new ContextState(1, 0, 2, 1, new Object[] {"E2"}, true));
    
            // assert E1 inactive
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            Assert.IsFalse(_listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 41});
    
            // activate context partition for "E1"
            GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, new Object[] {"E1"}, true), new ContextState(1, 0, 2, 1, new Object[] {"E2"}, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E1", 12});
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 63});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertPartitionedAllSelector() {
            String contextName = SetUpContextPartitioned();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E1", 10});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 20});
    
            // deactive partition for all
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractStopPaths(contextName, ContextPartitionSelectorAll.INSTANCE);
            AssertPathInfo(extract.Collection.Descriptors, new Object[][] { new Object[] {0, MakeIdentPart("E1"), "+"}, new Object[] {1, MakeIdentPart("E2"), "+"}});
            AssertCreateStmtNotActive("context PartitionByString select * from SupportBean", new SupportBean("E1", -1));
            AssertCreateStmtNotActive("context PartitionByString select * from SupportBean", new SupportBean("E2", -1));
    
            // assert E1 inactive
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            Assert.IsFalse(_listener.IsInvoked);
    
            // activate context partition for all
            GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E1", 12});
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 22});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertInitTermIndividualSelector(ContextPartitionSelector selector) {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextInitTerm();
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, true), new ContextState(1, 0, 2, 1, null, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[]{"E1", 10});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[]{"E2", 20});
    
            // deactive partition for "E1" code
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractStopPaths(contextName, selector);
            AssertPathInfo(extract.Collection.Descriptors, new Object[][] { new Object[] {0, null, "+"}});
            AssertCreateStmtNotActive("context InitAndTermCtx select * from SupportBean(TheString = context.sbs0.p00)", new SupportBean("E1", -1));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, false), new ContextState(1, 0, 2, 1, null, true));
    
            // assert E1 inactive
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            Assert.IsFalse(_listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 41});
    
            // activate context partition for "E1"
            GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 0, null, true), new ContextState(1, 0, 2, 1, null, true));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E1", 12});
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 63});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertInitTermAllSelector() {
            String contextName = SetUpContextInitTerm();
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[]{"E1", 10});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[]{"E2", 20});
    
            // deactive partitions (all)
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractStopPaths(contextName, ContextPartitionSelectorAll.INSTANCE);
            AssertPathInfo(extract.Collection.Descriptors, new Object[][] { new Object[] {0, null, "+"}, new Object[] {0, null, "+"}});
            AssertCreateStmtNotActive("context InitAndTermCtx select * from SupportBean(TheString = context.sbs0.p00)", new SupportBean("E1", -1));
            AssertCreateStmtNotActive("context InitAndTermCtx select * from SupportBean(TheString = context.sbs0.p00)", new SupportBean("E2", -1));
    
            // assert all inactive
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            Assert.IsFalse(_listener.IsInvoked);
    
            // activate context partition (all)
            GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E1", 12});
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), FIELDS, new Object[] {"E2", 22});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertNestedContextIndividualSelector(ContextPartitionSelector selector)
        {
            SupportContextStateCacheImpl.Reset();
            String contextName = SetUpContextNested();
            String[] fields = "c0,c1,c2".Split(',');
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 1, null, true),
                    new ContextState(2, 1, 1, 0, null, true), new ContextState(2, 1, 2, 1, null, true));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 0, 1 }, GetAllCPIds(_epService, "NestedContext", false));
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 11));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 12));
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 1, null, true),
                    new ContextState(2, 1, 1, 0, null, true), new ContextState(2, 1, 2, 1, null, true),
                    new ContextState(1, 0, 2, 2, null, true),
                    new ContextState(2, 2, 1, 2, null, true), new ContextState(2, 2, 2, 3, null, true));
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 13));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, fields,
                    new Object[][]{new Object[] {"E1", 1, 10L}, new Object[] {"E1", -1, 11L}, new Object[] {"E2", 1, 12L}, new Object[] {"E2", -1, 13L}});
    
            // deactive partition for E2/positive code
            EPContextPartitionExtract extract = GetSpi(_epService).ExtractStopPaths(contextName, selector);
            AssertPathInfo(extract.Collection.Descriptors, new Object[][] { new Object[] {
                    3, MakeIdentNested(MakeIdentPart("E2"), MakeIdentCat("positive")), "+"}});
            SupportContextStateCacheImpl.AssertState(new ContextState(1, 0, 1, 1, null, true),
                    new ContextState(2, 1, 1, 0, null, true), new ContextState(2, 1, 2, 1, null, true),
                    new ContextState(1, 0, 2, 2, null, true),
                    new ContextState(2, 2, 1, 2, null, true), new ContextState(2, 2, 2, 3, null, false));
    
            // assert E2/G2(1) inactive
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 20));
            _epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 21));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 22)); // not used
            _epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 23));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, fields,
                    new Object[][] {new Object[] {"E1", 1, 30L}, new Object[] {"E1", -1, 32L}, new Object[] {"E2", -1, 36L}});
            AssertCreateStmtNotActive("context NestedContext select * from SupportBean", new SupportBean("E2", 10000));
    
            // activate context partition for E2/positive
            GetSpi(_epService).ImportStartPaths(contextName, extract.Importable, new AgentInstanceSelectorAll());
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30));
            _epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 31));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 32));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 33));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened().First, fields,
                    new Object[][] {new Object[] {"E1", 1, 60L}, new Object[] {"E1", -1, 63L}, new Object[] {"E2", 1, 32L}, new Object[] {"E2", -1, 33L}});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private String SetUpContextNested() {
    
            String createCtx = CONTEXT_CACHE_HOOK + "create context NestedContext as " +
                    "context ACtx partition by TheString from SupportBean, " +
                    "context BCtx " +
                    "  group by IntPrimitive < 0 as negative," +
                    "  group by IntPrimitive > 0 as positive from SupportBean";
            _epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("context NestedContext " +
                    "select TheString as c0, IntPrimitive as c1, Sum(LongPrimitive) as c2, context.id as c3 from SupportBean");
            stmt.Events += _listener.Update;
    
            return "NestedContext";
        }
    
        private String SetUpContextHashSegmented() {
    
            String createCtx = CONTEXT_CACHE_HOOK + "create context HashSegByString as coalesce by Consistent_hash_crc32(TheString) from SupportBean granularity 64";
            _epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("context HashSegByString " +
                    "select TheString as c0, Sum(IntPrimitive) as c1, context.id as c2 " +
                    "from SupportBean group by TheString");
            stmt.Events += _listener.Update;
    
            return "HashSegByString";
        }
    
        private String SetUpContextPartitioned() {
    
            String createCtx = CONTEXT_CACHE_HOOK + "create context PartitionByString as partition by TheString from SupportBean";
            _epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("context PartitionByString " +
                    "select TheString as c0, Sum(IntPrimitive) as c1, context.id as c2 " +
                    "from SupportBean");
            stmt.Events += _listener.Update;
    
            return "PartitionByString";
        }
    
        private String SetUpContextCategory() {
    
            String createCtx = CONTEXT_CACHE_HOOK + "create context CategoryContext as " +
                    "group by TheString = 'G1' as G1," +
                    "group by TheString = 'G2' as G2," +
                    "group by TheString = 'G3' as G3 from SupportBean";
            _epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("context CategoryContext " +
                    "select TheString as c0, Sum(IntPrimitive) as c1, context.id as c2 " +
                    "from SupportBean");
            stmt.Events += _listener.Update;
    
            return "CategoryContext";
        }
    
        private Object MakeEvent(String theString, int intPrimitive, long longPrimitive) {
            SupportBean bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        private String SetUpContextInitTerm() {
    
            String createCtx = CONTEXT_CACHE_HOOK + "create context InitAndTermCtx as " +
                    "initiated by SupportBean_S0 sbs0 " +
                    "terminated after 24 hours";
            _epService.EPAdministrator.CreateEPL(createCtx);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("context InitAndTermCtx " +
                    "select TheString as c0, Sum(IntPrimitive) as c1, context.id as c2 " +
                    "from SupportBean(TheString = context.sbs0.p00)");
            stmt.Events += _listener.Update;
    
            return "InitAndTermCtx";
        }
    
        public class MySelectorHashById : ContextPartitionSelectorHash {
    
            private readonly ICollection<int> _hashes;

            public MySelectorHashById(ICollection<int> hashes)
            {
                _hashes = hashes;
            }

            public ICollection<int> Hashes
            {
                get { return _hashes; }
            }
        }
    
        public class MySelectorHashFiltered : ContextPartitionSelectorFiltered {
            private readonly int _hashCode;
    
            public MySelectorHashFiltered(int hashCode) {
                _hashCode = hashCode;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                var hash = (ContextPartitionIdentifierHash) contextPartitionIdentifier;
                return hash.Hash == _hashCode;
            }
        }
    
        public class MySelectorCategoryFiltered : ContextPartitionSelectorFiltered {
            private readonly String _label;
    
            public MySelectorCategoryFiltered(String label) {
                _label = label;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                var cat = (ContextPartitionIdentifierCategory) contextPartitionIdentifier;
                return cat.Label.Equals(_label);
            }
        }
    
        public class MySelectorPartitionFiltered : ContextPartitionSelectorFiltered {
            private readonly Object[] _keys;
    
            public MySelectorPartitionFiltered(Object[] keys) {
                _keys = keys;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                var part = (ContextPartitionIdentifierPartitioned) contextPartitionIdentifier;
                return Collections.AreEqual(part.Keys, _keys);
            }
        }
    
        public class MySelectorInitTermFiltered : ContextPartitionSelectorFiltered
        {
            private readonly String _p00PropertyValue;
    
            public MySelectorInitTermFiltered(String p00PropertyValue)
            {
                _p00PropertyValue = p00PropertyValue;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var id = (ContextPartitionIdentifierInitiatedTerminated) contextPartitionIdentifier;
                var @event = (EventBean) id.Properties.Get("sbs0");
                return _p00PropertyValue.Equals(@event.Get("p00"));
            }
        }
    
        public static void AssertImportsCPids(IDictionary<int, int> received, int[][] expected)
        {
            if (expected == null)
            {
                if (received == null)
                {
                    return;
                }
            }
            else {
                ScopeTestHelper.AssertNotNull(received);
            }
    
            if (expected != null) {
                for (int j = 0; j < expected.Length; j++)
                {
                    int key = expected[j][0];
                    int value = expected[j][1];
                    var receivevalue = received.Get(key);
                    ScopeTestHelper.AssertEquals("Error asserting key '" + key + "'", value, receivevalue);
                }
            }
        }
    
        private static void AssertPathInfo(IDictionary<int, ContextPartitionDescriptor> cpinfo, Object[][] expected)
        {
            Assert.AreEqual(expected.Length, cpinfo.Count);
    
            for (int i = 0; i < expected.Length; i++) {
                var expectedRow = expected[i];
                var message = "failed assertion for item " + i;
                var expectedId = (int) expectedRow[0];
                ContextPartitionDescriptor desc = cpinfo.Get(expectedId);
                AssertPathInfo(message, desc, expectedRow);
            }
        }
    
        private static void AssertPathInfo(String message, ContextPartitionDescriptor desc, Object[] expectedRow)
        { 
            var expectedId = (int) expectedRow[0];
            var expectedIdent = (ContextPartitionIdentifier) expectedRow[1];
            var expectedState = (String) expectedRow[2];

            Assert.AreEqual(desc.AgentInstanceId, expectedId, message);
            if (expectedIdent != null) {
                Assert.IsTrue(expectedIdent.CompareTo(desc.Identifier), message);
            }
            else {
                Assert.IsTrue(desc.Identifier is ContextPartitionIdentifierInitiatedTerminated, message);
            }
    
            ContextPartitionState stateEnum;
            if (expectedState.Equals("+")) {
                stateEnum = ContextPartitionState.STARTED;
            }
            else if (expectedState.Equals("-")) {
                stateEnum = ContextPartitionState.STOPPED;
            }
            else {
                throw new IllegalStateException("Failed to parse expected state '" + expectedState + "' as {+,-}");
            }
            Assert.AreEqual(stateEnum, desc.State, message);
        }
    
        private static IDictionary<int, ContextPartitionDescriptor> GetAllCPDescriptors(EPServiceProvider epService, String contextName, bool nested)
        {
            ContextPartitionSelector selector = ContextPartitionSelectorAll.INSTANCE;
            if (nested) {
                selector = new SupportSelectorNested(ContextPartitionSelectorAll.INSTANCE, ContextPartitionSelectorAll.INSTANCE);
            }
            return GetSpi(epService).GetContextPartitions(contextName, selector).Descriptors;
        }

        private static ISet<int> GetAllCPIds(EPServiceProvider epService, String contextName, bool nested)
        {
            ContextPartitionSelector selector = ContextPartitionSelectorAll.INSTANCE;
            if (nested)
            {
                selector = new SupportSelectorNested(ContextPartitionSelectorAll.INSTANCE, ContextPartitionSelectorAll.INSTANCE);
            }
            return GetSpi(epService).GetContextPartitionIds(contextName, selector);
        }

        private static EPContextPartitionAdminSPI GetSpi(EPServiceProvider epService) {
            return ((EPContextPartitionAdminSPI) epService.EPAdministrator.ContextPartitionAdmin);
        }
    
        private static ContextPartitionIdentifier MakeIdentCat(String label) {
            return new ContextPartitionIdentifierCategory(label);
        }
    
        private static ContextPartitionIdentifier MakeIdentHash(int code) {
            return new ContextPartitionIdentifierHash(code);
        }
    
        private static ContextPartitionIdentifier MakeIdentPart(Object singleKey) {
            return new ContextPartitionIdentifierPartitioned(new Object[] {singleKey});
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
}
