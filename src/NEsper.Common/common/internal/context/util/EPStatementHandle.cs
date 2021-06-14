///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    [Serializable]
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
        public int StatementId {
            get => statementId;
        }

        /// <summary>
        /// Returns true if the statement uses variables, false if not.
        /// </summary>
        /// <returns>indicator if variables are used by statement</returns>
        public bool HasVariables {
            get => hasVariables;
        }

        /// <summary>
        /// Returns the statement priority.
        /// </summary>
        /// <returns>priority, default 0</returns>
        public int Priority {
            get => priority;
        }

        /// <summary>
        /// True for preemptive (drop) statements.
        /// </summary>
        /// <returns>preemptive indicator</returns>
        public bool IsPreemptive {
            get => preemptive;
        }

        /// <summary>
        /// Returns true if the statement potentially self-joins amojng the events it processes.
        /// </summary>
        /// <returns>true for self-joins possible, false for not possible (most statements)</returns>
        public bool IsCanSelfJoin {
            get => canSelfJoin;
        }

        /// <summary>
        /// Returns handle for metrics reporting.
        /// </summary>
        /// <returns>handle for metrics reporting</returns>
        public StatementMetricHandle MetricsHandle {
            get => metricsHandle;
        }

        public bool HasTableAccess {
            get => hasTableAccess;
        }

        public MultiMatchHandler MultiMatchHandler {
            get => multiMatchHandler;
        }

        public string StatementName {
            get => statementName;
        }

        public string DeploymentId {
            get => deploymentId;
        }

        public string OptionalStatementEPL {
            get => optionalStatementEPL;
        }

        public InsertIntoLatchFactory InsertIntoFrontLatchFactory {
            get => insertIntoFrontLatchFactory;
        }

        public InsertIntoLatchFactory InsertIntoBackLatchFactory {
            get => insertIntoBackLatchFactory;
        }

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

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((EPStatementHandle) obj);
        }

        public override int GetHashCode()
        {
            return statementId;
        }
    }
} // end of namespace