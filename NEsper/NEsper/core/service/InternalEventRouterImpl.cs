///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Routing implementation that allows to pre-process events.
    /// </summary>
    public class InternalEventRouterImpl : InternalEventRouter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _engineURI;
        private readonly IDictionary<EventType, NullableObject<InternalEventRouterPreprocessor>> _preprocessors;
        private readonly IDictionary<UpdateDesc, IRDescEntry> _descriptors;
        private bool _hasPreprocessing;
        private InsertIntoListener _insertIntoListener;

        /// <summary>Ctor. </summary>
        public InternalEventRouterImpl(String engineURI)
        {
            _engineURI = engineURI;
            _hasPreprocessing = false;
            _preprocessors = new ConcurrentDictionary<EventType, NullableObject<InternalEventRouterPreprocessor>>();
            _descriptors = new LinkedHashMap<UpdateDesc, IRDescEntry>();
        }

        /// <summary>Return true to indicate that there is pre-processing to take place. </summary>
        /// <value>preprocessing indicator</value>
        public bool HasPreprocessing
        {
            get { return _hasPreprocessing; }
        }

        /// <summary>Pre-process the event. </summary>
        /// <param name="theEvent">to preprocess</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>preprocessed event</returns>
        public EventBean Preprocess(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetPreprocessedEvent(theEvent, exprEvaluatorContext);
        }

        public InsertIntoListener InsertIntoListener
        {
            set { _insertIntoListener = value; }
        }

        public void Route(
            EventBean theEvent,
            EPStatementHandle statementHandle,
            InternalEventRouteDest routeDest,
            ExprEvaluatorContext exprEvaluatorContext,
            bool addToFront)
        {
            if (!_hasPreprocessing)
            {
                if (_insertIntoListener != null)
                {
                    bool route = _insertIntoListener.Inserted(theEvent, statementHandle);
                    if (route)
                    {
                        routeDest.Route(theEvent, statementHandle, addToFront);
                    }
                }
                else
                {
                    routeDest.Route(theEvent, statementHandle, addToFront);
                }
                return;
            }

            EventBean preprocessed = GetPreprocessedEvent(theEvent, exprEvaluatorContext);
            if (preprocessed != null)
            {
                if (_insertIntoListener != null)
                {
                    bool route = _insertIntoListener.Inserted(theEvent, statementHandle);
                    if (route)
                    {
                        routeDest.Route(preprocessed, statementHandle, addToFront);
                    }
                }
                else
                {
                    routeDest.Route(preprocessed, statementHandle, addToFront);
                }
            }
        }

        public InternalEventRouterDesc GetValidatePreprocessing(EventType eventType, UpdateDesc desc, Attribute[] annotations)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Validating route preprocessing for type '" + eventType.Name + "'");
            }

            if (!(eventType is EventTypeSPI))
            {
                throw new ExprValidationException("Update statements require the event type to implement the " + typeof(EventTypeSPI) + " interface");
            }

            var eventTypeSPI = (EventTypeSPI)eventType;
            var wideners = new TypeWidener[desc.Assignments.Count];
            var properties = new List<String>();
            for (int i = 0; i < desc.Assignments.Count; i++)
            {
                var xxx = desc.Assignments[i];
                var assignmentPair = ExprNodeUtility.CheckGetAssignmentToProp(xxx.Expression);
                if (assignmentPair == null)
                {
                    throw new ExprValidationException("Missing property assignment expression in assignment number " + i);
                }

                var writableProperty = eventTypeSPI.GetWritableProperty(assignmentPair.First);
                if (writableProperty == null)
                {
                    throw new ExprValidationException("Property '" + assignmentPair.First + "' is not available for write access");
                }

                wideners[i] = TypeWidenerFactory.GetCheckPropertyAssignType(
                    ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(assignmentPair.Second),
                    assignmentPair.Second.ExprEvaluator.ReturnType,
                    writableProperty.PropertyType, assignmentPair.First,
                    false, null, null, _engineURI);
                properties.Add(assignmentPair.First);
            }

            // check copy-able
            var copyMethod = eventTypeSPI.GetCopyMethod(properties.ToArray());
            if (copyMethod == null)
            {
                throw new ExprValidationException("The update-clause requires the underlying event representation to support copy (via Serializable by default)");
            }

            return new InternalEventRouterDesc(desc, copyMethod, wideners, eventType, annotations);
        }

        public void AddPreprocessing(InternalEventRouterDesc internalEventRouterDesc, InternalRoutePreprocessView outputView, IReaderWriterLock agentInstanceLock, bool hasSubselect)
        {
            lock (this)
            {
                _descriptors.Put(internalEventRouterDesc.UpdateDesc, new IRDescEntry(internalEventRouterDesc, outputView, agentInstanceLock, hasSubselect));

                // remove all preprocessors for this type as well as any known child types, forcing re-init on next use
                RemovePreprocessors(internalEventRouterDesc.EventType);

                _hasPreprocessing = true;
            }
        }

        public void RemovePreprocessing(EventType eventType, UpdateDesc desc)
        {
            lock (this)
            {
                if (Log.IsInfoEnabled)
                {
                    Log.Info("Removing route preprocessing for type '" + eventType.Name);
                }

                // remove all preprocessors for this type as well as any known child types
                RemovePreprocessors(eventType);

                _descriptors.Remove(desc);
                if (_descriptors.IsEmpty())
                {
                    _hasPreprocessing = false;
                    _preprocessors.Clear();
                }
            }
        }

        private EventBean GetPreprocessedEvent(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            NullableObject<InternalEventRouterPreprocessor> processor = _preprocessors.Get(theEvent.EventType);
            if (processor == null)
            {
                lock (this)
                {
                    processor = Initialize(theEvent.EventType);
                    _preprocessors.Put(theEvent.EventType, processor);
                }
            }

            if (processor.Value == null)
            {
                return theEvent;
            }
            else
            {
                return processor.Value.Process(theEvent, exprEvaluatorContext);
            }
        }

        private void RemovePreprocessors(EventType eventType)
        {
            _preprocessors.Remove(eventType);

            // find each child type entry
            foreach (EventType type in _preprocessors.Keys)
            {
                if (type.DeepSuperTypes != null)
                {
                    // TODO: can be minimized
                    foreach (var dtype in type.DeepSuperTypes.Where(dtype => dtype == eventType))
                    {
                        _preprocessors.Remove(type);
                    }
                }
            }
        }

        private NullableObject<InternalEventRouterPreprocessor> Initialize(EventType eventType)
        {
            var eventTypeSPI = (EventTypeSPI)eventType;
            var desc = new List<InternalEventRouterEntry>();

            // determine which ones to process for this types, and what priority and drop
            var eventPropertiesWritten = new HashSet<String>();
            foreach (var entry in _descriptors)
            {
                bool applicable = entry.Value.EventType == eventType;
                if (!applicable)
                {
                    if (eventType.DeepSuperTypes != null)
                    {
                        if (eventType.DeepSuperTypes.Where(dtype => dtype == entry.Value.EventType).Any())
                        {
                            applicable = true;
                        }
                    }
                }

                if (!applicable)
                {
                    continue;
                }

                var priority = 0;
                var isDrop = false;
                var annotations = entry.Value.Annotations;
                for (int i = 0; i < annotations.Length; i++)
                {
                    if (annotations[i] is PriorityAttribute)
                    {
                        priority = ((PriorityAttribute)annotations[i]).Value;
                    }
                    if (annotations[i] is DropAttribute)
                    {
                        isDrop = true;
                    }
                }

                var properties = new List<String>();
                var expressions = new ExprNode[entry.Key.Assignments.Count];
                for (int i = 0; i < entry.Key.Assignments.Count; i++)
                {
                    var assignment = entry.Key.Assignments[i];
                    var assignmentPair = ExprNodeUtility.CheckGetAssignmentToProp(assignment.Expression);
                    expressions[i] = assignmentPair.Second;
                    properties.Add(assignmentPair.First);
                    eventPropertiesWritten.Add(assignmentPair.First);
                }

                var writer = eventTypeSPI.GetWriter(properties.ToArray());
                desc.Add(new InternalEventRouterEntry(priority, isDrop, entry.Key.OptionalWhereClause, expressions, writer, entry.Value.Wideners, entry.Value.OutputView, entry.Value.AgentInstanceLock, entry.Value.HasSubselect));
            }

            var copyMethod = eventTypeSPI.GetCopyMethod(eventPropertiesWritten.ToArray());
            if (copyMethod == null)
            {
                return new NullableObject<InternalEventRouterPreprocessor>(null);
            }
            return new NullableObject<InternalEventRouterPreprocessor>(new InternalEventRouterPreprocessor(copyMethod, desc));
        }

        private class IRDescEntry
        {
            private readonly InternalEventRouterDesc _internalEventRouterDesc;
            private readonly InternalRoutePreprocessView _outputView;
            private readonly IReaderWriterLock _agentInstanceLock;
            private readonly bool _hasSubselect;

            internal IRDescEntry(InternalEventRouterDesc internalEventRouterDesc, InternalRoutePreprocessView outputView, IReaderWriterLock agentInstanceLock, bool hasSubselect)
            {
                _internalEventRouterDesc = internalEventRouterDesc;
                _outputView = outputView;
                _agentInstanceLock = agentInstanceLock;
                _hasSubselect = hasSubselect;
            }

            public EventType EventType
            {
                get { return _internalEventRouterDesc.EventType; }
            }

            public Attribute[] Annotations
            {
                get { return _internalEventRouterDesc.Annotations; }
            }

            public TypeWidener[] Wideners
            {
                get { return _internalEventRouterDesc.Wideners; }
            }

            public InternalRoutePreprocessView OutputView
            {
                get { return _outputView; }
            }

            public IReaderWriterLock AgentInstanceLock
            {
                get { return _agentInstanceLock; }
            }

            public bool HasSubselect
            {
                get { return _hasSubselect; }
            }
        }
    }
}
