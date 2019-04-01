///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.createschema
{
	public class StatementAgentInstanceFactoryCreateSchemaForge {

	    private readonly EventType eventType;

	    public StatementAgentInstanceFactoryCreateSchemaForge(EventType eventType) {
	        this.eventType = eventType;
	    }

	    public CodegenMethod InitializeCodegen(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateSchema), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(StatementAgentInstanceFactoryCreateSchema), "saiff", NewInstance(typeof(StatementAgentInstanceFactoryCreateSchema)))
	                .ExprDotMethod(@Ref("saiff"), "setEventType", EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)))
	                .MethodReturn(@Ref("saiff"));
	        return method;
	    }
	}
} // end of namespace