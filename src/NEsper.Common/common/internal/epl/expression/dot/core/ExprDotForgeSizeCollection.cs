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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
	public class ExprDotForgeSizeCollection : ExprDotForge, ExprDotEval
	{
	    public object Evaluate(object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
		    return target?.AsObjectList(MagicMarker.SingletonInstance).Count;
	    }

	    public EPType TypeInfo => EPTypeHelper.SingleValue(typeof(int?));

	    public void Visit(ExprDotEvalVisitor visitor) {
	        visitor.VisitArrayLength();
	    }

	    public ExprDotEval DotEvaluator => this;

	    public ExprDotForge DotForge => this;

	    public CodegenExpression Codegen(CodegenExpression inner, Type innerType, CodegenMethodScope parent, ExprForgeCodegenSymbol symbols, CodegenClassScope classScope) {
	        var method = parent.MakeChild(typeof(int?), typeof(ExprDotForgeSizeCollection), classScope).AddParam(innerType, "target").Block
	            .IfRefNullReturnNull("target")
	            .MethodReturn(ExprDotName(Ref("target"), "Count"));
	        return LocalMethodBuild(method).Pass(inner).Call();
	    }
	}
} // end of namespace
