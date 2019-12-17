///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     A view that handles the "output snapshot" keyword in output rate stabilizing.
    /// </summary>
    public class OutputProcessViewConditionSpec
    {
        public OutputConditionFactory OutputConditionFactory { get; set; }

        public int StreamCount { get; set; }

        public ResultSetProcessorOutputConditionType ConditionType { get; set; }

        public bool IsTerminable { get; set; }

        public bool HasAfter { get; set; }

        public bool IsUnaggregatedUngrouped { get; set; }

        public SelectClauseStreamSelectorEnum SelectClauseStreamSelector { get; set; }

        public bool IsDistinct { get; set; }

        public TimePeriodCompute AfterTimePeriod { get; set; }

        public int? AfterConditionNumberOfEvents { get; set; }

        public OutputStrategyPostProcessFactory PostProcessFactory { get; set; }

        public EventType ResultEventType { get; set; }

        public EventType[] EventTypes { get; set; }
    }
} // end of namespace