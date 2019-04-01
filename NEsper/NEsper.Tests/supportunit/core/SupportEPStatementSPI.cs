///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.core.service;
using com.espertech.esper.view;

namespace com.espertech.esper.supportunit.core
{
    public class SupportEPStatementSPI : EPStatementSPI
    {
#pragma warning disable CS0414
        public event UpdateEventHandler Events;
#pragma warning restore CS0414

        public void RemoveAllEventHandlers()
        {
            Events = null;
        }

        public void AddEventHandlerWithReplay(UpdateEventHandler eventHandler)
        {
        }

        public int StatementId
        {
            get { return 1; }
        }

        public void SetServiceIsolated(String serviceIsolated)
        {
        }

        public string ExpressionNoAnnotations
        {
            get { return null; }
        }

        public void SetListenerSet(EPStatementListenerSet value, bool isRecovery)
        {
        }

        public EPStatementListenerSet GetListenerSet()
        {
            return null;
        }

        public void SetCurrentState(EPStatementState currentState, long timeLastStateChange)
        {
        }

        public Viewable ParentView
        {
            get { return null; }
            set { }
        }

        public StatementMetadata StatementMetadata
        {
            get { return null; }
        }

        public StatementContext StatementContext
        {
            get { return null; }
        }

        public bool IsNameProvided
        {
            get { return false; }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }

        public EPStatementState State
        {
            get { return EPStatementState.STOPPED; }
        }

        public bool IsStarted
        {
            get { return false; }
        }

        public bool IsStopped
        {
            get { return false; }
        }

        public bool IsDisposed
        {
            get { return false; }
        }

        public string Text
        {
            get { return null; }
        }

        public string Name
        {
            get { return null; }
        }

        public long TimeLastStateChange
        {
            get { return 0; }
        }

        public object Subscriber
        {
            get { return null; }
            set { }
        }

        public string SubscriberMethod
        {
            get { return null; }
            set { }
        }

        public bool IsPattern
        {
            get { return false; }
        }

        public object UserObject
        {
            get { return null; }
        }

        public ICollection<Attribute> Annotations
        {
            get { return new Attribute[0]; }
        }

        public string ServiceIsolated
        {
            get { return null; }
            set { }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator(ContextPartitionSelector selector)
        {
            return null;
        }

        public IEnumerator<EventBean> GetSafeEnumerator(ContextPartitionSelector selector)
        {
            return null;
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return null;
        }

        public IEnumerator<EventBean> GetSafeEnumerator()
        {
            return null;
        }

        public EventType EventType
        {
            get { return null; }
        }
    }
}