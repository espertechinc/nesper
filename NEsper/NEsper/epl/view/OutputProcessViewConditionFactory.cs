///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.view
{
	/// <summary>
	/// A view that handles the "output snapshot" keyword in output rate stabilizing.
	/// </summary>
	public class OutputProcessViewConditionFactory : OutputProcessViewDirectDistinctOrAfterFactory
	{
	    private readonly ConditionType _conditionType;
	    private readonly ResultSetProcessorHelperFactory _resultSetProcessorHelperFactory;

	    public OutputProcessViewConditionFactory(StatementContext statementContext, OutputStrategyPostProcessFactory postProcessFactory, bool distinct, ExprTimePeriod afterTimePeriod, int? afterConditionNumberOfEvents, EventType resultEventType, OutputConditionFactory outputConditionFactory, int streamCount, ConditionType conditionType, OutputLimitLimitType outputLimitLimitType, bool terminable, bool hasAfter, bool isUnaggregatedUngrouped, SelectClauseStreamSelectorEnum selectClauseStreamSelectorEnum, ResultSetProcessorHelperFactory resultSetProcessorHelperFactory)
	        : base(statementContext, postProcessFactory, resultSetProcessorHelperFactory, distinct, afterTimePeriod, afterConditionNumberOfEvents, resultEventType)
        {
	        OutputConditionFactory = outputConditionFactory;
	        StreamCount = streamCount;
	        _conditionType = conditionType;
	        OutputLimitLimitType = outputLimitLimitType;
	        IsTerminable = terminable;
	        HasAfter = hasAfter;
	        IsUnaggregatedUngrouped = isUnaggregatedUngrouped;
	        SelectClauseStreamSelectorEnum = selectClauseStreamSelectorEnum;
	        _resultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
	    }

	    public override OutputProcessViewBase MakeView(ResultSetProcessor resultSetProcessor, AgentInstanceContext agentInstanceContext)
        {
	        // determine after-stuff
	        bool isAfterConditionSatisfied = true;
	        long? afterConditionTime = null;
	        var afterConditionNumberOfEvents = AfterConditionNumberOfEvents;
	        if (afterConditionNumberOfEvents != null)
	        {
	            isAfterConditionSatisfied = false;
	        }
	        else if (AfterTimePeriod != null)
	        {
	            isAfterConditionSatisfied = false;
	            long delta = AfterTimePeriod.NonconstEvaluator().DeltaMillisecondsUseEngineTime(null, agentInstanceContext);
	            afterConditionTime = agentInstanceContext.StatementContext.TimeProvider.Time + delta;
	        }

	        if (_conditionType == ConditionType.SNAPSHOT)
            {
                if (base.PostProcessFactory == null)
                {
	                return new OutputProcessViewConditionSnapshot(_resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext);
	            }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
	            return new OutputProcessViewConditionSnapshotPostProcess(_resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext, postProcess);
	        }
	        else if (_conditionType == ConditionType.POLICY_FIRST)
            {
                if (base.PostProcessFactory == null)
                {
	                return new OutputProcessViewConditionFirst(_resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext);
	            }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
	            return new OutputProcessViewConditionFirstPostProcess(_resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext, postProcess);
	        }
	        else if (_conditionType == ConditionType.POLICY_LASTALL_UNORDERED)
            {
                if (base.PostProcessFactory == null)
                {
	                return new OutputProcessViewConditionLastAllUnord(_resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext);
	            }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
	            return new OutputProcessViewConditionLastAllUnordPostProcessAll(_resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext, postProcess);
	        }
	        else
            {
                if (base.PostProcessFactory == null)
                {
	                return new OutputProcessViewConditionDefault(_resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext, StreamCount > 1);
	            }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
	            return new OutputProcessViewConditionDefaultPostProcess(resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext, postProcess, StreamCount > 1, _resultSetProcessorHelperFactory);
	        }
	    }

	    public OutputConditionFactory OutputConditionFactory { get; private set; }

	    public int StreamCount { get; private set; }

	    public OutputLimitLimitType OutputLimitLimitType { get; private set; }

	    public bool IsTerminable { get; private set; }

	    public bool HasAfter { get; private set; }

	    public bool IsUnaggregatedUngrouped { get; private set; }

	    public SelectClauseStreamSelectorEnum SelectClauseStreamSelectorEnum { get; private set; }

	    public enum ConditionType
        {
	        SNAPSHOT,
	        POLICY_FIRST,
	        POLICY_LASTALL_UNORDERED,
	        POLICY_NONFIRST
	    }
	}
} // end of namespace
