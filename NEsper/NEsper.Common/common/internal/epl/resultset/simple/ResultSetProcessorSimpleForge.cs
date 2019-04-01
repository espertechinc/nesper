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
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.simple
{
	/// <summary>
	/// Result set processor prototype for the simplest case: no aggregation functions used in the select clause, and no group-by.
	/// </summary>
	public class ResultSetProcessorSimpleForge : ResultSetProcessorFactoryForge {
	    private readonly EventType resultEventType;
	    private readonly bool isSelectRStream;
	    private readonly SelectExprProcessorForge selectExprProcessorForge;
	    private readonly ExprForge optionalHavingNode;
	    private readonly OutputLimitSpec outputLimitSpec;
	    private readonly ResultSetProcessorOutputConditionType outputConditionType;
	    private readonly bool isSorting;
	    private readonly EventType[] eventTypes;

	    public ResultSetProcessorSimpleForge(EventType resultEventType,
	                                         SelectExprProcessorForge selectExprProcessorForge,
	                                         ExprForge optionalHavingNode,
	                                         bool isSelectRStream,
	                                         OutputLimitSpec outputLimitSpec,
	                                         ResultSetProcessorOutputConditionType outputConditionType,
	                                         bool isSorting,
	                                         EventType[] eventTypes) {
	        this.resultEventType = resultEventType;
	        this.selectExprProcessorForge = selectExprProcessorForge;
	        this.optionalHavingNode = optionalHavingNode;
	        this.isSelectRStream = isSelectRStream;
	        this.outputLimitSpec = outputLimitSpec;
	        this.outputConditionType = outputConditionType;
	        this.isSorting = isSorting;
	        this.eventTypes = eventTypes;
	    }

	    public EventType ResultEventType {
	        get => resultEventType;
	    }

	    public bool IsSelectRStream() {
	        return isSelectRStream;
	    }

	    public ExprForge OptionalHavingNode {
	        get => optionalHavingNode;
	    }

	    public bool IsOutputLast() {
	        return outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;
	    }

	    public bool IsOutputAll() {
	        return outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL;
	    }

	    public ResultSetProcessorOutputConditionType OutputConditionType {
	        get => outputConditionType;
	    }

	    public int NumStreams {
	        get => eventTypes.Length;
	    }

	    public EventType[] GetEventTypes() {
	        return eventTypes;
	    }

	    public bool IsSorting() {
	        return isSorting;
	    }

	    public Type InterfaceClass {
	        get => typeof(ResultSetProcessorSimple);
	    }

	    public void InstanceCodegen(CodegenInstanceAux instance, CodegenClassScope classScope, CodegenCtor factoryCtor, IList<CodegenTypedParam> factoryMembers) {
	        instance.Methods.AddMethod(typeof(bool), "hasHavingClause", Collections.GetEmptyList<object>(), typeof(ResultSetProcessorSimple), classScope, methodNode -> methodNode.Block.MethodReturn(Constant(optionalHavingNode != null)));
	        ResultSetProcessorUtil.EvaluateHavingClauseCodegen(optionalHavingNode, classScope, instance);
	        instance.Methods.AddMethod(typeof(ExprEvaluatorContext), "getAgentInstanceContext", Collections.GetEmptyList<object>(), this.GetType(), classScope, methodNode -> methodNode.Block.MethodReturn(REF_AGENTINSTANCECONTEXT));
	    }

	    public void ProcessViewResultCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.ProcessViewResultCodegen(this, classScope, method, instance);
	    }

	    public void ProcessJoinResultCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
	    }

	    public void GetIteratorViewCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.GetIteratorViewCodegen(this, classScope, method, instance);
	    }

	    public void GetIteratorJoinCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.GetIteratorJoinCodegen(this, classScope, method, instance);
	    }

	    public void ProcessOutputLimitedViewCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.ProcessOutputLimitedViewCodegen(this, method);
	    }

	    public void ProcessOutputLimitedJoinCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.ProcessOutputLimitedJoinCodegen(this, method);
	    }

	    public void ApplyViewResultCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        // no action
	    }

	    public void ApplyJoinResultCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        // no action
	    }

	    public void ContinueOutputLimitedLastAllNonBufferedViewCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, classScope, method, instance);
	    }

	    public void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, classScope, method, instance);
	    }

	    public void ProcessOutputLimitedLastAllNonBufferedViewCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(this, classScope, method, instance);
	    }

	    public void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(this, classScope, method, instance);
	    }

	    public void AcceptHelperVisitorCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.AcceptHelperVisitorCodegen(method, instance);
	    }

	    public void StopMethodCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorSimpleImpl.StopMethodCodegen(method, instance);
	    }

	    public void ClearMethodCodegen(CodegenClassScope classScope, CodegenMethod method) {
	        // no clearing aggregations
	    }

	    public string InstrumentedQName {
	        get => "ResultSetProcessSimple";
	    }
	}
} // end of namespace