///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.patternassert
{
	/// <summary>
	/// Test harness for testing expressions and comparing received MatchedEventMap instances against against expected results.
	/// </summary>
	public class PatternTestHarness
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(PatternTestHarness));

		private readonly EventCollection sendEventCollection;
		private readonly CaseList caseList;

		// Array of expressions and match listeners for listening to events for each test descriptor

		public PatternTestHarness(
			EventCollection sendEventCollection,
			CaseList caseList)
		{
			this.sendEventCollection = sendEventCollection;
			this.caseList = caseList;
		}

		public PatternTestHarness(
			EventCollection sendEventCollection,
			EventExpressionCase testCase)
		{
			this.sendEventCollection = sendEventCollection;
			this.caseList = new CaseList();
			this.caseList.AddTest(testCase);
		}

		public void RunTest(RegressionEnvironment env)
		{
			var milestone = new AtomicLong();
			if (!env.IsHA) {
				RunTest(env, PatternTestStyle.USE_EPL, milestone);
				RunTest(env, PatternTestStyle.COMPILE_TO_MODEL, milestone);
				RunTest(env, PatternTestStyle.COMPILE_TO_EPL, milestone);
				RunTest(env, PatternTestStyle.USE_EPL_AND_CONSUME_NOCHECK, milestone);
			}
			else {
				RunTest(env, PatternTestStyle.USE_EPL, milestone);
				RunTest(env, PatternTestStyle.COMPILE_TO_EPL, milestone);
			}
		}

		private void RunTest(
			RegressionEnvironment env,
			PatternTestStyle testStyle,
			AtomicLong milestone)
		{

			// Send the start time to the eventService
			if (sendEventCollection.GetTime(EventCollection.ON_START_EVENT_ID) != null) {
				var startTime = sendEventCollection.GetTime(EventCollection.ON_START_EVENT_ID);
				env.AdvanceTime(startTime.Value);
				log.Debug(".RunTest Start time is " + startTime);
			}

			// Set up expression filters and match listeners
			var expressions = new string[caseList.NumTests];
			var index = -1;
			foreach (var descriptor in caseList.Results) {
				index++;
				var epl = descriptor.ExpressionText;
				var model = descriptor.ObjectModel;
				var statementName = NameOfStatement(descriptor);
				var nameAnnotation = "@name(\"" + statementName + "\") ";
				EPCompiled compiled;
				log.Debug(".runTest Deploying " + epl);

				try {
					if (model != null) {
						model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation(statementName));
						var module = new Module();
						module.Items.Add(new ModuleItem(model));
						module.ModuleText = model.ToEPL();
						compiled = env.Compile(module);
					}
					else {
						if (testStyle == PatternTestStyle.USE_EPL) {
							var text = nameAnnotation +
							           "@Audit('pattern') @Audit('pattern-instances') select * from pattern [" +
							           epl +
							           "]";
							compiled = env.Compile(text);
							epl = text;
						}
						else if (testStyle == PatternTestStyle.USE_EPL_AND_CONSUME_NOCHECK) {
							var text = nameAnnotation +
							           "select * from pattern @DiscardPartialsOnMatch @SuppressOverlappingMatches [" +
							           epl +
							           "]";
							compiled = env.Compile(text);
							epl = text;
						}
						else if (testStyle == PatternTestStyle.COMPILE_TO_MODEL) {
							var text = nameAnnotation + "select * from pattern [" + epl + "]";
							var mymodel = env.Compiler.EplToModel(text, env.Configuration);
							var module = new Module();
							module.Items.Add(new ModuleItem(mymodel));
							compiled = env.Compiler.Compile(module, new CompilerArguments(env.Configuration));
							epl = text;
						}
						else if (testStyle == PatternTestStyle.COMPILE_TO_EPL) {
							var text = "select * from pattern [" + epl + "]";
							var mymodel = env.EplToModel(text);
							var reverse = nameAnnotation + mymodel.ToEPL();
							compiled = env.Compile(reverse);
							epl = reverse;
						}
						else {
							throw new ArgumentException("Unknown test style");
						}
					}
				}
				catch (Exception ex) {
					var text = epl;
					if (model != null) {
						text = "Model: " + model.ToEPL();
					}

					log.Error(
						".RunTest Failed to create statement for style " + testStyle + " pattern expression=" + text,
						ex);
					Assert.Fail();
					compiled = null;
				}

				// We stop the statement again and start after the first listener was added.
				// Thus we can handle patterns that fireStatementStopped on startup.
				var unit = compiled;
				env.Deploy(unit).AddListener(statementName);
				expressions[index] = epl;
			}

			// milestone
			env.Milestone(milestone.GetAndIncrement());

			// Some expressions may fireStatementStopped as soon as they are started, such as a "not b()" expression, for example.
			// Check results for any such listeners/expressions.
			// NOTE: For EPL statements we do not support calling listeners when a pattern that fires upon start.
			// Reason is that this should not be a relevant functionality of a pattern, the start pattern
			// event itself cannot carry any information and is thus ignore. Note subsequent events
			// generated by the same pattern are fine.
			env.AssertThat(() => CheckResults(testStyle, EventCollection.ON_START_EVENT_ID, expressions, env));
			var totalEventsReceived = new AtomicLong(CountExpectedEvents(EventCollection.ON_START_EVENT_ID));
			ClearListenerEvents(caseList, env);

			// Send actual test events
			foreach (var entry in sendEventCollection) {
				var eventId = entry.Key;

				// Manipulate the time when this event was send
				if (sendEventCollection.GetTime(eventId) != null) {
					if (sendEventCollection.TryGetTime(eventId, out var currentTime)) {
						env.AdvanceTime(currentTime);
						log.Debug(
							".RunTest Sending event " +
							entry.Key +
							" = " +
							entry.Value +
							"  timed " +
							currentTime);
					}
				}

				// Send event itself
				env.SendEventBean(entry.Value);

				// Check expected results for this event
				env.AssertThat(
					() => {
						if (testStyle != PatternTestStyle.USE_EPL_AND_CONSUME_NOCHECK) {
							env.AssertThat(() => CheckResults(testStyle, eventId, expressions, env));

							// Count and clear the list of events that each listener has received
							totalEventsReceived.IncrementAndGet(CountListenerEvents(caseList, env));
						}

						ClearListenerEvents(caseList, env);
					});

				env.Milestone(milestone.GetAndIncrement());
			}

			// Count number of expected matches
			var totalExpected = new AtomicLong();
			foreach (var descriptor in caseList.Results) {
				foreach (var events in descriptor.ExpectedResults.Values) {
					totalExpected.IncrementAndGet(events.Count);
				}
			}

			env.AssertThat(
				() => {
					if (totalExpected.Get() != totalEventsReceived.Get() &&
					    testStyle != PatternTestStyle.USE_EPL_AND_CONSUME_NOCHECK) {
						log.Debug(
							".test Count expected does not match count received, expected=" +
							totalExpected +
							" received=" +
							totalEventsReceived);
						Assert.Fail();
					}
				});

			// Kill all expressions
			env.UndeployAll();

			// Send test events again to also test that all were indeed killed
			foreach (var entry in sendEventCollection) {
				env.SendEventBean(entry.Value);
			}

			// Make sure all listeners are still at zero
			foreach (var descriptor in caseList.Results) {
				var statementName = NameOfStatement(descriptor);
				env.AssertThat(() => Assert.IsNull(env.Statement(statementName)));
			}
		}

		private void CheckResults(
			PatternTestStyle testStyle,
			string eventId,
			string[] expressions,
			RegressionEnvironment env)
		{
			// For each test descriptor, make sure the listener has received exactly the events expected
			var index = 0;
			log.Debug(".checkResults Checking results for event " + eventId);

			foreach (var descriptor in caseList.Results) {
				var statementName = NameOfStatement(descriptor);
				var expressionText = expressions[index];

				var allExpectedResults = descriptor.ExpectedResults;

				var listener = env.Listener(statementName);
				var receivedResults = listener.LastNewData;
				index++;

				// If nothing at all was expected for this event, make sure nothing was received
				if (!(allExpectedResults.ContainsKey(eventId))) {
					if ((receivedResults != null) && (receivedResults.Length > 0)) {
						log.Debug(
							".checkResults Incorrect result for style " +
							testStyle +
							" expression : " +
							expressionText);
						log.Debug(
							".checkResults Expected no results for event " +
							eventId +
							", but received " +
							receivedResults.Length +
							" events");
						log.Debug(".checkResults Received, have " + receivedResults.Length + " entries");
						PrintList(receivedResults);
						Assert.Fail();
					}

					continue;
				}

				var expectedResults = allExpectedResults.Get(eventId);

				// Compare the result lists, not caring about the order of the elements
				try {
					if (!(CompareLists(receivedResults, expectedResults))) {
						log.Debug(
							".checkResults Incorrect result for style " +
							testStyle +
							" expression : " +
							expressionText);
						log.Debug(
							".checkResults Expected size=" +
							expectedResults.Count +
							" received size=" +
							(receivedResults == null ? 0 : receivedResults.Length));

						log.Debug(".checkResults Expected, have " + expectedResults.Count + " entries");
						PrintList(expectedResults);
						log.Debug(
							".checkResults Received, have " +
							(receivedResults == null ? 0 : receivedResults.Length) +
							" entries");
						PrintList(receivedResults);

						Assert.Fail();
					}
				}
				catch (Exception ex) {
					log.Error("Unexpected exception", ex);
					Assert.Fail("For statement '" + expressionText + "' failed to assert: " + ex.Message);
				}
			}
		}

		private bool CompareLists(
			ICollection<EventBean> receivedResults,
			ICollection<EventDescriptor> expectedResults)
		{
			var receivedSize = receivedResults?.Count ?? 0;
			if (expectedResults.Count != receivedSize) {
				return false;
			}

			// To make sure all received events have been expected
			var expectedResultsClone = new LinkedList<EventDescriptor>(expectedResults);

			// Go through the list of expected results and remove from received result list if found
			foreach (var desc in expectedResults) {
				EventDescriptor foundMatch = null;

				foreach (var received in receivedResults) {
					if (CompareEvents(desc, received)) {
						foundMatch = desc;
						break;
					}
				}

				// No match between expected and received
				if (foundMatch == null) {
					return false;
				}

				expectedResultsClone.Remove(foundMatch);
			}

			// Any left over received results also invalidate the test
			if (expectedResultsClone.Count > 0) {
				return false;
			}

			return true;
		}

		private static bool CompareEvents(
			EventDescriptor eventDesc,
			EventBean eventBean)
		{
			foreach (var entry in eventDesc.EventProperties) {
				var result = eventBean.Get(entry.Key);
				if ((result == null) && (entry.Value == null)) {
					continue;
				}

				if (result == null) {
					log.Debug("For tag " + entry.Key + " the value is NULL");
					return false;
				}

				if (!result.Equals(entry.Value)) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Clear the event list of all listeners
		/// </summary>
		/// <param name="caseList"></param>
		/// <param name="env"></param>
		private int CountListenerEvents(
			CaseList caseList,
			RegressionEnvironment env)
		{
			var count = 0;
			foreach (var descriptor in caseList.Results) {
				var statementName = NameOfStatement(descriptor);
				foreach (var events in env.Listener(statementName).NewDataList) {
					count += events.Length;
				}
			}

			return count;
		}

		private void ClearListenerEvents(
			CaseList caseList,
			RegressionEnvironment env)
		{
			foreach (var descriptor in caseList.Results) {
				var statementName = NameOfStatement(descriptor);
				env.ListenerReset(statementName);
			}
		}

		private void PrintList(IEnumerable<EventDescriptor> events)
		{
			var index = 0;
			foreach (var desc in events) {
				var buffer = new StringBuilder();
				var count = 0;

				foreach (var entry in desc.EventProperties) {
					buffer.Append(" (" + (count++) + ") ");
					buffer.Append("tag=" + entry.Key);

					var id = FindValue(entry.Value);
					buffer.Append("  eventId=" + id);
				}

				log.Debug(".printList (" + index + ") : " + buffer);
				index++;
			}
		}

		private void PrintList(EventBean[] events)
		{
			if (events == null) {
				log.Debug(".printList : null-value events array");
				return;
			}

			log.Debug(".printList : " + events.Length + " elements...");
			for (var i = 0; i < events.Length; i++) {
				log.Debug("  " + EventBeanUtility.PrintEvent(events[i]));
			}
		}

		/// <summary>
		/// Find the value object in the map of object names and values
		/// </summary>
		private string FindValue(object value)
		{
			foreach (var entry in sendEventCollection) {
				if (value == entry.Value) {
					return entry.Key;
				}
			}

			return null;
		}

		private int CountExpectedEvents(string eventId)
		{
			var result = 0;
			foreach (var descriptor in caseList.Results) {
				var allExpectedResults = descriptor.ExpectedResults;

				// If nothing at all was expected for this event, make sure nothing was received
				if (allExpectedResults.ContainsKey(eventId)) {
					result++;
				}
			}

			return result;
		}

		private string NameOfStatement(EventExpressionCase descriptor)
		{
            return "name--" + descriptor.ExpressionText;
		}
		
		private enum PatternTestStyle
		{
			USE_EPL,
			COMPILE_TO_MODEL,
			COMPILE_TO_EPL,
			USE_EPL_AND_CONSUME_NOCHECK,
		}
	}
} // end of namespace
