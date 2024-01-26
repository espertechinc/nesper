///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Configuration for metrics reporting.
    /// </summary>
    public class ConfigurationRuntimeMetricsReporting
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationRuntimeMetricsReporting()
        {
            IsRuntimeMetrics = false;
            IsEnableMetricsReporting = false;
            IsThreading = true;
            RuntimeInterval = 10 * 1000; // 10 seconds
            StatementInterval = 10 * 1000;
            StatementGroups = new LinkedHashMap<string, StmtGroupMetrics>();
        }

        /// <summary>
        ///     Returns true if metrics reporting is turned on, false if not.
        /// </summary>
        /// <returns>indicator whether metrics reporting is turned on</returns>
        public bool IsEnableMetricsReporting { get; set; }

        /// <summary>
        ///     Returns true to indicate that metrics reporting takes place in a separate thread (default),
        ///     or false to indicate that metrics reporting takes place as part of timer processing.
        /// </summary>
        /// <returns>indicator whether metrics reporting is threaded</returns>
        public bool IsThreading { get; set; }

        /// <summary>
        ///     Returns the runtime metrics production interval in milliseconds.
        /// </summary>
        /// <returns>runtime metrics production interval</returns>
        public long RuntimeInterval { get; set; }

        /// <summary>
        ///     Returns the statement metrics production interval in milliseconds,
        ///     unless statement groups have been defined that override this setting for certain statements.
        /// </summary>
        /// <returns>statement metrics production interval</returns>
        public long StatementInterval { get; set; }

        /// <summary>
        ///     that provide key runtime metrics.
        /// </summary>
        /// <returns>indicator</returns>
        public bool IsRuntimeMetrics { get; set; }

        /// <summary>
        ///     Returns a map of statement group and metrics configuration for the statement group.
        /// </summary>
        /// <value>map of statement group and metrics configuration</value>
        public IDictionary<string, StmtGroupMetrics> StatementGroups { get; set; }


        public ConfigurationRuntimeMetricsReporting WithMetricsReporting(bool value)
        {
            IsEnableMetricsReporting = value;
            return this;
        }

        public ConfigurationRuntimeMetricsReporting WithThreading(bool value)
        {
            IsThreading = value;
            return this;
        }


        public ConfigurationRuntimeMetricsReporting WithRuntimeInterval(long value)
        {
            RuntimeInterval = value;
            return this;
        }

        public ConfigurationRuntimeMetricsReporting WithStatementInterval(long value)
        {
            StatementInterval = value;
            return this;
        }

        public ConfigurationRuntimeMetricsReporting WithRuntimeMetrics(bool value)
        {
            IsRuntimeMetrics = value;
            return this;
        }

        /// <summary>
        ///     Add a statement group, allowing control of metrics reporting interval per statement or
        ///     per multiple statements. The reporting interval and be changed at runtime.
        ///     <para />
        ///     Add pattern include and exclude criteria to control which
        /// </summary>
        /// <param name="name">
        ///     of statement group, not connected to statement name, assigned as anarbitrary identifier for runtime changes to the
        ///     interval
        /// </param>
        /// <param name="config">the statement group metrics configuration</param>
        public void AddStmtGroup(
            string name,
            StmtGroupMetrics config)
        {
            StatementGroups.Put(name, config);
        }

        /// <summary>
        ///     Sets a new interval for a statement group identified by name.
        /// </summary>
        /// <param name="stmtGroupName">name of statement group as assigned through configuration</param>
        /// <param name="newInterval">new interval, or a -1 or zero value to disable reporting</param>
        public void SetStatementGroupInterval(
            string stmtGroupName,
            long newInterval)
        {
            var metrics = StatementGroups.Get(stmtGroupName);
            if (metrics != null) {
                metrics.Interval = newInterval;
            }
            else {
                throw new ConfigurationException("Statement group by name '" + stmtGroupName + "' could not be found");
            }
        }

        /// <summary>
        ///     Class to configure statement metrics reporting for a group of one or more statements.
        /// </summary>
        public class StmtGroupMetrics
        {
            /// <summary>
            ///     Ctor.
            /// </summary>
            public StmtGroupMetrics()
            {
                Patterns = new List<Pair<StringPatternSet, bool>>();
                Interval = 10000;
                NumStatements = 100;
            }

            [JsonConstructor]
            public StmtGroupMetrics(
                long interval,
                int numStatements,
                bool isReportInactive,
                bool isDefaultInclude,
                IList<Pair<StringPatternSet, bool>> patterns)
            {
                Patterns = patterns;
                Interval = interval;
                NumStatements = numStatements;
                IsReportInactive = isReportInactive;
                IsDefaultInclude = isDefaultInclude;
            }

            /// <summary>
            ///     Returns the reporting interval for statement metrics for statements in the statement group.
            /// </summary>
            /// <returns>interval</returns>
            public long Interval { get; set; }

            /// <summary>
            ///     Returns the initial capacity number of statements held by the statement group.
            /// </summary>
            /// <returns>initial capacity</returns>
            public int NumStatements { get; set; }

            /// <summary>
            ///     Returns true to indicate that inactive statements (statements without events or timer activity)
            ///     are also reported.
            /// </summary>
            /// <returns>true for reporting inactive statements</returns>
            public bool IsReportInactive { get; set; }

            /// <summary>
            ///     If this flag is set then all statement names are automatically included in this
            ///     statement group, and through exclude-pattern certain statement names can be omitted
            ///     <para />
            ///     If this flag is not set then all statement names are automatically excluded in this
            ///     statement group, and through include-pattern certain statement names can be included.
            ///     <para />
            ///     The default is false, i.e. statements must be explicitly included.
            /// </summary>
            /// <returns>true for include all statements, false for explicitly include</returns>
            public bool IsDefaultInclude { get; set; }

            /// <summary>
            ///     Returns a list of patterns that indicate whether a statement, by the statement name matching or
            ///     not matching each pattern, falls within the statement group.
            ///     <para />
            ///     Include-patterns are boolean true in the pair of pattern and boolean. Exclude-patterns are
            ///     boolean false.
            /// </summary>
            /// <value>list of include and exclude pattern</value>
            public IList<Pair<StringPatternSet, bool>> Patterns { get; }

            /// <summary>
            ///     Include all statements in the statement group that match the SQL like-expression by statement name.
            /// </summary>
            /// <param name="likeExpression">to match</param>
            public void AddIncludeLike(string likeExpression)
            {
                Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike(likeExpression), true));
            }

            /// <summary>
            ///     Exclude all statements from the statement group that match the SQL like-expression by statement name.
            /// </summary>
            /// <param name="likeExpression">to match</param>
            public void AddExcludeLike(string likeExpression)
            {
                Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike(likeExpression), false));
            }

            /// <summary>
            ///     Include all statements in the statement group that match the regular expression by statement name.
            /// </summary>
            /// <param name="regexExpression">to match</param>
            public void AddIncludeRegex(string regexExpression)
            {
                Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex(regexExpression), true));
            }

            /// <summary>
            ///     Exclude all statements in the statement group that match the regular expression by statement name.
            /// </summary>
            /// <param name="regexExpression">to match</param>
            public void AddExcludeRegEx(string regexExpression)
            {
                Patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex(regexExpression), false));
            }
        }
    }
} // end of namespace