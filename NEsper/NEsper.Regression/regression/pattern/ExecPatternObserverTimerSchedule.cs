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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternObserverTimerSchedule : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsShareViews = false;
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            // just-date: "<date>" : non-recurring, typically a future start time, no period
            RunAssertionJustFutureDate(epService);
            RunAssertionJustPastDate(epService);
    
            // just-period: "P<...>" : non-recurring
            RunAssertionJustPeriod(epService);
    
            // partial-form-2: "<date>/P<period>": non-recurring, no start date (starts from current date), with period
            TryAssertionDateWithPeriod(epService);
    
            // partial-form-1: "R<?>/P<period>": recurring, no start date (starts from current date), with period
            RunAssertionRecurringLimitedWithPeriod(epService);
            RunAssertionRecurringUnlimitedWithPeriod(epService);
            RunAssertionRecurringAnchoring(epService);
    
            // full form: "R<?>/<date>/P<period>" : recurring, start time, with period
            RunAssertionFullFormLimitedFutureDated(epService);
            RunAssertionFullFormLimitedPastDated(epService);
            RunAssertionFullFormUnlimitedFutureDated(epService);
            RunAssertionFullFormUnlimitedPastDated(epService);
            RunAssertionFullFormUnlimitedPastDatedAnchoring(epService);
    
            // equivalent formulations
            RunAssertionEquivalent(epService);
    
            // invalid tests
            RunAssertionInvalid(epService);
    
            // followed-by
            RunAssertionFollowedBy(epService);
            RunAssertionFollowedByDynamicallyComputed(epService);
    
            // named parameters
            RunAssertionNameParameters(epService);
    
            // For Testing, could also use this:
            /*
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSecWZone("2001-10-01T05:51:00.000GMT-0:00")));
            epService.EPAdministrator.CreateEPL("select * from pattern[timer:schedule('2008-03-01T13:00:00Z/P1Y2M10DT2H30M')]").Events += listener.Update;
    
            long next = epService.EPRuntime.NextScheduledTime;
            Log.Info(DateTime.Print(next));
            */
        }
    
        private void RunAssertionFollowedByDynamicallyComputed(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("computeISO8601String", GetType(), "ComputeISO8601String");
    
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
            SendCurrentTime(iso, "2012-10-01T05:51:07.000GMT-0:00");
    
            string epl = "select * from pattern[every sb=SupportBean -> timer:schedule(iso: computeISO8601String(sb))]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            SupportBean b1 = MakeSendEvent(iso, "E1", 5);
    
            SendCurrentTime(iso, "2012-10-01T05:51:9.999GMT-0:00");
            Assert.IsFalse(listener.IsInvokedAndReset());
    
            SendCurrentTime(iso, "2012-10-01T05:51:10.000GMT-0:00");
            Assert.AreEqual(b1, listener.AssertOneGetNewAndReset().Get("sb"));
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionFollowedBy(EPServiceProvider epService) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
            SendCurrentTime(iso, "2012-10-01T05:51:07.000GMT-0:00");
    
            string epl = "select * from pattern[every sb=SupportBean -> timer:schedule(iso: 'R/1980-01-01T00:00:00Z/PT15S')]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            SupportBean b1 = MakeSendEvent(iso, "E1");
    
            SendCurrentTime(iso, "2012-10-01T05:51:14.999GMT-0:00");
            Assert.IsFalse(listener.IsInvokedAndReset());
    
            SendCurrentTime(iso, "2012-10-01T05:51:15.000GMT-0:00");
            Assert.AreEqual(b1, listener.AssertOneGetNewAndReset().Get("sb"));
    
            SendCurrentTime(iso, "2012-10-01T05:51:16.000GMT-0:00");
            SupportBean b2 = MakeSendEvent(iso, "E2");
    
            SendCurrentTime(iso, "2012-10-01T05:51:18.000GMT-0:00");
            SupportBean b3 = MakeSendEvent(iso, "E3");
    
            SendCurrentTime(iso, "2012-10-01T05:51:30.000GMT-0:00");
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "sb".Split(','), new object[][]{new object[] {b2}, new object[] {b3}});
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
    
            // the ISO 8601 parse tests reside with the parser
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[every timer:schedule(iso: 'x')]",
                    "Invalid parameter for pattern observer 'timer:schedule(iso:\"x\")': Failed to parse 'x': Exception parsing date 'x', the date is not a supported ISO 8601 date");
    
            // named parameter tests: absence, typing, etc.
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[timer:schedule()]",
                    "Invalid parameter for pattern observer 'timer:schedule()': No parameters provided");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[timer:schedule(x:1)]",
                    "Invalid parameter for pattern observer 'timer:schedule(x:1)': Unexpected named parameter 'x', expecting any of the following: [iso, repetitions, date, period]");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[timer:schedule(period:1)]",
                    "Invalid parameter for pattern observer 'timer:schedule(period:1)': Failed to validate named parameter 'period', expected a single expression returning a TimePeriod-typed value");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[timer:schedule(repetitions:'a', period:1 seconds)]",
                    "Invalid parameter for pattern observer 'timer:schedule(repetitions:\"a\",period:1 seconds)': Failed to validate named parameter 'repetitions', expected a single expression returning any of the following types: int,long");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[timer:schedule(date:1 seconds)]",
                    "Invalid parameter for pattern observer 'timer:schedule(date:1 seconds)': Failed to validate named parameter 'date', expected a single expression returning any of the following types: string,DateTime,DateTimeOffset,long,DateTimeEx");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[timer:schedule(repetitions:1)]",
                    "Invalid parameter for pattern observer 'timer:schedule(repetitions:1)': Either the date or period parameter is required");
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[timer:schedule(iso: 'R/1980-01-01T00:00:00Z/PT15S', repetitions:1)]",
                    "Invalid parameter for pattern observer 'timer:schedule(iso:\"R/1980-01-01T00:00:00Z/PT15S\",repetitions:1)': The 'iso' parameter is exclusive of other parameters");
        }
    
        private void RunAssertionEquivalent(EPServiceProvider epService) {
    
            string first = "select * from pattern[every timer:schedule(iso: 'R2/2008-03-01T13:00:00Z/P1Y2M10DT2H30M')]";
            TryAssertionEquivalent(epService, first);
    
            string second = "select * from pattern[every " +
                    "(timer:schedule(iso: '2008-03-01T13:00:00Z') or" +
                    " timer:schedule(iso: '2009-05-11T15:30:00Z'))]";
            TryAssertionEquivalent(epService, second);
    
            string third = "select * from pattern[every " +
                    "(timer:schedule(iso: '2008-03-01T13:00:00Z') or" +
                    " timer:schedule(iso: '2008-03-01T13:00:00Z/P1Y2M10DT2H30M'))]";
            TryAssertionEquivalent(epService, third);
        }
    
        private void TryAssertionEquivalent(EPServiceProvider epService, string epl) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
            SendCurrentTime(iso, "2001-10-01T05:51:00.000GMT-0:00");
    
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2008-03-01T13:00:00.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2009-05-11T15:30:00.000GMT-0:00");
            AssertSendNoMoreCallback(listener, iso, "2012-10-01T05:52:04.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void TryAssertionDateWithPeriod(EPServiceProvider epService) {
            TryAssertionDateWithPeriod(epService, "iso: '2012-10-01T05:52:00Z/PT2S'");
            TryAssertionDateWithPeriod(epService, "date: '2012-10-01T05:52:00Z', period: 2 seconds");
        }
    
        private void TryAssertionDateWithPeriod(EPServiceProvider epService, string parameters) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
            SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");
    
            // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
            string epl = "select * from pattern[timer:schedule(" + parameters + ")]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:02.000GMT-0:00");
            AssertSendNoMoreCallback(listener, iso, "2012-10-01T05:52:04.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionFullFormLimitedFutureDated(EPServiceProvider epService) {
            TryAssertionFullFormLimitedFutureDated(epService, true, "iso: 'R3/2012-10-01T05:52:00Z/PT2S'");
            TryAssertionFullFormLimitedFutureDated(epService, false, "iso: 'R3/2012-10-01T05:52:00Z/PT2S'");
            TryAssertionFullFormLimitedFutureDated(epService, false, "repetitions: 3L, date:'2012-10-01T05:52:00Z', period: 2 seconds");
        }
    
        private void TryAssertionFullFormLimitedFutureDated(EPServiceProvider epService, bool audit, string parameters) {
    
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
            SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");
    
            // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
            string epl = (audit ? "@Audit " : "") + "select * from pattern[every timer:schedule(" + parameters + ")]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:00.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:02.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:04.000GMT-0:00");
            AssertSendNoMoreCallback(listener, iso, "2012-10-01T05:52:06.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionJustFutureDate(EPServiceProvider epService) {
            TryAssertionJustFutureDate(epService, true, "iso: '2012-10-01T05:52:00Z'");
            TryAssertionJustFutureDate(epService, false, "iso: '2012-10-01T05:52:00Z'");
            TryAssertionJustFutureDate(epService, false, "date: '2012-10-01T05:52:00Z'");
        }
    
        private void TryAssertionJustFutureDate(EPServiceProvider epService, bool hasEvery, string parameters) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
            SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");
    
            // Fire once at "2012-10-01T05:52:00Z" (UTC)
            string epl = "select * from pattern[" + (hasEvery ? "every " : "") + "timer:schedule(" + parameters + ")]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:00.000GMT-0:00");
            AssertSendNoMoreCallback(listener, iso, "2012-10-01T05:53:00.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionJustPastDate(EPServiceProvider epService) {
            TryAssertionJustPastDate(epService, true);
            TryAssertionJustPastDate(epService, false);
        }
    
        private void TryAssertionJustPastDate(EPServiceProvider epService, bool hasEvery) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
            SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");
    
            // Fire once at "2012-10-01T05:52:00Z" (UTC)
            string epl = "select * from pattern[" + (hasEvery ? "every " : "") + "timer:schedule(iso: '2010-10-01T05:52:00Z')]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertSendNoMoreCallback(listener, iso, "2012-10-01T05:53:00.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionJustPeriod(EPServiceProvider epService) {
            TryAssertionJustPeriod(epService, "iso:'P1DT2H'");
            TryAssertionJustPeriod(epService, "period: 1 day 2 hours");
        }
    
        private void TryAssertionJustPeriod(EPServiceProvider epService, string parameters) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
            SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");
    
            // Fire once after 1 day and 2 hours
            string epl = "select * from pattern[timer:schedule(" + parameters + ")]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2012-10-02T07:51:00.000GMT-0:00");
            AssertSendNoMoreCallback(listener, iso, "2012-10-03T09:51:00.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionRecurringLimitedWithPeriod(EPServiceProvider epService) {
            TryAssertionRecurringLimitedWithPeriod(epService, "iso:'R3/PT2S'");
            TryAssertionRecurringLimitedWithPeriod(epService, "repetitions:3L, period: 2 seconds");
        }
    
        private void TryAssertionRecurringLimitedWithPeriod(EPServiceProvider epService, string parameters) {
    
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
    
            // Fire 3 times after 2 seconds from current time
            SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
            string epl = "select * from pattern[every timer:schedule(" + parameters + ")]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:02.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:04.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:06.000GMT-0:00");
            AssertSendNoMoreCallback(listener, iso, "2012-10-01T05:52:08.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionRecurringUnlimitedWithPeriod(EPServiceProvider epService) {
    
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
    
            // Fire 3 times after 2 seconds from current time
            SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
            string epl = "select * from pattern[every timer:schedule(iso:'R/PT1M10S')]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:53:10.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:54:20.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:55:30.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:56:40.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionFullFormUnlimitedPastDated(EPServiceProvider epService) {
    
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
    
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
            string epl = "select * from pattern[every timer:schedule(iso:'R/1980-01-01T00:00:00Z/PT1S')]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:01.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:02.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:03.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionNameParameters(EPServiceProvider epService) {
            foreach (string name in "GetThe1980Calendar,GetThe1980Date,GetThe1980Long,GetTheSeconds".Split(',')) {
                epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(name, GetType(), name);
            }
    
            TryAssertionNameParameters(epService, "repetitions:-1L, date:'1980-01-01T00:00:00Z', period: 1 seconds");
            TryAssertionNameParameters(epService, "repetitions:-1, date:getThe1980Calendar(), period: 1 seconds");
            TryAssertionNameParameters(epService, "repetitions:-1, date:getThe1980Date(), period: getTheSeconds() seconds");
            TryAssertionNameParameters(epService, "repetitions:-1, date:getThe1980Long(), period: 1 seconds");
        }
    
        private void TryAssertionNameParameters(EPServiceProvider epService, string parameters) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
    
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
            string epl = "select * from pattern[every timer:schedule(" + parameters + ")]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:01.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:02.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:03.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionFullFormUnlimitedPastDatedAnchoring(EPServiceProvider epService) {
    
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
    
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(iso, "2012-01-01T00:0:00.000GMT-0:00");
            string epl = "select * from pattern[every timer:schedule(iso:'R/1980-01-01T00:00:00Z/PT10S')]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            SendCurrentTime(iso, "2012-01-01T00:0:15.000GMT-0:00");
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            SendCurrentTime(iso, "2012-01-01T00:0:20.000GMT-0:00");
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            AssertReceivedAtTime(listener, iso, "2012-01-01T00:0:30.000GMT-0:00");
    
            SendCurrentTime(iso, "2012-01-01T00:0:55.000GMT-0:00");
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            AssertReceivedAtTime(listener, iso, "2012-01-01T00:1:00.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionRecurringAnchoring(EPServiceProvider epService) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
    
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(iso, "2012-01-01T00:0:00.000GMT-0:00");
            string epl = "select * from pattern[every timer:schedule(iso: 'R/PT10S')]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            SendCurrentTime(iso, "2012-01-01T00:0:15.000GMT-0:00");
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            SendCurrentTime(iso, "2012-01-01T00:0:20.000GMT-0:00");
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            AssertReceivedAtTime(listener, iso, "2012-01-01T00:0:30.000GMT-0:00");
    
            SendCurrentTime(iso, "2012-01-01T00:0:55.000GMT-0:00");
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            AssertReceivedAtTime(listener, iso, "2012-01-01T00:1:00.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionFullFormLimitedPastDated(EPServiceProvider epService) {
    
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
    
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
            string epl = "select * from pattern[every timer:schedule(iso: 'R8/2012-10-01T05:51:00Z/PT10S')]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2012-10-01T05:52:10.000GMT-0:00");
            AssertSendNoMoreCallback(listener, iso, "2012-10-01T05:52:20.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void RunAssertionFullFormUnlimitedFutureDated(EPServiceProvider epService) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
    
            // Repeat unlimited number of times, reference-dated to future date, period of 1 day
            SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
            string epl = "select * from pattern[every timer:schedule(iso: 'R/2013-01-01T02:00:05Z/P1D')]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            AssertReceivedAtTime(listener, iso, "2013-01-01T02:00:05.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2013-01-02T02:00:05.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2013-01-03T02:00:05.000GMT-0:00");
            AssertReceivedAtTime(listener, iso, "2013-01-04T02:00:05.000GMT-0:00");
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void AssertSendNoMoreCallback(SupportUpdateListener listener, EPServiceProviderIsolated iso, string time) {
            SendCurrentTime(iso, time);
            Assert.IsFalse(listener.IsInvokedAndReset());
            SendCurrentTime(iso, "2999-01-01T00:0:00.000GMT-0:00");
            Assert.IsFalse(listener.IsInvokedAndReset());
        }
    
        private void AssertReceivedAtTime(SupportUpdateListener listener, EPServiceProviderIsolated iso, string time) {
            long msec = DateTimeParser.ParseDefaultMSecWZone(time);
    
            iso.EPRuntime.SendEvent(new CurrentTimeEvent(msec - 1));
            Assert.IsFalse(listener.IsInvokedAndReset());
    
            iso.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
            Assert.IsTrue(listener.IsInvokedAndReset(), "expected but not received at " + time);
        }
    
        private void SendCurrentTime(EPServiceProviderIsolated iso, string time) {
            iso.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSecWZone(time)));
        }
    
        private SupportBean MakeSendEvent(EPServiceProviderIsolated iso, string theString) {
            return MakeSendEvent(iso, theString, 0);
        }
    
        private SupportBean MakeSendEvent(EPServiceProviderIsolated iso, string theString, int intPrimitive) {
            var b = new SupportBean(theString, intPrimitive);
            iso.EPRuntime.SendEvent(b);
            return b;
        }
    
        public static string ComputeISO8601String(SupportBean bean) {
            return "R/1980-01-01T00:00:00Z/PT" + bean.IntPrimitive + "S";
        }

        public static DateTimeOffset GetThe1980Calendar()
        {
            return DateTimeParser.ParseDefaultExWZone("1980-01-01T00:00:00.000GMT-0:00").DateTime;
        }

        public static DateTimeOffset GetThe1980Date()
        {
            return GetThe1980Calendar();
        }

        public static long GetThe1980Long()
        {
            return GetThe1980Calendar().TimeInMillis();
        }

        public static int GetTheSeconds()
        {
            return 1;
        }
    }
} // end of namespace
