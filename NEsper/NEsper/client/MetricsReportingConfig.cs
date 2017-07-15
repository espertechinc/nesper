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
    /// <summary>Configuratiom for metrics reporting. </summary>
    [Serializable]
    public class MetricsReportingConfig
    {
        /// <summary>Ctor. </summary>
        public MetricsReportingConfig()
        {
            IsEnableMetricsReporting = false;
            IsThreading = true;
            EngineInterval = 10 * 1000; // 10 seconds
            StatementInterval = 10 * 1000;
            StatementGroups = new LinkedHashMap<String, StmtGroupMetrics>();
        }

        /// <summary>
        /// Add a statement group, allowing control of metrics reporting interval per statement or per multiple statements.
        /// The reporting interval and be changed at runtime.
        /// <para/>
        /// Add pattern include and exclude criteria to control which
        /// </summary>
        /// <param name="name">of statement group, not connected to statement name, assigned as anarbitrary identifier for runtime changes to the interval</param>
        /// <param name="config">the statement group metrics configuration</param>
        public void AddStmtGroup(String name, StmtGroupMetrics config)
        {
            StatementGroups.Put(name, config);
        }

        /// <summary>Returns true if metrics reporting is turned on, false if not. </summary>
        /// <returns>indicator whether metrics reporting is turned on</returns>
        public bool IsEnableMetricsReporting { get; set; }

        /// <summary>Returns true to indicate that metrics reporting takes place in a separate thread (default), or false to indicate that metrics reporting takes place as part of timer processing. </summary>
        /// <returns>indicator whether metrics reporting is threaded</returns>
        public bool IsThreading { get; set; }

        /// <summary>
        /// Gets or sets the engine metrics production interval in milliseconds.
        /// <para/>
        /// Use a negative or zero value to disable engine metrics production.
        /// </summary>
        /// <value>The engine interval.</value>
        public long EngineInterval { set; get; }

        /// <summary>
        /// Gets or sets the statement metrics production interval in milliseconds, unless statement groups have been defined that override this setting for certain statements.
        /// </summary>
        /// <value>The statement interval.</value>
        public long StatementInterval { get; set; }

        /// <summary>Returns a map of statement group and metrics configuration for the statement group. </summary>
        /// <returns>map of statement group and metrics configuration</returns>
        public IDictionary<string, StmtGroupMetrics> StatementGroups { get; private set; }

        /// <summary>Sets a new interval for a statement group identified by name. </summary>
        /// <param name="stmtGroupName">name of statement group as assigned through configuration</param>
        /// <param name="newInterval">new interval, or a -1 or zero value to disable reporting</param>
        public void SetStatementGroupInterval(String stmtGroupName, long newInterval)
        {
            StmtGroupMetrics metrics = StatementGroups.Get(stmtGroupName);
            if (metrics != null)
            {
                metrics.Interval = newInterval;
            }
            else
            {
                throw new ConfigurationException("Statement group by name '" + stmtGroupName + "' could not be found");
            }
        }
    
        /// <summary>
        /// Class to configure statement metrics reporting for a group of one or more statements.
        /// </summary>
        [Serializable]
        public class StmtGroupMetrics
        {
            /// <summary>
            /// Ctor.
            /// </summary>
            public StmtGroupMetrics()
            {
                Patterns = new List<Pair<StringPatternSet, Boolean>>();
                Interval =  10000;
                NumStatements = 100;
            }

            /// <summary>
            /// Include all statements in the statement group that match the SQL like-expression by statement name.
            /// </summary>
            /// <param name="likeExpression">to match</param>
            public void AddIncludeLike(String likeExpression)
            {
                Patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetLike(likeExpression), true));
            }

            /// <summary>
            /// Exclude all statements from the statement group that match the SQL like-expression by statement name.
            /// </summary>
            /// <param name="likeExpression">to match</param>
            public void AddExcludeLike(String likeExpression)
            {
                Patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetLike(likeExpression), false));
            }

            /// <summary>
            /// Include all statements in the statement group that match the regular expression by statement name.
            /// </summary>
            /// <param name="regexExpression">to match</param>
            public void AddIncludeRegex(String regexExpression)
            {
                Patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex(regexExpression), true));
            }

            /// <summary>
            /// Exclude all statements in the statement group that match the regular expression by statement name.
            /// </summary>
            /// <param name="regexExpression">to match</param>
            public void AddExcludeRegEx(String regexExpression)
            {
                Patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex(regexExpression), false));
            }

            /// <summary>
            /// Gets or sets the reporting interval for statement metrics for statements in the statement group.
            /// </summary>
            /// <value>The interval.</value>
            public long Interval { get; set; }

            /// <summary>
            /// Returns a list of patterns that indicate whether a statement, by the statement name matching or not matching 
            /// each pattern, falls within the statement group.
            /// <para/>
            /// Include-patterns are bool true in the pair of pattern and bool. Exclude-patterns are bool false.
            /// </summary>
            /// <value>The patterns.</value>
            /// <returns>list of include and exclude pattern</returns>
            public IList<Pair<StringPatternSet, bool>> Patterns { get; private set; }

            /// <summary>
            /// Gets or sets the initial capacity number of statements held by the statement group.
            /// </summary>
            /// <value>The num statements.</value>
            public int NumStatements { get; set; }

            /// <summary>
            /// Gets or sets a value to indicate that inactive statements (statements without events or timer activity) 
            /// are also reported, or false to omit reporting for inactive statements.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is report inactive; otherwise, <c>false</c>.
            /// </value>
            public bool IsReportInactive { get; set; }

            /// <summary>
            /// If this flag is set then all statement names are automatically included in this statement group, 
            /// and through exclude-pattern certain statement names can be omitted 
            /// <para/>
            /// If this flag is not set then all statement names are automatically excluded in this statement group, 
            /// and through include-pattern certain statement names can be included. 
            /// <para/>
            /// The default is false, i.e. statements must be explicitly included.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is default include; otherwise, <c>false</c>.
            /// </value>
            /// <returns>true for include all statements, false for explicitly include</returns>
            public bool IsDefaultInclude { get; set; }
        }
    }
}
