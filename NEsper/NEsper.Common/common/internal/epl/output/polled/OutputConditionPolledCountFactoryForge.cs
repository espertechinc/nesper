///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
	/// <summary>
	/// Output limit condition that is satisfied when either
	/// the total number of new events arrived or the total number
	/// of old events arrived is greater than a preset value.
	/// </summary>
	public sealed class OutputConditionPolledCountFactoryForge : OutputConditionPolledFactoryForge {
	    private readonly int eventRate;
	    private readonly VariableMetaData variableMetaData;

	    public OutputConditionPolledCountFactoryForge(int eventRate, VariableMetaData variableMetaData) {
	        this.eventRate = eventRate;
	        this.variableMetaData = variableMetaData;

	        if ((eventRate < 1) && (variableMetaData == null)) {
	            throw new ArgumentException("Limiting output by event count requires an event count of at least 1 or a variable name");
	        }
	    }

	    public CodegenExpression Make(CodegenMethodScope parent, CodegenClassScope classScope) {
	        // resolve variable at init-time via field
	        CodegenExpression variableExpression = ConstantNull();
	        if (variableMetaData != null) {
	            variableExpression = VariableDeployTimeResolver.MakeVariableField(variableMetaData, classScope, this.GetType());
	        }

	        CodegenMethod method = parent.MakeChild(typeof(OutputConditionPolledCountFactory), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(OutputConditionPolledCountFactory), "factory", NewInstance(typeof(OutputConditionPolledCountFactory)))
	                .ExprDotMethod(@Ref("factory"), "setEventRate", Constant(eventRate))
	                .ExprDotMethod(@Ref("factory"), "setVariable", variableExpression)
	                .MethodReturn(@Ref("factory"));
	        return LocalMethod(method);
	    }
	}
} // end of namespace