///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    /// <summary>
    ///     Implements a poller viewable that uses a polling strategy, a cache and
    ///     some input parameters extracted from event streams to perform the polling.
    /// </summary>
    public abstract class HistoricalEventViewableFactoryBase : HistoricalEventViewableFactory
    {
        private static readonly EventBean[][] NULL_ROWS;

        protected internal ExprEvaluator evaluator;
        protected internal EventType eventType;
        protected internal bool hasRequiredStreams;
        protected internal int scheduleCallbackId;
        protected internal int streamNumber;

        static HistoricalEventViewableFactoryBase()
        {
            NULL_ROWS = new EventBean[1][];
            NULL_ROWS[0] = new EventBean[1];
        }

        public IThreadLocal<HistoricalDataCache> DataCacheThreadLocal { get; } =
            new SlimThreadLocal<HistoricalDataCache>(() => null);

        public abstract void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery);

        public abstract HistoricalEventViewable Activate(AgentInstanceContext agentInstanceContext);

        public bool HasRequiredStreams {
            get => hasRequiredStreams;
            set => hasRequiredStreams = value;
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
        }

        public int StreamNumber {
            get => streamNumber;
            set => streamNumber = value;
        }

        public ExprEvaluator Evaluator {
            get => evaluator;
            set => evaluator = value;
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }
    }
} // end of namespace