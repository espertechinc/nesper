///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.threading.locks;


namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>
    /// Holder for statement group's statement metrics.
    /// <para/>Changes to StatementMetric instances must be done in a read-lock:
    /// getRwLock.readLock.lock()
    /// metric = getAddMetric(index)
    /// metric.accountFor(cpu, wall, etc)
    /// getRwLock.readLock.unlock()
    /// <para/>All other changes are done under write lock for this class.
    /// <para/>This is a collection backed by an array that grows by 50% each time expanded, maintains a free/busy list of statement names,
    /// maintains an element number of last used element.
    /// <para/>The flush operaton copies the complete array, thereby keeping array size. Statement names are only removed on the next flush.
    /// </summary>
    public class StatementMetricArray
    {
        private readonly string _runtimeUri;

        // Lock
        //  Read lock applies to each current transaction on a StatementMetric instance
        //  Write lock applies to flush and to add a new statement
        private readonly ManagedReadWriteLock _rwLock;
        private readonly string _name;

        private readonly bool _isReportInactive;

        // Active statements
        private DeploymentIdNamePair[] _statementNames;

        // Count of active statements
        private int _currentLastElement;

        // Flushed metric per statement
        private volatile StatementMetric[] _metrics;

        // Statements ids to remove with the next flush
        private ISet<DeploymentIdNamePair> _removedStatementNames;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "runtimeURI">runtime URI</param>
        /// <param name = "name">name of statement group</param>
        /// <param name = "initialSize">initial size of array</param>
        /// <param name = "isReportInactive">true to indicate to report on inactive statements</param>
        public StatementMetricArray(
            string runtimeURI,
            string name,
            int initialSize,
            bool isReportInactive,
            IReaderWriterLockManager rwLockManager)
        {
            _runtimeUri = runtimeURI;
            _isReportInactive = isReportInactive;
            _name = name;
            _metrics = new StatementMetric[initialSize];
            _statementNames = new DeploymentIdNamePair[initialSize];
            _currentLastElement = -1;
            // rwLock = rwLockManager.CreateLock(GetType());
            _rwLock = new ManagedReadWriteLock("StatementMetricArray-" + name, true);
            _removedStatementNames = new HashSet<DeploymentIdNamePair>();
        }

        /// <summary>
        /// Remove a statement.
        /// <para/>Next flush actually frees the slot that this statement occupied.
        /// </summary>
        /// <param name = "statement">to remove</param>
        public void RemoveStatement(DeploymentIdNamePair statement)
        {
            _rwLock.AcquireWriteLock();
            try {
                _removedStatementNames.Add(statement);
                if (_removedStatementNames.Count > 1000) {
                    for (var i = 0; i <= _currentLastElement; i++) {
                        if (_removedStatementNames.Contains(_statementNames[i])) {
                            _statementNames[i] = null;
                        }
                    }

                    _removedStatementNames.Clear();
                }
            }
            finally {
                _rwLock.ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Adds a statement and returns the index added at.
        /// <para/>May reuse an empty slot, grow the underlying array, or append to the end.
        /// </summary>
        /// <param name = "statement">deployment-id and name pair</param>
        /// <returns>index added to</returns>
        public int AddStatementGetIndex(DeploymentIdNamePair statement)
        {
            _rwLock.AcquireWriteLock();
            try {
                // see if there is room
                if (_currentLastElement + 1 < _metrics.Length) {
                    _currentLastElement++;
                    _statementNames[_currentLastElement] = statement;
                    return _currentLastElement;
                }

                // no room, try to use an existing slot of a removed statement
                for (var i = 0; i < _statementNames.Length; i++) {
                    if (_statementNames[i] == null) {
                        _statementNames[i] = statement;
                        if (i + 1 > _currentLastElement) {
                            _currentLastElement = i;
                        }

                        return i;
                    }
                }

                // still no room, expand storage by 50%
                var newSize = (int)(_metrics.Length * 1.5);
                var newStatementNames = new DeploymentIdNamePair[newSize];
                var newMetrics = new StatementMetric[newSize];
                Array.Copy(_statementNames, 0, newStatementNames, 0, _statementNames.Length);
                Array.Copy(_metrics, 0, newMetrics, 0, _metrics.Length);
                _statementNames = newStatementNames;
                _metrics = newMetrics;
                _currentLastElement++;
                _statementNames[_currentLastElement] = statement;
                return _currentLastElement;
            }
            finally {
                _rwLock.ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Flushes the existing metrics via array copy and swap.
        /// <para/>May report all statements (empty and non-empty slots) and thereby null values.
        /// <para/>Returns null to indicate no reports to do.
        /// </summary>
        /// <returns>metrics</returns>
        public StatementMetric[] FlushMetrics()
        {
            _rwLock.AcquireWriteLock();
            try {
                var isEmpty = _currentLastElement == -1;
                Housekeeping();
                if (isEmpty) {
                    return null; // no copies made if empty collection
                }

                // perform flush
                var newMetrics = new StatementMetric[_metrics.Length];
                var oldMetrics = _metrics;
                _metrics = newMetrics;
                return oldMetrics;
            }
            finally {
                _rwLock.ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Returns an existing or creates a new statement metric for the index.
        /// </summary>
        /// <param name = "index">of statement</param>
        /// <returns>metric to modify under read lock</returns>
        public StatementMetric GetAddMetric(int index)
        {
            var metric = _metrics[index];
            if (metric == null) {
                metric = new StatementMetric(
                    _runtimeUri,
                    _statementNames[index].DeploymentId,
                    _statementNames[index].Name);
                _metrics[index] = metric;
            }

            return metric;
        }

        /// <summary>
        /// Returns maximum collection size (last used element), which may not truely reflect the number
        /// of actual statements held as some slots may empty up when statements are removed.
        /// </summary>
        /// <returns>known maximum size</returns>
        public int SizeLastElement()
        {
            return _currentLastElement + 1;
        }

        public void Enumerate(Consumer<EPMetricsStatement> consumer)
        {
            _rwLock.AcquireReadLock();
            try {
                Housekeeping();
                foreach (var metric in _metrics) {
                    if (metric != null) {
                        consumer.Invoke(new EPMetricsStatement(metric));
                    }
                }
            }
            finally {
                _rwLock.ReleaseReadLock();
            }
        }

        private void Housekeeping()
        {
            // first fill in the blanks if there are no reports and we report inactive statements
            if (_isReportInactive) {
                for (var i = 0; i <= _currentLastElement; i++) {
                    if (_statementNames[i] != null) {
                        _metrics[i] = new StatementMetric(
                            _runtimeUri,
                            _statementNames[i].DeploymentId,
                            _statementNames[i].Name);
                    }
                }
            }

            // remove statement ids that disappeared during the interval
            if (_currentLastElement > -1 && !_removedStatementNames.IsEmpty()) {
                for (var i = 0; i <= _currentLastElement; i++) {
                    if (_removedStatementNames.Contains(_statementNames[i])) {
                        _statementNames[i] = null;
                    }
                }
            }

            // adjust last used element
            while (_currentLastElement != -1 && _statementNames[_currentLastElement] == null) {
                _currentLastElement--;
            }
        }

        public bool IsReportInactive => _isReportInactive;

        public ManagedReadWriteLock RWLock => _rwLock;

        public string Name => _name;
    }
} // end of namespace