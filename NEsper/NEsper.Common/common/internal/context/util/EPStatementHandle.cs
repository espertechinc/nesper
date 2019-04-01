///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.statement.insertintolatch;
using com.espertech.esper.common.@internal.statement.multimatch;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    ///     Class exists once per statement and hold statement resource lock(s).
    ///     <para />
    ///     Use by <seealso cref="EPRuntimeImpl" /> for determining callback-statement affinity and locking of statement
    ///     resources.
    /// </summary>
    [Serializable]
    public class EPStatementHandle
    {
        private readonly int _hashCode;
        [NonSerialized] private readonly StatementMetricHandle _metricsHandle;
        [NonSerialized] private InsertIntoLatchFactory _insertIntoBackLatchFactory;

        [NonSerialized] private InsertIntoLatchFactory _insertIntoFrontLatchFactory;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="statementId">is the statement id uniquely indentifying the handle</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="statementText">The statement text.</param>
        /// <param name="statementType">Type of the statement.</param>
        /// <param name="expressionText">is the expression</param>
        /// <param name="hasVariables">indicator whether the statement uses variables</param>
        /// <param name="metricsHandle">handle for metrics reporting</param>
        /// <param name="priority">priority, zero is default</param>
        /// <param name="preemptive">true for drop after done</param>
        /// <param name="hasTableAccess">if set to <c>true</c> [has table access].</param>
        /// <param name="multiMatchHandler">The multi match handler.</param>
        public EPStatementHandle(
            int statementId, string statementName, string statementText, StatementType statementType,
            string expressionText, bool hasVariables, StatementMetricHandle metricsHandle, int priority,
            bool preemptive, bool hasTableAccess, MultiMatchHandler multiMatchHandler)
        {
            StatementId = statementId;
            StatementName = statementName;
            EPL = statementText;
            StatementType = statementType;
            HasVariables = hasVariables;
            Priority = priority;
            IsPreemptive = preemptive;
            _metricsHandle = metricsHandle;
            HasTableAccess = hasTableAccess;
            MultiMatchHandler = multiMatchHandler;

            unchecked {
                _hashCode = statementName != null ? statementName.GetHashCode() : 0;
                _hashCode = (_hashCode * 397) ^ statementId.GetHashCode();
                _hashCode = (_hashCode * 397) ^ (expressionText != null ? expressionText.GetHashCode() : 0);
            }
        }

        /// <summary>Returns the statement id. </summary>
        /// <value>statement id</value>
        public int StatementId { get; }

        /// <summary>Returns the factory for latches in insert-into guaranteed order of delivery. </summary>
        /// <value>latch factory for the statement if it performs insert-into (route) of events</value>
        public InsertIntoLatchFactory InsertIntoFrontLatchFactory {
            get => _insertIntoFrontLatchFactory;
            set => _insertIntoFrontLatchFactory = value;
        }

        public InsertIntoLatchFactory InsertIntoBackLatchFactory {
            get => _insertIntoBackLatchFactory;
            set => _insertIntoBackLatchFactory = value;
        }

        /// <summary>Returns true if the statement uses variables, false if not. </summary>
        /// <value>indicator if variables are used by statement</value>
        public bool HasVariables { get; }

        /// <summary>Returns the statement priority. </summary>
        /// <value>priority, default 0</value>
        public int Priority { get; }

        /// <summary>True for preemptive (drop) statements. </summary>
        /// <value>preemptive indicator</value>
        public bool IsPreemptive { get; }

        /// <summary>Returns true if the statement potentially self-joins amojng the events it processes. </summary>
        /// <value>true for self-joins possible, false for not possible (most statements)</value>
        public bool IsCanSelfJoin { get; set; }

        /// <summary>Returns handle for metrics reporting. </summary>
        /// <value>handle for metrics reporting</value>
        public StatementMetricHandle MetricsHandle => _metricsHandle;

        public string StatementName { get; }

        public string EPL { get; }

        public StatementType StatementType { get; }

        public bool HasTableAccess { get; }

        public MultiMatchHandler MultiMatchHandler { get; set; }

        public bool Equals(EPStatementHandle other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.StatementId == StatementId;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />;
        ///     otherwise, false.
        /// </returns>
        /// <param name="obj">
        ///     The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.
        /// </param>
        /// <exception cref="T:System.NullReferenceException">
        ///     The <paramref name="obj" /> parameter is null.
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != typeof(EPStatementHandle)) {
                return false;
            }

            return Equals((EPStatementHandle) obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}