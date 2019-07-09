///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client.hook.condition;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.pool
{
    public class PatternSubexpressionPoolRuntimeSvcImpl : PatternSubexpressionPoolRuntimeSvc
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISet<StatementEntry> patternContexts;
        private readonly AtomicLong poolCount;
        private readonly bool preventStart;
        private readonly long maxPoolCountConfigured;

        public PatternSubexpressionPoolRuntimeSvcImpl(
            long maxPoolCountConfigured,
            bool preventStart)
        {
            this.maxPoolCountConfigured = maxPoolCountConfigured;
            this.preventStart = preventStart;
            poolCount = new AtomicLong();
            patternContexts = CompatExtensions.AsSyncSet(new HashSet<StatementEntry>());
        }

        public void AddPatternContext(
            int statementId,
            string statementName,
            PatternSubexpressionPoolStmtHandler stmtCounts)
        {
            patternContexts.Add(new StatementEntry(statementId, statementName, stmtCounts));
        }

        public void RemoveStatement(int statementId)
        {
            ISet<StatementEntry> removed = new HashSet<StatementEntry>();
            foreach (var context in patternContexts) {
                if (context.StatementId == statementId) {
                    removed.Add(context);
                }
            }

            patternContexts.RemoveAll(removed);
        }

        public bool TryIncreaseCount(
            EvalNode evalNode,
            AgentInstanceContext agentInstanceContext)
        {
            // test pool max
            var newMax = poolCount.IncrementAndGet();
            if (newMax > maxPoolCountConfigured && maxPoolCountConfigured >= 0) {
                var counts = Counts;
                agentInstanceContext.StatementContext.ExceptionHandlingService.HandleCondition(
                    new ConditionPatternRuntimeSubexpressionMax(maxPoolCountConfigured, counts),
                    agentInstanceContext.StatementContext);
                if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled &&
                    ExecutionPathDebugLog.IsTimerDebugEnabled) {
                    var stmtHandler = agentInstanceContext.StatementContext.PatternSubexpressionPoolSvc.StmtHandler;
                    var stmtName = agentInstanceContext.StatementContext.StatementName;
                    Log.Debug(
                        ".tryIncreaseCount For statement '" + stmtName + "' pool count overflow at " + newMax +
                        " statement count was " + stmtHandler.Count + " preventStart=" + preventStart);
                }

                if (preventStart) {
                    poolCount.DecrementAndGet();
                    return false;
                }

                return true;
            }

            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                var stmtHandler = agentInstanceContext.StatementContext.PatternSubexpressionPoolSvc.StmtHandler;
                var stmtName = agentInstanceContext.StatementContext.StatementName;
                Log.Debug(
                    ".tryIncreaseCount For statement '" + stmtName + "' pool count increases to " + newMax +
                    " statement count was " + stmtHandler.Count);
            }

            return true;
        }

        // Relevant for recovery of state
        public void ForceIncreaseCount(
            EvalNode evalNode,
            AgentInstanceContext agentInstanceContext)
        {
            var newMax = poolCount.IncrementAndGet();
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                var stmtHandler = agentInstanceContext.StatementContext.PatternSubexpressionPoolSvc.StmtHandler;
                var stmtName = agentInstanceContext.StatementContext.StatementName;
                Log.Debug(
                    ".forceIncreaseCount For statement '" + stmtName + "' pool count increases to " + newMax +
                    " statement count was " + stmtHandler.Count);
            }
        }

        public void DecreaseCount(
            EvalNode evalNode,
            AgentInstanceContext agentInstanceContext)
        {
            var newMax = poolCount.DecrementAndGet();
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                var stmtHandler = agentInstanceContext.StatementContext.PatternSubexpressionPoolSvc.StmtHandler;
                var stmtName = agentInstanceContext.StatementContext.StatementName;
                Log.Debug(
                    ".decreaseCount For statement '" + stmtName + "' pool count decreases to " + newMax +
                    " statement count was " + stmtHandler.Count);
            }
        }

        private IDictionary<string, long> Counts {
            get {
                IDictionary<string, long> counts = new Dictionary<string, long>();
                foreach (var context in patternContexts) {
                    if (!counts.TryGetValue(context.StatementName, out var count))
                    { 
                        count = 0L;
                    }

                    count += context.StmtCounts.Count;
                    counts.Put(context.StatementName, count);
                }

                return counts;
            }
        }

        public class StatementEntry
        {
            public StatementEntry(
                int statementId,
                string statementName,
                PatternSubexpressionPoolStmtHandler stmtCounts)
            {
                StatementId = statementId;
                StatementName = statementName;
                StmtCounts = stmtCounts;
            }

            public int StatementId { get; }

            public string StatementName { get; }

            public PatternSubexpressionPoolStmtHandler StmtCounts { get; }
        }
    }
} // end of namespace