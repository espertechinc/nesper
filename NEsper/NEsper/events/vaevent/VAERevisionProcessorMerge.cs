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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.view;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Provides a set of merge-strategies for merging individual properties (rather then overlaying groups).
    /// </summary>
    public class VAERevisionProcessorMerge
        : VAERevisionProcessorBase
        , ValueAddEventProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly RevisionTypeDesc _infoFullType;
        private readonly IDictionary<Object, RevisionStateMerge> _statePerKey;
        private readonly UpdateStrategy _updateStrategy;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="revisioneventTypeName">name</param>
        /// <param name="spec">specification</param>
        /// <param name="statementStopService">for stop handling</param>
        /// <param name="eventAdapterService">for nested property handling</param>
        /// <param name="eventTypeIdGenerator">The event type id generator.</param>
        public VAERevisionProcessorMerge(String revisioneventTypeName, RevisionSpec spec, StatementStopService statementStopService, EventAdapterService eventAdapterService, EventTypeIdGenerator eventTypeIdGenerator)
            : base(spec, revisioneventTypeName, eventAdapterService)
        {
            // on statement stop, remove versions
            statementStopService.StatementStopped += () => _statePerKey.Clear();
            _statePerKey = new Dictionary<Object, RevisionStateMerge>();

            // For all changeset properties, add type descriptors (property number, getter etc)
            var propertyDesc = new Dictionary<String, RevisionPropertyTypeDesc>();
            var count = 0;

            foreach (String property in spec.ChangesetPropertyNames)
            {
                var fullGetter = spec.BaseEventType.GetGetter(property);
                var propertyNumber = count;
                var paramList = new RevisionGetterParameters(property, propertyNumber, fullGetter, null);

                // if there are no groups (full event property only), then simply use the full event getter
                EventPropertyGetterSPI revisionGetter = new VAERevisionEventPropertyGetterMerge(paramList);

                var type = spec.BaseEventType.GetPropertyType(property);
                if (type == null)
                {
                    foreach (EventType deltaType in spec.DeltaTypes)
                    {
                        var dtype = deltaType.GetPropertyType(property);
                        if (dtype != null)
                        {
                            type = dtype;
                            break;
                        }
                    }
                }

                var propertyTypeDesc = new RevisionPropertyTypeDesc(revisionGetter, paramList, type);
                propertyDesc.Put(property, propertyTypeDesc);
                count++;
            }

            count = 0;
            foreach (String property in spec.KeyPropertyNames)
            {
                var keyPropertyNumber = count;
                EventPropertyGetterSPI revisionGetter;
                if (spec.KeyPropertyNames.Length == 1)
                {
                    revisionGetter = new VAERevisionEventPropertyGetterMergeOneKey();
                }
                else
                {
                    revisionGetter = new VAERevisionEventPropertyGetterMergeNKey(keyPropertyNumber);
                }

                var type = spec.BaseEventType.GetPropertyType(property);
                if (type == null)
                {
                    foreach (EventType deltaType in spec.DeltaTypes)
                    {
                        var dtype = deltaType.GetPropertyType(property);
                        if (dtype != null)
                        {
                            type = dtype;
                            break;
                        }
                    }
                }
                var propertyTypeDesc = new RevisionPropertyTypeDesc(revisionGetter, null, type);
                propertyDesc.Put(property, propertyTypeDesc);
                count++;
            }

            // compile for each event type a list of getters and indexes within the overlay
            foreach (EventType deltaType in spec.DeltaTypes)
            {
                RevisionTypeDesc typeDesc = MakeTypeDesc(deltaType, spec.PropertyRevision);
                TypeDescriptors.Put(deltaType, typeDesc);
            }
            _infoFullType = MakeTypeDesc(spec.BaseEventType, spec.PropertyRevision);

            // how to handle updates to a full event
            if (spec.PropertyRevision == PropertyRevisionEnum.MERGE_DECLARED)
            {
                _updateStrategy = new UpdateStrategyDeclared(spec);
            }
            else if (spec.PropertyRevision == PropertyRevisionEnum.MERGE_NON_NULL)
            {
                _updateStrategy = new UpdateStrategyNonNull(spec);
            }
            else if (spec.PropertyRevision == PropertyRevisionEnum.MERGE_EXISTS)
            {
                _updateStrategy = new UpdateStrategyExists(spec);
            }
            else
            {
                throw new ArgumentException("Unknown revision type '" + spec.PropertyRevision + "'");
            }

            EventTypeMetadata metadata = EventTypeMetadata.CreateValueAdd(revisioneventTypeName, TypeClass.REVISION);
            RevisionEventType = new RevisionEventType(metadata, eventTypeIdGenerator.GetTypeId(revisioneventTypeName), propertyDesc, eventAdapterService);
        }

        public override EventBean GetValueAddEventBean(EventBean theEvent)
        {
            return new RevisionEventBeanMerge(RevisionEventType, theEvent);
        }

        public override void OnUpdate(EventBean[] newData, EventBean[] oldData, NamedWindowRootViewInstance namedWindowRootView, EventTableIndexRepository indexRepository)
        {
            // If new data is filled, it is not a delete
            RevisionEventBeanMerge revisionEvent;
            Object key;
            if ((newData == null) || (newData.Length == 0))
            {
                // we are removing an event
                revisionEvent = (RevisionEventBeanMerge)oldData[0];
                key = revisionEvent.Key;
                _statePerKey.Remove(key);

                // Insert into indexes for fast deletion, if there are any
                foreach (EventTable table in indexRepository.Tables)
                {
                    table.Remove(oldData, namedWindowRootView.AgentInstanceContext);
                }

                // make as not the latest event since its due for removal
                revisionEvent.IsLatest = false;

                namedWindowRootView.UpdateChildren(null, oldData);
                return;
            }

            revisionEvent = (RevisionEventBeanMerge)newData[0];
            EventBean underlyingEvent = revisionEvent.UnderlyingFullOrDelta;
            EventType underyingEventType = underlyingEvent.EventType;

            // obtain key values
            key = null;
            RevisionTypeDesc typesDesc;
            Boolean isBaseEventType = false;
            if (underyingEventType == RevisionSpec.BaseEventType)
            {
                typesDesc = _infoFullType;
                key = PropertyUtility.GetKeys(underlyingEvent, _infoFullType.KeyPropertyGetters);
                isBaseEventType = true;
            }
            else
            {
                typesDesc = TypeDescriptors.Get(underyingEventType);

                // if this type cannot be found, check all supertypes, if any
                if (typesDesc == null)
                {
                    EventType[] superTypes = underyingEventType.DeepSuperTypes;
                    if (superTypes != null)
                    {
                        foreach (var superType in superTypes)
                        {
                            if (superType == RevisionSpec.BaseEventType)
                            {
                                typesDesc = _infoFullType;
                                key = PropertyUtility.GetKeys(underlyingEvent, _infoFullType.KeyPropertyGetters);
                                isBaseEventType = true;
                                break;
                            }
                            typesDesc = TypeDescriptors.Get(superType);
                            if (typesDesc != null)
                            {
                                TypeDescriptors.Put(underyingEventType, typesDesc);
                                key = PropertyUtility.GetKeys(underlyingEvent, typesDesc.KeyPropertyGetters);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    key = PropertyUtility.GetKeys(underlyingEvent, typesDesc.KeyPropertyGetters);
                }
            }

            // get the state for this key value
            RevisionStateMerge revisionState = _statePerKey.Get(key);

            // Delta event and no full
            if ((!isBaseEventType) && (revisionState == null))
            {
                return; // Ignore the event, its a delta and we don't currently have a full event for it
            }

            // New full event
            if (revisionState == null)
            {
                revisionState = new RevisionStateMerge(underlyingEvent, null, null);
                _statePerKey.Put(key, revisionState);

                // prepare revison event
                revisionEvent.LastBaseEvent = underlyingEvent;
                revisionEvent.Key = key;
                revisionEvent.Overlay = null;
                revisionEvent.IsLatest = true;

                // Insert into indexes for fast deletion, if there are any
                foreach (EventTable table in indexRepository.Tables)
                {
                    table.Add(newData, namedWindowRootView.AgentInstanceContext);
                }

                // post to data window
                revisionState.LastEvent = revisionEvent;
                namedWindowRootView.UpdateChildren(new EventBean[] { revisionEvent }, null);
                return;
            }

            // handle Update, changing revision state and event as required
            _updateStrategy.HandleUpdate(isBaseEventType, revisionState, revisionEvent, typesDesc);

            // prepare revision event
            revisionEvent.LastBaseEvent = revisionState.BaseEventUnderlying;
            revisionEvent.Overlay = revisionState.Overlays;
            revisionEvent.Key = key;
            revisionEvent.IsLatest = true;

            // get prior event
            RevisionEventBeanMerge lastEvent = revisionState.LastEvent;
            lastEvent.IsLatest = false;

            // data to post
            var newDataPost = new EventBean[] { revisionEvent };
            var oldDataPost = new EventBean[] { lastEvent };

            // Update indexes
            foreach (EventTable table in indexRepository.Tables)
            {
                table.Remove(oldDataPost, namedWindowRootView.AgentInstanceContext);
                table.Add(newDataPost, namedWindowRootView.AgentInstanceContext);
            }

            // keep reference to last event
            revisionState.LastEvent = revisionEvent;

            namedWindowRootView.UpdateChildren(newDataPost, oldDataPost);
        }

        public override ICollection<EventBean> GetSnapshot(EPStatementAgentInstanceHandle createWindowStmtHandle, Viewable parent)
        {
            using (createWindowStmtHandle.StatementAgentInstanceLock.AcquireReadLock())
            {
                IEnumerator<EventBean> it = parent.GetEnumerator();
                if (!it.MoveNext())
                {
                    return new List<EventBean>();
                }

                var list = new LinkedList<EventBean>();
                do
                {
                    var fullRevision = (RevisionEventBeanMerge)it.Current;
                    var key = fullRevision.Key;
                    var state = _statePerKey.Get(key);
                    list.AddLast(state.LastEvent);
                } while (it.MoveNext());
                return list;
            }
        }

        public override void RemoveOldData(EventBean[] oldData, EventTableIndexRepository indexRepository, AgentInstanceContext agentInstanceContext)
        {
            foreach (EventBean anOldData in oldData)
            {
                var theEvent = (RevisionEventBeanMerge)anOldData;

                // If the remove event is the latest event, remove from all caches
                if (theEvent.IsLatest)
                {
                    var key = theEvent.Key;
                    _statePerKey.Remove(key);

                    foreach (EventTable table in indexRepository.Tables)
                    {
                        table.Remove(oldData, agentInstanceContext);
                    }
                }
            }
        }

        private RevisionTypeDesc MakeTypeDesc(EventType eventType, PropertyRevisionEnum propertyRevision)
        {
            EventPropertyGetter[] keyPropertyGetters = PropertyUtility.GetGetters(eventType, RevisionSpec.KeyPropertyNames);

            var len = RevisionSpec.ChangesetPropertyNames.Length;
            var listOfGetters = new List<EventPropertyGetter>();
            var listOfIndexes = new List<int>();

            for (int i = 0; i < len; i++)
            {
                String propertyName = RevisionSpec.ChangesetPropertyNames[i];
                EventPropertyGetter getter = null;

                if (propertyRevision != PropertyRevisionEnum.MERGE_EXISTS)
                {
                    getter = eventType.GetGetter(RevisionSpec.ChangesetPropertyNames[i]);
                }
                else
                {
                    // only declared properties may be used a dynamic properties to avoid confusion of properties suddenly appearing
                    foreach (String propertyNamesDeclared in eventType.PropertyNames)
                    {
                        if (propertyNamesDeclared == propertyName)
                        {
                            // use dynamic properties
                            getter = eventType.GetGetter(RevisionSpec.ChangesetPropertyNames[i] + "?");
                            break;
                        }
                    }
                }

                if (getter != null)
                {
                    listOfGetters.Add(getter);
                    listOfIndexes.Add(i);
                }
            }

            var changesetPropertyGetters = listOfGetters.ToArray();
            var changesetPropertyIndex = new int[listOfIndexes.Count];
            for (int i = 0; i < listOfIndexes.Count; i++)
            {
                changesetPropertyIndex[i] = listOfIndexes[i];
            }

            return new RevisionTypeDesc(keyPropertyGetters, changesetPropertyGetters, changesetPropertyIndex);
        }
    }
}
