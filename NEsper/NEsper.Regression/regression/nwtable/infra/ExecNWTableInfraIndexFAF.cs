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
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraIndexFAF : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
    
            RunAssertionSelectIndexChoiceJoin(epService, true);
            RunAssertionSelectIndexChoiceJoin(epService, false);
    
            RunAssertionSelectIndexChoice(epService, true);
            RunAssertionSelectIndexChoice(epService, false);
        }
    
        private void RunAssertionSelectIndexChoiceJoin(EPServiceProvider epService, bool namedWindow) {
    
            var preloadedEventsOne = new object[]{
                    new SupportSimpleBeanOne("E1", 10, 1, 2),
                    new SupportSimpleBeanOne("E2", 11, 3, 4),
                    new SupportSimpleBeanTwo("E1", 20, 1, 2),
                    new SupportSimpleBeanTwo("E2", 21, 3, 4),
            };
            var fafAssertion = new IndexAssertionFAF((result) => {
                string[] fields = "w1.s1,w2.s2,w1.i1,w2.i2".Split(',');
                EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields,
                        new object[][]{new object[] {"E1", "E1", 10, 20}, new object[] {"E2", "E2", 11, 21}});
            });
    
            var assertionsSingleProp = new IndexAssertion[]{
                    new IndexAssertion(null, "s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "s1 = s2 and l1 = l2", true, fafAssertion),
                    new IndexAssertion(null, "l1 = l2 and s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "d1 = d2 and l1 = l2 and s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "d1 = d2 and l1 = l2", false, fafAssertion),
            };
    
            // single prop, no index, both declared unique (named window only)
            if (namedWindow) {
                AssertIndexChoiceJoin(epService, namedWindow, new string[0], preloadedEventsOne, "std:unique(s1)", "std:unique(s2)", assertionsSingleProp);
            }
    
            // single prop, unique indexes, both declared keepall
            var uniqueIndex = new string[]{"create unique index W1I1 on W1(s1)", "create unique index W1I2 on W2(s2)"};
            AssertIndexChoiceJoin(epService, namedWindow, uniqueIndex, preloadedEventsOne, "win:keepall()", "win:keepall()", assertionsSingleProp);
    
            // single prop, mixed indexes, both declared keepall
            var assertionsMultiProp = new IndexAssertion[]{
                    new IndexAssertion(null, "s1 = s2", false, fafAssertion),
                    new IndexAssertion(null, "s1 = s2 and l1 = l2", true, fafAssertion),
                    new IndexAssertion(null, "l1 = l2 and s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "d1 = d2 and l1 = l2 and s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "d1 = d2 and l1 = l2", false, fafAssertion),
            };
            if (namedWindow) {
                var mixedIndex = new string[]{"create index W1I1 on W1(s1, l1)", "create unique index W1I2 on W2(s2)"};
                AssertIndexChoiceJoin(epService, namedWindow, mixedIndex, preloadedEventsOne, "std:unique(s1)", "win:keepall()", assertionsSingleProp);
    
                // multi prop, no index, both declared unique
                AssertIndexChoiceJoin(epService, namedWindow, new string[0], preloadedEventsOne, "std:unique(s1, l1)", "std:unique(s2, l2)", assertionsMultiProp);
            }
    
            // multi prop, unique indexes, both declared keepall
            var uniqueIndexMulti = new string[]{"create unique index W1I1 on W1(s1, l1)", "create unique index W1I2 on W2(s2, l2)"};
            AssertIndexChoiceJoin(epService, namedWindow, uniqueIndexMulti, preloadedEventsOne, "win:keepall()", "win:keepall()", assertionsMultiProp);
    
            // multi prop, mixed indexes, both declared keepall
            if (namedWindow) {
                var mixedIndexMulti = new string[]{"create index W1I1 on W1(s1)", "create unique index W1I2 on W2(s2, l2)"};
                AssertIndexChoiceJoin(epService, namedWindow, mixedIndexMulti, preloadedEventsOne, "std:unique(s1, l1)", "win:keepall()", assertionsMultiProp);
            }
        }
    
        private void AssertIndexChoiceJoin(EPServiceProvider epService, bool namedWindow, string[] indexes, object[] preloadedEvents, string datawindowOne, string datawindowTwo,
                                           params IndexAssertion[] assertions) {
            if (namedWindow) {
                epService.EPAdministrator.CreateEPL("create window W1." + datawindowOne + " as SSB1");
                epService.EPAdministrator.CreateEPL("create window W2." + datawindowTwo + " as SSB2");
            } else {
                epService.EPAdministrator.CreateEPL("create table W1 (s1 string primary key, i1 int primary key, d1 double primary key, l1 long primary key)");
                epService.EPAdministrator.CreateEPL("create table W2 (s2 string primary key, i2 int primary key, d2 double primary key, l2 long primary key)");
            }
            epService.EPAdministrator.CreateEPL("insert into W1 select s1,i1,d1,l1 from SSB1");
            epService.EPAdministrator.CreateEPL("insert into W2 select s2,i2,d2,l2 from SSB2");
    
            foreach (string index in indexes) {
                epService.EPAdministrator.CreateEPL(index);
            }
            foreach (Object @event in preloadedEvents) {
                epService.EPRuntime.SendEvent(@event);
            }
    
            int count = 0;
            foreach (IndexAssertion assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                string epl = INDEX_CALLBACK_HOOK +
                        (assertion.Hint == null ? "" : assertion.Hint) +
                        "select * from W1 as w1, W2 as w2 " +
                        "where " + assertion.WhereClause;
                EPOnDemandQueryResult result;
                try {
                    result = epService.EPRuntime.ExecuteQuery(epl);
                } catch (EPStatementException ex) {
                    Log.Error("Failed to process:" + ex.Message, ex);
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }
                    throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                }
    
                // assert index and access
                SupportQueryPlanIndexHook.AssertJoinAllStreamsAndReset(assertion.IsUnique);
                assertion.FAFAssertion.Invoke(result);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("W1", false);
            epService.EPAdministrator.Configuration.RemoveEventType("W2", false);
        }
    
        private void RunAssertionSelectIndexChoice(EPServiceProvider epService, bool namedWindow) {
            var preloadedEventsOne = new object[]{new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
            var fafAssertion = new IndexAssertionFAF(result =>
                {
                    string[] fields = "s1,i1".Split(',');
                    EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][]{new object[] {"E2", 20}});
                });
    
            // single index one field (plus declared unique)
            var noindexes = new string[0];
            AssertIndexChoice(epService, namedWindow, noindexes, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = 'E2'", null, null, fafAssertion),
                            new IndexAssertion(null, "s1 = 'E2' and l1 = 22", null, null, fafAssertion),
                            new IndexAssertion("@Hint('Index(One)')", "s1 = 'E2' and l1 = 22", null, null, fafAssertion),
                            new IndexAssertion("@Hint('Index(Two,bust)')", "s1 = 'E2' and l1 = 22"), // should bust
                    });
    
            // single index one field (plus declared unique)
            var indexOneField = new string[]{"create unique index One on MyInfra (s1)"};
            AssertIndexChoice(epService, namedWindow, indexOneField, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = 'E2'", "One", BACKING_SINGLE_UNIQUE, fafAssertion),
                            new IndexAssertion(null, "s1 in ('E2')", "One", BACKING_SINGLE_UNIQUE, fafAssertion),
                            new IndexAssertion(null, "s1 = 'E2' and l1 = 22", "One", BACKING_SINGLE_UNIQUE, fafAssertion),
                            new IndexAssertion("@Hint('Index(One)')", "s1 = 'E2' and l1 = 22", "One", BACKING_SINGLE_UNIQUE, fafAssertion),
                            new IndexAssertion("@Hint('Index(Two,bust)')", "s1 = 'E2' and l1 = 22"), // should bust
                    });
    
            // single index two field (plus declared unique)
            var indexTwoField = new string[]{"create unique index One on MyInfra (s1, l1)"};
            AssertIndexChoice(epService, namedWindow, indexTwoField, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = 'E2'", null, null, fafAssertion),
                            new IndexAssertion(null, "s1 = 'E2' and l1 = 22", "One", BACKING_MULTI_UNIQUE, fafAssertion),
                    });
    
            // two index one unique (plus declared unique)
            var indexSetTwo = new string[]{
                    "create index One on MyInfra (s1)",
                    "create unique index Two on MyInfra (s1, d1)"};
            AssertIndexChoice(epService, namedWindow, indexSetTwo, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = 'E2'", "One", BACKING_SINGLE_DUPS, fafAssertion),
                            new IndexAssertion(null, "s1 = 'E2' and l1 = 22", "One", BACKING_SINGLE_DUPS, fafAssertion),
                            new IndexAssertion("@Hint('Index(One)')", "s1 = 'E2' and l1 = 22", "One", BACKING_SINGLE_DUPS, fafAssertion),
                            new IndexAssertion("@Hint('Index(Two,One)')", "s1 = 'E2' and l1 = 22", "One", BACKING_SINGLE_DUPS, fafAssertion),
                            new IndexAssertion("@Hint('Index(Two,bust)')", "s1 = 'E2' and l1 = 22"),  // busted
                            new IndexAssertion("@Hint('Index(explicit,bust)')", "s1 = 'E2' and l1 = 22", "One", BACKING_SINGLE_DUPS, fafAssertion),
                            new IndexAssertion(null, "s1 = 'E2' and d1 = 21 and l1 = 22", "Two", BACKING_MULTI_UNIQUE, fafAssertion),
                            new IndexAssertion("@Hint('Index(explicit,bust)')", "d1 = 22 and l1 = 22"),   // busted
                    });
    
            // range (unique)
            var indexSetThree = new string[]{
                    "create index One on MyInfra (l1 btree)",
                    "create index Two on MyInfra (d1 btree)"};
            AssertIndexChoice(epService, namedWindow, indexSetThree, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "l1 between 22 and 23", "One", BACKING_SORTED_COERCED, fafAssertion),
                            new IndexAssertion(null, "d1 between 21 and 22", "Two", BACKING_SORTED_COERCED, fafAssertion),
                            new IndexAssertion("@Hint('Index(One, bust)')", "d1 between 21 and 22"), // busted
                    });
        }
    
        private void AssertIndexChoice(
            EPServiceProvider epService,
            bool namedWindow,
            string[] indexes,
            object[] preloadedEvents,
            string datawindow,
            IndexAssertion[] assertions)
        {
            string eplCreate = namedWindow ?
                    "create window MyInfra." + datawindow + " as SSB1" :
                    "create table MyInfra(s1 string primary key, i1 int primary key, d1 double primary key, l1 long primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select s1,i1,d1,l1 from SSB1");
            foreach (string index in indexes) {
                epService.EPAdministrator.CreateEPL(index);
            }
            foreach (Object @event in preloadedEvents) {
                epService.EPRuntime.SendEvent(@event);
            }
    
            int count = 0;
            foreach (IndexAssertion assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                string epl = INDEX_CALLBACK_HOOK +
                        (assertion.Hint == null ? "" : assertion.Hint) +
                        "select * from MyInfra where " + assertion.WhereClause;
                EPOnDemandQueryResult result;
                try {
                    result = epService.EPRuntime.ExecuteQuery(epl);
                } catch (EPStatementException ex) {
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }
                    throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                }
    
                // assert index and access
                SupportQueryPlanIndexHook.AssertFAFAndReset(assertion.ExpectedIndexName, assertion.IndexBackingClass);
                assertion.FAFAssertion.Invoke(result);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    }
} // end of namespace
