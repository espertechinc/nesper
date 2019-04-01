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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.variant
{
	public class VariantPropertyGetterCacheCodegenField : CodegenFieldSharable {

	    private readonly VariantEventType variantEventType;

	    public VariantPropertyGetterCacheCodegenField(VariantEventType variantEventType) {
	        this.variantEventType = variantEventType;
	    }

	    public Type Type() {
	        return typeof(VariantPropertyGetterCache);
	    }

	    public CodegenExpression InitCtorScoped() {
	        CodegenExpression type = Cast(typeof(VariantEventType), EventTypeUtility.ResolveTypeCodegen(variantEventType, EPStatementInitServicesConstants.REF));
	        return ExprDotMethod(type, "getVariantPropertyGetterCache");
	    }

	    public override bool Equals(object o) {
	        if (this == o) return true;
	        if (o == null || GetType() != o.GetType()) return false;

	        VariantPropertyGetterCacheCodegenField that = (VariantPropertyGetterCacheCodegenField) o;

	        return variantEventType.Equals(that.variantEventType);
	    }

	    public override int GetHashCode() {
	        return variantEventType.GetHashCode();
	    }
	}
} // end of namespace