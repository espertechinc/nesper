///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client.hook.condition;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    public class RowRecogStatePoolRuntimeSvc
    {
        private readonly ISet<StatementEntry> matchRecognizeContexts;
        private readonly AtomicLong poolCount;
        private readonly bool preventStart;
        private long maxPoolCountConfigured;

        public RowRecogStatePoolRuntimeSvc(
            long maxPoolCountConfigured,
            bool preventStart)
        {
            this.maxPoolCountConfigured = maxPoolCountConfigured;
            this.preventStart = preventStart;
            poolCount = new AtomicLong();
            matchRecognizeContexts = Collections.SynchronizedSet(new HashSet<StatementEntry>());
        }

        public void SetMatchRecognizeMaxStates(long? maxStates)
        {
            if (maxStates == null) {
                maxPoolCountConfigured = -1;
            }
            else {
                maxPoolCountConfigured = maxStates.Value;
            }
        }

        public void AddPatternContext(
            DeploymentIdNamePair statement,
            RowRecogStatePoolStmtHandler stmtCounts)
        {
            matchRecognizeContexts.Add(new StatementEntry(statement, stmtCounts));
        }

        public void RemoveStatement(DeploymentIdNamePair statement)
        {
            // counts get reduced upon view stop
            ISet<StatementEntry> removed = new HashSet<StatementEntry>();
            foreach (var context in matchRecognizeContexts) {
                if (context.Statement.Equals(statement)) {
                    removed.Add(context);
                }
            }

            matchRecognizeContexts.RemoveAll(removed);
        }

        public bool TryIncreaseCount(AgentInstanceContext agentInstanceContext)
        {
            // test pool max
            var newMax = poolCount.IncrementAndGet();
            if (newMax > maxPoolCountConfigured && maxPoolCountConfigured >= 0) {
                IDictionary<DeploymentIdNamePair, long> counts = GetCounts();
                agentInstanceContext.StatementContext.ExceptionHandlingService.HandleCondition(
                    new ConditionMatchRecognizeStatesMax(maxPoolCountConfigured, counts), 
                    agentInstanceContext.StatementContext);

                if (preventStart) {
                    poolCount.DecrementAndGet();
                    return false;
                }

                return true;
            }

            return true;
        }

        public void DecreaseCount(AgentInstanceContext agentInstanceContext)
        {
            DecreaseCount(agentInstanceContext, 1);
        }

        public void DecreaseCount(
            AgentInstanceContext agentInstanceContext,
            int numRemoved)
        {
            long newMax = poolCount.IncrementAndGet(-1 * numRemoved);
            if (newMax < 0) {
                poolCount.Set(0);
            }

            LogDecrease(agentInstanceContext, newMax);
        }

        private void LogDecrease(
            AgentInstanceContext agentInstanceContext,
            long newMax)
        {
        }

        private IDictionary<DeploymentIdNamePair, long> GetCounts()
        {
            IDictionary<DeploymentIdNamePair, long> counts = new Dictionary<DeploymentIdNamePair, long>();
            foreach (var context in matchRecognizeContexts) {
                if (!counts.TryGetValue(context.Statement, out var count)) {
                    count = 0L;
                }

                count += context.StmtCounts.Count;
                counts.Put(context.Statement, count);
            }

            return counts;
        }

        public class StatementEntry
        {
            public StatementEntry(
                DeploymentIdNamePair statement,
                RowRecogStatePoolStmtHandler stmtCounts)
            {
                Statement = statement;
                StmtCounts = stmtCounts;
            }

            public DeploymentIdNamePair Statement { get; }

            public RowRecogStatePoolStmtHandler StmtCounts { get; }
        }
    }
} // end of namespace