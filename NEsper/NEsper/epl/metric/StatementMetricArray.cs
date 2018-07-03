///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client.metric;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.epl.metric
{
    /// <summary>
    /// Holder for statement group's statement metrics.
    /// <para/>
    /// Changes to StatementMetric instances must be done in a read-lock:
    /// <pre>
    /// getRwLock.readLock.Lock()
    /// metric = GetAddMetric(index) metric.AccountFor(cpu, wall, etc)
    /// getRwLock.readLock.unLock()
    /// </pre>
    ///  namespace com.espertech.esper.epl.metric {
    /// <para/>
    /// All other changes are done under write lock for this class.
    /// <para/>
    /// This is a collection backed by an array that grows by 50% each time expanded, maintains
    /// a free/busy list of statement names, maintains an element number of last used element.
    /// <para/>
    /// The flush operaton copies the complete array, thereby keeping array size. Statement names
    /// are only removed on the next flush.
    /// </summary>
    public class StatementMetricArray
    {
        private readonly String engineURI;

        // Lock
        //  Read lock applies to each current transaction on a StatementMetric instance
        //  Write lock applies to flush and to add a new statement
        private readonly bool isReportInactive;
        private readonly ICollection<String> removedStatementNames;
        private readonly IReaderWriterLock rwLock;

        // Active statements

        // Count of active statements
        private int currentLastElement;

        // Flushed metric per statement
        private volatile StatementMetric[] metrics;
        private String[] statementNames;

        // Statements ids to remove with the next flush

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="name">name of statement group</param>
        /// <param name="initialSize">initial size of array</param>
        /// <param name="isReportInactive">true to indicate to report on inactive statements</param>
        /// <param name="rwLockManager">The rw lock manager.</param>
        public StatementMetricArray(
            String engineURI, 
            String name, 
            int initialSize, 
            bool isReportInactive,
            IReaderWriterLockManager rwLockManager)
        {
            this.engineURI = engineURI;
            this.isReportInactive = isReportInactive;

            metrics = new StatementMetric[initialSize];
            statementNames = new String[initialSize];
            currentLastElement = -1;
            rwLock = rwLockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            removedStatementNames = new HashSet<String>();
        }

        /// <summary>Returns the read-write lock, for read-lock when modifications are made. </summary>
        /// <returns>lock</returns>
        public IReaderWriterLock RWLock
        {
            get { return rwLock; }
        }

        /// <summary>Returns maximum collection size (last used element), which may not truely reflect the number of actual statements held as some slots may empty up when statements are removed. </summary>
        /// <returns>known maximum size</returns>
        public int SizeLastElement
        {
            get { return currentLastElement + 1; }
        }

        /// <summary>
        /// Remove a statement.
        /// <para/>
        /// Next flush actually frees the slot that this statement occupied.
        /// </summary>
        /// <param name="statementName">to remove</param>
        public void RemoveStatement(String statementName)
        {
            using (rwLock.AcquireWriteLock()) {
                removedStatementNames.Add(statementName);
                if (removedStatementNames.Count > 1000)
                {
                    for (int i = 0; i <= currentLastElement; i++)
                    {
                        if (removedStatementNames.Contains(statementNames[i]))
                        {
                            statementNames[i] = null;
                        }
                    }

                    removedStatementNames.Clear();
                }
            }
        }

        /// <summary>
        /// Adds a statement and returns the index added at.
        /// <para/>
        /// May reuse an empty slot, grow the underlying array, or append to the end.
        /// </summary>
        /// <param name="statementName">to add</param>
        /// <returns>index added to</returns>
        public int AddStatementGetIndex(String statementName)
        {
            using (rwLock.AcquireWriteLock()) {
                // see if there is room
                if ((currentLastElement + 1) < metrics.Length) {
                    currentLastElement++;
                    statementNames[currentLastElement] = statementName;
                    return currentLastElement;
                }

                // no room, try to use an existing slot of a removed statement
                for (int i = 0; i < statementNames.Length; i++) {
                    if (statementNames[i] == null) {
                        statementNames[i] = statementName;
                        if ((i + 1) > currentLastElement) {
                            currentLastElement = i;
                        }
                        return i;
                    }
                }

                // still no room, expand storage by 50%
                var newSize = (int) (metrics.Length*1.5);
                var newStatementNames = new String[newSize];
                var newMetrics = new StatementMetric[newSize];
                Array.Copy(statementNames, 0, newStatementNames, 0, statementNames.Length);
                Array.Copy(metrics, 0, newMetrics, 0, metrics.Length);

                statementNames = newStatementNames;
                metrics = newMetrics;

                currentLastElement++;
                statementNames[currentLastElement] = statementName;

                return currentLastElement;
            }
        }

        /// <summary>
        /// Flushes the existing metrics via array copy and swap.
        /// <para/>
        /// May report all statements (empty and non-empty slots) and thereby null values. 
        /// <para/>
        /// Returns null to indicate no reports to do.
        /// </summary>
        /// <returns>metrics</returns>
        public StatementMetric[] FlushMetrics()
        {
            using (rwLock.AcquireWriteLock()) {
                bool isEmpty = false;
                if (currentLastElement == -1) {
                    isEmpty = true;
                }

                // first fill in the blanks if there are no reports and we report inactive statements
                if (isReportInactive) {
                    for (int i = 0; i <= currentLastElement; i++) {
                        if (statementNames[i] != null) {
                            metrics[i] = new StatementMetric(engineURI, statementNames[i]);
                        }
                    }
                }

                // remove statement ids that disappeared during the interval
                if ((currentLastElement > -1) && (removedStatementNames.IsNotEmpty())) {
                    for (int i = 0; i <= currentLastElement; i++) {
                        if (removedStatementNames.Contains(statementNames[i])) {
                            statementNames[i] = null;
                        }
                    }
                }

                // adjust last used element
                while ((currentLastElement != -1) && (statementNames[currentLastElement] == null)) {
                    currentLastElement--;
                }

                if (isEmpty) {
                    return null; // no copies made if empty collection
                }

                // perform flush
                var newMetrics = new StatementMetric[metrics.Length];
                StatementMetric[] oldMetrics = metrics;
                metrics = newMetrics;
                return oldMetrics;
            }
        }

        /// <summary>Returns an existing or creates a new statement metric for the index. </summary>
        /// <param name="index">of statement</param>
        /// <returns>metric to modify under read lock</returns>
        public StatementMetric GetAddMetric(int index)
        {
            StatementMetric metric = metrics[index];
            if (metric == null) {
                metric = new StatementMetric(engineURI, statementNames[index]);
                metrics[index] = metric;
            }
            return metric;
        }
    }
}
