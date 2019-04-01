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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.variable.compiletime
{
	public class VariableReaderCodegenFieldSharable : CodegenFieldSharable {
	    private readonly VariableMetaData metaWVisibility;

	    public VariableReaderCodegenFieldSharable(VariableMetaData metaWVisibility) {
	        this.metaWVisibility = metaWVisibility;
	    }

	    public Type Type() {
	        return typeof(VariableReader);
	    }

	    public CodegenExpression InitCtorScoped()
	    {
	        return StaticMethod(
	            typeof(VariableDeployTimeResolver), "resolveVariableReader",
	            Constant(metaWVisibility.VariableName),
	            Constant(metaWVisibility.VariableVisibility),
	            Constant(metaWVisibility.VariableModuleName),
	            Constant(metaWVisibility.OptionalContextName),
	            EPStatementInitServicesConstants.REF);
	    }

	    public override bool Equals(object o) {
	        if (this == o) return true;
	        if (o == null || GetType() != o.GetType()) return false;

	        VariableReaderCodegenFieldSharable that = (VariableReaderCodegenFieldSharable) o;

	        return metaWVisibility.VariableName.Equals(that.metaWVisibility.VariableName);
	    }

	    public override int GetHashCode() {
	        return metaWVisibility.VariableName.GetHashCode();
	    }
	}
} // end of namespace