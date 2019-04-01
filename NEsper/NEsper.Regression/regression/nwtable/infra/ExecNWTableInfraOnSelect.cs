///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

namespace com.espertech.esper.regression.nwtable.infra {
    using Map = IDictionary<string, object>;

    public class ExecNWTableInfraOnSelect : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }

        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));

            RunAssertionOnSelectIndexChoice(epService, true);
            RunAssertionOnSelectIndexChoice(epService, false);

            RunAssertionWindowAgg(epService, true);
            RunAssertionWindowAgg(epService, false);

            RunAssertionSelectAggregationHavingStreamWildcard(epService, true);
            RunAssertionSelectAggregationHavingStreamWildcard(epService, false);

            RunAssertionPatternTimedSelect(epService, true);
            RunAssertionPatternTimedSelect(epService, false);

            RunAssertionInvalid(epService, true);
            RunAssertionInvalid(epService, false);

            RunAssertionSelectCondition(epService, true);
            epService.Initialize(); // BUG: Incompatible event types used
            RunAssertionSelectCondition(epService, false);

            RunAssertionSelectJoinColumnsLimit(epService, true);
            RunAssertionSelectJoinColumnsLimit(epService, false);

            RunAssertionSelectAggregation(epService, true);
            RunAssertionSelectAggregation(epService, false);

            RunAssertionSelectAggregationCorrelated(epService, true);
            RunAssertionSelectAggregationCorrelated(epService, false);

            RunAssertionSelectAggregationGrouping(epService, true);
            RunAssertionSelectAggregationGrouping(epService, false);

            RunAssertionSelectCorrelationDelete(epService, true);
            epService.Initialize(); // BUG: Incompatible event types used
            RunAssertionSelectCorrelationDelete(epService, false);

            RunAssertionPatternCorrelation(epService, true);
            RunAssertionPatternCorrelation(epService, false);
        }

        private void RunAssertionPatternCorrelation(EPServiceProvider epService, bool namedWindow) {
            var fields = new[] {"a", "b"};

            // create window
            var stmtTextCreate = namedWindow
                ? "create window MyInfraPC#keepall as select TheString as a, IntPrimitive as b from " +
                  typeof(SupportBean).FullName
                : "create table MyInfraPC(a string primary key, b int primary key)";
            var stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);

            // create select stmt
            var stmtTextSelect = "on pattern [every ea=" + typeof(SupportBean_A).FullName +
                                 " or every eb=" + typeof(SupportBean_B).FullName +
                                 "] select mywin.* from MyInfraPC as mywin where a = coalesce(ea.id, eb.id)";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;

            // create insert into
            var stmtTextInsertOne = "insert into MyInfraPC select TheString as a, IntPrimitive as b from " +
                                    typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

            // send 3 event
            SendSupportBean(epService, "E1", 1);
            SendSupportBean(epService, "E2", 2);
            SendSupportBean(epService, "E3", 3);
            Assert.IsFalse(listenerSelect.IsInvoked);

            // fire trigger
            SendSupportBean_A(epService, "X1");
            Assert.IsFalse(listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, null);
            }

            SendSupportBean_B(epService, "E2");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {"E2", 2});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {"E2", 2}});
            }

            SendSupportBean_A(epService, "E1");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {"E1", 1}});
            }

            SendSupportBean_B(epService, "E3");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {"E3", 3});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {"E3", 3}});
            }

            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});

            stmtCreate.Dispose();
            stmtSelect.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraPC", false);
        }

        private void RunAssertionSelectCorrelationDelete(EPServiceProvider epService, bool namedWindow) {
            var fields = new[] {"a", "b"};

            // create window
            var stmtTextCreate = namedWindow
                ? "create window MyInfraSCD#keepall as select TheString as a, IntPrimitive as b from " +
                  typeof(SupportBean).FullName
                : "create table MyInfraSCD(a string primary key, b int primary key)";
            var stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);

            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName +
                                 " select mywin.* from MyInfraSCD as mywin where id = a";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;

            // create insert into
            var stmtTextInsertOne = "insert into MyInfraSCD select TheString as a, IntPrimitive as b from " +
                                    typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportBean_B).FullName + " delete from MyInfraSCD where a = id";
            var stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);

            // send 3 event
            SendSupportBean(epService, "E1", 1);
            SendSupportBean(epService, "E2", 2);
            SendSupportBean(epService, "E3", 3);
            Assert.IsFalse(listenerSelect.IsInvoked);

            // fire trigger
            SendSupportBean_A(epService, "X1");
            Assert.IsFalse(listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, null);
            }

            SendSupportBean_A(epService, "E2");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {"E2", 2});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {"E2", 2}});
            }

            SendSupportBean_A(epService, "E1");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {"E1", 1}});
            }

            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});

            // delete event
            SendSupportBean_B(epService, "E1");
            Assert.IsFalse(listenerSelect.IsInvoked);

            SendSupportBean_A(epService, "E1");
            Assert.IsFalse(listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields, new object[][] {new object[] {"E2", 2}, new object[] {"E3", 3}});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, null);
            }

            SendSupportBean_A(epService, "E2");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {"E2", 2});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {"E2", 2}});
            }

            stmtSelect.Dispose();
            stmtDelete.Dispose();
            stmtCreate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraSCD", false);
        }

        private void RunAssertionSelectAggregationGrouping(EPServiceProvider epService, bool namedWindow) {
            var fields = new[] {"a", "sumb"};
            var listenerSelectTwo = new SupportUpdateListener();

            // create window
            var stmtTextCreate = namedWindow
                ? "create window MyInfraSAG#keepall as select TheString as a, IntPrimitive as b from " +
                  typeof(SupportBean).FullName
                : "create table MyInfraSAG(a string primary key, b int primary key)";
            var stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);

            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName +
                                 " select a, sum(b) as sumb from MyInfraSAG group by a order by a desc";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;

            // create select stmt
            var stmtTextSelectTwo = "on " + typeof(SupportBean_A).FullName +
                                    " select a, sum(b) as sumb from MyInfraSAG group by a having sum(b) > 5 order by a desc";
            var stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            stmtSelectTwo.Events += listenerSelectTwo.Update;

            // create insert into
            var stmtTextInsertOne = "insert into MyInfraSAG select TheString as a, IntPrimitive as b from " +
                                    typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

            // fire trigger
            SendSupportBean_A(epService, "A1");
            Assert.IsFalse(listenerSelect.IsInvoked);
            Assert.IsFalse(listenerSelectTwo.IsInvoked);

            // send 3 events
            SendSupportBean(epService, "E1", 1);
            SendSupportBean(epService, "E2", 2);
            SendSupportBean(epService, "E1", 5);
            Assert.IsFalse(listenerSelect.IsInvoked);
            Assert.IsFalse(listenerSelectTwo.IsInvoked);

            // fire trigger
            SendSupportBean_A(epService, "A1");
            EPAssertionUtil.AssertPropsPerRow(
                listenerSelect.LastNewData, fields, new object[][] {new object[] {"E2", 2}, new object[] {"E1", 6}});
            Assert.IsNull(listenerSelect.LastOldData);
            listenerSelect.Reset();
            EPAssertionUtil.AssertPropsPerRow(listenerSelectTwo.LastNewData, fields, new object[][] {new object[] {"E1", 6}});
            Assert.IsNull(listenerSelect.LastOldData);
            listenerSelect.Reset();

            // send 3 events
            SendSupportBean(epService, "E4", -1);
            SendSupportBean(epService, "E2", 10);
            SendSupportBean(epService, "E1", 100);
            Assert.IsFalse(listenerSelect.IsInvoked);

            SendSupportBean_A(epService, "A2");
            EPAssertionUtil.AssertPropsPerRow(
                listenerSelect.LastNewData, fields, new object[][] {new object[] {"E4", -1}, new object[] {"E2", 12}, new object[] {"E1", 106}});

            // create delete stmt, delete E2
            var stmtTextDelete = "on " + typeof(SupportBean_B).FullName + " delete from MyInfraSAG where id = a";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
            SendSupportBean_B(epService, "E2");

            SendSupportBean_A(epService, "A3");
            EPAssertionUtil.AssertPropsPerRow(
                listenerSelect.LastNewData, fields, new object[][] {new object[] {"E4", -1}, new object[] {"E1", 106}});
            Assert.IsNull(listenerSelect.LastOldData);
            listenerSelect.Reset();
            EPAssertionUtil.AssertPropsPerRow(listenerSelectTwo.LastNewData, fields, new object[][] {new object[] {"E1", 106}});
            Assert.IsNull(listenerSelectTwo.LastOldData);
            listenerSelectTwo.Reset();

            var resultType = stmtSelect.EventType;
            Assert.AreEqual(2, resultType.PropertyNames.Length);
            Assert.AreEqual(typeof(string), resultType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), resultType.GetPropertyType("sumb"));

            stmtSelect.Dispose();
            stmtCreate.Dispose();
            stmtSelectTwo.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraSAG", false);
        }

        private void RunAssertionSelectAggregationCorrelated(EPServiceProvider epService, bool namedWindow) {
            var fields = new[] {"sumb"};

            // create window
            var stmtTextCreate = namedWindow
                ? "create window MyInfraSAC#keepall as select TheString as a, IntPrimitive as b from " +
                  typeof(SupportBean).FullName
                : "create table MyInfraSAC(a string primary key, b int primary key)";
            var stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);

            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName +
                                 " select sum(b) as sumb from MyInfraSAC where a = id";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;

            // create insert into
            var stmtTextInsertOne = "insert into MyInfraSAC select TheString as a, IntPrimitive as b from " +
                                    typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

            // send 3 event
            SendSupportBean(epService, "E1", 1);
            SendSupportBean(epService, "E2", 2);
            SendSupportBean(epService, "E3", 3);
            Assert.IsFalse(listenerSelect.IsInvoked);

            // fire trigger
            SendSupportBean_A(epService, "A1");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {null});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {null}});
            }

            // fire trigger
            SendSupportBean_A(epService, "E2");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {2});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {2}});
            }

            SendSupportBean(epService, "E2", 10);
            SendSupportBean_A(epService, "E2");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {12});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {12}});
            }

            var resultType = stmtSelect.EventType;
            Assert.AreEqual(1, resultType.PropertyNames.Length);
            Assert.AreEqual(typeof(int), resultType.GetPropertyType("sumb"));

            stmtSelect.Dispose();
            stmtCreate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraSAC", false);
        }

        private void RunAssertionSelectAggregation(EPServiceProvider epService, bool namedWindow) {
            var fields = new[] {"sumb"};

            // create window
            var stmtTextCreate = namedWindow
                ? "create window MyInfraSA#keepall as select TheString as a, IntPrimitive as b from " +
                  typeof(SupportBean).FullName
                : "create table MyInfraSA (a string primary key, b int primary key)";
            var stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);

            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " select sum(b) as sumb from MyInfraSA";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;

            // create insert into
            var stmtTextInsertOne = "insert into MyInfraSA select TheString as a, IntPrimitive as b from " +
                                    typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

            // send 3 event
            SendSupportBean(epService, "E1", 1);
            SendSupportBean(epService, "E2", 2);
            SendSupportBean(epService, "E3", 3);
            Assert.IsFalse(listenerSelect.IsInvoked);

            // fire trigger
            SendSupportBean_A(epService, "A1");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {6});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {6}});
            }

            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportBean_B).FullName + " delete from MyInfraSA where id = a";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);

            // Delete E2
            SendSupportBean_B(epService, "E2");

            // fire trigger
            SendSupportBean_A(epService, "A2");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {
                4
            });
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {4}});
            }

            SendSupportBean(epService, "E4", 10);
            SendSupportBean_A(epService, "A3");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {14});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {14}});
            }

            var resultType = stmtSelect.EventType;
            Assert.AreEqual(1, resultType.PropertyNames.Length);
            Assert.AreEqual(typeof(int), resultType.GetPropertyType("sumb"));

            stmtSelect.Dispose();
            stmtCreate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraSA", false);
        }

        private void RunAssertionSelectJoinColumnsLimit(EPServiceProvider epService, bool namedWindow) {
            var fields = new[] {"triggerid", "wina", "b"};

            // create window
            var stmtTextCreate = namedWindow
                ? "create window MyInfraSA#keepall as select TheString as a, IntPrimitive as b from " +
                  typeof(SupportBean).FullName
                : "create table MyInfraSA (a string primary key, b int)";
            var stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);

            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName +
                                 " as trigger select trigger.id as triggerid, win.a as wina, b from MyInfraSA as win order by wina";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;

            // create insert into
            var stmtTextInsertOne = "insert into MyInfraSA select TheString as a, IntPrimitive as b from " +
                                    typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

            // send 3 event
            SendSupportBean(epService, "E1", 1);
            SendSupportBean(epService, "E2", 2);
            Assert.IsFalse(listenerSelect.IsInvoked);

            // fire trigger
            SendSupportBean_A(epService, "A1");
            Assert.AreEqual(2, listenerSelect.LastNewData.Length);
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fields, new object[] {"A1", "E1", 1});
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[1], fields, new object[] {"A1", "E2", 2});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(
                    stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {"A1", "E1", 1}, new object[] {"A1", "E2", 2}});
            }

            // try limit clause
            stmtSelect.Dispose();
            stmtTextSelect = "on " + typeof(SupportBean_A).FullName +
                             " as trigger select trigger.id as triggerid, win.a as wina, b from MyInfraSA as win order by wina limit 1";
            stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.Events += listenerSelect.Update;

            SendSupportBean_A(epService, "A1");
            Assert.AreEqual(1, listenerSelect.LastNewData.Length);
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fields, new object[] {"A1", "E1", 1});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] {new object[] {"A1", "E1", 1}});
            }

            stmtCreate.Dispose();
            listenerSelect.Reset();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraSA", false);
        }

        private void RunAssertionSelectCondition(EPServiceProvider epService, bool namedWindow) {
            var fieldsCreate = new[] {"a", "b"};
            var fieldsOnSelect = new[] {"a", "b", "id"};

            // create window
            var stmtTextCreate = namedWindow
                ? "create window MyInfraSC#keepall as select TheString as a, IntPrimitive as b from " +
                  typeof(SupportBean).FullName
                : "create table MyInfraSC (a string primary key, b int)";
            var stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);

            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName +
                                 " select mywin.*, id from MyInfraSC as mywin where MyInfraSC.b < 3 order by a asc";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;
            Assert.AreEqual(StatementType.ON_SELECT, ((EPStatementSPI) stmtSelect).StatementMetadata.StatementType);

            // create insert into
            var stmtTextInsertOne = "insert into MyInfraSC select TheString as a, IntPrimitive as b from " +
                                    typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

            // send 3 event
            SendSupportBean(epService, "E1", 1);
            SendSupportBean(epService, "E2", 2);
            SendSupportBean(epService, "E3", 3);
            Assert.IsFalse(listenerSelect.IsInvoked);

            // fire trigger
            SendSupportBean_A(epService, "A1");
            Assert.AreEqual(2, listenerSelect.LastNewData.Length);
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fieldsCreate, new object[] {"E1", 1});
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[1], fieldsCreate, new object[] {"E2", 2});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fieldsCreate, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(
                    stmtSelect.GetEnumerator(), fieldsOnSelect, new object[][] {new object[] {"E1", 1, "A1"}, new object[] {"E2", 2, "A1"}});
            }
            else {
                Assert.IsFalse(stmtSelect.HasFirst());
            }

            SendSupportBean(epService, "E4", 0);
            SendSupportBean_A(epService, "A2");
            Assert.AreEqual(3, listenerSelect.LastNewData.Length);
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fieldsOnSelect, new object[] {"E1", 1, "A2"});
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[1], fieldsOnSelect, new object[] {"E2", 2, "A2"});
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[2], fieldsOnSelect, new object[] {"E4", 0, "A2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fieldsCreate, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}, new object[] {"E4", 0}});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(
                    stmtSelect.GetEnumerator(), fieldsCreate, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E4", 0}});
            }

            stmtSelect.Dispose();
            stmtCreate.Dispose();
            listenerSelect.Reset();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraSC", false);
        }

        private void RunAssertionInvalid(EPServiceProvider epService, bool namedWindow) {
            var stmtTextCreate = namedWindow
                ? "create window MyInfraInvalid#keepall as select * from " + typeof(SupportBean).FullName
                : "create table MyInfraInvalid (TheString string, IntPrimitive int)";
            epService.EPAdministrator.CreateEPL(stmtTextCreate);

            SupportMessageAssertUtil.TryInvalid(
                epService,
                "on " + typeof(SupportBean_A).FullName + " select * from MyInfraInvalid where sum(IntPrimitive) > 100",
                "Error validating expression: An aggregate function may not appear in a WHERE clause (use the HAVING clause) [");

            SupportMessageAssertUtil.TryInvalid(
                epService, "on " + typeof(SupportBean_A).FullName + " insert into MyStream select * from DUMMY",
                "Named window or table 'DUMMY' has not been declared [");

            SupportMessageAssertUtil.TryInvalid(
                epService, "on " + typeof(SupportBean_A).FullName + " select prev(1, TheString) from MyInfraInvalid",
                "Error starting statement: Failed to validate select-clause expression 'prev(1,TheString)': Previous function cannot be used in this context [");

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraInvalid", false);
        }

        private void RunAssertionPatternTimedSelect(EPServiceProvider epService, bool namedWindow) {
            // test for JIRA ESPER-332
            SendTimer(0, epService);

            var stmtTextCreate = namedWindow
                ? "create window MyInfraPTS#keepall as select * from " + typeof(SupportBean).FullName
                : "create table MyInfraPTS as (TheString string)";
            epService.EPAdministrator.CreateEPL(stmtTextCreate);

            var stmtCount = "on pattern[every timer:interval(10 sec)] select count(eve), eve from MyInfraPTS as eve";
            epService.EPAdministrator.CreateEPL(stmtCount);

            var stmtTextOnSelect =
                "on pattern [ every timer:interval(10 sec)] select TheString from MyInfraPTS having count(TheString) > 0";
            var stmt = epService.EPAdministrator.CreateEPL(stmtTextOnSelect);
            var listenerSelect = new SupportUpdateListener();
            stmt.Events += listenerSelect.Update;

            var stmtTextInsertOne = namedWindow
                ? "insert into MyInfraPTS select * from " + typeof(SupportBean).FullName
                : "insert into MyInfraPTS select TheString from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

            SendTimer(11000, epService);
            Assert.IsFalse(listenerSelect.IsInvoked);

            SendTimer(21000, epService);
            Assert.IsFalse(listenerSelect.IsInvoked);

            SendSupportBean(epService, "E1", 1);
            SendTimer(31000, epService);
            Assert.AreEqual("E1", listenerSelect.AssertOneGetNewAndReset().Get("TheString"));

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraPTS", false);
        }

        private void RunAssertionSelectAggregationHavingStreamWildcard(EPServiceProvider epService, bool namedWindow) {
            // create window
            var stmtTextCreate = namedWindow
                ? "create window MyInfraSHS#keepall as (a string, b int)"
                : "create table MyInfraSHS as (a string primary key, b int primary key)";
            epService.EPAdministrator.CreateEPL(stmtTextCreate);

            var stmtTextInsertOne = "insert into MyInfraSHS select TheString as a, IntPrimitive as b from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

            var stmtTextSelect =
                "on SupportBean_A select mwc.* as mwcwin from MyInfraSHS mwc where id = a group by a having sum(b) = 20";
            var select = (EPStatementSPI) epService.EPAdministrator.CreateEPL(stmtTextSelect);
            Assert.IsFalse(select.StatementContext.IsStatelessSelect);
            var listenerSelect = new SupportUpdateListener();
            select.Events += listenerSelect.Update;

            // send 3 event
            SendSupportBean(epService, "E1", 16);
            SendSupportBean(epService, "E2", 2);
            SendSupportBean(epService, "E1", 4);

            // fire trigger
            SendSupportBean_A(epService, "E1");
            var events = listenerSelect.LastNewData;
            Assert.AreEqual(2, events.Length);
            Assert.AreEqual("E1", events[0].Get("mwcwin.a"));
            Assert.AreEqual("E1", events[1].Get("mwcwin.a"));

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraSHS", false);
        }

        private void RunAssertionWindowAgg(EPServiceProvider epService, bool namedWindow) {
            var eplCreate = namedWindow
                ? "create window MyInfraWA#keepall as SupportBean"
                : "create table MyInfraWA(TheString string primary key, IntPrimitive int)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            var eplInsert = namedWindow
                ? "insert into MyInfraWA select * from SupportBean"
                : "insert into MyInfraWA select TheString, IntPrimitive from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInsert);
            epService.EPAdministrator.CreateEPL("on S1 as s1 delete from MyInfraWA where s1.p10 = TheString");

            var stmt = epService.EPAdministrator.CreateEPL(
                "on S0 as s0 " +
                "select window(win.*) as c0," +
                "window(win.*).where(v => v.IntPrimitive < 2) as c1, " +
                "window(win.*).ToMap(k=>k.TheString,v=>v.IntPrimitive) as c2 " +
                "from MyInfraWA as win");
            var listenerSelect = new SupportUpdateListener();
            stmt.Events += listenerSelect.Update;

            var beans = new SupportBean[3];
            for (var i = 0; i < beans.Length; i++) {
                beans[i] = new SupportBean("E" + i, i);
            }

            epService.EPRuntime.SendEvent(beans[0]);
            epService.EPRuntime.SendEvent(beans[1]);
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertReceived(
                listenerSelect, namedWindow, beans, new[] {0, 1}, new[] {0, 1}, "E0,E1".Split(','), new object[] {
                0, 1
            });

            // add bean
            epService.EPRuntime.SendEvent(beans[2]);
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertReceived(
                listenerSelect, namedWindow, beans, new[] {0, 1, 2}, new[] {0, 1}, "E0,E1,E2".Split(','), new object[] {
                0, 1, 2
            });

            // delete bean
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(12));
            AssertReceived(
                listenerSelect, namedWindow, beans, new[] {0, 2}, new[] {0}, "E0,E2".Split(','), new object[] {0, 2});

            // delete another bean
            epService.EPRuntime.SendEvent(new SupportBean_S1(13, "E0"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(14));
            AssertReceived(
                listenerSelect, namedWindow, beans, new[] {2}, new int[0], "E2".Split(','), new object[] {2});

            // delete last bean
            epService.EPRuntime.SendEvent(new SupportBean_S1(15, "E2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(16));
            AssertReceived(listenerSelect, namedWindow, beans, null, null, null, null);

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraWA", false);
        }

        private void AssertReceived(
            SupportUpdateListener listenerSelect, bool namedWindow, SupportBean[] beans, int[] indexesAll,
            int[] indexesWhere, string[] mapKeys, object[] mapValues) {
            var received = listenerSelect.AssertOneGetNewAndReset();
            object[] expectedAll;
            object[] expectedWhere;
            if (!namedWindow) {
                expectedAll = SupportBean.GetOAStringAndIntPerIndex(beans, indexesAll);
                expectedWhere = SupportBean.GetOAStringAndIntPerIndex(beans, indexesWhere);
                EPAssertionUtil.AssertEqualsAnyOrder(expectedAll, (object[]) received.Get("c0"));
                var receivedColl = (ICollection<object>) received.Get("c1");
                EPAssertionUtil.AssertEqualsAnyOrder(
                    expectedWhere, receivedColl == null ? null : receivedColl.ToArray());
            }
            else {
                expectedAll = SupportBean.GetBeansPerIndex(beans, indexesAll);
                expectedWhere = SupportBean.GetBeansPerIndex(beans, indexesWhere);
                EPAssertionUtil.AssertEqualsExactOrder(expectedAll, (object[]) received.Get("c0"));
                EPAssertionUtil.AssertEqualsExactOrder(expectedWhere, (ICollection<object>) received.Get("c1"));
            }

            EPAssertionUtil.AssertPropsMap((IDictionary<object, object>) received.Get("c2"), mapKeys, mapValues);
        }

        private void RunAssertionOnSelectIndexChoice(EPServiceProvider epService, bool isNamedWindow) {
            epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));

            var backingUniqueS1 = "unique hash={s1(string)} btree={} advanced={}";
            var backingUniqueS1L1 = "unique hash={s1(string),l1(long)} btree={} advanced={}";
            var backingNonUniqueS1 = "non-unique hash={s1(string)} btree={} advanced={}";
            var backingUniqueS1D1 = "unique hash={s1(string),d1(double)} btree={} advanced={}";
            var backingBtreeI1 = "non-unique hash={} btree={i1(int)} advanced={}";
            var backingBtreeD1 = "non-unique hash={} btree={d1(double)} advanced={}";
            var expectedIdxNameS1 = isNamedWindow ? null : "MyInfra";
            var listener = new SupportUpdateListener();

            var preloadedEventsOne = new object[]
                {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
            var eventSendAssertion = new IndexAssertionEventSend(
                () => {
                    var fields = "ssb2.s2,ssb1.s1,ssb1.i1".Split(',');
                    epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                    EPAssertionUtil.AssertProps(
                        listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", "E2", 20});
                    epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                    EPAssertionUtil.AssertProps(
                        listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", "E1", 10});
                });

            // single index one field (std:unique(s1))
            AssertIndexChoice(
                epService, listener, isNamedWindow, new string[0], preloadedEventsOne, "std:unique(s1)",
                new[] {
                    new IndexAssertion(null, "s1 = s2", expectedIdxNameS1, backingUniqueS1, eventSendAssertion),
                    new IndexAssertion(
                        null, "s1 = ssb2.s2 and l1 = ssb2.l2", expectedIdxNameS1, backingUniqueS1, eventSendAssertion),
                    new IndexAssertion(
                        "@Hint('Index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", expectedIdxNameS1, backingUniqueS1,
                        eventSendAssertion),
                    new IndexAssertion("@Hint('Index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2") // busted
                });

            // single index one field (std:unique(s1))
            if (isNamedWindow) {
                var indexOneField = new[] {"create unique index One on MyInfra (s1)"};
                AssertIndexChoice(
                    epService, listener, isNamedWindow, indexOneField, preloadedEventsOne, "std:unique(s1)",
                    new[] {
                        new IndexAssertion(null, "s1 = s2", "One", backingUniqueS1, eventSendAssertion),
                        new IndexAssertion(
                            null, "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1, eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('Index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('Index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2") // busted
                    });
            }

            // single index two field  (std:unique(s1))
            var indexTwoField = new[] {"create unique index One on MyInfra (s1, l1)"};
            AssertIndexChoice(
                epService, listener, isNamedWindow, indexTwoField, preloadedEventsOne, "std:unique(s1)",
                new[] {
                    new IndexAssertion(null, "s1 = ssb2.s2", expectedIdxNameS1, backingUniqueS1, eventSendAssertion),
                    new IndexAssertion(
                        null, "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1L1, eventSendAssertion)
                });
            AssertIndexChoice(
                epService, listener, isNamedWindow, indexTwoField, preloadedEventsOne, "win:keepall()",
                new[] {
                    new IndexAssertion(
                        null, "s1 = ssb2.s2", expectedIdxNameS1, isNamedWindow ? backingNonUniqueS1 : backingUniqueS1,
                        eventSendAssertion),
                    new IndexAssertion(
                        null, "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1L1, eventSendAssertion)
                });

            // two index one unique  (std:unique(s1))
            var indexSetTwo = new[] {
                "create index One on MyInfra (s1)",
                "create unique index Two on MyInfra (s1, d1)"
            };
            AssertIndexChoice(
                epService, listener, isNamedWindow, indexSetTwo, preloadedEventsOne, "std:unique(s1)",
                new[] {
                    new IndexAssertion(
                        null, "s1 = ssb2.s2", isNamedWindow ? "One" : "MyInfra",
                        isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                    new IndexAssertion(
                        null, "s1 = ssb2.s2 and l1 = ssb2.l2", isNamedWindow ? "One" : "MyInfra",
                        isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                    new IndexAssertion(
                        "@Hint('Index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1,
                        eventSendAssertion),
                    new IndexAssertion(
                        "@Hint('Index(Two,One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1,
                        eventSendAssertion),
                    new IndexAssertion("@Hint('Index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"), // busted
                    new IndexAssertion(
                        "@Hint('Index(explicit,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2",
                        isNamedWindow ? "One" : "MyInfra", isNamedWindow ? backingNonUniqueS1 : backingUniqueS1,
                        eventSendAssertion),
                    new IndexAssertion(
                        null, "s1 = ssb2.s2 and d1 = ssb2.d2 and l1 = ssb2.l2", isNamedWindow ? "Two" : "MyInfra",
                        isNamedWindow ? backingUniqueS1D1 : backingUniqueS1, eventSendAssertion),
                    new IndexAssertion("@Hint('Index(explicit,bust)')", "d1 = ssb2.d2 and l1 = ssb2.l2") // busted
                });

            // two index one unique  (win:keepall)
            AssertIndexChoice(
                epService, listener, isNamedWindow, indexSetTwo, preloadedEventsOne, "win:keepall()",
                new[] {
                    new IndexAssertion(
                        null, "s1 = ssb2.s2", isNamedWindow ? "One" : "MyInfra",
                        isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                    new IndexAssertion(
                        null, "s1 = ssb2.s2 and l1 = ssb2.l2", isNamedWindow ? "One" : "MyInfra",
                        isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                    new IndexAssertion(
                        "@Hint('Index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1,
                        eventSendAssertion),
                    new IndexAssertion(
                        "@Hint('Index(Two,One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1,
                        eventSendAssertion),
                    new IndexAssertion("@Hint('Index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"), // busted
                    new IndexAssertion(
                        "@Hint('Index(explicit,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2",
                        isNamedWindow ? "One" : "MyInfra", isNamedWindow ? backingNonUniqueS1 : backingUniqueS1,
                        eventSendAssertion),
                    new IndexAssertion(
                        null, "s1 = ssb2.s2 and d1 = ssb2.d2 and l1 = ssb2.l2", isNamedWindow ? "Two" : "MyInfra",
                        isNamedWindow ? backingUniqueS1D1 : backingUniqueS1, eventSendAssertion),
                    new IndexAssertion("@Hint('Index(explicit,bust)')", "d1 = ssb2.d2 and l1 = ssb2.l2") // busted
                });

            // range  (std:unique(s1))
            var noAssertion = new IndexAssertionEventSend(() => { });
            var indexSetThree = new[] {
                "create index One on MyInfra (i1 btree)",
                "create index Two on MyInfra (d1 btree)"
            };
            AssertIndexChoice(
                epService, listener, isNamedWindow, indexSetThree, preloadedEventsOne, "std:unique(s1)",
                new[] {
                    new IndexAssertion(null, "i1 between 1 and 10", "One", backingBtreeI1, noAssertion),
                    new IndexAssertion(null, "d1 between 1 and 10", "Two", backingBtreeD1, noAssertion),
                    new IndexAssertion("@Hint('Index(One, bust)')", "d1 between 1 and 10") // busted
                });

            // rel ops
            var preloadedEventsRelOp = new object[] {new SupportSimpleBeanOne("E1", 10, 11, 12)};
            var relOpAssertion = new IndexAssertionEventSend(
                () => {
                    var fields = "ssb2.s2,ssb1.s1,ssb1.i1".Split(',');
                    epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("EX", 0, 0, 0));
                    EPAssertionUtil.AssertProps(
                        listener.AssertOneGetNewAndReset(), fields, new object[] {"EX", "E1", 10});
                });

            AssertIndexChoice(
                epService, listener, isNamedWindow, new string[0], preloadedEventsRelOp, "win:keepall()",
                new[] {
                    new IndexAssertion(null, "9 < i1", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion),
                    new IndexAssertion(null, "10 <= i1", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion),
                    new IndexAssertion(null, "i1 <= 10", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion),
                    new IndexAssertion(null, "i1 < 11", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion),
                    new IndexAssertion(null, "11 > i1", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion)
                });
        }

        private void AssertIndexChoice(
            EPServiceProvider epService,
            SupportUpdateListener listenerSelect,
            bool isNamedWindow,
            string[] indexes,
            object[] preloadedEvents,
            string datawindow,
            IndexAssertion[] assertions) {
            var eplCreate = isNamedWindow
                ? "@Name('create-window') create window MyInfra." + datawindow + " as SSB1"
                : "@Name('create-table') create table MyInfra(s1 string primary key, i1 int, d1 double, l1 long)";
            epService.EPAdministrator.CreateEPL(eplCreate);

            epService.EPAdministrator.CreateEPL("insert into MyInfra select s1,i1,d1,l1 from SSB1");
            foreach (var index in indexes) {
                epService.EPAdministrator.CreateEPL(index, "create-index '" + index + "'");
            }

            foreach (var @event in preloadedEvents) {
                epService.EPRuntime.SendEvent(@event);
            }

            var count = 0;
            foreach (var assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                var consumeEpl = INDEX_CALLBACK_HOOK +
                                 (assertion.Hint == null ? "" : assertion.Hint) +
                                 "@Name('on-select') on SSB2 as ssb2 " +
                                 "select * " +
                                 "from MyInfra as ssb1 where " + assertion.WhereClause;

                EPStatement consumeStmt;
                try {
                    consumeStmt = epService.EPAdministrator.CreateEPL(consumeEpl);
                }
                catch (EPStatementException ex) {
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }

                    throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                }

                // assert index and access
                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    assertion.ExpectedIndexName, assertion.IndexBackingClass);
                consumeStmt.Events += listenerSelect.Update;
                assertion.EventSendAssertion.Invoke();
                consumeStmt.Dispose();
            }

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }

        private void SendSupportBean_A(EPServiceProvider epService, string id) {
            var bean = new SupportBean_A(id);
            epService.EPRuntime.SendEvent(bean);
        }

        private void SendSupportBean_B(EPServiceProvider epService, string id) {
            var bean = new SupportBean_B(id);
            epService.EPRuntime.SendEvent(bean);
        }

        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }

        private void SendTimer(long timeInMSec, EPServiceProvider epService) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace