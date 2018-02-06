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
using System.Reflection;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.support
{
    /// <summary>
    /// Test harness for testing expressions and comparing received MatchedEventMap instances 
    /// against against expected results.
    /// </summary>
    public class PatternTestHarness : SupportBeanConstants
    {
        private readonly EventCollection _sendEventCollection;
        private readonly CaseList _caseList;
        private readonly Type _testClass;

        // Array of expressions and match listeners for listening to events for each test descriptor
        private readonly EPStatement[] _expressions;
        private readonly SupportUpdateListener[] _listeners;

        public PatternTestHarness(
            EventCollection sendEventCollection,
            CaseList caseList,
            Type testClass)
        {
            _sendEventCollection = sendEventCollection;
            _caseList = caseList;
            _testClass = testClass;
    
            // Create a listener for each test descriptor
            _listeners = new SupportUpdateListener[_caseList.Count];
            for (int i = 0; i < _listeners.Length; i++)
            {
                _listeners[i] = new SupportUpdateListener();
            }
            _expressions = new EPStatement[_listeners.Length];
        }

        public static void RunTest(EventCollection eventCollection, EventExpressionCase caseExpr, Type testClass)
        {
            var caseList = new CaseList();
            caseList.AddTest(caseExpr);

            var config = SupportConfigFactory.GetConfiguration();
            var serviceProvider = EPServiceProviderManager.GetDefaultProvider(config);
            serviceProvider.Initialize();

            var patternTestHarness = new PatternTestHarness(eventCollection, caseList, testClass);
            patternTestHarness.RunTest(serviceProvider);
        }

        public void RunTest(EPServiceProvider epService)
        {
            var config = epService.EPAdministrator.Configuration;
            config.AddEventType("A", typeof(SupportBean_A));
            config.AddEventType("B", typeof(SupportBean_B));
            config.AddEventType("C", typeof(SupportBean_C));
            config.AddEventType("D", typeof(SupportBean_D));
            config.AddEventType("E", typeof(SupportBean_E));
            config.AddEventType("F", typeof(SupportBean_F));
            config.AddEventType("G", typeof(SupportBean_G));

            RunTest(epService, PatternTestStyle.USE_PATTERN_LANGUAGE);
            RunTest(epService, PatternTestStyle.USE_EPL);
            RunTest(epService, PatternTestStyle.COMPILE_TO_MODEL);
            RunTest(epService, PatternTestStyle.COMPILE_TO_EPL);
            RunTest(epService, PatternTestStyle.USE_EPL_AND_CONSUME_NOCHECK);
        }

        private void RunTest(EPServiceProvider epService, PatternTestStyle testStyle)
        {
            var runtime = epService.EPRuntime;

            // Send the start time to the runtime
            if (_sendEventCollection.GetTime(EventCollection.ON_START_EVENT_ID) != null)
            {
                TimerEvent startTime = new CurrentTimeEvent(_sendEventCollection.GetTime(EventCollection.ON_START_EVENT_ID).Value);
                runtime.SendEvent(startTime);
                Log.Debug("RunTest: Start time is " + startTime);
            }
    
            // Set up expression filters and match listeners
    
            int index = 0;
            foreach (EventExpressionCase descriptor in _caseList.Results)
            {
                String expressionText = descriptor.ExpressionText;
                EPStatementObjectModel model = descriptor.ObjectModel;
    
                EPStatement statement = null;
    
                try
                {
                    if (model != null)
                    {
                        statement = epService.EPAdministrator.Create(model, "name--" + expressionText);
                    }
                    else
                    {
                        if (testStyle == PatternTestStyle.USE_PATTERN_LANGUAGE)
                        {
                            statement = epService.EPAdministrator.CreatePattern(expressionText, "name--" + expressionText);
                        }
                        else if (testStyle == PatternTestStyle.USE_EPL)
                        {
                            String text = "@Audit('pattern') @Audit('pattern-instances') select * from pattern [" + expressionText + "]";
                            statement = epService.EPAdministrator.CreateEPL(text);
                            expressionText = text;
                        }
                        else if (testStyle == PatternTestStyle.USE_EPL_AND_CONSUME_NOCHECK)
                        {
                            String text = "select * from pattern @DiscardPartialsOnMatch @SuppressOverlappingMatches [" + expressionText + "]";
                            statement = epService.EPAdministrator.CreateEPL(text);
                            expressionText = text;
                        }
                        else if (testStyle == PatternTestStyle.COMPILE_TO_MODEL)
                        {
                            String text = "select * from pattern [" + expressionText + "]";
                            EPStatementObjectModel mymodel = epService.EPAdministrator.CompileEPL(text);
                            statement = epService.EPAdministrator.Create(mymodel);
                            expressionText = text;
                        }
                        else if (testStyle == PatternTestStyle.COMPILE_TO_EPL)
                        {
                            String text = "select * from pattern [" + expressionText + "]";
                            EPStatementObjectModel mymodel = epService.EPAdministrator.CompileEPL(text);
                            String reverse = mymodel.ToEPL();
                            statement = epService.EPAdministrator.CreateEPL(reverse);
                            expressionText = reverse;
                        }
                        else
                        {
                            throw new ArgumentException("Unknown test style");
                        }
                    }
                }
                catch (Exception ex)
                {
                    String text = expressionText;
                    if (model != null)
                    {
                        text = "Model: " + model.ToEPL();
                    }
                    Log.Error(".runTest Failed to create statement for style " + testStyle + " pattern expression=" + text, ex);
                    Assert.Fail(".runTest Failed to create statement for style " + testStyle + " pattern expression=" + text);
                }
    
                // We stop the statement again and start after the first listener was added.
                // Thus we can handle patterns that fireStatementStopped on startup.
                statement.Stop();
    
                _expressions[index] = statement;
                _expressions[index].Events += _listeners[index].Update;
    
                // Start the statement again: listeners now got called for on-start events such as for a "not"
                statement.Start();
    
                index++;
            }
    
            // Some expressions may fireStatementStopped as soon as they are started, such as a "not b()" expression, for example.
            // Check results for any such listeners/expressions.
            // NOTE: For EPL statements we do not support calling listeners when a pattern that fires upon start.
            // Reason is that this should not be a relevant functionality of a pattern, the start pattern
            // event itself cannot carry any information and is thus ignore. Note subsequent events
            // generated by the same pattern are fine.
            int totalEventsReceived = 0;
            if (testStyle != PatternTestStyle.USE_PATTERN_LANGUAGE)
            {
                ClearListenerEvents();
                totalEventsReceived += CountExpectedEvents(EventCollection.ON_START_EVENT_ID);
            }
            else    // Patterns do need to handle event publishing upon pattern expression start (patterns that turn true right away)
            {
                CheckResults(testStyle, EventCollection.ON_START_EVENT_ID);
                totalEventsReceived += CountListenerEvents();
                ClearListenerEvents();
            }
    
            // Send actual test events
            var entryCollection = _sendEventCollection.ToArray();
            for(int ii = 0 ; ii < entryCollection.Length ; ii++)
            {
                var entry = entryCollection[ii];
                var eventId = entry.Key;
    
                // Manipulate the time when this event was send
                if (_sendEventCollection.GetTime(eventId) != null)
                {
                    TimerEvent currentTimeEvent = new CurrentTimeEvent(_sendEventCollection.GetTime(eventId).Value);
                    runtime.SendEvent(currentTimeEvent);
                    Log.Debug("RunTest: Sending event {0} = {1} timed {2}", entry.Key, entry.Value, currentTimeEvent);
                }
    
                // Send event itself
                runtime.SendEvent(entry.Value);
    
                // Check expected results for this event
                if (testStyle != PatternTestStyle.USE_EPL_AND_CONSUME_NOCHECK)
                {
                    CheckResults(testStyle, eventId);

                    // Count and clear the list of events that each listener has received
                    totalEventsReceived += CountListenerEvents();
                }

                ClearListenerEvents();
            }
    
            // Count number of expected matches
            int totalExpected = 0;
            foreach (EventExpressionCase descriptor in _caseList.Results)
            {
                totalExpected += descriptor.ExpectedResults.Values.Sum(events => events.Count);
            }

            if (totalExpected != totalEventsReceived && testStyle != PatternTestStyle.USE_EPL_AND_CONSUME_NOCHECK)
            {
                Log.Debug(".test Count expected does not match count received, expected=" + totalExpected +
                        " received=" + totalEventsReceived);
                Assert.Fail();
            }
    
            // Kill all expressions
            foreach (EPStatement expression in _expressions)
            {
                expression.RemoveAllEventHandlers();
            }
    
            // Send test events again to also test that all were indeed killed
            foreach (var entry in _sendEventCollection)
            {
                runtime.SendEvent(entry.Value);
            }
    
            // Make sure all listeners are still at zero
            foreach (SupportUpdateListener listener in _listeners)
            {
                if (listener.NewDataList.Count > 0)
                {
                    Log.Debug(".test A match was received after stopping all expressions");
                    Assert.Fail();
                }
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void CheckResults(PatternTestStyle testStyle, String eventId)
        {
            // For each test descriptor, make sure the listener has received exactly the events expected
            int index = 0;
            Log.Debug("CheckResults: Checking results for event " + eventId);
    
            foreach (EventExpressionCase descriptor in _caseList.Results)
            {
                String expressionText = _expressions[index].Text;
    
                LinkedHashMap<String, LinkedList<EventDescriptor>> allExpectedResults = descriptor.ExpectedResults;
                EventBean[] receivedResults = _listeners[index].LastNewData;
                index++;
    
                // If nothing at all was expected for this event, make sure nothing was received
                if (!(allExpectedResults.ContainsKey(eventId)))
                {
                    if ((receivedResults != null) && (receivedResults.Length > 0))
                    {
                        Log.Debug("CheckResults: Incorrect result for style " + testStyle + " expression : " + expressionText);
                        Log.Debug("CheckResults: Expected no results for event " + eventId + ", but received " + receivedResults.Length + " events");
                        Log.Debug("CheckResults: Received, have " + receivedResults.Length + " entries");
                        PrintList(receivedResults);
                        Assert.Fail();
                    }
                    continue;
                }
    
                LinkedList<EventDescriptor> expectedResults = allExpectedResults.Get(eventId);
    
                // Compare the result lists, not caring about the order of the elements
                try {
                    if (!(CompareLists(receivedResults, expectedResults)))
                    {
                        Log.Debug("CheckResults: Incorrect result for style " + testStyle + " expression : " + expressionText);
                        Log.Debug("CheckResults: Expected size=" + expectedResults.Count + " received size=" + (receivedResults == null ? 0 : receivedResults.Length));
    
                        Log.Debug("CheckResults: Expected, have " + expectedResults.Count + " entries");
                        PrintList(expectedResults);
                        Log.Debug("CheckResults: Received, have " + (receivedResults == null ? 0 : receivedResults.Length) + " entries");
                        PrintList(receivedResults);
    
                        Assert.Fail();
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail("For statement '" + expressionText + "' failed to assert: " + ex.Message);
                }
            }
        }
    
        private bool CompareLists(EventBean[] receivedResults,
                                     LinkedList<EventDescriptor> expectedResults)
        {
            int receivedSize = (receivedResults == null) ? 0 : receivedResults.Length;
            if (expectedResults.Count != receivedSize)
            {
                return false;
            }
    
            // To make sure all received events have been expected
            var expectedResultsClone = new LinkedList<EventDescriptor>(expectedResults);
    
            // Go through the list of expected results and remove from received result list if found
            foreach (EventDescriptor desc in expectedResults)
            {
                EventDescriptor foundMatch = null;
    
                foreach (EventBean received in receivedResults)
                {
                    if (CompareEvents(desc, received))
                    {
                        foundMatch = desc;
                        break;
                    }
                }
    
                // No match between expected and received
                if (foundMatch == null)
                {
                    return false;
                }
    
                expectedResultsClone.Remove(foundMatch);
            }
    
            // Any left over received results also invalidate the test
            if (expectedResultsClone.Count > 0)
            {
                return false;
            }
            return true;
        }
    
        private static bool CompareEvents(EventDescriptor eventDesc, EventBean eventBean)
        {
            foreach (var entry in eventDesc.EventProperties)
            {
                var left = eventBean.Get(entry.Key);
                var right = entry.Value;
                if (left != right)
                {
                    return false;
                }
            }
            return true;
        }
    
        /// <summary>Clear the event list of all listeners </summary>
        private void ClearListenerEvents()
        {
            foreach (SupportUpdateListener listener in _listeners)
            {
                listener.Reset();
            }
        }
    
        /// <summary>Clear the event list of all listeners </summary>
        private int CountListenerEvents()
        {
            return _listeners.SelectMany(listener => listener.NewDataList).Sum(events => events.Length);
        }

        private void PrintList(IEnumerable<EventDescriptor> events)
        {
            int index = 0;
            foreach (EventDescriptor desc in events)
            {
                StringBuilder buffer = new StringBuilder();
                int count = 0;
    
                foreach (var entry in desc.EventProperties)
                {
                    buffer.Append(" (" + (count++) + ") ");
                    buffer.Append("tag=" + entry.Key);
    
                    String id = FindValue(entry.Value);
                    buffer.Append("  eventId=" + id);
                }
    
                Log.Debug("PrintList: (" + index + ") : " + buffer.ToString());
                index++;
            }
        }
    
        private void PrintList(EventBean[] events)
        {
            if (events == null)
            {
                Log.Debug("PrintList: null-value events array");
                return;
            }
    
            Log.Debug("PrintList: " + events.Length + " elements...");
            for (int i = 0; i < events.Length; i++)
            {
                Log.Debug("  " + EventBeanUtility.PrintEvent(events[i]));
            }
        }
    
        /// <summary>Find the value object in the map of object names and values </summary>
        private String FindValue(Object value)
        {
            foreach (var entry in _sendEventCollection)
            {
                if (value == entry.Value)
                {
                    return entry.Key;
                }
            }
            return null;
        }
    
        private int CountExpectedEvents(String eventId)
        {
            int result = 0;
            foreach (EventExpressionCase descriptor in _caseList.Results)
            {
                LinkedHashMap<String, LinkedList<EventDescriptor>> allExpectedResults = descriptor.ExpectedResults;
    
                // If nothing at all was expected for this event, make sure nothing was received
                if (allExpectedResults.ContainsKey(eventId))
                {
                    result++;
                }
            }
            return result;
        }
    
        private enum PatternTestStyle
        {
            USE_PATTERN_LANGUAGE,
            USE_EPL,
            COMPILE_TO_MODEL,
            COMPILE_TO_EPL,
            USE_EPL_AND_CONSUME_NOCHECK,
        }
    
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
