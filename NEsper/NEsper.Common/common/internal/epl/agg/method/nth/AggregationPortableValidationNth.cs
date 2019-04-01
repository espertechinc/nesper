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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
	public class AggregationPortableValidationNth : AggregationPortableValidationWFilterWInputType {

	    private int size;

	    public AggregationPortableValidationNth(bool distinct, bool hasFilter, Type inputValueType, int size)

	    	 : base(distinct, hasFilter, inputValueType)

	    {
	        this.size = size;
	    }

	    public AggregationPortableValidationNth() {
	    }

	    protected override Type TypeOf() {
	        return typeof(AggregationPortableValidationNth);
	    }

	    protected override void CodegenInlineSetWFilterWInputType(CodegenExpressionRef @ref, CodegenMethod method, ModuleTableInitializeSymbol symbols, CodegenClassScope classScope) {
	        method.Block.ExprDotMethod(@ref, "setSize", Constant(size));
	    }

	    protected override void ValidateIntoTableWFilterWInputType(string tableExpression, AggregationPortableValidation intoTableAgg, string intoExpression, AggregationForgeFactory factory) {
	        AggregationPortableValidationNth that = (AggregationPortableValidationNth) intoTableAgg;
	        if (size != that.size) {
	            throw new ExprValidationException("The size is " +
	                    size +
	                    " and provided is " +
	                    that.size);
	        }
	    }

	    public void SetSize(int size) {
	        this.size = size;
	    }
	}
} // end of namespace