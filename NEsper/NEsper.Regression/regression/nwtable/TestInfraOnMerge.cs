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

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraOnMerge
    {
        private readonly string NEWLINE = Environment.NewLine;
    
        private EPServiceProviderSPI _epService;
        private SupportUpdateListener _mergeListener;
        private SupportUpdateListener _createListener;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _mergeListener = new SupportUpdateListener();
            _createListener = new SupportUpdateListener();
            foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_A), typeof(SupportBean_B), typeof(SupportBean_S0)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _mergeListener = null;
            _createListener = null;
        }
    
        [Test]
        public void TestUpdateNestedEvent() {
            RunAssertionUpdateNestedEvent(true);
            RunAssertionUpdateNestedEvent(false);
    
            // invalid assignment: wrong event type
            _epService.EPAdministrator.CreateEPL("create map schema Composite as (c0 int)");
            _epService.EPAdministrator.CreateEPL("create window AInfra#keepall as (c Composite)");
            _epService.EPAdministrator.CreateEPL("create map schema SomeOther as (c1 int)");
            _epService.EPAdministrator.CreateEPL("create map schema MyEvent as (so SomeOther)");
    
            SupportMessageAssertUtil.TryInvalid(_epService, "on MyEvent as me update AInfra set c = me.so",
                    "Error starting statement: Invalid assignment to property 'c' event type 'Composite' from event type 'SomeOther' [on MyEvent as me update AInfra set c = me.so]");
        }
    
        [Test]
        public void TestInsertOtherStream() {
            RunAssertionInsertOtherStream(true);
            RunAssertionInsertOtherStream(false);
        }
    
        private void RunAssertionInsertOtherStream(bool namedWindow)
        {
            EnumHelper.ForEach<EventRepresentationChoice>(
                rep => RunAssertionInsertOtherStream(namedWindow, rep));
        }
    
        [Test]
        public void TestUpdateOrderOfFields() {
            RunAssertionUpdateOrderOfFields(true);
            RunAssertionUpdateOrderOfFields(false);
        }
    
        [Test]
        public void TestSubqueryNotMatched() {
            RunAssertionSubqueryNotMatched(true);
            RunAssertionSubqueryNotMatched(false);
        }
    
        [Test]
        public void TestMultiactionDeleteUpdate() {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
    
            RunAssertionMultiactionDeleteUpdate(true);
            RunAssertionMultiactionDeleteUpdate(false);
        }
    
        [Test]
        public void TestOnMergeInsertStream() {
            RunAssertionOnMergeInsertStream(true);
            RunAssertionOnMergeInsertStream(false);
        }
    
        [Test]
        public void TestPatternMultimatch() {
            RunAssertionPatternMultimatch(true);
            RunAssertionPatternMultimatch(false);
        }
    
        [Test]
        public void TestInnerTypeAndVariable() {
            EnumHelper.ForEach<EventRepresentationChoice>(
                rep => RunAssertionInnerTypeAndVariable(true, rep));

            RunAssertionInnerTypeAndVariable(false, EventRepresentationChoice.MAP);
            RunAssertionInnerTypeAndVariable(false, EventRepresentationChoice.ARRAY);
            RunAssertionInnerTypeAndVariable(false, EventRepresentationChoice.DEFAULT);
        }
    
        [Test]
        public void TestInvalid() {
            RunAssertionInvalid(true);
            RunAssertionInvalid(false);
        }
    
        [Test]
        public void TestNoWhereClause() {
            RunAssertionNoWhereClause(true);
            RunAssertionNoWhereClause(false);
        }
    
        [Test]
        public void TestMultipleInsert() {
            RunAssertionMultipleInsert(true);
            RunAssertionMultipleInsert(false);
        }
    
        [Test]
        public void TestFlow() {
            RunAssertionFlow(true);
            RunAssertionFlow(false);
        }
    
        private void RunAssertionFlow(bool namedWindow) 
        {
            var fields = "TheString,IntPrimitive,IntBoxed".Split(',');
            var createEPL = namedWindow ?
                    "@Name('Window') create window MyMergeInfra#unique(TheString) as SupportBean" :
                    "@Name('Window') create table MyMergeInfra (TheString string primary key, IntPrimitive int, IntBoxed int)";
            var createStmt = _epService.EPAdministrator.CreateEPL(createEPL);
            createStmt.AddListener(_createListener);
    
            _epService.EPAdministrator.CreateEPL("@Name('Insert') insert into MyMergeInfra select TheString, IntPrimitive, IntBoxed from SupportBean(BoolPrimitive)");
            _epService.EPAdministrator.CreateEPL("@Name('Delete') on SupportBean_A delete from MyMergeInfra");
    
            var epl =  "@Name('Merge') on SupportBean(BoolPrimitive=false) as up " +
                    "merge MyMergeInfra as mv " +
                    "where mv.TheString=up.TheString " +
                    "when matched and up.IntPrimitive<0 then " +
                    "delete " +
                    "when matched and up.IntPrimitive=0 then " +
                    "update set IntPrimitive=0, IntBoxed=0 " +
                    "when matched then " +
                    "update set IntPrimitive=up.IntPrimitive, IntBoxed=up.IntBoxed+mv.IntBoxed " +
                    "when not matched then " +
                    "insert select " + (namedWindow ? "*" : "TheString, IntPrimitive, IntBoxed");
            var merged = _epService.EPAdministrator.CreateEPL(epl);
            merged.AddListener(_mergeListener);
    
            RunAssertionFlow(namedWindow, createStmt, fields);
    
            merged.Dispose();
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _createListener.Reset();
            _mergeListener.Reset();
    
            var model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL().Trim());
            merged = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(merged.Text.Trim(), model.ToEPL().Trim());
            merged.AddListener(_mergeListener);
    
            RunAssertionFlow(namedWindow, createStmt, fields);
    
            // test stream wildcard
            _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            merged.Dispose();
            epl =  "on SupportBean(BoolPrimitive = false) as up " +
                    "merge MyMergeInfra as mv " +
                    "where mv.TheString = up.TheString " +
                    "when not matched then " +
                    "insert select " + (namedWindow ? "up.*" : "TheString, IntPrimitive, IntBoxed");
            merged = _epService.EPAdministrator.CreateEPL(epl);
            merged.AddListener(_mergeListener);
    
            SendSupportBeanEvent(false, "E99", 2, 3); // insert via merge
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E99", 2, 3 } });
    
            // Test ambiguous columns.
            _epService.EPAdministrator.CreateEPL("create schema TypeOne (id long, mylong long, mystring long)");
            var eplCreateInfraTwo = namedWindow ?
                    "create window MyInfraTwo#unique(id) as select * from TypeOne" :
                    "create table MyInfraTwo (id long, mylong long, mystring long)";
            _epService.EPAdministrator.CreateEPL(eplCreateInfraTwo);
    
            // The "and not matched" should not complain if "mystring" is ambiguous.
            // The "insert" should not complain as column names have been provided.
            epl =  "on TypeOne as t1 merge MyInfraTwo nm where nm.id = t1.id\n" +
                    "  when not matched and mystring = 0 then insert select *\n" +
                    "  when not matched then insert (id, mylong, mystring) select 0L, 0L, 0L\n" +
                    " ";
            _epService.EPAdministrator.CreateEPL(epl);
    
            _epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyInfraTwo,TypeOne,MyMergeInfra".Split(',')) {
                _epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }
    
        private void RunAssertionFlow(bool namedWindow, EPStatement createStmt, string[] fields) {
            _createListener.Reset();
            _mergeListener.Reset();
    
            SendSupportBeanEvent(true, "E1", 10, 200); // insert via insert-into
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, 200});
            }
            else {
                Assert.IsFalse(_createListener.IsInvoked);
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 10, 200 } });
            Assert.IsFalse(_mergeListener.IsInvoked);
    
            SendSupportBeanEvent(false, "E1", 11, 201);    // update via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetNew(), fields, new object[]{"E1", 11, 401});
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetOld(), fields, new object[]{"E1", 10, 200});
                _createListener.Reset();
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 11, 401 } });
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNew(), fields, new object[]{"E1", 11, 401});
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetOld(), fields, new object[]{"E1", 10, 200});
            _mergeListener.Reset();
    
            SendSupportBeanEvent(false, "E2", 13, 300); // insert via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 13, 300});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E2", 13, 300 } });
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 13, 300});
    
            SendSupportBeanEvent(false, "E2", 14, 301); // update via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetNew(), fields, new object[]{"E2", 14, 601});
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetOld(), fields, new object[]{"E2", 13, 300});
                _createListener.Reset();
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E2", 14, 601 } });
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNew(), fields, new object[]{"E2", 14, 601});
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetOld(), fields, new object[]{"E2", 13, 300});
            _mergeListener.Reset();
    
            SendSupportBeanEvent(false, "E2", 15, 302); // update via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetNew(), fields, new object[]{"E2", 15, 903});
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetOld(), fields, new object[]{"E2", 14, 601});
                _createListener.Reset();
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E2", 15, 903 } });
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNew(), fields, new object[]{"E2", 15, 903});
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetOld(), fields, new object[]{"E2", 14, 601});
            _mergeListener.Reset();
    
            SendSupportBeanEvent(false, "E3", 40, 400); // insert via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 40, 400});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E2", 15, 903 }, new object[] { "E3", 40, 400 } });
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 40, 400});
    
            SendSupportBeanEvent(false, "E3", 0, 1000); // reset E3 via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetNew(), fields, new object[]{"E3", 0, 0});
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetOld(), fields, new object[]{"E3", 40, 400});
                _createListener.Reset();
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E2", 15, 903 }, new object[] { "E3", 0, 0 } });
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNew(), fields, new object[]{"E3", 0, 0});
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetOld(), fields, new object[]{"E3", 40, 400});
            _mergeListener.Reset();
    
            SendSupportBeanEvent(false, "E2", -1, 1000); // delete E2 via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetOldAndReset(), fields, new object[]{"E2", 15, 903});
            }
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetOldAndReset(), fields, new object[]{"E2", 15, 903});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 11, 401 }, new object[] { "E3", 0, 0 } });
    
            SendSupportBeanEvent(false, "E1", -1, 1000); // delete E1 via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_createListener.AssertOneGetOldAndReset(), fields, new object[]{"E1", 11, 401});
                _createListener.Reset();
            }
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetOldAndReset(), fields, new object[]{"E1", 11, 401});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][] { new object[] { "E3", 0, 0 } });
        }
    
        private void SendSupportBeanEvent(bool boolPrimitive, string theString, int intPrimitive, int? intBoxed) {
            var theEvent = new SupportBean(theString, intPrimitive);
            theEvent.IntBoxed = intBoxed;
            theEvent.BoolPrimitive = boolPrimitive;
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void RunAssertionMultipleInsert(bool namedWindow) {
    
            var fields = "col1,col2".Split(',');
            _epService.EPAdministrator.CreateEPL("create schema MyEvent as (in1 string, in2 int)");
            _epService.EPAdministrator.CreateEPL("create schema MySchema as (col1 string, col2 int)");
            var eplCreate = namedWindow ?
                    "create window MyInfra#keepall as MySchema" :
                    "create table MyInfra (col1 string primary key, col2 int)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
    
            var epl =  "on MyEvent " +
                    "merge MyInfra " +
                    "where col1=in1 " +
                    "when not matched and in1 like \"A%\" then " +
                    "insert(col1, col2) select in1, in2 " +
                    "when not matched and in1 like \"B%\" then " +
                    "insert select in1 as col1, in2 as col2 " +
                    "when not matched and in1 like \"C%\" then " +
                    "insert select \"Z\" as col1, -1 as col2 " +
                    "when not matched and in1 like \"D%\" then " +
                    "insert select \"x\"||in1||\"x\" as col1, in2*-1 as col2 ";
            var merged = _epService.EPAdministrator.CreateEPL(epl);
            merged.AddListener(_mergeListener);

            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "E1", 0);
            Assert.IsFalse(_mergeListener.IsInvoked);

            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "A1", 1);
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"A1", 1});

            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "B1", 2);
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"B1", 2});
    
            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "C1", 3);
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"Z", -1});
    
            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "D1", 4);
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"xD1x", -4});
    
            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "B1", 2);
            Assert.IsFalse(_mergeListener.IsInvoked);
    
            var model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl.Trim(), model.ToEPL().Trim());
            merged = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(merged.Text.Trim(), model.ToEPL().Trim());
    
            _epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyEvent,MySchema,MyInfra".Split(',')) {
                _epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }
    
        private void RunAssertionNoWhereClause(bool namedWindow) 
        {
            var fields = "col1,col2".Split(',');
            _epService.EPAdministrator.CreateEPL("create schema MyEvent as (in1 string, in2 int)");
            _epService.EPAdministrator.CreateEPL("create schema MySchema as (col1 string, col2 int)");
            var eplCreate = namedWindow ?
                    "create window MyInfra#keepall as MySchema" :
                    "create table MyInfra (col1 string, col2 int)";
            var namedWindowStmt = _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyInfra");
    
            var epl =  "on MyEvent me " +
                    "merge MyInfra mw " +
                    "when not matched and me.in1 like \"A%\" then " +
                    "insert(col1, col2) select me.in1, me.in2 " +
                    "when not matched and me.in1 like \"B%\" then " +
                    "insert select me.in1 as col1, me.in2 as col2 " +
                    "when matched and me.in1 like \"C%\" then " +
                    "update set col1='Z', col2=-1 " +
                    "when not matched then " +
                    "insert select \"x\" || me.in1 || \"x\" as col1, me.in2 * -1 as col2 ";
            _epService.EPAdministrator.CreateEPL(epl);
    
            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "E1", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "xE1x", -2 } });
    
            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "A1", 3);   // matched : no where clause
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "xE1x", -2 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("Ax1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "A1", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "A1", 4 } });
    
            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "B1", 5);   // matched : no where clause
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "A1", 4 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("Ax1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "B1", 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "B1", 5 } });
    
            SendMyEvent(EventRepresentationChoiceExtensions.GetEngineDefault(_epService), "C", 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "Z", -1 } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyEvent,MySchema,MyInfra".Split(',')) {
                _epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }
    
        private void RunAssertionInvalid(bool namedWindow) {
            string epl;
            var eplCreateMergeInfra = namedWindow ?
                    "create window MergeInfra#unique(TheString) as SupportBean" :
                    "create table MergeInfra as (TheString string, IntPrimitive int, BoolPrimitive bool)";
            _epService.EPAdministrator.CreateEPL(eplCreateMergeInfra);
            _epService.EPAdministrator.CreateEPL("create schema ABCSchema as (val int)");
            var eplCreateABCInfra = namedWindow ?
                    "create window ABCInfra#keepall as ABCSchema" :
                    "create table ABCInfra (val int)";
            _epService.EPAdministrator.CreateEPL(eplCreateABCInfra);
    
            epl = "on SupportBean_A merge MergeInfra as windowevent where id = TheString when not matched and exists(select * from MergeInfra mw where mw.TheString = windowevent.TheString) is not null then insert into ABC select '1'";
            SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: On-Merge not-matched filter expression may not use properties that are provided by the named window event [on SupportBean_A merge MergeInfra as windowevent where id = TheString when not matched and exists(select * from MergeInfra mw where mw.TheString = windowevent.TheString) is not null then insert into ABC select '1']");
    
            epl = "on SupportBean_A as up merge ABCInfra as mv when not matched then insert (col) select 1";
            if (namedWindow) {
                SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Validation failed in when-not-matched (clause 1): Event type named 'ABCInfra' has already been declared with differing column name or type information: The property 'val' is not provided but required [on SupportBean_A as up merge ABCInfra as mv when not matched then insert (col) select 1]");
            }
            else {
                SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Validation failed in when-not-matched (clause 1): Column 'col' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [");
            }
    
            epl = "on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then update set IntPrimitive = 1";
            SupportMessageAssertUtil.TryInvalid(_epService, epl, "Incorrect syntax near 'update' (a reserved keyword) expecting 'insert' but found 'update' at line 1 column 9");
    
            if (namedWindow) {
                epl = "on SupportBean_A as up merge MergeInfra as mv where mv.TheString=id when matched then insert select *";
                SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Validation failed in when-not-matched (clause 1): Expression-returned event type 'SupportBean_A' with underlying type '" + Name.Of<SupportBean_A>() + "' cannot be converted to target event type 'MergeInfra' with underlying type '" + Name.Of<SupportBean>() + "' [on SupportBean_A as up merge MergeInfra as mv where mv.TheString=id when matched then insert select *]");
            }
    
            epl = "on SupportBean as up merge MergeInfra as mv";
            SupportMessageAssertUtil.TryInvalid(_epService, epl, "Unexpected end-of-input at line 1 column 4");
    
            epl = "on SupportBean as up merge MergeInfra as mv where a=b when matched";
            SupportMessageAssertUtil.TryInvalid(_epService, epl, "Incorrect syntax near end-of-input ('matched' is a reserved keyword) expecting 'then' but found end-of-input at line 1 column 66 [");
    
            epl = "on SupportBean as up merge MergeInfra as mv where a=b when matched and then delete";
            SupportMessageAssertUtil.TryInvalid(_epService, epl, "Incorrect syntax near 'then' (a reserved keyword) at line 1 column 71 [on SupportBean as up merge MergeInfra as mv where a=b when matched and then delete]");
    
            epl = "on SupportBean as up merge MergeInfra as mv where BoolPrimitive=true when not matched then insert select *";
            SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Failed to validate where-clause expression 'BoolPrimitive=true': Property named 'BoolPrimitive' is ambiguous as is valid for more then one stream [on SupportBean as up merge MergeInfra as mv where BoolPrimitive=true when not matched then insert select *]");
    
            epl = "on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select IntPrimitive";
            SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Failed to validate select-clause expression 'IntPrimitive': Property named 'IntPrimitive' is not valid in any stream [on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select IntPrimitive]");
    
            epl = "on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select * where TheString = 'A'";
            SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Failed to validate match where-clause expression 'TheString=\"A\"': Property named 'TheString' is not valid in any stream [on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select * where TheString = 'A']");
    
            _epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "ABCSchema,ABCInfra,MergeInfra".Split(',')) {
                _epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }
    
        private void RunAssertionInnerTypeAndVariable(bool namedWindow, EventRepresentationChoice eventRepresentationEnum) {
    
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyInnerSchema(in1 string, in2 int)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventSchema(col1 string, col2 MyInnerSchema)");
            var eplCreate = namedWindow ?
                    eventRepresentationEnum.GetAnnotationText() + " create window MyInfra#keepall as (c1 string, c2 MyInnerSchema)" :
                    eventRepresentationEnum.GetAnnotationText() + " create table MyInfra as (c1 string primary key, c2 MyInnerSchema)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("create variable boolean myvar");
    
            var epl =  "on MyEventSchema me " +
                    "merge MyInfra mw " +
                    "where me.col1 = mw.c1 " +
                    " when not matched and myvar then " +
                    "  insert select col1 as c1, col2 as c2 " +
                    " when not matched and myvar = false then " +
                    "  insert select 'A' as c1, null as c2 " +
                    " when not matched and myvar is null then " +
                    "  insert select 'B' as c1, me.col2 as c2 " +
                    " when matched then " +
                    "  delete";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_mergeListener);
            var fields = "c1,c2.in1,c2.in2".Split(',');
    
            SendMyInnerSchemaEvent(eventRepresentationEnum, "X1", "Y1", 10);
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"B", "Y1", 10});
    
            SendMyInnerSchemaEvent(eventRepresentationEnum, "B", "0", 0);    // delete
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetOldAndReset(), fields, new object[]{"B", "Y1", 10});
    
            _epService.EPRuntime.SetVariableValue("myvar", true);
            SendMyInnerSchemaEvent(eventRepresentationEnum, "X2", "Y2", 11);
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"X2", "Y2", 11});
    
            _epService.EPRuntime.SetVariableValue("myvar", false);
            SendMyInnerSchemaEvent(eventRepresentationEnum, "X3", "Y3", 12);
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"A", null, null});
    
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            var subscriber = new SupportSubscriberMRD();
            stmt.Subscriber = subscriber;
            _epService.EPRuntime.SetVariableValue("myvar", true);
    
            SendMyInnerSchemaEvent(eventRepresentationEnum, "X4", "Y4", 11);
            object[][] result = subscriber.InsertStreamList[0];
            if (eventRepresentationEnum.IsObjectArrayEvent() || !namedWindow) {
                var row = (object[]) result[0][0];
                Assert.AreEqual("X4", row[0]);
                var theEvent = (EventBean) row[1];
                Assert.AreEqual("Y4", theEvent.Get("in1"));
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                var map = (IDictionary<string, object>) result[0][0];
                Assert.AreEqual("X4", map.Get("c1"));
                var theEvent = (EventBean) map.Get("c2");
                Assert.AreEqual("Y4", theEvent.Get("in1"));
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                GenericRecord avro = (GenericRecord)result[0][0];
                Assert.AreEqual("X4", avro.Get("c1"));
                GenericRecord theEvent = (GenericRecord)avro.Get("c2");
                Assert.AreEqual("Y4", theEvent.Get("in1"));
            }

            _epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyInfra,MyEventSchema,MyInnerSchema,MyInfra,table_MyInfra__internal,table_MyInfra__public".Split(',')) {
                _epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionPatternMultimatch(bool namedWindow) {
            var fields = "c1,c2".Split(',');
            var eplCreate = namedWindow ?
                    "create window MyInfra#keepall as (c1 string, c2 string)" :
                    "create table MyInfra as (c1 string primary key, c2 string primary key)";
            var namedWindowStmt = _epService.EPAdministrator.CreateEPL(eplCreate);
    
            var epl =  "on pattern[every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%', IntPrimitive = a.IntPrimitive)] me " +
                    "merge MyInfra mw " +
                    "where me.a.TheString = mw.c1 and me.b.TheString = mw.c2 " +
                    "when not matched then " +
                    "insert select me.a.TheString as c1, me.b.TheString as c2 ";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_mergeListener);
    
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("B1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "A1", "B1" }, new object[] { "A2", "B1" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("A4", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("B2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "A1", "B1" }, new object[] { "A2", "B1" }, new object[] { "A3", "B2" }, new object[] { "A4", "B2" } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionOnMergeInsertStream(bool namedWindow) {
            var listenerOne = new SupportUpdateListener();
            var listenerTwo = new SupportUpdateListener();
            var listenerThree = new SupportUpdateListener();
            var listenerFour = new SupportUpdateListener();
    
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
    
            _epService.EPAdministrator.CreateEPL("create schema WinSchema as (v1 string, v2 int)");
    
            var eplCreate = namedWindow ?
                    "create window Win#keepall as WinSchema " :
                    "create table Win as (v1 string primary key, v2 int)";
            var nmStmt = _epService.EPAdministrator.CreateEPL(eplCreate);
            var epl = "on SupportBean_ST0 as st0 merge Win as win where win.v1=st0.key0 " +
                    "when not matched " +
                    "then insert into StreamOne select * " +
                    "then insert into StreamTwo select st0.id as id, st0.key0 as key0 " +
                    "then insert into StreamThree(id, key0) select st0.id, st0.key0 " +
                    "then insert into StreamFour select id, key0 where key0=\"K2\" " +
                    "then insert into Win select key0 as v1, p00 as v2";
            _epService.EPAdministrator.CreateEPL(epl);
    
            _epService.EPAdministrator.CreateEPL("select * from StreamOne").AddListener(listenerOne);
            _epService.EPAdministrator.CreateEPL("select * from StreamTwo").AddListener(listenerTwo);
            _epService.EPAdministrator.CreateEPL("select * from StreamThree").AddListener(listenerThree);
            _epService.EPAdministrator.CreateEPL("select * from StreamFour").AddListener(listenerFour);
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID1", "K1", 1));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), "id,key0".Split(','), new object[]{"ID1", "K1"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), "id,key0".Split(','), new object[]{"ID1", "K1"});
            EPAssertionUtil.AssertProps(listenerThree.AssertOneGetNewAndReset(), "id,key0".Split(','), new object[]{"ID1", "K1"});
            Assert.IsFalse(listenerFour.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID1", "K2", 2));
            EPAssertionUtil.AssertProps(listenerFour.AssertOneGetNewAndReset(), "id,key0".Split(','), new object[]{"ID1", "K2"});

            EPAssertionUtil.AssertPropsPerRow(nmStmt.GetEnumerator(), "v1,v2".Split(','), new object[][] { new object[] { "K1", 1 }, new object[] { "K2", 2 } });
    
            var model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl.Trim(), model.ToEPL().Trim());
            var merged = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(merged.Text.Trim(), model.ToEPL().Trim());
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("Win", false);
        }
    
        private void RunAssertionMultiactionDeleteUpdate(bool namedWindow) {
            var eplCreate = namedWindow ?
                    "create window Win#keepall as SupportBean" :
                    "create table Win (TheString string primary key, IntPrimitive int)";
            var nmStmt = _epService.EPAdministrator.CreateEPL(eplCreate);
    
            _epService.EPAdministrator.CreateEPL("insert into Win select TheString, IntPrimitive from SupportBean");
            var epl = "on SupportBean_ST0 as st0 merge Win as win where st0.key0=win.TheString " +
                    "when matched " +
                    "then delete where IntPrimitive<0 " +
                    "then update set IntPrimitive=st0.p00 where IntPrimitive=3000 or p00=3000 " +
                    "then update set IntPrimitive=999 where IntPrimitive=1000 " +
                    "then delete where IntPrimitive=1000 " +
                    "then update set IntPrimitive=1999 where IntPrimitive=2000 " +
                    "then delete where IntPrimitive=2000 ";
            var eplFormatted = "on SupportBean_ST0 as st0" + NEWLINE +
                    "merge Win as win" + NEWLINE +
                    "where st0.key0=win.TheString" + NEWLINE +
                    "when matched" + NEWLINE +
                    "then delete where IntPrimitive<0" + NEWLINE +
                    "then update set IntPrimitive=st0.p00 where IntPrimitive=3000 or p00=3000" + NEWLINE +
                    "then update set IntPrimitive=999 where IntPrimitive=1000" + NEWLINE +
                    "then delete where IntPrimitive=1000" + NEWLINE +
                    "then update set IntPrimitive=1999 where IntPrimitive=2000" + NEWLINE +
                    "then delete where IntPrimitive=2000";
            _epService.EPAdministrator.CreateEPL(epl);
            var fields = "TheString,IntPrimitive".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E1", 0));
            EPAssertionUtil.AssertPropsPerRow(nmStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E2", 0));
            EPAssertionUtil.AssertPropsPerRow(nmStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3000));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E3", 3));
            EPAssertionUtil.AssertPropsPerRow(nmStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E4", 3000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nmStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 }, new object[] { "E4", 3000 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 1000));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E5", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nmStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 }, new object[] { "E4", 3000 }, new object[] { "E5", 999 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 2000));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E6", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nmStmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 }, new object[] { "E4", 3000 }, new object[] { "E5", 999 }, new object[] { "E6", 1999 } });
    
            var model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl.Trim(), model.ToEPL().Trim());
            Assert.AreEqual(eplFormatted.Trim(), model.ToEPL(new EPStatementFormatter(true)));
            var merged = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(merged.Text.Trim(), model.ToEPL().Trim());
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("Win", false);
        }
    
        private void RunAssertionSubqueryNotMatched(bool namedWindow) {
            var eplCreateOne = namedWindow ?
                    "create window InfraOne#unique(string) (string string, IntPrimitive int)" :
                    "create table InfraOne (string string primary key, IntPrimitive int)";
            var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplCreateOne);
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
    
            var eplCreateTwo = namedWindow ?
                    "create window InfraTwo#unique(val0) (val0 string, val1 int)" :
                    "create table InfraTwo (val0 string primary key, val1 int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreateTwo);
            _epService.EPAdministrator.CreateEPL("insert into InfraTwo select 'W2' as val0, id as val1 from SupportBean_S0");
    
            var epl = "on SupportBean sb merge InfraOne w1 " +
                    "where sb.TheString = w1.string " +
                    "when not matched then insert select 'Y' as string, (select val1 from InfraTwo as w2 where w2.val0 = sb.TheString) as IntPrimitive";
            _epService.EPAdministrator.CreateEPL(epl);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(50));  // InfraTwo now has a row {W2, 1}
            _epService.EPRuntime.SendEvent(new SupportBean("W2", 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "string,IntPrimitive".Split(','), new object[][] { new object[] { "Y", 50 } });
    
            if (namedWindow) {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(51));  // InfraTwo now has a row {W2, 1}
                _epService.EPRuntime.SendEvent(new SupportBean("W2", 2));
                EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "string,IntPrimitive".Split(','), new object[][] { new object[] { "Y", 51 } });
            }
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("InfraOne", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("InfraTwo", false);
        }
    
        private void RunAssertionUpdateOrderOfFields(bool namedWindow) {
            var eplCreate = namedWindow ?
                    "create window MyInfra#keepall as SupportBean" :
                    "create table MyInfra(TheString string primary key, IntPrimitive int, IntBoxed int, DoublePrimitive double)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive, IntBoxed, DoublePrimitive from SupportBean");
            var stmt = _epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "merge MyInfra as mywin where mywin.TheString = sb.p00 when matched then " +
                    "update set IntPrimitive=id, IntBoxed=mywin.IntPrimitive, DoublePrimitive=initial.IntPrimitive");
            stmt.AddListener(_mergeListener);
            var fields = "IntPrimitive,IntBoxed,DoublePrimitive".Split(',');
    
            _epService.EPRuntime.SendEvent(MakeSupportBean("E1", 1, 2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "E1"));
            EPAssertionUtil.AssertProps(_mergeListener.GetAndResetLastNewData()[0], fields, new object[]{5, 5, 1.0});
    
            _epService.EPRuntime.SendEvent(MakeSupportBean("E2", 10, 20));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(6, "E2"));
            EPAssertionUtil.AssertProps(_mergeListener.GetAndResetLastNewData()[0], fields, new object[] { 6, 6, 10.0 });
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(7, "E1"));
            EPAssertionUtil.AssertProps(_mergeListener.GetAndResetLastNewData()[0], fields, new object[] { 7, 7, 5.0 });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionInsertOtherStream(bool namedWindow, EventRepresentationChoice eventRepresentationEnum) {
            var epl = eventRepresentationEnum.GetAnnotationText() + " create schema MyEvent as (name string, value double);\n" +
                    (namedWindow ? 
                        eventRepresentationEnum.GetAnnotationText() + " create window MyInfra#unique(name) as MyEvent;\n":
                        "create table MyInfra (name string primary key, value double primary key);\n"
                    ) +
                    "insert into MyInfra select * from MyEvent;\n" +
                    eventRepresentationEnum.GetAnnotationText() + " create schema InputEvent as (col1 string, col2 double);\n" +
                    "\n" +
                    "on MyEvent as eme\n" +
                    "  merge MyInfra as MyInfra where MyInfra.name = eme.name\n" +
                    "   when matched then\n" +
                    "      insert into OtherStreamOne select eme.name as event_name, MyInfra.value as status\n" +
                    "   when not matched then\n" +
                    "      insert into OtherStreamOne select eme.name as event_name, 0d as status\n" +
                    ";";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl, null, null, null);
            _epService.EPAdministrator.CreateEPL("select * from OtherStreamOne").AddListener(_mergeListener);
    
            MakeSendNameValueEvent(_epService, eventRepresentationEnum, "MyEvent", "name1", 10d);
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), "event_name,status".Split(','), new object[]{"name1", namedWindow ? 0d : 10d});
    
            // for named windows we can send same-value keys now
            if (namedWindow) {
                MakeSendNameValueEvent(_epService, eventRepresentationEnum, "MyEvent", "name1", 11d);
                EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), "event_name,status".Split(','), new object[]{"name1", 10d});
    
                MakeSendNameValueEvent(_epService, eventRepresentationEnum, "MyEvent", "name1", 12d);
                EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), "event_name,status".Split(','), new object[]{"name1", 11d});
            }
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyEvent", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", true);
        }

        private void MakeSendNameValueEvent(EPServiceProvider engine, EventRepresentationChoice eventRepresentationEnum, string typeName, string name, double value)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                engine.EPRuntime.SendEvent(new Object[] { name, value }, typeName);
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                var theEvent = new Dictionary<string, Object>();
                theEvent.Put("name", name);
                theEvent.Put("value", value);
                engine.EPRuntime.SendEvent(theEvent, typeName);
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var record = new GenericRecord(SupportAvroUtil.GetAvroSchema(_epService, typeName).AsRecordSchema());
                record.Put("name", name);
                record.Put("value", value);
                engine.EPRuntime.SendEventAvro(record, typeName);
            }
            else
            {
                Assert.Fail();
            }
        }

        private void RunAssertionUpdateNestedEvent(bool namedWindow) {
            RunUpdateNestedEvent(namedWindow, "map");
            RunUpdateNestedEvent(namedWindow, "objectarray");
        }
    
        private void RunUpdateNestedEvent(bool namedWindow, string metaType) {
            var eplTypes =
                    "create " + metaType + " schema Composite as (c0 int);\n" +
                    "create " + metaType + " schema AInfraType as (k string, cflat Composite, carr Composite[]);\n" +
                    (namedWindow ?
                            "create window AInfra#lastevent as AInfraType;\n":
                            "create table AInfra (k string, cflat Composite, carr Composite[]);\n") +
                    "insert into AInfra select TheString as k, null as cflat, null as carr from SupportBean;\n" +
                    "create " + metaType + " schema MyEvent as (cf Composite, ca Composite[]);\n" +
                    "on MyEvent e merge AInfra when matched then update set cflat = e.cf, carr = e.ca";
            var deployed = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplTypes);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            if (metaType.Equals("map")) {
                _epService.EPRuntime.SendEvent(MakeNestedMapEvent(), "MyEvent");
            }
            else {
                _epService.EPRuntime.SendEvent(MakeNestedOAEvent(), "MyEvent");
            }
    
            var result = _epService.EPRuntime.ExecuteQuery("select cflat.c0 as cf0, carr[0].c0 as ca0, carr[1].c0 as ca1 from AInfra");
            EPAssertionUtil.AssertProps(result.Array[0], "cf0,ca0,ca1".Split(','), new object[]{1, 1, 2});
    
            _epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
        }
    
        private static IDictionary<String, object> MakeNestedMapEvent() {
            IDictionary<String, object> cf1 = Collections.SingletonMap<string, object>("c0", 1);
            IDictionary<String, object> cf2 = Collections.SingletonMap<string, object>("c0", 2);
            IDictionary<String, object> myEvent = new Dictionary<string, object>();
            myEvent.Put("cf", cf1);
            myEvent.Put("ca", new IDictionary<string, object>[] {cf1, cf2});
            return myEvent;
        }
    
        private static object[] MakeNestedOAEvent() {
            var cf1 = new object[] {1};
            var cf2 = new object[] {2};
            return new object[] {cf1, new object[] {cf1, cf2}};
        }
    
        private SupportBean MakeSupportBean(string theString, int intPrimitive, double doublePrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    
        private void SendMyInnerSchemaEvent(EventRepresentationChoice eventRepresentationEnum, string col1, string col2in1, int col2in2) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(new object[] {col1, new object[] {col2in1, col2in2}}, "MyEventSchema");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<String, object> inner = new Dictionary<string, object>();
                inner.Put("in1", col2in1);
                inner.Put("in2", col2in2);
                IDictionary<String, object> theEvent = new Dictionary<string, object>();
                theEvent.Put("col1", col1);
                theEvent.Put("col2", inner);
                _epService.EPRuntime.SendEvent(theEvent, "MyEventSchema");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var schema = SupportAvroUtil.GetAvroSchema(_epService, "MyEventSchema").AsRecordSchema();
                var innerSchema = schema.GetField("col2").Schema.AsRecordSchema();
                GenericRecord innerRecord = new GenericRecord(innerSchema);
                innerRecord.Put("in1", col2in1);
                innerRecord.Put("in2", col2in2);
                GenericRecord record = new GenericRecord(schema);
                record.Put("col1", col1);
                record.Put("col2", innerRecord);
                _epService.EPRuntime.SendEventAvro(record, "MyEventSchema");
            }
            else
            {
                Assert.Fail();
            }
        }
    
        private void SendMyEvent(EventRepresentationChoice eventRepresentationEnum, string in1, int in2) {
            IDictionary<String, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("in1", in1);
            theEvent.Put("in2", in2);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "MyEvent");
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, "MyEvent");
            }
        }
    }
}
