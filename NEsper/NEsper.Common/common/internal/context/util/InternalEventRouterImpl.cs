///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.context.aifactory.update;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Routing implementation that allows to pre-process events.
    /// </summary>
    public class InternalEventRouterImpl : InternalEventRouter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly IDictionary<EventType, NullableObject<InternalEventRouterPreprocessor>> preprocessors;
        private readonly IDictionary<InternalEventRouterDesc, IRDescEntry> descriptors;
        private bool hasPreprocessing = false;
        private InsertIntoListener insertIntoListener;

        public InternalEventRouterImpl(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.preprocessors = new ConcurrentDictionary<EventType, NullableObject<InternalEventRouterPreprocessor>>();
            this.descriptors = new LinkedHashMap<InternalEventRouterDesc, IRDescEntry>();
        }

        /// <summary>
        /// Return true to indicate that there is pre-processing to take place.
        /// </summary>
        /// <returns>preprocessing indicator</returns>
        public bool HasPreprocessing => hasPreprocessing;

        /// <summary>
        /// Pre-process the event.
        /// </summary>
        /// <param name="theEvent">to preprocess</param>
        /// <param name="runtimeFilterAndDispatchTimeContext">expression evaluation context</param>
        /// <param name="instrumentation">instrumentation</param>
        /// <returns>preprocessed event</returns>
        public EventBean Preprocess(
            EventBean theEvent,
            ExprEvaluatorContext runtimeFilterAndDispatchTimeContext,
            InstrumentationCommon instrumentation)
        {
            return GetPreprocessedEvent(theEvent, runtimeFilterAndDispatchTimeContext, instrumentation);
        }

        public InsertIntoListener InsertIntoListener {
            set => this.insertIntoListener = value;
        }

        public void Route(
            EventBean theEvent,
            AgentInstanceContext agentInstanceContext,
            bool addToFront)
        {
            Route(
                theEvent,
                agentInstanceContext.StatementContext.EpStatementHandle,
                agentInstanceContext.InternalEventRouteDest,
                agentInstanceContext,
                addToFront);
        }

        public void Route(
            EventBean theEvent,
            EPStatementHandle statementHandle,
            InternalEventRouteDest routeDest,
            ExprEvaluatorContext exprEvaluatorContext,
            bool addToFront)
        {
            if (!hasPreprocessing) {
                if (insertIntoListener != null) {
                    var route = insertIntoListener.Inserted(theEvent, statementHandle);
                    if (route) {
                        routeDest.Route(theEvent, statementHandle, addToFront);
                    }
                }
                else {
                    routeDest.Route(theEvent, statementHandle, addToFront);
                }

                return;
            }

            var preprocessed = GetPreprocessedEvent(
                theEvent,
                exprEvaluatorContext,
                exprEvaluatorContext.InstrumentationProvider);
            if (preprocessed != null) {
                if (insertIntoListener != null) {
                    var route = insertIntoListener.Inserted(theEvent, statementHandle);
                    if (route) {
                        routeDest.Route(preprocessed, statementHandle, addToFront);
                    }
                }
                else {
                    routeDest.Route(preprocessed, statementHandle, addToFront);
                }
            }
        }

        public void AddPreprocessing(
            InternalEventRouterDesc internalEventRouterDesc,
            InternalRoutePreprocessView outputView,
            StatementContext statementContext,
            bool hasSubselect)
        {
            lock (this) {
                descriptors.Put(
                    internalEventRouterDesc,
                    new IRDescEntry(
                        internalEventRouterDesc,
                        outputView,
                        statementContext,
                        hasSubselect,
                        internalEventRouterDesc.OptionalWhereClauseEval));

                // remove all preprocessors for this type as well as any known child types, forcing re-init on next use
                RemovePreprocessors(internalEventRouterDesc.EventType);

                hasPreprocessing = true;
            }
        }

        public void RemovePreprocessing(
            EventType eventType,
            InternalEventRouterDesc desc)
        {
            lock (this) {
                if (Log.IsInfoEnabled) {
                    Log.Debug("Removing route preprocessing for type '" + eventType.Name);
                }

                // remove all preprocessors for this type as well as any known child types
                RemovePreprocessors(eventType);

                descriptors.Remove(desc);
                if (descriptors.IsEmpty()) {
                    hasPreprocessing = false;
                    preprocessors.Clear();
                }
            }
        }

        private EventBean GetPreprocessedEvent(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext,
            InstrumentationCommon instrumentation)
        {
            var processor = preprocessors.Get(theEvent.EventType);
            if (processor == null) {
                lock (this) {
                    processor = Initialize(theEvent.EventType);
                    preprocessors.Put(theEvent.EventType, processor);
                }
            }

            if (processor.Value == null) {
                return theEvent;
            }
            else {
                return processor.Value.Process(theEvent, exprEvaluatorContext, instrumentation);
            }
        }

        private void RemovePreprocessors(EventType eventType)
        {
            preprocessors.Remove(eventType);

            // find each child type entry
            foreach (var type in preprocessors.Keys) {
                if (type.DeepSuperTypes != null) {
                    foreach (var testType in type.DeepSuperTypes) {
                        if (Equals(testType, eventType)) {
                            preprocessors.Remove(type);
                        }
                    }
                }
            }
        }

        private NullableObject<InternalEventRouterPreprocessor> Initialize(EventType eventType)
        {
            var eventTypeSPI = (EventTypeSPI) eventType;
            var desc = new List<InternalEventRouterEntry>();

            // determine which ones to process for this types, and what priority and drop
            var eventPropertiesWritten = new HashSet<string>();
            foreach (var entry in descriptors) {
                var applicable = Equals(entry.Value.EventType, eventType);
                if (!applicable) {
                    if (eventType.DeepSuperTypes != null) {
                        foreach (var testType in eventType.DeepSuperTypes) {
                            if (Equals(testType, entry.Value.EventType)) {
                                applicable = true;
                                break;
                            }
                        }
                    }
                }

                if (!applicable) {
                    continue;
                }

                var priority = 0;
                var isDrop = false;
                var annotations = entry.Value.Annotations;
                for (var i = 0; i < annotations.Length; i++) {
                    if (annotations[i] is PriorityAttribute) {
                        priority = ((PriorityAttribute) annotations[i]).Value;
                    }

                    if (annotations[i] is DropAttribute) {
                        isDrop = true;
                    }
                }

                eventPropertiesWritten.AddAll(entry.Key.Properties);
                var writer = eventTypeSPI.GetWriter(entry.Key.Properties);
                desc.Add(
                    new InternalEventRouterEntry(
                        priority,
                        isDrop,
                        entry.Value.OptionalWhereClauseEvaluator,
                        entry.Key.Assignments,
                        writer,
                        entry.Value.Wideners,
                        entry.Value.Writers,
                        entry.Value.OutputView,
                        entry.Value.StatementContext,
                        entry.Value.HasSubselect));
            }

            var copyMethodForge =
                eventTypeSPI.GetCopyMethodForge(eventPropertiesWritten.ToArray());
            if (copyMethodForge == null) {
                return new NullableObject<InternalEventRouterPreprocessor>(null);
            }

            return new NullableObject<InternalEventRouterPreprocessor>(
                new InternalEventRouterPreprocessor(copyMethodForge.GetCopyMethod(eventBeanTypedEventFactory), desc));
        }

        public void MovePreprocessing(
            StatementContext statementContext,
            InternalEventRouter internalEventRouter)
        {
            var moved = descriptors
                .Where(entry => entry.Value.StatementContext == statementContext)
                .ToList();

            foreach (var entry in moved) {
                RemovePreprocessing(entry.Key.EventType, entry.Value.InternalEventRouterDesc);
                internalEventRouter.AddPreprocessing(
                    entry.Value.InternalEventRouterDesc,
                    entry.Value.OutputView,
                    statementContext,
                    entry.Value.HasSubselect);
            }
        }

        private class IRDescEntry
        {
            private readonly InternalEventRouterDesc internalEventRouterDesc;
            private readonly InternalRoutePreprocessView outputView;
            private readonly StatementContext statementContext;
            private readonly bool hasSubselect;
            private readonly ExprEvaluator optionalWhereClauseEvaluator;

            internal IRDescEntry(
                InternalEventRouterDesc internalEventRouterDesc,
                InternalRoutePreprocessView outputView,
                StatementContext statementContext,
                bool hasSubselect,
                ExprEvaluator optionalWhereClauseEvaluator)
            {
                this.internalEventRouterDesc = internalEventRouterDesc;
                this.outputView = outputView;
                this.statementContext = statementContext;
                this.hasSubselect = hasSubselect;
                this.optionalWhereClauseEvaluator = optionalWhereClauseEvaluator;
            }

            public ExprEvaluator OptionalWhereClauseEvaluator => optionalWhereClauseEvaluator;

            public InternalEventRouterDesc InternalEventRouterDesc => internalEventRouterDesc;

            public EventType EventType => internalEventRouterDesc.EventType;

            public Attribute[] Annotations => internalEventRouterDesc.Annotations;

            public TypeWidener[] Wideners => internalEventRouterDesc.Wideners;

            public InternalRoutePreprocessView OutputView => outputView;

            public StatementContext StatementContext => statementContext;

            public bool HasSubselect => hasSubselect;

            public InternalEventRouterWriter[] Writers => internalEventRouterDesc.Writers;
        }
    }
} // end of namespace