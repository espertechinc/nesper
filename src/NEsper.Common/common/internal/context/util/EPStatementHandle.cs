///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.statement.insertintolatch;
using com.espertech.esper.common.@internal.statement.multimatch;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Class exists once per statement and hold statement resource lock(s).
    /// <para />The statement's self-join flag indicates the the statement may join to itself,
    /// that is a single event may dispatch into multiple streams or patterns for the same statement,
    /// requiring internal dispatch logic to not shortcut evaluation of all filters for the statement
    /// within one lock, requiring the callback handle to be sorted.
    /// </summary>
    public class EPStatementHandle
    {
        private readonly string statementName;
        private readonly string deploymentId;
        private readonly int statementId;
        private readonly string optionalStatementEPL;
        private readonly int priority;
        private readonly bool preemptive;
        private readonly bool canSelfJoin;
        private readonly MultiMatchHandler multiMatchHandler;
        private readonly bool hasVariables;
        private readonly bool hasTableAccess;
        private readonly StatementMetricHandle metricsHandle;
        private readonly InsertIntoLatchFactory insertIntoFrontLatchFactory;
        private readonly InsertIntoLatchFactory insertIntoBackLatchFactory;

        public EPStatementHandle(
            string statementName,
            string deploymentId,
            int statementId,
            string optionalStatementEPL,
            int priority,
            bool preemptive,
            bool canSelfJoin,
            MultiMatchHandler multiMatchHandler,
            bool hasVariables,
            bool hasTableAccess,
            StatementMetricHandle metricsHandle,
            InsertIntoLatchFactory insertIntoFrontLatchFactory,
            InsertIntoLatchFactory insertIntoBackLatchFactory)
        {
            this.statementName = statementName;
            this.deploymentId = deploymentId;
            this.statementId = statementId;
            this.optionalStatementEPL = optionalStatementEPL;
            this.priority = priority;
            this.preemptive = preemptive;
            this.canSelfJoin = canSelfJoin;
            this.multiMatchHandler = multiMatchHandler;
            this.hasVariables = hasVariables;
            this.hasTableAccess = hasTableAccess;
            this.metricsHandle = metricsHandle;
            this.insertIntoFrontLatchFactory = insertIntoFrontLatchFactory;
            this.insertIntoBackLatchFactory = insertIntoBackLatchFactory;
        }

        /// <summary>
        /// Returns the statement id.
        /// </summary>
        /// <returns>statement id</returns>
        public int StatementId => statementId;

        /// <summary>
        /// Returns true if the statement uses variables, false if not.
        /// </summary>
        /// <returns>indicator if variables are used by statement</returns>
        public bool HasVariables => hasVariables;

        /// <summary>
        /// Returns the statement priority.
        /// </summary>
        /// <returns>priority, default 0</returns>
        public int Priority => priority;

        /// <summary>
        /// True for preemptive (drop) statements.
        /// </summary>
        /// <returns>preemptive indicator</returns>
        public bool IsPreemptive => preemptive;

        /// <summary>
        /// Returns true if the statement potentially self-joins amojng the events it processes.
        /// </summary>
        /// <returns>true for self-joins possible, false for not possible (most statements)</returns>
        public bool IsCanSelfJoin => canSelfJoin;

        /// <summary>
        /// Returns handle for metrics reporting.
        /// </summary>
        /// <returns>handle for metrics reporting</returns>
        public StatementMetricHandle MetricsHandle => metricsHandle;

        public bool HasTableAccess => hasTableAccess;

        public MultiMatchHandler MultiMatchHandler => multiMatchHandler;

        public string StatementName => statementName;

        public string DeploymentId => deploymentId;

        public string OptionalStatementEPL => optionalStatementEPL;

        public InsertIntoLatchFactory InsertIntoFrontLatchFactory => insertIntoFrontLatchFactory;

        public InsertIntoLatchFactory InsertIntoBackLatchFactory => insertIntoBackLatchFactory;

        protected bool Equals(EPStatementHandle other)
        {
            return statementId == other.statementId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((EPStatementHandle)obj);
        }

        public override int GetHashCode()
        {
            return statementId;
        }
    }
} // end of namespace