///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.update;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    ///     Interface for a service that routes events within the runtimefor further processing.
    /// </summary>
    public class InternalEventRouterPreprocessor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IComparer<InternalEventRouterEntry> COMPARATOR =
            new ProxyComparer<InternalEventRouterEntry> {
                ProcCompare = (
                    o1,
                    o2) => {
                    if (o1.Priority > o2.Priority) {
                        return 1;
                    }

                    if (o1.Priority < o2.Priority) {
                        return -1;
                    }

                    if (o1.IsDrop) {
                        return -1;
                    }

                    if (o2.IsDrop) {
                        return -1;
                    }

                    return 0;
                }
            };

        private readonly EventBeanCopyMethod copyMethod;
        private readonly bool empty;
        private readonly InternalEventRouterEntry[] entries;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="copyMethod">for copying the events to preprocess</param>
        /// <param name="entries">descriptors for pre-processing to apply</param>
        public InternalEventRouterPreprocessor(
            EventBeanCopyMethod copyMethod,
            IList<InternalEventRouterEntry> entries)
        {
            this.copyMethod = copyMethod;
            entries.SortInPlace(COMPARATOR);
            this.entries = entries.ToArray();
            empty = this.entries.Length == 0;
        }

        /// <summary>
        ///     Pre-proces the event.
        /// </summary>
        /// <param name="theEvent">to pre-process</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <param name="instrumentation">instrumentation</param>
        /// <returns>processed event</returns>
        public EventBean Process(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext,
            InstrumentationCommon instrumentation)
        {
            if (empty) {
                return theEvent;
            }

            instrumentation.QUpdateIStream(entries);

            var oldEvent = theEvent;
            var haveCloned = false;
            var eventsPerStream = new EventBean[1];
            eventsPerStream[0] = theEvent;
            InternalEventRouterEntry lastEntry = null;

            for (var i = 0; i < entries.Length; i++) {
                var entry = entries[i];
                instrumentation.QUpdateIStreamApply(i, entry);

                var whereClause = entry.OptionalWhereClause;
                if (whereClause != null) {
                    instrumentation.QUpdateIStreamApplyWhere();
                    var result = whereClause.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                    if (result == null || false.Equals(result)) {
                        instrumentation.AUpdateIStreamApplyWhere(false);
                        instrumentation.AUpdateIStreamApply(null, false);
                        continue;
                    }

                    instrumentation.AUpdateIStreamApplyWhere(true);
                }

                if (entry.IsDrop) {
                    instrumentation.AUpdateIStreamApply(null, false);
                    return null;
                }

                // before applying the changes, indicate to last-entries output view
                if (lastEntry != null) {
                    var view = lastEntry.OutputView;
                    if (view.IsIndicate) {
                        var copied = copyMethod.Copy(theEvent);
                        view.Indicate(copied, oldEvent);
                        oldEvent = copied;
                    }
                    else {
                        if (entries[i].OutputView.IsIndicate) {
                            oldEvent = copyMethod.Copy(theEvent);
                        }
                    }
                }

                // copy event for the first update that applies
                if (!haveCloned) {
                    var copiedEvent = copyMethod.Copy(theEvent);
                    if (copiedEvent == null) {
                        Log.Warn("Event of type " + theEvent.EventType.Name + " could not be copied");
                        instrumentation.AUpdateIStreamApply(null, false);
                        return null;
                    }

                    haveCloned = true;
                    eventsPerStream[0] = copiedEvent;
                    theEvent = copiedEvent;
                }

                Apply(theEvent, eventsPerStream, entry, exprEvaluatorContext, instrumentation);
                lastEntry = entry;

                instrumentation.AUpdateIStreamApply(theEvent, true);
            }

            if (lastEntry != null) {
                var view = lastEntry.OutputView;
                if (view.IsIndicate) {
                    view.Indicate(theEvent, oldEvent);
                }
            }

            instrumentation.AUpdateIStream(theEvent, haveCloned);
            return theEvent;
        }

        private void Apply(
            EventBean theEvent,
            EventBean[] eventsPerStream,
            InternalEventRouterEntry entry,
            ExprEvaluatorContext exprEvaluatorContext,
            InstrumentationCommon instrumentation)
        {
            // evaluate
            object[] values;
            if (entry.IsSubselect) {
                StatementResourceHolder holder = entry.StatementContext.StatementCPCacheService.MakeOrGetEntryCanNull(
                    StatementCPCacheService.DEFAULT_AGENT_INSTANCE_ID, entry.StatementContext);
                using (holder.AgentInstanceContext.AgentInstanceLock.AcquireWriteLock())
                {
                    values = ObtainValues(eventsPerStream, entry, exprEvaluatorContext, instrumentation);
                }
            }
            else {
                values = ObtainValues(eventsPerStream, entry, exprEvaluatorContext, instrumentation);
            }

            // apply
            entry.Writer.Write(values, theEvent);
            
            if (entry.SpecialPropWriters.Length > 0) {
                foreach (var special in entry.SpecialPropWriters) {
                    if (special is InternalEventRouterWriterArrayElement array) {
                        var value = array.RhsExpression.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                        if ((value != null) && (array.TypeWidener != null)) {
                            value = array.TypeWidener.Widen(value);
                        }
                        var arrayValue = theEvent.Get(array.PropertyName);
                        if (arrayValue is Array asArrayValue) {
                            var index = array.IndexExpression.Evaluate(eventsPerStream, true, exprEvaluatorContext).AsBoxedInt32();
                            if (index != null) {
                                int len = asArrayValue.Length;
                                if (index < len) {
                                    if (value != null || !asArrayValue.GetType().GetElementType().IsPrimitive) {
                                        asArrayValue.SetValue(value, index.Value);
                                    }
                                } else {
                                    throw new EPException("Array length " + len + " less than index " + index + " for property '" + array.PropertyName + "'");
                                }
                            }
                        }
                    } else if (special is InternalEventRouterWriterCurly) {
                        InternalEventRouterWriterCurly curly = (InternalEventRouterWriterCurly) special;
                        curly.Expression.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                    } else {
                        throw new IllegalStateException("Unrecognized writer " + special);
                    }
                }
            }
        }

        private object[] ObtainValues(
            EventBean[] eventsPerStream,
            InternalEventRouterEntry entry,
            ExprEvaluatorContext exprEvaluatorContext,
            InstrumentationCommon instrumentation)
        {
            instrumentation.QUpdateIStreamApplyAssignments(entry);
            var values = new object[entry.Assignments.Length];
            for (var i = 0; i < entry.Assignments.Length; i++) {
                instrumentation.QUpdateIStreamApplyAssignmentItem(i);
                var value = entry.Assignments[i].Evaluate(eventsPerStream, true, exprEvaluatorContext);
                if (value != null && entry.Wideners[i] != null) {
                    value = entry.Wideners[i].Widen(value);
                }

                values[i] = value;
                instrumentation.AUpdateIStreamApplyAssignmentItem(value);
            }

            instrumentation.AUpdateIStreamApplyAssignments(values);
            return values;
        }
    }
} // end of namespace