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
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.handthru
{
	/// <summary>
	/// Result set processor prototye for the hand-through case:
	/// no aggregation functions used in the select clause, and no group-by, no having and ordering.
	/// </summary>
	public class ResultSetProcessorHandThroughFactoryForge : ResultSetProcessorFactoryForge {
	    private readonly EventType resultEventType;
	    private readonly SelectExprProcessorForge selectExprProcessorForge;
	    private readonly bool isSelectRStream;

	    public ResultSetProcessorHandThroughFactoryForge(EventType resultEventType, SelectExprProcessorForge selectExprProcessorForge, bool selectRStream) {
	        this.resultEventType = resultEventType;
	        this.selectExprProcessorForge = selectExprProcessorForge;
	        this.isSelectRStream = selectRStream;
	    }

	    public EventType ResultEventType {
	        get => resultEventType;
	    }

	    public bool IsSelectRStream() {
	        return isSelectRStream;
	    }

	    public Type InterfaceClass {
	        get => typeof(ResultSetProcessor);
	    }

	    public void InstanceCodegen(CodegenInstanceAux instance, CodegenClassScope classScope, CodegenCtor factoryCtor, IList<CodegenTypedParam> factoryMembers) {
	    }

	    public void ProcessViewResultCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorHandThrough.ProcessViewResultCodegen(this, method);
	    }

	    public void ProcessJoinResultCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorHandThrough.ProcessJoinResultCodegen(this, method);
	    }

	    public void GetIteratorViewCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorHandThrough.GetIteratorViewCodegen(method);
	    }

	    public void GetIteratorJoinCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        ResultSetProcessorHandThrough.GetIteratorJoinCodegen(method);
	    }

	    public void ProcessOutputLimitedViewCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        method.Block.MethodReturn(ConstantNull());
	    }

	    public void ProcessOutputLimitedJoinCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        method.Block.MethodReturn(ConstantNull());
	    }

	    public void ApplyViewResultCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	    }

	    public void ApplyJoinResultCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	    }

	    public void ContinueOutputLimitedLastAllNonBufferedViewCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        method.Block.MethodReturn(ConstantNull());
	    }

	    public void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	        method.Block.MethodReturn(ConstantNull());
	    }

	    public void ProcessOutputLimitedLastAllNonBufferedViewCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	    }

	    public void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	    }

	    public void AcceptHelperVisitorCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	    }

	    public void StopMethodCodegen(CodegenClassScope classScope, CodegenMethod method, CodegenInstanceAux instance) {
	    }

	    public void ClearMethodCodegen(CodegenClassScope classScope, CodegenMethod method) {
	    }

	    public string InstrumentedQName {
	        get => "ResultSetProcessSimple";
	    }
	}
} // end of namespace