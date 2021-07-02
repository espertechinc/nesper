///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
	public class ExprStreamUnderlyingNodeEnumerationForge : ExprEnumerationForge {
	    private readonly string _streamName;
	    private readonly int _streamNum;
	    private readonly EventType _eventType;

	    public ExprStreamUnderlyingNodeEnumerationForge(string streamName, int streamNum, EventType eventType) {
	        this._streamName = streamName;
	        this._streamNum = streamNum;
	        this._eventType = eventType;
	    }

	    public Type ComponentTypeCollection => null;

	    public EventType GetEventTypeCollection(StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices) {
	        return null;
	    }

	    public EventType GetEventTypeSingle(StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices) {
	        return _eventType;
	    }

	    public CodegenExpression EvaluateGetROCollectionEventsCodegen(CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression EvaluateGetROCollectionScalarCodegen(CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression EvaluateGetEventBeanCodegen(CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return ArrayAtIndex(exprSymbol.GetAddEPS(codegenMethodScope), Constant(_streamNum));
	    }


	    public ExprNodeRenderable EnumForgeRenderable {
		    get {
			    return new ProxyExprNodeRenderable() {
				    procToEPL = (
					    writer,
					    parentPrecedence,
					    flags) => {
					    writer.Write(_streamName);
				    },
			    };
		    }
	    }

	    public ExprEnumerationEval ExprEvaluatorEnumeration {
		    get {
			    return new ProxyExprEnumerationEval() {
				    procEvaluateGetRoCollectionEvents = (
					    eventsPerStream,
					    isNewData,
					    context) => null,
				    procEvaluateGetRoCollectionScalar = (
					    eventsPerStream,
					    isNewData,
					    context) => null,
				    procEvaluateGetEventBean = (
					    eventsPerStream,
					    isNewData,
					    context) => eventsPerStream[_streamNum],
			    };
		    }
	    }
	}
} // end of namespace
