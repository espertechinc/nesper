///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern.pool
{
    public class PatternSubexpressionPoolEngineSvc
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICollection<StatementEntry> _patternContexts;
        private readonly AtomicLong _poolCount;
        private readonly bool _preventStart;
        private long _maxPoolCountConfigured;

        public PatternSubexpressionPoolEngineSvc(long maxPoolCountConfigured,
                                                 bool preventStart)
        {
            _maxPoolCountConfigured = maxPoolCountConfigured;
            _preventStart = preventStart;
            _poolCount = new AtomicLong();
            _patternContexts = new HashSet<StatementEntry>();
        }

        public long? PatternMaxSubexpressions
        {
            get { return _maxPoolCountConfigured; }
            set { _maxPoolCountConfigured = value.GetValueOrDefault(-1); }
        }

        public void AddPatternContext(String statementName,
                                      PatternSubexpressionPoolStmtHandler stmtCounts)
        {
            lock (_patternContexts)
            {
                _patternContexts.Add(new StatementEntry(statementName, stmtCounts));
            }
        }

        public void RemoveStatement(String name)
        {
            ICollection<StatementEntry> removed = new HashSet<StatementEntry>();
            lock (_patternContexts)
            {
                foreach (StatementEntry context in _patternContexts)
                {
                    if (context.StatementName.Equals(name))
                    {
                        removed.Add(context);
                    }
                }
                _patternContexts.RemoveAll(removed);
            }
        }

        public bool TryIncreaseCount(EvalFollowedByNode evalFollowedByNode)
        {
            // test pool max
            long newMax = _poolCount.IncrementAndGet();
            if (newMax > _maxPoolCountConfigured && _maxPoolCountConfigured >= 0)
            {
                var counts = Counts;
                evalFollowedByNode.Context.AgentInstanceContext.StatementContext.ExceptionHandlingService.
                    HandleCondition(new ConditionPatternEngineSubexpressionMax(_maxPoolCountConfigured, counts),
                                    evalFollowedByNode.Context.AgentInstanceContext.StatementContext.EpStatementHandle);
                if ((ExecutionPathDebugLog.IsEnabled) &&
                    (Log.IsDebugEnabled && (ExecutionPathDebugLog.IsTimerDebugEnabled)))
                {
                    PatternSubexpressionPoolStmtHandler stmtHandler =
                        evalFollowedByNode.Context.AgentInstanceContext.StatementContext.PatternSubexpressionPoolSvc
                            .StmtHandler;
                    String stmtName = evalFollowedByNode.Context.AgentInstanceContext.StatementContext.StatementName;
                    Log.Debug(".tryIncreaseCount For statement '" + stmtName + "' pool count overflow at " + newMax +
                              " statement count was " + stmtHandler.Count + " preventStart=" + _preventStart);
                }

                if (_preventStart)
                {
                    _poolCount.DecrementAndGet();
                    return false;
                }
                
                return true;
            }
            if ((ExecutionPathDebugLog.IsEnabled) && Log.IsDebugEnabled)
            {
                PatternSubexpressionPoolStmtHandler stmtHandler =
                    evalFollowedByNode.Context.AgentInstanceContext.StatementContext.PatternSubexpressionPoolSvc.
                        StmtHandler;
                String stmtName = evalFollowedByNode.Context.AgentInstanceContext.StatementContext.StatementName;
                Log.Debug(".tryIncreaseCount For statement '" + stmtName + "' pool count increases to " + newMax +
                          " statement count was " + stmtHandler.Count);
            }
            return true;
        }

        // Relevant for recovery of state
        public void ForceIncreaseCount(EvalFollowedByNode evalFollowedByNode)
        {
            long newMax = _poolCount.IncrementAndGet();
            if ((ExecutionPathDebugLog.IsEnabled) && Log.IsDebugEnabled)
            {
                PatternSubexpressionPoolStmtHandler stmtHandler =
                    evalFollowedByNode.Context.AgentInstanceContext.StatementContext.PatternSubexpressionPoolSvc.
                        StmtHandler;
                String stmtName = evalFollowedByNode.Context.AgentInstanceContext.StatementContext.StatementName;
                Log.Debug(".forceIncreaseCount For statement '" + stmtName + "' pool count increases to " + newMax +
                          " statement count was " + stmtHandler.Count);
            }
        }

        public void DecreaseCount(EvalFollowedByNode evalFollowedByNode)
        {
            long newMax = _poolCount.DecrementAndGet();
            if ((ExecutionPathDebugLog.IsEnabled) && Log.IsDebugEnabled)
            {
                PatternSubexpressionPoolStmtHandler stmtHandler =
                    evalFollowedByNode.Context.AgentInstanceContext.StatementContext.PatternSubexpressionPoolSvc.
                        StmtHandler;
                String stmtName = evalFollowedByNode.Context.AgentInstanceContext.StatementContext.StatementName;
                Log.Debug(".decreaseCount For statement '" + stmtName + "' pool count decreases to " + newMax +
                          " statement count was " + stmtHandler.Count);
            }
        }

        private IDictionary<string, long?> Counts
        {
            get
            {
                lock (_patternContexts)
                {
                    IDictionary<String, long?> counts = new Dictionary<String, long?>();
                    foreach (StatementEntry context in _patternContexts)
                    {
                        long? count = counts.Get(context.StatementName);
                        if (count == null)
                        {
                            count = 0L;
                        }
                        count += context.StmtCounts.Count;
                        counts.Put(context.StatementName, count);
                    }
                    return counts;
                }
            }
        }

        #region Nested type: StatementEntry

        public class StatementEntry
        {
            public StatementEntry(String statementName,
                                  PatternSubexpressionPoolStmtHandler stmtCounts)
            {
                StatementName = statementName;
                StmtCounts = stmtCounts;
            }

            public string StatementName { get; private set; }
            public PatternSubexpressionPoolStmtHandler StmtCounts { get; private set; }
        }

        #endregion
    }
}