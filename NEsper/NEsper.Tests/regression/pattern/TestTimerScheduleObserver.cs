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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
	public class TestTimerScheduleObserver : SupportBeanConstants
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
	        config.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _listener = new SupportUpdateListener();
	        _epService.Initialize();
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestScheduling()
        {
	        // just-date: "<date>" : non-recurring, typically a future start time, no period
	        RunAssertionJustFutureDate();
	        RunAssertionJustPastDate();

	        // just-period: "P<...>" : non-recurring
	        RunAssertionJustPeriod();

	        // partial-form-2: "<date>/P<period>": non-recurring, no start date (starts from current date), with period
	        RunAssertionDateWithPeriod();

            // partial-form-1: "R<?>/P<period>": recurring, no start date (starts from current date), with period
	        RunAssertionRecurringLimitedWithPeriod();
	        RunAssertionRecurringUnlimitedWithPeriod();
	        RunAssertionRecurringAnchoring();

	        // full form: "R<?>/<date>/P<period>" : recurring, start time, with period
	        RunAssertionFullFormLimitedFutureDated();
	        RunAssertionFullFormLimitedPastDated();
            RunAssertionFullFormUnlimitedFutureDated();
            RunAssertionFullFormUnlimitedPastDated();
	        RunAssertionFullFormUnlimitedPastDatedAnchoring();

            // equivalent formulations
	        RunAssertionEquivalent();

            // invalid tests
	        RunAssertionInvalid();

	        // followed-by
	        RunAssertionFollowedBy();
	        RunAssertionFollowedByDynamicallyComputed();

            // named parameters
	        RunAssertionNameParameters();
        }

	    private void RunAssertionFollowedByDynamicallyComputed()
        {
	        _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("computeISO8601String", this.GetType().FullName, "ComputeISO8601String");

	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");
	        SendCurrentTime(iso, "2012-10-01T05:51:07.000GMT-0:00");

	        string epl = "select * from pattern[every sb=SupportBean -> timer:schedule(iso: computeISO8601String(sb))]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        SupportBean b1 = MakeSendEvent(iso, "E1", 5);

	        SendCurrentTime(iso, "2012-10-01T05:51:09.999GMT-0:00");
	        Assert.IsFalse(_listener.IsInvokedAndReset());

	        SendCurrentTime(iso, "2012-10-01T05:51:10.000GMT-0:00");
	        Assert.AreEqual(b1, _listener.AssertOneGetNewAndReset().Get("sb"));

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionFollowedBy() {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");
	        SendCurrentTime(iso, "2012-10-01T05:51:07.000GMT-0:00");

	        string epl = "select * from pattern[every sb=SupportBean -> timer:schedule(iso: 'R/1980-01-01T00:00:00Z/PT15S')]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        SupportBean b1 = MakeSendEvent(iso, "E1");

	        SendCurrentTime(iso, "2012-10-01T05:51:14.999GMT-0:00");
	        Assert.IsFalse(_listener.IsInvokedAndReset());

	        SendCurrentTime(iso, "2012-10-01T05:51:15.000GMT-0:00");
	        Assert.AreEqual(b1, _listener.AssertOneGetNewAndReset().Get("sb"));

	        SendCurrentTime(iso, "2012-10-01T05:51:16.000GMT-0:00");
	        SupportBean b2 = MakeSendEvent(iso, "E2");

	        SendCurrentTime(iso, "2012-10-01T05:51:18.000GMT-0:00");
	        SupportBean b3 = MakeSendEvent(iso, "E3");

	        SendCurrentTime(iso, "2012-10-01T05:51:30.000GMT-0:00");
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "sb".Split(','), new object[][]{new object[] {b2}, new object[] {b3}});

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionInvalid() {

	        // the ISO 8601 parse tests reside with the parser
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from pattern[every timer:schedule(iso: 'x')]",
	                "Invalid parameter for pattern observer 'timer:schedule(iso:\"x\")': Failed to parse 'x': Exception parsing date 'x', the date is not a supported ISO 8601 date");

	        // named parameter tests: absence, typing, etc.
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from pattern[timer:schedule()]",
	                "Invalid parameter for pattern observer 'timer:schedule()': No parameters provided");
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from pattern[timer:schedule(x:1)]",
	                "Invalid parameter for pattern observer 'timer:schedule(x:1)': Unexpected named parameter 'x', expecting any of the following: [iso, repetitions, date, period]");
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from pattern[timer:schedule(period:1)]",
	                "Invalid parameter for pattern observer 'timer:schedule(period:1)': Failed to validate named parameter 'period', expected a single expression returning a TimePeriod-typed value");
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from pattern[timer:schedule(repetitions:'a', period:1 seconds)]",
	                "Invalid parameter for pattern observer 'timer:schedule(repetitions:\"a\",period:1 seconds)': Failed to validate named parameter 'repetitions', expected a single expression returning any of the following types: int,long");
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from pattern[timer:schedule(date:1 seconds)]",
	                "Invalid parameter for pattern observer 'timer:schedule(date:1 seconds)': Failed to validate named parameter 'date', expected a single expression returning any of the following types: string,DateTime,DateTimeOffset,long");
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from pattern[timer:schedule(repetitions:1)]",
	                "Invalid parameter for pattern observer 'timer:schedule(repetitions:1)': Either the date or period parameter is required");
	        SupportMessageAssertUtil.TryInvalid(_epService, "select * from pattern[timer:schedule(iso: 'R/1980-01-01T00:00:00Z/PT15S', repetitions:1)]",
	                "Invalid parameter for pattern observer 'timer:schedule(iso:\"R/1980-01-01T00:00:00Z/PT15S\",repetitions:1)': The 'iso' parameter is exclusive of other parameters");
	    }

	    private void RunAssertionEquivalent() {

	        string first = "select * from pattern[every timer:schedule(iso: 'R2/2008-03-01T13:00:00Z/P1Y2M10DT2H30M')]";
	        RunAssertionEquivalent(first);

	        string second = "select * from pattern[every " +
	                "(timer:schedule(iso: '2008-03-01T13:00:00Z') or" +
	                " timer:schedule(iso: '2009-05-11T15:30:00Z'))]";
	        RunAssertionEquivalent(second);

	        string third = "select * from pattern[every " +
	                "(timer:schedule(iso: '2008-03-01T13:00:00Z') or" +
	                " timer:schedule(iso: '2008-03-01T13:00:00Z/P1Y2M10DT2H30M'))]";
	        RunAssertionEquivalent(third);
	    }

	    private void RunAssertionEquivalent(string epl) {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");
	        SendCurrentTime(iso, "2001-10-01T05:51:00.000GMT-0:00");

	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2008-03-01T13:00:00.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2009-05-11T15:30:00.000GMT-0:00");
	        AssertSendNoMoreCallback(iso, "2012-10-01T05:52:04.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionDateWithPeriod() {
	        RunAssertionDateWithPeriod("iso: '2012-10-01T05:52:00Z/PT2S'");
	        RunAssertionDateWithPeriod("date: '2012-10-01T05:52:00Z', period: 2 seconds");
	    }

	    private void RunAssertionDateWithPeriod(string parameters) {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");
	        SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");

	        // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
	        string epl = "select * from pattern[timer:schedule(" + parameters + ")]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2012-10-01T05:52:02.000GMT-0:00");
	        AssertSendNoMoreCallback(iso, "2012-10-01T05:52:04.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionFullFormLimitedFutureDated() {
	        RunAssertionFullFormLimitedFutureDated(true, "iso: 'R3/2012-10-01T05:52:00Z/PT2S'");
	        RunAssertionFullFormLimitedFutureDated(false, "iso: 'R3/2012-10-01T05:52:00Z/PT2S'");
	        RunAssertionFullFormLimitedFutureDated(false, "repetitions: 3L, date:'2012-10-01T05:52:00Z', period: 2 seconds");
	    }

	    private void RunAssertionFullFormLimitedFutureDated(bool audit, string parameters) {

	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");
	        SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");

	        // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
	        string epl = (audit ? "@Audit " : "") + "select * from pattern[every timer:schedule(" + parameters + ")]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:52:02.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:52:04.000GMT-0:00");
	        AssertSendNoMoreCallback(iso, "2012-10-01T05:52:06.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionJustFutureDate() {
	        RunAssertionJustFutureDate(true, "iso: '2012-10-01T05:52:00Z'");
	        RunAssertionJustFutureDate(false, "iso: '2012-10-01T05:52:00Z'");
	        RunAssertionJustFutureDate(false, "date: '2012-10-01T05:52:00Z'");
	    }

	    private void RunAssertionJustFutureDate(bool hasEvery, string parameters) {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");
	        SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");

	        // Fire once at "2012-10-01T05:52:00Z" (UTC)
	        string epl = "select * from pattern[" + (hasEvery ? "every " : "") + "timer:schedule(" + parameters + ")]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
	        AssertSendNoMoreCallback(iso, "2012-10-01T05:53:00.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionJustPastDate() {
	        RunAssertionJustPastDate(true);
	        RunAssertionJustPastDate(false);
	    }

	    private void RunAssertionJustPastDate(bool hasEvery) {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");
	        SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");

	        // Fire once at "2012-10-01T05:52:00Z" (UTC)
	        string epl = "select * from pattern[" + (hasEvery ? "every " : "") + "timer:schedule(iso: '2010-10-01T05:52:00Z')]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertSendNoMoreCallback(iso, "2012-10-01T05:53:00.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionJustPeriod() {
	        RunAssertionJustPeriod("iso:'P1DT2H'");
	        RunAssertionJustPeriod("period: 1 day 2 hours");
	    }

	    private void RunAssertionJustPeriod(string parameters) {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");
	        SendCurrentTime(iso, "2012-10-01T05:51:00.000GMT-0:00");

	        // Fire once after 1 day and 2 hours
	        string epl = "select * from pattern[timer:schedule(" + parameters + ")]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2012-10-02T07:51:00.000GMT-0:00");
	        AssertSendNoMoreCallback(iso, "2012-10-03T09:51:00.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionRecurringLimitedWithPeriod() {
	        RunAssertionRecurringLimitedWithPeriod("iso:'R3/PT2S'");
	        RunAssertionRecurringLimitedWithPeriod("repetitions:3L, period: 2 seconds");
	    }

	    private void RunAssertionRecurringLimitedWithPeriod(string parameters) {

	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");

	        // Fire 3 times after 2 seconds from current time
	        SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
	        string epl = "select * from pattern[every timer:schedule(" + parameters + ")]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2012-10-01T05:52:02.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:52:04.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:52:06.000GMT-0:00");
	        AssertSendNoMoreCallback(iso, "2012-10-01T05:52:08.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionRecurringUnlimitedWithPeriod() {

	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");

	        // Fire 3 times after 2 seconds from current time
	        SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
	        string epl = "select * from pattern[every timer:schedule(iso:'R/PT1M10S')]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2012-10-01T05:53:10.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:54:20.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:55:30.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:56:40.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionFullFormUnlimitedPastDated() {

	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");

	        // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
	        SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
	        string epl = "select * from pattern[every timer:schedule(iso:'R/1980-01-01T00:00:00Z/PT1S')]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2012-10-01T05:52:01.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:52:02.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:52:03.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionNameParameters() {
	        _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("getThe1980Calendar", this.GetType().FullName, "GetThe1980Calendar");
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("getThe1980Date", this.GetType().FullName, "GetThe1980Date");
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("getThe1980Long", this.GetType().FullName, "GetThe1980Long");
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("getTheSeconds", this.GetType().FullName, "GetTheSeconds");

	        RunAssertionNameParameters("repetitions:-1L, date:'1980-01-01T00:00:00Z', period: 1 seconds");
	        RunAssertionNameParameters("repetitions:-1, date:getThe1980Calendar(), period: 1 seconds");
	        RunAssertionNameParameters("repetitions:-1, date:getThe1980Date(), period: getTheSeconds() seconds");
	        RunAssertionNameParameters("repetitions:-1, date:getThe1980Long(), period: 1 seconds");
	    }

	    private void RunAssertionNameParameters(string parameters) {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");

	        // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
	        SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
	        string epl = "select * from pattern[every timer:schedule(" + parameters + ")]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2012-10-01T05:52:01.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:52:02.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2012-10-01T05:52:03.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionFullFormUnlimitedPastDatedAnchoring() {

	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");

	        // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
	        SendCurrentTime(iso, "2012-01-01T00:00:00.000GMT-0:00");
	        string epl = "select * from pattern[every timer:schedule(iso:'R/1980-01-01T00:00:00Z/PT10S')]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        SendCurrentTime(iso, "2012-01-01T00:00:15.000GMT-0:00");
	        Assert.IsTrue(_listener.IsInvokedAndReset());

	        SendCurrentTime(iso, "2012-01-01T00:00:20.000GMT-0:00");
	        Assert.IsTrue(_listener.IsInvokedAndReset());

	        AssertReceivedAtTime(iso, "2012-01-01T00:00:30.000GMT-0:00");

	        SendCurrentTime(iso, "2012-01-01T00:00:55.000GMT-0:00");
	        Assert.IsTrue(_listener.IsInvokedAndReset());

	        AssertReceivedAtTime(iso, "2012-01-01T00:01:00.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionRecurringAnchoring() {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");

	        // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
	        SendCurrentTime(iso, "2012-01-01T00:00:00.000GMT-0:00");
	        string epl = "select * from pattern[every timer:schedule(iso: 'R/PT10S')]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        SendCurrentTime(iso, "2012-01-01T00:00:15.000GMT-0:00");
	        Assert.IsTrue(_listener.IsInvokedAndReset());

	        SendCurrentTime(iso, "2012-01-01T00:00:20.000GMT-0:00");
	        Assert.IsTrue(_listener.IsInvokedAndReset());

	        AssertReceivedAtTime(iso, "2012-01-01T00:00:30.000GMT-0:00");

	        SendCurrentTime(iso, "2012-01-01T00:00:55.000GMT-0:00");
	        Assert.IsTrue(_listener.IsInvokedAndReset());

	        AssertReceivedAtTime(iso, "2012-01-01T00:01:00.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionFullFormLimitedPastDated() {

	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");

	        // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
	        SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
	        string epl = "select * from pattern[every timer:schedule(iso: 'R8/2012-10-01T05:51:00Z/PT10S')]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2012-10-01T05:52:10.000GMT-0:00");
	        AssertSendNoMoreCallback(iso, "2012-10-01T05:52:20.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void RunAssertionFullFormUnlimitedFutureDated() {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");

	        // Repeat unlimited number of times, reference-dated to future date, period of 1 day
	        SendCurrentTime(iso, "2012-10-01T05:52:00.000GMT-0:00");
	        string epl = "select * from pattern[every timer:schedule(iso: 'R/2013-01-01T02:00:05Z/P1D')]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        AssertReceivedAtTime(iso, "2013-01-01T02:00:05.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2013-01-02T02:00:05.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2013-01-03T02:00:05.000GMT-0:00");
	        AssertReceivedAtTime(iso, "2013-01-04T02:00:05.000GMT-0:00");

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void AssertSendNoMoreCallback(EPServiceProviderIsolated iso, string time) {
	        SendCurrentTime(iso, time);
	        Assert.IsFalse(_listener.IsInvokedAndReset());
	        SendCurrentTime(iso, "2999-01-01T00:00:00.000GMT-0:00");
	        Assert.IsFalse(_listener.IsInvokedAndReset());
	    }

	    private void AssertReceivedAtTime(EPServiceProviderIsolated iso, string time) {
            long msec = DateTimeParser.ParseDefaultMSecWZone(time);

	        iso.EPRuntime.SendEvent(new CurrentTimeEvent(msec - 1));
	        Assert.IsFalse(_listener.IsInvokedAndReset());

	        iso.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
            Assert.IsTrue(_listener.IsInvokedAndReset(), "expected but not received at " + time);
	    }

	    private void SendCurrentTime(EPServiceProviderIsolated iso, string time) {
            iso.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSecWZone(time)));
	    }

	    private SupportBean MakeSendEvent(EPServiceProviderIsolated iso, string theString) {
	        return MakeSendEvent(iso, theString, 0);
	    }

	    private SupportBean MakeSendEvent(EPServiceProviderIsolated iso, string theString, int intPrimitive) {
	        SupportBean b = new SupportBean(theString, intPrimitive);
	        iso.EPRuntime.SendEvent(b);
	        return b;
	    }

	    public static string ComputeISO8601String(SupportBean bean)
        {
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
