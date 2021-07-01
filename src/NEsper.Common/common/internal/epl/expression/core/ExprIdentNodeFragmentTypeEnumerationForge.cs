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
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
	public class ExprIdentNodeFragmentTypeEnumerationForge : ExprEnumerationForge {
	    private readonly string _propertyName;
	    private readonly int _streamId;
	    private readonly EventType _fragmentEventType;
	    private readonly EventPropertyGetterSPI _getterSpi;

	    public ExprIdentNodeFragmentTypeEnumerationForge(string propertyName, int streamId, EventType fragmentEventType, EventPropertyGetterSPI getterSpi) {
	        _propertyName = propertyName;
	        _streamId = streamId;
	        _fragmentEventType = fragmentEventType;
	        _getterSpi = getterSpi;
	    }

	    public Type ComponentTypeCollection {
		    get { return null; }
	    }

	    public EventType GetEventTypeCollection(StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices) {
	        return null;
	    }

	    public EventType GetEventTypeSingle(StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices) {
	        return _fragmentEventType;
	    }

	    public CodegenExpression EvaluateGetROCollectionEventsCodegen(CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression EvaluateGetROCollectionScalarCodegen(CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression EvaluateGetEventBeanCodegen(CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        var method = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
	        method.Block
	            .DeclareVar<EventBean>("@event", ArrayAtIndex(exprSymbol.GetAddEPS(method), Constant(_streamId)))
	            .IfRefNullReturnNull("@event")
	            .MethodReturn(Cast(typeof(EventBean), _getterSpi.EventBeanFragmentCodegen(Ref("@event"), method, codegenClassScope)));
	        return LocalMethod(method);
	    }

	    public ExprNodeRenderable EnumForgeRenderable {
		    get {
			    return new ProxyExprNodeRenderable() {
				    ProcToEPL = (
					    writer,
					    parentPrecedence,
					    flags) => {
					    writer.Write(_propertyName);
				    },
			    };
		    }
	    }

	    public ExprEnumerationEval ExprEvaluatorEnumeration {
		    get {
			    return new ProxyExprEnumerationEval() {
				    ProcEvaluateGetROCollectionEvents = (
					    eventsPerStream,
					    isNewData,
					    context) => {
					    return null;
				    },

				    ProcEvaluateGetROCollectionScalar = (
					    eventsPerStream,
					    isNewData,
					    context) => {
					    return null;
				    },

				    ProcEvaluateGetEventBean = (
					    eventsPerStream,
					    isNewData,
					    context) => {
					    EventBean @event = eventsPerStream[_streamId];
					    if (@event == null) {
						    return null;
					    }

					    return (EventBean) _getterSpi.GetFragment(@event);
				    },
			    };
		    }
	    }
	}
} // end of namespace
