///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.diagnostics;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>
    ///     A repository for all statement metrics that organizes statements into statement groups.
    ///     <para />
    ///     At a minimum there is one group (the default) of index zero.
    /// </summary>
    public class StatementMetricRepository
    {
        private readonly StatementMetricArray[] _groupMetrics;
        private readonly ConfigurationRuntimeMetricsReporting _specification;
        private readonly IDictionary<DeploymentIdNamePair, int> _statementGroups;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeUri">runtime URI</param>
        /// <param name="specification">specifies statement groups</param>
        /// <param name="rwLockManager">the lock manager</param>
        public StatementMetricRepository(
            string runtimeUri,
            ConfigurationRuntimeMetricsReporting specification,
            IReaderWriterLockManager rwLockManager)
        {
            _specification = specification;
            var numGroups = specification.StatementGroups.Count + 1; // +1 for default group (remaining stmts)
            _groupMetrics = new StatementMetricArray[numGroups];

            // default group
            _groupMetrics[0] = new StatementMetricArray(
                runtimeUri,
                "group-default",
                100,
                false,
                rwLockManager);

            // initialize all other groups
            var countGroups = 1;
            foreach (var entry in specification.StatementGroups) {
                var config = entry.Value;

                var initialNumStmts = config.NumStatements;
                if (initialNumStmts < 10) {
                    initialNumStmts = 10;
                }

                _groupMetrics[countGroups] = new StatementMetricArray(
                    runtimeUri,
                    "group-" + countGroups,
                    initialNumStmts,
                    config.IsReportInactive,
                    rwLockManager);
                countGroups++;
            }

            _statementGroups = new Dictionary<DeploymentIdNamePair, int>();
        }

        /// <summary>
        ///     Add a statement, inspecting the statement name and adding it to a statement group or the default group, if none.
        /// </summary>
        /// <param name="statement">name to inspect</param>
        /// <returns>handle for statement</returns>
        public StatementMetricHandle AddStatement(DeploymentIdNamePair statement)
        {
            // determine group
            var countGroups = 1;
            var groupNumber = -1;
            foreach (var entry in _specification.StatementGroups) {
                var patterns = entry.Value.Patterns;
                var result = StringPatternSetUtil.Evaluate(entry.Value.IsDefaultInclude, patterns, statement.Name);
                if (result) {
                    groupNumber = countGroups;
                    break;
                }

                countGroups++;
            }

            // assign to default group if none other apply
            if (groupNumber == -1) {
                groupNumber = 0;
            }

            var index = _groupMetrics[groupNumber].AddStatementGetIndex(statement);

            _statementGroups.Put(statement, groupNumber);

            return new StatementMetricHandle(groupNumber, index);
        }

        /// <summary>
        ///     Remove statement.
        /// </summary>
        /// <param name="statement">to remove</param>
        public void RemoveStatement(DeploymentIdNamePair statement)
        {
            if (_statementGroups.TryRemove(statement, out var group)) {
                _groupMetrics[group].RemoveStatement(statement);
            }
        }

        /// <summary>
        ///     Account statement times.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="performanceMetrics">performance metrics</param>
        /// <param name="numInput">number of input rows</param>
        public void AccountTimes(
            StatementMetricHandle handle,
            PerformanceMetrics performanceMetrics,
            int numInput)
        {
            var array = _groupMetrics[handle.GroupNum];
            using (array.RWLock.AcquireDisposableReadLock()) {
                var metric = array.GetAddMetric(handle.Index);
                metric.AddMetrics(performanceMetrics);
                metric.AddNumInput(numInput);
            }
        }

        /// <summary>
        ///     Account row output.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="numIStream">num rows insert stream</param>
        /// <param name="numRStream">num rows remove stream</param>
        public void AccountOutput(
            StatementMetricHandle handle,
            int numIStream,
            int numRStream)
        {
            var array = _groupMetrics[handle.GroupNum];
            using (array.RWLock.AcquireDisposableReadLock()) {
                var metric = array.GetAddMetric(handle.Index);
                metric.AddNumOutputIStream(numIStream);
                metric.AddNumOutputRStream(numRStream);
            }
        }

        /// <summary>
        ///     Report for a given statement group.
        /// </summary>
        /// <param name="group">to report</param>
        /// <returns>metrics or null if none</returns>
        public StatementMetric[] ReportGroup(int group)
        {
            return _groupMetrics[group].FlushMetrics();
        }

        public void EnumerateMetrics(Consumer<EPMetricsStatementGroup> consumer)
        {
            for (var i = 0; i < _groupMetrics.Length; i++) {
                var array = _groupMetrics[i];
                consumer.Invoke(new EPMetricsStatementGroup(array));
            }
        }
    }
} // end of namespace