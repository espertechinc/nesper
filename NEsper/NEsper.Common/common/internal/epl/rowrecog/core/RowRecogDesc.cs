///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    public class RowRecogDesc
    {
        public EventType ParentEventType { get; set; }

        public EventType MultimatchEventType { get; set; }

        public EventType RowEventType { get; set; }

        public EventType CompositeEventType { get; set; }

        public Type[] PartitionEvalTypes { get; set; }

        public int[] MultimatchStreamNumToVariable { get; set; }

        public ExprEvaluator PartitionEvalMayNull { get; set; }

        public LinkedHashMap<string, Pair<int, bool>> VariableStreams { get; set; }

        public bool HasInterval { get; set; }

        public bool IsIterateOnly { get; set; }

        public bool IsUnbound { get; set; }

        public bool IsOrTerminated { get; set; }

        public bool IsCollectMultimatches { get; set; }

        public bool IsDefineAsksMultimatches { get; set; }

        public int NumEventsEventsPerStreamDefine { get; set; }

        public string[] MultimatchVariablesArray { get; set; }

        public RowRecogNFAStateBase[] StatesOrdered { get; set; }

        public IList<Pair<int, int[]>> NextStatesPerState { get; set; }

        public int[] StartStates { get; set; }

        public bool IsAllMatches { get; set; }

        public MatchRecognizeSkipEnum Skip { get; set; }

        public ExprEvaluator[] ColumnEvaluators { get; set; }

        public string[] ColumnNames { get; set; }

        public int[] MultimatchVariableToStreamNum { get; set; }

        public TimePeriodCompute IntervalCompute { get; set; }

        public int[] PreviousRandomAccessIndexes { get; set; }

        public AggregationServiceFactory[] AggregationServiceFactories { get; set; }

        public AggregationResultFutureAssignable[] AggregationResultFutureAssignables { get; set; }
    }
} // end of namespace