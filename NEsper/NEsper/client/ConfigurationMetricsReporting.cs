///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.type;

namespace com.espertech.esper.client
{
    /// <summary>Configuratiom for metrics reporting.</summary>
    [Serializable]
    public class ConfigurationMetricsReporting {
        private bool _isEnableMetricsReporting;
        private bool _isThreading;
        private long _engineInterval;
        private long _statementInterval;
        private readonly IDictionary<string, StmtGroupMetrics> _statementGroups;
    
        /// <summary>Ctor.</summary>
        public ConfigurationMetricsReporting() {
            _isEnableMetricsReporting = false;
            _isThreading = true;
            _engineInterval = 10 * 1000; // 10 seconds
            _statementInterval = 10 * 1000;
            _statementGroups = new LinkedHashMap<string, StmtGroupMetrics>();
        }
    
        /// <summary>
        /// Add a statement group, allowing control of metrics reporting interval per statement or
        /// per multiple statements. The reporting interval and be changed at runtime.
        /// <para>
        /// Add pattern include and exclude criteria to control which
        /// </para>
        /// </summary>
        /// <param name="name">
        /// of statement group, not connected to statement name, assigned as an
        /// arbitrary identifier for runtime changes to the interval
        /// </param>
        /// <param name="config">the statement group metrics configuration</param>
        public void AddStmtGroup(string name, StmtGroupMetrics config) {
            _statementGroups.Put(name, config);
        }

        /// <summary>
        /// Gets or sets true if metrics reporting is turned on, false if not.
        /// </summary>
        public bool IsEnableMetricsReporting {
            get => _isEnableMetricsReporting;
            set => _isEnableMetricsReporting = value;
        }

        /// <summary>
        /// Gets or sets true to indicate that metrics reporting takes place in a
        /// separate thread (default), or false to indicate that metrics reporting 
        /// takes place as part of timer processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is threading; otherwise, <c>false</c>.
        /// </value>
        public bool IsThreading {
            get => _isThreading;
            set => _isThreading = value;
        }

        /// <summary>
        /// Gets or sets the engine metrics production interval in milliseconds.
        /// <para>
        /// Use a negative or zero value to disable engine metrics production.
        /// </para>
        /// </summary>
        /// <value>
        /// The engine interval.
        /// </value>
        public long EngineInterval {
            get => _engineInterval;
            set => _engineInterval = value;
        }

        /// <summary>
        /// Gets or sets the statement metrics production interval in milliseconds,
        /// unless statement groups have been defined that override this setting for certain statements.
        /// </summary>
        /// <value>
        /// The statement interval.
        /// </value>
        public long StatementInterval {
            get => _statementInterval;
            set => _statementInterval = value;
        }

        /// <summary>
        /// Returns a map of statement group and metrics configuration for the statement group.
        /// </summary>
        /// <value>map of statement group and metrics configuration</value>
        public IDictionary<string, StmtGroupMetrics> StatementGroups {
            get { return _statementGroups; }
        }

        /// <summary>
        /// Sets a new interval for a statement group identified by name.
        /// </summary>
        /// <param name="stmtGroupName">name of statement group as assigned through configuration</param>
        /// <param name="newInterval">new interval, or a -1 or zero value to disable reporting</param>
        public void SetStatementGroupInterval(string stmtGroupName, long newInterval) {
            StmtGroupMetrics metrics = _statementGroups.Get(stmtGroupName);
            if (metrics != null) {
                metrics.Interval = newInterval;
            } else {
                throw new ConfigurationException("Statement group by name '" + stmtGroupName + "' could not be found");
            }
        }
    
        /// <summary>
        /// Type to configure statement metrics reporting for a group of one or more statements.
        /// </summary>
        [Serializable]
        public class StmtGroupMetrics {
            private readonly IList<Pair<StringPatternSet, bool>> _patterns;
            private int _numStatements;
            private long _interval;
            private bool _reportInactive;
            private bool _defaultInclude;
    
            /// <summary>Ctor.</summary>
            public StmtGroupMetrics() {
                _patterns = new List<Pair<StringPatternSet, bool>>();
                _interval = 10000;
                _numStatements = 100;
            }
    
            /// <summary>
            /// Include all statements in the statement group that match the SQL like-expression by statement name.
            /// </summary>
            /// <param name="likeExpression">to match</param>
            public void AddIncludeLike(string likeExpression) {
                _patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike(likeExpression), true));
            }
    
            /// <summary>
            /// Exclude all statements from the statement group that match the SQL like-expression by statement name.
            /// </summary>
            /// <param name="likeExpression">to match</param>
            public void AddExcludeLike(string likeExpression) {
                _patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike(likeExpression), false));
            }
    
            /// <summary>
            /// Include all statements in the statement group that match the regular expression by statement name.
            /// </summary>
            /// <param name="regexExpression">to match</param>
            public void AddIncludeRegex(string regexExpression) {
                _patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex(regexExpression), true));
            }
    
            /// <summary>
            /// Exclude all statements in the statement group that match the regular expression by statement name.
            /// </summary>
            /// <param name="regexExpression">to match</param>
            public void AddExcludeRegEx(string regexExpression) {
                _patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex(regexExpression), false));
            }

            /// <summary>
            /// Gets or sets the reporting interval for statement metrics for statements in the statement group.
            /// </summary>
            public long Interval {
                get => _interval;
                set => _interval = value;
            }

            /// <summary>
            /// Returns a list of patterns that indicate whether a statement, by the statement name matching or
            /// not matching each pattern, falls within the statement group.
            /// <para>
            /// Include-patterns are bool true in the pair of pattern and bool. Exclude-patterns are
            /// bool false.
            /// </para>
            /// </summary>
            public IList<Pair<StringPatternSet, bool>> Patterns => _patterns;

            /// <summary>
            /// Gets or sets the initial capacity number of statements held by the statement group.
            /// </summary>
            public int NumStatements {
                get => _numStatements;
                set => _numStatements = value;
            }

            /// <summary>
            /// Returns true to indicate that inactive statements (statements without events or timer activity)
            /// are also reported, or false to omit reporting for inactive statements.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is report inactive; otherwise, <c>false</c>.
            /// </value>
            public bool IsReportInactive {
                get => _reportInactive;
                set => _reportInactive = value;
            }

            /// <summary>
            /// If this flag is set then all statement names are automatically included in this
            /// statement group, and through exclude-pattern certain statement names can be omitted
            /// <para>
            /// If this flag is not set then all statement names are automatically excluded in this
            /// statement group, and through include-pattern certain statement names can be included.
            /// </para>
            /// <para>
            /// The default is false, i.e. statements must be explicitly included.
            /// </para>
            /// </summary>
            public bool IsDefaultInclude {
                get => _defaultInclude;
                set => _defaultInclude = value;
            }
        }
    }
} // end of namespace
