///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using java.util.function;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // exprDotMethodChain;
using static com.espertech.esper.common.@internal.@event.path.EventTypeResolver; // GETEVENTSERDEFACTORY;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
	public class DataInputOutputSerdeForgeEventSerde : DataInputOutputSerdeForge {
	    private readonly string methodName;
	    private readonly Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[] functions;

	    public DataInputOutputSerdeForgeEventSerde(string methodName, Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>... functions) {
	        this.methodName = methodName;
	        this.functions = functions;
	    }

	    public string ForgeClassName() {
	        return typeof(DataInputOutputSerde).Name;
	    }

	    public CodegenExpression Codegen(CodegenMethod method, CodegenClassScope classScope, CodegenExpression optionalEventTypeResolver) {
	        CodegenExpression[] @params = new CodegenExpression[functions.Length];
	        DataInputOutputSerdeForgeParameterizedVars vars = new DataInputOutputSerdeForgeParameterizedVars(method, classScope, optionalEventTypeResolver);
	        for (int i = 0; i < @params.Length; i++) {
	            @params[i] = functions[i].Apply(vars);
	        }
	        return ExprDotMethodChain(optionalEventTypeResolver)
		        .Add(GETEVENTSERDEFACTORY)
		        .Add(methodName, @params);
	    }
	}
} // end of namespace
