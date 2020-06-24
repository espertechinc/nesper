///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
	public class VariableTriggerWriteArrayElementForge : VariableTriggerWriteForge {
	    private readonly string variableName;
	    private readonly ExprForge indexExpression;
	    private readonly TypeWidenerSPI widener;

	    public VariableTriggerWriteArrayElementForge(
		    string variableName,
		    ExprForge indexExpression,
		    TypeWidenerSPI widener)
	    {
		    this.variableName = variableName;
		    this.indexExpression = indexExpression;
		    this.widener = widener;
	    }

	    public override CodegenExpression Make(
		    CodegenMethodScope parent,
		    SAIFFInitializeSymbol symbols,
		    CodegenClassScope classScope)
	    {
		    CodegenMethod method = parent.MakeChild(typeof(VariableTriggerWriteArrayElement), this.GetType(), classScope);
		    method.Block
			    .DeclareVar(typeof(VariableTriggerWriteArrayElement), "desc", NewInstance(typeof(VariableTriggerWriteArrayElement)))
			    .SetProperty(Ref("desc"), "VariableName", Constant(variableName))
			    .SetProperty(
				    Ref("desc"),
				    "IndexExpression",
				    ExprNodeUtilityCodegen.CodegenEvaluator(indexExpression, method, typeof(VariableTriggerWriteArrayElementForge), classScope))
			    .SetProperty(
				    Ref("desc"),
				    "TypeWidener",
				    widener == null ? ConstantNull() : TypeWidenerFactory.CodegenWidener(widener, method, this.GetType(), classScope))
			    .MethodReturn(Ref("desc"));
		    return LocalMethod(method);
	    }
	}
} // end of namespace
