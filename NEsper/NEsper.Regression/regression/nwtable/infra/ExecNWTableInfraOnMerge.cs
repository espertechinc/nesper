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

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    using Map = IDictionary<string, object>;

    public class ExecNWTableInfraOnMerge : RegressionExecution
    {
        private static readonly string NEWLINE = Environment.NewLine;
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_A), typeof(SupportBean_B), typeof(SupportBean_S0), typeof(SupportBean_ST0)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionUpdateNestedEvent(epService, true);
            RunAssertionUpdateNestedEvent(epService, false);
    
            RunAssertionOnMergeInsertStream(epService, true);
            RunAssertionOnMergeInsertStream(epService, false);
    
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                RunAssertionInsertOtherStream(epService, true, rep);
                RunAssertionInsertOtherStream(epService, false, rep);
            }
    
            RunAssertionMultiactionDeleteUpdate(epService, true);
            RunAssertionMultiactionDeleteUpdate(epService, false);
    
            RunAssertionUpdateOrderOfFields(epService, true);
            RunAssertionUpdateOrderOfFields(epService, false);
    
            RunAssertionSubqueryNotMatched(epService, true);
            RunAssertionSubqueryNotMatched(epService, false);
    
            RunAssertionPatternMultimatch(epService, true);
            RunAssertionPatternMultimatch(epService, false);
    
            RunAssertionNoWhereClause(epService, true);
            RunAssertionNoWhereClause(epService, false);
    
            RunAssertionMultipleInsert(epService, true);
            RunAssertionMultipleInsert(epService, false);
    
            RunAssertionFlow(epService, true);
            RunAssertionFlow(epService, false);
    
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                RunAssertionInnerTypeAndVariable(epService, true, rep);
                if (rep != EventRepresentationChoice.AVRO) {
                    RunAssertionInnerTypeAndVariable(epService, false, rep);
                }
            }
    
            RunAssertionInvalid(epService, true);
            RunAssertionInvalid(epService, false);
        }
    
        private void RunAssertionFlow(EPServiceProvider epService, bool namedWindow) {
            var fields = "TheString,IntPrimitive,IntBoxed".Split(',');
            var createEPL = namedWindow ?
                    "@Name('Window') create window MyMergeInfra#unique(TheString) as SupportBean" :
                    "@Name('Window') create table MyMergeInfra (TheString string primary key, IntPrimitive int, IntBoxed int)";
            var createStmt = epService.EPAdministrator.CreateEPL(createEPL);
            var createListener = new SupportUpdateListener();
            createStmt.Events += createListener.Update;
    
            epService.EPAdministrator.CreateEPL("@Name('Insert') insert into MyMergeInfra select TheString, IntPrimitive, IntBoxed from SupportBean(BoolPrimitive)");
            epService.EPAdministrator.CreateEPL("@Name('Delete') on SupportBean_A delete from MyMergeInfra");
    
            var epl = "@Name('Merge') on SupportBean(BoolPrimitive=false) as up " +
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
            var merged = epService.EPAdministrator.CreateEPL(epl);
            var mergeListener = new SupportUpdateListener();
            merged.Events += mergeListener.Update;
    
            RunAssertionFlow(epService, namedWindow, createStmt, fields, createListener, mergeListener);
    
            merged.Dispose();
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            createListener.Reset();
            mergeListener.Reset();
    
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL().Trim());
            merged = epService.EPAdministrator.Create(model);
            Assert.AreEqual(merged.Text.Trim(), model.ToEPL().Trim());
            merged.Events += mergeListener.Update;
    
            RunAssertionFlow(epService, namedWindow, createStmt, fields, createListener, mergeListener);
    
            // test stream wildcard
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            merged.Dispose();
            epl = "on SupportBean(BoolPrimitive = false) as up " +
                    "merge MyMergeInfra as mv " +
                    "where mv.TheString = up.TheString " +
                    "when not matched then " +
                    "insert select " + (namedWindow ? "up.*" : "TheString, IntPrimitive, IntBoxed");
            merged = epService.EPAdministrator.CreateEPL(epl);
            merged.Events += mergeListener.Update;
    
            SendSupportBeanEvent(epService, false, "E99", 2, 3); // insert via merge
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E99", 2, 3}});
    
            // Test ambiguous columns.
            epService.EPAdministrator.CreateEPL("create schema TypeOne (id long, mylong long, mystring long)");
            var eplCreateInfraTwo = namedWindow ?
                    "create window MyInfraTwo#unique(id) as select * from TypeOne" :
                    "create table MyInfraTwo (id long, mylong long, mystring long)";
            epService.EPAdministrator.CreateEPL(eplCreateInfraTwo);
    
            // The "and not matched" should not complain if "mystring" is ambiguous.
            // The "insert" should not complain as column names have been provided.
            epl = "on TypeOne as t1 merge MyInfraTwo nm where nm.id = t1.id\n" +
                    "  when not matched and mystring = 0 then insert select *\n" +
                    "  when not matched then insert (id, mylong, mystring) select 0L, 0L, 0L\n" +
                    " ";
            epService.EPAdministrator.CreateEPL(epl);
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyInfraTwo,TypeOne,MyMergeInfra".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }
    
        private void RunAssertionFlow(EPServiceProvider epService, bool namedWindow, EPStatement createStmt, string[] fields, SupportUpdateListener createListener, SupportUpdateListener mergeListener) {
            createListener.Reset();
            mergeListener.Reset();
    
            SendSupportBeanEvent(epService, true, "E1", 10, 200); // insert via insert-into
            if (namedWindow) {
                EPAssertionUtil.AssertProps(createListener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, 200});
            } else {
                Assert.IsFalse(createListener.IsInvoked);
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 10, 200}});
            Assert.IsFalse(mergeListener.IsInvoked);
    
            SendSupportBeanEvent(epService, false, "E1", 11, 201);    // update via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(createListener.AssertOneGetNew(), fields, new object[]{"E1", 11, 401});
                EPAssertionUtil.AssertProps(createListener.AssertOneGetOld(), fields, new object[]{"E1", 10, 200});
                createListener.Reset();
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 11, 401}});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNew(), fields, new object[]{"E1", 11, 401});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetOld(), fields, new object[]{"E1", 10, 200});
            mergeListener.Reset();
    
            SendSupportBeanEvent(epService, false, "E2", 13, 300); // insert via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(createListener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 13, 300});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 11, 401}, new object[] {"E2", 13, 300}});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 13, 300});
    
            SendSupportBeanEvent(epService, false, "E2", 14, 301); // update via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(createListener.AssertOneGetNew(), fields, new object[]{"E2", 14, 601});
                EPAssertionUtil.AssertProps(createListener.AssertOneGetOld(), fields, new object[]{"E2", 13, 300});
                createListener.Reset();
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 11, 401}, new object[] {"E2", 14, 601}});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNew(), fields, new object[]{"E2", 14, 601});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetOld(), fields, new object[]{"E2", 13, 300});
            mergeListener.Reset();
    
            SendSupportBeanEvent(epService, false, "E2", 15, 302); // update via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(createListener.AssertOneGetNew(), fields, new object[]{"E2", 15, 903});
                EPAssertionUtil.AssertProps(createListener.AssertOneGetOld(), fields, new object[]{"E2", 14, 601});
                createListener.Reset();
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 11, 401}, new object[] {"E2", 15, 903}});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNew(), fields, new object[]{"E2", 15, 903});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetOld(), fields, new object[]{"E2", 14, 601});
            mergeListener.Reset();
    
            SendSupportBeanEvent(epService, false, "E3", 40, 400); // insert via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(createListener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 40, 400});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 11, 401}, new object[] {"E2", 15, 903}, new object[] {"E3", 40, 400}});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 40, 400});
    
            SendSupportBeanEvent(epService, false, "E3", 0, 1000); // reset E3 via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(createListener.AssertOneGetNew(), fields, new object[]{"E3", 0, 0});
                EPAssertionUtil.AssertProps(createListener.AssertOneGetOld(), fields, new object[]{"E3", 40, 400});
                createListener.Reset();
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 11, 401}, new object[] {"E2", 15, 903}, new object[] {"E3", 0, 0}});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNew(), fields, new object[]{"E3", 0, 0});
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetOld(), fields, new object[]{"E3", 40, 400});
            mergeListener.Reset();
    
            SendSupportBeanEvent(epService, false, "E2", -1, 1000); // delete E2 via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(createListener.AssertOneGetOldAndReset(), fields, new object[]{"E2", 15, 903});
            }
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetOldAndReset(), fields, new object[]{"E2", 15, 903});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 11, 401}, new object[] {"E3", 0, 0}});
    
            SendSupportBeanEvent(epService, false, "E1", -1, 1000); // delete E1 via merge
            if (namedWindow) {
                EPAssertionUtil.AssertProps(createListener.AssertOneGetOldAndReset(), fields, new object[]{"E1", 11, 401});
                createListener.Reset();
            }
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetOldAndReset(), fields, new object[]{"E1", 11, 401});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(createStmt.GetEnumerator(), fields, new object[][]{new object[] {"E3", 0, 0}});
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, bool boolPrimitive, string theString, int intPrimitive, int? intBoxed) {
            var theEvent = new SupportBean(theString, intPrimitive);
            theEvent.IntBoxed = intBoxed;
            theEvent.BoolPrimitive = boolPrimitive;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void RunAssertionMultipleInsert(EPServiceProvider epService, bool namedWindow) {
    
            var fields = "col1,col2".Split(',');
            epService.EPAdministrator.CreateEPL("create schema MyEvent as (in1 string, in2 int)");
            epService.EPAdministrator.CreateEPL("create schema MySchema as (col1 string, col2 int)");
            var eplCreate = namedWindow ?
                    "create window MyInfraMI#keepall as MySchema" :
                    "create table MyInfraMI (col1 string primary key, col2 int)";
            epService.EPAdministrator.CreateEPL(eplCreate);
    
            var epl = "on MyEvent " +
                    "merge MyInfraMI " +
                    "where col1=in1 " +
                    "when not matched and in1 like \"A%\" then " +
                    "insert(col1, col2) select in1, in2 " +
                    "when not matched and in1 like \"B%\" then " +
                    "insert select in1 as col1, in2 as col2 " +
                    "when not matched and in1 like \"C%\" then " +
                    "insert select \"Z\" as col1, -1 as col2 " +
                    "when not matched and in1 like \"D%\" then " +
                    "insert select \"x\"||in1||\"x\" as col1, in2*-1 as col2 ";
            var merged = epService.EPAdministrator.CreateEPL(epl);
            var mergeListener = new SupportUpdateListener();
            merged.Events += mergeListener.Update;
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "E1", 0);
            Assert.IsFalse(mergeListener.IsInvoked);
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "A1", 1);
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"A1", 1});
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "B1", 2);
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"B1", 2});
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "C1", 3);
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"Z", -1});
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "D1", 4);
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"xD1x", -4});
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "B1", 2);
            Assert.IsFalse(mergeListener.IsInvoked);
    
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl.Trim(), model.ToEPL().Trim());
            merged = epService.EPAdministrator.Create(model);
            Assert.AreEqual(merged.Text.Trim(), model.ToEPL().Trim());
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyEvent,MySchema,MyInfraMI".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }
    
        private void RunAssertionNoWhereClause(EPServiceProvider epService, bool namedWindow) {
            var fields = "col1,col2".Split(',');
            epService.EPAdministrator.CreateEPL("create schema MyEvent as (in1 string, in2 int)");
            epService.EPAdministrator.CreateEPL("create schema MySchema as (col1 string, col2 int)");
            var eplCreate = namedWindow ?
                    "create window MyInfraNWC#keepall as MySchema" :
                    "create table MyInfraNWC (col1 string, col2 int)";
            var namedWindowStmt = epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyInfraNWC");
    
            var epl = "on MyEvent me " +
                    "merge MyInfraNWC mw " +
                    "when not matched and me.in1 like \"A%\" then " +
                    "insert(col1, col2) select me.in1, me.in2 " +
                    "when not matched and me.in1 like \"B%\" then " +
                    "insert select me.in1 as col1, me.in2 as col2 " +
                    "when matched and me.in1 like \"C%\" then " +
                    "update set col1='Z', col2=-1 " +
                    "when not matched then " +
                    "insert select \"x\" || me.in1 || \"x\" as col1, me.in2 * -1 as col2 ";
            epService.EPAdministrator.CreateEPL(epl);
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "E1", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"xE1x", -2}});
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "A1", 3);   // matched : no where clause
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"xE1x", -2}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("Ax1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "A1", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"A1", 4}});
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "B1", 5);   // matched : no where clause
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"A1", 4}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("Ax1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "B1", 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"B1", 5}});
    
            SendMyEvent(epService, EventRepresentationChoiceExtensions.GetEngineDefault(epService), "C", 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"Z", -1}});
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyEvent,MySchema,MyInfraNWC".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService, bool namedWindow) {
            string epl;
            var eplCreateMergeInfra = namedWindow ?
                    "create window MergeInfra#unique(TheString) as SupportBean" :
                    "create table MergeInfra as (TheString string, IntPrimitive int, BoolPrimitive bool)";
            epService.EPAdministrator.CreateEPL(eplCreateMergeInfra);
            epService.EPAdministrator.CreateEPL("create schema ABCSchema as (val int)");
            var eplCreateABCInfra = namedWindow ?
                    "create window ABCInfra#keepall as ABCSchema" :
                    "create table ABCInfra (val int)";
            epService.EPAdministrator.CreateEPL(eplCreateABCInfra);
    
            epl = "on SupportBean_A merge MergeInfra as windowevent where id = TheString when not matched and Exists(select * from MergeInfra mw where mw.TheString = windowevent.TheString) is not null then insert into ABC select '1'";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: On-Merge not-matched filter expression may not use properties that are provided by the named window event [on SupportBean_A merge MergeInfra as windowevent where id = TheString when not matched and Exists(select * from MergeInfra mw where mw.TheString = windowevent.TheString) is not null then insert into ABC select '1']");
    
            epl = "on SupportBean_A as up merge ABCInfra as mv when not matched then insert (col) select 1";
            if (namedWindow) {
                SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Validation failed in when-not-matched (clause 1): Event type named 'ABCInfra' has already been declared with differing column name or type information: The property 'val' is not provided but required [on SupportBean_A as up merge ABCInfra as mv when not matched then insert (col) select 1]");
            } else {
                SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Validation failed in when-not-matched (clause 1): Column 'col' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [");
            }
    
            epl = "on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then update set IntPrimitive = 1";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Incorrect syntax near 'update' (a reserved keyword) expecting 'insert' but found 'update' at line 1 column 9");
    
            if (namedWindow) {
                epl = "on SupportBean_A as up merge MergeInfra as mv where mv.TheString=id when matched then insert select *";
                SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Validation failed in when-not-matched (clause 1): Expression-returned event type 'SupportBean_A' with underlying type '" + Name.Clean<SupportBean_A>() + "' cannot be converted to target event type 'MergeInfra' with underlying type '" + Name.Clean<SupportBean>() + "' [on SupportBean_A as up merge MergeInfra as mv where mv.TheString=id when matched then insert select *]");
            }
    
            epl = "on SupportBean as up merge MergeInfra as mv";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Unexpected end-of-input at line 1 column 4");
    
            epl = "on SupportBean as up merge MergeInfra as mv where a=b when matched";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Incorrect syntax near end-of-input ('matched' is a reserved keyword) expecting 'then' but found end-of-input at line 1 column 66 [");
    
            epl = "on SupportBean as up merge MergeInfra as mv where a=b when matched and then delete";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Incorrect syntax near 'then' (a reserved keyword) at line 1 column 71 [on SupportBean as up merge MergeInfra as mv where a=b when matched and then delete]");
    
            epl = "on SupportBean as up merge MergeInfra as mv where BoolPrimitive=true when not matched then insert select *";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate where-clause expression 'BoolPrimitive=true': Property named 'BoolPrimitive' is ambiguous as is valid for more then one stream [on SupportBean as up merge MergeInfra as mv where BoolPrimitive=true when not matched then insert select *]");
    
            epl = "on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select IntPrimitive";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'IntPrimitive': Property named 'IntPrimitive' is not valid in any stream [on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select IntPrimitive]");
    
            epl = "on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select * where TheString = 'A'";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to validate match where-clause expression 'TheString=\"A\"': Property named 'TheString' is not valid in any stream [on SupportBean_A as up merge MergeInfra as mv where mv.BoolPrimitive=true when not matched then insert select * where TheString = 'A']");
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "ABCSchema,ABCInfra,MergeInfra".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
    
            // invalid assignment: wrong event type
            epService.EPAdministrator.CreateEPL("create map schema Composite as (c0 int)");
            epService.EPAdministrator.CreateEPL("create window AInfra#keepall as (c Composite)");
            epService.EPAdministrator.CreateEPL("create map schema SomeOther as (c1 int)");
            epService.EPAdministrator.CreateEPL("create map schema MyEvent as (so SomeOther)");
    
            SupportMessageAssertUtil.TryInvalid(epService, "on MyEvent as me update AInfra set c = me.so",
                    "Error starting statement: Invalid assignment to property 'c' event type 'Composite' from event type 'SomeOther' [on MyEvent as me update AInfra set c = me.so]");
        }
    
        private void RunAssertionInnerTypeAndVariable(EPServiceProvider epService, bool namedWindow, EventRepresentationChoice eventRepresentationEnum) {
    
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyInnerSchema(in1 string, in2 int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventSchema(col1 string, col2 MyInnerSchema)");
            var eplCreate = namedWindow ?
                    eventRepresentationEnum.GetAnnotationText() + " create window MyInfraITV#keepall as (c1 string, c2 MyInnerSchema)" :
                    eventRepresentationEnum.GetAnnotationText() + " create table MyInfraITV as (c1 string primary key, c2 MyInnerSchema)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("create variable bool myvar");
    
            var epl = "on MyEventSchema me " +
                    "merge MyInfraITV mw " +
                    "where me.col1 = mw.c1 " +
                    " when not matched and myvar then " +
                    "  insert select col1 as c1, col2 as c2 " +
                    " when not matched and myvar = false then " +
                    "  insert select 'A' as c1, null as c2 " +
                    " when not matched and myvar is null then " +
                    "  insert select 'B' as c1, me.col2 as c2 " +
                    " when matched then " +
                    "  delete";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var mergeListener = new SupportUpdateListener();
            stmt.Events += mergeListener.Update;
            var fields = "c1,c2.in1,c2.in2".Split(',');
    
            SendMyInnerSchemaEvent(epService, eventRepresentationEnum, "X1", "Y1", 10);
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"B", "Y1", 10});
    
            SendMyInnerSchemaEvent(epService, eventRepresentationEnum, "B", "0", 0);    // delete
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetOldAndReset(), fields, new object[]{"B", "Y1", 10});
    
            epService.EPRuntime.SetVariableValue("myvar", true);
            SendMyInnerSchemaEvent(epService, eventRepresentationEnum, "X2", "Y2", 11);
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"X2", "Y2", 11});
    
            epService.EPRuntime.SetVariableValue("myvar", false);
            SendMyInnerSchemaEvent(epService, eventRepresentationEnum, "X3", "Y3", 12);
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"A", null, null});
    
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL(epl);
            var subscriber = new SupportSubscriberMRD();
            stmt.Subscriber = subscriber;
            epService.EPRuntime.SetVariableValue("myvar", true);
    
            SendMyInnerSchemaEvent(epService, eventRepresentationEnum, "X4", "Y4", 11);
            var result = subscriber.InsertStreamList[0];
            if (eventRepresentationEnum.IsObjectArrayEvent() || !namedWindow) {
                var row = (object[]) result[0][0];
                Assert.AreEqual("X4", row[0]);
                var theEvent = (EventBean) row[1];
                Assert.AreEqual("Y4", theEvent.Get("in1"));
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var map = (Map) result[0][0];
                Assert.AreEqual("X4", map.Get("c1"));
                var theEvent = (EventBean) map.Get("c2");
                Assert.AreEqual("Y4", theEvent.Get("in1"));
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var avro = (GenericRecord) result[0][0];
                Assert.AreEqual("X4", avro.Get("c1"));
                var theEvent = (GenericRecord) avro.Get("c2");
                Assert.AreEqual("Y4", theEvent.Get("in1"));
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyInfraITV,MyEventSchema,MyInnerSchema,MyInfraITV,table_MyInfraITV__internal,table_MyInfraITV__public".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionPatternMultimatch(EPServiceProvider epService, bool namedWindow) {
            var fields = "c1,c2".Split(',');
            var eplCreate = namedWindow ?
                    "create window MyInfraPM#keepall as (c1 string, c2 string)" :
                    "create table MyInfraPM as (c1 string primary key, c2 string primary key)";
            var namedWindowStmt = epService.EPAdministrator.CreateEPL(eplCreate);
    
            var epl = "on pattern[every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%', IntPrimitive = a.IntPrimitive)] me " +
                    "merge MyInfraPM mw " +
                    "where me.a.TheString = mw.c1 and me.b.TheString = mw.c2 " +
                    "when not matched then " +
                    "insert select me.a.TheString as c1, me.b.TheString as c2 ";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var mergeListener = new SupportUpdateListener();
            stmt.Events += mergeListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"A1", "B1"}, new object[] {"A2", "B1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 2));
            epService.EPRuntime.SendEvent(new SupportBean("A4", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"A1", "B1"}, new object[] {"A2", "B1"}, new object[] {"A3", "B2"}, new object[] {"A4", "B2"}});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraPM", false);
        }
    
        private void RunAssertionOnMergeInsertStream(EPServiceProvider epService, bool namedWindow) {
            var listenerOne = new SupportUpdateListener();
            var listenerTwo = new SupportUpdateListener();
            var listenerThree = new SupportUpdateListener();
            var listenerFour = new SupportUpdateListener();
    
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
    
            epService.EPAdministrator.CreateEPL("create schema WinOMISSchema as (v1 string, v2 int)");
    
            var eplCreate = namedWindow ?
                    "create window WinOMIS#keepall as WinOMISSchema " :
                    "create table WinOMIS as (v1 string primary key, v2 int)";
            var nmStmt = epService.EPAdministrator.CreateEPL(eplCreate);
            var epl = "on SupportBean_ST0 as st0 merge WinOMIS as win where win.v1=st0.key0 " +
                    "when not matched " +
                    "then insert into StreamOne select * " +
                    "then insert into StreamTwo select st0.id as id, st0.key0 as key0 " +
                    "then insert into StreamThree(id, key0) select st0.id, st0.key0 " +
                    "then insert into StreamFour select id, key0 where key0=\"K2\" " +
                    "then insert into WinOMIS select key0 as v1, p00 as v2";
            epService.EPAdministrator.CreateEPL(epl);
    
            epService.EPAdministrator.CreateEPL("select * from StreamOne").Events += listenerOne.Update;
            epService.EPAdministrator.CreateEPL("select * from StreamTwo").Events += listenerTwo.Update;
            epService.EPAdministrator.CreateEPL("select * from StreamThree").Events += listenerThree.Update;
            epService.EPAdministrator.CreateEPL("select * from StreamFour").Events += listenerFour.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID1", "K1", 1));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), "id,key0".Split(','), new object[]{"ID1", "K1"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), "id,key0".Split(','), new object[]{"ID1", "K1"});
            EPAssertionUtil.AssertProps(listenerThree.AssertOneGetNewAndReset(), "id,key0".Split(','), new object[]{"ID1", "K1"});
            Assert.IsFalse(listenerFour.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID1", "K2", 2));
            EPAssertionUtil.AssertProps(listenerFour.AssertOneGetNewAndReset(), "id,key0".Split(','), new object[]{"ID1", "K2"});
    
            EPAssertionUtil.AssertPropsPerRow(nmStmt.GetEnumerator(), "v1,v2".Split(','), new object[][]{new object[] {"K1", 1}, new object[] {"K2", 2}});
    
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl.Trim(), model.ToEPL().Trim());
            var merged = epService.EPAdministrator.Create(model);
            Assert.AreEqual(merged.Text.Trim(), model.ToEPL().Trim());
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("WinOMIS", false);
        }
    
        private void RunAssertionMultiactionDeleteUpdate(EPServiceProvider epService, bool namedWindow) {
            var eplCreate = namedWindow ?
                    "create window WinMDU#keepall as SupportBean" :
                    "create table WinMDU (TheString string primary key, IntPrimitive int)";
            var nmStmt = epService.EPAdministrator.CreateEPL(eplCreate);
    
            epService.EPAdministrator.CreateEPL("insert into WinMDU select TheString, IntPrimitive from SupportBean");
            var epl = "on SupportBean_ST0 as st0 merge WinMDU as win where st0.key0=win.TheString " +
                    "when matched " +
                    "then delete where IntPrimitive<0 " +
                    "then update set IntPrimitive=st0.p00 where IntPrimitive=3000 or p00=3000 " +
                    "then update set IntPrimitive=999 where IntPrimitive=1000 " +
                    "then delete where IntPrimitive=1000 " +
                    "then update set IntPrimitive=1999 where IntPrimitive=2000 " +
                    "then delete where IntPrimitive=2000 ";
            var eplFormatted = "on SupportBean_ST0 as st0" + NEWLINE +
                    "merge WinMDU as win" + NEWLINE +
                    "where st0.key0=win.TheString" + NEWLINE +
                    "when matched" + NEWLINE +
                    "then delete where IntPrimitive<0" + NEWLINE +
                    "then update set IntPrimitive=st0.p00 where IntPrimitive=3000 or p00=3000" + NEWLINE +
                    "then update set IntPrimitive=999 where IntPrimitive=1000" + NEWLINE +
                    "then delete where IntPrimitive=1000" + NEWLINE +
                    "then update set IntPrimitive=1999 where IntPrimitive=2000" + NEWLINE +
                    "then delete where IntPrimitive=2000";
            epService.EPAdministrator.CreateEPL(epl);
            var fields = "TheString,IntPrimitive".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E1", 0));
            EPAssertionUtil.AssertPropsPerRow(nmStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E2", 0));
            EPAssertionUtil.AssertPropsPerRow(nmStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3000));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E3", 3));
            EPAssertionUtil.AssertPropsPerRow(nmStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E4", 3000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nmStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}, new object[] {"E4", 3000}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 1000));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E5", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nmStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}, new object[] {"E4", 3000}, new object[] {"E5", 999}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 2000));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "E6", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nmStmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}, new object[] {"E4", 3000}, new object[] {"E5", 999}, new object[] {"E6", 1999}});
    
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl.Trim(), model.ToEPL().Trim());
            Assert.AreEqual(eplFormatted.Trim(), model.ToEPL(new EPStatementFormatter(true)));
            var merged = epService.EPAdministrator.Create(model);
            Assert.AreEqual(merged.Text.Trim(), model.ToEPL().Trim());
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("WinMDU", false);
        }
    
        private void RunAssertionSubqueryNotMatched(EPServiceProvider epService, bool namedWindow) {
            var eplCreateOne = namedWindow ?
                    "create window InfraOne#unique(string) (string string, IntPrimitive int)" :
                    "create table InfraOne (string string primary key, IntPrimitive int)";
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplCreateOne);
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
    
            var eplCreateTwo = namedWindow ?
                    "create window InfraTwo#unique(val0) (val0 string, val1 int)" :
                    "create table InfraTwo (val0 string primary key, val1 int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreateTwo);
            epService.EPAdministrator.CreateEPL("insert into InfraTwo select 'W2' as val0, id as val1 from SupportBean_S0");
    
            var epl = "on SupportBean sb merge InfraOne w1 " +
                    "where sb.TheString = w1.string " +
                    "when not matched then insert select 'Y' as string, (select val1 from InfraTwo as w2 where w2.val0 = sb.TheString) as IntPrimitive";
            epService.EPAdministrator.CreateEPL(epl);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(50));  // InfraTwo now has a row {W2, 1}
            epService.EPRuntime.SendEvent(new SupportBean("W2", 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "string,IntPrimitive".Split(','), new object[][]{new object[] {"Y", 50}});
    
            if (namedWindow) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(51));  // InfraTwo now has a row {W2, 1}
                epService.EPRuntime.SendEvent(new SupportBean("W2", 2));
                EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "string,IntPrimitive".Split(','), new object[][]{new object[] {"Y", 51}});
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("InfraOne", false);
            epService.EPAdministrator.Configuration.RemoveEventType("InfraTwo", false);
        }
    
        private void RunAssertionUpdateOrderOfFields(EPServiceProvider epService, bool namedWindow) {
            var eplCreate = namedWindow ?
                    "create window MyInfraUOF#keepall as SupportBean" :
                    "create table MyInfraUOF(TheString string primary key, IntPrimitive int, IntBoxed int, DoublePrimitive double)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfraUOF select TheString, IntPrimitive, IntBoxed, DoublePrimitive from SupportBean");
            var stmt = epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "merge MyInfraUOF as mywin where mywin.TheString = sb.p00 when matched then " +
                    "update set IntPrimitive=id, IntBoxed=mywin.IntPrimitive, DoublePrimitive=initial.IntPrimitive");
            var mergeListener = new SupportUpdateListener();
            stmt.Events += mergeListener.Update;
            var fields = "IntPrimitive,IntBoxed,DoublePrimitive".Split(',');
    
            epService.EPRuntime.SendEvent(MakeSupportBean("E1", 1, 2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "E1"));
            EPAssertionUtil.AssertProps(mergeListener.GetAndResetLastNewData()[0], fields, new object[]{5, 5, 1.0});
    
            epService.EPRuntime.SendEvent(MakeSupportBean("E2", 10, 20));
            epService.EPRuntime.SendEvent(new SupportBean_S0(6, "E2"));
            EPAssertionUtil.AssertProps(mergeListener.GetAndResetLastNewData()[0], fields, new object[]{6, 6, 10.0});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(7, "E1"));
            EPAssertionUtil.AssertProps(mergeListener.GetAndResetLastNewData()[0], fields, new object[]{7, 7, 5.0});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraUOF", false);
        }
    
        private void RunAssertionInsertOtherStream(EPServiceProvider epService, bool namedWindow, EventRepresentationChoice eventRepresentationEnum) {
            var epl = eventRepresentationEnum.GetAnnotationText() + " create schema MyEvent as (name string, value double);\n" +
                    (namedWindow ?
                            eventRepresentationEnum.GetAnnotationText() + " create window MyInfraIOS#unique(name) as MyEvent;\n" :
                            "create table MyInfraIOS (name string primary key, value double primary key);\n"
                    ) +
                    "insert into MyInfraIOS select * from MyEvent;\n" +
                    eventRepresentationEnum.GetAnnotationText() + " create schema InputEvent as (col1 string, col2 double);\n" +
                    "\n" +
                    "on MyEvent as eme\n" +
                    "  merge MyInfraIOS as MyInfraIOS where MyInfraIOS.name = eme.name\n" +
                    "   when matched then\n" +
                    "      insert into OtherStreamOne select eme.name as event_name, MyInfraIOS.value as status\n" +
                    "   when not matched then\n" +
                    "      insert into OtherStreamOne select eme.name as event_name, 0d as status\n" +
                    ";";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl, null, null, null);
            var mergeListener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from OtherStreamOne").Events += mergeListener.Update;
    
            MakeSendNameValueEvent(epService, eventRepresentationEnum, "MyEvent", "name1", 10d);
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), "event_name,status".Split(','), new object[]{"name1", namedWindow ? 0d : 10d});
    
            // for named windows we can send same-value keys now
            if (namedWindow) {
                MakeSendNameValueEvent(epService, eventRepresentationEnum, "MyEvent", "name1", 11d);
                EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), "event_name,status".Split(','), new object[]{"name1", 10d});
    
                MakeSendNameValueEvent(epService, eventRepresentationEnum, "MyEvent", "name1", 12d);
                EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), "event_name,status".Split(','), new object[]{"name1", 11d});
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyEvent", true);
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraIOS", true);
        }
    
        private void MakeSendNameValueEvent(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string typeName, string name, double value) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{name, value}, typeName);
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new Dictionary<string, object>();
                theEvent.Put("name", name);
                theEvent.Put("value", value);
                epService.EPRuntime.SendEvent(theEvent, typeName);
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, typeName).AsRecordSchema());
                record.Put("name", name);
                record.Put("value", value);
                epService.EPRuntime.SendEventAvro(record, typeName);
            } else {
                Assert.Fail();
            }
        }
    
        private void RunAssertionUpdateNestedEvent(EPServiceProvider epService, bool namedWindow) {
            RunUpdateNestedEvent(epService, namedWindow, "map");
            RunUpdateNestedEvent(epService, namedWindow, "objectarray");
        }
    
        private void RunUpdateNestedEvent(EPServiceProvider epService, bool namedWindow, string metaType) {
            var eplTypes =
                    "create " + metaType + " schema Composite as (c0 int);\n" +
                            "create " + metaType + " schema AInfraType as (k string, cflat Composite, carr Composite[]);\n" +
                            (namedWindow ?
                                    "create window AInfra#lastevent as AInfraType;\n" :
                                    "create table AInfra (k string, cflat Composite, carr Composite[]);\n") +
                            "insert into AInfra select TheString as k, null as cflat, null as carr from SupportBean;\n" +
                            "create " + metaType + " schema MyEvent as (cf Composite, ca Composite[]);\n" +
                            "on MyEvent e merge AInfra when matched then update set cflat = e.cf, carr = e.ca";
            var deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplTypes);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            if (metaType.Equals("map")) {
                epService.EPRuntime.SendEvent(MakeNestedMapEvent(), "MyEvent");
            } else {
                epService.EPRuntime.SendEvent(MakeNestedOAEvent(), "MyEvent");
            }
    
            var result = epService.EPRuntime.ExecuteQuery("select cflat.c0 as cf0, carr[0].c0 as ca0, carr[1].c0 as ca1 from AInfra");
            EPAssertionUtil.AssertProps(result.Array[0], "cf0,ca0,ca1".Split(','), new object[]{1, 1, 2});
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
        }
    
        private static IDictionary<string, object> MakeNestedMapEvent() {
            var cf1 = Collections.SingletonDataMap("c0", 1);
            var cf2 = Collections.SingletonDataMap("c0", 2);
            var myEvent = new Dictionary<string, object>();
            myEvent.Put("cf", cf1);
            myEvent.Put("ca", new Map[]{cf1, cf2});
            return myEvent;
        }
    
        private static object[] MakeNestedOAEvent() {
            var cf1 = new object[]{1};
            var cf2 = new object[]{2};
            return new object[]{cf1, new object[]{cf1, cf2}};
        }
    
        private SupportBean MakeSupportBean(string theString, int intPrimitive, double doublePrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    
        private void SendMyInnerSchemaEvent(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string col1, string col2in1, int col2in2) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{col1, new object[]{col2in1, col2in2}}, "MyEventSchema");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var inner = new Dictionary<string, object>();
                inner.Put("in1", col2in1);
                inner.Put("in2", col2in2);
                var theEvent = new Dictionary<string, object>();
                theEvent.Put("col1", col1);
                theEvent.Put("col2", inner);
                epService.EPRuntime.SendEvent(theEvent, "MyEventSchema");
            } else if (eventRepresentationEnum.IsAvroEvent())
            {
                var schema = SupportAvroUtil.GetAvroSchema(epService, "MyEventSchema").AsRecordSchema();
                var innerSchema = schema.GetField("col2").Schema.AsRecordSchema();
                var innerRecord = new GenericRecord(innerSchema);
                innerRecord.Put("in1", col2in1);
                innerRecord.Put("in2", col2in2);
                var record = new GenericRecord(schema);
                record.Put("col1", col1);
                record.Put("col2", innerRecord);
                epService.EPRuntime.SendEventAvro(record, "MyEventSchema");
            } else {
                Assert.Fail();
            }
        }
    
        private void SendMyEvent(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string in1, int in2) {
            var theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("in1", in1);
            theEvent.Put("in2", in2);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "MyEvent");
            } else {
                epService.EPRuntime.SendEvent(theEvent, "MyEvent");
            }
        }
    }
} // end of namespace
