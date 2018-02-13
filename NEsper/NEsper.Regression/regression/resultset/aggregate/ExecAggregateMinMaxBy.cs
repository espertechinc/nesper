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
using com.espertech.esper.client.soda;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateMinMaxBy : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionGroupedSortedMinMax(epService);
            RunAssertionMinByMaxByOverWindow(epService);
            RunAssertionNoAlias(epService);
            RunAssertionMultipleOverlappingCategories(epService);
            RunAssertionMultipleCriteria(epService);
            RunAssertionNoDataWindow(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionGroupedSortedMinMax(EPServiceProvider epService) {
            var epl = "select " +
                    "window(*) as c0, " +
                    "sorted(intPrimitive desc) as c1, " +
                    "sorted(intPrimitive asc) as c2, " +
                    "maxby(intPrimitive) as c3, " +
                    "minby(intPrimitive) as c4, " +
                    "maxbyever(intPrimitive) as c5, " +
                    "minbyever(intPrimitive) as c6 " +
                    "from SupportBean#Groupwin(longPrimitive)#length(3) " +
                    "group by longPrimitive";
            var stmtPlain = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmtPlain.AddListener(listener);
    
            TryAssertionGroupedSortedMinMax(epService, listener);
            stmtPlain.Dispose();
    
            // test SODA
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            var stmtSoda = epService.EPAdministrator.Create(model);
            stmtSoda.AddListener(listener);
            Assert.AreEqual(epl, stmtSoda.Text);
            TryAssertionGroupedSortedMinMax(epService, listener);
            stmtSoda.Dispose();
    
            // test join
            var eplJoin = "select " +
                    "window(sb.*) as c0, " +
                    "sorted(intPrimitive desc) as c1, " +
                    "sorted(intPrimitive asc) as c2, " +
                    "maxby(intPrimitive) as c3, " +
                    "minby(intPrimitive) as c4, " +
                    "maxbyever(intPrimitive) as c5, " +
                    "minbyever(intPrimitive) as c6 " +
                    "from S0#lastevent, SupportBean#Groupwin(longPrimitive)#length(3) as sb " +
                    "group by longPrimitive";
            var stmtJoin = epService.EPAdministrator.CreateEPL(eplJoin);
            stmtJoin.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "p00"));
            TryAssertionGroupedSortedMinMax(epService, listener);
            stmtJoin.Dispose();
    
            // test join multirow
            var fields = "c0".Split(',');
            var joinMultirow = "select sorted(intPrimitive desc) as c0 from S0#keepall, SupportBean#length(2)";
            var stmtJoinMultirow = epService.EPAdministrator.CreateEPL(joinMultirow);
            stmtJoinMultirow.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S3"));
    
            var eventOne = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(eventOne);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{new Object[]{eventOne}});
    
            var eventTwo = new SupportBean("E2", 2);
            epService.EPRuntime.SendEvent(eventTwo);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{new Object[]{eventTwo, eventOne}});
    
            var eventThree = new SupportBean("E3", 0);
            epService.EPRuntime.SendEvent(eventThree);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{new Object[]{eventTwo, eventThree}});
    
            stmtJoinMultirow.Dispose();
        }
    
        private void TryAssertionGroupedSortedMinMax(EPServiceProvider epService, SupportUpdateListener listener) {
    
            var fields = "c0,c1,c2,c3,c4,c5,c6".Split(',');
            var eventOne = MakeEvent("E1", 1, 1);
            epService.EPRuntime.SendEvent(eventOne);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{
                            new Object[]{eventOne},
                            new Object[]{eventOne},
                            new Object[]{eventOne},
                            eventOne, eventOne, eventOne, eventOne});
    
            var eventTwo = MakeEvent("E2", 2, 1);
            epService.EPRuntime.SendEvent(eventTwo);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{
                            new Object[]{eventOne, eventTwo},
                            new Object[]{eventTwo, eventOne},
                            new Object[]{eventOne, eventTwo},
                            eventTwo, eventOne, eventTwo, eventOne});
    
            var eventThree = MakeEvent("E3", 0, 1);
            epService.EPRuntime.SendEvent(eventThree);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{
                            new Object[]{eventOne, eventTwo, eventThree},
                            new Object[]{eventTwo, eventOne, eventThree},
                            new Object[]{eventThree, eventOne, eventTwo},
                            eventTwo, eventThree, eventTwo, eventThree});
    
            var eventFour = MakeEvent("E4", 3, 1);   // pushes out E1
            epService.EPRuntime.SendEvent(eventFour);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{
                            new Object[]{eventTwo, eventThree, eventFour},
                            new Object[]{eventFour, eventTwo, eventThree},
                            new Object[]{eventThree, eventTwo, eventFour},
                            eventFour, eventThree, eventFour, eventThree});
    
            var eventFive = MakeEvent("E5", -1, 2);   // group 2
            epService.EPRuntime.SendEvent(eventFive);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{
                            new Object[]{eventFive},
                            new Object[]{eventFive},
                            new Object[]{eventFive},
                            eventFive, eventFive, eventFive, eventFive});
    
            var eventSix = MakeEvent("E6", -1, 1);   // pushes out E2
            epService.EPRuntime.SendEvent(eventSix);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{
                            new Object[]{eventThree, eventFour, eventSix},
                            new Object[]{eventFour, eventThree, eventSix},
                            new Object[]{eventSix, eventThree, eventFour},
                            eventFour, eventSix, eventFour, eventSix});
    
            var eventSeven = MakeEvent("E7", 2, 2);   // group 2
            epService.EPRuntime.SendEvent(eventSeven);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{
                            new Object[]{eventFive, eventSeven},
                            new Object[]{eventSeven, eventFive},
                            new Object[]{eventFive, eventSeven},
                            eventSeven, eventFive, eventSeven, eventFive});
    
        }
    
        private void RunAssertionMinByMaxByOverWindow(EPServiceProvider epService) {
            var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9".Split(',');
            var epl = "select " +
                    "maxbyever(longPrimitive) as c0, " +
                    "minbyever(longPrimitive) as c1, " +
                    "maxby(longPrimitive).longPrimitive as c2, " +
                    "maxby(longPrimitive).theString as c3, " +
                    "maxby(longPrimitive).intPrimitive as c4, " +
                    "maxby(longPrimitive) as c5, " +
                    "minby(longPrimitive).longPrimitive as c6, " +
                    "minby(longPrimitive).theString as c7, " +
                    "minby(longPrimitive).intPrimitive as c8, " +
                    "minby(longPrimitive) as c9 " +
                    "from SupportBean#length(5)";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var eventOne = MakeEvent("E1", 1, 10);
            epService.EPRuntime.SendEvent(eventOne);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{eventOne, eventOne, 10L, "E1", 1, eventOne, 10L, "E1", 1, eventOne});
    
            var eventTwo = MakeEvent("E2", 2, 20);
            epService.EPRuntime.SendEvent(eventTwo);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{eventTwo, eventOne, 20L, "E2", 2, eventTwo, 10L, "E1", 1, eventOne});
    
            var eventThree = MakeEvent("E3", 3, 5);
            epService.EPRuntime.SendEvent(eventThree);
            var resultThree = new Object[]{eventTwo, eventThree, 20L, "E2", 2, eventTwo, 5L, "E3", 3, eventThree};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, resultThree);
    
            var eventFour = MakeEvent("E4", 4, 5);
            epService.EPRuntime.SendEvent(eventFour); // same as E3
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, resultThree);
    
            var eventFive = MakeEvent("E5", 5, 20);
            epService.EPRuntime.SendEvent(eventFive); // same as E2
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, resultThree);
    
            var eventSix = MakeEvent("E6", 6, 10);
            epService.EPRuntime.SendEvent(eventSix); // expires E1
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, resultThree);
    
            var eventSeven = MakeEvent("E7", 7, 20);
            epService.EPRuntime.SendEvent(eventSeven); // expires E2
            var resultSeven = new Object[]{eventTwo, eventThree, 20L, "E5", 5, eventFive, 5L, "E3", 3, eventThree};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, resultSeven);
    
            epService.EPRuntime.SendEvent(MakeEvent("E8", 8, 20)); // expires E3
            var resultEight = new Object[]{eventTwo, eventThree, 20L, "E5", 5, eventFive, 5L, "E4", 4, eventFour};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, resultEight);
    
            epService.EPRuntime.SendEvent(MakeEvent("E9", 9, 19)); // expires E4
            var resultNine = new Object[]{eventTwo, eventThree, 20L, "E5", 5, eventFive, 10L, "E6", 6, eventSix};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, resultNine);
    
            epService.EPRuntime.SendEvent(MakeEvent("E10", 10, 12)); // expires E5
            var resultTen = new Object[]{eventTwo, eventThree, 20L, "E7", 7, eventSeven, 10L, "E6", 6, eventSix};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, resultTen);
    
            stmt.Dispose();
        }
    
        private void RunAssertionNoAlias(EPServiceProvider epService) {
            var stmt = epService.EPAdministrator.CreateEPL("select " +
                    "maxby(intPrimitive).theString, " +
                    "minby(intPrimitive)," +
                    "maxbyever(intPrimitive).theString, " +
                    "minbyever(intPrimitive)," +
                    "sorted(intPrimitive asc, theString desc)" +
                    " from SupportBean#Time(10)");
    
            var props = stmt.EventType.PropertyDescriptors;
            Assert.AreEqual("maxby(intPrimitive).TheString()", props[0].PropertyName);
            Assert.AreEqual("minby(intPrimitive)", props[1].PropertyName);
            Assert.AreEqual("maxbyever(intPrimitive).TheString()", props[2].PropertyName);
            Assert.AreEqual("minbyever(intPrimitive)", props[3].PropertyName);
            Assert.AreEqual("sorted(intPrimitive,theString desc)", props[4].PropertyName);
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultipleOverlappingCategories(EPServiceProvider epService) {
            var fields = "c0,c1,c2,c3,c4,c5,c6,c7".Split(',');
            var stmt = epService.EPAdministrator.CreateEPL("select " +
                    "maxbyever(intPrimitive).longPrimitive as c0," +
                    "maxbyever(theString).longPrimitive as c1," +
                    "minbyever(intPrimitive).longPrimitive as c2," +
                    "minbyever(theString).longPrimitive as c3," +
                    "maxby(intPrimitive).longPrimitive as c4," +
                    "maxby(theString).longPrimitive as c5," +
                    "minby(intPrimitive).longPrimitive as c6," +
                    "minby(theString).longPrimitive as c7 " +
                    "from SupportBean#keepall");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(MakeEvent("C", 10, 1L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L});
    
            epService.EPRuntime.SendEvent(MakeEvent("P", 5, 2L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L});
    
            epService.EPRuntime.SendEvent(MakeEvent("G", 7, 3L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L});
    
            epService.EPRuntime.SendEvent(MakeEvent("A", 7, 4L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{1L, 2L, 2L, 4L, 1L, 2L, 2L, 4L});
    
            epService.EPRuntime.SendEvent(MakeEvent("G", 1, 5L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{1L, 2L, 5L, 4L, 1L, 2L, 5L, 4L});
    
            epService.EPRuntime.SendEvent(MakeEvent("X", 7, 6L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{1L, 6L, 5L, 4L, 1L, 6L, 5L, 4L});
    
            epService.EPRuntime.SendEvent(MakeEvent("G", 100, 7L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{7L, 6L, 5L, 4L, 7L, 6L, 5L, 4L});
    
            epService.EPRuntime.SendEvent(MakeEvent("Z", 1000, 8L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{8L, 8L, 5L, 4L, 8L, 8L, 5L, 4L});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultipleCriteria(EPServiceProvider epService) {
            // test sorted multiple criteria
            var fields = "c0,c1,c2,c3".Split(',');
            var stmt = epService.EPAdministrator.CreateEPL("select " +
                    "sorted(theString desc, intPrimitive desc) as c0," +
                    "sorted(theString, intPrimitive) as c1," +
                    "sorted(theString asc, intPrimitive asc) as c2," +
                    "sorted(theString desc, intPrimitive asc) as c3 " +
                    "from SupportBean#keepall");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var eventOne = new SupportBean("C", 10);
            epService.EPRuntime.SendEvent(eventOne);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[][]{
                    new Object[]{eventOne},
                    new Object[]{eventOne},
                    new Object[]{eventOne},
                    new Object[]{eventOne}});
    
            var eventTwo = new SupportBean("D", 20);
            epService.EPRuntime.SendEvent(eventTwo);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[][]{
                    new Object[]{eventTwo, eventOne},
                    new Object[]{eventOne, eventTwo},
                    new Object[]{eventOne, eventTwo},
                    new Object[]{eventTwo, eventOne}});
    
            var eventThree = new SupportBean("C", 15);
            epService.EPRuntime.SendEvent(eventThree);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[][]{
                    new Object[]{eventTwo, eventThree, eventOne},
                    new Object[]{eventOne, eventThree, eventTwo},
                    new Object[]{eventOne, eventThree, eventTwo},
                    new Object[]{eventTwo, eventOne, eventThree}});
    
            var eventFour = new SupportBean("D", 19);
            epService.EPRuntime.SendEvent(eventFour);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[][]{
                    new Object[]{eventTwo, eventFour, eventThree, eventOne},
                    new Object[]{eventOne, eventThree, eventFour, eventTwo},
                    new Object[]{eventOne, eventThree, eventFour, eventTwo},
                    new Object[]{eventFour, eventTwo, eventOne, eventThree}});
    
            stmt.Dispose();
    
            // test min/max
            var fieldsTwo = "c0,c1,c2,c3,c4,c5,c6,c7".Split(',');
            var stmtTwo = epService.EPAdministrator.CreateEPL("select " +
                    "maxbyever(intPrimitive, theString).longPrimitive as c0," +
                    "minbyever(intPrimitive, theString).longPrimitive as c1," +
                    "maxbyever(theString, intPrimitive).longPrimitive as c2," +
                    "minbyever(theString, intPrimitive).longPrimitive as c3," +
                    "maxby(intPrimitive, theString).longPrimitive as c4," +
                    "minby(intPrimitive, theString).longPrimitive as c5," +
                    "maxby(theString, intPrimitive).longPrimitive as c6," +
                    "minby(theString, intPrimitive).longPrimitive as c7 " +
                    "from SupportBean#keepall");
            stmtTwo.AddListener(listener);
    
            epService.EPRuntime.SendEvent(MakeEvent("C", 10, 1L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo,
                    new Object[]{1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L});
    
            epService.EPRuntime.SendEvent(MakeEvent("P", 5, 2L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo,
                    new Object[]{1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L});
    
            epService.EPRuntime.SendEvent(MakeEvent("C", 9, 3L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo,
                    new Object[]{1L, 2L, 2L, 3L, 1L, 2L, 2L, 3L});
    
            epService.EPRuntime.SendEvent(MakeEvent("C", 11, 4L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo,
                    new Object[]{4L, 2L, 2L, 3L, 4L, 2L, 2L, 3L});
    
            epService.EPRuntime.SendEvent(MakeEvent("X", 11, 5L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo,
                    new Object[]{5L, 2L, 5L, 3L, 5L, 2L, 5L, 3L});
    
            epService.EPRuntime.SendEvent(MakeEvent("X", 0, 6L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo,
                    new Object[]{5L, 6L, 5L, 3L, 5L, 6L, 5L, 3L});
    
            stmtTwo.Dispose();
        }
    
        private void RunAssertionNoDataWindow(EPServiceProvider epService) {
            var fields = "c0,c1,c2,c3".Split(',');
            var stmt = epService.EPAdministrator.CreateEPL("select " +
                    "maxbyever(intPrimitive).theString as c0, " +
                    "minbyever(intPrimitive).theString as c1, " +
                    "maxby(intPrimitive).theString as c2, " +
                    "minby(intPrimitive).theString as c3 " +
                    "from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", "E1", "E1", "E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", "E1", "E2", "E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", "E3", "E2", "E3"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E4", "E3", "E4", "E3"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "select MaxBy(p00||p10) from S0#lastevent, S1#lastevent",
                    "Error starting statement: Failed to validate select-clause expression 'maxby(p00||p10)': The 'maxby' aggregation function requires that any parameter expressions evaluate properties of the same stream [select MaxBy(p00||p10) from S0#lastevent, S1#lastevent]");
    
            TryInvalid(epService, "select sorted(p00) from S0",
                    "Error starting statement: Failed to validate select-clause expression 'sorted(p00)': The 'sorted' aggregation function requires that a data window is declared for the stream [select sorted(p00) from S0]");
        }
    
        private SupportBean MakeEvent(string @string, int intPrimitive, long longPrimitive) {
            var @event = new SupportBean(@string, intPrimitive);
            @event.LongPrimitive = longPrimitive;
            return @event;
        }
    }
} // end of namespace
