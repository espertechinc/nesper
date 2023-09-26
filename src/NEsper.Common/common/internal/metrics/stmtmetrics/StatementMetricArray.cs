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
        private readonly string runtimeURI;

        // Lock
        //  Read lock applies to each current transaction on a StatementMetric instance
        //  Write lock applies to flush and to add a new statement
        private readonly ManagedReadWriteLock rwLock;
        private readonly string name;

        private readonly bool isReportInactive;

        // Active statements
        private DeploymentIdNamePair[] statementNames;

        // Count of active statements
        private int currentLastElement;

        // Flushed metric per statement
        private volatile StatementMetric[] metrics;

        // Statements ids to remove with the next flush
        private ISet<DeploymentIdNamePair> removedStatementNames;

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
            this.runtimeURI = runtimeURI;
            this.isReportInactive = isReportInactive;
            this.name = name;
            metrics = new StatementMetric[initialSize];
            statementNames = new DeploymentIdNamePair[initialSize];
            currentLastElement = -1;
            // rwLock = rwLockManager.CreateLock(GetType());
            rwLock = new ManagedReadWriteLock("StatementMetricArray-" + name, true);
            removedStatementNames = new HashSet<DeploymentIdNamePair>();
        }

        /// <summary>
        /// Remove a statement.
        /// <para/>Next flush actually frees the slot that this statement occupied.
        /// </summary>
        /// <param name = "statement">to remove</param>
        public void RemoveStatement(DeploymentIdNamePair statement)
        {
            rwLock.AcquireWriteLock();
            try {
                removedStatementNames.Add(statement);
                if (removedStatementNames.Count > 1000) {
                    for (var i = 0; i <= currentLastElement; i++) {
                        if (removedStatementNames.Contains(statementNames[i])) {
                            statementNames[i] = null;
                        }
                    }

                    removedStatementNames.Clear();
                }
            }
            finally {
                rwLock.ReleaseWriteLock();
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
            rwLock.AcquireWriteLock();
            try {
                // see if there is room
                if (currentLastElement + 1 < metrics.Length) {
                    currentLastElement++;
                    statementNames[currentLastElement] = statement;
                    return currentLastElement;
                }

                // no room, try to use an existing slot of a removed statement
                for (var i = 0; i < statementNames.Length; i++) {
                    if (statementNames[i] == null) {
                        statementNames[i] = statement;
                        if (i + 1 > currentLastElement) {
                            currentLastElement = i;
                        }

                        return i;
                    }
                }

                // still no room, expand storage by 50%
                var newSize = (int)(metrics.Length * 1.5);
                var newStatementNames = new DeploymentIdNamePair[newSize];
                var newMetrics = new StatementMetric[newSize];
                Array.Copy(statementNames, 0, newStatementNames, 0, statementNames.Length);
                Array.Copy(metrics, 0, newMetrics, 0, metrics.Length);
                statementNames = newStatementNames;
                metrics = newMetrics;
                currentLastElement++;
                statementNames[currentLastElement] = statement;
                return currentLastElement;
            }
            finally {
                rwLock.ReleaseWriteLock();
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
            rwLock.AcquireWriteLock();
            try {
                var isEmpty = currentLastElement == -1;
                Housekeeping();
                if (isEmpty) {
                    return null; // no copies made if empty collection
                }

                // perform flush
                var newMetrics = new StatementMetric[metrics.Length];
                var oldMetrics = metrics;
                metrics = newMetrics;
                return oldMetrics;
            }
            finally {
                rwLock.ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Returns an existing or creates a new statement metric for the index.
        /// </summary>
        /// <param name = "index">of statement</param>
        /// <returns>metric to modify under read lock</returns>
        public StatementMetric GetAddMetric(int index)
        {
            var metric = metrics[index];
            if (metric == null) {
                metric = new StatementMetric(
                    runtimeURI,
                    statementNames[index].DeploymentId,
                    statementNames[index].Name);
                metrics[index] = metric;
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
            return currentLastElement + 1;
        }

        public void Enumerate(Consumer<EPMetricsStatement> consumer)
        {
            rwLock.AcquireReadLock();
            try {
                Housekeeping();
                foreach (var metric in metrics) {
                    if (metric != null) {
                        consumer.Invoke(new EPMetricsStatement(metric));
                    }
                }
            }
            finally {
                rwLock.ReleaseReadLock();
            }
        }

        private void Housekeeping()
        {
            // first fill in the blanks if there are no reports and we report inactive statements
            if (isReportInactive) {
                for (var i = 0; i <= currentLastElement; i++) {
                    if (statementNames[i] != null) {
                        metrics[i] = new StatementMetric(
                            runtimeURI,
                            statementNames[i].DeploymentId,
                            statementNames[i].Name);
                    }
                }
            }

            // remove statement ids that disappeared during the interval
            if (currentLastElement > -1 && !removedStatementNames.IsEmpty()) {
                for (var i = 0; i <= currentLastElement; i++) {
                    if (removedStatementNames.Contains(statementNames[i])) {
                        statementNames[i] = null;
                    }
                }
            }

            // adjust last used element
            while (currentLastElement != -1 && statementNames[currentLastElement] == null) {
                currentLastElement--;
            }
        }

        public bool IsReportInactive => isReportInactive;

        public ManagedReadWriteLock RWLock => rwLock;

        public string Name => name;
    }
} // end of namespace