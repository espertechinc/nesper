///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.patternassert
{
	public class ResultAssertExecution
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ResultAssertExecution));
		private static readonly ILog PREFORMATLOG = LogManager.GetLogger("PREFORMATTED");

		private readonly string epl;
		private readonly RegressionEnvironment env;
		private readonly ResultAssertTestResult expected;
		private readonly ResultAssertExecutionTestSelector irTestSelector;

		private static readonly SortedDictionary<long, TimeAction> INPUT = ResultAssertInput.Actions;

		public ResultAssertExecution(
			string epl,
			RegressionEnvironment env,
			ResultAssertTestResult expected) :
			this(epl, env, expected, ResultAssertExecutionTestSelector.TEST_BOTH_ISTREAM_AND_IRSTREAM)
		{
		}

		public ResultAssertExecution(
			string epl,
			RegressionEnvironment env,
			ResultAssertTestResult expected,
			ResultAssertExecutionTestSelector irTestSelector)
		{
			this.epl = epl;
			this.env = env;
			this.expected = expected;
			this.irTestSelector = irTestSelector;
		}

		public void Execute(
			bool assertAllowAnyOrder,
			AtomicLong milestone)
		{
			// run
			var isAssert = Environment.GetEnvironmentVariable("ASSERTION_DISABLED") == null;
			var expectRemoveStream = epl.ToLowerInvariant().Contains("select irstream");
			Execute(isAssert, !expectRemoveStream, assertAllowAnyOrder, milestone);
			env.UndeployAll();

			// Second execution is for IRSTREAM, asserting both the insert and remove stream
			if (!env.IsHA) {
				if (irTestSelector != ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED) {
					var irStreamEPL = epl.Replace("select ", "select irstream ");
					env.CompileDeploy(irStreamEPL).AddListener("s0");
					Execute(isAssert, false, assertAllowAnyOrder, milestone);
					env.UndeployAll();
				}
			}
		}

		private void Execute(
			bool isAssert,
			bool isExpectNullRemoveStream,
			bool assertAllowAnyOrder,
			AtomicLong milestone)
		{
			// For use in join tests, send join-to events
			env.SendEventBean(new SupportBean("IBM", 0));
			env.SendEventBean(new SupportBean("MSFT", 0));
			env.SendEventBean(new SupportBean("YAH", 0));

			if (PREFORMATLOG.IsDebugEnabled) {
				PREFORMATLOG.Debug(
					string.Format("Category: %s   Output rate limiting: %s", expected.Category, expected.Title));
				PREFORMATLOG.Debug("");
				PREFORMATLOG.Debug("Statement:");
				PREFORMATLOG.Debug(epl);
				PREFORMATLOG.Debug("");
				PREFORMATLOG.Debug(string.Format("%28s  %38s", "Input", "Output"));
				PREFORMATLOG.Debug(string.Format("%45s  %15s  %15s", "", "Insert Stream", "Remove Stream"));
				PREFORMATLOG.Debug(
					string.Format(
						"%28s  %30s",
						"-----------------------------------------------",
						"----------------------------------"));
				PREFORMATLOG.Debug(string.Format("%5s %5s%8s%8s", "Time", "Symbol", "Volume", "Price"));
			}

			foreach (var timeEntry in INPUT) {
				env.MilestoneInc(milestone);

				var time = timeEntry.Key;
				var timeInSec = string.Format("%3.1f", time / 1000.0);

				log.Info(".execute At " + timeInSec + " sending timer event");
				SendTimer(time);

				if (PREFORMATLOG.IsDebugEnabled) {
					var comment = timeEntry.Value.ActionDesc;
					comment = (comment == null) ? "" : comment;
					PREFORMATLOG.Debug(string.Format("%5s %24s %s", timeInSec, "", comment));
				}

				ProcessAction(
					time,
					timeInSec,
					timeEntry.Value,
					isAssert,
					isExpectNullRemoveStream,
					assertAllowAnyOrder);
			}
		}

		private void ProcessAction(
			long currentTime,
			string timeInSec,
			TimeAction value,
			bool isAssert,
			bool isExpectNullRemoveStream,
			bool assertAllowAnyOrder)
		{

			var assertions = expected.Assertions.Get(currentTime);

			// Assert step 0 which is the timer event result then send events and assert remaining
			AssertStep(
				timeInSec,
				0,
				assertions,
				expected.Properties,
				isAssert,
				isExpectNullRemoveStream,
				assertAllowAnyOrder);

			for (var step = 1; step < 10; step++) {
				if (value.Events.Count >= step) {
					var sendEvent = value.Events[step - 1];
					log.Info(
						".execute At " +
						timeInSec +
						" sending event: " +
						sendEvent.TheEvent +
						" " +
						sendEvent.EventDesc);
					env.SendEventBean(sendEvent.TheEvent);

					if (PREFORMATLOG.IsDebugEnabled) {
						PREFORMATLOG.Debug(
							string.Format(
								"%5s  %5s%8s %7.1f   %s",
								"",
								sendEvent.TheEvent.Symbol,
								sendEvent.TheEvent.Volume,
								sendEvent.TheEvent.Price,
								sendEvent.EventDesc));
					}
				}

				AssertStep(
					timeInSec,
					step,
					assertions,
					expected.Properties,
					isAssert,
					isExpectNullRemoveStream,
					assertAllowAnyOrder);
			}
		}

		private void AssertStep(
			string timeInSec,
			int step,
			IDictionary<int, StepDesc> stepAssertions,
			string[] fields,
			bool isAssert,
			bool isExpectNullRemoveStream,
			bool assertAllowAnyOrder)
		{

			if (PREFORMATLOG.IsDebugEnabled) {
				if (env.Listener("s0").IsInvoked) {
					var received = RenderReceived(fields);
					var newRows = received.First;
					var oldRows = received.Second;
					var numMaxRows = (newRows.Length > oldRows.Length) ? newRows.Length : oldRows.Length;
					for (var i = 0; i < numMaxRows; i++) {
						var newRow = (newRows.Length > i) ? newRows[i] : "";
						var oldRow = (oldRows.Length > i) ? oldRows[i] : "";
						PREFORMATLOG.Debug(string.Format("%48s %-18s %-20s", "", newRow, oldRow));
					}

					if (numMaxRows == 0) {
						PREFORMATLOG.Debug(string.Format("%48s %-18s %-20s", "", "(empty result)", "(empty result)"));
					}
				}
			}

			if (!isAssert) {
				env.Listener("s0").Reset();
				return;
			}

			StepDesc stepDesc;
			if (stepAssertions != null) {
				stepDesc = stepAssertions.Get(step);
			}
			else {
				stepDesc = null;
			}

			// If there is no assertion, there should be no event received
			env.AssertListener(
				"s0",
				listener => {
					if (stepDesc == null) {
						Assert.IsFalse(listener.IsInvoked, "At time " + timeInSec + " expected no events but received one or more");
					}
					else {
						// If we do expect remove stream events, asset both
						if (!isExpectNullRemoveStream) {
							var message = "At time " + timeInSec;
							Assert.IsTrue(listener.IsInvoked, message + " expected events but received none");
							if (assertAllowAnyOrder) {
								EPAssertionUtil.AssertPropsPerRowAnyOrder(
									listener.LastNewData,
									expected.Properties,
									stepDesc.NewDataPerRow);
								EPAssertionUtil.AssertPropsPerRowAnyOrder(
									listener.LastOldData,
									expected.Properties,
									stepDesc.OldDataPerRow);
							}
							else {
								EPAssertionUtil.AssertPropsPerRow(
									listener.LastNewData,
									expected.Properties,
									stepDesc.NewDataPerRow,
									"newData");
								EPAssertionUtil.AssertPropsPerRow(
									listener.LastOldData,
									expected.Properties,
									stepDesc.OldDataPerRow,
									"oldData");
							}
						}
						else {
							// If we don't expect remove stream events (istream only), then asset new data only if there
							// If we do expect new data, assert
							if (stepDesc.NewDataPerRow != null) {
								Assert.IsTrue(listener.IsInvoked, "At time " + timeInSec + " expected events but received none");
								if (assertAllowAnyOrder) {
									EPAssertionUtil.AssertPropsPerRowAnyOrder(
										listener.LastNewData,
										expected.Properties,
										stepDesc.NewDataPerRow);
								}
								else {
									EPAssertionUtil.AssertPropsPerRow(
										listener.LastNewData,
										expected.Properties,
										stepDesc.NewDataPerRow,
										"newData");
								}
							}
							else {
								// If we don't expect new data, make sure its null
								Assert.IsNull(
									listener.LastNewData,
									"At time " + timeInSec + " expected no insert stream events but received some");
							}

							Assert.IsNull(
								listener.LastOldData,
								"At time " +
								timeInSec +
								" expected no remove stream events but received some(check irstream/istream(default) test case)");
						}
					}

					listener.Reset();
				});
		}

		private UniformPair<string[]> RenderReceived(string[] fields)
		{
			var renderNew = RenderReceived(env.Listener("s0").NewDataListFlattened, fields);
			var renderOld = RenderReceived(env.Listener("s0").OldDataListFlattened, fields);
			return new UniformPair<string[]>(renderNew, renderOld);
		}

		private string[] RenderReceived(
			EventBean[] newDataListFlattened,
			string[] fields)
		{
			if (newDataListFlattened == null) {
				return Array.Empty<string>();
			}

			var result = new string[newDataListFlattened.Length];
			for (var i = 0; i < newDataListFlattened.Length; i++) {
				var values = new object[fields.Length];
				var theEvent = newDataListFlattened[i];
				for (var j = 0; j < fields.Length; j++) {
					values[j] = theEvent.Get(fields[j]);
				}

				result[i] = CompatExtensions.RenderAny(values);
			}

			return result;
		}

		private void SendTimer(long timeInMSec)
		{
			env.AdvanceTime(timeInMSec);
		}

		public static string GetEPL(
			bool join,
			bool irstream,
			string eplNonJoin,
			string eplJoin)
		{
			var eplSelect = join ? eplJoin : eplNonJoin;
			var streamPrefix = irstream ? "select irstream" : "select";
			return "@name('s0') " + streamPrefix + " " + eplSelect;
		}

		public static string GetEPL(
			bool join,
			bool irstream,
			SupportOutputLimitOpt opt,
			string eplNonJoin,
			string eplJoin)
		{
			return opt.GetHint() + ResultAssertExecution.GetEPL(join, irstream, eplNonJoin, eplJoin);
		}
	}
} // end of namespace
