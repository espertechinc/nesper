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
        public bool HasPreprocessing {
            get => hasPreprocessing;
        }

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
                    bool route = insertIntoListener.Inserted(theEvent, statementHandle);
                    if (route) {
                        routeDest.Route(theEvent, statementHandle, addToFront);
                    }
                }
                else {
                    routeDest.Route(theEvent, statementHandle, addToFront);
                }

                return;
            }

            EventBean preprocessed = GetPreprocessedEvent(
                theEvent,
                exprEvaluatorContext,
                exprEvaluatorContext.InstrumentationProvider);
            if (preprocessed != null) {
                if (insertIntoListener != null) {
                    bool route = insertIntoListener.Inserted(theEvent, statementHandle);
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
            IReaderWriterLock agentInstanceLock,
            bool hasSubselect)
        {
            lock (this) {
                descriptors.Put(
                    internalEventRouterDesc,
                    new IRDescEntry(
                        internalEventRouterDesc,
                        outputView,
                        agentInstanceLock,
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
            NullableObject<InternalEventRouterPreprocessor> processor = preprocessors.Get(theEvent.EventType);
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
            foreach (EventType type in preprocessors.Keys) {
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
            EventTypeSPI eventTypeSPI = (EventTypeSPI) eventType;
            IList<InternalEventRouterEntry> desc = new List<InternalEventRouterEntry>();

            // determine which ones to process for this types, and what priority and drop
            ISet<string> eventPropertiesWritten = new HashSet<string>();
            foreach (KeyValuePair<InternalEventRouterDesc, IRDescEntry> entry in descriptors) {
                bool applicable = Equals(entry.Value.EventType, eventType);
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

                int priority = 0;
                bool isDrop = false;
                Attribute[] annotations = entry.Value.Annotations;
                for (int i = 0; i < annotations.Length; i++) {
                    if (annotations[i] is PriorityAttribute) {
                        priority = ((PriorityAttribute) annotations[i]).Value;
                    }

                    if (annotations[i] is DropAttribute) {
                        isDrop = true;
                    }
                }

                eventPropertiesWritten.AddAll(entry.Key.Properties);
                EventBeanWriter writer = eventTypeSPI.GetWriter(entry.Key.Properties);
                desc.Add(
                    new InternalEventRouterEntry(
                        priority,
                        isDrop,
                        entry.Value.OptionalWhereClauseEvaluator,
                        entry.Key.Assignments,
                        writer,
                        entry.Value.Wideners,
                        entry.Value.OutputView,
                        entry.Value.AgentInstanceLock,
                        entry.Value.HasSubselect));
            }

            EventBeanCopyMethodForge copyMethodForge =
                eventTypeSPI.GetCopyMethodForge(eventPropertiesWritten.ToArray());
            if (copyMethodForge == null) {
                return new NullableObject<InternalEventRouterPreprocessor>(null);
            }

            return new NullableObject<InternalEventRouterPreprocessor>(
                new InternalEventRouterPreprocessor(copyMethodForge.GetCopyMethod(eventBeanTypedEventFactory), desc));
        }

        private class IRDescEntry
        {
            private readonly InternalEventRouterDesc internalEventRouterDesc;
            private readonly InternalRoutePreprocessView outputView;
            private readonly IReaderWriterLock agentInstanceLock;
            private readonly bool hasSubselect;
            private readonly ExprEvaluator optionalWhereClauseEvaluator;

            internal IRDescEntry(
                InternalEventRouterDesc internalEventRouterDesc,
                InternalRoutePreprocessView outputView,
                IReaderWriterLock agentInstanceLock,
                bool hasSubselect,
                ExprEvaluator optionalWhereClauseEvaluator)
            {
                this.internalEventRouterDesc = internalEventRouterDesc;
                this.outputView = outputView;
                this.agentInstanceLock = agentInstanceLock;
                this.hasSubselect = hasSubselect;
                this.optionalWhereClauseEvaluator = optionalWhereClauseEvaluator;
            }

            public ExprEvaluator OptionalWhereClauseEvaluator {
                get => optionalWhereClauseEvaluator;
            }

            public EventType EventType {
                get => internalEventRouterDesc.EventType;
            }

            public Attribute[] Annotations {
                get => internalEventRouterDesc.Annotations;
            }

            public TypeWidener[] Wideners {
                get => internalEventRouterDesc.Wideners;
            }

            public InternalRoutePreprocessView OutputView {
                get => outputView;
            }

            public IReaderWriterLock AgentInstanceLock {
                get => agentInstanceLock;
            }

            public bool HasSubselect {
                get => hasSubselect;
            }
        }
    }
} // end of namespace