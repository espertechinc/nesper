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

namespace com.espertech.esper.common.@internal.epl.agg.method.avedev
{
	public class AggregationPortableValidationAvedev : AggregationPortableValidationWFilterWInputType {
	    public AggregationPortableValidationAvedev() {
	    }

	    public AggregationPortableValidationAvedev(bool distinct, bool hasFilter, Type inputValueType)

	    	 : base(distinct, hasFilter, inputValueType)

	    {
	    }

	    protected override Type TypeOf() {
	        return typeof(AggregationPortableValidationAvedev);
	    }

	    protected override void ValidateIntoTableWFilterWInputType(string tableExpression, AggregationPortableValidation intoTableAgg, string intoExpression, AggregationForgeFactory factory) {
	    }

	    protected override void CodegenInlineSetWFilterWInputType(CodegenExpressionRef @ref, CodegenMethod method, ModuleTableInitializeSymbol symbols, CodegenClassScope classScope) {
	    }
	}
} // end of namespace