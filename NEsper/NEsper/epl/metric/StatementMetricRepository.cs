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
using com.espertech.esper.client.metric;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.metric
{
    /// <summary>
    /// A repository for all statement metrics that organizes statements into statement groups. 
    /// <para /> 
    /// At a minimum there is one group (the default) of index zero. </summary>
    public class StatementMetricRepository
    {
        private readonly StatementMetricArray[] _groupMetrics;
        private readonly ConfigurationMetricsReporting _specification;
        private readonly IDictionary<String, int> _statementGroups;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="specification">specifies statement groups</param>
        /// <param name="rwLockManager">The rw lock manager.</param>
        public StatementMetricRepository(
            String engineURI, 
            ConfigurationMetricsReporting specification,
            IReaderWriterLockManager rwLockManager)
        {
            _specification = specification;
            int numGroups = specification.StatementGroups.Count + 1; // +1 for default group (remaining stmts)
            _groupMetrics = new StatementMetricArray[numGroups];

            // default group
            _groupMetrics[0] = new StatementMetricArray(engineURI, "group-default", 100, false, rwLockManager);

            // initialize all other groups
            int countGroups = 1;
            foreach (var entry in specification.StatementGroups)
            {
                ConfigurationMetricsReporting.StmtGroupMetrics config = entry.Value;

                int initialNumStmts = config.NumStatements;
                if (initialNumStmts < 10)
                {
                    initialNumStmts = 10;
                }

                _groupMetrics[countGroups] = new StatementMetricArray(
                    engineURI, "group-" + countGroups, initialNumStmts, config.IsReportInactive, rwLockManager);
                countGroups++;
            }

            _statementGroups = new Dictionary<String, int>();
        }

        /// <summary>Add a statement, inspecting the statement name and adding it to a statement group or the default group, if none. </summary>
        /// <param name="stmtName">name to inspect</param>
        /// <returns>handle for statement</returns>
        public StatementMetricHandle AddStatement(String stmtName)
        {
            // determine group
            int countGroups = 1;
            int groupNumber = -1;
            foreach (var entry in _specification.StatementGroups)
            {
                IList<Pair<StringPatternSet, bool>> patterns = entry.Value.Patterns;
                bool result = StringPatternSetUtil.Evaluate(entry.Value.IsDefaultInclude, patterns, stmtName);

                if (result)
                {
                    groupNumber = countGroups;
                    break;
                }
                countGroups++;
            }

            // assign to default group if none other apply
            if (groupNumber == -1)
            {
                groupNumber = 0;
            }

            int index = _groupMetrics[groupNumber].AddStatementGetIndex(stmtName);

            _statementGroups.Put(stmtName, groupNumber);

            return new StatementMetricHandle(groupNumber, index);
        }

        /// <summary>Remove statement. </summary>
        /// <param name="stmtName">to remove</param>
        public void RemoveStatement(String stmtName)
        {
            if (_statementGroups.TryGetValue(stmtName, out var group)) {
                _statementGroups.Remove(stmtName);
                _groupMetrics[group].RemoveStatement(stmtName);
            }
        }

        /// <summary>
        /// Account statement times.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="cpu">time</param>
        /// <param name="wall">time</param>
        /// <param name="numInput">The num input.</param>
        public void AccountTimes(StatementMetricHandle handle,
                                 long cpu,
                                 long wall,
                                 int numInput)
        {
            StatementMetricArray array = _groupMetrics[handle.GroupNum];
            using (array.RWLock.AcquireReadLock())
            {
                StatementMetric metric = array.GetAddMetric(handle.Index);
                metric.IncrementTime(cpu, wall);
                metric.AddNumInput(numInput);
            }
        }

        /// <summary>Account row output. </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="numIStream">num rows insert stream</param>
        /// <param name="numRStream">num rows remove stream</param>
        public void AccountOutput(StatementMetricHandle handle,
                                  int numIStream,
                                  int numRStream)
        {
            StatementMetricArray array = _groupMetrics[handle.GroupNum];
            using (array.RWLock.AcquireReadLock())
            {
                StatementMetric metric = array.GetAddMetric(handle.Index);
                metric.AddNumOutputIStream(numIStream);
                metric.AddNumOutputRStream(numRStream);
            }
        }

        /// <summary>Report for a given statement group. </summary>
        /// <param name="group">to report</param>
        /// <returns>metrics or null if none</returns>
        public StatementMetric[] ReportGroup(int group)
        {
            return _groupMetrics[group].FlushMetrics();
        }
    }
}