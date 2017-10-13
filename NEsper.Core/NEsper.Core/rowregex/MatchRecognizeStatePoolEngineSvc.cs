///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.util;

namespace com.espertech.esper.rowregex
{
    public class MatchRecognizeStatePoolEngineSvc
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private long _maxPoolCountConfigured;
        private readonly bool _preventStart;
        private readonly AtomicLong _poolCount;
        private readonly ISet<StatementEntry> _matchRecognizeContexts;

        public MatchRecognizeStatePoolEngineSvc(long maxPoolCountConfigured, bool preventStart)
        {
            _maxPoolCountConfigured = maxPoolCountConfigured;
            _preventStart = preventStart;
            _poolCount = new AtomicLong();
            _matchRecognizeContexts = new HashSet<StatementEntry>().AsSyncSet();
        }

        public long? MatchRecognizeMaxStates
        {
            get { return _maxPoolCountConfigured; }
            set
            {
                if (value == null)
                {
                    _maxPoolCountConfigured = -1;
                }
                else
                {
                    _maxPoolCountConfigured = value.GetValueOrDefault();
                }
            }
        }

        public void AddPatternContext(string statementName, MatchRecognizeStatePoolStmtHandler stmtCounts)
        {
            _matchRecognizeContexts.Add(new StatementEntry(statementName, stmtCounts));
        }

        public void RemoveStatement(string name)
        {
            // counts get reduced upon view stop
            var removed = new HashSet<StatementEntry>(_matchRecognizeContexts.Where(c => c.StatementName == name));
            _matchRecognizeContexts.RemoveAll(removed);
        }

        public bool TryIncreaseCount(AgentInstanceContext agentInstanceContext)
        {
            // test pool max
            long newMax = _poolCount.IncrementAndGet();
            if (newMax > _maxPoolCountConfigured && _maxPoolCountConfigured >= 0)
            {
                var counts = GetCounts();
                agentInstanceContext.StatementContext.ExceptionHandlingService.HandleCondition(new ConditionMatchRecognizeStatesMax(_maxPoolCountConfigured, counts), agentInstanceContext.StatementContext.EpStatementHandle);
                if (ExecutionPathDebugLog.IsEnabled && Log.IsDebugEnabled && ExecutionPathDebugLog.IsTimerDebugEnabled)
                {
                    MatchRecognizeStatePoolStmtHandler stmtHandler = agentInstanceContext.StatementContext.MatchRecognizeStatePoolStmtSvc.StmtHandler;
                    string stmtName = agentInstanceContext.StatementContext.StatementName;
                    Log.Debug(".tryIncreaseCount For statement '" + stmtName + "' pool count overflow at " + newMax + " statement count was " + stmtHandler.Count + " preventStart=" + _preventStart);
                }

                if (_preventStart)
                {
                    _poolCount.DecrementAndGet();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            if (ExecutionPathDebugLog.IsEnabled && Log.IsDebugEnabled)
            {
                MatchRecognizeStatePoolStmtHandler stmtHandler = agentInstanceContext.StatementContext.MatchRecognizeStatePoolStmtSvc.StmtHandler;
                string stmtName = agentInstanceContext.StatementContext.StatementName;
                Log.Debug(".tryIncreaseCount For statement '" + stmtName + "' pool count increases to " + newMax + " statement count was " + stmtHandler.Count);
            }
            return true;
        }

        public void DecreaseCount(AgentInstanceContext agentInstanceContext)
        {
            DecreaseCount(agentInstanceContext, 1);
        }

        public void DecreaseCount(AgentInstanceContext agentInstanceContext, int numRemoved)
        {
            long newMax = _poolCount.IncrementAndGet(-1 * numRemoved);
            if (newMax < 0)
            {
                _poolCount.Set(0);
            }
            LogDecrease(agentInstanceContext, newMax);
        }

        private void LogDecrease(AgentInstanceContext agentInstanceContext, long newMax)
        {
            if (ExecutionPathDebugLog.IsEnabled && Log.IsDebugEnabled)
            {
                MatchRecognizeStatePoolStmtHandler stmtHandler = agentInstanceContext.StatementContext.MatchRecognizeStatePoolStmtSvc.StmtHandler;
                string stmtName = agentInstanceContext.StatementContext.StatementName;
                Log.Debug(".decreaseCount For statement '" + stmtName + "' pool count decreases to " + newMax + " statement count was " + stmtHandler.Count);
            }
        }

        private IDictionary<string, long> GetCounts()
        {
            var counts = new Dictionary<string, long>();
            foreach (StatementEntry context in _matchRecognizeContexts)
            {
                long count;

                if (!counts.TryGetValue(context.StatementName, out count))
                {
                    count = 0L;
                }

                count += context.StmtCounts.Count;
                counts.Put(context.StatementName, count);
            }
            return counts;
        }

        public class StatementEntry
        {
            public StatementEntry(string statementName, MatchRecognizeStatePoolStmtHandler stmtCounts)
            {
                StatementName = statementName;
                StmtCounts = stmtCounts;
            }

            public string StatementName { get; private set; }

            public MatchRecognizeStatePoolStmtHandler StmtCounts { get; private set; }
        }
    }
} // end of namespace
