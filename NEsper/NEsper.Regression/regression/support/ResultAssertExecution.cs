///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.regression.support
{
    public class ResultAssertExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly SortedDictionary<long, TimeAction> Input = ResultAssertInput.GetActions();

        private readonly EPServiceProvider _engine;
        private EPStatement _stmt;
        private readonly SupportUpdateListener _listener;
        private readonly ResultAssertTestResult _expected;
        private ResultAssertExecutionTestSelector _irTestSelector;

        public ResultAssertExecution(
            EPServiceProvider engine,
            EPStatement stmt,
            SupportUpdateListener listener,
            ResultAssertTestResult expected,
            ResultAssertExecutionTestSelector irTestSelector)
        {
            _engine = engine;
            _stmt = stmt;
            _listener = listener;
            _expected = expected;
            _irTestSelector = irTestSelector;
        }

        public ResultAssertExecution(
            EPServiceProvider engine,
            EPStatement stmt,
            SupportUpdateListener listener,
            ResultAssertTestResult expected)
            : this(engine, stmt, listener, expected, ResultAssertExecutionTestSelector.TEST_BOTH_ISTREAM_AND_IRSTREAM)
        {
        }
    
        public void Execute(bool allowAnyOrder)
        {
            bool isAssert = Environment.GetEnvironmentVariable("ASSERTION_DISABLED") == null;

            bool expectRemoveStream = _stmt.Text.ToLower().Contains("select irstream");
            Execute(isAssert, !expectRemoveStream, allowAnyOrder);
            _stmt.Stop();

            // Second execution is for IRSTREAM, asserting both the insert and remove stream
            if (_irTestSelector != ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED)
            {
                String epl = _stmt.Text;
                String irStreamEPL = epl.Replace("select ", "select irstream ");
                _stmt = _engine.EPAdministrator.CreateEPL(irStreamEPL);
                _stmt.Events += _listener.Update;
                Execute(isAssert, false, allowAnyOrder);
                _stmt.Stop();
            }
        }

        private void Execute(bool isAssert, bool isExpectNullRemoveStream, bool allowAnyOrder)
        {
            // For use in join tests, send join-to events
            _engine.EPRuntime.SendEvent(new SupportBean("IBM", 0));
            _engine.EPRuntime.SendEvent(new SupportBean("MSFT", 0));
            _engine.EPRuntime.SendEvent(new SupportBean("YAH", 0));
    
            if (Log.IsDebugEnabled)
            {
                Log.Debug(String.Format("Category: {0}   Output rate limiting: {1}", _expected.Category, _expected.Title));
                Log.Debug("");
                Log.Debug("Statement:");
                Log.Debug(_stmt.Text);
                Log.Debug("");
                Log.Debug(String.Format("{0,-28}  {1,-38}", "Input", "Output"));
                Log.Debug(String.Format("{0,-45}  {1,-15}  {2,-15}", "", "Insert Stream", "Remove Stream"));
                Log.Debug(String.Format("{0,-28}  {1,-30}", "-----------------------------------------------", "----------------------------------"));
                Log.Debug(String.Format("{0,-5} {1,-5}{2,-8}{3,-8}", "TimeInMillis", "Symbol", "Volume", "Price"));
            }

            foreach (KeyValuePair<long, TimeAction> timeEntry in Input)
            {
                long time = timeEntry.Key;
                String timeInSec = String.Format("{0,3:F1}", time / 1000.0);

                Log.Info(".execute At " + timeInSec + " sending timer event");
                SendTimer(time);

                if (Log.IsDebugEnabled)
                {
                    String comment = timeEntry.Value.ActionDesc;
                    comment = comment ?? "";
                    Log.Debug(String.Format("{0,-5} {1,-24} {2}", timeInSec, "", comment));
                }

                ProcessAction(time, timeInSec, timeEntry.Value, isAssert, isExpectNullRemoveStream, allowAnyOrder);
            }
        }
    
        private void ProcessAction(long currentTime, String timeInSec, TimeAction value, bool isAssert, bool isExpectNullRemoveStream, bool allowAnyOrder)
        {
            IDictionary<int, StepDesc> assertions = _expected.Assertions.Get(currentTime);
    
            // Assert step 0 which is the timer event result then send events and assert remaining
            AssertStep(timeInSec, 0, assertions, _expected.Properties, isAssert, isExpectNullRemoveStream, allowAnyOrder);
    
            for (int step = 1; step < 10; step++)
            {
                if (value.Events.Count >= step)
                {
                    EventSendDesc sendEvent = value.Events[step - 1];
                    Log.Info(".execute At " + timeInSec + " sending event: " + sendEvent.Event + " " + sendEvent.EventDesc);
                    _engine.EPRuntime.SendEvent(sendEvent.Event);
    
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug(
                            "{0,5}  {1,5}{2,8} {3,7:F1}   {4}",
                            "",
                            sendEvent.Event.Symbol, 
                            sendEvent.Event.Volume, 
                            sendEvent.Event.Price, 
                            sendEvent.EventDesc);
                    }
                }

                AssertStep(timeInSec, step, assertions, _expected.Properties, isAssert, isExpectNullRemoveStream, allowAnyOrder);
            }
        }
    
        private void AssertStep(String timeInSec, int step, IDictionary<int, StepDesc> stepAssertions, String[] fields, bool isAssert, bool isExpectNullRemoveStream, bool allowAnyOrder)
        {
            if (Log.IsDebugEnabled)
            {
                if (_listener.IsInvoked)
                {
                    UniformPair<String[]> received = RenderReceived(fields);
                    String[] newRows = received.First;
                    String[] oldRows = received.Second;
                    int numMaxRows = (newRows.Length > oldRows.Length) ? newRows.Length : oldRows.Length;
                    for (int i = 0; i < numMaxRows; i++)
                    {
                        String newRow = (newRows.Length > i) ? newRows[i] : "";
                        String oldRow = (oldRows.Length > i) ? oldRows[i] : "";
                        Log.Debug(
                            "{0,48} {1,-18} {2,-20}", "", newRow, oldRow);
                    }
                    if (numMaxRows == 0)
                    {
                        Log.Debug(
                            "{0,48} {1,-18} {2,-20}", "", "(empty result)", "(empty result)");
                    }
                }
            }
    
            if (!isAssert)
            {
                _listener.Reset();
                return;
            }
    
            StepDesc stepDesc = null;
            if (stepAssertions != null)
            {
                stepDesc = stepAssertions.Get(step);
            }
            
            // If there is no assertion, there should be no event received
            if (stepDesc == null)
            {
                Assert.IsFalse(_listener.IsInvoked, "At time " + timeInSec + " expected no events but received one or more");
            }
            else
            {
                // If we do expect remove stream events, asset both
                if (!isExpectNullRemoveStream)
                {
                    String message = "At time " + timeInSec;
                    Assert.IsTrue(_listener.IsInvoked, message + " expected events but received none");


                    if (allowAnyOrder)
                    {
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.LastNewData, _expected.Properties, stepDesc.NewDataPerRow);
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.LastOldData, _expected.Properties, stepDesc.OldDataPerRow);
                    }
                    else
                    {
                        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, _expected.Properties, stepDesc.NewDataPerRow, "newData");
                        EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, _expected.Properties, stepDesc.OldDataPerRow, "oldData");
                    }
                }
                // If we don't expect remove stream events (istream only), then asset new data only if there
                else
                {
                    // If we do expect new data, assert
                    if (stepDesc.NewDataPerRow != null)
                    {
                        Assert.IsTrue(_listener.IsInvoked, "At time " + timeInSec + " expected events but received none");
                        if (allowAnyOrder)
                        {
                            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.LastNewData, _expected.Properties, stepDesc.NewDataPerRow);
                        }
                        else
                        {
                            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, _expected.Properties, stepDesc.NewDataPerRow, "newData");
                        }
                    }
                    else
                    {
                        // If we don't expect new data, make sure its null
                        Assert.IsNull(_listener.LastNewData, "At time " + timeInSec + " expected no insert stream events but received some");
                    }

                    Assert.IsNull(_listener.LastOldData, "At time " + timeInSec + " expected no remove stream events but received Some(check irstream/istream(default) test case)");
                }
            }
            _listener.Reset();
        }
    
        private UniformPair<String[]> RenderReceived(String[] fields) {
    
            String[] renderNew = RenderReceived(_listener.GetNewDataListFlattened(), fields);
            String[] renderOld = RenderReceived(_listener.GetOldDataListFlattened(), fields);
            return new UniformPair<String[]>(renderNew, renderOld);
        }
    
        private String[] RenderReceived(EventBean[] newDataListFlattened, String[] fields) {
            if (newDataListFlattened == null)
            {
                return new String[0];
            }
            String[] result = new String[newDataListFlattened.Length];
            for (int i = 0; i < newDataListFlattened.Length; i++)
            {
                object[] values = new Object[fields.Length];
                EventBean theEvent = newDataListFlattened[i];
                for (int j = 0; j < fields.Length; j++)
                {
                    values[j] = theEvent.Get(fields[j]);
                }
                result[i] = values.Render();
            }
            return result;
        }
    
        private void SendTimer(long timeInMSec)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _engine.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
}
