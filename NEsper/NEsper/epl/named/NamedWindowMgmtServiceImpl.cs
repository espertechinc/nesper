///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// This service hold for each named window a dedicated processor and a lock to the named window.
    /// This lock is shrared between the named window and on-delete statements.
    /// </summary>
    public class NamedWindowMgmtServiceImpl : NamedWindowMgmtService
    {
        private readonly IDictionary<string, NamedWindowProcessor> _processors;
        private readonly IDictionary<string, NamedWindowLockPair> _windowStatementLocks;
        private readonly ISet<NamedWindowLifecycleObserver> _observers;
        private readonly bool _enableQueryPlanLog;
        private readonly MetricReportingService _metricReportingService;

        /// <summary>
        /// Ctor.
        /// </summary>
        public NamedWindowMgmtServiceImpl(bool enableQueryPlanLog, MetricReportingService metricReportingService)
        {
            _processors = new Dictionary<string, NamedWindowProcessor>().WithNullSupport();
            _windowStatementLocks = new Dictionary<string, NamedWindowLockPair>().WithNullSupport();
            _observers = new HashSet<NamedWindowLifecycleObserver>();
            _enableQueryPlanLog = enableQueryPlanLog;
            _metricReportingService = metricReportingService;
        }

        public void Dispose()
        {
            _processors.Clear();
        }

        public string[] NamedWindows => _processors.Keys.ToArrayOrNull();

        public IReaderWriterLock GetNamedWindowLock(string windowName)
        {
            var pair = _windowStatementLocks.Get(windowName);
            if (pair == null)
            {
                return null;
            }
            return pair.Lock;
        }

        public void AddNamedWindowLock(string windowName, IReaderWriterLock statementResourceLock, string statementName)
        {
            _windowStatementLocks.Put(windowName, new NamedWindowLockPair(statementName, statementResourceLock));
        }

        public void RemoveNamedWindowLock(string statementName)
        {
            foreach (var entry in _windowStatementLocks)
            {
                if (entry.Value.StatementName == statementName)
                {
                    _windowStatementLocks.Remove(entry.Key);
                    return;
                }
            }
        }

        public bool IsNamedWindow(string name)
        {
            return _processors.ContainsKey(name);
        }

        public NamedWindowProcessor GetProcessor(string name)
        {
            return _processors.Get(name);
        }

        public IndexMultiKey[] GetNamedWindowIndexes(string windowName)
        {
            var processor = _processors.Get(windowName);
            if (processor == null)
            {
                return null;
            }
            return processor.GetProcessorInstance(null).IndexDescriptors;
        }

        public void RemoveNamedWindowIfFound(string namedWindowName)
        {
            var processor = _processors.Get(namedWindowName);
            if (processor == null)
            {
                return;
            }
            processor.ClearProcessorInstances();
            RemoveProcessor(namedWindowName);
        }

        public NamedWindowProcessor AddProcessor(
            string name,
            string contextName,
            EventType eventType,
            StatementResultService statementResultService,
            ValueAddEventProcessor revisionProcessor,
            string eplExpression,
            string statementName,
            bool isPrioritized,
            bool isEnableSubqueryIndexShare,
            bool isBatchingDataWindow,
            bool isVirtualDataWindow,
            ICollection<string> optionalUniqueKeyProps,
            string eventTypeAsName,
            StatementContext statementContextCreateWindow,
            NamedWindowDispatchService namedWindowDispatchService,
            ILockManager lockManager)
        {
            if (_processors.ContainsKey(name))
            {
                throw new ViewProcessingException("A named window by name '" + name + "' has already been created");
            }

            var processor = namedWindowDispatchService.CreateProcessor(
                name, this, 
                namedWindowDispatchService, 
                contextName, 
                eventType, 
                statementResultService, 
                revisionProcessor,
                eplExpression, 
                statementName, 
                isPrioritized, 
                isEnableSubqueryIndexShare, 
                _enableQueryPlanLog,
                _metricReportingService, 
                isBatchingDataWindow, 
                isVirtualDataWindow, 
                optionalUniqueKeyProps, 
                eventTypeAsName,
                statementContextCreateWindow,
                lockManager);
            _processors.Put(name, processor);

            if (!_observers.IsEmpty())
            {
                var theEvent = new NamedWindowLifecycleEvent(name, processor, NamedWindowLifecycleEvent.LifecycleEventType.CREATE);
                foreach (var observer in _observers)
                {
                    observer.Observe(theEvent);
                }
            }

            return processor;
        }

        public void RemoveProcessor(string name)
        {
            var processor = _processors.Get(name);
            if (processor != null)
            {
                processor.Dispose();
                _processors.Remove(name);

                if (!_observers.IsEmpty())
                {
                    var theEvent = new NamedWindowLifecycleEvent(name, processor, NamedWindowLifecycleEvent.LifecycleEventType.DESTROY);
                    foreach (var observer in _observers)
                    {
                        observer.Observe(theEvent);
                    }
                }
            }
        }

        public void AddObserver(NamedWindowLifecycleObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(NamedWindowLifecycleObserver observer)
        {
            _observers.Remove(observer);
        }

        internal class NamedWindowLockPair
        {
            internal string StatementName { get; private set; }
            internal IReaderWriterLock Lock { get; private set; }

            internal NamedWindowLockPair(string statementName, IReaderWriterLock mlock)
            {
                StatementName = statementName;
                Lock = mlock;
            }
        }
    }
} // end of namespace
