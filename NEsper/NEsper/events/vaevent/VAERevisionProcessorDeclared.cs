///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Provides overlay strategy for property group-based versioning.
    /// </summary>
    public class VAERevisionProcessorDeclared 
        : VAERevisionProcessorBase
        , ValueAddEventProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly PropertyGroupDesc[] _groups;
        private readonly EventType _baseEventType;
        private readonly EventPropertyGetter[] _fullKeyGetters;
        private readonly IDictionary<Object, RevisionStateDeclared> _statePerKey;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="revisionEventTypeName">name</param>
        /// <param name="spec">specification</param>
        /// <param name="statementStopService">for stop handling</param>
        /// <param name="eventAdapterService">for nested property handling</param>
        /// <param name="eventTypeIdGenerator">The event type id generator.</param>
        public VAERevisionProcessorDeclared(String revisionEventTypeName, RevisionSpec spec, StatementStopService statementStopService, EventAdapterService eventAdapterService, EventTypeIdGenerator eventTypeIdGenerator)

                    : base(spec, revisionEventTypeName, eventAdapterService)
        {
    
            // on statement stop, remove versions
            statementStopService.StatementStopped += () => _statePerKey.Clear();

            _statePerKey = new Dictionary<Object, RevisionStateDeclared>().WithNullSupport();
            _baseEventType = spec.BaseEventType;
            _fullKeyGetters = PropertyUtility.GetGetters(_baseEventType, spec.KeyPropertyNames);
    
            // sort non-key properties, removing keys
            _groups = PropertyUtility.AnalyzeGroups(spec.ChangesetPropertyNames, spec.DeltaTypes, spec.DeltaNames);
            IDictionary<String, RevisionPropertyTypeDesc> propertyDesc = CreatePropertyDescriptors(spec, _groups);
    
            TypeDescriptors = PropertyUtility.GetPerType(_groups, spec.ChangesetPropertyNames, spec.KeyPropertyNames);
            EventTypeMetadata metadata = EventTypeMetadata.CreateValueAdd(revisionEventTypeName, TypeClass.REVISION);
            RevisionEventType = new RevisionEventType(metadata, eventTypeIdGenerator.GetTypeId(revisionEventTypeName), propertyDesc, eventAdapterService);
        }
    
        public override EventBean GetValueAddEventBean(EventBean theEvent)
        {
            return new RevisionEventBeanDeclared(RevisionEventType, theEvent);
        }

        public override void OnUpdate(EventBean[] newData, EventBean[] oldData, NamedWindowRootViewInstance namedWindowRootView, EventTableIndexRepository indexRepository)
        {
            // If new data is filled, it is not a delete
            RevisionEventBeanDeclared revisionEvent;
            Object key;
            if ((newData == null) || (newData.Length == 0))
            {
                // we are removing an event
                revisionEvent = (RevisionEventBeanDeclared) oldData[0];
                key = revisionEvent.Key;
                _statePerKey.Remove(key);
    
                // Insert into indexes for fast deletion, if there are any
                foreach (EventTable table in indexRepository.Tables)
                {
                    table.Remove(oldData);
                }
    
                // make as not the latest event since its due for removal
                revisionEvent.IsLatest = false;
    
                namedWindowRootView.UpdateChildren(null, oldData);
                return;
            }
    
            revisionEvent = (RevisionEventBeanDeclared) newData[0];
            EventBean underlyingEvent = revisionEvent.UnderlyingFullOrDelta;
            EventType underyingEventType = underlyingEvent.EventType;
    
            // obtain key values
            key = null;
            RevisionTypeDesc typesDesc = null;
            bool isBaseEventType = false;
            if (underyingEventType == _baseEventType)
            {
                key = PropertyUtility.GetKeys(underlyingEvent, _fullKeyGetters);
                isBaseEventType = true;
            }
            else
            {
                typesDesc = TypeDescriptors.Get(underyingEventType);
    
                // if this type cannot be found, check all supertypes, if any
                if (typesDesc == null)
                {
                    IEnumerable<EventType> superTypes = underyingEventType.DeepSuperTypes;
                    if (superTypes != null)
                    {
                        foreach(var superType in superTypes)
                        {
                            if (superType == _baseEventType)
                            {
                                key = PropertyUtility.GetKeys(underlyingEvent, _fullKeyGetters);
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
            RevisionStateDeclared revisionState = _statePerKey.Get(key);
    
            // Delta event and no full
            if ((!isBaseEventType) && (revisionState == null))
            {
                return; // Ignore the event, its a delta and we don't currently have a full event for it
            }
    
            // New full event
            if (revisionState == null)
            {
                revisionState = new RevisionStateDeclared(underlyingEvent, null, null);
                _statePerKey.Put(key, revisionState);
    
                // prepare revison event
                revisionEvent.LastBaseEvent = underlyingEvent;
                revisionEvent.Key = key;
                revisionEvent.Holders = null;
                revisionEvent.IsLatest = true;
    
                // Insert into indexes for fast deletion, if there are any
                foreach (EventTable table in indexRepository.Tables)
                {
                    table.Add(newData);
                }
    
                // post to data window
                revisionState.LastEvent = revisionEvent;
                namedWindowRootView.UpdateChildren(new EventBean[] {revisionEvent}, null);
                return;
            }
    
            // new version
            long versionNumber = revisionState.IncRevisionNumber();
    
            // Previously-seen full event
            if (isBaseEventType)
            {
                revisionState.Holders = null;
                revisionState.BaseEventUnderlying = underlyingEvent;
            }
            // Delta event to existing full event
            else
            {
                var groupNum = typesDesc.Group.GroupNum;
                var holders = revisionState.Holders;
                if (holders == null)    // optimization - the full event sets it to null, deltas all get a new one
                {
                    holders = new RevisionBeanHolder[_groups.Length];
                }
                else
                {
                    holders = ArrayCopy(holders);   // preserve the last revisions
                }
    
                // add the new revision for a property group on top
                holders[groupNum] = new RevisionBeanHolder(versionNumber, underlyingEvent, typesDesc.ChangesetPropertyGetters);
                revisionState.Holders = holders;
            }
    
            // prepare revision event
            revisionEvent.LastBaseEvent = revisionState.BaseEventUnderlying;
            revisionEvent.Holders = revisionState.Holders;
            revisionEvent.Key = key;
            revisionEvent.IsLatest = true;
    
            // get prior event
            RevisionEventBeanDeclared lastEvent = revisionState.LastEvent;
            lastEvent.IsLatest = false;
    
            // data to post
            var newDataPost = new EventBean[]{revisionEvent};
            var oldDataPost = new EventBean[]{lastEvent};
    
            // Update indexes
            foreach (EventTable table in indexRepository.Tables)
            {
                table.Remove(oldDataPost);
                table.Add(newDataPost);
            }
    
            // keep reference to last event
            revisionState.LastEvent = revisionEvent;
    
            namedWindowRootView.UpdateChildren(newDataPost, oldDataPost);
        }
    
        public override ICollection<EventBean> GetSnapshot(EPStatementAgentInstanceHandle createWindowStmtHandle, Viewable parent) {
            using(createWindowStmtHandle.StatementAgentInstanceLock.ReadLock.Acquire())
            {
                var it = parent.GetEnumerator();
                var list = new LinkedList<EventBean>();
                while(it.MoveNext())
                {
                    var fullRevision = (RevisionEventBeanDeclared) it.Current;
                    var key = fullRevision.Key;
                    var state = _statePerKey.Get(key);
                    list.AddLast(state.LastEvent);
                }
                return list;
            }
        }
    
        public override void RemoveOldData(EventBean[] oldData, EventTableIndexRepository indexRepository)
        {
            for (int i = 0; i < oldData.Length; i++)
            {
                var theEvent = (RevisionEventBeanDeclared) oldData[i];
    
                // If the remove event is the latest event, remove from all caches
                if (theEvent.IsLatest)
                {
                    var key = theEvent.Key;
                    _statePerKey.Remove(key);
    
                    foreach (EventTable table in indexRepository.Tables)
                    {
                        table.Remove(oldData);
                    }
                }
            }
        }
    
        private static RevisionBeanHolder[] ArrayCopy(RevisionBeanHolder[] array)
        {
            if (array == null)
            {
                return null;
            }
            var result = new RevisionBeanHolder[array.Length];
            Array.Copy(array, 0, result, 0, array.Length);
            return result;
        }
    
        /// <summary>Creates property descriptors for revision. </summary>
        /// <param name="spec">specifies revision</param>
        /// <param name="groups">the groups that group properties</param>
        /// <returns>map of property and descriptor</returns>
        public static IDictionary<String, RevisionPropertyTypeDesc> CreatePropertyDescriptors(RevisionSpec spec, PropertyGroupDesc[] groups)
        {
            IDictionary<String, int[]> propsPerGroup = PropertyUtility.GetGroupsPerProperty(groups);
    
            IDictionary<String, RevisionPropertyTypeDesc> propertyDesc = new Dictionary<String, RevisionPropertyTypeDesc>();
            int count = 0;
    
            foreach (String property in spec.ChangesetPropertyNames)
            {
                var fullGetter = spec.BaseEventType.GetGetter(property);
                var propertyNumber = count;
                var propGroupsProperty = propsPerGroup.Get(property);
                var paramList = new RevisionGetterParameters(property, propertyNumber, fullGetter, propGroupsProperty);
    
                // if there are no groups (full event property only), then simply use the full event getter
                EventPropertyGetter revisionGetter = new ProxyEventPropertyGetter(
                    eventBean => ((RevisionEventBeanDeclared) eventBean).GetVersionedValue(paramList),
                    eventBean => null,
                    eventBean => true);
    
                var type = spec.BaseEventType.GetPropertyType(property);
                var propertyTypeDesc = new RevisionPropertyTypeDesc(revisionGetter, paramList, type);
                propertyDesc.Put(property, propertyTypeDesc);
                count++;
            }
    
            foreach (String property in spec.BaseEventOnlyPropertyNames)
            {
                EventPropertyGetter fullGetter = spec.BaseEventType.GetGetter(property);
    
                // if there are no groups (full event property only), then simply use the full event getter
                EventPropertyGetter revisionGetter =  new ProxyEventPropertyGetter(
                    eventBean =>
                    {
                        var riv = (RevisionEventBeanDeclared)eventBean;
                        var bean = riv.LastBaseEvent;
                        return bean == null ? null : fullGetter.Get(bean);
                    },
                    eventBean => null,
                    eventBean => true);
    
                var type = spec.BaseEventType.GetPropertyType(property);
                var propertyTypeDesc = new RevisionPropertyTypeDesc(revisionGetter, null, type);
                propertyDesc.Put(property, propertyTypeDesc);
                count++;
            }
    
            count = 0;
            foreach (String property in spec.KeyPropertyNames)
            {
                int keyPropertyNumber = count;

                EventPropertyGetter revisionGetter;
                if (spec.KeyPropertyNames.Length == 1)
                {
                    revisionGetter = new ProxyEventPropertyGetter
                    {
                        ProcGet = eventBean => ((RevisionEventBeanDeclared) eventBean).Key,
                        ProcIsExistsProperty = eventBean => true,
                        ProcGetFragment = eventBean => null
                    };
                }
                else
                {
                    revisionGetter = new ProxyEventPropertyGetter
                    {
                        ProcGet = eventBean =>
                        {
                            var riv = (RevisionEventBeanDeclared) eventBean;
                            return ((MultiKeyUntyped) riv.Key).Keys[keyPropertyNumber];
                        },
                        ProcIsExistsProperty = eventBean => true,
                        ProcGetFragment = eventBean => null
                    };
                }
    
                var type = spec.BaseEventType.GetPropertyType(property);
                var propertyTypeDesc = new RevisionPropertyTypeDesc(revisionGetter, null, type);
                propertyDesc.Put(property, propertyTypeDesc);
                count++;
            }
    
            return propertyDesc;
        }
    }
}
