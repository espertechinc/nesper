///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.view
{
	/// <summary>
	/// A view that handles the "output snapshot" keyword in output rate stabilizing.
	/// </summary>
	public class OutputProcessViewConditionSpec {
	    private EventType resultEventType;
	    private OutputStrategyPostProcessFactory postProcessFactory;
	    private OutputConditionFactory outputConditionFactory;
	    private int streamCount;
	    private ResultSetProcessorOutputConditionType conditionType;
	    private bool terminable;
	    private bool isUnaggregatedUngrouped;
	    private SelectClauseStreamSelectorEnum selectClauseStreamSelector;
	    private bool isDistinct;
	    private bool hasAfter;
	    private TimePeriodCompute afterTimePeriod;
	    private int? afterConditionNumberOfEvents;
	    private EventType[] eventTypes;

	    public OutputConditionFactory OutputConditionFactory {
	        get => outputConditionFactory;
	    }

	    public void SetOutputConditionFactory(OutputConditionFactory outputConditionFactory) {
	        this.outputConditionFactory = outputConditionFactory;
	    }

	    public int StreamCount {
	        get => streamCount;
	    }

	    public void SetStreamCount(int streamCount) {
	        this.streamCount = streamCount;
	    }

	    public ResultSetProcessorOutputConditionType ConditionType {
	        get => conditionType;
	    }

	    public void SetConditionType(ResultSetProcessorOutputConditionType conditionType) {
	        this.conditionType = conditionType;
	    }

	    public bool IsTerminable() {
	        return terminable;
	    }

	    public void SetTerminable(bool terminable) {
	        this.terminable = terminable;
	    }

	    public bool HasAfter() {
	        return hasAfter;
	    }

	    public void SetHasAfter(bool hasAfter) {
	        this.hasAfter = hasAfter;
	    }

	    public bool IsUnaggregatedUngrouped() {
	        return isUnaggregatedUngrouped;
	    }

	    public void SetUnaggregatedUngrouped(bool unaggregatedUngrouped) {
	        isUnaggregatedUngrouped = unaggregatedUngrouped;
	    }

	    public SelectClauseStreamSelectorEnum SelectClauseStreamSelector {
	        get => selectClauseStreamSelector;
	    }

	    public void SetSelectClauseStreamSelector(SelectClauseStreamSelectorEnum selectClauseStreamSelector) {
	        this.selectClauseStreamSelector = selectClauseStreamSelector;
	    }

	    public bool IsDistinct() {
	        return isDistinct;
	    }

	    public void SetDistinct(bool distinct) {
	        isDistinct = distinct;
	    }

	    public TimePeriodCompute AfterTimePeriod {
	        get => afterTimePeriod;
	    }

	    public void SetAfterTimePeriod(TimePeriodCompute afterTimePeriod) {
	        this.afterTimePeriod = afterTimePeriod;
	    }

	    public int? GetAfterConditionNumberOfEvents() {
	        return afterConditionNumberOfEvents;
	    }

	    public void SetAfterConditionNumberOfEvents(int? afterConditionNumberOfEvents) {
	        this.afterConditionNumberOfEvents = afterConditionNumberOfEvents;
	    }

	    public OutputStrategyPostProcessFactory PostProcessFactory {
	        get => postProcessFactory;
	    }

	    public void SetPostProcessFactory(OutputStrategyPostProcessFactory postProcessFactory) {
	        this.postProcessFactory = postProcessFactory;
	    }

	    public EventType ResultEventType {
	        get => resultEventType;
	    }

	    public void SetResultEventType(EventType resultEventType) {
	        this.resultEventType = resultEventType;
	    }

	    public EventType[] GetEventTypes() {
	        return eventTypes;
	    }

	    public void SetEventTypes(EventType[] eventTypes) {
	        this.eventTypes = eventTypes;
	    }
	}
} // end of namespace