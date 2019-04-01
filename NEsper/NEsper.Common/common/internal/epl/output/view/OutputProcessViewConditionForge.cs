///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.view
{
	public class OutputProcessViewConditionForge : OutputProcessViewFactoryForge {
	    private readonly OutputStrategyPostProcessForge outputStrategyPostProcessForge;
	    private readonly bool isDistinct;
	    private readonly ExprTimePeriod afterTimePeriodExpr;
	    private readonly int? afterNumberOfEvents;
	    private readonly OutputConditionFactoryForge outputConditionFactoryForge;
	    private readonly int streamCount;
	    private readonly ResultSetProcessorOutputConditionType conditionType;
	    private readonly bool terminable;
	    private readonly bool hasAfter;
	    private readonly bool unaggregatedUngrouped;
	    private readonly SelectClauseStreamSelectorEnum selectClauseStreamSelector;
	    private readonly EventType[] eventTypes;
	    private readonly EventType resultEventType;

	    public OutputProcessViewConditionForge(OutputStrategyPostProcessForge outputStrategyPostProcessForge, bool isDistinct, ExprTimePeriod afterTimePeriodExpr, int? afterNumberOfEvents, OutputConditionFactoryForge outputConditionFactoryForge, int streamCount, ResultSetProcessorOutputConditionType conditionType, bool terminable, bool hasAfter, bool unaggregatedUngrouped, SelectClauseStreamSelectorEnum selectClauseStreamSelector, EventType[] eventTypes, EventType resultEventType) {
	        this.outputStrategyPostProcessForge = outputStrategyPostProcessForge;
	        this.isDistinct = isDistinct;
	        this.afterTimePeriodExpr = afterTimePeriodExpr;
	        this.afterNumberOfEvents = afterNumberOfEvents;
	        this.outputConditionFactoryForge = outputConditionFactoryForge;
	        this.streamCount = streamCount;
	        this.conditionType = conditionType;
	        this.terminable = terminable;
	        this.hasAfter = hasAfter;
	        this.unaggregatedUngrouped = unaggregatedUngrouped;
	        this.selectClauseStreamSelector = selectClauseStreamSelector;
	        this.eventTypes = eventTypes;
	        this.resultEventType = resultEventType;
	    }

	    public bool IsCodeGenerated {
	        get { return false; }
	    }

	    public void ProvideCodegen(CodegenMethod method, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        CodegenExpressionRef spec = @Ref("spec");
	        method.Block
	                .DeclareVar(typeof(OutputProcessViewConditionSpec), spec.Ref, NewInstance(typeof(OutputProcessViewConditionSpec)))
	                .ExprDotMethod(spec, "setConditionType", EnumValue(typeof(ResultSetProcessorOutputConditionType), conditionType.Name()))
	                .ExprDotMethod(spec, "setOutputConditionFactory", outputConditionFactoryForge.Make(method, symbols, classScope))
	                .ExprDotMethod(spec, "setStreamCount", Constant(streamCount))
	                .ExprDotMethod(spec, "setTerminable", Constant(terminable))
	                .ExprDotMethod(spec, "setSelectClauseStreamSelector", EnumValue(typeof(SelectClauseStreamSelectorEnum), selectClauseStreamSelector.Name()))
	                .ExprDotMethod(spec, "setPostProcessFactory", outputStrategyPostProcessForge == null ? ConstantNull() : outputStrategyPostProcessForge.Make(method, symbols, classScope))
	                .ExprDotMethod(spec, "setHasAfter", Constant(hasAfter))
	                .ExprDotMethod(spec, "setDistinct", Constant(isDistinct))
	                .ExprDotMethod(spec, "setResultEventType", EventTypeUtility.ResolveTypeCodegen(resultEventType, symbols.GetAddInitSvc(method)))
	                .ExprDotMethod(spec, "setAfterTimePeriod", afterTimePeriodExpr == null ? ConstantNull() : afterTimePeriodExpr.TimePeriodComputeForge.MakeEvaluator(method, classScope))
	                .ExprDotMethod(spec, "setAfterConditionNumberOfEvents", Constant(afterNumberOfEvents))
	                .ExprDotMethod(spec, "setUnaggregatedUngrouped", Constant(unaggregatedUngrouped))
	                .ExprDotMethod(spec, "setEventTypes", EventTypeUtility.ResolveTypeArrayCodegen(eventTypes, EPStatementInitServicesConstants.REF))
	                .MethodReturn(NewInstance(typeof(OutputProcessViewConditionFactory), spec));
	    }

	    public void UpdateCodegen(CodegenMethod method, CodegenClassScope classScope) {
	    }

	    public void ProcessCodegen(CodegenMethod method, CodegenClassScope classScope) {
	    }

	    public void IteratorCodegen(CodegenMethod method, CodegenClassScope classScope) {
	    }

	    public void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders) {
	        if (outputConditionFactoryForge != null) {
	            outputConditionFactoryForge.CollectSchedules(scheduleHandleCallbackProviders);
	        }
	    }
	}
} // end of namespace