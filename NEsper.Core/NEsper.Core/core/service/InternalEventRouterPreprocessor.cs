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

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Interface for a service that routes events within the engine for further
    /// processing.
    /// </summary>
    public class InternalEventRouterPreprocessor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventBeanCopyMethod _copyMethod;
        private readonly bool _empty;
        private readonly InternalEventRouterEntry[] _entries;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="copyMethod">for copying the events to preprocess</param>
        /// <param name="entries">descriptors for pre-processing to apply</param>
        public InternalEventRouterPreprocessor(EventBeanCopyMethod copyMethod, IEnumerable<InternalEventRouterEntry> entries)
        {
            var tempIndex = 0;
            var tempList = entries
                .Select(tempEntry => new Pair<int, InternalEventRouterEntry>(tempIndex++, tempEntry))
                .ToList();

            tempList.Sort(DoCompare);

            _copyMethod = copyMethod;
            _entries = tempList.Select(pair => pair.Second).ToArray();
            _empty = _entries.Length == 0;
        }

        private int DoCompare(Pair<int, InternalEventRouterEntry> op1,
                              Pair<int, InternalEventRouterEntry> op2)
        {
            var o1 = op1.Second;
            var o2 = op2.Second;

            if (o1.Priority > o2.Priority)
            {
                return 1;
            }

            if (o1.Priority < o2.Priority)
            {
                return -1;
            }

            if (o1.IsDrop)
            {
                return -1;
            }

            if (o2.IsDrop)
            {
                return -1;
            }

            return op1.First.CompareTo(op2.First);
        }

        /// <summary>
        /// Pre-proces the event.
        /// </summary>
        /// <param name="theEvent">to pre-process</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>
        /// processed event
        /// </returns>
        public EventBean Process(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (_empty)
            {
                return theEvent;
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QUpdateIStream(_entries); }

            EventBean oldEvent = theEvent;
            bool haveCloned = false;
            var eventsPerStream = new EventBean[1];
            eventsPerStream[0] = theEvent;
            InternalEventRouterEntry lastEntry = null;

            for (int i = 0; i < _entries.Length; i++)
            {
                InternalEventRouterEntry entry = _entries[i];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QUpdateIStreamApply(i, entry); }

                ExprEvaluator whereClause = entry.OptionalWhereClause;
                if (whereClause != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QUpdateIStreamApplyWhere(); }

                    var result = whereClause.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                    if ((result == null) || (false.Equals(result)))
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AUpdateIStreamApplyWhere((bool?)result); }
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AUpdateIStreamApply(null, false); }

                        continue;
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AUpdateIStreamApplyWhere(true); }
                }

                if (entry.IsDrop)
                {
                    return null;
                }

                // before applying the changes, indicate to last-entries output view
                if (lastEntry != null)
                {
                    InternalRoutePreprocessView view = lastEntry.OutputView;
                    if (view.IsIndicate)
                    {
                        EventBean copied = _copyMethod.Copy(theEvent);
                        view.Indicate(copied, oldEvent);
                        oldEvent = copied;
                    }
                    else
                    {
                        if (_entries[i].OutputView.IsIndicate)
                        {
                            oldEvent = _copyMethod.Copy(theEvent);
                        }
                    }
                }

                // copy event for the first Update that applies
                if (!haveCloned)
                {
                    EventBean copiedEvent = _copyMethod.Copy(theEvent);
                    if (copiedEvent == null)
                    {
                        Log.Warn("Event of type " + theEvent.EventType.Name + " could not be copied");
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AUpdateIStreamApply(null, false); }
                        return null;
                    }
                    haveCloned = true;
                    eventsPerStream[0] = copiedEvent;
                    theEvent = copiedEvent;
                }

                Apply(theEvent, eventsPerStream, entry, exprEvaluatorContext);
                lastEntry = entry;
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AUpdateIStreamApply(theEvent, true); }
            }

            if (lastEntry != null)
            {
                InternalRoutePreprocessView view = lastEntry.OutputView;
                if (view.IsIndicate)
                {
                    view.Indicate(theEvent, oldEvent);
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AUpdateIStream(theEvent, haveCloned); }
            return theEvent;
        }

        private void Apply(EventBean theEvent, EventBean[] eventsPerStream, InternalEventRouterEntry entry, ExprEvaluatorContext exprEvaluatorContext)
        {
            // evaluate
            Object[] values;
            if (entry.HasSubselect)
            {
                using (entry.AgentInstanceLock.AcquireWriteLock())
                {
                    values = ObtainValues(eventsPerStream, entry, exprEvaluatorContext);
                }
            }
            else
            {
                values = ObtainValues(eventsPerStream, entry, exprEvaluatorContext);
            }

            // apply
            entry.Writer.Write(values, theEvent);
        }

        private Object[] ObtainValues(EventBean[] eventsPerStream, InternalEventRouterEntry entry, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QUpdateIStreamApplyAssignments(entry); }

            Object[] values = new Object[entry.Assignments.Length];
            for (int i = 0; i < entry.Assignments.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QUpdateIStreamApplyAssignmentItem(i); }
                Object value = entry.Assignments[i].Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                if ((value != null) && (entry.Wideners[i] != null))
                {
                    value = entry.Wideners[i].Invoke(value);
                }
                values[i] = value;
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AUpdateIStreamApplyAssignmentItem(value); }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AUpdateIStreamApplyAssignments(values); }
            return values;
        }
    }
}
